#ifndef IMAGE_H
#define IMAGE_H

#include <stdio.h>
#include <stdlib.h>
#include <stdint.h> // Include for int16_t type
#include <string.h> // Include for memcpy
#include <stddef.h>
#include "libretro.h"
#include "environment.h"

#ifdef __cplusplus
extern "C" {
#endif

typedef void (*AudioBufferLock)();
typedef void (*AudioBufferUnlock)();

// See audio.c: sets the resampler output rate to the measured consumer
// (Unity audio thread) drain rate so the ring buffer doesn't overflow.
void wrapper_audio_set_output_rate(double rate);

#ifdef __cplusplus
}
#endif

#endif

