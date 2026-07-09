// Copyright (C) 2026 the Age of Joy project contributors.
//
// This file is part of Age of Joy.
//
// Age of Joy is free software: you can redistribute it and/or modify it under the terms of the
// GNU General Public License as published by the Free Software Foundation, either version 2 of
// the License, or (at your option) any later version.
//
// Age of Joy is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without
// even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If
// not, see <https://www.gnu.org/licenses/>.
//
// See the note in libpdlr.cpp on why this file is GPL-2.0-or-later and not GPL-3.0.

// libpdlr.h — C ABI for the embedded libretro frontend ("pd libretro").
//
// Loads a hardware (Vulkan) libretro core — primarily Flycast/Dreamcast — INSIDE this native .so,
// lets the core negotiate and own its Vulkan device (Option B), and (later milestones) routes the
// core's rendered frame into Unity via the proven AHardwareBuffer transport from libpdvk.
//
// This is the libretro counterpart to libpdvk (the checkerboard transport). See
// claudedocs/geometrizer_vulkan_cores.md. Quest-only; calls no-op cleanly off Android.
//
// Build stages:
//   2a (this file's first cut): pdlr_probe — load + bind + retro_init + system info. No video yet.
//   2b: run + capture the core's set_image VkImage, CPU-readback a real frame.
//   2c: blit that image into an AHB and hand it to Unity (zero-copy), reusing libpdvk's Step B.
#ifndef LIBPDLR_H
#define LIBPDLR_H

#include <stdint.h>

#if defined(__GNUC__)
#define PDLR_API __attribute__((visibility("default")))
#else
#define PDLR_API
#endif

