#include <dlfcn.h>
#include "image.h"
#include "image_conversion.h"
#include "environment.h"

//#define IMAGE_DEBUG

static CreateTexture CreateTextureCB;
static LoadTextureData LoadTextureDataCB;
static char create_texture_called;
static unsigned image_width;
static unsigned image_height;

struct 
{
  void* handle;
  void (*retro_set_video_refresh)(retro_video_refresh_t);
} handlers;

void wrapper_image_init()
{
  printf("[wrapper_image_init]\n");
  CreateTextureCB = NULL;
  LoadTextureDataCB = NULL;

  handlers.handle = dlopen("mame2003_plus_libretro_android.so", RTLD_LAZY);
  handlers.retro_set_video_refresh = (void (*)(retro_video_refresh_t))dlsym(handlers.handle, "retro_set_video_refresh");
  handlers.retro_set_video_refresh(&wrapper_image_video_refresh_cb);

}

void wrapper_image_set_texture_cb(CreateTexture createTexture, LoadTextureData loadTextureData)
{
  CreateTextureCB = createTexture;
  LoadTextureDataCB = loadTextureData;
}


void wrapper_image_prev_load_game()
{
  create_texture_called = 0;
  image_width = 0;
  image_height = 0;
}

#ifdef IMAGE_DEBUG
def IMAGE_DEBUGint debug_sum(const void *data, unsigned width, unsigned height, size_t pitch)
{
    int sum = 0;
    uint8_t* pData = (uint8_t*)data;

    //wrapper_environment_log("[cwraper.sum] (%i, %i - %i)\n", width, height, pitch);

    for (uint32_t row = 0; row < height; row++)
    {
        uint8_t* pRow = pData + (row * pitch);

        for (uint32_t col = 0; col < width; col++)
        {
            sum += *pRow;
            pRow++;
        }
    }

    return sum;
}
#endif

void wrapper_image_video_refresh_cb(const void *data, unsigned width, unsigned height, size_t pitch)
{
  if (!data)
    return;
  
  if (!CreateTextureCB || !LoadTextureDataCB)
    return;

  enum retro_pixel_format pixel_format = wrapper_environment_get_pixel_format();
  if (pixel_format == RETRO_PIXEL_FORMAT_UNKNOWN)
    return;

  if (!create_texture_called || image_width != width || image_height != height)
  {
    create_texture_called = (char)1;
    wrapper_environment_log(RETRO_LOG_INFO, "[wrapper_image_video_refresh_cb] create texture (%u, %u) pitch: %u\n", width, height, pitch);
    CreateTextureCB(width, height);
    image_height = height;
    image_width = width;
  }

  #ifdef IMAGE_DEBUG
  wrapper_environment_log(RETRO_LOG_INFO, "[wrapper_image_video_refresh_cb] image sum %i\n", debug_sum(data, width, height, pitch));
  #endif /* ifdef IMAGE_DEBUG */

  unsigned char *imageBuf = NULL;
  if (pixel_format == RETRO_PIXEL_FORMAT_0RGB1555)
  {
    imageBuf = wrapper_image_conversion_convert0RGB1555ToRGB565(data, width, height, pitch);
    if (imageBuf)
      LoadTextureDataCB(imageBuf, width * height * 2);
  }
  else if (pixel_format == RETRO_PIXEL_FORMAT_XRGB8888)
  {
    imageBuf = wrapper_image_conversion_convertXRGB8888ToRGB565(data, width, height, pitch);
    if (imageBuf)
      LoadTextureDataCB(imageBuf, width * height * 2);
  }
  else {
    LoadTextureDataCB((unsigned char *)data, height * pitch);
  }
}
