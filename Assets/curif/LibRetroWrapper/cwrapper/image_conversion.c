#include <stddef.h>

#include "image_conversion.h"

#define MAX_OUTPUT_SIZE (1024 * 768 * 2)

static unsigned char outputData[MAX_OUTPUT_SIZE];

unsigned char *wrapper_image_conversion_convertXRGB8888ToRGB565(const void *data, unsigned width, unsigned height, size_t pitch)
{
    // Allocate the output buffer if it hasn't been allocated yet
    if ((width * height * 2) > MAX_OUTPUT_SIZE)
    {
        return NULL; //failure
    }

    int inputOffset = 0;
    int outputOffset = 0;
    unsigned char *outputRow = outputData;

    for (int y = 0; y < height; y++)
    {
        unsigned char *inputRow = (unsigned char *)data + inputOffset;

        for (int x = 0; x < width; x++)
        {
            unsigned char r = inputRow[2];
            unsigned char g = inputRow[1];
            unsigned char b = inputRow[0];

            // Pack the RGB565 values
            unsigned short rgb565 = ((r & 0xF8) << 8) | ((g & 0xFC) << 3) | (b >> 3);

            // Store the packed RGB565 value in the output buffer
            *(unsigned short*)outputRow = rgb565;

            // Move to the next pixel
            inputRow += 4;
            outputRow += 2;
        }

        // Move to the next row
        inputOffset += pitch;
        outputOffset += width * 2;
    }

    return outputData; // Image conversion successful
}

unsigned char* wrapper_image_conversion_convert0RGB1555ToRGB565(const void *data, unsigned width, unsigned height, size_t pitch)
{
    // Check if the output buffer is large enough to store the converted image
    if ((width * height * 2) > MAX_OUTPUT_SIZE)
    {
        return NULL; //failure
    }

    int inputOffset = 0;
    int outputOffset = 0;
    for (int y = 0; y < height; y++)
    {
        unsigned char* inputRow = (unsigned char *)data + inputOffset;
        unsigned short* outputRow = (unsigned short*)(outputData + outputOffset);

        for (int x = 0; x < width; x++)
        {
            unsigned short pixel = *(unsigned short*)inputRow;
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
    return outputData;
}

