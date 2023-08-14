#include "environment.h"

#define LOG_BUFFER_SIZE 4096
//#define ENVIRONMENT_DEBUG

struct handlers_struct {
  void *handle;
  void (*retro_set_environment)(retro_environment_t);
  void (*retro_get_system_av_info)(struct retro_system_av_info *info);
  retro_log_printf_t log;
} handlers;
static enum retro_pixel_format pixel_format;
static char system_directory[500];
static char save_directory[500];
static char gamma[50];
static char brightness[50];
static char sample_rate[50];
static struct retro_game_geometry geometry;
static struct retro_system_av_info av_info;
static char log_buffer[LOG_BUFFER_SIZE];
static int xy_control_type; //0 mouse, 1 ligthgun

enum retro_pixel_format wrapper_environment_get_pixel_format() {
  return pixel_format;
}

char *wrapper_environment_log(enum retro_log_level level, char *format, ...) {
  // is preferable to send a string than many pointers to unknown variable types
  // (fail often)
  va_list args;

  va_start(args, format);
  vsnprintf(log_buffer, LOG_BUFFER_SIZE, format, args);
  va_end(args);

  log_buffer[LOG_BUFFER_SIZE - 1] = '\0';
  handlers.log(level, log_buffer);
  return log_buffer;
}
int wrapper_environment_init(retro_log_printf_t log, char *_save_directory,
                              char *_system_directory, char *_sample_rate
                            ) {
  memset(&handlers, 0, sizeof(struct handlers_struct));
  pixel_format = RETRO_PIXEL_FORMAT_UNKNOWN;

  
  handlers.log = log;
  char *core = "mame2003_plus_libretro_android.so";
  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_init] start -----------\n");
  handlers.handle = dlopen(core, RTLD_LAZY);
  if ( handlers.handle == NULL)
  {
    const char* error = dlerror();
    if (error != NULL) {
        wrapper_environment_log(RETRO_LOG_ERROR, "dlopen Error: %s\n", error);
    }
    wrapper_environment_log(RETRO_LOG_ERROR,
                          "[wrapper_environment_init] dlopen %s failed\n", core);
    return -1;
  }
  
  
  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_init] dlsym retro_set_environment\n");
  handlers.retro_set_environment = (void (*)(retro_environment_t))dlsym(
      handlers.handle, "retro_set_environment");
  if ( handlers.retro_set_environment == NULL)
  {  
    wrapper_environment_log(RETRO_LOG_ERROR,
                          "[wrapper_environment_init] retro_set_environment not found\n");
    return -1;
  }

  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_init] dlsym retro_get_system_av_info\n");
  handlers.retro_get_system_av_info =
      (void (*)(struct retro_get_system_av_info *))dlsym(
          handlers.handle, "retro_get_system_av_info");
  if ( handlers.retro_get_system_av_info == NULL)
  {  
    wrapper_environment_log(RETRO_LOG_ERROR,
                          "[wrapper_environment_init] retro_get_system_av_info not found\n");
    return -1;
  }
  
  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_init] call retro_set_environment\n");
  handlers.retro_set_environment(&wrapper_environment_cb);

  strncpy(save_directory, _save_directory, 500);
  strncpy(system_directory, _system_directory, 500);
  strncpy(sample_rate, _sample_rate, 50);

  memset(&av_info, 0, sizeof(struct retro_system_av_info));

  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_init] sample_rate: %s\n",
                          sample_rate);
  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_init] save_directory: %s\n",
                          save_directory);
  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_init] system_directory: %s\n",
                          system_directory);
  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_init] end ----------\n");

  return 0;

}

void wrapper_environment_set_game_parameters(char *_gamma, char *_brightness, int _xy_control_type) {
  strncpy(gamma, _gamma, 50);
  strncpy(brightness, _brightness, 50);
  xy_control_type = _xy_control_type;

  wrapper_environment_log(
      RETRO_LOG_INFO,
      "[wrapper_environment_set_game_parameters] gamma: %s brightness: %s XY control type: %i \n",
      gamma, brightness, xy_control_type);
}

void wrapper_environment_get_av_info() {
  handlers.retro_get_system_av_info(&av_info);
}

double wrapper_environment_get_fps() { return av_info.timing.fps; }
double wrapper_environment_get_sample_rate() {
  return av_info.timing.sample_rate;
}

void wrapper_environment_log_geometry() {
  char *fmt = "[wrapper_environment_log_geometry] Geo:\n"
              "    base_width: %i\n"
              "    base_height: %i\n"
              "    max_width: %i\n"
              "    max_height: %i\n"
              "    aspect_ratio: %f\n";
  wrapper_environment_log(
      RETRO_LOG_INFO, fmt, av_info.geometry.base_width,
      av_info.geometry.base_height, av_info.geometry.max_width,
      av_info.geometry.max_height, av_info.geometry.aspect_ratio);
}

