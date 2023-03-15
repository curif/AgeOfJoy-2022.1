

#ifndef ENVIRONMENT_H
#define ENVIRONMENT_H

#ifdef __cplusplus
extern "C" {
#endif

#include <dlfcn.h>
#include <stdarg.h>
#include <stddef.h>
#include <stdint.h>
#include <stdio.h>

#include "libretro.h"

void wrapper_environment_init(retro_log_printf_t log, char *_save_directory,
                              char *_system_directory, char *_sample_rate);
void wrapper_environment_set_game_parameters(char *_gamma, char *_brightness);
bool wrapper_environment_cb(unsigned cmd, void *data);
enum retro_pixel_format wrapper_environment_get_pixel_format();
char *wrapper_environment_log(enum retro_log_level, char *format, ...);
void wrapper_environment_get_av_info();
double wrapper_environment_get_fps();
double wrapper_environment_get_sample_rate();

#ifdef __cplusplus
}
#endif

#endif
