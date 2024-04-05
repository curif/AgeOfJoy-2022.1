#include "vulkan.h"

static struct retro_hw_render_callback hw_render_callback;
static struct retro_hw_render_context_negotiation_interface_vulkan hw_vulkan_interface;
static struct retro_hw_render_interface_vulkan hw_render_interface;
static struct retro_vulkan_context context;
static VkInstance vkInstance;
static PFN_vkGetInstanceProcAddr vk_get_instance_proc_addr;
static PFN_vkCreateInstance vk_create_instance;
static PFN_vkGetDeviceProcAddr vk_get_device_proc_addr;


void wrapper_environment_log_render_callback() {
	char* fmt = "[wrapper_environment_log_render_callback] Render callback:\n"
		"    version_major: %i\n"
		"    version_minor: %i\n";
	wrapper_environment_log(RETRO_LOG_INFO, fmt,
		hw_render_callback.version_major,
		hw_render_callback.version_minor
	);
}

void wrapper_environment_log_vulkan_interface() {
	char* fmt = "[wrapper_environment_log_vulkan_interface] Vulkan interface:\n"
		"    interface_type: %i\n"
		"    interface_version: %i\n"
		"    get_application_info: %016x\n"
		"    create_device:        %016x\n"
		"    destroy_device:       %016x\n"
		;
	wrapper_environment_log(RETRO_LOG_INFO, fmt,
		hw_vulkan_interface.interface_type,
		hw_vulkan_interface.interface_version,
		(long)hw_vulkan_interface.get_application_info,
		(long)hw_vulkan_interface.create_device,
		(long)hw_vulkan_interface.destroy_device
	);
}


// VULCAN HANDLERS. THESE NEED TO BE IMPLEMENTED (SOMEHOW)

static void vulkan_set_image(void* handle,
	const struct retro_vulkan_image* image,
	uint32_t num_semaphores,
	const VkSemaphore* semaphores,
	uint32_t src_queue_family)
{
	wrapper_environment_log(RETRO_LOG_INFO, "[VULCAN_HANDLERS] vulkan_set_image\n"
		//"  image %016x\n"
		//"  num_semaphores %i\n"
		//"  semaphores %016x\n"
		//"  src_queue_family %i\n",
		//image,
		//num_semaphores,
		//semaphores,
		//src_queue_family
	);
}

static uint32_t frameIndex = 0;
static uint32_t vulkan_get_sync_index(void* handle)
{
	wrapper_environment_log(RETRO_LOG_INFO, "[VULCAN_HANDLERS] vulkan_get_sync_index\n"
		//"  frameIndex %i\n", frameIndex
	);
	frameIndex++;
	return frameIndex;
}

static uint32_t vulkan_get_sync_index_mask(void* handle)
{
	wrapper_environment_log(RETRO_LOG_INFO, "[VULCAN_HANDLERS] vulkan_get_sync_index_mask\n");
	return 1;   // no idea !
}

static void vulkan_wait_sync_index(void* handle)
{
	wrapper_environment_log(RETRO_LOG_INFO, "[VULCAN_HANDLERS] vulkan_wait_sync_index\n");
}

static void vulkan_set_command_buffers(void* handle, uint32_t num_cmd, const VkCommandBuffer* cmd)
{
	wrapper_environment_log(RETRO_LOG_INFO, "[VULCAN_HANDLERS] vulkan_set_command_buffers\n"
		//"  num_cmd %i\n"
		//"  cmd %016x\n",
		//num_cmd,
		//cmd
	);
}

static void vulkan_lock_queue(void* handle)
{
	wrapper_environment_log(RETRO_LOG_INFO, "[VULCAN_HANDLERS] vulkan_lock_queue\n");
}

static void vulkan_unlock_queue(void* handle)
{
	wrapper_environment_log(RETRO_LOG_INFO, "[VULCAN_HANDLERS] vulkan_unlock_queue\n");
}

static void vulkan_set_signal_semaphore(void* handle, VkSemaphore semaphore)
{
	wrapper_environment_log(RETRO_LOG_INFO, "[VULCAN_HANDLERS] vulkan_set_signal_semaphore\n"
		"  semaphore %016x\n",
		&semaphore
	);
}


bool init_retro_hw_render_callback(struct retro_hw_render_callback* callback)
{
	memcpy(&(hw_render_callback), callback, sizeof(hw_render_callback));
	wrapper_environment_log_render_callback();
	return true;
}

