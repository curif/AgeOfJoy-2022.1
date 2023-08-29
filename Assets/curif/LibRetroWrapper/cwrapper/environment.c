#include "environment.h"

// #define ENVIRONMENT_DEBUG

struct handlers_struct {
  void *handle;
  void (*retro_set_environment)(retro_environment_t);
  void (*retro_get_system_av_info)(struct retro_system_av_info *info);
  void (*retro_get_system_info)(struct retro_system_info *info);
  void (*retro_init)();
  void (*retro_deinit)();
  bool (*retro_load_game)(struct retro_game_info *game);
  void (*retro_unload_game)();
  void (*retro_set_controller_port_device)(unsigned port, unsigned device);
  wrapper_log_printf_t log;
} handlers;
static enum retro_pixel_format pixel_format;
static char system_directory[500];
static char save_directory[500];
static char game_path[500];
static char gamma[50];
static char brightness[50];
static char sample_rate[50];
static char enabled[10];
static char disabled[10];
static char xy_input_type[15];
static char vector_intensity[5];

static struct retro_game_geometry geometry;
static struct retro_system_info system_info;
static struct retro_system_av_info av_info;
static struct retro_game_info game_info;

#define LOG_BUFFER_SIZE 4096
static char log_buffer[LOG_BUFFER_SIZE];
static enum retro_log_level minLogLevel;

enum retro_pixel_format wrapper_environment_get_pixel_format() {
  return pixel_format;
}

void wrapper_environment_log(enum retro_log_level level, char *format, ...) {
  // is preferable to send a string than many pointers to unknown variable types
  // (fail often)
  va_list args;
  
  if (!handlers.log || level < minLogLevel)
  {
    return;
  }

  memset(log_buffer, 0, LOG_BUFFER_SIZE);
  va_start(args, format);
  vsnprintf(log_buffer, LOG_BUFFER_SIZE, format, args);
  va_end(args);

  handlers.log(level, log_buffer);
  return;
}
int wrapper_environment_init(wrapper_log_printf_t log, 
                              enum retro_log_level _minLogLevel, 
                              char *_save_directory,
                              char *_system_directory, 
                              char *_sample_rate
                            ) {
  memset(&handlers, 0, sizeof(struct handlers_struct));
  pixel_format = RETRO_PIXEL_FORMAT_UNKNOWN;
  
  handlers.log = log;
  minLogLevel = _minLogLevel;

  // load core.
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
  if (handlers.retro_set_environment == NULL)
  {
    wrapper_environment_log(RETRO_LOG_ERROR,
                          "[wrapper_environment_init] retro_set_environment not found\n");
    return -1;
  }

  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_init] dlsym retro_get_system_av_info\n");
  handlers.retro_get_system_av_info =
      (void (*)(struct retro_system_av_info *))dlsym(handlers.handle, "retro_get_system_av_info");
  if ( handlers.retro_get_system_av_info == NULL)
  {  
    wrapper_environment_log(RETRO_LOG_ERROR,
                          "[wrapper_environment_init] retro_get_system_av_info not found\n");
    return -1;
  }

  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_init] dlsym retro_get_system_info\n");
  handlers.retro_get_system_info =
      (void (*)(struct retro_system_info *))dlsym(
          handlers.handle, "retro_get_system_info");
  if ( handlers.retro_get_system_info == NULL)
  {  
    wrapper_environment_log(RETRO_LOG_ERROR,
                          "[wrapper_environment_init] retro_get_system_info not found\n");
    return -1;
  }

  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_init] dlsym retro_init\n");
  handlers.retro_init = (void (*)(void))dlsym(handlers.handle, "retro_init");
  if (handlers.retro_init == NULL)
  {  
    wrapper_environment_log(RETRO_LOG_ERROR,
                          "[wrapper_environment_init] retro_init not found\n");
    return -1;
  }

  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_init] dlsym retro_deinit\n");
  handlers.retro_deinit = (void (*)(void))dlsym(handlers.handle, "retro_deinit");
  if (handlers.retro_deinit == NULL)
  {  
    wrapper_environment_log(RETRO_LOG_ERROR,
                          "[wrapper_environment_init] retro_deinit not found\n");
    return -1;
  }
  
  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_init] dlsym retro_load_game\n");
  handlers.retro_load_game = (bool (*)(struct retro_game_info *))dlsym(handlers.handle, "retro_load_game");
  if (handlers.retro_load_game == NULL)
  {  
    wrapper_environment_log(RETRO_LOG_ERROR,
                          "[wrapper_environment_init] retro_load_game not found\n");
    return -1;
  }
  
  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_init] dlsym retro_unload_game\n");
  handlers.retro_unload_game = (void (*)())dlsym(handlers.handle, "retro_unload_game");
  if (handlers.retro_unload_game == NULL)
  {  
    wrapper_environment_log(RETRO_LOG_ERROR,
                          "[wrapper_environment_init] retro_unload_game not found\n");
    return -1;
  }

  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_init] dlsym retro_set_controller_port_device\n");
  handlers.retro_set_controller_port_device = 
      (void (*)(unsigned port, unsigned device))dlsym(handlers.handle, "retro_set_controller_port_device");
  if (handlers.retro_set_controller_port_device == NULL)
  {  
    wrapper_environment_log(RETRO_LOG_ERROR,
                          "[wrapper_environment_init] retro_set_controller_port_device not found\n");
    return -1;
  }

  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_init] call retro_set_environment\n");
  handlers.retro_set_environment(&wrapper_environment_cb);

  memset(save_directory, 0, 500);
  memset(system_directory, 0, 500);
  memset(sample_rate, 0, 50);

  strncpy(save_directory, _save_directory, 500);
  strncpy(system_directory, _system_directory, 500);
  strncpy(sample_rate, _sample_rate, 50);

  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_init] sample_rate: %s\n",
                          sample_rate);
  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_init] save_directory: %s\n",
                          save_directory);
  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_init] system_directory: %s\n",
                          system_directory);
  
  handlers.retro_get_system_info(&system_info);
  wrapper_environment_log(RETRO_LOG_INFO,
                        "[_retro_get_system_info] Library Name: %s\n"
                        "Library Version: %s\n"
                        "valid_extensions: %s\n"
                        "need_fullpath: %i\n", 
                        system_info.library_name, system_info.library_version, 
                        system_info.valid_extensions,  (int)system_info.need_fullpath);


  wrapper_environment_log(RETRO_LOG_INFO, "[wrapper_environment_init] end ----------\n");
  return 0;
}

