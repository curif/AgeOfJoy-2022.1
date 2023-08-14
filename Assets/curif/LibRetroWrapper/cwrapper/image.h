
#ifndef IMAGE_H
#define IMAGE_H

#include "libretro.h"
#include <stddef.h>
#include <pthread.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef void (*CreateTexture)(unsigned width, unsigned height);
typedef void (*LoadTextureData)(const void *data, unsigned size);
typedef void (*TextureLock)();
typedef void (*TextureUnlock)();

void wrapper_image_init();
void wrapper_image_prev_load_game();
void wrapper_image_set_texture_cb(CreateTexture createTexture,  
                                    TextureLock textureLock, TextureUnlock textureUnlock);
void wrapper_image_video_refresh_cb(const void *data, unsigned width, unsigned height, size_t pitch);

void wrapper_image_lock();
void wrapper_image_unlock();
unsigned char *wrapper_image_get_buffer();
unsigned wrapper_image_get_buffer_size();

#ifdef __cplusplus
}
#endif

#endif

