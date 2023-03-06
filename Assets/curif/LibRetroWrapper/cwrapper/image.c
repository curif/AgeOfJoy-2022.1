#include "image.h"
#include "image_conversion.h"

#include <dlfcn.h>

static CreateTexture CreateTextureCB;
static LoadTextureData LoadTextureDataCB;
static enum retro_pixel_format pixel_format;
static char create_texture_called;

void wrapper_image_init()
{
  CreateTextureCB = NULL;
  LoadTextureDataCB = NULL;
  //retro_set_video_refresh(&wrapper_image_video_refresh_cb);

  void* handle = dlopen("mame2003_plus_libretro_android.so", RTLD_LAZY);
  void (*myFunctionPtr)(retro_video_refresh_t) = (void (*)(retro_video_refresh_t))dlsym(handle, "retro_set_video_refresh");
  myFunctionPtr(&wrapper_image_video_refresh_cb);

}

void wrapper_image_set_texture_cb(CreateTexture createTexture, LoadTextureData loadTextureData)
{
  CreateTextureCB = createTexture;
  LoadTextureDataCB = loadTextureData;
}


void wrapper_image_prev_load_game()
{
  pixel_format = RETRO_PIXEL_FORMAT_UNKNOWN;
  create_texture_called = 0;
}

void wrapper_image_set_pixel_format(enum retro_pixel_format pxf)
{
  pixel_format = pxf;
}

void wrapper_image_video_refresh_cb(const void *data, unsigned width, unsigned height, size_t pitch)
{
  if (!data)
    return;
  
  if (!CreateTextureCB || !LoadTextureDataCB)
    return;
  if (pixel_format == RETRO_PIXEL_FORMAT_UNKNOWN)
    return;

  if (!create_texture_called)
  {
    create_texture_called = (char)1;
    CreateTextureCB(width, height);
  }

  unsigned char *imageBuf = NULL;
  if (pixel_format == RETRO_PIXEL_FORMAT_0RGB1555)
  {
    imageBuf = wrapper_image_conversion_convert0RGB1555ToRGB565(data, width, height, pitch);
    if (imageBuf)
      LoadTextureDataCB(imageBuf, width * height * 2);
  }
  else if (pixel_format == RETRO_PIXEL_FORMAT_XRGB8888)
    imageBuf = wrapper_image_conversion_convertXRGB8888ToRGB565(data, width, height, pitch);
    if (imageBuf)
      LoadTextureDataCB(imageBuf, width * height * 2);
  else {
    LoadTextureDataCB((unsigned char *)data, height * pitch);
  }
}
