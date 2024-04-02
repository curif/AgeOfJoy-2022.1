#include "environment.h"

// #define ENVIRONMENT_DEBUG

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
static char core[500];

static handlers_t handlers;
static wrapper_log_printf_t log;
static retro_input_state_t input_state_cb;
static struct retro_system_info system_info;
static struct retro_system_av_info av_info;
static struct retro_game_info game_info;

static struct retro_hw_render_callback hw_render_callback;
static struct retro_hw_render_context_negotiation_interface_vulkan hw_vulkan_interface;
static struct retro_hw_render_interface_vulkan hw_render_interface;
static struct retro_vulkan_context context;
static VkInstance vkInstance;
static PFN_vkGetInstanceProcAddr vk_get_instance_proc_addr;
static PFN_vkCreateInstance vk_create_instance;
static PFN_vkGetDeviceProcAddr vk_get_device_proc_addr;

#define LOG_BUFFER_SIZE 4096
static char log_buffer[LOG_BUFFER_SIZE];
static enum retro_log_level minLogLevel;

static char* cap32_gfx_colors = "16bit";
static char* cap32_scr_crop = "enabled";
static char* cap32_model = "6128+ (experimental)";


// VULCAN HANDLERS. THESE NEED TO BE IMPLEMENTED (SOMEHOW)

static void vulkan_set_image(void* handle,
    const struct retro_vulkan_image* image,
    uint32_t num_semaphores,
    const VkSemaphore* semaphores,
    uint32_t src_queue_family)
{
    wrapper_environment_log(RETRO_LOG_INFO, "[VULCAN_HANDLERS] vulkan_set_image\n");
}

uint32_t frameIndex= 0;
static uint32_t vulkan_get_sync_index(void* handle)
{
    wrapper_environment_log(RETRO_LOG_INFO, "[VULCAN_HANDLERS] vulkan_get_sync_index\n");
    frameIndex++;
    return frameIndex;
}

static uint32_t vulkan_get_sync_index_mask(void* handle)
{
    wrapper_environment_log(RETRO_LOG_INFO, "[VULCAN_HANDLERS] vulkan_get_sync_index_mask\n");
    return 1;   // no idea !
}

static void vulkan_wait_sync_index(void* handle)
{
    wrapper_environment_log(RETRO_LOG_INFO, "[VULCAN_HANDLERS] vulkan_wait_sync_index\n");
}

static void vulkan_set_command_buffers(void* handle, uint32_t num_cmd, const VkCommandBuffer* cmd)
{
    wrapper_environment_log(RETRO_LOG_INFO, "[VULCAN_HANDLERS] vulkan_set_command_buffers\n");
}

static void vulkan_lock_queue(void* handle)
{
    wrapper_environment_log(RETRO_LOG_INFO, "[VULCAN_HANDLERS] vulkan_lock_queue\n");
}

static void vulkan_unlock_queue(void* handle)
{
    wrapper_environment_log(RETRO_LOG_INFO, "[VULCAN_HANDLERS] vulkan_unlock_queue\n");
}

static void vulkan_set_signal_semaphore(void* handle, VkSemaphore semaphore)
{
    wrapper_environment_log(RETRO_LOG_INFO, "[VULCAN_HANDLERS] vulkan_set_signal_semaphore\n");
}






#define INIT_AND_COPY_STRING(dest, src)                                        \
  do {                                                                         \
    size_t srcLen = strnlen((src), sizeof(dest) - 1);                          \
    strncpy((dest), (src), srcLen);                                            \
    (dest)[srcLen] = '\0';                                                     \
  } while (0)

#define INIT_STRUCT(structure) memset(&structure, 0, sizeof(structure))

enum retro_pixel_format wrapper_environment_get_pixel_format() {
  return pixel_format;
}

handlers_t *wrapper_environment_get_handlers() { return &handlers; }

void wrapper_environment_log(enum retro_log_level level, const char *format,
                             ...) {
  // is preferable to send a string than many pointers to unknown variable types
  // (fail often)
  va_list args;

  if (!log || level < minLogLevel) {
    return;
  }

  memset(log_buffer, 0, LOG_BUFFER_SIZE);
  va_start(args, format);
  vsnprintf(log_buffer, LOG_BUFFER_SIZE, format, args);
  va_end(args);

  log(level, log_buffer);
  return;
}

int wrapper_dlopen() {
  INIT_STRUCT(handlers);
  handlers.handle = dlopen(core, RTLD_LAZY | RTLD_LOCAL);
  if (handlers.handle == NULL) {
    const char *error = dlerror();
    if (error != NULL) {
      wrapper_environment_log(RETRO_LOG_ERROR, "core: %s dlopen Error: %s\n",
                              core, error);
    }
    wrapper_environment_log(RETRO_LOG_ERROR,
                            "[wrapper_environment_open] dlopen %s failed\n",
                            core);
    return false;
  }
  return true;
}