bool init_retro_hw_render_context_negotiation_interface_vulkan(struct retro_hw_render_context_negotiation_interface_vulkan* interface) {
	memcpy(&(hw_vulkan_interface), interface, sizeof(hw_vulkan_interface));
	wrapper_environment_log_vulkan_interface();

	// 1- Have to negotiate thru retro_hw_render_context_negotiation_interface_vulkan
	wrapper_environment_log(RETRO_LOG_INFO, "[init_retro_hw_render_context_negotiation_interface_vulkan]\n");

	void* vulkan_library = dlopen("libvulkan.so", RTLD_NOW);
	if (!vulkan_library) {
		wrapper_environment_log(RETRO_LOG_INFO, "[RETRO_ENVIRONMENT_GET_HW_RENDER_INTERFACE] Failed to load libvulkan.so\n");
		return false;
	}

	vk_get_instance_proc_addr = (PFN_vkGetInstanceProcAddr)dlsym(vulkan_library, "vkGetInstanceProcAddr");
	if (!vk_get_instance_proc_addr) {
		wrapper_environment_log(RETRO_LOG_INFO, "[RETRO_ENVIRONMENT_GET_HW_RENDER_INTERFACE] Failed to find vkGetInstanceProcAddr\n");
		return false;
	}

	vk_create_instance = (PFN_vkCreateInstance)vk_get_instance_proc_addr(NULL, "vkCreateInstance");
	if (!vk_create_instance) {
		wrapper_environment_log(RETRO_LOG_INFO, "[RETRO_ENVIRONMENT_GET_HW_RENDER_INTERFACE] Failed to find vkCreateInstance\n");
		return false;
	}

	vk_get_device_proc_addr = (PFN_vkGetDeviceProcAddr)vk_get_instance_proc_addr(NULL, "vkGetDeviceProcAddr");
	if (!vk_get_device_proc_addr) {
		wrapper_environment_log(RETRO_LOG_INFO, "[RETRO_ENVIRONMENT_GET_HW_RENDER_INTERFACE] Failed to find vkGetDeviceProcAddr\n");
		//return false;
	}

	// Initialize the VkApplicationInfo structure
	VkApplicationInfo appInfo = {};
	appInfo.sType = VK_STRUCTURE_TYPE_APPLICATION_INFO;
	appInfo.pApplicationName = "Hello Vulkan";
	appInfo.applicationVersion = VK_MAKE_VERSION(1, 0, 0);
	appInfo.pEngineName = "No Engine";
	appInfo.engineVersion = VK_MAKE_VERSION(1, 0, 0);
	appInfo.apiVersion = VK_API_VERSION_1_0;

	// Initialize the VkInstanceCreateInfo structure
	VkInstanceCreateInfo createInfo = {};
	createInfo.sType = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO;
	createInfo.pApplicationInfo = &appInfo;

	// Create the Vulkan instance
	VkResult vkCreateInstanceResult = vk_create_instance(&createInfo, NULL, &vkInstance);
	if (vkCreateInstanceResult != VK_SUCCESS) {
		wrapper_environment_log(RETRO_LOG_INFO, "[init_retro_hw_render_context_negotiation_interface_vulkan] Failed to create Vulkan instance\n");
		return false;
	}

	char* required_device_extensions = VK_KHR_SWAPCHAIN_EXTENSION_NAME;
	VkPhysicalDeviceFeatures required_features = { false };

	bool result = interface->create_device(&context,
		vkInstance,                   // VkInstance instance
		VK_NULL_HANDLE,               // VkPhysicalDevice gpu
		VK_NULL_HANDLE,               // VkSurfaceKHR surface
		vk_get_instance_proc_addr,    // PFN_vkGetInstanceProcAddr get_instance_proc_addr
		&required_device_extensions,  // const char **required_device_extensions
		0,                            // unsigned num_required_device_extensions
		NULL,                         // const char **required_device_layers
		0,                            // unsigned num_required_device_layers
		&required_features            // const VkPhysicalDeviceFeatures *required_features
	);
	wrapper_environment_log(RETRO_LOG_INFO, "[init_retro_hw_render_context_negotiation_interface_vulkan] create_device: %d\n", result);

	wrapper_environment_log(RETRO_LOG_INFO,
		"[init_retro_hw_render_context_negotiation_interface_vulkan] create_device result:\n"
		"	VkInstance instance: %016x\n"
		"	VkPhysicalDevice gpu: %016x\n"
		"	VkDevice device: %016x\n"
		"	VkQueue queue: %016x\n"
		"	unsigned queue_family_index: %i\n"
		"	VkQueue presentation_queue: %016x\n"
		"	unsigned presentation_queue_family_index: %i\n",
		&context.gpu,
		&context.device,
		&context.queue,
		context.queue_family_index,
		&context.presentation_queue,
		context.presentation_queue_family_index
	);

	// 2- Now we have to call context_reset to finalize the initialization
	hw_render_callback.get_proc_address = vk_get_instance_proc_addr;
	hw_render_callback.context_reset();
	wrapper_environment_log(RETRO_LOG_INFO, "[init_retro_hw_render_context_negotiation_interface_vulkan] Vulkan context_reset done\n");

	return true;
}

struct retro_hw_render_interface_vulkan* get_vulkan_interface() {

	hw_render_interface.interface_type = RETRO_HW_RENDER_INTERFACE_VULKAN;
	hw_render_interface.interface_version = RETRO_HW_RENDER_INTERFACE_VULKAN_VERSION;
	hw_render_interface.instance = vkInstance;
	hw_render_interface.gpu = context.gpu;
	hw_render_interface.device = context.device;
	hw_render_interface.queue = context.queue;
	hw_render_interface.queue_index = context.queue_family_index;
	hw_render_interface.get_instance_proc_addr = vk_get_instance_proc_addr;
	hw_render_interface.get_device_proc_addr = vk_get_device_proc_addr;

	hw_render_interface.set_image = vulkan_set_image;
	hw_render_interface.get_sync_index = vulkan_get_sync_index;
	hw_render_interface.get_sync_index_mask = vulkan_get_sync_index_mask;
	hw_render_interface.wait_sync_index = vulkan_wait_sync_index;
	hw_render_interface.set_command_buffers = vulkan_set_command_buffers;
	hw_render_interface.lock_queue = vulkan_lock_queue;
	hw_render_interface.unlock_queue = vulkan_unlock_queue;
	hw_render_interface.set_signal_semaphore = vulkan_set_signal_semaphore;

	return &hw_render_interface;
}