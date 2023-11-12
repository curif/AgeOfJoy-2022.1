#ifndef IMAGE_CONVERSION_H
#define IMAGE_CONVERSION_H

#ifdef __cplusplus
extern "C" {
#endif

#include <stddef.h>
unsigned char *wrapper_image_conversion_convertXRGB8888ToRGB565(const void *data, unsigned width, 
                                                                    unsigned height, size_t pitch, 
                                                                    int idxBuf);
                                                                    
unsigned char* wrapper_image_conversion_convert0RGB1555ToRGB565(const void *data, unsigned width, 
                                                                    unsigned height, size_t pitch,
                                                                    int idxBuf);
unsigned char *wrapper_image_preserve(const void *data, size_t size,
                                      int idxBuf);
unsigned char *storeRGB565Image(const void *data, unsigned width, unsigned height, size_t pitch, int idxBuf);


#ifdef __cplusplus
}
#endif

#endif