void wrapper_dlclose() {
  if (handlers.handle) {
    wrapper_environment_log(RETRO_LOG_INFO,
                            "[wrapper_dlclose] close libretro\n");
    dlclose(handlers.handle);
    INIT_STRUCT(handlers);
  }
}

int wrapper_environment_open(wrapper_log_printf_t _log,
                             enum retro_log_level _minLogLevel,
                             char *_save_directory, char *_system_directory,
                             char *_sample_rate,
                             retro_input_state_t _input_state_cb, char *_core) {
  log = _log;
  minLogLevel = _minLogLevel;
  pixel_format = RETRO_PIXEL_FORMAT_UNKNOWN;
  input_state_cb = _input_state_cb;
  INIT_AND_COPY_STRING(core, _core);

  // load core.
  if (!wrapper_dlopen())
    return -1;

  wrapper_environment_log(
      RETRO_LOG_INFO,
      "[wrapper_environment_open] dlsym retro_set_environment\n");
  handlers.retro_set_environment = (void (*)(retro_environment_t))dlsym(
      handlers.handle, "retro_set_environment");
  if (handlers.retro_set_environment == NULL) {
    wrapper_environment_log(
        RETRO_LOG_ERROR,
        "[wrapper_environment_open] retro_set_environment not found\n");
    wrapper_dlclose();
    return -1;
  }

  wrapper_environment_log(
      RETRO_LOG_INFO,
      "[wrapper_environment_open] dlsym retro_get_system_av_info\n");
  handlers.retro_get_system_av_info =
      (void (*)(struct retro_system_av_info *))dlsym(
          handlers.handle, "retro_get_system_av_info");
  if (handlers.retro_get_system_av_info == NULL) {
    wrapper_environment_log(
        RETRO_LOG_ERROR,
        "[wrapper_environment_open] retro_get_system_av_info not found\n");
    wrapper_dlclose();
    return -1;
  }

  wrapper_environment_log(
      RETRO_LOG_INFO,
      "[wrapper_environment_open] dlsym retro_get_system_info\n");
  handlers.retro_get_system_info = (void (*)(struct retro_system_info *))dlsym(
      handlers.handle, "retro_get_system_info");
  if (handlers.retro_get_system_info == NULL) {
    wrapper_environment_log(RETRO_LOG_ERROR,
                            "[wrapper_enwrapper_environment_openvironment_init]"
                            " retro_get_system_info not found\n");
    wrapper_dlclose();
    return -1;
  }

  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_open] dlsym retro_init\n");
  handlers.retro_init = (void (*)(void))dlsym(handlers.handle, "retro_init");
  if (handlers.retro_init == NULL) {
    wrapper_environment_log(
        RETRO_LOG_ERROR, "[wrapper_environment_open] retro_init not found\n");
    wrapper_dlclose();
    return -1;
  }

  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_open] dlsym retro_run\n");
  handlers.retro_run = (void (*)(void))dlsym(handlers.handle, "retro_run");
  if (handlers.retro_run == NULL) {
    wrapper_environment_log(RETRO_LOG_ERROR,
                            "[wrapper_environment_open] retro_run not found\n");
    wrapper_dlclose();
    return -1;
  }

  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_open] dlsym retro_deinit\n");
  handlers.retro_deinit =
      (void (*)(void))dlsym(handlers.handle, "retro_deinit");
  if (handlers.retro_deinit == NULL) {
    wrapper_environment_log(
        RETRO_LOG_ERROR, "[wrapper_environment_open] retro_deinit not found\n");
    wrapper_dlclose();
    return -1;
  }

  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_open] dlsym retro_load_game\n");
  handlers.retro_load_game = (bool (*)(struct retro_game_info *))dlsym(
      handlers.handle, "retro_load_game");
  if (handlers.retro_load_game == NULL) {
    wrapper_environment_log(
        RETRO_LOG_ERROR,
        "[wrapper_environment_open] retro_load_game not found\n");
    wrapper_dlclose();
    return -1;
  }

  wrapper_environment_log(
      RETRO_LOG_INFO, "[wrapper_environment_open] dlsym retro_unload_game\n");
  handlers.retro_unload_game =
      (void (*)())dlsym(handlers.handle, "retro_unload_game");
  if (handlers.retro_unload_game == NULL) {
    wrapper_environment_log(
        RETRO_LOG_ERROR,
        "[wrapper_environment_open] retro_unload_game not found\n");
    wrapper_dlclose();
    return -1;
  }

  wrapper_environment_log(
      RETRO_LOG_INFO,
      "[wrapper_environment_open] dlsym retro_set_controller_port_device\n");
  handlers.retro_set_controller_port_device =
      (void (*)(unsigned port, unsigned device))dlsym(
          handlers.handle, "retro_set_controller_port_device");
  if (handlers.retro_set_controller_port_device == NULL) {
    wrapper_environment_log(RETRO_LOG_ERROR,
                            "[wrapper_environment_open] "
                            "retro_set_controller_port_device not found\n");
    wrapper_dlclose();
    return -1;
  }
  handlers.retro_set_video_refresh = (void (*)(retro_video_refresh_t))dlsym(
      handlers.handle, "retro_set_video_refresh");
  if (handlers.retro_set_video_refresh == NULL) {
    wrapper_environment_log(
        RETRO_LOG_ERROR,
        "[wrapper_environment_open] retro_set_video_refresh not found\n");
    wrapper_dlclose();
    return -1;
  }
  handlers.retro_set_audio_sample = (void (*)(retro_audio_sample_t))dlsym(
      handlers.handle, "retro_set_audio_sample");
  if (handlers.retro_set_audio_sample == NULL) {
    wrapper_environment_log(
        RETRO_LOG_ERROR,
        "[wrapper_environment_open] retro_set_audio_sample not found\n");
    wrapper_dlclose();
    return -1;
  }
  handlers.retro_set_audio_sample_batch =
      (void (*)(retro_audio_sample_batch_t))dlsym(
          handlers.handle, "retro_set_audio_sample_batch");
  if (handlers.retro_set_audio_sample_batch == NULL) {
    wrapper_environment_log(
        RETRO_LOG_ERROR,
        "[wrapper_environment_open] retro_set_audio_sample_batch not found\n");
    wrapper_dlclose();
    return -1;
  }
  handlers.retro_set_input_poll = (void (*)(retro_input_poll_t))dlsym(
      handlers.handle, "retro_set_input_poll");
  if (handlers.retro_set_input_poll == NULL) {
    wrapper_environment_log(
        RETRO_LOG_ERROR,
        "[wrapper_environment_open] retro_set_input_poll not found\n");
    wrapper_dlclose();
    return -1;
  }
  handlers.retro_set_input_state = (void (*)(retro_input_state_t))dlsym(
      handlers.handle, "retro_set_input_state");
  if (handlers.retro_set_input_state == NULL) {
    wrapper_environment_log(
        RETRO_LOG_ERROR,
        "[wrapper_environment_open] retro_set_input_state not found\n");
    wrapper_dlclose();
    return -1;
  }

  INIT_STRUCT(system_info);
  INIT_STRUCT(av_info);

  INIT_AND_COPY_STRING(save_directory, _save_directory);
  INIT_AND_COPY_STRING(system_directory, _system_directory);
  INIT_AND_COPY_STRING(sample_rate, _sample_rate);

  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_open] sample_rate: %s\n "
                          "save_directory: %s\n  system_directory: %s\n",
                          sample_rate, save_directory, system_directory);

  return 0;
}

