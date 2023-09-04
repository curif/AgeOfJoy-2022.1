
#ifndef IMAGE_H
#define IMAGE_H

#include <stddef.h>
#include "libretro.h"
#include "environment.h"

#ifdef __cplusplus
extern "C" {
#endif

typedef void (*CreateTexture)(unsigned width, unsigned height);
typedef void (*LoadTextureData)(const void *data, unsigned size);
typedef void (*TextureLock)();
typedef void (*TextureUnlock)();
typedef void (*TextureSemAvailable)();

void wrapper_image_init(CreateTexture createTexture,  
                        TextureLock textureLock, 
                        TextureUnlock textureUnlock,
                        TextureSemAvailable textureSemAvailableCB);
void wrapper_image_prev_load_game();
void wrapper_image_video_refresh_cb(const void *data, unsigned width, unsigned height, size_t pitch);

void wrapper_image_lock();
void wrapper_image_unlock();
unsigned char *wrapper_image_get_buffer();
unsigned wrapper_image_get_buffer_size();


#ifdef __cplusplus
}
#endif

#endif