#ifdef __cplusplus
extern "C" {
#endif

// Stage 2a-lite: dlopen the core at `core_path`, bind the retro_* entry points, install the
// environment + I/O callbacks, call retro_set_environment + retro_init, and read retro_system_info.
// Returns 0 on success, non-zero on failure (see logcat tag "pdlr"). No game is loaded and no
// Vulkan context is created yet — this only proves the core loads and initializes in-process.
PDLR_API int pdlr_probe(const char* core_path);

// Core identity, valid after a successful pdlr_probe/pdlr_start (else NULL).
PDLR_API const char* pdlr_core_name(void);
PDLR_API const char* pdlr_core_version(void);

// Stage 2b-1: load + bind the core, point it at the BIOS (system_dir) + save_dir, load the game at
// game_path, negotiate a Vulkan context (the core builds its device on an instance WE create), and
// call context_reset. After this, pdlr_run() drives one frame per call. Returns 0 on success
// (logcat tags "pdlr"/"flycast"). game_path must be a full path (Flycast need_fullpath=1).
PDLR_API int pdlr_start(const char* core_path, const char* system_dir,
                        const char* save_dir, const char* game_path);

// One retro_run tick (call per frame). 0 on success, -1 if not started.
PDLR_API int pdlr_run(void);

// Suspend/resume the emu pump thread. Wire to Unity's OnApplicationPause: while the app is paused
// (headset off / system overlay) Unity stops pulling audio, so running through the pause just drops
// every sample and advances the guest invisibly — RetroArch pauses there too. The pump resyncs its
// deadline schedule on resume (no catch-up burst). Safe any time; no-op before pdlr_start.
PDLR_API void pdlr_set_paused(int paused);

// Display-locked pacing. The emu pump normally paces itself off the VR display's render cadence
// instead of a free-running wall clock — this removes the beat/judder of showing ≈60 Hz content on a
// 72 Hz display (the 6:5 pulldown becomes phase-stable). Unity must call pdlr_notify_display_frame()
// exactly once per rendered frame (cheap: an atomic bump + condvar signal); the pump produces one
// emulated frame per (coreFps/displayHz) ticks. pdlr_set_display_hz() sets the display refresh used
// for that ratio (call once at start; default 72). Knob displaylock.txt=0 reverts to the old pacer.
PDLR_API void pdlr_notify_display_frame(void);
PDLR_API void pdlr_set_display_hz(double hz);

// Latest emulated frame, CPU-read-back (stage 2b-2). After pdlr_run, returns a pointer to tightly
// packed RGBA8 pixels (row-major) + dimensions. 0 on success, non-zero if no frame yet. The pointer
// is stable until pdlr_shutdown; copy it after each pdlr_run.
PDLR_API int pdlr_get_frame(const void** out_pixels, int* out_width, int* out_height);

// Number of frames the core has rendered (set_image calls so far) — proves it's actually drawing.
PDLR_API int pdlr_frame_count(void);

// Input (port 0 only). Push the current gamepad state to be returned by the core's input_state
// callback on the next retro_run. `buttons` is a bitmask where bit N is RETRO_DEVICE_ID_JOYPAD_N
// (e.g. (1<<RETRO_DEVICE_ID_JOYPAD_A)). `lx`/`ly` are the left analog stick, -32768..32767
// (RETRO_DEVICE_INDEX_ANALOG_LEFT). `lt`/`rt` are the Dreamcast analog triggers L2/R2, 0..0x7fff
// (RETRO_DEVICE_INDEX_ANALOG_BUTTON) — used by racing games; pass 0 for both on d-pad titles.
// Call once per frame before pdlr_run.
PDLR_API void pdlr_set_input(uint32_t buttons, int16_t lx, int16_t ly, int16_t lt, int16_t rt);

// Override a libretro core option for the NEXT pdlr_start (from a cabinet's description.yaml
// `environment:` block). Call BEFORE pdlr_start — the core reads options during retro_load_game.
// Overlays the Flycast.opt defaults (this wins); options left unset keep their default value.
// Cleared on pdlr_shutdown. key/value are the raw libretro option id + value (e.g.
// "reicast_broadcast", "PAL").
PDLR_API void pdlr_set_option(const char* key, const char* value);

// Declare what device a core port should be at content load (RETRO_DEVICE_JOYPAD=1,
// RETRO_DEVICE_LIGHTGUN=4, …). Call BEFORE pdlr_start — Flycast builds its maple bus at
// retro_load_game, so a post-load change is ignored for arcade titles. Unset ports default to
// JOYPAD (the 4-pad RetroArch maple parity that gates NAOMI audio init). Reset on pdlr_shutdown.
PDLR_API void pdlr_set_port_device(unsigned port, unsigned device);

// Light-gun state for port 0 (a port previously declared LIGHTGUN via pdlr_set_port_device).
// x/y: libretro virtual screen coords [-0x7fff, 0x7fff], top-left negative. offscreen!=0 → the
// gun isn't pointing at the screen (the core turns that into an offscreen shot / reload).
// `buttons` is a bitmask where bit N is RETRO_DEVICE_ID_LIGHTGUN_N (TRIGGER=2, AUX_A=3, AUX_B=4,
// START=6, SELECT=7 (=NAOMI coin), AUX_C=8, DPAD 9-12, RELOAD=16). Call once per frame.
PDLR_API void pdlr_set_lightgun(int16_t x, int16_t y, int offscreen, uint32_t buttons);

// Stage 2c — zero-copy: the core's frame is blit'd into an AHB-backed image on the core's device,
// then the SAME AHardwareBuffer is imported onto Unity's VkDevice (reusing libpdvk's Step B) and
// sampled with no CPU copy. Default on; if the core's device can't enable the AHB extensions, the
// frontend falls back to the CPU read-back path (pdlr_get_frame) automatically.

// Toggle the zero-copy path. Call BEFORE pdlr_start (it decides which device extensions to request).
// enabled=0 → legacy CPU read-back (pdlr_get_frame); enabled!=0 → AHB zero-copy (default).
PDLR_API void pdlr_set_zero_copy(int enabled);

// Whether zero-copy is actually active. After pdlr_start this reflects auto-fallback: it returns 0
// if the core's device couldn't get the AHB extensions (so the caller should use the CPU path).
PDLR_API int pdlr_zero_copy_active(void);

// Emulated frame dimensions (from the core's video_refresh) — the ACTIVE region. 0 on success, -1 if
// no frame yet. On the zero-copy path this is the sub-rect to UV-crop within the fixed-size buffer.
PDLR_API int pdlr_frame_size(int* out_width, int* out_height);

// Fixed AHB/external-texture dimensions (the kCeilW/kCeilH ceiling the buffers were allocated at).
// Size the external Texture2D to THIS, then UV-crop the active pdlr_frame_size sub-rect within it.
// 0 on success, -1 before the buffers are allocated.
PDLR_API int pdlr_buffer_size(int* out_width, int* out_height);

// The core's declared native video rate (Hz) — Dreamcast NTSC ≈ 59.94, PAL = 50. 0 if unknown
// (valid after pdlr_start). Tick the emulator at this rate to keep emulated time correct.
PDLR_API double pdlr_frame_fps(void);

// The core's declared audio sample rate (Hz). 0 if unknown. Valid after pdlr_start.
PDLR_API double pdlr_sample_rate(void);

// --- Audio: the core's PCM (interleaved stereo S16) is buffered, resampled to Unity's output rate,
// and pulled by Unity's OnAudioFilterRead. ---

// Set Unity's audio output rate (AudioSettings.outputSampleRate). Call before/at start.
PDLR_API void pdlr_audio_set_output_rate(int hz);

// Drain up to `count` interleaved-stereo floats into dst; returns the count written (may be < count
// on underflow — zero-fill the remainder for silence). Call from OnAudioFilterRead.
PDLR_API int pdlr_audio_read(float* dst, int count);

// Event id to pass to GL.IssuePluginEvent (issue once, after frames flow, to import the AHB onto
// Unity's device on the render thread). Matches PDLR_EVENT_IMPORT_AHB in the .cpp.
enum { PDLR_EVENT_IMPORT_AHB = 1 };

// The native render-event callback to hand to GL.IssuePluginEvent. NULL-safe.
PDLR_API void* pdlr_GetRenderEventFunc(void);

// 1 once ALL the Unity-side VkImages have been imported, else 0.
PDLR_API int pdlr_unity_image_ready(void);

// Step C — triple buffering. The frame is round-robin'd across N AHB buffers so Unity never samples
// the buffer being written. C# creates one external Texture2D per buffer and, each frame, displays
// the buffer index returned by pdlr_ready_buffer_index().

// Number of AHB buffers (create this many external textures).
PDLR_API int pdlr_buffer_count(void);

// Index of the buffer C# should display this frame (the most recently completed blit). -1 until the
// first frame. Read each frame after Run().
PDLR_API int pdlr_ready_buffer_index(void);

// Address of buffer idx's Unity-device VkImage handle (a VkImage*), for Texture2D.CreateExternalTexture.
// NULL until pdlr_unity_image_ready() == 1 or idx out of range. Stays valid until pdlr_shutdown().
PDLR_API const void* pdlr_get_unity_image_ptr_at(int idx);

// Tear down: unload game, destroy Vulkan, retro_deinit, dlclose. Idempotent.
PDLR_API void pdlr_shutdown(void);

#ifdef __cplusplus
}
#endif

#endif // LIBPDLR_H