void wrapper_input_poll_cb() { return; }

int16_t wrapper_input_state_cb(unsigned port, unsigned device, unsigned index,
                               unsigned id) {
  // wrapper_environment_log(RETRO_LOG_INFO, "[input_state_cb]\n");
  return input_state_cb(port, device, index, id);
}

void wrapper_environment_init() {
  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_init] call retro init\n");

  handlers.retro_set_environment(&wrapper_environment_cb);
  handlers.retro_set_input_poll(&wrapper_input_poll_cb);
  handlers.retro_set_input_state(&wrapper_input_state_cb);

  // do almost nothing
  // https://github.com/libretro/mame2003-plus-libretro/blob/f34453af7f71c31a48d26db9d78aa04a5575ef9a/src/mame2003/mame2003.c#L182
  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_init] retro_init\n");
  handlers.retro_init();

  wrapper_environment_get_system_info();

  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_init] end ----------\n");
}

void wrapper_environment_get_system_info() {
  INIT_STRUCT(system_info);
  handlers.retro_get_system_info(&system_info);
  wrapper_environment_log(
      RETRO_LOG_INFO,
      "[wrapper_environment_get_system_info] Library Name: %s\n"
      "Library Version: %s\n"
      "valid_extensions: %s\n"
      "need_fullpath: %i\n",
      system_info.library_name, system_info.library_version,
      system_info.valid_extensions, (int)system_info.need_fullpath);
  return;
}

void wrapper_environment_set_game_parameters(char *_gamma, char *_brightness,
                                             int _xy_control_type) {

  INIT_AND_COPY_STRING(gamma, _gamma);
  INIT_AND_COPY_STRING(brightness, _brightness);
  INIT_AND_COPY_STRING(enabled, "enabled");
  INIT_AND_COPY_STRING(disabled, "disabled");
  INIT_AND_COPY_STRING(vector_intensity, "2.0");

  if (_xy_control_type == 0) {
    INIT_AND_COPY_STRING(xy_input_type, "mouse");
  } else {
    INIT_AND_COPY_STRING(xy_input_type, "lightgun");
  }

  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_environment_set_game_parameters] gamma: %s "
                          "brightness: %s XY control type: %s \n",
                          gamma, brightness, xy_input_type);
}