void wrapper_environment_set_game_parameters(char *_gamma, char *_brightness, int _xy_control_type) {
  wrapper_environment_log(RETRO_LOG_INFO, "[wrapper_environment_set_game_parameters]\n");

  memset(gamma, 0, 50);
  memset(brightness, 0, 50);
  strncpy(gamma, _gamma, 50);
  strncpy(brightness, _brightness, 50);
  
  sprintf(enabled, "enabled");
  sprintf(disabled, "disabled");
  sprintf(vector_intensity, "2.0");

  if (_xy_control_type == 0)
    sprintf(xy_input_type, "mouse");
  else   
    sprintf(xy_input_type, "lightgun");

  wrapper_environment_log(
      RETRO_LOG_INFO,
      "[wrapper_environment_set_game_parameters] gamma: %s brightness: %s XY control type: %s \n",
      gamma, brightness, xy_input_type);
}

double wrapper_environment_get_fps() { 
  return av_info.timing.fps; 
}
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

void wrapper_environment_get_av_info() {
  wrapper_environment_log(RETRO_LOG_INFO, "[wrapper_environment_get_av_info]\n");
  memset(&av_info, 0, sizeof(struct retro_system_av_info));
  handlers.retro_get_system_av_info(&av_info);
  wrapper_environment_log_geometry();
}

