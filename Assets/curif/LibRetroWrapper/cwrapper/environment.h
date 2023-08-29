

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
#include <string.h>

#include "libretro.h"

typedef void (RETRO_CALLCONV *wrapper_log_printf_t)(enum retro_log_level level, const char *value);
int wrapper_environment_init(wrapper_log_printf_t log, 
                              enum retro_log_level _minLogLevel, char *_save_directory,
                              char *_system_directory, char *_sample_rate);
void wrapper_environment_set_game_parameters(char *_gamma, char *_brightness, int _xy_control_type);
bool wrapper_environment_cb(unsigned cmd, void *data);
enum retro_pixel_format wrapper_environment_get_pixel_format();
void wrapper_environment_log(enum retro_log_level, char *format, ...);
void wrapper_environment_get_av_info();
double wrapper_environment_get_fps();
double wrapper_environment_get_sample_rate();
int wrapper_system_info_need_full_path();
int wrapper_retro_load_game(char *path,  char *_gamma, char *_brightness, int _xy_control_type);

#ifdef __cplusplus
}
#endif

#endif