void wrapper_environment_log_geometry() {
  char *fmt = "[wrapper_environment_log_geometry] Geo:\n"
              "    base_width: %i\n"
              "    base_height: %i\n"
              "    max_width: %i\n"
              "    max_height: %i\n"
              "    aspect_ratio: %f\n"
              "    fps: %f\n";
  wrapper_environment_log(RETRO_LOG_INFO, fmt, av_info.geometry.base_width,
                          av_info.geometry.base_height,
                          av_info.geometry.max_width,
                          av_info.geometry.max_height,
                          av_info.geometry.aspect_ratio, av_info.timing.fps);
}

void wrapper_environment_log_render_callback() {
    char* fmt = "[wrapper_environment_log_render_callback] Render callback:\n"
        "    version_major: %i\n"
        "    version_minor: %i\n";
    wrapper_environment_log(RETRO_LOG_INFO, fmt, 
        hw_render_callback.version_major,
        hw_render_callback.version_minor
        );
}

void wrapper_environment_log_vulkan_interface() {
    char* fmt = "[wrapper_environment_log_vulkan_interface] Vulkan interface:\n"
        "    interface_type: %i\n"
        "    interface_version: %i\n"
        "    get_application_info: %016x\n"
        "    create_device:        %016x\n"
        "    destroy_device:       %016x\n"
        ;
    wrapper_environment_log(RETRO_LOG_INFO, fmt,
        hw_vulkan_interface.interface_type,
        hw_vulkan_interface.interface_version,
        (long)hw_vulkan_interface.get_application_info,
        (long)hw_vulkan_interface.create_device,
        (long)hw_vulkan_interface.destroy_device
        );
}

void wrapper_environment_get_av_info() {
  // wrapper_environment_log(RETRO_LOG_INFO,
  //                         "[wrapper_environment_get_av_info]\n");
  INIT_STRUCT(av_info);

  if (!handlers.handle)
    return;

  handlers.retro_get_system_av_info(&av_info);
  wrapper_environment_log_geometry();

  if (av_info.geometry.base_width < 0 || av_info.geometry.base_width > 1024 ||
      av_info.geometry.base_height < 0 || av_info.geometry.base_height > 1024) {
    wrapper_environment_log(RETRO_LOG_ERROR,
                            "[wrapper_environment_get_av_info] inconsistence "
                            "detected / memory corruption\n");
  }
}

double wrapper_environment_get_fps() {
  // wrapper_environment_log(RETRO_LOG_INFO, "[wrapper_environment_get_fps]\n");
  if (av_info.timing.fps == 0) {
    wrapper_environment_get_av_info();
  }
  return av_info.timing.fps;
}
double wrapper_environment_get_sample_rate() {
  // wrapper_environment_log(RETRO_LOG_INFO,
  // "[wrapper_environment_get_sample_rate]\n");
  if (av_info.timing.sample_rate == 0) {
    wrapper_environment_get_av_info();
  }
  return av_info.timing.sample_rate;
}

