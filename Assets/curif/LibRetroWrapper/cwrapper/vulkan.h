#ifndef VULKAN_H
#define VULKAN_H

#ifdef __cplusplus
extern "C" {
#endif

#include <dlfcn.h>
#include <stdarg.h>
#include <stddef.h>
#include <stdint.h>
#include <stdio.h>
#include <string.h>

#include "libretro.h"
#include "libretro_vulkan.h"

	// stupid visual studio doesn't know what vulkan_core.h is so we have to include it here
#ifndef VULKAN_CORE_H_
#include "c:\Program Files\Unity\Hub\Editor\2022.3.18f1\Editor\Data\PlaybackEngines\AndroidPlayer\NDK\toolchains\llvm\prebuilt\windows-x86_64\sysroot\usr\include\vulkan\vulkan_core.h"
#endif

	typedef void (*VulcanImageCB)(void* vkImage);

	bool init_retro_hw_render_callback(struct retro_hw_render_callback* callback);
	bool init_retro_hw_render_context_negotiation_interface_vulkan(struct retro_hw_render_context_negotiation_interface_vulkan* interface);
	struct retro_hw_render_interface_vulkan* get_vulkan_interface();
	void wrapper_set_vulkan_image_cb(VulcanImageCB vulcanImageCB);

#ifdef __cplusplus
}
#endif

#endif
