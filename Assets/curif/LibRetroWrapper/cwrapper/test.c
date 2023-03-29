

#include "il2cpp-config.h"
#include "il2cpp-runtime-metadata.h"
#include <stdint.h>
#include <stdio.h>

#include "test.h"

int MyCFunction()
{
    return 42;
}
int sum(const void *data, unsigned width, unsigned height, size_t pitch)
{
    int sum = 0;
    uint8_t* pData = (uint8_t*)data;

    printf("[cwraper.sum] (%i, %i - %i)\n", width, height, pitch);

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
