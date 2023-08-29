#include <dlfcn.h>
#include "image.h"
#include "image_conversion.h"
#include "environment.h"

//#define IMAGE_DEBUG

static CreateTexture CreateTextureCB;
static TextureLock TextureLockCB;
static TextureUnlock TextureUnlockCB;
static TextureSemAvailable TextureSemAvailableCB;
static char create_texture_called;
static unsigned image_width;
static unsigned image_height;
static unsigned no_draw;
static int bufIdx;
static unsigned char *imageBuffer;
static unsigned imageSize;

struct 
{
  void* handle;
  void (*retro_set_video_refresh)(retro_video_refresh_t);
} image_handlers;

void wrapper_image_init(CreateTexture createTexture, 
                        TextureLock textureLock, 
                        TextureUnlock textureUnlock,
                        TextureSemAvailable textureSemAvailable)
{

  image_handlers.handle = dlopen("mame2003_plus_libretro_android.so", RTLD_LAZY);
  image_handlers.retro_set_video_refresh = (void (*)(retro_video_refresh_t))dlsym(image_handlers.handle,
                                             "retro_set_video_refresh");
  image_handlers.retro_set_video_refresh(&wrapper_image_video_refresh_cb);
  
  no_draw = 0;
  bufIdx = 0;
  imageBuffer = NULL;
  imageSize = 0;

  CreateTextureCB = createTexture;
  TextureLockCB = textureLock;
  TextureUnlockCB = textureUnlock;
  TextureSemAvailableCB = textureSemAvailable;

  no_draw = 0;
}

void wrapper_image_prev_load_game()
{
  create_texture_called = 0;
  image_width = 0;
  image_height = 0;
  bufIdx = 0;
  imageBuffer = NULL;
  imageSize = 0;
}

void wrapper_image_suspend_image(unsigned _no_draw)
{
  no_draw = _no_draw;
}

#ifdef IMAGE_DEBUG
int debug_sum(const void *data, unsigned width, unsigned height, size_t pitch)
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

void swapBuffers(unsigned char *imageBuf, unsigned size)
{
  // wrapper_environment_log(RETRO_LOG_INFO, "[swapBuffers] START\n");
  TextureLockCB();

  imageBuffer = imageBuf;
  imageSize = size;

  bufIdx = (bufIdx + 1) % 2;

  TextureSemAvailableCB();

  TextureUnlockCB();
  // wrapper_environment_log(RETRO_LOG_INFO, "[swapBuffers] END\n");
}

unsigned char *wrapper_image_get_buffer()
{
  return imageBuffer;
}
unsigned wrapper_image_get_buffer_size()
{
  return imageSize;
}

void wrapper_image_video_refresh_cb(const void *data, unsigned width, unsigned height, size_t pitch)
{
  if (!data)
    return;
  
  if (!CreateTextureCB)
    return;
  
  if (no_draw)
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
    imageBuf = wrapper_image_conversion_convert0RGB1555ToRGB565(data, width, height, pitch, bufIdx);
    if (imageBuf)
      swapBuffers(imageBuf, width * height * 2);
  }
  else if (pixel_format == RETRO_PIXEL_FORMAT_XRGB8888)
  {
    imageBuf = wrapper_image_conversion_convertXRGB8888ToRGB565(data, width, height, pitch, bufIdx);
    if (imageBuf)
      swapBuffers(imageBuf, width * height * 2);
  }
  else {
    imageBuf = wrapper_image_preserve(data, width, height, pitch, bufIdx);
    if (imageBuf)
      swapBuffers(imageBuf, height * pitch);
  }
}
