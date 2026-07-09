// pdvk.h — C ABI for the isolated Vulkan render context (PDDocs/vulkancontext.md).
//
// The library owns its OWN VkInstance / VkDevice (approach B — a genuinely separate Vulkan
// context, NOT Unity's device). It renders a rotating checkerboard into an offscreen image
// and reads it back to a CPU-visible buffer that Unity copies onto a quad. This is the
// Milestone-1 / CPU-blit path; the zero-copy AHardwareBuffer import is a later upgrade.
//
// All functions are no-ops / return failure if the context isn't initialized. The matching
// managed binding is PDUnity/Assets/Script/PdVk.cs — keep them in sync.
#ifndef PDVK_H
#define PDVK_H

#include <stdint.h>

// Force export even under -fvisibility=hidden so the P/Invoke entry points resolve.
#if defined(__GNUC__)
#define PDVK_API __attribute__((visibility("default")))
#else
#define PDVK_API
#endif

#ifdef __cplusplus
extern "C" {
#endif

// Build the isolated Vulkan context and an offscreen RGBA8 render target of w×h. Returns 0
// on success, non-zero on failure (see logcat tag "pdvk"). Safe to call again after
// pdvk_shutdown(); calling twice without shutdown is a no-op that returns 0.
PDVK_API int  pdvk_init(int width, int height);

// Render one checkerboard frame, advancing the rotation by dt seconds, then read the result
// back into the CPU staging buffer. Returns 0 on success. No-op (returns -1) if not inited.
PDVK_API int  pdvk_render(float dt_seconds);

// Hand back a pointer to the most recent frame's pixels (tightly-packed RGBA8, row-major,
// top-to-bottom) plus its dimensions. The pointer is stable for the life of the context
// (persistently mapped) and valid to read after a successful pdvk_render(). Returns 0 on
// success, non-zero if not inited.
PDVK_API int  pdvk_get_pixels(const void** out_pixels, int* out_width, int* out_height);

// Tear down the context and free all Vulkan objects. Idempotent.
PDVK_API void pdvk_shutdown(void);

// --- Step B: zero-copy handoff to Unity's Vulkan device --------------------------------------
// (See claudedocs/geometrizer_vulkan_cores.md.) The render target is AHB-backed; Step B imports
// that SAME AHardwareBuffer onto Unity's VkDevice (reached via IUnityGraphicsVulkan), creates a
// VkImage there, and hands its address to Texture2D.CreateExternalTexture — no CPU copy.

// Event id to pass to GL.IssuePluginEvent (issue once, after pdvk_init, to import the AHB onto
// Unity's device on the render thread).
enum { PDVK_EVENT_IMPORT_AHB = 1 };

// The native render-event callback to hand to GL.IssuePluginEvent. NULL-safe.
PDVK_API void* pdvk_GetRenderEventFunc(void);

// 1 once the Unity-side VkImage import has succeeded, else 0.
PDVK_API int pdvk_unity_image_ready(void);

// Address of the Unity-device VkImage handle (i.e. a VkImage*), for Texture2D.CreateExternalTexture.
// NULL until pdvk_unity_image_ready() == 1. Stays valid until pdvk_shutdown().
PDVK_API const void* pdvk_get_unity_image_ptr(void);

#ifdef __cplusplus
}
#endif

#endif // PDVK_H