// https://github.com/libretro/RetroArch/blob/437ed733f5822934e6a422e09cbc9efdacfe7f60/runloop.c#L1408
bool wrapper_environment_cb(unsigned cmd, void *data) {
  struct retro_variable *var;
  unsigned int *ptr;

  /*
  #ifdef ENVIRONMENT_DEBUG
    wrapper_environment_log(RETRO_LOG_INFO, "[wrapper_environment_cb] cmd:
  %i\n", (int)cmd); #endif
  */

  if (cmd == RETRO_ENVIRONMENT_GET_VARIABLE_UPDATE) {
    // *(bool*)data = false;
    return false;
  }

  switch (cmd) {

  case RETRO_ENVIRONMENT_GET_VARIABLE:
      // 15

      if (!data)
          return false;

      var = (struct retro_variable*)data;
      if (!var->key)
          return false;

#ifdef ENVIRONMENT_DEBUG
      wrapper_environment_log(RETRO_LOG_INFO,
          "[wrapper_environment_cb] get var: %s", var->key);
#endif /* ifdef ENVIRONMENT_DEBUG */

      if (strcmp(var->key, "mame2003-plus_skip_disclaimer") == 0) {
          var->value = enabled;
          wrapper_environment_log(RETRO_LOG_INFO,
              "[wrapper_environment_cb] get var: %s: %s",
              var->key, var->value);
          return true;
      }
      else if (strcmp(var->key, "mame2003-plus_skip_warnings") == 0) {
          var->value = enabled;
          wrapper_environment_log(RETRO_LOG_INFO,
              "[wrapper_environment_cb] get var: %s: %s",
              var->key, var->value);
          return true;
      }
      else if (strcmp(var->key, "mame2003-plus_xy_device") == 0) {
          var->value = xy_input_type;
          wrapper_environment_log(RETRO_LOG_INFO,
              "[wrapper_environment_cb] get var: %s: %s",
              var->key, var->value);
          return true;
      }
      else if (strcmp(var->key, "mame2003-plus_gamma") == 0 ||
          strcmp(var->key, "mame_current_adj_gamma") == 0) {
          var->value = gamma;
          wrapper_environment_log(RETRO_LOG_INFO,
              "[wrapper_environment_cb] get var: %s: %s",
              var->key, var->value);
          return true;
      }
      else if (strcmp(var->key, "mame2003-plus_brightness") == 0 ||
          strcmp(var->key, "mame_current_adj_brightness") == 0) {
          var->value = brightness;
          wrapper_environment_log(RETRO_LOG_INFO,
              "[wrapper_environment_cb] get var: %s: %s",
              var->key, var->value);
          return true;
      }
      else if (strcmp(var->key, "mame2003-plus_sample_rate") == 0) {
          var->value = sample_rate;
          wrapper_environment_log(RETRO_LOG_INFO,
              "[wrapper_environment_cb] get var: %s: %s",
              var->key, var->value);
          return true;
      }
      else if (strcmp(var->key, "mame2003-plus_mame_remapping") == 0) {
          var->value = enabled;
          wrapper_environment_log(RETRO_LOG_INFO,
              "[wrapper_environment_cb] get var: %s: %s",
              var->key, var->value);
          return true;
      }
      else if (strcmp(var->key, "mame2003-plus_vector_intensity") == 0) {
          var->value = vector_intensity;
          wrapper_environment_log(RETRO_LOG_INFO,
              "[wrapper_environment_cb] get var: %s: %s",
              var->key, var->value);
          return true;
      }
      else if (strcmp(var->key, "mame2003-plus_use_samples") == 0) {
          var->value = enabled;
          wrapper_environment_log(RETRO_LOG_INFO,
              "[wrapper_environment_cb] get var: %s: %s",
              var->key, var->value);
          return true;
      }
      else if (strcmp(var->key, "mame2003-plus_vector_vector_translusency") ==
          0) {
          var->value = disabled;
          wrapper_environment_log(RETRO_LOG_INFO,
              "[wrapper_environment_cb] get var: %s: %s",
              var->key, var->value);
          return true;
      }
      else if (strcmp(var->key, "fbneo-lightgun-crosshair-emulation") == 0) {
          var->value = enabled;
          wrapper_environment_log(RETRO_LOG_INFO,
              "[wrapper_environment_cb] get var: %s: %s",
              var->key, var->value);
          return true;
      }
      else if (strcmp(var->key, "cap32_gfx_colors") == 0) {
          var->value = cap32_gfx_colors;
          wrapper_environment_log(RETRO_LOG_INFO,
              "[wrapper_environment_cb] get var: %s: %s",
              var->key, var->value);
          return true;
      }
      else if (strcmp(var->key, "cap32_scr_crop") == 0) {
          var->value = cap32_scr_crop;
          wrapper_environment_log(RETRO_LOG_INFO,
              "[wrapper_environment_cb] get var: %s: %s",
              var->key, var->value);
          return true;
      }
	  else if (strcmp(var->key, "cap32_model") == 0) {
		  var->value = cap32_model;
		  wrapper_environment_log(RETRO_LOG_INFO,
			  "[wrapper_environment_cb] get var: %s: %s",
			  var->key, var->value);
		  return true;
	  }
      else if (strcmp(var->key, "cap32_autorun") == 0) {
          var->value = enabled;
          wrapper_environment_log(RETRO_LOG_INFO,
              "[wrapper_environment_cb] get var: %s: %s",
              var->key, var->value);
          return true;
      }

      wrapper_environment_log(RETRO_LOG_INFO, "[RETRO_ENVIRONMENT_GET_VARIABLE] Unhandled setting: %s\n", var->key);


      return false;

  case RETRO_ENVIRONMENT_GET_CORE_OPTIONS_VERSION:
      wrapper_environment_log(RETRO_LOG_INFO, "[GET_CORE_OPTIONS_VERSION] v2\n");
      *(unsigned*)data = 0;
      break;

  case RETRO_ENVIRONMENT_GET_LOG_INTERFACE:
      if (!data || !log)
          return false;
      wrapper_environment_log(RETRO_LOG_INFO,
          "[RETRO_ENVIRONMENT_GET_LOG_INTERFACE]\n");
      struct retro_log_callback* cb = (struct retro_log_callback*)data;
      cb->log = &wrapper_environment_log;
      return true;

  case RETRO_ENVIRONMENT_SET_PIXEL_FORMAT:
      pixel_format = *(const enum retro_pixel_format*)data;
      wrapper_environment_log(RETRO_LOG_INFO,
          "[RETRO_ENVIRONMENT_SET_PIXEL_FORMAT] %i\n",
          pixel_format);
      return true;

  case RETRO_ENVIRONMENT_GET_SYSTEM_DIRECTORY:
      // 9
      if (!data)
          return false;
      wrapper_environment_log(RETRO_LOG_INFO,
          "[RETRO_ENVIRONMENT_GET_SYSTEM_DIRECTORY]: %s\n",
          system_directory);
      *(char**)data = system_directory;
      return true;

  case RETRO_ENVIRONMENT_GET_SAVE_DIRECTORY:
      // 31
      if (!data)
          return false;
      wrapper_environment_log(RETRO_LOG_INFO,
          "[RETRO_ENVIRONMENT_GET_SAVE_DIRECTORY]%s\n",
          save_directory);
      *(char**)data = save_directory;
      return true;

  case RETRO_ENVIRONMENT_SET_ROTATION:
      if (!data)
          return false;
      // rotation should be specified in CDL
      ptr = (unsigned int*)data;
      wrapper_environment_log(RETRO_LOG_INFO,
          "[RETRO_ENVIRONMENT_SET_ROTATION] %i\n", *ptr);
      return true;

  case RETRO_ENVIRONMENT_SET_GEOMETRY:
      if (!data)
          return false;

      wrapper_environment_log(RETRO_LOG_INFO,
          "[RETRO_ENVIRONMENT_SET_GEOMETRY]\n");
      struct retro_game_geometry* geo = (struct retro_game_geometry*)data;
      memcpy(&(av_info.geometry), geo, sizeof(av_info.geometry));
      wrapper_environment_log_geometry();
      return true;

  case RETRO_ENVIRONMENT_SET_CONTROLLER_INFO:
      wrapper_environment_log(RETRO_LOG_INFO,
          "[RETRO_ENVIRONMENT_SET_CONTROLLER_INFO] last "
          "environment call from retro_load_game\n");
      return false;

  case RETRO_ENVIRONMENT_GET_CAN_DUPE:
      if (!data)
          return false;
      wrapper_environment_log(RETRO_LOG_INFO,
          "[RETRO_ENVIRONMENT_GET_CAN_DUPE]\n");
      *(bool*)data = true;
      return true;

  case RETRO_ENVIRONMENT_GET_VARIABLE_UPDATE:
      if (!data)
          return false;
      wrapper_environment_log(RETRO_LOG_INFO,
          "[RETRO_ENVIRONMENT_GET_VARIABLE_UPDATE]\n");
      *(bool*)data = false;
      return false;

  case RETRO_ENVIRONMENT_SHUTDOWN:
      wrapper_environment_log(RETRO_LOG_INFO,
          "[RETRO_ENVIRONMENT_SHUTDOWN] ???\n");
      return false;

  case RETRO_ENVIRONMENT_GET_PREFERRED_HW_RENDER:
      wrapper_environment_log(RETRO_LOG_INFO,
          "[RETRO_ENVIRONMENT_GET_PREFERRED_HW_RENDER] VULKAN\n");
      unsigned* hw_context = (unsigned*)data;
      *hw_context = RETRO_HW_CONTEXT_VULKAN;
      return true;

  case RETRO_ENVIRONMENT_SET_HW_RENDER:
      wrapper_environment_log(RETRO_LOG_INFO,
          "[RETRO_ENVIRONMENT_SET_HW_RENDER]\n");
      if (!data)
          return false;
      struct retro_hw_render_callback* callback = (struct retro_hw_render_callback*)data;
      memcpy(&(hw_render_callback), callback, sizeof(hw_render_callback));
      wrapper_environment_log_render_callback();
      return true;

  case RETRO_ENVIRONMENT_SET_HW_RENDER_CONTEXT_NEGOTIATION_INTERFACE:
      wrapper_environment_log(RETRO_LOG_INFO,
          "[RETRO_ENVIRONMENT_SET_HW_RENDER_CONTEXT_NEGOTIATION_INTERFACE]\n");
      if (!data)
          return false;
      struct retro_hw_render_context_negotiation_interface_vulkan *interface = (struct retro_hw_render_context_negotiation_interface_vulkan*)data;
      memcpy(&(hw_vulkan_interface), interface, sizeof(hw_vulkan_interface));
      wrapper_environment_log_vulkan_interface();


      // 1- Have to negotiate thru retro_hw_render_context_negotiation_interface_vulkan
        wrapper_environment_log(RETRO_LOG_INFO, "[RETRO_ENVIRONMENT_GET_HW_RENDER_INTERFACE] create_device\n");

      void* vulkan_library = dlopen("libvulkan.so", RTLD_NOW);
      if (!vulkan_library) {
          wrapper_environment_log(RETRO_LOG_INFO, "[RETRO_ENVIRONMENT_GET_HW_RENDER_INTERFACE] Failed to load libvulkan.so\n");
          return false;
      }

      vk_get_instance_proc_addr = (PFN_vkGetInstanceProcAddr)dlsym(vulkan_library, "vkGetInstanceProcAddr");
      if (!vk_get_instance_proc_addr) {
          wrapper_environment_log(RETRO_LOG_INFO, "[RETRO_ENVIRONMENT_GET_HW_RENDER_INTERFACE] Failed to find vkGetInstanceProcAddr\n");
          return false;
      }

      vk_create_instance = (PFN_vkCreateInstance)vk_get_instance_proc_addr(NULL, "vkCreateInstance");
      if (!vk_create_instance) {
          wrapper_environment_log(RETRO_LOG_INFO, "[RETRO_ENVIRONMENT_GET_HW_RENDER_INTERFACE] Failed to find vkCreateInstance\n");
          return false;
      }

      vk_get_device_proc_addr = (PFN_vkGetDeviceProcAddr)vk_get_instance_proc_addr(NULL, "vkGetDeviceProcAddr");
      if (!vk_get_device_proc_addr) {
          wrapper_environment_log(RETRO_LOG_INFO, "[RETRO_ENVIRONMENT_GET_HW_RENDER_INTERFACE] Failed to find vkGetDeviceProcAddr\n");
          //return false;
      }

      // Initialize the VkApplicationInfo structure
      VkApplicationInfo appInfo = {};
      appInfo.sType = VK_STRUCTURE_TYPE_APPLICATION_INFO;
      appInfo.pApplicationName = "Hello Vulkan";
      appInfo.applicationVersion = VK_MAKE_VERSION(1, 0, 0);
      appInfo.pEngineName = "No Engine";
      appInfo.engineVersion = VK_MAKE_VERSION(1, 0, 0);
      appInfo.apiVersion = VK_API_VERSION_1_0;

      // Initialize the VkInstanceCreateInfo structure
      VkInstanceCreateInfo createInfo = {};
      createInfo.sType = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO;
      createInfo.pApplicationInfo = &appInfo;

      // Create the Vulkan instance
      VkResult vkCreateInstanceResult = vk_create_instance(&createInfo, NULL, &vkInstance);
      if (vkCreateInstanceResult != VK_SUCCESS) {
          wrapper_environment_log(RETRO_LOG_INFO, "[RETRO_ENVIRONMENT_GET_HW_RENDER_INTERFACE] Failed to create Vulkan instance\n");
          return false;
      }

      char* required_device_extensions = VK_KHR_SWAPCHAIN_EXTENSION_NAME;
      VkPhysicalDeviceFeatures required_features = { false };

      bool result = interface->create_device(&context, 
          vkInstance,                   // VkInstance instance
          VK_NULL_HANDLE,               // VkPhysicalDevice gpu
          VK_NULL_HANDLE,               // VkSurfaceKHR surface
          vk_get_instance_proc_addr,    // PFN_vkGetInstanceProcAddr get_instance_proc_addr
          &required_device_extensions,  // const char **required_device_extensions
          0,                            // unsigned num_required_device_extensions
          NULL,                         // const char **required_device_layers
          0,                            // unsigned num_required_device_layers
          &required_features            // const VkPhysicalDeviceFeatures *required_features
          );
      wrapper_environment_log(RETRO_LOG_INFO, "[RETRO_ENVIRONMENT_GET_HW_RENDER_INTERFACE] create_device %d\n", result);

      wrapper_environment_log(RETRO_LOG_INFO, 
          "[RETRO_ENVIRONMENT_GET_HW_RENDER_INTERFACE] create_device result\n"
          "	VkInstance instance: %016x\n"
          "	VkPhysicalDevice gpu: %016x\n"
          "	VkDevice device: %016x\n"
          "	VkQueue queue: %016x\n"
          "	unsigned queue_family_index: %i\n"
          "	VkQueue presentation_queue: %016x\n"
          "	unsigned presentation_queue_family_index: %i\n",
          &context.gpu,
          &context.device,
          &context.queue,
          context.queue_family_index,
          &context.presentation_queue,
          context.presentation_queue_family_index
		  );

      // 2- Now we have to call context_reset to finalize the initialization
      hw_render_callback.get_proc_address = vk_get_instance_proc_addr;
      hw_render_callback.context_reset();
      wrapper_environment_log(RETRO_LOG_INFO, "[RETRO_ENVIRONMENT_SET_HW_RENDER] Vulkan context_reset done\n");

      return true;

  case RETRO_ENVIRONMENT_GET_HW_RENDER_INTERFACE:
      wrapper_environment_log(RETRO_LOG_INFO,
          "[RETRO_ENVIRONMENT_GET_HW_RENDER_INTERFACE]\n");
      if (!data)
          return false;
      hw_render_interface.interface_type = RETRO_HW_RENDER_INTERFACE_VULKAN;
      hw_render_interface.interface_version = RETRO_HW_RENDER_INTERFACE_VULKAN_VERSION;
      hw_render_interface.instance = vkInstance;
      hw_render_interface.gpu = context.gpu;
      hw_render_interface.device = context.device;
      hw_render_interface.queue = context.queue;
      hw_render_interface.queue_index = context.queue_family_index;
      hw_render_interface.get_instance_proc_addr = vk_get_instance_proc_addr;
      hw_render_interface.get_device_proc_addr = vk_get_device_proc_addr;

      hw_render_interface.set_image = vulkan_set_image;
      hw_render_interface.get_sync_index = vulkan_get_sync_index;
      hw_render_interface.get_sync_index_mask = vulkan_get_sync_index_mask;
      hw_render_interface.wait_sync_index = vulkan_wait_sync_index;
      hw_render_interface.set_command_buffers = vulkan_set_command_buffers;
      hw_render_interface.lock_queue = vulkan_lock_queue;
      hw_render_interface.unlock_queue = vulkan_unlock_queue;
      hw_render_interface.set_signal_semaphore = vulkan_set_signal_semaphore;

      *(void**)data = &hw_render_interface;
      return true;

  case RETRO_ENVIRONMENT_SET_CORE_OPTIONS_DISPLAY:
      if (!data)
          return false;
      struct retro_core_option_display* display = (struct retro_core_option_display*)data;
      //wrapper_environment_log(RETRO_LOG_INFO, "[RETRO_ENVIRONMENT_SET_CORE_OPTIONS_DISPLAY] %s=%d",
      //    display->key, display->visible
      //);
      return true;

  case RETRO_ENVIRONMENT_SET_KEYBOARD_CALLBACK:
      wrapper_environment_log(RETRO_LOG_INFO,
		  "[RETRO_ENVIRONMENT_SET_KEYBOARD_CALLBACK]\n");
      // we receive struct retro_keyboard_callback *
      return true;

  case RETRO_ENVIRONMENT_GET_DISK_CONTROL_INTERFACE_VERSION:
      wrapper_environment_log(RETRO_LOG_INFO,
		  "[RETRO_ENVIRONMENT_GET_DISK_CONTROL_INTERFACE_VERSION]\n");
      *(unsigned*)data = 1;
      return true;

  case RETRO_ENVIRONMENT_SET_DISK_CONTROL_INTERFACE:
      wrapper_environment_log(RETRO_LOG_INFO,
          "[RETRO_ENVIRONMENT_SET_DISK_CONTROL_INTERFACE]\n");
      // we receive struct retro_disk_control_callback *
      return true;

  case RETRO_ENVIRONMENT_SET_DISK_CONTROL_EXT_INTERFACE:
      wrapper_environment_log(RETRO_LOG_INFO,
		  "[RETRO_ENVIRONMENT_SET_DISK_CONTROL_EXT_INTERFACE]\n");
	  // we receive struct retro_disk_control_ext_callback *
	  return true;

  default:
	  wrapper_environment_log(RETRO_LOG_WARN,
		  "[wrapper_environment_cb] Unsupported command: %i\n", (int)cmd);
	  return false;
  }

  return false;
}

