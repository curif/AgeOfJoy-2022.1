#include "environment.h"
#include "vulkan.h"

#define ENVIRONMENT_DEBUG

static enum retro_pixel_format pixel_format;
static char system_directory[500];
static char save_directory[500];
static char game_path[500];
static char gamma[50];
static char brightness[50];
static char sample_rate[50];
static char xy_input_type[15];
static char core[500];
static size_t savestate_size;
static char persistent_savestate_file[1000];
static bool persistent_enabled;

static handlers_t handlers;
static wrapper_log_printf_t log;
static retro_input_state_t input_state_cb;
static struct retro_system_info system_info;
static struct retro_system_av_info av_info;
static struct retro_game_info game_info;
static struct retro_frame_time_callback frame_time_callback;
static long frame_counter;

#define LOG_BUFFER_SIZE 4096
static char log_buffer[LOG_BUFFER_SIZE];
static enum retro_log_level minLogLevel;

static Environment EnvironmentCB;

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

handlers_t* wrapper_environment_get_handlers() { return &handlers; }

void wrapper_environment_log(enum retro_log_level level, const char* format,
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
		const char* error = dlerror();
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

void wrapper_load_savestate(char* savestateFile) {
	wrapper_environment_log(RETRO_LOG_INFO,
		"[wrapper_load_savestate] savestate_file: %s\n", savestateFile);

	if (savestate_size <= 0) {
		wrapper_environment_log(RETRO_LOG_INFO,
			"[wrapper_load_savestate] No savestate to process, skipping\n");
		return;
	}

	FILE* file = fopen(savestateFile, "rb");
	if (!file) {
		wrapper_environment_log(RETRO_LOG_INFO,
			"[wrapper_load_savestate] No existing savestate\n");
	}
	else {
		wrapper_environment_log(RETRO_LOG_INFO,
			"[wrapper_load_savestate] Loading existing savestate\n");
		void* data = malloc(savestate_size);
		if (data) {
			fread(data, 1, savestate_size, file);
			bool success = handlers.retro_unserialize(data, savestate_size);
			wrapper_environment_log(RETRO_LOG_INFO,
				"[wrapper_load_savestate] retro_unserialize %d\n", success);
			free(data);
		}
		fclose(file);
	}
}

void wrapper_save_savestate(char* savestateFile) {
	wrapper_environment_log(RETRO_LOG_INFO,
		"[wrapper_save_savestate] savestate_file: %s\n", savestateFile);

	if (savestate_size <= 0) {
		wrapper_environment_log(RETRO_LOG_INFO,
			"[wrapper_save_savestate] No savestate to process, skipping\n");
		return;
	}

	void* data = malloc(savestate_size);
	if (data) {
		bool success = handlers.retro_serialize(data, savestate_size);
		wrapper_environment_log(RETRO_LOG_INFO,
			"[wrapper_save_savestate] retro_serialize %d\n", success);
		FILE* file = fopen(savestateFile, "wb");
		if (file) {
			fwrite(data, 1, savestate_size, file);
			fclose(file);
		}
		free(data);
	}
}

int wrapper_environment_open(wrapper_log_printf_t _log,
	enum retro_log_level _minLogLevel,
	char* _save_directory,
	char* _system_directory,
	char* _sample_rate,
	char* _persistent_savestate_file,
	retro_input_state_t _input_state_cb,
	char* _core,
	Environment _environment) {
	log = _log;
	frame_counter = 0;
	minLogLevel = _minLogLevel;
	pixel_format = RETRO_PIXEL_FORMAT_UNKNOWN;
	input_state_cb = _input_state_cb;
	INIT_AND_COPY_STRING(core, _core);
	EnvironmentCB = _environment;
	savestate_size = 0;

	// load core.
	if (!wrapper_dlopen())
		return -1;

	if (load_symbol((void**)&handlers.retro_set_environment, "retro_set_environment") < 0) return -1;
	if (load_symbol((void**)&handlers.retro_get_system_av_info, "retro_get_system_av_info") < 0) return -1;
	if (load_symbol((void**)&handlers.retro_get_system_info, "retro_get_system_info") < 0) return -1;
	if (load_symbol((void**)&handlers.retro_init, "retro_init") < 0) return -1;
	if (load_symbol((void**)&handlers.retro_run, "retro_run") < 0) return -1;
	if (load_symbol((void**)&handlers.retro_deinit, "retro_deinit") < 0) return -1;
	if (load_symbol((void**)&handlers.retro_load_game, "retro_load_game") < 0) return -1;
	if (load_symbol((void**)&handlers.retro_unload_game, "retro_unload_game") < 0) return -1;
	if (load_symbol((void**)&handlers.retro_set_controller_port_device, "retro_set_controller_port_device") < 0) return -1;
	if (load_symbol((void**)&handlers.retro_set_video_refresh, "retro_set_video_refresh") < 0) return -1;
	if (load_symbol((void**)&handlers.retro_set_audio_sample, "retro_set_audio_sample") < 0) return -1;
	if (load_symbol((void**)&handlers.retro_set_audio_sample_batch, "retro_set_audio_sample_batch") < 0) return -1;
	if (load_symbol((void**)&handlers.retro_set_input_poll, "retro_set_input_poll") < 0) return -1;
	if (load_symbol((void**)&handlers.retro_set_input_state, "retro_set_input_state") < 0) return -1;
	if (load_symbol((void**)&handlers.retro_serialize_size, "retro_serialize_size") < 0) return -1;
	if (load_symbol((void**)&handlers.retro_serialize, "retro_serialize") < 0) return -1;
	if (load_symbol((void**)&handlers.retro_unserialize, "retro_unserialize") < 0) return -1;

	INIT_STRUCT(system_info);
	INIT_STRUCT(av_info);
	INIT_STRUCT(frame_time_callback);

	INIT_AND_COPY_STRING(save_directory, _save_directory);
	INIT_AND_COPY_STRING(system_directory, _system_directory);
	INIT_AND_COPY_STRING(sample_rate, _sample_rate);

	if (_persistent_savestate_file != NULL) {
		persistent_enabled = true;
		INIT_AND_COPY_STRING(persistent_savestate_file, _persistent_savestate_file);
	}
	else {
		persistent_enabled = false;
	}

	wrapper_environment_log(RETRO_LOG_INFO,
		"[wrapper_environment_open] persistent_enabled: %d\n "
		"  persistent_savestate_file: %s\n",
		persistent_enabled, persistent_savestate_file);

	wrapper_environment_log(RETRO_LOG_INFO,
		"[wrapper_environment_open] sample_rate: %s\n "
		"  save_directory: %s\n"
		"  system_directory: %s\n",
		sample_rate, save_directory, system_directory);

	return 0;
}

int load_symbol(void** handler, const char* symbol_name) {
	wrapper_environment_log(RETRO_LOG_INFO, "[wrapper_environment_open] dlsym %s", symbol_name);
	*handler = dlsym(handlers.handle, symbol_name);
	if (*handler == NULL) {
		wrapper_environment_log(RETRO_LOG_ERROR, "[wrapper_environment_open] %s not found\n", symbol_name);
		wrapper_dlclose();
		return -1;
	}
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

void wrapper_environment_set_game_parameters(char* _gamma, char* _brightness,
	int _xy_control_type) {

	INIT_AND_COPY_STRING(gamma, _gamma);
	INIT_AND_COPY_STRING(brightness, _brightness);

	if (_xy_control_type == 0) {
		INIT_AND_COPY_STRING(xy_input_type, "mouse");
	}
	else {
		INIT_AND_COPY_STRING(xy_input_type, "lightgun");
	}

	wrapper_environment_log(RETRO_LOG_INFO,
		"[wrapper_environment_set_game_parameters] gamma: %s "
		"brightness: %s XY control type: %s \n",
		gamma, brightness, xy_input_type);
}

void wrapper_environment_log_geometry() {
	char* fmt = "[wrapper_environment_log_geometry] Geo:\n"
		"    base_width: %i\n"
		"    base_height: %i\n"
		"    max_width: %i\n"
		"    max_height: %i\n"
		"    aspect_ratio: %f\n"
		"    sample_rate: %f\n"
		"    fps: %f\n";
	wrapper_environment_log(RETRO_LOG_INFO, fmt, av_info.geometry.base_width,
		av_info.geometry.base_height,
		av_info.geometry.max_width,
		av_info.geometry.max_height,
		av_info.geometry.aspect_ratio,
		av_info.timing.sample_rate,
		av_info.timing.fps);
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
bool wrapper_environment_cb(unsigned cmd, void* data) {
	struct retro_variable* var;
	unsigned int* ptr;
	char* envValue;

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
		if (!data)
			return false;

		var = (struct retro_variable*)data;
		if (!var->key)
			return false;

		envValue = EnvironmentCB(var->key);
		if (envValue) {
			wrapper_environment_log(RETRO_LOG_INFO,
				"[wrapper_environment_cb] Found in Environment: %s=%s",
				var->key,
				envValue);
			var->value = envValue;
			return true;
		}

#ifdef ENVIRONMENT_DEBUG
		wrapper_environment_log(RETRO_LOG_INFO,
			"[wrapper_environment_cb] get var: %s", var->key);
#endif /* ifdef ENVIRONMENT_DEBUG */

		if (strcmp(var->key, "mame2003-plus_xy_device") == 0) {
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

#ifdef ENVIRONMENT_DEBUG
		wrapper_environment_log(RETRO_LOG_INFO, "[wrapper_environment_cb] Unspecified setting: %s\n", var->key);
#endif

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
			"[RETRO_ENVIRONMENT_GET_PREFERRED_HW_RENDER] RETRO_HW_CONTEXT_VULKAN\n");
		unsigned* hw_context = (unsigned*)data;
		*hw_context = RETRO_HW_CONTEXT_VULKAN;
		return true;

	case RETRO_ENVIRONMENT_SET_HW_RENDER:
		wrapper_environment_log(RETRO_LOG_INFO,
			"[RETRO_ENVIRONMENT_SET_HW_RENDER]\n");
		if (!data)
			return false;
		return init_retro_hw_render_callback((struct retro_hw_render_callback*)data);

	case RETRO_ENVIRONMENT_SET_HW_RENDER_CONTEXT_NEGOTIATION_INTERFACE:
		wrapper_environment_log(RETRO_LOG_INFO,
			"[RETRO_ENVIRONMENT_SET_HW_RENDER_CONTEXT_NEGOTIATION_INTERFACE]\n");
		if (!data)
			return false;
		return init_retro_hw_render_context_negotiation_interface_vulkan((struct retro_hw_render_context_negotiation_interface_vulkan*)data);

	case RETRO_ENVIRONMENT_GET_HW_RENDER_INTERFACE:
		wrapper_environment_log(RETRO_LOG_INFO,
			"[RETRO_ENVIRONMENT_GET_HW_RENDER_INTERFACE]\n");
		if (!data)
			return false;
		*(void**)data = get_vulkan_interface();
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

	case RETRO_ENVIRONMENT_GET_MESSAGE_INTERFACE_VERSION:
		wrapper_environment_log(RETRO_LOG_INFO,
			"[RETRO_ENVIRONMENT_GET_MESSAGE_INTERFACE_VERSION]\n");
		*(unsigned*)data = 1;
		return true;

	case RETRO_ENVIRONMENT_SET_MESSAGE_EXT:
		wrapper_environment_log(RETRO_LOG_INFO,
			"[RETRO_ENVIRONMENT_SET_MESSAGE_EXT]\n");
		if (!data)
			return false;
		struct retro_message_ext* message = (struct retro_message_ext*)data;
		wrapper_environment_log(RETRO_LOG_INFO,
			"[RETRO_ENVIRONMENT_SET_MESSAGE_EXT] Message from core: %s\n", message->msg);
		return true;

	case RETRO_ENVIRONMENT_SET_FRAME_TIME_CALLBACK:
		wrapper_environment_log(RETRO_LOG_INFO,
			"[RETRO_ENVIRONMENT_SET_FRAME_TIME_CALLBACK]\n");
		struct retro_frame_time_callback* retro_frame_time_cb = (struct retro_frame_time_callback*)data;
		memcpy(&(frame_time_callback), retro_frame_time_cb, sizeof(frame_time_callback));
		wrapper_environment_log(RETRO_LOG_INFO,
			"[RETRO_ENVIRONMENT_SET_FRAME_TIME_CALLBACK] Reference:%llu\n", frame_time_callback.reference);
		return true;

	case RETRO_ENVIRONMENT_GET_AUDIO_VIDEO_ENABLE:
		wrapper_environment_log(RETRO_LOG_INFO,
			"[RETRO_ENVIRONMENT_GET_AUDIO_VIDEO_ENABLE]\n");
		if (!data)
			return false;
		//* Bit 0 (value 1) : Enable Video
		//* Bit 1 (value 2) : Enable Audio
		//* Bit 2 (value 4) : Use Fast Savestates.
		//* Bit 3 (value 8) : Hard Disable Audio
		*(int*)data = 3;
		return true;

#ifdef ENVIRONMENT_DEBUG
	default:
		wrapper_environment_log(RETRO_LOG_WARN,
			"[wrapper_environment_cb] Unsupported command: %i\n", (int)cmd);
		return false;
#endif
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

int wrapper_load_game(char* path, char* _gamma, char* _brightness,
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

	if (persistent_enabled) {
		savestate_size = handlers.retro_serialize_size();
		wrapper_environment_log(RETRO_LOG_INFO,
			"[wrapper_load_game] savestate_size %i\n", savestate_size);
		wrapper_load_savestate(persistent_savestate_file);
	}

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
	if (persistent_enabled) {
		wrapper_save_savestate(persistent_savestate_file);
	}
	handlers.retro_unload_game();
	wrapper_audio_free();
}

void wrapper_run() {
	// wrapper_environment_log(RETRO_LOG_INFO, "[wrapper_run] retro_run start\n");
	if (frame_time_callback.callback)
		frame_time_callback.callback(1000000 / av_info.timing.fps);
	if (handlers.handle)
		handlers.retro_run();
	frame_counter++;
	// wrapper_environment_log(RETRO_LOG_INFO, "[wrapper_run] retro_run end\n");
}