bool wrapper_environment_cb(unsigned cmd, void *data) {
  struct retro_variable *var;
  unsigned int *ptr;

#ifdef ENVIRONMENT_DEBUG
  wrapper_environment_log(RETRO_LOG_INFO, "[wrapper_environment_cb] cmd: %i\n",(int)cmd);
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
      var->value = enabled;
      wrapper_environment_log(RETRO_LOG_INFO,
                              "[wrapper_environment_cb] get var: %s: %s", var->key, var->value);
      return true;
    } else if (strcmp(var->key, "mame2003-plus_skip_warnings") == 0) {
      var->value = enabled;
      wrapper_environment_log(RETRO_LOG_INFO,
                              "[wrapper_environment_cb] get var: %s: %s", var->key, var->value);
      return true;
    } else if (strcmp(var->key, "mame2003-plus_xy_device") == 0) {
      var->value = xy_input_type;
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
      var->value = enabled;
      wrapper_environment_log(RETRO_LOG_INFO,
                              "[wrapper_environment_cb] get var: %s: %s", var->key, var->value);
      return true;
    } else if (strcmp(var->key, "mame2003-plus_vector_intensity") == 0) {
      var->value = vector_intensity;
      wrapper_environment_log(RETRO_LOG_INFO,
                              "[wrapper_environment_cb] get var: %s: %s", var->key, var->value);
      return true;
   } else if (strcmp(var->key, "mame2003-plus_use_samples") == 0) {
      var->value = enabled;
      wrapper_environment_log(RETRO_LOG_INFO,
                              "[wrapper_environment_cb] get var: %s: %s", var->key, var->value);
      return true;
    /*} else if (strcmp(var->key, "mame2003-plus_cpu_clock_scale") == 0) {
      // https://github.com/libretro/mame2003-plus-libretro/blob/5a4eb1e4da0788d265e28480568cbbb92ddd4a84/src/mame2003/core_options.c#L718
      var->value = "200";
      wrapper_environment_log(RETRO_LOG_INFO,
                              "[wrapper_environment_cb] get var: %s: %s", var->key, var->value);
      return true;*/
    } else if (strcmp(var->key, "mame2003-plus_vector_vector_translusency") == 0) {
      var->value = disabled;
      wrapper_environment_log(RETRO_LOG_INFO,
                              "[wrapper_environment_cb] get var: %s: %s", var->key, var->value);
      return true;
    }
    return false;

  case RETRO_ENVIRONMENT_GET_LOG_INTERFACE:
    if (!data || !handlers.log)
      return false;
    wrapper_environment_log(RETRO_LOG_INFO, "[RETRO_ENVIRONMENT_GET_LOG_INTERFACE]\n");
    // ((struct retro_log_callback *)data)->log = handlers.log;
    ((struct retro_log_callback *)data)->log = &wrapper_environment_log;
    return true;

  case RETRO_ENVIRONMENT_SET_PIXEL_FORMAT:
    // 1
    wrapper_environment_log(RETRO_LOG_INFO, "[RETRO_ENVIRONMENT_SET_PIXEL_FORMAT]\n");
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
    wrapper_environment_log(RETRO_LOG_INFO, "[RETRO_ENVIRONMENT_GET_SYSTEM_DIRECTORY]\n");
    *(char **)data = system_directory;
    return true;

  case RETRO_ENVIRONMENT_GET_SAVE_DIRECTORY:
    // 31
    if (!data)
      return false;
    wrapper_environment_log(RETRO_LOG_INFO, "[RETRO_ENVIRONMENT_GET_SAVE_DIRECTORY]\n");
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
    wrapper_environment_log(RETRO_LOG_INFO, "[RETRO_ENVIRONMENT_SET_GEOMETRY]\n");
    memcpy(&av_info.geometry, (struct retro_game_geometry *)data,
           sizeof(struct retro_game_geometry));
    wrapper_environment_log_geometry();
    return true;
  }
  return false;
}

int wrapper_system_info_need_full_path()
{
    return (int)system_info.need_fullpath;
}

void wrapper_retro_init()
{
   //handlers.retro_set_controller_port_device(0, RETRO_DEVICE_JOYPAD);
    wrapper_environment_log(RETRO_LOG_INFO, "[wrapper_retro_init]\n");
    handlers.retro_init();
}

void wrapper_retro_deinit()
{
    handlers.retro_deinit();
}

int wrapper_load_game(char *path, char *_gamma, char *_brightness, int _xy_control_type)
{
  wrapper_environment_log(RETRO_LOG_INFO, "[wrapper_load_game]\n");
  memset(&game_info, 0, sizeof(struct retro_game_info));
  memset(game_path, 0, 500);
  strncpy(game_path, path, 500);
  game_info.path = game_path;
  
  wrapper_image_prev_load_game();
  wrapper_environment_set_game_parameters(_gamma, _brightness, _xy_control_type); //0 mouse, 1 ligthgun

  wrapper_environment_log(RETRO_LOG_INFO, "[wrapper_load_game] (%s)--\n", game_info.path);
  bool ret = handlers.retro_load_game(&game_info);
  if (ret)
  {
    wrapper_environment_get_av_info();
  }

  wrapper_environment_log(RETRO_LOG_INFO, "[wrapper_load_game] END (ret:%i)\n", ret);
  return (int)ret;
}
void wrapper_unload_game()
{
    //https://github.com/libretro/mame2000-libretro/blob/6d0b1e1fe287d6d8536b53a4840e7d152f86b34b/src/libretro/libretro.c#L1054
  wrapper_environment_log(RETRO_LOG_INFO, "[retro_unload_game]\n");
  handlers.retro_unload_game();
}