
#ifndef IMAGE_H
#define IMAGE_H

#include "libretro.h"
#include <stddef.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef void (*CreateTexture)(unsigned width, unsigned height);
typedef void (*LoadTextureData)(const void *data, unsigned size);

void wrapper_image_init();
void wrapper_image_prev_load_game();
void wrapper_image_set_texture_cb(CreateTexture createTexture, LoadTextureData loadTextureData);
void wrapper_image_video_refresh_cb(const void *data, unsigned width, unsigned height, size_t pitch);

#ifdef __cplusplus
}
#endif

#endif

