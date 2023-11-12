
#include "image_conversion.h"

#define MAX_OUTPUT_SIZE (1024 * 768 * 2)
static unsigned char outputData[2][MAX_OUTPUT_SIZE];

void wrapper_image_initialize()
{
  memset(outputData, 0, sizeof(outputData));
  return;
}

unsigned char *wrapper_image_conversion_convertXRGB8888ToRGB565(
    const void *data, unsigned width, unsigned height, size_t pitch,
    int idxBuf) {
  // Allocate the output buffer if it hasn't been allocated yet
  if ((width * height * 2) > MAX_OUTPUT_SIZE) {
    return NULL; // failure
  }

  int inputOffset = 0;
  int outputOffset = 0;
  unsigned char *outputRow = outputData[idxBuf];

  for (int y = 0; y < height; y++) {
    unsigned char *inputRow = (unsigned char *)data + inputOffset;

    for (int x = 0; x < width; x++) {
      unsigned char r = inputRow[2];
      unsigned char g = inputRow[1];
      unsigned char b = inputRow[0];

      // Pack the RGB565 values
      unsigned short rgb565 = ((r & 0xF8) << 8) | ((g & 0xFC) << 3) | (b >> 3);

      // Store the packed RGB565 value in the output buffer
      *(unsigned short *)outputRow = rgb565;

      // Move to the next pixel
      inputRow += 4;
      outputRow += 2;
    }

    // Move to the next row
    inputOffset += pitch;
    outputOffset += width * 2;
  }

  return outputData[idxBuf]; // Image conversion successful
}

unsigned char *wrapper_image_conversion_convert0RGB1555ToRGB565(
    const void *data, unsigned width, unsigned height, size_t pitch, int idxBuf)

{
  // Check if the output buffer is large enough to store the converted image
  if ((width * height * 2) > MAX_OUTPUT_SIZE) {
    return NULL; // failure
  }

  int inputOffset = 0;
  int outputOffset = 0;
  for (int y = 0; y < height; y++) {
    unsigned char *inputRow = (unsigned char *)data + inputOffset;
    unsigned short *outputRow =
        (unsigned short *)(outputData[idxBuf] + outputOffset);

    for (int x = 0; x < width; x++) {
      unsigned short pixel = *(unsigned short *)inputRow;
      unsigned char r = ((pixel >> 10) & 0x1F) << 3;
      unsigned char g = ((pixel >> 5) & 0x1F) << 2;
      unsigned char b = (pixel & 0x1F) << 3;
      *outputRow++ = (unsigned short)((r << 8) | (g << 3) | (b >> 3));
      inputRow += 2;
    }

    inputOffset += pitch;
    outputOffset += width * 2;
  }

  // Return the pointer to the output buffer
  return outputData[idxBuf];
}

unsigned char *wrapper_image_preserve(const void *data, size_t size,
                                      int idxBuf) {
  if (size > MAX_OUTPUT_SIZE)
    return NULL; // failure

  memcpy(outputData[idxBuf], data, size);
  return outputData[idxBuf];
}

// Function to store a RGB565 image in memory without pitch
unsigned char *storeRGB565Image(const void *data, unsigned width, unsigned height, size_t pitch, int idxBuf) {
    // Calculate the size needed for the final image data
    size_t image_size = width * height * 2; // 2 bytes per pixel for RGB565
    if (image_size > MAX_OUTPUT_SIZE)
        return NULL; // failure

    const unsigned char *src_data = (const unsigned char *)data;
    unsigned char *dst_data = outputData[idxBuf];

    for (unsigned y = 0; y < height; y++) {
        // Copy a row from the source image to the destination image
        memcpy(dst_data, src_data, width * 2); // 2 bytes per pixel
        src_data += pitch; // Move to the next row in the source image
        dst_data += width * 2; // Move to the next row in the destination image
    }

    return outputData[idxBuf];
}
