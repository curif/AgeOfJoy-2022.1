// pdvk.cpp — isolated Vulkan context rendering a rotating checkerboard to CPU.
// See pdvk.h for the ABI and PDDocs/vulkancontext.md for the design.
//
// Milestone 1 (done): own VkInstance/VkDevice (no Unity interop), render a fullscreen
// rotating checkerboard into an offscreen R8G8B8A8 image, copy it to a host-visible buffer
// that stays mapped, and expose that pointer. Unity's PdVkQuad copies it into a Texture2D.
//
// Step A (vulkancontext.md §7): the offscreen color target is now backed by an
// AHardwareBuffer instead of plain device-local memory. We AHardwareBuffer_allocate() an
// R8G8B8A8 buffer with GPU color-output + sampled usage, then IMPORT it as the backing memory
// of the render-target VkImage (VK_ANDROID_external_memory_android_hardware_buffer). The CPU
// read-back is deliberately KEPT — this proves AHB-backed rendering in isolation before any
// Unity interop (Step B imports the same AHB on Unity's device and drops the copy).
//
// (Import vs export: the doc sketched the export direction; we own the AHB and import it, which
// gives explicit control over the AHB format + usage flags Unity's importer will need in Step B.)
//
// Deliberately single-threaded and synchronous (record → submit → vkQueueWaitIdle → read).
// A checkerboard does not need pipelining; correctness and legibility win here.

#include "pdvk.h"

#define VK_USE_PLATFORM_ANDROID_KHR   // exposes the AHardwareBuffer Vulkan structs/enums
#include <vulkan/vulkan.h>
#include <android/hardware_buffer.h>
#include <android/log.h>

// Unity native plugin API (Step B: reach Unity's VkDevice to import our AHB there).
// Headers ship with the editor under Editor/Data/PluginAPI — added to the build include path.
#include "IUnityInterface.h"
#include "IUnityGraphics.h"
#include "IUnityGraphicsVulkan.h"

#include <cstring>
#include <cmath>
#include <cstdlib>

// Compiled-at-build SPIR-V (glslc → xxd). See build_android.sh. These headers define
// `checker_vert_spv[]` / `checker_vert_spv_len` and the frag equivalents.
#include "checker_vert.spv.h"
#include "checker_frag.spv.h"

#define LOGI(...) __android_log_print(ANDROID_LOG_INFO,  "pdvk", __VA_ARGS__)
#define LOGE(...) __android_log_print(ANDROID_LOG_ERROR, "pdvk", __VA_ARGS__)