int wrapper_system_info_need_full_path() {
  return (int)system_info.need_fullpath;
}

void wrapper_retro_deinit() {
  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_retro_deinit] retro_deinit\n");
  if (!handlers.handle)
    return;

  handlers.retro_deinit(); // do nothing (mame 2003.c line 401)
  wrapper_dlclose();
}

int wrapper_load_game(char *path, char *_gamma, char *_brightness,
                      int _xy_control_type) {

  wrapper_environment_log(RETRO_LOG_INFO, "[wrapper_load_game]\n");
  if (!handlers.handle)
    return false;

  INIT_STRUCT(game_info);
  INIT_AND_COPY_STRING(game_path, path);
  game_info.path = game_path;

  wrapper_image_prev_load_game();
  wrapper_environment_set_game_parameters(
      _gamma, _brightness, _xy_control_type); // 0 mouse, 1 ligthgun

  wrapper_environment_log(RETRO_LOG_INFO, "[wrapper_load_game] (%s)--\n",
                          game_info.path);
  bool ret = handlers.retro_load_game(&game_info);

  if (_xy_control_type != 0) {
    //there isn't a way to say "mouse" yet in others than mame2003. So it's lightgun o joypad. 
    handlers.retro_set_controller_port_device(0, RETRO_DEVICE_LIGHTGUN);
  }

  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_load_game] END (ret:%i) --------- \n", ret);
  return (int)ret;
}

void wrapper_unload_game() {
  // https://github.com/libretro/mame2000-libretro/blob/6d0b1e1fe287d6d8536b53a4840e7d152f86b34b/src/libretro/libretro.c#L1054
  wrapper_environment_log(RETRO_LOG_INFO,
                          "[wrapper_unload_game] retro_unload_game\n");
  if (!handlers.handle)
    return;
  handlers.retro_unload_game();
  wrapper_audio_free();
}

void wrapper_run() {
  // wrapper_environment_log(RETRO_LOG_INFO, "[wrapper_run] retro_run start\n");
  if (handlers.handle)
    handlers.retro_run();

  // wrapper_environment_log(RETRO_LOG_INFO, "[wrapper_run] retro_run end\n");
}