bool wrapper_environment_cb(unsigned cmd, void *data) {
  struct retro_variable *var;
  unsigned int *ptr;

#ifdef ENVIRONMENT_DEBUG
  wrapper_environment_log(RETRO_LOG_INFO, "[wrapper_environment_cb] cmd: %i\n",
                          (int)cmd);
#endif /* ifdef ENVIRONMENT_DEBUG */

  switch (cmd) {
  case RETRO_ENVIRONMENT_GET_VARIABLE:
    // 15
    if (!data)
      return false;

    var = (struct retro_variable *)data;
    if (!var->key)
      return false;

#ifdef ENVIRONMENT_DEBUG
    wrapper_environment_log(RETRO_LOG_INFO,
                            "[wrapper_environment_cb] get var: %s", var->key);
#endif /* ifdef ENVIRONMENT_DEBUG */

    if (strcmp(var->key, "mame2003-plus_skip_disclaimer") == 0) {
      var->value = "enabled";
      wrapper_environment_log(RETRO_LOG_INFO,
                              "[wrapper_environment_cb] get var: %s: %s", var->key, var->value);
      return true;
    } else if (strcmp(var->key, "mame2003-plus_skip_warnings") == 0) {
      var->value = "enabled";
      wrapper_environment_log(RETRO_LOG_INFO,
                              "[wrapper_environment_cb] get var: %s: %s", var->key, var->value);
      return true;
    } else if (strcmp(var->key, "mame2003-plus_xy_device") == 0) {
      if (xy_control_type == 0)
        var->value = "mouse";
      else   
        var->value = "lightgun";
      wrapper_environment_log(RETRO_LOG_INFO,
                              "[wrapper_environment_cb] get var: %s: %s", var->key, var->value);
      return true;
    } else if (strcmp(var->key, "mame2003-plus_gamma") == 0) {
      var->value = gamma;
      wrapper_environment_log(RETRO_LOG_INFO,
                              "[wrapper_environment_cb] get var: %s: %s", var->key, var->value);
      return true;
    } else if (strcmp(var->key, "mame2003-plus_brightness") == 0) {
      var->value = brightness;
      wrapper_environment_log(RETRO_LOG_INFO,
                              "[wrapper_environment_cb] get var: %s: %s", var->key, var->value);
      return true;
    } else if (strcmp(var->key, "mame2003-plus_sample_rate") == 0) {
      var->value = sample_rate;
      wrapper_environment_log(RETRO_LOG_INFO,
                              "[wrapper_environment_cb] get var: %s: %s", var->key, var->value);
      return true;
    } else if (strcmp(var->key, "mame2003-plus_mame_remapping") == 0) {
      var->value = "enabled";
      wrapper_environment_log(RETRO_LOG_INFO,
                              "[wrapper_environment_cb] get var: %s: %s", var->key, var->value);
      return true;
    } else if (strcmp(var->key, "mame2003-plus_vector_intensity") == 0) {
      var->value = "2.0";
      wrapper_environment_log(RETRO_LOG_INFO,
                              "[wrapper_environment_cb] get var: %s: %s", var->key, var->value);
      return true;
    } /*else if (strcmp(var->key, "mame2003-plus_cpu_clock_scale") == 0) {
      // https://github.com/libretro/mame2003-plus-libretro/blob/5a4eb1e4da0788d265e28480568cbbb92ddd4a84/src/mame2003/core_options.c#L718
      var->value = "200";
      wrapper_environment_log(RETRO_LOG_INFO,
                              "[wrapper_environment_cb] get var: %s: %s", var->key, var->value);
      return true;*/
    } else if (strcmp(var->key, "mame2003-plus_vector_vector_translusency") == 0) {
      var->value = "disabled";
      wrapper_environment_log(RETRO_LOG_INFO,
                              "[wrapper_environment_cb] get var: %s: %s", var->key, var->value);
      return true;
    }
    return false;

  case RETRO_ENVIRONMENT_GET_LOG_INTERFACE:
    if (!data || !handlers.log)
      return false;
    ((struct retro_log_callback *)data)->log = handlers.log;
    return true;

  case RETRO_ENVIRONMENT_SET_PIXEL_FORMAT:
    // 1
    ptr = (unsigned int *)data;
    pixel_format = (enum retro_pixel_format) * ptr;
    wrapper_environment_log(RETRO_LOG_INFO,
                            "[RETRO_ENVIRONMENT_SET_PIXEL_FORMAT] %i\n",
                            pixel_format);
    return true;

  case RETRO_ENVIRONMENT_GET_SYSTEM_DIRECTORY:
    // 9
    if (!data)
      return false;
    *(char **)data = system_directory;
    return true;

  case RETRO_ENVIRONMENT_GET_SAVE_DIRECTORY:
    // 31
    if (!data)
      return false;
    *(char **)data = save_directory;
    return true;

  case RETRO_ENVIRONMENT_SET_ROTATION:
    // rotation should be specified in CDL
    ptr = (unsigned int *)data;
    wrapper_environment_log(RETRO_LOG_INFO,
                            "[RETRO_ENVIRONMENT_SET_ROTATION] %i\n", *ptr);
    return true;

  case RETRO_ENVIRONMENT_SET_GEOMETRY:
    if (!data)
      return false;
    memcpy(&av_info.geometry, (struct retro_game_geometry *)data,
           sizeof(struct retro_game_geometry));
    wrapper_environment_log_geometry();

    return true;
  }
  return false;
}