#define VK_CHECK(expr)                                                        \
    do {                                                                      \
        VkResult _r = (expr);                                                 \
        if (_r != VK_SUCCESS) {                                               \
            LOGE("%s:%d %s -> VkResult %d", __FILE__, __LINE__, #expr, _r);   \
            return -1;                                                        \
        }                                                                     \
    } while (0)

namespace {

struct Ctx {
    bool             inited = false;
    int              width  = 0;
    int              height = 0;
    float            angle  = 0.0f;
    uint64_t         frameCount = 0;

    VkInstance       instance   = VK_NULL_HANDLE;
    VkPhysicalDevice phys       = VK_NULL_HANDLE;
    VkDevice         device     = VK_NULL_HANDLE;
    uint32_t         queueFamily = 0;
    VkQueue          queue      = VK_NULL_HANDLE;
    VkCommandPool    cmdPool    = VK_NULL_HANDLE;
    VkCommandBuffer  cmd        = VK_NULL_HANDLE;
    VkFence          fence      = VK_NULL_HANDLE;

    // Offscreen color target (rendered into, then copied out). Its memory is imported from
    // an AHardwareBuffer (Step A) rather than a plain device-local allocation.
    AHardwareBuffer* ahb        = nullptr;
    VkImage          image      = VK_NULL_HANDLE;
    VkDeviceMemory   imageMem   = VK_NULL_HANDLE;
    VkImageView      imageView  = VK_NULL_HANDLE;
    VkRenderPass     renderPass = VK_NULL_HANDLE;
    VkFramebuffer    framebuffer = VK_NULL_HANDLE;

    // Step B: the SAME AHB imported onto Unity's VkDevice for zero-copy sampling.
    VkImage          unityImage      = VK_NULL_HANDLE;
    VkDeviceMemory   unityImageMem   = VK_NULL_HANDLE;
    bool             unityImageReady = false;

    // AHB-import entry point (resolved via vkGetDeviceProcAddr — not a core symbol).
    PFN_vkGetAndroidHardwareBufferPropertiesANDROID fpGetAhbProps = nullptr;

    // Host-visible readback buffer, persistently mapped.
    VkBuffer         readback   = VK_NULL_HANDLE;
    VkDeviceMemory   readbackMem = VK_NULL_HANDLE;
    void*            readbackPtr = nullptr;

    // Pipeline.
    VkShaderModule   vert       = VK_NULL_HANDLE;
    VkShaderModule   frag       = VK_NULL_HANDLE;
    VkPipelineLayout pipeLayout = VK_NULL_HANDLE;
    VkPipeline       pipeline   = VK_NULL_HANDLE;
};

Ctx g;

// Unity native plugin interfaces (Step B). Captured in UnityPluginLoad; used to reach Unity's
// VkDevice so we can import our AHB there and hand the VkImage to Texture2D.CreateExternalTexture.
IUnityInterfaces*     s_interfaces = nullptr;
IUnityGraphicsVulkan* s_uvk        = nullptr;

struct PushConstants {
    float angle;
    float gridN;
};

uint32_t findMemoryType(uint32_t typeBits, VkMemoryPropertyFlags want)
{
    VkPhysicalDeviceMemoryProperties mp;
    vkGetPhysicalDeviceMemoryProperties(g.phys, &mp);
    for (uint32_t i = 0; i < mp.memoryTypeCount; ++i) {
        if ((typeBits & (1u << i)) &&
            (mp.memoryTypes[i].propertyFlags & want) == want) {
            LOGI("  memType %u chosen for typeBits=0x%x want=0x%x (heap %u)",
                 i, typeBits, want, mp.memoryTypes[i].heapIndex);
            return i;
        }
    }
    LOGE("  NO memType for typeBits=0x%x want=0x%x (%u types available)",
         typeBits, want, mp.memoryTypeCount);
    return UINT32_MAX;
}

// Throttled per-frame diagnostic: sampled checksum + the centre pixel, so we can confirm the
// board is non-black AND changing (rotating) without ever seeing it. Cheap; only on log frames.
void logFrame()
{
    const uint8_t* p = static_cast<const uint8_t*>(g.readbackPtr);
    if (!p) { LOGI("frame %llu — no readback ptr", (unsigned long long)g.frameCount); return; }
    const size_t n = (size_t)g.width * g.height * 4;
    uint32_t sum = 0;
    for (size_t i = 0; i < n; i += 257) sum += p[i];     // sparse sample of the whole image
    const size_t ctr = ((size_t)(g.height / 2) * g.width + g.width / 2) * 4;
    LOGI("frame %llu angle=%.3f centerRGBA=(%u,%u,%u,%u) sampledSum=%u",
         (unsigned long long)g.frameCount, g.angle,
         p[ctr], p[ctr + 1], p[ctr + 2], p[ctr + 3], sum);
}

VkShaderModule makeShader(const uint32_t* code, size_t bytes)
{
    VkShaderModuleCreateInfo ci{ VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO };
    ci.codeSize = bytes;
    ci.pCode    = code;
    VkShaderModule m = VK_NULL_HANDLE;
    if (vkCreateShaderModule(g.device, &ci, nullptr, &m) != VK_SUCCESS) return VK_NULL_HANDLE;
    return m;
}

int createInstance()
{
    VkApplicationInfo app{ VK_STRUCTURE_TYPE_APPLICATION_INFO };
    app.pApplicationName = "pdvk";
    app.apiVersion       = VK_API_VERSION_1_1;

    // Log the loader's advertised instance version (helps spot a too-old loader on device).
    // Directly linked at API 29 (Vulkan 1.1), so it's always present.
    uint32_t loaderVer = 0;
    vkEnumerateInstanceVersion(&loaderVer);
    LOGI("  loader instance version %u.%u.%u",
         VK_VERSION_MAJOR(loaderVer), VK_VERSION_MINOR(loaderVer), VK_VERSION_PATCH(loaderVer));

    VkInstanceCreateInfo ci{ VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO };
    ci.pApplicationInfo = &app;
    // No layers/extensions: offscreen-only, no surface/swapchain.
    VK_CHECK(vkCreateInstance(&ci, nullptr, &g.instance));
    LOGI("  vkCreateInstance OK (instance=%p)", (void*)g.instance);
    return 0;
}

int pickDeviceAndQueue()
{
    uint32_t n = 0;
    VK_CHECK(vkEnumeratePhysicalDevices(g.instance, &n, nullptr));
    if (n == 0) { LOGE("no Vulkan physical devices"); return -1; }
    VkPhysicalDevice devs[8];
    if (n > 8) n = 8;
    VK_CHECK(vkEnumeratePhysicalDevices(g.instance, &n, devs));
    g.phys = devs[0]; // Quest has one GPU

    VkPhysicalDeviceProperties props;
    vkGetPhysicalDeviceProperties(g.phys, &props);
    LOGI("device: %s (Vulkan %u.%u.%u)", props.deviceName,
         VK_VERSION_MAJOR(props.apiVersion), VK_VERSION_MINOR(props.apiVersion),
         VK_VERSION_PATCH(props.apiVersion));

    LOGI("  %u physical device(s); using devs[0]", n);

    uint32_t qn = 0;
    vkGetPhysicalDeviceQueueFamilyProperties(g.phys, &qn, nullptr);
    VkQueueFamilyProperties qf[32];
    if (qn > 32) qn = 32;
    vkGetPhysicalDeviceQueueFamilyProperties(g.phys, &qn, qf);
    LOGI("  %u queue famil(ies):", qn);
    for (uint32_t i = 0; i < qn; ++i)
        LOGI("    family %u: flags=0x%x count=%u", i, qf[i].queueFlags, qf[i].queueCount);
    bool found = false;
    for (uint32_t i = 0; i < qn; ++i) {
        if (qf[i].queueFlags & VK_QUEUE_GRAPHICS_BIT) { g.queueFamily = i; found = true; break; }
    }
    if (!found) { LOGE("no graphics queue family"); return -1; }
    LOGI("  graphics queue family = %u", g.queueFamily);

    // Device extensions for AHardwareBuffer import. The KHR deps (external_memory,
    // sampler_ycbcr_conversion, dedicated_allocation, bind_memory2, get_memory_requirements2)
    // are all CORE in Vulkan 1.1, so they need not be listed — only these two non-core ones do.
    // VK_EXT_queue_family_foreign is a required dependency of the AHB extension even though we
    // never do a foreign-queue ownership transfer here.
    const char* wantExts[] = {
        "VK_ANDROID_external_memory_android_hardware_buffer",
        "VK_EXT_queue_family_foreign",
    };
    const uint32_t wantExtCount = (uint32_t)(sizeof(wantExts) / sizeof(wantExts[0]));

    // Enumerate + log availability of the wanted extensions (good device bring-up signal).
    uint32_t extCount = 0;
    vkEnumerateDeviceExtensionProperties(g.phys, nullptr, &extCount, nullptr);
    VkExtensionProperties* exts =
        (VkExtensionProperties*)malloc((size_t)extCount * sizeof(VkExtensionProperties));
    if (exts) vkEnumerateDeviceExtensionProperties(g.phys, nullptr, &extCount, exts);
    LOGI("  %u device extension(s) available; checking the %u we need:", extCount, wantExtCount);
    for (uint32_t w = 0; w < wantExtCount; ++w) {
        bool present = false;
        for (uint32_t i = 0; exts && i < extCount; ++i)
            if (strcmp(exts[i].extensionName, wantExts[w]) == 0) { present = true; break; }
        LOGI("    %s : %s", wantExts[w], present ? "present" : "MISSING");
        if (!present) LOGE("    required extension %s not advertised — vkCreateDevice will fail",
                           wantExts[w]);
    }
    free(exts);

    float prio = 1.0f;
    VkDeviceQueueCreateInfo qci{ VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO };
    qci.queueFamilyIndex = g.queueFamily;
    qci.queueCount       = 1;
    qci.pQueuePriorities = &prio;

    // Enable samplerYcbcrConversion (core 1.1). Not used for our plain RGBA8 target, but the AHB
    // extension is built on the ycbcr machinery and some drivers want the feature on to use it.
    VkPhysicalDeviceSamplerYcbcrConversionFeatures ycbcr{
        VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_SAMPLER_YCBCR_CONVERSION_FEATURES };
    ycbcr.samplerYcbcrConversion = VK_TRUE;

    VkDeviceCreateInfo dci{ VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO };
    dci.pNext                   = &ycbcr;
    dci.queueCreateInfoCount    = 1;
    dci.pQueueCreateInfos       = &qci;
    dci.enabledExtensionCount   = wantExtCount;
    dci.ppEnabledExtensionNames = wantExts;
    VK_CHECK(vkCreateDevice(g.phys, &dci, nullptr, &g.device));
    vkGetDeviceQueue(g.device, g.queueFamily, 0, &g.queue);
    LOGI("  vkCreateDevice OK (device=%p queue=%p) with %u AHB extension(s)",
         (void*)g.device, (void*)g.queue, wantExtCount);

    // Resolve the AHB-properties query (not a statically exported symbol).
    g.fpGetAhbProps = (PFN_vkGetAndroidHardwareBufferPropertiesANDROID)
        vkGetDeviceProcAddr(g.device, "vkGetAndroidHardwareBufferPropertiesANDROID");
    if (!g.fpGetAhbProps) { LOGE("  vkGetAndroidHardwareBufferPropertiesANDROID unresolved"); return -1; }
    LOGI("  resolved vkGetAndroidHardwareBufferPropertiesANDROID=%p", (void*)g.fpGetAhbProps);
    return 0;
}

int createTarget()
{
    const VkFormat fmt = VK_FORMAT_R8G8B8A8_UNORM; // byte order R,G,B,A == Unity RGBA32

    // Confirm the GPU can use this format as a color attachment + transfer source.
    VkFormatProperties fp;
    vkGetPhysicalDeviceFormatProperties(g.phys, fmt, &fp);
    LOGI("  R8G8B8A8_UNORM optimalTilingFeatures=0x%x", fp.optimalTilingFeatures);
    if (!(fp.optimalTilingFeatures & VK_FORMAT_FEATURE_COLOR_ATTACHMENT_BIT))
        LOGE("  WARNING: R8G8B8A8_UNORM lacks COLOR_ATTACHMENT feature on this GPU");

    // --- Step A: allocate the color target as an AHardwareBuffer, then import it as the
    // backing memory of the render-target VkImage. SAMPLED is added so the same AHB is usable
    // as a Unity texture in Step B; COLOR_OUTPUT lets us render into it now.
    AHardwareBuffer_Desc ahbDesc{};
    ahbDesc.width  = (uint32_t)g.width;
    ahbDesc.height = (uint32_t)g.height;
    ahbDesc.layers = 1;
    ahbDesc.format = AHARDWAREBUFFER_FORMAT_R8G8B8A8_UNORM;
    ahbDesc.usage  = AHARDWAREBUFFER_USAGE_GPU_COLOR_OUTPUT |
                     AHARDWAREBUFFER_USAGE_GPU_SAMPLED_IMAGE;
    if (AHardwareBuffer_allocate(&ahbDesc, &g.ahb) != 0 || g.ahb == nullptr) {
        LOGE("  AHardwareBuffer_allocate(%dx%d RGBA8) FAILED", g.width, g.height);
        return -1;
    }
    LOGI("  AHardwareBuffer_allocate OK ahb=%p (%dx%d R8G8B8A8 usage=0x%llx)",
         (void*)g.ahb, g.width, g.height, (unsigned long long)ahbDesc.usage);

    // Query the AHB's Vulkan-side properties: allocation size, allowed memory-type bits, and
    // its resolved VkFormat (should be R8G8B8A8_UNORM for a non-YCbCr buffer — no external format).
    VkAndroidHardwareBufferFormatPropertiesANDROID ahbFmt{
        VK_STRUCTURE_TYPE_ANDROID_HARDWARE_BUFFER_FORMAT_PROPERTIES_ANDROID };
    VkAndroidHardwareBufferPropertiesANDROID ahbProps{
        VK_STRUCTURE_TYPE_ANDROID_HARDWARE_BUFFER_PROPERTIES_ANDROID };
    ahbProps.pNext = &ahbFmt;
    VK_CHECK(g.fpGetAhbProps(g.device, g.ahb, &ahbProps));
    LOGI("  AHB props: allocationSize=%llu memoryTypeBits=0x%x format=%d externalFormat=%llu",
         (unsigned long long)ahbProps.allocationSize, ahbProps.memoryTypeBits,
         (int)ahbFmt.format, (unsigned long long)ahbFmt.externalFormat);
    if (ahbFmt.format != fmt)
        LOGE("  WARNING: AHB resolved format %d != expected R8G8B8A8_UNORM (%d)", (int)ahbFmt.format, fmt);

    // Image created with an external-memory handle type so we may bind imported AHB memory.
    VkExternalMemoryImageCreateInfo extImg{ VK_STRUCTURE_TYPE_EXTERNAL_MEMORY_IMAGE_CREATE_INFO };
    extImg.handleTypes = VK_EXTERNAL_MEMORY_HANDLE_TYPE_ANDROID_HARDWARE_BUFFER_BIT_ANDROID;

    VkImageCreateInfo ici{ VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO };
    ici.pNext       = &extImg;
    ici.imageType   = VK_IMAGE_TYPE_2D;
    ici.format      = fmt;
    ici.extent      = { (uint32_t)g.width, (uint32_t)g.height, 1 };
    ici.mipLevels   = 1;
    ici.arrayLayers = 1;
    ici.samples     = VK_SAMPLE_COUNT_1_BIT;
    ici.tiling      = VK_IMAGE_TILING_OPTIMAL;
    ici.usage       = VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT | VK_IMAGE_USAGE_TRANSFER_SRC_BIT |
                      VK_IMAGE_USAGE_SAMPLED_BIT;
    ici.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
    VK_CHECK(vkCreateImage(g.device, &ici, nullptr, &g.image));

    // Pick a memory type allowed by the AHB; prefer DEVICE_LOCAL but accept any allowed type.
    uint32_t memType = findMemoryType(ahbProps.memoryTypeBits, VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT);
    if (memType == UINT32_MAX) memType = findMemoryType(ahbProps.memoryTypeBits, 0);
    if (memType == UINT32_MAX) { LOGE("  no memory type for AHB import (bits=0x%x)", ahbProps.memoryTypeBits); return -1; }

    // Import the AHB as this image's dedicated memory (AHB import requires a dedicated alloc).
    VkImportAndroidHardwareBufferInfoANDROID importInfo{
        VK_STRUCTURE_TYPE_IMPORT_ANDROID_HARDWARE_BUFFER_INFO_ANDROID };
    importInfo.buffer = g.ahb;
    VkMemoryDedicatedAllocateInfo dedicated{ VK_STRUCTURE_TYPE_MEMORY_DEDICATED_ALLOCATE_INFO };
    dedicated.pNext = &importInfo;
    dedicated.image = g.image;
    VkMemoryAllocateInfo mai{ VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO };
    mai.pNext           = &dedicated;
    mai.allocationSize  = ahbProps.allocationSize;
    mai.memoryTypeIndex = memType;
    VK_CHECK(vkAllocateMemory(g.device, &mai, nullptr, &g.imageMem));
    VK_CHECK(vkBindImageMemory(g.device, g.image, g.imageMem, 0));
    LOGI("  imported AHB into image memory (memType=%u, %llu bytes) image=%p mem=%p",
         memType, (unsigned long long)ahbProps.allocationSize, (void*)g.image, (void*)g.imageMem);

    VkImageViewCreateInfo vci{ VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO };
    vci.image    = g.image;
    vci.viewType = VK_IMAGE_VIEW_TYPE_2D;
    vci.format   = fmt;
    vci.subresourceRange = { VK_IMAGE_ASPECT_COLOR_BIT, 0, 1, 0, 1 };
    VK_CHECK(vkCreateImageView(g.device, &vci, nullptr, &g.imageView));

    // Render pass: clear → render → leave in TRANSFER_SRC so the copy-out needs no barrier.
    VkAttachmentDescription att{};
    att.format         = fmt;
    att.samples        = VK_SAMPLE_COUNT_1_BIT;
    att.loadOp         = VK_ATTACHMENT_LOAD_OP_CLEAR;
    att.storeOp        = VK_ATTACHMENT_STORE_OP_STORE;
    att.stencilLoadOp  = VK_ATTACHMENT_LOAD_OP_DONT_CARE;
    att.stencilStoreOp = VK_ATTACHMENT_STORE_OP_DONT_CARE;
    att.initialLayout  = VK_IMAGE_LAYOUT_UNDEFINED;
    att.finalLayout    = VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL;

    VkAttachmentReference ref{ 0, VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL };
    VkSubpassDescription sub{};
    sub.pipelineBindPoint    = VK_PIPELINE_BIND_POINT_GRAPHICS;
    sub.colorAttachmentCount = 1;
    sub.pColorAttachments    = &ref;

    // Make the color writes visible to the subsequent transfer read.
    VkSubpassDependency dep{};
    dep.srcSubpass    = 0;
    dep.dstSubpass    = VK_SUBPASS_EXTERNAL;
    dep.srcStageMask  = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
    dep.dstStageMask  = VK_PIPELINE_STAGE_TRANSFER_BIT;
    dep.srcAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
    dep.dstAccessMask = VK_ACCESS_TRANSFER_READ_BIT;

    VkRenderPassCreateInfo rci{ VK_STRUCTURE_TYPE_RENDER_PASS_CREATE_INFO };
    rci.attachmentCount = 1;
    rci.pAttachments    = &att;
    rci.subpassCount    = 1;
    rci.pSubpasses      = &sub;
    rci.dependencyCount = 1;
    rci.pDependencies   = &dep;
    VK_CHECK(vkCreateRenderPass(g.device, &rci, nullptr, &g.renderPass));

    VkFramebufferCreateInfo fci{ VK_STRUCTURE_TYPE_FRAMEBUFFER_CREATE_INFO };
    fci.renderPass      = g.renderPass;
    fci.attachmentCount = 1;
    fci.pAttachments    = &g.imageView;
    fci.width           = g.width;
    fci.height          = g.height;
    fci.layers          = 1;
    VK_CHECK(vkCreateFramebuffer(g.device, &fci, nullptr, &g.framebuffer));

    // Host-visible readback buffer (tightly packed RGBA8), kept mapped.
    VkBufferCreateInfo bci{ VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO };
    bci.size  = (VkDeviceSize)g.width * g.height * 4;
    bci.usage = VK_BUFFER_USAGE_TRANSFER_DST_BIT;
    bci.sharingMode = VK_SHARING_MODE_EXCLUSIVE;
    VK_CHECK(vkCreateBuffer(g.device, &bci, nullptr, &g.readback));

    VkMemoryRequirements bmr;
    vkGetBufferMemoryRequirements(g.device, g.readback, &bmr);
    VkMemoryAllocateInfo bmai{ VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO };
    bmai.allocationSize  = bmr.size;
    bmai.memoryTypeIndex = findMemoryType(bmr.memoryTypeBits,
        VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_COHERENT_BIT);
    if (bmai.memoryTypeIndex == UINT32_MAX) { LOGE("no host-visible memory"); return -1; }
    VK_CHECK(vkAllocateMemory(g.device, &bmai, nullptr, &g.readbackMem));
    VK_CHECK(vkBindBufferMemory(g.device, g.readback, g.readbackMem, 0));
    VK_CHECK(vkMapMemory(g.device, g.readbackMem, 0, VK_WHOLE_SIZE, 0, &g.readbackPtr));
    LOGI("  createTarget OK: image=%p view=%p fb=%p readback=%p mapped=%p (%llu bytes)",
         (void*)g.image, (void*)g.imageView, (void*)g.framebuffer, (void*)g.readback,
         g.readbackPtr, (unsigned long long)bci.size);
    return 0;
}

int createPipeline()
{
    LOGI("  shader SPIR-V: vert=%u bytes frag=%u bytes", checker_vert_spv_len, checker_frag_spv_len);
    g.vert = makeShader(reinterpret_cast<const uint32_t*>(checker_vert_spv), checker_vert_spv_len);
    g.frag = makeShader(reinterpret_cast<const uint32_t*>(checker_frag_spv), checker_frag_spv_len);
    if (!g.vert || !g.frag) { LOGE("shader module creation failed"); return -1; }

    VkPushConstantRange pcr{ VK_SHADER_STAGE_FRAGMENT_BIT, 0, sizeof(PushConstants) };
    VkPipelineLayoutCreateInfo plci{ VK_STRUCTURE_TYPE_PIPELINE_LAYOUT_CREATE_INFO };
    plci.pushConstantRangeCount = 1;
    plci.pPushConstantRanges    = &pcr;
    VK_CHECK(vkCreatePipelineLayout(g.device, &plci, nullptr, &g.pipeLayout));

    VkPipelineShaderStageCreateInfo stages[2]{};
    stages[0].sType  = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
    stages[0].stage  = VK_SHADER_STAGE_VERTEX_BIT;
    stages[0].module = g.vert;
    stages[0].pName  = "main";
    stages[1].sType  = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
    stages[1].stage  = VK_SHADER_STAGE_FRAGMENT_BIT;
    stages[1].module = g.frag;
    stages[1].pName  = "main";

    VkPipelineVertexInputStateCreateInfo vin{ VK_STRUCTURE_TYPE_PIPELINE_VERTEX_INPUT_STATE_CREATE_INFO };
    VkPipelineInputAssemblyStateCreateInfo ia{ VK_STRUCTURE_TYPE_PIPELINE_INPUT_ASSEMBLY_STATE_CREATE_INFO };
    ia.topology = VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST;

    VkViewport vp{ 0, 0, (float)g.width, (float)g.height, 0.0f, 1.0f };
    VkRect2D   sc{ {0, 0}, { (uint32_t)g.width, (uint32_t)g.height } };
    VkPipelineViewportStateCreateInfo vps{ VK_STRUCTURE_TYPE_PIPELINE_VIEWPORT_STATE_CREATE_INFO };
    vps.viewportCount = 1; vps.pViewports = &vp;
    vps.scissorCount  = 1; vps.pScissors  = &sc;

    VkPipelineRasterizationStateCreateInfo rs{ VK_STRUCTURE_TYPE_PIPELINE_RASTERIZATION_STATE_CREATE_INFO };
    rs.polygonMode = VK_POLYGON_MODE_FILL;
    rs.cullMode    = VK_CULL_MODE_NONE;
    rs.frontFace   = VK_FRONT_FACE_COUNTER_CLOCKWISE;
    rs.lineWidth   = 1.0f;

    VkPipelineMultisampleStateCreateInfo ms{ VK_STRUCTURE_TYPE_PIPELINE_MULTISAMPLE_STATE_CREATE_INFO };
    ms.rasterizationSamples = VK_SAMPLE_COUNT_1_BIT;

    VkPipelineColorBlendAttachmentState cba{};
    cba.colorWriteMask = VK_COLOR_COMPONENT_R_BIT | VK_COLOR_COMPONENT_G_BIT |
                         VK_COLOR_COMPONENT_B_BIT | VK_COLOR_COMPONENT_A_BIT;
    VkPipelineColorBlendStateCreateInfo cb{ VK_STRUCTURE_TYPE_PIPELINE_COLOR_BLEND_STATE_CREATE_INFO };
    cb.attachmentCount = 1; cb.pAttachments = &cba;

    VkGraphicsPipelineCreateInfo gp{ VK_STRUCTURE_TYPE_GRAPHICS_PIPELINE_CREATE_INFO };
    gp.stageCount          = 2;
    gp.pStages             = stages;
    gp.pVertexInputState   = &vin;
    gp.pInputAssemblyState = &ia;
    gp.pViewportState      = &vps;
    gp.pRasterizationState = &rs;
    gp.pMultisampleState   = &ms;
    gp.pColorBlendState    = &cb;
    gp.layout              = g.pipeLayout;
    gp.renderPass          = g.renderPass;
    gp.subpass             = 0;
    VK_CHECK(vkCreateGraphicsPipelines(g.device, VK_NULL_HANDLE, 1, &gp, nullptr, &g.pipeline));
    LOGI("  createPipeline OK (pipeline=%p layout=%p)", (void*)g.pipeline, (void*)g.pipeLayout);
    return 0;
}

int createCommands()
{
    VkCommandPoolCreateInfo pci{ VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO };
    pci.flags            = VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT;
    pci.queueFamilyIndex = g.queueFamily;
    VK_CHECK(vkCreateCommandPool(g.device, &pci, nullptr, &g.cmdPool));

    VkCommandBufferAllocateInfo cai{ VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO };
    cai.commandPool        = g.cmdPool;
    cai.level              = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
    cai.commandBufferCount = 1;
    VK_CHECK(vkAllocateCommandBuffers(g.device, &cai, &g.cmd));

    VkFenceCreateInfo fci{ VK_STRUCTURE_TYPE_FENCE_CREATE_INFO };
    VK_CHECK(vkCreateFence(g.device, &fci, nullptr, &g.fence));
    return 0;
}

// --- Step B: import the AHB onto Unity's VkDevice -------------------------------------------

uint32_t findMemoryTypeOn(VkPhysicalDevice phys, uint32_t typeBits, VkMemoryPropertyFlags want)
{
    VkPhysicalDeviceMemoryProperties mp;
    vkGetPhysicalDeviceMemoryProperties(phys, &mp);
    for (uint32_t i = 0; i < mp.memoryTypeCount; ++i)
        if ((typeBits & (1u << i)) && (mp.memoryTypes[i].propertyFlags & want) == want) return i;
    return UINT32_MAX;
}

// Import the SAME AHardwareBuffer onto Unity's VkDevice, create a VkImage there, and transition it
// to SHADER_READ_ONLY_OPTIMAL. Runs on Unity's render thread via GL.IssuePluginEvent, with queue
// access granted by ConfigureEvent. Returns 0 on success. The first decisive check is whether the
// AHB import extension is even enabled on Unity's device (it must be, or Step B v2 needs an
// InterceptInitialization hook to add it at device creation).
int doUnityImport()
{
    if (g.unityImageReady)      { LOGI("[unity-import] already done"); return 0; }
    if (!g.inited || !g.ahb)    { LOGE("[unity-import] pdvk not inited / no AHB"); return -1; }
    if (!s_uvk)                 { LOGE("[unity-import] no IUnityGraphicsVulkan — UnityPluginLoad never ran?"); return -1; }

    UnityVulkanInstance uvi = s_uvk->Instance();
    LOGI("[unity-import] Unity instance=%p physDev=%p device=%p gfxQueue=%p qFamily=%u",
         (void*)uvi.instance, (void*)uvi.physicalDevice, (void*)uvi.device,
         (void*)uvi.graphicsQueue, uvi.queueFamilyIndex);
    if (uvi.device == VK_NULL_HANDLE) { LOGE("[unity-import] Unity device is NULL"); return -1; }

    // KEY DIAGNOSTIC: is VK_ANDROID_external_memory_android_hardware_buffer enabled on UNITY's
    // device? If this fn doesn't resolve, the extension is off and we need Step B v2.
    PFN_vkGetAndroidHardwareBufferPropertiesANDROID fp =
        (PFN_vkGetAndroidHardwareBufferPropertiesANDROID)
        vkGetDeviceProcAddr(uvi.device, "vkGetAndroidHardwareBufferPropertiesANDROID");
    if (!fp) {
        LOGE("[unity-import] Unity's VkDevice does NOT expose vkGetAndroidHardwareBufferPropertiesANDROID");
        LOGE("[unity-import] => the AHB extension is NOT enabled on Unity's device. Step B v2 needs");
        LOGE("[unity-import]    InterceptInitialization to add it at Unity's vkCreateDevice.");
        return -1;
    }
    LOGI("[unity-import] Unity device exposes AHB-props fn=%p — extension IS enabled, proceeding", (void*)fp);

    const VkFormat fmt = VK_FORMAT_R8G8B8A8_UNORM;
    VkAndroidHardwareBufferFormatPropertiesANDROID ahbFmt{
        VK_STRUCTURE_TYPE_ANDROID_HARDWARE_BUFFER_FORMAT_PROPERTIES_ANDROID };
    VkAndroidHardwareBufferPropertiesANDROID ahbProps{
        VK_STRUCTURE_TYPE_ANDROID_HARDWARE_BUFFER_PROPERTIES_ANDROID };
    ahbProps.pNext = &ahbFmt;
    VK_CHECK(fp(uvi.device, g.ahb, &ahbProps));
    LOGI("[unity-import] AHB props on Unity dev: allocSize=%llu memTypeBits=0x%x format=%d",
         (unsigned long long)ahbProps.allocationSize, ahbProps.memoryTypeBits, (int)ahbFmt.format);

    VkExternalMemoryImageCreateInfo extImg{ VK_STRUCTURE_TYPE_EXTERNAL_MEMORY_IMAGE_CREATE_INFO };
    extImg.handleTypes = VK_EXTERNAL_MEMORY_HANDLE_TYPE_ANDROID_HARDWARE_BUFFER_BIT_ANDROID;
    VkImageCreateInfo ici{ VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO };
    ici.pNext         = &extImg;
    ici.imageType     = VK_IMAGE_TYPE_2D;
    ici.format        = fmt;
    ici.extent        = { (uint32_t)g.width, (uint32_t)g.height, 1 };
    ici.mipLevels     = 1;
    ici.arrayLayers   = 1;
    ici.samples       = VK_SAMPLE_COUNT_1_BIT;
    ici.tiling        = VK_IMAGE_TILING_OPTIMAL;
    ici.usage         = VK_IMAGE_USAGE_SAMPLED_BIT;
    ici.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
    VK_CHECK(vkCreateImage(uvi.device, &ici, nullptr, &g.unityImage));

    uint32_t memType = findMemoryTypeOn(uvi.physicalDevice, ahbProps.memoryTypeBits, VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT);
    if (memType == UINT32_MAX) memType = findMemoryTypeOn(uvi.physicalDevice, ahbProps.memoryTypeBits, 0);
    if (memType == UINT32_MAX) { LOGE("[unity-import] no memtype for AHB on Unity dev (bits=0x%x)", ahbProps.memoryTypeBits); return -1; }

    VkImportAndroidHardwareBufferInfoANDROID importInfo{
        VK_STRUCTURE_TYPE_IMPORT_ANDROID_HARDWARE_BUFFER_INFO_ANDROID };
    importInfo.buffer = g.ahb;
    VkMemoryDedicatedAllocateInfo dedicated{ VK_STRUCTURE_TYPE_MEMORY_DEDICATED_ALLOCATE_INFO };
    dedicated.pNext = &importInfo;
    dedicated.image = g.unityImage;
    VkMemoryAllocateInfo mai{ VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO };
    mai.pNext           = &dedicated;
    mai.allocationSize  = ahbProps.allocationSize;
    mai.memoryTypeIndex = memType;
    VK_CHECK(vkAllocateMemory(uvi.device, &mai, nullptr, &g.unityImageMem));
    VK_CHECK(vkBindImageMemory(uvi.device, g.unityImage, g.unityImageMem, 0));
    LOGI("[unity-import] imported AHB onto Unity device: image=%p mem=%p", (void*)g.unityImage, (void*)g.unityImageMem);

    // One-time layout transition UNDEFINED -> SHADER_READ_ONLY_OPTIMAL on Unity's queue (queue
    // access granted by ConfigureEvent). v1 uses IGNORED queue families (no foreign-queue ownership
    // transfer) to avoid depending on VK_EXT_queue_family_foreign on Unity's device; proper
    // ordering comes with the external semaphore in Step C.
    VkCommandPool pool = VK_NULL_HANDLE;
    VkCommandPoolCreateInfo pci{ VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO };
    pci.queueFamilyIndex = uvi.queueFamilyIndex;
    VK_CHECK(vkCreateCommandPool(uvi.device, &pci, nullptr, &pool));
    VkCommandBuffer cb = VK_NULL_HANDLE;
    VkCommandBufferAllocateInfo cai{ VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO };
    cai.commandPool        = pool;
    cai.level              = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
    cai.commandBufferCount = 1;
    VK_CHECK(vkAllocateCommandBuffers(uvi.device, &cai, &cb));
    VkCommandBufferBeginInfo bbi{ VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO };
    bbi.flags = VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT;
    VK_CHECK(vkBeginCommandBuffer(cb, &bbi));
    VkImageMemoryBarrier bar{ VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER };
    bar.srcAccessMask       = 0;
    bar.dstAccessMask       = VK_ACCESS_SHADER_READ_BIT;
    bar.oldLayout           = VK_IMAGE_LAYOUT_UNDEFINED;
    bar.newLayout           = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
    bar.srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
    bar.dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
    bar.image               = g.unityImage;
    bar.subresourceRange    = { VK_IMAGE_ASPECT_COLOR_BIT, 0, 1, 0, 1 };
    vkCmdPipelineBarrier(cb, VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT, VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT,
                         0, 0, nullptr, 0, nullptr, 1, &bar);
    VK_CHECK(vkEndCommandBuffer(cb));
    VkSubmitInfo si{ VK_STRUCTURE_TYPE_SUBMIT_INFO };
    si.commandBufferCount = 1;
    si.pCommandBuffers    = &cb;
    VK_CHECK(vkQueueSubmit(uvi.graphicsQueue, 1, &si, VK_NULL_HANDLE));
    VK_CHECK(vkQueueWaitIdle(uvi.graphicsQueue));
    vkDestroyCommandPool(uvi.device, pool, nullptr);

    g.unityImageReady = true;
    LOGI("[unity-import] DONE — Unity VkImage=%p ready for CreateExternalTexture (ptr=%p)",
         (void*)g.unityImage, (void*)&g.unityImage);
    return 0;
}

void UNITY_INTERFACE_API OnRenderEvent(int eventId)
{
    if (eventId == PDVK_EVENT_IMPORT_AHB) {
        LOGI("[OnRenderEvent] import event received");
        if (doUnityImport() != 0) LOGE("[OnRenderEvent] doUnityImport FAILED");
    }
}

} // namespace

int pdvk_init(int width, int height)
{
    if (g.inited) { LOGI("pdvk_init: already inited (%dx%d) — no-op", g.width, g.height); return 0; }
    LOGI("pdvk_init begin %dx%d  [build %s %s]", width, height, __DATE__, __TIME__);
    if (width <= 0 || height <= 0) { LOGE("bad size %dx%d", width, height); return -1; }
    g = Ctx{};
    g.width  = width;
    g.height = height;

    LOGI(" step 1/5 createInstance");
    if (createInstance()      != 0) { LOGE("pdvk_init FAILED at createInstance");     pdvk_shutdown(); return -1; }
    LOGI(" step 2/5 pickDeviceAndQueue");
    if (pickDeviceAndQueue()  != 0) { LOGE("pdvk_init FAILED at pickDeviceAndQueue"); pdvk_shutdown(); return -1; }
    LOGI(" step 3/5 createTarget");
    if (createTarget()        != 0) { LOGE("pdvk_init FAILED at createTarget");       pdvk_shutdown(); return -1; }
    LOGI(" step 4/5 createPipeline");
    if (createPipeline()      != 0) { LOGE("pdvk_init FAILED at createPipeline");     pdvk_shutdown(); return -1; }
    LOGI(" step 5/5 createCommands");
    if (createCommands()      != 0) { LOGE("pdvk_init FAILED at createCommands");     pdvk_shutdown(); return -1; }

    g.inited = true;
    LOGI("pdvk_init OK (%dx%d) — ready to render", width, height);
    return 0;
}

int pdvk_render(float dt_seconds)
{
    if (!g.inited) {
        // Log the first few misuse calls so a "nothing happens" bug is obvious, then go quiet.
        if (g.frameCount < 3) LOGE("pdvk_render called before init (call %llu)",
                                   (unsigned long long)g.frameCount);
        g.frameCount++;
        return -1;
    }
    g.angle += dt_seconds; // ~1 rad/s

    VK_CHECK(vkResetCommandBuffer(g.cmd, 0));
    VkCommandBufferBeginInfo bi{ VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO };
    bi.flags = VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT;
    VK_CHECK(vkBeginCommandBuffer(g.cmd, &bi));

    VkClearValue clear{};
    clear.color = { { 0.0f, 0.0f, 0.0f, 1.0f } };
    VkRenderPassBeginInfo rp{ VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO };
    rp.renderPass        = g.renderPass;
    rp.framebuffer       = g.framebuffer;
    rp.renderArea.extent = { (uint32_t)g.width, (uint32_t)g.height };
    rp.clearValueCount   = 1;
    rp.pClearValues      = &clear;
    vkCmdBeginRenderPass(g.cmd, &rp, VK_SUBPASS_CONTENTS_INLINE);

    vkCmdBindPipeline(g.cmd, VK_PIPELINE_BIND_POINT_GRAPHICS, g.pipeline);
    PushConstants pc{ g.angle, 8.0f };
    vkCmdPushConstants(g.cmd, g.pipeLayout, VK_SHADER_STAGE_FRAGMENT_BIT, 0, sizeof(pc), &pc);
    vkCmdDraw(g.cmd, 3, 1, 0, 0); // fullscreen triangle
    vkCmdEndRenderPass(g.cmd);    // leaves image in TRANSFER_SRC_OPTIMAL

    VkBufferImageCopy region{};
    region.imageSubresource = { VK_IMAGE_ASPECT_COLOR_BIT, 0, 0, 1 };
    region.imageExtent      = { (uint32_t)g.width, (uint32_t)g.height, 1 };
    vkCmdCopyImageToBuffer(g.cmd, g.image, VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,
                           g.readback, 1, &region);

    VK_CHECK(vkEndCommandBuffer(g.cmd));

    VK_CHECK(vkResetFences(g.device, 1, &g.fence));
    VkSubmitInfo si{ VK_STRUCTURE_TYPE_SUBMIT_INFO };
    si.commandBufferCount = 1;
    si.pCommandBuffers    = &g.cmd;
    VK_CHECK(vkQueueSubmit(g.queue, 1, &si, g.fence));
    VK_CHECK(vkWaitForFences(g.device, 1, &g.fence, VK_TRUE, UINT64_MAX));
    // HOST_COHERENT memory: the copy is visible to the host without an explicit invalidate.

    g.frameCount++;
    // Verbose for the first handful of frames (catch a render that never starts), then ~1/sec
    // at 72 Hz (catch a render that freezes — the centre pixel/checksum should keep changing).
    if (g.frameCount <= 5 || (g.frameCount % 72) == 0) logFrame();
    return 0;
}

int pdvk_get_pixels(const void** out_pixels, int* out_width, int* out_height)
{
    if (!g.inited || !g.readbackPtr) return -1;
    if (out_pixels) *out_pixels = g.readbackPtr;
    if (out_width)  *out_width  = g.width;
    if (out_height) *out_height = g.height;
    static bool firstHandout = true;       // confirm Unity actually pulls the buffer once
    if (firstHandout) { firstHandout = false; LOGI("pdvk_get_pixels: first handout %dx%d ptr=%p",
                                                   g.width, g.height, g.readbackPtr); }
    return 0;
}

void pdvk_shutdown(void)
{
    // Step B: free the Unity-device import first — it lives on Unity's VkDevice, not ours.
    // Best-effort; C# must have destroyed its external Texture2D before calling this.
    if ((g.unityImage != VK_NULL_HANDLE || g.unityImageMem != VK_NULL_HANDLE) && s_uvk) {
        UnityVulkanInstance uvi = s_uvk->Instance();
        if (uvi.device != VK_NULL_HANDLE) {
            vkDeviceWaitIdle(uvi.device);
            if (g.unityImage)    vkDestroyImage(uvi.device, g.unityImage, nullptr);
            if (g.unityImageMem) vkFreeMemory(uvi.device, g.unityImageMem, nullptr);
        }
    }
    g.unityImage = VK_NULL_HANDLE; g.unityImageMem = VK_NULL_HANDLE; g.unityImageReady = false;

    if (g.device != VK_NULL_HANDLE) {
        vkDeviceWaitIdle(g.device);
        if (g.pipeline)    vkDestroyPipeline(g.device, g.pipeline, nullptr);
        if (g.pipeLayout)  vkDestroyPipelineLayout(g.device, g.pipeLayout, nullptr);
        if (g.vert)        vkDestroyShaderModule(g.device, g.vert, nullptr);
        if (g.frag)        vkDestroyShaderModule(g.device, g.frag, nullptr);
        if (g.framebuffer) vkDestroyFramebuffer(g.device, g.framebuffer, nullptr);
        if (g.renderPass)  vkDestroyRenderPass(g.device, g.renderPass, nullptr);
        if (g.imageView)   vkDestroyImageView(g.device, g.imageView, nullptr);
        if (g.image)       vkDestroyImage(g.device, g.image, nullptr);
        if (g.imageMem)    vkFreeMemory(g.device, g.imageMem, nullptr);  // frees the imported AHB ref
        if (g.readbackPtr) vkUnmapMemory(g.device, g.readbackMem);
        if (g.readback)    vkDestroyBuffer(g.device, g.readback, nullptr);
        if (g.readbackMem) vkFreeMemory(g.device, g.readbackMem, nullptr);
        if (g.fence)       vkDestroyFence(g.device, g.fence, nullptr);
        if (g.cmdPool)     vkDestroyCommandPool(g.device, g.cmdPool, nullptr);
        vkDestroyDevice(g.device, nullptr);
    }
    if (g.instance != VK_NULL_HANDLE) vkDestroyInstance(g.instance, nullptr);
    g = Ctx{};
    LOGI("pdvk_shutdown");
}

// --- Step B: C ABI accessors + Unity native plugin entry points -----------------------------

const void* pdvk_get_unity_image_ptr(void)
{
    return g.unityImageReady ? (const void*)&g.unityImage : nullptr;
}

int pdvk_unity_image_ready(void) { return g.unityImageReady ? 1 : 0; }

void* pdvk_GetRenderEventFunc(void) { return (void*)OnRenderEvent; }

// Unity calls these when it loads/unloads the plugin (gives us IUnityInterfaces → Unity's
// VkDevice). Logged loudly: if [UnityPluginLoad] never appears, the plugin was loaded too late
// (via System.loadLibrary only) and we'd need it preloaded for InterceptInitialization.
extern "C" UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API
UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
    s_interfaces = unityInterfaces;
    s_uvk = unityInterfaces ? unityInterfaces->Get<IUnityGraphicsVulkan>() : nullptr;
    LOGI("[UnityPluginLoad] called. interfaces=%p IUnityGraphicsVulkan=%p",
         (void*)s_interfaces, (void*)s_uvk);
    if (s_uvk) {
        UnityVulkanPluginEventConfig cfg{};
        cfg.renderPassPrecondition = kUnityVulkanRenderPass_EnsureOutside;
        cfg.graphicsQueueAccess    = kUnityVulkanGraphicsQueueAccess_Allow;
        cfg.flags = kUnityVulkanEventConfigFlag_EnsurePreviousFrameSubmission |
                    kUnityVulkanEventConfigFlag_ModifiesCommandBuffersState;
        s_uvk->ConfigureEvent(PDVK_EVENT_IMPORT_AHB, &cfg);
        LOGI("[UnityPluginLoad] ConfigureEvent(IMPORT_AHB) done");
    } else {
        LOGE("[UnityPluginLoad] IUnityGraphicsVulkan unavailable (is the app on Vulkan?)");
    }
}

extern "C" UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API UnityPluginUnload()
{
    LOGI("[UnityPluginUnload]");
    s_uvk = nullptr;
    s_interfaces = nullptr;
}
