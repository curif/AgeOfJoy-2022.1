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

	bool init_retro_hw_render_callback(struct retro_hw_render_callback* callback);
	bool init_retro_hw_render_context_negotiation_interface_vulkan(struct retro_hw_render_context_negotiation_interface_vulkan* interface);
	struct retro_hw_render_interface_vulkan* get_vulkan_interface();

#ifdef __cplusplus
}
#endif

#endif
