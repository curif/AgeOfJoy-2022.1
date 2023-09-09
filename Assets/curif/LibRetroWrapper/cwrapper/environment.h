

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

typedef struct handlers_struct {
  void *handle;
  void (*retro_set_environment)(retro_environment_t);
  void (*retro_get_system_av_info)(struct retro_system_av_info *info);
  void (*retro_get_system_info)(struct retro_system_info *info);
  void (*retro_init)();
  void (*retro_deinit)();
  bool (*retro_load_game)(struct retro_game_info *game);
  void (*retro_unload_game)();
  void (*retro_run)();
  void (*retro_set_controller_port_device)(unsigned port, unsigned device);
  void (*retro_set_video_refresh)(retro_video_refresh_t);
  void (*retro_set_audio_sample)(retro_audio_sample_t);
  void (*retro_set_audio_sample_batch)(retro_audio_sample_batch_t);
  void (*retro_set_input_poll)(retro_input_poll_t);
  void (*retro_set_input_state)(retro_input_state_t);
} handlers_t;

handlers_t *wrapper_environment_get_handlers();
int wrapper_environment_open(wrapper_log_printf_t log, 
                              enum retro_log_level _minLogLevel, char *_save_directory,
                              char *_system_directory, char *_sample_rate,
                              retro_input_state_t _retro_input_state_cb);
void wrapper_environment_init();                              
void wrapper_environment_set_game_parameters(char *_gamma, char *_brightness, int _xy_control_type);
void wrapper_retro_init();
bool wrapper_environment_cb(unsigned cmd, void *data);
enum retro_pixel_format wrapper_environment_get_pixel_format();
void wrapper_environment_log(enum retro_log_level level, const char *format, ...);
void wrapper_environment_get_system_info();
void wrapper_environment_get_av_info();
double wrapper_environment_get_fps();
double wrapper_environment_get_sample_rate();
int wrapper_system_info_need_full_path();
int wrapper_retro_load_game(char *path,  char *_gamma, char *_brightness, int _xy_control_type);

#ifdef __cplusplus
}
#endif

#endif
