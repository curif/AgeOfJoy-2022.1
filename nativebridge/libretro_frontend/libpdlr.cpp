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
// libpdlr is deliberately GPL-2.0-or-later rather than GPL-3.0: it shares an address space with a
// dlopen'd libretro core (Flycast, GPL-2.0-or-later), and v2-or-later keeps that combination
// unambiguous while still folding cleanly into the GPL-3 Age of Joy work as a whole.
//
// libretro.h and libretro_vulkan.h are used under their own permissive (MIT-style) license.
// See THIRD-PARTY-NOTICES.md.

// libpdlr.cpp — embedded libretro frontend.
// See libpdlr.h and claudedocs/geometrizer_vulkan_cores.md.
//
// Stage 2a-lite: pdlr_probe — dlopen + bind + retro_init + system info.
// Stage 2b-1:    pdlr_start/pdlr_run — Vulkan HW-render negotiation (the core builds its device on
//                an instance WE create — Option B), load a game, run, and confirm the core renders
//                (it calls set_image each frame). Frame capture/readback is stage 2b-2.
//
// The Vulkan negotiation mirrors the proven pattern in AoJ's Assets/.../cwrapper/vulkan.c, except
// the device is OURS (not Unity's). Heavy logging (tags "pdlr" = us, "flycast" = the core).

#include "libpdlr.h"

#include <dlfcn.h>
#include <android/log.h>
#include <unistd.h>
#include <cerrno>
#include <cstdlib>
#include <cstdarg>
#include <cstring>
#include <cstdio>
#include <mutex>
#include <condition_variable>
#include <chrono>
#include <map>
#include <deque>
#include <string>
#include <fstream>
#include <cstdint>
#include <ctime>
#include <atomic>
#include <pthread.h>

#define VK_USE_PLATFORM_ANDROID_KHR
#include <vulkan/vulkan.h>
#include <android/hardware_buffer.h>
#include "libretro.h"
#include "libretro_vulkan.h"

// Unity native plugin API (Step B: reach Unity's VkDevice to import our AHB there). Headers ship
// with the editor under Editor/Data/PluginAPI — added to the build include path (build script).
#include "IUnityInterface.h"
#include "IUnityGraphics.h"
#include "IUnityGraphicsVulkan.h"

// LOGE also feeds the diagnostic ring (see the "diagnostics" block at the top of the anonymous
// namespace), so the frontend can read a failed boot's reason back out of the .so instead of the
// tester having to attach `adb logcat`.
namespace { void diag_errf(const char* fmt, ...) __attribute__((format(printf, 1, 2))); }

#define LOGI(...) __android_log_print(ANDROID_LOG_INFO,  "pdlr", __VA_ARGS__)
#define LOGE(...) do { __android_log_print(ANDROID_LOG_ERROR, "pdlr", __VA_ARGS__); diag_errf(__VA_ARGS__); } while (0)

// --- ASharedMemory_create shim (Flycast nvmem fix) ---------------------------------------------
// The Flycast core has a WEAK undefined ASharedMemory_create and does NOT link libandroid, so in
// this embedded process the symbol resolves to NULL and Flycast falls back to legacy /dev/ashmem
// (SELinux-blocked on Quest → EACCES → nvmem off → fastmem crash). We export a STRONG definition
// here, forwarding to the real libandroid implementation. Because libpdlr is the library that
// dlopen()s the core, our symbol is in the core's resolution scope, so the core's weak ref binds
// here → modern shared memory → nvmem works → dynarec/fastmem at full speed. If the shim's log line
// appears, the core bound to us.
extern "C" __attribute__((visibility("default")))
int ASharedMemory_create(const char* name, size_t size)
{
    static void* la = dlopen("libandroid.so", RTLD_NOW);
    static int (*real)(const char*, size_t) =
        la ? (int (*)(const char*, size_t))dlsym(la, "ASharedMemory_create") : nullptr;
    static bool logged = false;
    if (!logged) { logged = true; LOGI("[ashmem-shim] core bound to our ASharedMemory_create; real=%p", (void*)real); }
    if (!real) { errno = ENOSYS; return -1; }
    return real(name, size);
}

namespace {

// --- diagnostics: keep the reason a boot failed ------------------------------------------------
// A libretro core reports why it refused to load on two channels, and both used to be discarded
// here: its log callback (RETRO_ENVIRONMENT_GET_LOG_INTERFACE → core_log) and
// RETRO_ENVIRONMENT_SET_MESSAGE. Flycast's loadGame() uses BOTH for the same text, so a missing
// arcade BIOS arrives twice as "Error: cannot load BIOS naomi.zip" and used to reach only logcat —
// which a beta tester wearing a headset cannot read. We now keep the last kDiagLines of core output
// (plus our own LOGE lines) in a ring the frontend drains with pdlr_recent_log(), and the single
// most specific reason in pdlr_last_error(). At the default capture level (WARN and above) nothing
// here runs per frame, so the std::string churn is off any hot path.
constexpr size_t kDiagLines = 64;

std::mutex              s_diagMx;
std::deque<std::string> s_diagRing;        // oldest first, capped at kDiagLines
std::string             s_diagLastError;   // set at every pdlr_start failure point
std::string             s_diagCoreError;   // last RETRO_LOG_ERROR line from the core
std::string             s_diagCoreMsg;     // last SET_MESSAGE text (may be benign — see below)
std::string             s_diagJoined;      // stable backing store for pdlr_recent_log()'s char*
std::atomic<int>        s_diagMinLevel{(int)RETRO_LOG_WARN};   // capture this level and above

void diag_trim(std::string& s)
{
    while (!s.empty() && (s.back() == '\n' || s.back() == '\r' || s.back() == ' ' || s.back() == '\t'))
        s.pop_back();
}

void diag_push(const char* tag, const char* text)
{
    if (!text) return;
    std::string s(text);
    diag_trim(s);
    if (s.empty()) return;
    std::lock_guard<std::mutex> lk(s_diagMx);
    s_diagRing.push_back(std::string(tag) + s);
    while (s_diagRing.size() > kDiagLines) s_diagRing.pop_front();
}

void diag_errf(const char* fmt, ...)
{
    char buf[1024];
    va_list ap; va_start(ap, fmt);
    vsnprintf(buf, sizeof(buf), fmt, ap);
    va_end(ap);
    diag_push("pdlr E: ", buf);
}

void diag_set_error(const char* fmt, ...)
{
    char buf[1024];
    va_list ap; va_start(ap, fmt);
    vsnprintf(buf, sizeof(buf), fmt, ap);
    va_end(ap);
    std::lock_guard<std::mutex> lk(s_diagMx);
    s_diagLastError = buf;
}

// Every `return -1` out of pdlr_start goes through this: record the specific reason AND log it
// (LOGE also lands it in the ring, so the one-line reason keeps its position in the boot trace).
#define SET_ERR(...) do { diag_set_error(__VA_ARGS__); LOGE(__VA_ARGS__); } while (0)

// What the core last told us went wrong.
//
// An ERROR log line beats a SET_MESSAGE, because a core also pushes benign notices through
// SET_MESSAGE (Flycast sends "Please upgrade to MAME romsets" that way) and reporting one of those
// as the cause of a failed boot would send a tester the wrong way. The exception: Flycast logs
// "path:line E[TAG]: <text>" and then notifies the bare <text>, so when the notification is exactly
// the tail of the log line the two are the same message and we report the clean one.
std::string diag_core_reason()
{
    std::lock_guard<std::mutex> lk(s_diagMx);
    if (s_diagCoreError.empty()) return s_diagCoreMsg;
    if (!s_diagCoreMsg.empty() && s_diagCoreError.size() >= s_diagCoreMsg.size() &&
        s_diagCoreError.compare(s_diagCoreError.size() - s_diagCoreMsg.size(),
                                s_diagCoreMsg.size(), s_diagCoreMsg) == 0)
        return s_diagCoreMsg;
    return s_diagCoreError;
}

// libretro.h typedefs the *callback* types but NOT the core's exported entry points.
typedef void     (*fp_set_environment)(retro_environment_t);
typedef void     (*fp_set_video_refresh)(retro_video_refresh_t);
typedef void     (*fp_set_audio_sample)(retro_audio_sample_t);
typedef void     (*fp_set_audio_sample_batch)(retro_audio_sample_batch_t);
typedef void     (*fp_set_input_poll)(retro_input_poll_t);
typedef void     (*fp_set_input_state)(retro_input_state_t);
typedef void     (*fp_init)(void);
typedef void     (*fp_deinit)(void);
typedef unsigned (*fp_api_version)(void);
typedef void     (*fp_get_system_info)(struct retro_system_info*);
typedef void     (*fp_get_system_av_info)(struct retro_system_av_info*);
typedef void     (*fp_run)(void);
typedef bool     (*fp_load_game)(const struct retro_game_info*);
typedef void     (*fp_unload_game)(void);
typedef void     (*fp_set_controller_port_device)(unsigned, unsigned);
typedef size_t   (*fp_serialize_size)(void);
typedef bool     (*fp_serialize)(void*, size_t);
typedef bool     (*fp_unserialize)(const void*, size_t);

struct Core {
    void* handle = nullptr;
    fp_set_environment        retro_set_environment        = nullptr;
    fp_set_video_refresh      retro_set_video_refresh      = nullptr;
    fp_set_audio_sample       retro_set_audio_sample       = nullptr;
    fp_set_audio_sample_batch retro_set_audio_sample_batch = nullptr;
    fp_set_input_poll         retro_set_input_poll         = nullptr;
    fp_set_input_state        retro_set_input_state        = nullptr;
    fp_init                   retro_init                   = nullptr;
    fp_deinit                 retro_deinit                 = nullptr;
    fp_api_version            retro_api_version            = nullptr;
    fp_get_system_info        retro_get_system_info        = nullptr;
    fp_get_system_av_info     retro_get_system_av_info     = nullptr;
    fp_run                    retro_run                    = nullptr;
    fp_load_game              retro_load_game              = nullptr;
    fp_unload_game            retro_unload_game            = nullptr;
    fp_set_controller_port_device retro_set_controller_port_device = nullptr;
    fp_serialize_size         retro_serialize_size         = nullptr;
    fp_serialize              retro_serialize              = nullptr;
    fp_unserialize            retro_unserialize            = nullptr;
    bool inited     = false;
    bool gameLoaded = false;
    char name[256]    = {0};
    char version[256] = {0};
};
Core g;

// Frontend-provided directories (returned to the core via the environment callback).
char s_systemDir[1024] = {0};
char s_saveDir[1024]   = {0};
char s_gameDir[1024]   = {0};   // dir holding the game + knob/state files (= optDir), for the boot-state transplant

// Optional libretro core-option overrides, keyed by option name (e.g. "reicast_enable_dsp").
// Loaded from a "Flycast.opt" file (RetroArch's `key = "value"` format) so the values can be
// tuned without rebuilding. Empty = we answer GET_VARIABLE with false → core uses its own defaults.
static std::map<std::string, std::string> s_coreOptions;

// Per-cabinet core-option overrides pushed from the frontend via pdlr_set_option BEFORE pdlr_start
// (from a cabinet's description.yaml `environment:` block). Overlaid on top of s_coreOptions after
// the Flycast.opt defaults load, so YAML wins and any option it doesn't name keeps its default.
static std::map<std::string, std::string> s_optOverrides;

// Parse <dir>/Flycast.opt into s_coreOptions. Lines look like: reicast_enable_dsp = "enabled".
// Missing file is fine (we just fall through to the core's built-in defaults).
static void load_core_options(const char* dir)
{
    s_coreOptions.clear();
    if (!dir || !*dir) return;
    std::string path = std::string(dir) + "/Flycast.opt";
    std::ifstream f(path);
    if (!f.is_open()) { LOGI("[opt] no '%s' — using core defaults", path.c_str()); return; }

    std::string line;
    int n = 0;
    while (std::getline(f, line)) {
        size_t eq = line.find('=');
        if (eq == std::string::npos) continue;

        // key = trimmed text left of '='
        size_t ks = line.find_first_not_of(" \t");
        if (ks == std::string::npos || ks >= eq) continue;
        size_t ke = line.find_last_not_of(" \t\r", eq - 1);
        std::string key = line.substr(ks, ke - ks + 1);

        // value = text between the first and last double-quote right of '='
        std::string rest = line.substr(eq + 1);
        size_t q1 = rest.find('"');
        size_t q2 = rest.rfind('"');
        if (q1 == std::string::npos || q2 <= q1) continue;
        s_coreOptions[key] = rest.substr(q1 + 1, q2 - q1 - 1);
        n++;
    }
    LOGI("[opt] loaded %d core option override(s) from '%s'", n, path.c_str());
}

// Core-declared A/V timing (from retro_get_system_av_info + SET_SYSTEM_AV_INFO updates).
// fps is the core's native video rate (Dreamcast NTSC ≈ 59.94, PAL = 50). 0 = unknown.
double s_fps        = 0.0;
double s_sampleRate = 0.0;

// --- Dedicated emu-pump thread -------------------------------------------------------------------
// retro_run is driven from OUR thread at exactly the core's native rate (59.94 Hz), not from Unity's
// Update. Why: with threaded rendering the guest is paced by retro_run consuming presented frames,
// and Flycast's frame wait is 5 × 20 ms timeouts — calling it on Unity's main thread both (a) stalls
// the VR loop up to ~100 ms whenever the guest hiccups (measured: retro_run max=109-183 ms) and
// (b) forces catch-up bursts afterwards (guest visibly fast-forwards, audio ring overflows → the
// core DROPS audio wholesale, shredding the mix). A dedicated absolute-deadline loop = RetroArch's
// run-loop architecture: steady cadence, hitches absorbed off the VR thread, no bursts (backlog is
// dropped, never replayed). Unity keeps displaying the triple-buffered AHB and draining the audio
// ring — both already thread-safe. pdlr_run() from C# becomes a no-op while the pump is active.
volatile bool s_pumpStop   = false;
bool          s_pumpActive = false;
pthread_t     s_pumpThread;

// Pause policy: the pump suspends while Unity's app is paused (headset off / system overlay took
// over). Running through the pause bought nothing — Unity stops pulling OnAudioFilterRead so every
// produced sample is dropped, the guest silently advances past whatever the user expected to see,
// and it burns battery. RetroArch pauses there too. Set from C# (OnApplicationPause) via
// pdlr_set_paused(); the pump resyncs its deadline schedule on resume, so no catch-up burst.
std::atomic<bool> s_pumpPaused{false};

// --- Display-locked pacing (phase-lock the pump to Unity's render cadence) -----------------------
// The pump's native rate (≈59.94 Hz) and the VR display (72 Hz) are unrelated clocks. A free-running
// wall-clock deadline makes the inherent 6:5 pulldown BEAT: the doubled frame drifts, and every
// retro_run hitch resyncs and re-randomizes the phase, so 60-on-72 reads as irregular judder even
// though the compositor holds 72 rock-solid. Fix: pace the pump off Unity's per-frame tick instead.
// Each rendered frame Unity calls pdlr_notify_display_frame() (bump s_displayFrame + signal the cv);
// the pump produces one emulated frame per (coreFps/displayHz ≈ 0.832) display ticks via a Bresenham
// credit accumulator — a steady, phase-stable 5:6 cadence locked to the ACTUAL presented frames.
// Emulated time still averages coreFps (audio stays correct; the governor/back-pressure below are
// untouched — this only replaces the video pacer's wall-clock sleep). displaylock.txt=0 reverts to
// the old free-running deadline for A/B.
std::atomic<uint64_t>   s_displayFrame{0};   // bumped once per Unity rendered frame (main thread)
std::mutex              s_displayMutex;
std::condition_variable s_displayCv;
double s_displayHz  = 72.0;    // Unity's actual refresh (pdlr_set_display_hz; default Quest 72)
bool   s_displayLock = true;   // knob displaylock.txt=0 → old wall-clock deadline pacer
double   s_paceCredit = 0.0;          // accumulated emulated-frame credit (pump thread only)
uint64_t s_paceLastDisplay = 0;       // last observed s_displayFrame (pump thread only)
std::atomic<bool> s_paceResync{true}; // set on resume → re-anchor the credit window

// Cumulative audio frames delivered by the core (audio_sample_batch). The pump's [speed] log prints
// the per-second delta — ground truth for guest speed: ≈44100/s = real-time, more = fast-forward,
// less = stalled.
std::atomic<uint64_t> s_prodFrames{0};

// --- Audio: interleaved-stereo float buffer, fed by the core's audio_sample_batch (producer = core
// thread during retro_run), drained by Unity's OnAudioFilterRead (consumer = audio thread) → mutex.
// S16 → float, linearly resampled from the core's rate to Unity's output rate. Mirrors cwrapper/audio.c.
const int  kAudioCap   = 1 << 16;          // floats (~0.68 s of 48 kHz stereo) — plenty of slack
float      s_audio[kAudioCap];
int        s_audioCount   = 0;             // floats currently buffered
int        s_audioOutRate = 48000;         // Unity output rate (set from C#)
std::mutex s_audioMutex;

// --- Audio back-pressure (RA-faithful pacing experiment) ----------------------------------------
// RetroArch's blocking audio driver paces retro_run by real audio consumption: audio_batch_cb blocks
// until the device buffer has room. We normally buffer-and-drop instead, so the emu is paced only by
// our wall-clock pump — same AVERAGE rate (realtime=1.00x) but a different sub-frame phase than RA.
// VF3's ARM7 sound-driver INIT is timing-sensitive (it fails on the tight buildbot core in our
// frontend: SCIEB stays 0 → no streamed BGM; it succeeds under RA and on our looser instrumented
// build). So we mirror RA: when the buffer already holds ~s_backpressureMs of audio, audio_batch_cb
// (called inside retro_run on the pump thread) WAITS until Unity's OnAudioFilterRead drains it — the
// consumer signals s_audioSpaceCv. A wait timeout keeps a stalled/idle consumer from freezing video
// (falls back to the old drop path). s_backpressureMs<=0 disables it (old behavior).
std::condition_variable s_audioSpaceCv;
// Default OFF. Back-pressure exists only for Flycast-DC's ARM7/AICA sound-driver INIT race, which is
// now fixed in the core (clean-throttle build) — so it earns nothing and costs a shallow, easily
// starved audio buffer plus an in-retro_run producer stall. Cores with no such race (Modelizer, a
// Model2 MAME fork) want the full buffer depth. Re-enable per-run via backpressure.txt if ever needed.
int  s_backpressureMs = 0;    // target buffered ms before the producer blocks (0 = off). Knob: backpressure.txt

// --- Boot governor: hard real-time cap on guest audio production --------------------------------
// Measured on a dead boot (2026-07-04): in the first ~3 s the guest free-runs at up to 2.28x real
// time, delivering 380 ms monster batches (17k frames) — Flycast's emu thread is unpaced until
// frame presentation settles, the condvar back-pressure times out (bp+20 ms) and the 100 ms latency
// cap silently drops the excess, so the storm sails through. The AICA/ARM7 sound-driver init runs
// INSIDE that storm; under RetroArch the storm cannot exist (its blocking audio driver stalls the
// first monster batch for its full duration, pacing the guest to 1.00x from sample #1) — and RA
// boots music 100%. So: govern production against the wall clock, consumer or no consumer, no
// timeout. If cumulative produced audio runs >100 ms ahead of elapsed real time, sleep the excess
// (chunked, abortable). Steady state stays dormant (production == real time). The pause path
// requests a resync so a suspended pump doesn't bank "debt" and free-run to catch up on resume.
int64_t  s_govStartMs   = -1;   // epoch of the governor window (pump thread only)
uint64_t s_govBaseFrames = 0;   // frames produced before the window began (pump thread only)
std::atomic<bool> s_govResync{false};   // set from other threads (resume) → re-anchor the window
bool     s_governorOn   = true; // knob governor.txt=0 disables the wall-clock sleeps (A/B the boot race)
static int64_t mono_ms()
{
    struct timespec ts; clock_gettime(CLOCK_MONOTONIC, &ts);
    return (int64_t)ts.tv_sec * 1000 + ts.tv_nsec / 1000000;
}

// ---- Vulkan HW render state (Option B: the core renders on a device WE create) ----------------
struct Vk {
    VkInstance       instance    = VK_NULL_HANDLE;
    VkPhysicalDevice phys        = VK_NULL_HANDLE;
    VkDevice         device      = VK_NULL_HANDLE;
    VkQueue          queue       = VK_NULL_HANDLE;
    uint32_t         queueFamily = 0;
    bool             contextReady = false;

    struct retro_hw_render_callback hwcb{};
    bool haveHwcb = false;
    struct retro_hw_render_context_negotiation_interface_vulkan nego{};
    bool haveNego = false;
    struct retro_hw_render_interface_vulkan iface{};

    // Latest frame handed to us by the core via set_image.
    struct retro_vulkan_image lastImage{};
    bool      haveImage      = false;
    uint64_t  imageCount     = 0;
    // Sync-index ring (the real libretro-vulkan contract, RetroArch-parity). Flycast sizes its
    // whole render chain from get_sync_index_mask (colorAttachments, CommandPool fences/pools,
    // descriptor sets — vk_context_lr.h GetSwapChainSize) and renders each frame into
    // colorAttachments[get_sync_index()]. Our old degenerate mask=1/index=0 forced chainSize=1:
    // ONE fence, ONE command pool, ONE attachment reused every frame with no ownership protocol —
    // the config RetroArch never runs (crzytaxi guest-spin, see flycast_bridge_sync_fix_plan.md).
    // 3 indices ≈ RA's swapchain depth; each presented frame lands in its own attachment.
    // Rotation happens in vk_set_image: every consumer of get_sync_index (beginFrame, quad/overlay
    // draws, the retro_image handoff) runs BEFORE set_image within PresentFrame on this same
    // thread, so advancing there never tears a frame. Flycast never calls wait_sync_index (verified
    // v7ec978e), and our blit of attachment[i] fence-completes inside the same pump tick — 2 frames
    // before the core can revisit index i — so the no-op wait stays correct by construction.
    uint32_t  syncIndex      = 0;
    const VkSemaphore* semaphores = nullptr;
    uint32_t  numSemaphores  = 0;
    uint32_t  srcQueueFamily = 0;

    std::mutex queueMutex;   // lock_queue/unlock_queue — core may submit from its own threads
};
Vk vk;

// --- CPU read-back of the core's set_image frame (stage 2b-2: simplest path to a visible frame) ---
int             frameW = 0, frameH = 0;   // from video_refresh
VkCommandPool   rbPool   = VK_NULL_HANDLE;
VkCommandBuffer rbCmd    = VK_NULL_HANDLE;
VkFence         rbFence  = VK_NULL_HANDLE;
VkBuffer        rbBuf    = VK_NULL_HANDLE;
VkDeviceMemory  rbMem    = VK_NULL_HANDLE;
void*           rbMapped = nullptr;
int             rbW = 0, rbH = 0;
bool            rbReady  = false;
uint64_t        rbErr    = 0;

// --- 2c zero-copy: AHB-backed shared image (core device) + Unity import (Unity device) ----------
// Default path. The core renders into its OWN image; each frame we blit that into an AHB-backed
// image on the core's device (one GPU copy, also normalizes layout), and Unity samples the SAME
// AHardwareBuffer via its own imported VkImage — no CPU copy. Reuses libpdvk's proven Step B.
volatile bool   s_zeroCopy = true;            // false → legacy CPU readback path (pdlr_get_frame)

// Step C: triple-buffered AHB. We rotate the blit destination across N buffers so Unity never
// samples the buffer we're currently overwriting (the writer/reader race). 3 gives a 2-tick safety
// margin against Unity's render-thread lag. Each buffer is imported once onto Unity's device; C#
// holds N external textures and each frame displays the just-blitted ("ready") one — zero added
// latency (Option 2: we keep the short blit-completion fence rather than show last frame's buffer).
const int kNumBuf = 3;

// Fixed AHB ceiling. The core (DC/NAOMI/Atomiswave at native internal res) never emits a frame
// larger than 640x480 (confirmed on record), but SOME titles change resolution mid-run — e.g.
// Metal Slug 6 (Atomiswave) boots in a transient 640x238 VO mode, then switches to 640x480. The
// AHB import is one-shot, so we allocate the buffers ONCE at this ceiling (never from the first
// frame's dims) and blit the active sub-rect into the top-left. C# UV-crops to the active region;
// when the active frame == the ceiling (the usual case: DOA2, in-game Metal Slug) the crop is
// identity, so full-frame titles behave exactly as before. See "MSLUG6 half-frame" in the doc.
const int kCeilW = 640, kCeilH = 480;

struct AhbBuf {
    AHardwareBuffer* ahb        = nullptr;        // shared buffer (we own it)
    VkImage          image      = VK_NULL_HANDLE; // core-device image aliasing the AHB (blit dest)
    VkDeviceMemory   mem        = VK_NULL_HANDLE;
    bool             firstUse   = true;           // first barrier: UNDEFINED -> TRANSFER_DST
    VkImage          unityImage = VK_NULL_HANDLE; // Unity-device image aliasing the SAME AHB
    VkDeviceMemory   unityMem   = VK_NULL_HANDLE;
};
AhbBuf bufs[kNumBuf];
int    ahbW = 0, ahbH = 0;
int    writeIdx = 0;                  // next buffer to blit into
int    readyIdx = -1;                 // last fully-blitted buffer (what C# should display)
bool   unityImagesReady = false;      // all kNumBuf imported onto Unity's device
PFN_vkGetAndroidHardwareBufferPropertiesANDROID fpGetAhbProps = nullptr;  // on the core's device
VkCommandPool    blitPool    = VK_NULL_HANDLE;   // shared (blit is fence-serialized)
VkCommandBuffer  blitCmd     = VK_NULL_HANDLE;
VkFence          blitFence   = VK_NULL_HANDLE;
bool             blitPending = false;   // pipelined fence: a blit was submitted and not yet waited
int              blitPendingIdx = -1;   // buffer that pending blit wrote; published once its fence signals
uint64_t         blitErr     = 0;
uint64_t         lastBlitImageCount = 0;     // skip the blit when the core produced no new frame
uint64_t         lastRbImageCount   = 0;     // same, for the CPU read-back path

// Unity native plugin interfaces (Step B import).
IUnityInterfaces*     s_interfaces    = nullptr;
IUnityGraphicsVulkan* s_uvk           = nullptr;
// PDLR_EVENT_IMPORT_AHB (per-plugin event-id namespace, no clash with pdvk) comes from libpdlr.h.

uint32_t findMemType(VkPhysicalDevice phys, uint32_t bits, VkMemoryPropertyFlags want)
{
    VkPhysicalDeviceMemoryProperties mp;
    vkGetPhysicalDeviceMemoryProperties(phys, &mp);
    for (uint32_t i = 0; i < mp.memoryTypeCount; ++i)
        if ((bits & (1u << i)) && (mp.memoryTypes[i].propertyFlags & want) == want) return i;
    return UINT32_MAX;
}

// ---- hw_render_interface callbacks given to the core (signatures from libretro_vulkan.h) -------

const uint32_t kSyncCount = 3;   // ring depth; mask below must stay contiguous-from-bit-0

void vk_set_image(void* /*handle*/, const struct retro_vulkan_image* image,
                  uint32_t num_semaphores, const VkSemaphore* semaphores, uint32_t src_queue_family)
{
    vk.lastImage      = *image;
    vk.semaphores     = semaphores;
    vk.numSemaphores  = num_semaphores;
    vk.srcQueueFamily = src_queue_family;
    vk.haveImage      = true;
    vk.imageCount++;
    if (vk.imageCount <= 3 || (vk.imageCount % 120) == 0)
        LOGI("[vk] set_image #%llu view=%p layout=%d img=%p fmt=%d numSem=%u srcQF=%u syncIdx=%u",
             (unsigned long long)vk.imageCount, (void*)image->image_view, image->image_layout,
             (void*)image->create_info.image, image->create_info.format, num_semaphores, src_queue_family,
             vk.syncIndex);
    // Advance the ring: the core's NEXT frame renders into a different attachment (see Vk::syncIndex).
    vk.syncIndex = (vk.syncIndex + 1) % kSyncCount;
}
uint32_t vk_get_sync_index(void*)        { return vk.syncIndex; }
uint32_t vk_get_sync_index_mask(void*)   { return (1u << kSyncCount) - 1; }
void     vk_set_command_buffers(void*, uint32_t, const VkCommandBuffer*) {}
void     vk_wait_sync_index(void*)       {}
void     vk_lock_queue(void*)            { vk.queueMutex.lock(); }
void     vk_unlock_queue(void*)          { vk.queueMutex.unlock(); }
void     vk_set_signal_semaphore(void*, VkSemaphore) {}

void fill_iface()
{
    memset(&vk.iface, 0, sizeof(vk.iface));
    vk.iface.interface_type       = RETRO_HW_RENDER_INTERFACE_VULKAN;
    vk.iface.interface_version    = RETRO_HW_RENDER_INTERFACE_VULKAN_VERSION;
    vk.iface.handle               = nullptr;
    vk.iface.instance             = vk.instance;
    vk.iface.gpu                  = vk.phys;
    vk.iface.device               = vk.device;
    vk.iface.get_device_proc_addr = vkGetDeviceProcAddr;
    vk.iface.get_instance_proc_addr = vkGetInstanceProcAddr;
    vk.iface.queue                = vk.queue;
    vk.iface.queue_index          = vk.queueFamily;
    vk.iface.set_image            = vk_set_image;
    vk.iface.get_sync_index       = vk_get_sync_index;
    vk.iface.get_sync_index_mask  = vk_get_sync_index_mask;
    vk.iface.set_command_buffers  = vk_set_command_buffers;
    vk.iface.wait_sync_index      = vk_wait_sync_index;
    vk.iface.lock_queue           = vk_lock_queue;
    vk.iface.unlock_queue         = vk_unlock_queue;
    vk.iface.set_signal_semaphore = vk_set_signal_semaphore;
    LOGI("[vk] sync ring: %u indices (mask=0x%x) — real rotating contract (was mask=1/idx=0)",
         kSyncCount, (1u << kSyncCount) - 1);
}

// --- 2c: intercept vkCreateDevice so the core's device gets our AHB extensions -----------------
// Flycast's create_device ignores the frontend's required_device_extensions (verified on device:
// it enabled only swapchain + provoking_vertex). But WE hand the core its get_instance_proc_addr,
// so we give it a shim whose vkCreateDevice appends the AHB exts to whatever the core requests,
// then forwards to the real one. The core is unmodified and unaware. (Same trick the doc calls
// "InterceptInitialization", here applied to the core's device instead of Unity's.)
const char* const kAhbDeviceExts[] = {
    "VK_ANDROID_external_memory_android_hardware_buffer",
    "VK_EXT_queue_family_foreign",
};

VKAPI_ATTR VkResult VKAPI_CALL pdlr_vkCreateDevice(
    VkPhysicalDevice physicalDevice, const VkDeviceCreateInfo* pCreateInfo,
    const VkAllocationCallbacks* pAllocator, VkDevice* pDevice)
{
    PFN_vkCreateDevice real = (PFN_vkCreateDevice)vkGetInstanceProcAddr(vk.instance, "vkCreateDevice");
    if (!real) { LOGE("[vk-shim] real vkCreateDevice unresolved"); return VK_ERROR_INITIALIZATION_FAILED; }

    const char* exts[64];
    uint32_t n = 0;
    for (uint32_t i = 0; i < pCreateInfo->enabledExtensionCount && n < 62; ++i)
        exts[n++] = pCreateInfo->ppEnabledExtensionNames[i];
    for (const char* want : kAhbDeviceExts) {
        bool have = false;
        for (uint32_t i = 0; i < n; ++i) if (strcmp(exts[i], want) == 0) { have = true; break; }
        if (!have && n < 64) exts[n++] = want;
    }
    VkDeviceCreateInfo ci = *pCreateInfo;   // keep the core's pNext/features chain as-is
    ci.enabledExtensionCount   = n;
    ci.ppEnabledExtensionNames = exts;
    LOGI("[vk-shim] vkCreateDevice: extensions %u -> %u (AHB injected)",
         pCreateInfo->enabledExtensionCount, n);
    return real(physicalDevice, &ci, pAllocator, pDevice);
}

VKAPI_ATTR PFN_vkVoidFunction VKAPI_CALL pdlr_GetInstanceProcAddr(VkInstance instance, const char* name)
{
    if (name && strcmp(name, "vkCreateDevice") == 0)
        return (PFN_vkVoidFunction)pdlr_vkCreateDevice;
    return vkGetInstanceProcAddr(instance, name);
}

// When the core provides NO negotiation create_device (the standard libretro-Vulkan model: the
// FRONTEND owns device creation and the core queries it via GET_HW_RENDER_INTERFACE), libpdlr builds
// the VkDevice itself — same as RetroArch's default path, plus the AHB import extensions our
// zero-copy blit needs on the render device. Flycast-DC is the odd core that does its own; every
// normal core (Modelizer, …) lands here.
bool create_device_frontend_default(struct retro_vulkan_context* ctx)
{
    // 1. A graphics+compute queue family (the interface contract requires the core's queue to support
    //    BOTH; Adreno exposes a single universal family that does).
    uint32_t qfCount = 0;
    vkGetPhysicalDeviceQueueFamilyProperties(vk.phys, &qfCount, nullptr);
    if (!qfCount) { LOGE("[vk] frontend-default: no queue families"); return false; }
    VkQueueFamilyProperties qfs[16];
    if (qfCount > 16) qfCount = 16;
    vkGetPhysicalDeviceQueueFamilyProperties(vk.phys, &qfCount, qfs);
    const VkQueueFlags need = VK_QUEUE_GRAPHICS_BIT | VK_QUEUE_COMPUTE_BIT;
    uint32_t family = UINT32_MAX;
    for (uint32_t i = 0; i < qfCount; ++i)
        if ((qfs[i].queueFlags & need) == need) { family = i; break; }
    if (family == UINT32_MAX) { LOGE("[vk] frontend-default: no graphics+compute queue family"); return false; }

    const float prio = 1.0f;
    VkDeviceQueueCreateInfo qci{ VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO };
    qci.queueFamilyIndex = family;
    qci.queueCount       = 1;
    qci.pQueuePriorities = &prio;

    // 2. Enable every feature the GPU supports — matches RetroArch's default device (the core logs
    //    features as "supported, not enabled" and renders correctly under it, so the full supported
    //    set is the safe superset). Core 1.0 features need no extensions to enable.
    VkPhysicalDeviceFeatures features{};
    vkGetPhysicalDeviceFeatures(vk.phys, &features);

    // 3. On the zero-copy path the render device must carry the AHB import extensions so we can alias
    //    the core's frame into an AndroidHardwareBuffer and hand it to Unity (same exts the shim
    //    injects on the negotiation path).
    const char* exts[8]; uint32_t n = 0;
    if (s_zeroCopy) for (const char* e : kAhbDeviceExts) exts[n++] = e;

    VkDeviceCreateInfo dci{ VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO };
    dci.queueCreateInfoCount    = 1;
    dci.pQueueCreateInfos       = &qci;
    dci.enabledExtensionCount   = n;
    dci.ppEnabledExtensionNames = n ? exts : nullptr;
    dci.pEnabledFeatures        = &features;

    VkDevice dev = VK_NULL_HANDLE;
    VkResult r = vkCreateDevice(vk.phys, &dci, nullptr, &dev);
    if (r != VK_SUCCESS) { LOGE("[vk] frontend-default vkCreateDevice failed (%d)", r); return false; }

    VkQueue q = VK_NULL_HANDLE;
    vkGetDeviceQueue(dev, family, 0, &q);

    ctx->gpu                             = vk.phys;
    ctx->device                          = dev;
    ctx->queue                           = q;
    ctx->queue_family_index              = family;
    ctx->presentation_queue              = q;      // no surface — present queue == render queue
    ctx->presentation_queue_family_index = family;
    LOGI("[vk] frontend-default create_device OK: device=%p queue=%p family=%u exts=%u features=all-supported",
         (void*)dev, (void*)q, family, n);
    return true;
}

// Create our VkInstance, let the core build its VkDevice on it (negotiation), then context_reset.
int create_vulkan_context()
{
    if (!vk.haveHwcb) { LOGE("[vk] create_vulkan_context: core never called SET_HW_RENDER"); return -1; }
    if (vk.hwcb.context_type != RETRO_HW_CONTEXT_VULKAN) {
        LOGE("[vk] core requested HW context_type=%d (NOT Vulkan=%d) — aborting",
             vk.hwcb.context_type, RETRO_HW_CONTEXT_VULKAN);
        return -1;
    }

    VkApplicationInfo defApp{ VK_STRUCTURE_TYPE_APPLICATION_INFO };
    defApp.pApplicationName = "pdlr";
    defApp.apiVersion       = VK_API_VERSION_1_1;
    const VkApplicationInfo* app = &defApp;
    if (vk.haveNego && vk.nego.get_application_info) {
        const VkApplicationInfo* a = vk.nego.get_application_info();
        if (a) { app = a; LOGI("[vk] using core app info (apiVersion=0x%x)", a->apiVersion); }
    }

    VkInstanceCreateInfo ici{ VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO };
    ici.pApplicationInfo = app;
    VkResult r = vkCreateInstance(&ici, nullptr, &vk.instance);
    if (r != VK_SUCCESS) { LOGE("[vk] vkCreateInstance failed (%d)", r); return -1; }
    LOGI("[vk] vkCreateInstance OK (instance=%p)", (void*)vk.instance);

    uint32_t n = 0;
    vkEnumeratePhysicalDevices(vk.instance, &n, nullptr);
    if (!n) { LOGE("[vk] no physical devices"); return -1; }
    VkPhysicalDevice devs[8]; if (n > 8) n = 8;
    vkEnumeratePhysicalDevices(vk.instance, &n, devs);
    vk.phys = devs[0];
    VkPhysicalDeviceProperties pp; vkGetPhysicalDeviceProperties(vk.phys, &pp);
    LOGI("[vk] gpu: %s", pp.deviceName);

    struct retro_vulkan_context ctx; memset(&ctx, 0, sizeof(ctx));
    if (vk.haveNego && vk.nego.create_device) {
        // Negotiation path: the core builds the VkDevice (Flycast-DC). On the zero-copy path we hand
        // it the AHB-injecting shim proc-addr so its vkCreateDevice picks up our extensions.
        // 2c: ask the core to enable the AHB-import extensions on the device it builds, so we can
        // allocate an AHB-backed image on THAT device and blit the core's frame into it. The KHR deps
        // (external_memory, ycbcr, dedicated_allocation, bind_memory2, get_memory_requirements2) are
        // core in Vulkan 1.1. queue_family_foreign is a required dep of the AHB extension. For a plain
        // RGBA8 (non-external-format) AHB no samplerYcbcrConversion *feature* is needed.
        VkPhysicalDeviceFeatures features; memset(&features, 0, sizeof(features));
        const char* reqExts[] = {
            "VK_ANDROID_external_memory_android_hardware_buffer",
            "VK_EXT_queue_family_foreign",
        };
        // Pass the shim get_instance_proc_addr on the zero-copy path so the core's vkCreateDevice gets
        // our AHB extensions injected (Flycast ignores required_device_extensions, so we also pass them
        // directly as a courtesy for cores that DO honor them).
        bool ok = vk.nego.create_device(&ctx, vk.instance, vk.phys, VK_NULL_HANDLE /*surface*/,
                                        s_zeroCopy ? pdlr_GetInstanceProcAddr : vkGetInstanceProcAddr,
                                        s_zeroCopy ? reqExts : nullptr,
                                        s_zeroCopy ? (unsigned)(sizeof(reqExts)/sizeof(reqExts[0])) : 0,
                                        nullptr, 0, &features);
        if (!ok) { LOGE("[vk] core create_device returned FALSE"); return -1; }
        LOGI("[vk] core negotiation create_device OK");
    } else {
        // Standard libretro-Vulkan model: no core create_device, so the frontend owns the device.
        // The core reads it back via GET_HW_RENDER_INTERFACE (Modelizer, and every normal core).
        if (!create_device_frontend_default(&ctx)) return -1;
    }
    vk.phys        = ctx.gpu;
    vk.device      = ctx.device;
    vk.queue       = ctx.queue;
    vk.queueFamily = ctx.queue_family_index;
    LOGI("[vk] create_device OK: gpu=%p device=%p queue=%p queueFamily=%u",
         (void*)vk.phys, (void*)vk.device, (void*)vk.queue, vk.queueFamily);

    if (s_zeroCopy) {
        fpGetAhbProps = (PFN_vkGetAndroidHardwareBufferPropertiesANDROID)
            vkGetDeviceProcAddr(vk.device, "vkGetAndroidHardwareBufferPropertiesANDROID");
        if (!fpGetAhbProps) {
            LOGE("[vk] AHB-props fn unresolved on the core's device — the core ignored our required "
                 "extensions. Falling back to CPU readback (set zeroCopy=false to silence).");
            s_zeroCopy = false;
        } else {
            LOGI("[vk] AHB extensions enabled on core device; AHB-props fn=%p", (void*)fpGetAhbProps);
        }
    }

    fill_iface();
    vk.contextReady = true;   // set before context_reset so GET_HW_RENDER_INTERFACE can succeed

    LOGI("[vk] calling context_reset");
    if (vk.hwcb.context_reset) vk.hwcb.context_reset();
    LOGI("[vk] context_reset returned");
    return 0;
}

// ---- environment + I/O callbacks --------------------------------------------------------------

void RETRO_CALLCONV core_log(enum retro_log_level level, const char* fmt, ...)
{
    int prio = (level == RETRO_LOG_ERROR) ? ANDROID_LOG_ERROR
             : (level == RETRO_LOG_WARN)  ? ANDROID_LOG_WARN : ANDROID_LOG_INFO;

    // Formatted once, then reused for logcat and the diagnostic ring.
    char buf[1024];
    va_list ap; va_start(ap, fmt);
    vsnprintf(buf, sizeof(buf), fmt, ap);
    va_end(ap);
    __android_log_write(prio, "flycast", buf);

    if ((int)level >= s_diagMinLevel.load(std::memory_order_relaxed))
        diag_push(level == RETRO_LOG_ERROR ? "core E: "
                : level == RETRO_LOG_WARN  ? "core W: "
                : level == RETRO_LOG_INFO  ? "core I: " : "core D: ", buf);

    // The channel Flycast's loadGame() reports a FlycastException on, right before returning false
    // from retro_load_game — this is where "Error: cannot load BIOS naomi.zip" comes from.
    if (level == RETRO_LOG_ERROR) {
        std::string s(buf);
        diag_trim(s);
        std::lock_guard<std::mutex> lk(s_diagMx);
        s_diagCoreError = s;
    }
}

bool RETRO_CALLCONV environment_cb(unsigned cmd, void* data)
{
    switch (cmd) {
        case RETRO_ENVIRONMENT_GET_LOG_INTERFACE:
            ((struct retro_log_callback*)data)->log = core_log;
            return true;
        case RETRO_ENVIRONMENT_GET_CAN_DUPE:
            if (data) *(bool*)data = true;
            return true;

        // The core's user-facing notification channel. We have no OSD, but the text is exactly what
        // a tester needs: Flycast routes its FlycastException here via os_notify() when a game
        // refuses to boot. Returning false (the old default-case behaviour) discarded it.
        case RETRO_ENVIRONMENT_SET_MESSAGE: {
            const struct retro_message* m = (const struct retro_message*)data;
            if (!m || !m->msg) return false;
            LOGI("[env] SET_MESSAGE: %s", m->msg);
            diag_push("core msg: ", m->msg);
            { std::string s(m->msg); diag_trim(s); std::lock_guard<std::mutex> lk(s_diagMx); s_diagCoreMsg = s; }
            return true;
        }
        case RETRO_ENVIRONMENT_SET_MESSAGE_EXT: {
            const struct retro_message_ext* m = (const struct retro_message_ext*)data;
            if (!m || !m->msg) return false;
            LOGI("[env] SET_MESSAGE_EXT level=%d: %s", (int)m->level, m->msg);
            diag_push("core msg: ", m->msg);
            { std::string s(m->msg); diag_trim(s); std::lock_guard<std::mutex> lk(s_diagMx); s_diagCoreMsg = s; }
            return true;
        }

        // Polled every frame — handle silently (no per-frame logcat traffic).
        case RETRO_ENVIRONMENT_GET_VARIABLE_UPDATE:
            if (data) *(bool*)data = false;   // core options never change at runtime here
            return true;
        case RETRO_ENVIRONMENT_GET_FASTFORWARDING:
            if (data) *(bool*)data = false;   // we never fast-forward
            return true;
        case RETRO_ENVIRONMENT_SET_PIXEL_FORMAT:
            LOGI("[env] SET_PIXEL_FORMAT %u", data ? *(const unsigned*)data : 0);
            return true;

        case RETRO_ENVIRONMENT_SET_SYSTEM_AV_INFO: {
            // The core is updating its timing/geometry mid-run (region/interlace change, etc.).
            const struct retro_system_av_info* av = (const struct retro_system_av_info*)data;
            if (av && av->timing.fps > 0.0) {
                s_fps = av->timing.fps;
                s_sampleRate = av->timing.sample_rate;
                LOGI("[env] SET_SYSTEM_AV_INFO fps=%.4f sample_rate=%.1f geom=%ux%u",
                     av->timing.fps, av->timing.sample_rate,
                     av->geometry.base_width, av->geometry.base_height);
            }
            return true;
        }

        case RETRO_ENVIRONMENT_GET_PREFERRED_HW_RENDER:
            if (data) *(unsigned*)data = RETRO_HW_CONTEXT_VULKAN;
            LOGI("[env] GET_PREFERRED_HW_RENDER -> VULKAN");
            return true;

        case RETRO_ENVIRONMENT_SET_HW_RENDER: {
            struct retro_hw_render_callback* cb = (struct retro_hw_render_callback*)data;
            if (!cb) return false;
            vk.hwcb     = *cb;
            vk.haveHwcb = true;
            LOGI("[env] SET_HW_RENDER context_type=%d version=%u.%u (Vulkan=%d)",
                 cb->context_type, cb->version_major, cb->version_minor, RETRO_HW_CONTEXT_VULKAN);
            return true;
        }
        case RETRO_ENVIRONMENT_SET_HW_RENDER_CONTEXT_NEGOTIATION_INTERFACE: {
            const struct retro_hw_render_context_negotiation_interface* base =
                (const struct retro_hw_render_context_negotiation_interface*)data;
            if (!base) return false;
            LOGI("[env] SET_HW_RENDER_CONTEXT_NEGOTIATION_INTERFACE type=%d version=%u",
                 base->interface_type, base->interface_version);
            if (base->interface_type == RETRO_HW_RENDER_CONTEXT_NEGOTIATION_INTERFACE_VULKAN) {
                vk.nego     = *(const struct retro_hw_render_context_negotiation_interface_vulkan*)data;
                vk.haveNego = true;
            }
            return true;
        }
        case RETRO_ENVIRONMENT_GET_HW_RENDER_INTERFACE:
            if (!vk.contextReady) { LOGE("[env] GET_HW_RENDER_INTERFACE before context ready"); return false; }
            *(const struct retro_hw_render_interface**)data =
                (const struct retro_hw_render_interface*)&vk.iface;
            LOGI("[env] GET_HW_RENDER_INTERFACE -> provided");
            return true;

        case RETRO_ENVIRONMENT_GET_SYSTEM_DIRECTORY:
            if (data) *(const char**)data = s_systemDir[0] ? s_systemDir : nullptr;
            LOGI("[env] GET_SYSTEM_DIRECTORY -> '%s'", s_systemDir);
            return s_systemDir[0] != 0;
        case RETRO_ENVIRONMENT_GET_SAVE_DIRECTORY:
            if (data) *(const char**)data = s_saveDir[0] ? s_saveDir : nullptr;
            LOGI("[env] GET_SAVE_DIRECTORY -> '%s'", s_saveDir);
            return s_saveDir[0] != 0;

        case RETRO_ENVIRONMENT_GET_VARIABLE: {
            struct retro_variable* v = (struct retro_variable*)data;
            if (!v || !v->key) return false;
            auto it = s_coreOptions.find(v->key);
            if (it != s_coreOptions.end()) {
                // c_str() stays valid: the string lives in the static map until reload/shutdown,
                // and the core reads it synchronously here.
                v->value = it->second.c_str();
                LOGI("[env] GET_VARIABLE key='%s' -> '%s'", v->key, v->value);
                return true;
            }
            LOGI("[env] GET_VARIABLE key='%s' (no override → core default)", v->key);
            return false;
        }
        case RETRO_ENVIRONMENT_SET_VARIABLES:
        case RETRO_ENVIRONMENT_SET_CORE_OPTIONS:
        case RETRO_ENVIRONMENT_SET_CORE_OPTIONS_V2:
        case RETRO_ENVIRONMENT_SET_CORE_OPTIONS_INTL:
        case RETRO_ENVIRONMENT_SET_CORE_OPTIONS_V2_INTL:
            return true;

        default: {
            // Log each distinct unhandled cmd ONCE — some (e.g. the per-frame polls above) would
            // otherwise spam logcat every frame. cmd & 0xffff strips the EXPERIMENTAL bit, giving
            // the canonical (small) command number used as the index.
            static bool loggedCmd[1024] = { false };
            unsigned idx = cmd & 0xffff;
            if (idx < 1024 && !loggedCmd[idx]) {
                loggedCmd[idx] = true;
                LOGI("[env] unhandled cmd %u (0x%x) [logged once]", idx, cmd);
            }
            return false;
        }
    }
}

void   RETRO_CALLCONV video_refresh_cb(const void* data, unsigned w, unsigned h, size_t /*pitch*/)
{
    static uint64_t vc = 0; vc++;
    if (w > 0 && h > 0) { frameW = (int)w; frameH = (int)h; }
    bool hw = (data == RETRO_HW_FRAME_BUFFER_VALID);
    if (vc <= 3 || (vc % 120) == 0)
        LOGI("[video] refresh #%llu %ux%u hw=%d setImageCount=%llu",
             (unsigned long long)vc, w, h, hw, (unsigned long long)vk.imageCount);
}
void     RETRO_CALLCONV audio_sample_cb(int16_t, int16_t)
{
    static bool logged = false;
    if (!logged) { logged = true; LOGI("[audio] core uses the SINGLE-sample callback (we only buffer the batch one!)"); }
}
size_t   RETRO_CALLCONV audio_sample_batch_cb(const int16_t* data, size_t frames)
{
    if (!data || frames < 1) return frames;
    s_prodFrames.fetch_add(frames, std::memory_order_relaxed);   // guest-speed ground truth (watcher logs Δ/s)
    double inRate  = (s_sampleRate > 0.0) ? s_sampleRate : (double)s_audioOutRate;
    double outRate = (double)s_audioOutRate;

    // Boot governor (see declaration): block until produced audio no longer leads real time.
    {
        int64_t now = mono_ms();
        uint64_t total = s_prodFrames.load(std::memory_order_relaxed);
        if (s_govStartMs < 0 || s_govResync.exchange(false)) {
            s_govStartMs = now;
            s_govBaseFrames = total - frames;   // this batch counts inside the fresh window
        }
        double sr = (inRate > 1.0) ? inRate : 44100.0;
        double aheadMs = (double)(total - s_govBaseFrames) * 1000.0 / sr - (double)(now - s_govStartMs);
        const double headroomMs = 100.0;
        if (s_governorOn && aheadMs > headroomMs) {
            int64_t sleepMs = (int64_t)(aheadMs - headroomMs);
            if (sleepMs > 2000) sleepMs = 2000;   // sanity clamp (a monster batch is ~400 ms)
            static uint64_t govN = 0;
            if (govN < 8 || (govN % 200) == 0)
                LOGI("[gov] guest audio %.0f ms ahead of real time — sleeping %lld ms (batch=%zu frames)",
                     aheadMs, (long long)sleepMs, frames);
            govN++;
            while (sleepMs > 0 && !s_pumpStop) {          // chunked so shutdown can't hang on us
                int64_t c = sleepMs > 50 ? 50 : sleepMs;
                struct timespec ts { 0, (long)(c * 1000000) };
                nanosleep(&ts, nullptr);
                sleepMs -= c;
            }
        }
    }

    static uint64_t calls = 0, totFrames = 0;
    calls++; totFrames += frames;
    if (calls <= 3 || (calls % 300) == 0)
        LOGI("[audio] batch #%llu frames=%zu inRate=%.0f outRate=%d buffered=%d",
             (unsigned long long)calls, frames, inRate, s_audioOutRate, s_audioCount);

    // DEBUG: peak amplitude of the mixed S16 output the core actually produced (post-MVOL). This is
    // the ground truth for "is real audio being generated?": high+sustained = rich audio (music
    // present); low with only transient spikes = one-shot SFX only. Logged ~1/sec. Since SFX are
    // audible, the output path works — so a low peak here means the looping channels emit silence.
    {
        int pk = 0;
        for (size_t i = 0; i < frames * 2; ++i) { int a = data[i]; if (a < 0) a = -a; if (a > pk) pk = a; }
        static int peakWin = 0; static time_t lastPk = 0;
        if (pk > peakWin) peakWin = pk;
        time_t now = time(nullptr);
        if (now != lastPk) { lastPk = now; LOGI("[audio-peak] max|sample|=%d/32767 over ~1s", peakWin); peakWin = 0; }
    }

    std::unique_lock<std::mutex> lk(s_audioMutex);

    // Back-pressure: if we've already buffered ~s_backpressureMs of audio, block until Unity drains it
    // (or a timeout) — this paces retro_run/the emu by real playback, like RA's blocking audio driver.
    // Predicate also breaks on shutdown so pdlr_shutdown's join can't hang on a blocked producer.
    if (s_backpressureMs > 0) {
        int hiBp = (s_audioOutRate * 2 * s_backpressureMs) / 1000;   // target interleaved-stereo floats
        if (s_audioCount >= hiBp)
            s_audioSpaceCv.wait_for(lk, std::chrono::milliseconds(s_backpressureMs + 20),
                                    [&]{ return s_audioCount < hiBp || s_pumpStop; });
    }

    if (inRate == outRate) {
        for (size_t i = 0; i < frames; ++i) {
            if (s_audioCount + 2 > kAudioCap) break;          // overflow: drop the tail (Unity is behind)
            s_audio[s_audioCount++] = data[2*i]     / 32768.0f;
            s_audio[s_audioCount++] = data[2*i + 1] / 32768.0f;
        }
    } else {
        double ratio = inRate / outRate;                      // linear resample, like cwrapper/audio.c
        for (size_t out = 0; ; ++out) {
            double pos = out * ratio;
            size_t idx = (size_t)pos;
            if (idx >= frames || s_audioCount + 2 > kAudioCap) break;
            double frac = pos - (double)idx;
            size_t up = (idx == frames - 1) ? idx : idx + 1;
            s_audio[s_audioCount++] = (float)((1.0 - frac) * (data[2*idx]   / 32768.0) + frac * (data[2*up]   / 32768.0));
            s_audio[s_audioCount++] = (float)((1.0 - frac) * (data[2*idx+1] / 32768.0) + frac * (data[2*up+1] / 32768.0));
        }
    }

    // Bound latency / recover from a flood: keep at most ~100 ms of stereo buffered by dropping the
    // oldest samples. (Before the consumer engages, the buffer would otherwise peg at the cap.)
    int hi = (s_audioOutRate * 2) / 10;   // 100 ms of interleaved-stereo floats
    if (s_audioCount > hi) {
        int drop = s_audioCount - hi;
        memmove(s_audio, s_audio + drop, (size_t)(s_audioCount - drop) * sizeof(float));
        s_audioCount -= drop;
    }
    return frames;
}
void     RETRO_CALLCONV input_poll_cb(void) {}

// --- DEBUG input state (set from C# via pdlr_set_input; read by the core each retro_run) ---------
// Single port (0). Same thread as retro_run (Unity main thread sets then runs), so plain volatile
// is enough — no locking. Temporary test scaffolding; the real control layer lives elsewhere.
volatile uint32_t s_buttons  = 0;   // bit N == RETRO_DEVICE_ID_JOYPAD_N
volatile int16_t  s_analogLX = 0;
volatile int16_t  s_analogLY = 0;
volatile int16_t  s_analogRX = 0;   // right analog stick X (twin-stick cores, e.g. m2-vk Virtual-On); 0 for Flycast
volatile int16_t  s_analogRY = 0;   // right analog stick Y
volatile int16_t  s_triggerL = 0;   // Dreamcast left  analog trigger (L2), [0, 0x7fff]
volatile int16_t  s_triggerR = 0;   // Dreamcast right analog trigger (R2), [0, 0x7fff]

// Pipelined blit fence (see blit_frame). ON only for the m2-vk Model 1/2 core, whose heavier GPU load
// benefits from overlapping the blit-completion wait with CPU emulation. OFF for Flycast and every
// other core — they keep the proven immediate-wait blit, byte-for-byte the shipping field path. Keyed
// off an "m2" core-name test at load. (A former sibling flag, s_arcadeRemap — the debug-quad
// Y→coin/A→accel/B→brake synthesis — was removed once C# began sending honest positional bits on
// both the cabinet and quad paths; it double-mapped them. This blit flag is real and independent, so
// it stayed.) Set at load.
bool s_pipelineBlit = false;

// Frame orientation correction (see blit_frame). ON only for the m2-vk Model 1/2 core, which writes
// its framebuffer 180° from Flycast's canonical orientation (mirrored + upside down on the quad).
// We correct it in the blit by reversing the source offsets on both axes — free, since m2's
// B8G8R8A8 already takes the vkCmdBlitImage branch — so the core stays intact and the whole
// downstream crop/shader convention is unchanged. Flycast (R8G8B8A8 copy path) never enters there.
// Set at load; keyed off the same "m2" core-name test as s_pipelineBlit.
bool s_flip180 = false;

// Native-sized zero-copy buffer (see blit_frame). ON only for the m2-vk Model 1/2 core, whose boards
// run at a fixed native resolution per game (Model 2 496x384, System 22 640x480, System 21 496x480).
// We allocate the AHB buffer at that native frame instead of the fixed kCeilW/kCeilH ceiling, so the
// frame FILLS the buffer: no black margin, no UV crop, and the screen shader samples native pixels
// (the CRT material ignores the _MainTex_ST crop, so a sub-buffer frame would otherwise sit in a
// corner). OFF for Flycast and every other core — DC titles change resolution mid-run (Soul Calibur
// 640x239 boot -> 640x480), which the fixed ceiling + per-frame crop is built to absorb; the one-shot
// AHB import here cannot resize. So this is not merely additive-safe for Flycast, it is correct only
// for fixed-resolution boards. Set at load; keyed off the same "m2" core-name test as s_flip180.
bool s_nativeBuffer = false;

// Per-port maple device, applied at content load (pdlr_set_port_device, pre-start). Default: 4
// JOYPADs — the RetroArch maple parity that gates NAOMI audio init; a gun cabinet flips port 0.
unsigned s_portDevice[4] = { RETRO_DEVICE_JOYPAD, RETRO_DEVICE_JOYPAD, RETRO_DEVICE_JOYPAD, RETRO_DEVICE_JOYPAD };

// Light-gun state, port 0 (set from C# via pdlr_set_lightgun; read by the core each retro_run).
// Coords are libretro virtual screen space [-0x7fff, 0x7fff]; buttons bit N == RETRO_DEVICE_ID_LIGHTGUN_N.
volatile int16_t  s_lgX = -0x7fff;
volatile int16_t  s_lgY = -0x7fff;
volatile int      s_lgOffscreen = 1;
volatile uint32_t s_lgButtons = 0;

int16_t  RETRO_CALLCONV input_state_cb(unsigned port, unsigned device, unsigned index, unsigned id)
{
    if (port != 0) return 0;
    if (device == RETRO_DEVICE_JOYPAD) {
        // Honest positional RetroPad. Every core (Flycast, m2-vk) maps these ids to per-game controls
        // itself; the frontend forwards the bits as C# sends them. (A debug-quad arcade remap once
        // synthesized coin/accel/brake here for the Model2 core when the quad sent only A/B/X/Y — it
        // was removed once C# began sending positional coin/accel/brake, which it double-mapped.)
        uint32_t bits = s_buttons;
        if (id == RETRO_DEVICE_ID_JOYPAD_MASK) return (int16_t)bits;  // bulk query
        return (bits >> id) & 1u;
    }
    if (device == RETRO_DEVICE_ANALOG && index == RETRO_DEVICE_INDEX_ANALOG_LEFT) {
        if (id == RETRO_DEVICE_ID_ANALOG_X) return s_analogLX;
        if (id == RETRO_DEVICE_ID_ANALOG_Y) return s_analogLY;
    }
    // Right analog stick — twin-stick cores (m2-vk Virtual-On). Stays 0 for Flycast, which only ever
    // pushes the left stick through pdlr_set_input, so this returns the same 0 the old fall-through did.
    if (device == RETRO_DEVICE_ANALOG && index == RETRO_DEVICE_INDEX_ANALOG_RIGHT) {
        if (id == RETRO_DEVICE_ID_ANALOG_X) return s_analogRX;
        if (id == RETRO_DEVICE_ID_ANALOG_Y) return s_analogRY;
    }
    // Dreamcast analog triggers (racing games). Flycast reads L2/R2 as ANALOG_BUTTON and *2s them
    // to the 0..0xffff trigger range; a value of 0 makes it fall back to the digital L2/R2 bit.
    if (device == RETRO_DEVICE_ANALOG && index == RETRO_DEVICE_INDEX_ANALOG_BUTTON) {
        if (id == RETRO_DEVICE_ID_JOYPAD_L2) return s_triggerL;
        if (id == RETRO_DEVICE_ID_JOYPAD_R2) return s_triggerR;
    }
    if (device == RETRO_DEVICE_LIGHTGUN) {
        switch (id) {
            case RETRO_DEVICE_ID_LIGHTGUN_SCREEN_X:     return s_lgX;
            case RETRO_DEVICE_ID_LIGHTGUN_SCREEN_Y:     return s_lgY;
            case RETRO_DEVICE_ID_LIGHTGUN_IS_OFFSCREEN: return s_lgOffscreen ? 1 : 0;
            default: return (id < 32) ? (int16_t)((s_lgButtons >> id) & 1u) : (int16_t)0;
        }
    }
    return 0;
}

// dlopen the core + bind retro_* + read system info. Shared by probe and start.
bool load_and_bind(const char* core_path)
{
    if (g.handle) return true;
    if (!core_path || !*core_path) { SET_ERR("load_and_bind: null/empty core_path"); return false; }

    // Flycast resolves ASharedMemory_create (modern Android shared memory, API 26+) as a WEAK
    // symbol. If it's null when the core loads, Flycast falls back to legacy /dev/ashmem, which
    // SELinux blocks on API 29+ (EACCES) → nvmem disabled → fastmem crash. RetroArch's loader keeps
    // libandroid in the core's resolution scope; we must too. Load it GLOBALLY *before* the core so
    // the core's weak ASharedMemory_create binds to it → nvmem works → dynarec/fastmem at full speed.
    void* la = dlopen("libandroid.so", RTLD_NOW | RTLD_GLOBAL);
    void* ashmemFn = la ? dlsym(la, "ASharedMemory_create") : nullptr;
    LOGI("load_and_bind: preload libandroid=%p ASharedMemory_create=%p", la, ashmemFn);

    g.handle = dlopen(core_path, RTLD_NOW | RTLD_LOCAL);
    if (!g.handle) { SET_ERR("load_and_bind: dlopen('%s') FAILED: %s", core_path, dlerror()); return false; }
    LOGI("load_and_bind: dlopen OK (%p)", g.handle);

    bool ok = true;
    #define BIND(field, type) do {                                                              \
        g.field = (type)dlsym(g.handle, #field);                                                \
        if (!g.field) { LOGE("load_and_bind: dlsym %s FAILED: %s", #field, dlerror()); ok = false; } \
    } while (0)
    BIND(retro_set_environment,        fp_set_environment);
    BIND(retro_set_video_refresh,      fp_set_video_refresh);
    BIND(retro_set_audio_sample,       fp_set_audio_sample);
    BIND(retro_set_audio_sample_batch, fp_set_audio_sample_batch);
    BIND(retro_set_input_poll,         fp_set_input_poll);
    BIND(retro_set_input_state,        fp_set_input_state);
    BIND(retro_init,                   fp_init);
    BIND(retro_deinit,                 fp_deinit);
    BIND(retro_api_version,            fp_api_version);
    BIND(retro_get_system_info,        fp_get_system_info);
    BIND(retro_get_system_av_info,     fp_get_system_av_info);
    BIND(retro_run,                    fp_run);
    BIND(retro_load_game,              fp_load_game);
    BIND(retro_unload_game,            fp_unload_game);
    BIND(retro_set_controller_port_device, fp_set_controller_port_device);
    #undef BIND
    // Savestate entry points (boot-race transplant). Optional — warn but don't fail if a core lacks them.
    g.retro_serialize_size = (fp_serialize_size)dlsym(g.handle, "retro_serialize_size");
    g.retro_serialize      = (fp_serialize)     dlsym(g.handle, "retro_serialize");
    g.retro_unserialize    = (fp_unserialize)   dlsym(g.handle, "retro_unserialize");
    if (!g.retro_serialize_size || !g.retro_serialize || !g.retro_unserialize)
        LOGE("load_and_bind: savestate fns missing (size=%p ser=%p unser=%p) — transplant disabled",
             (void*)g.retro_serialize_size, (void*)g.retro_serialize, (void*)g.retro_unserialize);
    if (!ok) { SET_ERR("load_and_bind: '%s' is missing required retro_* symbols", core_path); return false; }

    struct retro_system_info info; memset(&info, 0, sizeof(info));
    g.retro_get_system_info(&info);
    if (info.library_name)    snprintf(g.name,    sizeof(g.name),    "%s", info.library_name);
    if (info.library_version) snprintf(g.version, sizeof(g.version), "%s", info.library_version);
    s_pipelineBlit = (strstr(g.name, "m2") != nullptr);  // m2-vk → pipelined blit fence; others immediate-wait
    s_flip180      = (strstr(g.name, "m2") != nullptr);  // m2-vk writes 180° from canonical; corrected in blit_frame
    s_nativeBuffer = (strstr(g.name, "m2") != nullptr);  // m2-vk boards are fixed-res → native-sized AHB, no crop
    LOGI("load_and_bind: core='%s' v'%s' api=%u exts='%s' need_fullpath=%d",
         g.name, g.version, g.retro_api_version(), info.valid_extensions ? info.valid_extensions : "",
         info.need_fullpath);
    return true;
}

void install_callbacks()
{
    g.retro_set_environment(environment_cb);
    g.retro_set_video_refresh(video_refresh_cb);
    g.retro_set_audio_sample(audio_sample_cb);
    g.retro_set_audio_sample_batch(audio_sample_batch_cb);
    g.retro_set_input_poll(input_poll_cb);
    g.retro_set_input_state(input_state_cb);
}

// Lazily (re)create the host-visible read-back buffer + command resources on the core's device.
bool ensure_readback(int w, int h)
{
    if (rbBuf != VK_NULL_HANDLE && rbW == w && rbH == h) return true;
    if (rbBuf != VK_NULL_HANDLE) {
        vkDeviceWaitIdle(vk.device);
        vkDestroyBuffer(vk.device, rbBuf, nullptr); rbBuf = VK_NULL_HANDLE;
        vkFreeMemory(vk.device, rbMem, nullptr);    rbMem = VK_NULL_HANDLE; rbMapped = nullptr;
    }
    VkDeviceSize size = (VkDeviceSize)w * h * 4;
    VkBufferCreateInfo bci{ VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO };
    bci.size = size; bci.usage = VK_BUFFER_USAGE_TRANSFER_DST_BIT; bci.sharingMode = VK_SHARING_MODE_EXCLUSIVE;
    if (vkCreateBuffer(vk.device, &bci, nullptr, &rbBuf) != VK_SUCCESS) { LOGE("[rb] vkCreateBuffer failed"); return false; }
    VkMemoryRequirements mr; vkGetBufferMemoryRequirements(vk.device, rbBuf, &mr);
    uint32_t mt = findMemType(vk.phys, mr.memoryTypeBits,
                              VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_COHERENT_BIT);
    if (mt == UINT32_MAX) { LOGE("[rb] no host-visible coherent memtype"); return false; }
    VkMemoryAllocateInfo mai{ VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO };
    mai.allocationSize = mr.size; mai.memoryTypeIndex = mt;
    if (vkAllocateMemory(vk.device, &mai, nullptr, &rbMem) != VK_SUCCESS) { LOGE("[rb] vkAllocateMemory failed"); return false; }
    vkBindBufferMemory(vk.device, rbBuf, rbMem, 0);
    vkMapMemory(vk.device, rbMem, 0, size, 0, &rbMapped);
    rbW = w; rbH = h;

    if (rbPool == VK_NULL_HANDLE) {
        VkCommandPoolCreateInfo pci{ VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO };
        pci.flags = VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT;
        pci.queueFamilyIndex = vk.queueFamily;
        vkCreateCommandPool(vk.device, &pci, nullptr, &rbPool);
        VkCommandBufferAllocateInfo cai{ VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO };
        cai.commandPool = rbPool; cai.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY; cai.commandBufferCount = 1;
        vkAllocateCommandBuffers(vk.device, &cai, &rbCmd);
        VkFenceCreateInfo fci{ VK_STRUCTURE_TYPE_FENCE_CREATE_INFO };
        vkCreateFence(vk.device, &fci, nullptr, &rbFence);
    }
    LOGI("[rb] read-back buffer ready %dx%d (%llu bytes)", w, h, (unsigned long long)size);
    return true;
}

// Copy the core's latest set_image into the host buffer. Coarse-synced (fence wait); fine for now.
void readback_frame()
{
    if (!vk.contextReady || !vk.haveImage || frameW <= 0 || frameH <= 0) return;
    if (vk.imageCount == lastRbImageCount && rbReady) return;   // no new core frame — buffer current
    VkImage src = vk.lastImage.create_info.image;
    if (src == VK_NULL_HANDLE) return;
    if (!ensure_readback(frameW, frameH)) return;

    vkResetCommandBuffer(rbCmd, 0);
    VkCommandBufferBeginInfo bbi{ VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO };
    bbi.flags = VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT;
    vkBeginCommandBuffer(rbCmd, &bbi);

    VkImageMemoryBarrier b{ VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER };
    b.srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
    b.dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
    b.image = src;
    b.subresourceRange = { VK_IMAGE_ASPECT_COLOR_BIT, 0, 1, 0, 1 };
    // core's layout -> TRANSFER_SRC
    b.oldLayout = vk.lastImage.image_layout; b.newLayout = VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL;
    b.srcAccessMask = VK_ACCESS_SHADER_READ_BIT; b.dstAccessMask = VK_ACCESS_TRANSFER_READ_BIT;
    vkCmdPipelineBarrier(rbCmd, VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT, VK_PIPELINE_STAGE_TRANSFER_BIT,
                         0, 0, nullptr, 0, nullptr, 1, &b);

    VkBufferImageCopy region{};
    region.imageSubresource = { VK_IMAGE_ASPECT_COLOR_BIT, 0, 0, 1 };
    region.imageExtent = { (uint32_t)frameW, (uint32_t)frameH, 1 };
    vkCmdCopyImageToBuffer(rbCmd, src, VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL, rbBuf, 1, &region);

    // TRANSFER_SRC -> back to the core's layout
    b.oldLayout = VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL; b.newLayout = vk.lastImage.image_layout;
    b.srcAccessMask = VK_ACCESS_TRANSFER_READ_BIT; b.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;
    vkCmdPipelineBarrier(rbCmd, VK_PIPELINE_STAGE_TRANSFER_BIT, VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT,
                         0, 0, nullptr, 0, nullptr, 1, &b);
    vkEndCommandBuffer(rbCmd);

    VkSubmitInfo si{ VK_STRUCTURE_TYPE_SUBMIT_INFO };
    si.commandBufferCount = 1; si.pCommandBuffers = &rbCmd;
    vk.queueMutex.lock();
    vkResetFences(vk.device, 1, &rbFence);
    VkResult r = vkQueueSubmit(vk.queue, 1, &si, rbFence);
    vk.queueMutex.unlock();
    if (r != VK_SUCCESS) { if ((rbErr++ % 120) == 0) LOGE("[rb] vkQueueSubmit failed %d", r); return; }
    vkWaitForFences(vk.device, 1, &rbFence, VK_TRUE, UINT64_MAX);
    rbReady = true;
    lastRbImageCount = vk.imageCount;
}

// ---- 2c zero-copy: AHB-backed shared image + per-frame blit (core device) ----------------------

// Lazily allocate the kNumBuf AHB-backed images on the core's device, sized to the frame. Each is
// later imported onto Unity's device (doUnityImport). R8G8B8A8 with TRANSFER_DST (blit target) +
// SAMPLED (Unity reads it) usage. Mirrors libpdvk createTarget Step A, ×N.
bool ensure_ahb_buffers(int w, int h)
{
    if (bufs[0].image != VK_NULL_HANDLE && ahbW == w && ahbH == h) return true;
    if (bufs[0].image != VK_NULL_HANDLE) {
        // Should never happen: callers pass constant dims per run — the fixed kCeilW/kCeilH ceiling, or
        // (s_nativeBuffer) a fixed-resolution board's own frame size — so w/h don't change after the
        // first alloc and this early-returns above. Kept as a guard: the import is one-shot; never
        // silently resize (a hypothetical mid-run native-res change would land here and keep the first).
        LOGE("[ahb] unexpected size change %dx%d -> %dx%d after alloc; keeping original", ahbW, ahbH, w, h);
        return true;
    }

    const VkFormat fmt = VK_FORMAT_R8G8B8A8_UNORM;   // byte order R,G,B,A == Unity RGBA32
    for (int i = 0; i < kNumBuf; ++i) {
        AhbBuf& b = bufs[i];
        AHardwareBuffer_Desc d{};
        d.width  = (uint32_t)w; d.height = (uint32_t)h; d.layers = 1;
        d.format = AHARDWAREBUFFER_FORMAT_R8G8B8A8_UNORM;
        d.usage  = AHARDWAREBUFFER_USAGE_GPU_COLOR_OUTPUT | AHARDWAREBUFFER_USAGE_GPU_SAMPLED_IMAGE;
        if (AHardwareBuffer_allocate(&d, &b.ahb) != 0 || !b.ahb) { LOGE("[ahb] allocate buf %d %dx%d FAILED", i, w, h); return false; }

        VkAndroidHardwareBufferFormatPropertiesANDROID afmt{
            VK_STRUCTURE_TYPE_ANDROID_HARDWARE_BUFFER_FORMAT_PROPERTIES_ANDROID };
        VkAndroidHardwareBufferPropertiesANDROID aprops{
            VK_STRUCTURE_TYPE_ANDROID_HARDWARE_BUFFER_PROPERTIES_ANDROID };
        aprops.pNext = &afmt;
        if (fpGetAhbProps(vk.device, b.ahb, &aprops) != VK_SUCCESS) { LOGE("[ahb] GetAhbProps buf %d failed", i); return false; }

        VkExternalMemoryImageCreateInfo ext{ VK_STRUCTURE_TYPE_EXTERNAL_MEMORY_IMAGE_CREATE_INFO };
        ext.handleTypes = VK_EXTERNAL_MEMORY_HANDLE_TYPE_ANDROID_HARDWARE_BUFFER_BIT_ANDROID;
        VkImageCreateInfo ici{ VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO };
        ici.pNext = &ext; ici.imageType = VK_IMAGE_TYPE_2D; ici.format = fmt;
        ici.extent = { (uint32_t)w, (uint32_t)h, 1 }; ici.mipLevels = 1; ici.arrayLayers = 1;
        ici.samples = VK_SAMPLE_COUNT_1_BIT; ici.tiling = VK_IMAGE_TILING_OPTIMAL;
        ici.usage = VK_IMAGE_USAGE_TRANSFER_DST_BIT | VK_IMAGE_USAGE_SAMPLED_BIT;
        ici.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
        if (vkCreateImage(vk.device, &ici, nullptr, &b.image) != VK_SUCCESS) { LOGE("[ahb] vkCreateImage buf %d failed", i); return false; }

        uint32_t mt = findMemType(vk.phys, aprops.memoryTypeBits, VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT);
        if (mt == UINT32_MAX) mt = findMemType(vk.phys, aprops.memoryTypeBits, 0);
        if (mt == UINT32_MAX) { LOGE("[ahb] no memtype buf %d (bits=0x%x)", i, aprops.memoryTypeBits); return false; }

        VkImportAndroidHardwareBufferInfoANDROID imp{ VK_STRUCTURE_TYPE_IMPORT_ANDROID_HARDWARE_BUFFER_INFO_ANDROID };
        imp.buffer = b.ahb;
        VkMemoryDedicatedAllocateInfo ded{ VK_STRUCTURE_TYPE_MEMORY_DEDICATED_ALLOCATE_INFO };
        ded.pNext = &imp; ded.image = b.image;
        VkMemoryAllocateInfo mai{ VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO };
        mai.pNext = &ded; mai.allocationSize = aprops.allocationSize; mai.memoryTypeIndex = mt;
        if (vkAllocateMemory(vk.device, &mai, nullptr, &b.mem) != VK_SUCCESS) { LOGE("[ahb] vkAllocateMemory buf %d failed", i); return false; }
        vkBindImageMemory(vk.device, b.image, b.mem, 0);
        b.firstUse = true;
    }

    if (blitPool == VK_NULL_HANDLE) {   // shared blit resources (one set; blit is fence-serialized)
        VkCommandPoolCreateInfo pci{ VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO };
        pci.flags = VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT; pci.queueFamilyIndex = vk.queueFamily;
        vkCreateCommandPool(vk.device, &pci, nullptr, &blitPool);
        VkCommandBufferAllocateInfo cai{ VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO };
        cai.commandPool = blitPool; cai.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY; cai.commandBufferCount = 1;
        vkAllocateCommandBuffers(vk.device, &cai, &blitCmd);
        VkFenceCreateInfo fci{ VK_STRUCTURE_TYPE_FENCE_CREATE_INFO };
        vkCreateFence(vk.device, &fci, nullptr, &blitFence);
    }
    ahbW = w; ahbH = h; writeIdx = 0; readyIdx = -1;
    LOGI("[ahb] %d shared buffers ready %dx%d", kNumBuf, w, h);
    return true;
}

// Copy the core's latest set_image frame into the next AHB buffer (one on-GPU copy), then publish
// it as the ready buffer. Triple-buffered so Unity never samples the buffer we're writing. Option 2:
// keep the blit-completion fence (the buffer is complete before we publish it) → ~0 added latency.
void blit_frame()
{
    if (!vk.contextReady || !vk.haveImage || frameW <= 0 || frameH <= 0) return;

    // Pipelined fence (m2-vk only, gated by s_pipelineBlit): retire the PREVIOUS blit here rather than
    // immediately after submitting it. Between that submit and now the pump ran retro_run + display-
    // pacing, during which the GPU drained the blit — so this wait is normally ~0, and the fence stall
    // Model2's heavier GPU load used to add to the critical path (retro_run + blit serialized → sub-
    // realtime → audio underrun) is now overlapped with CPU emulation. Publishing moves here too: C#
    // sees the buffer one display frame later, fully rendered. Flycast and every other core skip this
    // block entirely (they take the immediate-wait path below). Shutdown's vkDeviceWaitIdle drains any
    // still-pending blit safely.
    if (s_pipelineBlit && blitPending) {
        vkWaitForFences(vk.device, 1, &blitFence, VK_TRUE, UINT64_MAX);
        blitPending = false;
        bufs[blitPendingIdx].firstUse = false;
        readyIdx = blitPendingIdx;                 // publish the now-complete buffer
    }

    if (vk.imageCount == lastBlitImageCount) return;   // no new core frame — ready buffer still current
    VkImage src = vk.lastImage.create_info.image;
    if (src == VK_NULL_HANDLE) return;
    // Buffer sizing. Two regimes:
    //  - Fixed ceiling (Flycast, default): the AHB is kCeilW×kCeilH; the active frame may be smaller and
    //    may change over the run, so it is copied into a corner and C# UV-crops to it.
    //  - Native (s_nativeBuffer, m2-vk): the AHB is the game's own fixed native frame, so the copy fills
    //    it edge to edge — no margin, no crop, native pixels straight to the screen shader.
    // ensure_ahb_buffers is one-shot; both regimes pass constant dims per run (kCeil is constant; a
    // fixed-res board's frameW/frameH is too), so it early-returns after the first allocation.
    const int bufW = s_nativeBuffer ? frameW : kCeilW;
    const int bufH = s_nativeBuffer ? frameH : kCeilH;
    if (!ensure_ahb_buffers(bufW, bufH)) return;
    const uint32_t copyW = (uint32_t)((frameW < bufW) ? frameW : bufW);
    const uint32_t copyH = (uint32_t)((frameH < bufH) ? frameH : bufH);

    AhbBuf& b   = bufs[writeIdx];
    VkImage dst = b.image;

    vkResetCommandBuffer(blitCmd, 0);
    VkCommandBufferBeginInfo bbi{ VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO };
    bbi.flags = VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT;
    vkBeginCommandBuffer(blitCmd, &bbi);

    VkImageSubresourceRange range{ VK_IMAGE_ASPECT_COLOR_BIT, 0, 1, 0, 1 };

    // core image: its layout -> TRANSFER_SRC
    VkImageMemoryBarrier sb{ VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER };
    sb.srcQueueFamilyIndex = sb.dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
    sb.image = src; sb.subresourceRange = range;
    sb.oldLayout = vk.lastImage.image_layout; sb.newLayout = VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL;
    sb.srcAccessMask = VK_ACCESS_SHADER_READ_BIT; sb.dstAccessMask = VK_ACCESS_TRANSFER_READ_BIT;

    // dst buffer: UNDEFINED (first use) or SHADER_READ (steady state) -> TRANSFER_DST
    VkImageMemoryBarrier db{ VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER };
    db.srcQueueFamilyIndex = db.dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
    db.image = dst; db.subresourceRange = range;
    db.oldLayout = b.firstUse ? VK_IMAGE_LAYOUT_UNDEFINED : VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
    db.newLayout = VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL;
    db.srcAccessMask = 0; db.dstAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;
    VkImageMemoryBarrier pre[2] = { sb, db };
    vkCmdPipelineBarrier(blitCmd, VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT, VK_PIPELINE_STAGE_TRANSFER_BIT,
                         0, 0, nullptr, 0, nullptr, 2, pre);

    // The AHB is R8G8B8A8_UNORM (== Unity RGBA32). vkCmdCopyImage is a RAW byte copy — it does NOT
    // convert component order, so a core whose frame is a different channel order (Modelizer delivers
    // B8G8R8A8_UNORM) would land R/B-swapped ("blue is orange"). When the core's format already
    // matches the AHB (Flycast delivers R8G8B8A8) the copy is identity and we keep it (proven path,
    // zero conversion cost). When it differs we blit instead: vkCmdBlitImage converts by LOGICAL
    // component (blue stays blue), and both formats support blit-src/blit-dst on Adreno. NEAREST at
    // 1:1 so there is no scaling/filtering — a converting copy in all but name.
    //
    // BOTTOM-left, not top-left, in both paths: C#'s UV crop (ApplyZeroCopyCrop) samples the V band
    // [1-av, 1] and the external texture samples V UNflipped (Soul Calibur 640x239 boot showed the
    // stale bottom half of the previous full frame; HotD2's 479-line frames left a never-written
    // white row at the bottom). Placing the active band at the bottom makes the written band and the
    // sampled band coincide; identity for full-height frames.
    if (vk.lastImage.create_info.format == VK_FORMAT_R8G8B8A8_UNORM) {
        VkImageCopy region{};
        region.srcSubresource = { VK_IMAGE_ASPECT_COLOR_BIT, 0, 0, 1 };
        region.dstSubresource = { VK_IMAGE_ASPECT_COLOR_BIT, 0, 0, 1 };
        region.dstOffset = { 0, (int32_t)(ahbH - (int)copyH), 0 };
        region.extent = { copyW, copyH, 1 };
        vkCmdCopyImage(blitCmd, src, VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,
                       dst, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, 1, &region);
    } else {
        VkImageBlit region{};
        region.srcSubresource = { VK_IMAGE_ASPECT_COLOR_BIT, 0, 0, 1 };
        region.dstSubresource = { VK_IMAGE_ASPECT_COLOR_BIT, 0, 0, 1 };
        // s_flip180 (m2-vk): reverse the source X and Y so dst top-left ← src bottom-right — a 180°
        // flip that lands m2's mirrored+upside-down frame in Flycast's canonical orientation. The dst
        // band is untouched, so C#'s UV crop convention is unaffected. Off for every other core.
        if (s_flip180) {
            region.srcOffsets[0] = { (int32_t)copyW, (int32_t)copyH, 0 };
            region.srcOffsets[1] = { 0, 0, 1 };
        } else {
            region.srcOffsets[0] = { 0, 0, 0 };
            region.srcOffsets[1] = { (int32_t)copyW, (int32_t)copyH, 1 };
        }
        region.dstOffsets[0] = { 0, (int32_t)(ahbH - (int)copyH), 0 };
        region.dstOffsets[1] = { (int32_t)copyW, (int32_t)ahbH, 1 };
        vkCmdBlitImage(blitCmd, src, VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,
                       dst, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, 1, &region, VK_FILTER_NEAREST);
    }

    // restore core image; dst -> SHADER_READ (the layout Unity's imported image expects)
    sb.oldLayout = VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL; sb.newLayout = vk.lastImage.image_layout;
    sb.srcAccessMask = VK_ACCESS_TRANSFER_READ_BIT; sb.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;
    db.oldLayout = VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL; db.newLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
    db.srcAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT; db.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;
    VkImageMemoryBarrier post[2] = { sb, db };
    vkCmdPipelineBarrier(blitCmd, VK_PIPELINE_STAGE_TRANSFER_BIT, VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT,
                         0, 0, nullptr, 0, nullptr, 2, post);
    vkEndCommandBuffer(blitCmd);

    VkSubmitInfo si{ VK_STRUCTURE_TYPE_SUBMIT_INFO };
    si.commandBufferCount = 1; si.pCommandBuffers = &blitCmd;
    vk.queueMutex.lock();
    vkResetFences(vk.device, 1, &blitFence);
    VkResult r = vkQueueSubmit(vk.queue, 1, &si, blitFence);
    vk.queueMutex.unlock();
    if (r != VK_SUCCESS) { if ((blitErr++ % 120) == 0) LOGE("[ahb] blit vkQueueSubmit failed %d", r); return; }
    if (s_pipelineBlit) {
        // m2-vk only: do NOT wait here — the fence is retired at the top of the NEXT call (pipelined).
        // This buffer's firstUse clear and readyIdx publish happen there, once the GPU has finished it.
        blitPending    = true;
        blitPendingIdx = writeIdx;
        writeIdx       = (writeIdx + 1) % kNumBuf;   // next blit targets a different buffer
    } else {
        // Flycast and every other core: the proven, shipping path — block on the blit, then publish
        // this buffer. Byte-for-byte the pre-pipelining behavior; nothing already in the field changes.
        vkWaitForFences(vk.device, 1, &blitFence, VK_TRUE, UINT64_MAX);   // buffer complete before publish
        b.firstUse = false;
        readyIdx   = writeIdx;                       // publish: C# displays this buffer
        writeIdx   = (writeIdx + 1) % kNumBuf;       // next blit targets a different buffer
    }
    lastBlitImageCount = vk.imageCount;
}

// ---- Step B: import the AHB onto Unity's VkDevice (ported from libpdvk) -------------------------

uint32_t findMemTypeOn(VkPhysicalDevice phys, uint32_t bits, VkMemoryPropertyFlags want)
{
    VkPhysicalDeviceMemoryProperties mp; vkGetPhysicalDeviceMemoryProperties(phys, &mp);
    for (uint32_t i = 0; i < mp.memoryTypeCount; ++i)
        if ((bits & (1u << i)) && (mp.memoryTypes[i].propertyFlags & want) == want) return i;
    return UINT32_MAX;
}

int doUnityImport()
{
    if (unityImagesReady)       { LOGI("[unity-import] already done"); return 0; }
    if (bufs[0].image == VK_NULL_HANDLE) { LOGE("[unity-import] AHB buffers not allocated yet"); return -1; }
    if (!s_uvk)                 { LOGE("[unity-import] no IUnityGraphicsVulkan — UnityPluginLoad never ran?"); return -1; }

    UnityVulkanInstance uvi = s_uvk->Instance();
    if (uvi.device == VK_NULL_HANDLE) { LOGE("[unity-import] Unity device is NULL"); return -1; }
    LOGI("[unity-import] Unity device=%p physDev=%p gfxQueue=%p qFamily=%u",
         (void*)uvi.device, (void*)uvi.physicalDevice, (void*)uvi.graphicsQueue, uvi.queueFamilyIndex);

    PFN_vkGetAndroidHardwareBufferPropertiesANDROID fp =
        (PFN_vkGetAndroidHardwareBufferPropertiesANDROID)
        vkGetDeviceProcAddr(uvi.device, "vkGetAndroidHardwareBufferPropertiesANDROID");
    if (!fp) { LOGE("[unity-import] Unity's device lacks the AHB extension — cannot import"); return -1; }

    const VkFormat fmt = VK_FORMAT_R8G8B8A8_UNORM;

    // One command buffer batches the UNDEFINED -> SHADER_READ_ONLY transition for all N images.
    VkCommandPool pool = VK_NULL_HANDLE;
    VkCommandPoolCreateInfo pci{ VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO };
    pci.queueFamilyIndex = uvi.queueFamilyIndex;
    vkCreateCommandPool(uvi.device, &pci, nullptr, &pool);
    VkCommandBuffer cb = VK_NULL_HANDLE;
    VkCommandBufferAllocateInfo cai{ VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO };
    cai.commandPool = pool; cai.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY; cai.commandBufferCount = 1;
    vkAllocateCommandBuffers(uvi.device, &cai, &cb);
    VkCommandBufferBeginInfo bbi{ VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO };
    bbi.flags = VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT;
    vkBeginCommandBuffer(cb, &bbi);

    for (int i = 0; i < kNumBuf; ++i) {
        AhbBuf& b = bufs[i];
        VkAndroidHardwareBufferFormatPropertiesANDROID afmt{
            VK_STRUCTURE_TYPE_ANDROID_HARDWARE_BUFFER_FORMAT_PROPERTIES_ANDROID };
        VkAndroidHardwareBufferPropertiesANDROID aprops{
            VK_STRUCTURE_TYPE_ANDROID_HARDWARE_BUFFER_PROPERTIES_ANDROID };
        aprops.pNext = &afmt;
        if (fp(uvi.device, b.ahb, &aprops) != VK_SUCCESS) { LOGE("[unity-import] GetAhbProps buf %d failed", i); vkDestroyCommandPool(uvi.device, pool, nullptr); return -1; }

        VkExternalMemoryImageCreateInfo ext{ VK_STRUCTURE_TYPE_EXTERNAL_MEMORY_IMAGE_CREATE_INFO };
        ext.handleTypes = VK_EXTERNAL_MEMORY_HANDLE_TYPE_ANDROID_HARDWARE_BUFFER_BIT_ANDROID;
        VkImageCreateInfo ici{ VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO };
        ici.pNext = &ext; ici.imageType = VK_IMAGE_TYPE_2D; ici.format = fmt;
        ici.extent = { (uint32_t)ahbW, (uint32_t)ahbH, 1 }; ici.mipLevels = 1; ici.arrayLayers = 1;
        ici.samples = VK_SAMPLE_COUNT_1_BIT; ici.tiling = VK_IMAGE_TILING_OPTIMAL;
        ici.usage = VK_IMAGE_USAGE_SAMPLED_BIT; ici.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
        if (vkCreateImage(uvi.device, &ici, nullptr, &b.unityImage) != VK_SUCCESS) { LOGE("[unity-import] vkCreateImage buf %d failed", i); vkDestroyCommandPool(uvi.device, pool, nullptr); return -1; }

        uint32_t mt = findMemTypeOn(uvi.physicalDevice, aprops.memoryTypeBits, VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT);
        if (mt == UINT32_MAX) mt = findMemTypeOn(uvi.physicalDevice, aprops.memoryTypeBits, 0);
        if (mt == UINT32_MAX) { LOGE("[unity-import] no memtype buf %d on Unity dev", i); vkDestroyCommandPool(uvi.device, pool, nullptr); return -1; }

        VkImportAndroidHardwareBufferInfoANDROID imp{ VK_STRUCTURE_TYPE_IMPORT_ANDROID_HARDWARE_BUFFER_INFO_ANDROID };
        imp.buffer = b.ahb;
        VkMemoryDedicatedAllocateInfo ded{ VK_STRUCTURE_TYPE_MEMORY_DEDICATED_ALLOCATE_INFO };
        ded.pNext = &imp; ded.image = b.unityImage;
        VkMemoryAllocateInfo mai{ VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO };
        mai.pNext = &ded; mai.allocationSize = aprops.allocationSize; mai.memoryTypeIndex = mt;
        if (vkAllocateMemory(uvi.device, &mai, nullptr, &b.unityMem) != VK_SUCCESS) { LOGE("[unity-import] vkAllocateMemory buf %d failed", i); vkDestroyCommandPool(uvi.device, pool, nullptr); return -1; }
        vkBindImageMemory(uvi.device, b.unityImage, b.unityMem, 0);

        VkImageMemoryBarrier bar{ VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER };
        bar.srcAccessMask = 0; bar.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;
        bar.oldLayout = VK_IMAGE_LAYOUT_UNDEFINED; bar.newLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
        bar.srcQueueFamilyIndex = bar.dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
        bar.image = b.unityImage; bar.subresourceRange = { VK_IMAGE_ASPECT_COLOR_BIT, 0, 1, 0, 1 };
        vkCmdPipelineBarrier(cb, VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT, VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT,
                             0, 0, nullptr, 0, nullptr, 1, &bar);
    }

    vkEndCommandBuffer(cb);
    VkSubmitInfo si{ VK_STRUCTURE_TYPE_SUBMIT_INFO };
    si.commandBufferCount = 1; si.pCommandBuffers = &cb;
    vkQueueSubmit(uvi.graphicsQueue, 1, &si, VK_NULL_HANDLE);
    vkQueueWaitIdle(uvi.graphicsQueue);
    vkDestroyCommandPool(uvi.device, pool, nullptr);

    unityImagesReady = true;
    LOGI("[unity-import] DONE — %d buffers imported onto Unity's device", kNumBuf);
    return 0;
}

void UNITY_INTERFACE_API OnRenderEvent(int eventId)
{
    if (eventId == PDLR_EVENT_IMPORT_AHB) {
        LOGI("[OnRenderEvent] import event received");
        if (doUnityImport() != 0) LOGE("[OnRenderEvent] doUnityImport FAILED");
    }
}

} // namespace

static void* pump_thread_fn(void*);    // dedicated retro_run loop (defined below pdlr_start)

// --- Savestate transplant (boot-race workaround) -------------------------------------------------
// VF3's ARM7 sound-driver init is a boot race: some boots come up SCIEB!=0 (music), some stay 0
// (silent) — nondeterministic, NOT controlled by pacing (the governor proved that). RetroArch boots
// this core to music 100% of the time. So capture one good post-boot state (retro_serialize) and
// restore it on every boot (retro_unserialize), skipping the racy init. Callers are single-threaded
// w.r.t. retro_run: pdlr_start before the pump starts, or the pump BETWEEN retro_run calls.
static bool do_save_state(const char* path)
{
    if (!g.retro_serialize_size || !g.retro_serialize) { LOGE("[state] serialize unavailable"); return false; }
    size_t n = g.retro_serialize_size();
    if (n == 0 || n > (64u << 20)) { LOGE("[state] implausible serialize size %zu", n); return false; }
    void* buf = malloc(n);
    if (!buf) { LOGE("[state] malloc %zu failed", n); return false; }
    bool ok = g.retro_serialize(buf, n);
    if (ok) {
        FILE* f = fopen(path, "wb");
        if (f) { size_t w = fwrite(buf, 1, n, f); fclose(f); ok = (w == n);
                 LOGI("[state] SAVED %zu bytes -> %s (%s)", n, path, ok ? "OK" : "SHORT WRITE"); }
        else { LOGE("[state] fopen('%s','wb') failed errno=%d", path, errno); ok = false; }
    } else LOGE("[state] retro_serialize returned false");
    free(buf);
    return ok;
}
static bool do_load_state(const char* path)
{
    if (!g.retro_unserialize) { LOGE("[state] unserialize unavailable"); return false; }
    FILE* f = fopen(path, "rb");
    if (!f) { LOGI("[state] no boot state at %s — normal (racy) boot", path); return false; }
    fseek(f, 0, SEEK_END); long sz = ftell(f); fseek(f, 0, SEEK_SET);
    if (sz <= 0 || sz > (64L << 20)) { LOGE("[state] bad state file size %ld", sz); fclose(f); return false; }
    void* buf = malloc((size_t)sz);
    if (!buf) { fclose(f); LOGE("[state] malloc failed"); return false; }
    size_t r = fread(buf, 1, (size_t)sz, f); fclose(f);
    bool ok = (r == (size_t)sz) && g.retro_unserialize(buf, (size_t)sz);
    LOGI("[state] LOAD %s (%ld bytes): %s", path, sz, ok ? "OK — boot state restored (driver pre-initialized)"
                                                          : "FAILED (core/version mismatch?) — falling through to normal boot");
    free(buf);
    return ok;
}

int pdlr_probe(const char* core_path)
{
    if (g.inited) { LOGI("pdlr_probe: already inited — no-op"); return 0; }
    LOGI("pdlr_probe begin: core='%s'  [build %s %s]", core_path ? core_path : "(null)", __DATE__, __TIME__);
    if (!load_and_bind(core_path)) { pdlr_shutdown(); return -1; }
    install_callbacks();
    LOGI("pdlr_probe: calling retro_init");
    g.retro_init();
    g.inited = true;
    LOGI("pdlr_probe OK — core '%s' %s loaded + initialized", g.name, g.version);
    return 0;
}

int pdlr_start(const char* core_path, const char* system_dir, const char* save_dir, const char* game_path)
{
    LOGI("pdlr_start begin: core='%s' sys='%s' save='%s' game='%s'  [build %s %s]",
         core_path ? core_path : "(null)", system_dir ? system_dir : "", save_dir ? save_dir : "",
         game_path ? game_path : "(null)", __DATE__, __TIME__);

    // Fresh diagnostics per boot — a reason left over from the previous attempt must never be
    // reported as this one's. Nothing clears these on the way out: pdlr_start may call
    // pdlr_shutdown() before returning -1, and the frontend reads the reason back afterwards.
    { std::lock_guard<std::mutex> lk(s_diagMx);
      s_diagRing.clear(); s_diagLastError.clear(); s_diagCoreError.clear(); s_diagCoreMsg.clear(); }

    if (!game_path || !*game_path) { SET_ERR("pdlr_start: null game_path"); return -1; }
    if (!load_and_bind(core_path)) { pdlr_shutdown(); return -1; }

    if (system_dir) snprintf(s_systemDir, sizeof(s_systemDir), "%s", system_dir);
    if (save_dir)   snprintf(s_saveDir,   sizeof(s_saveDir),   "%s", save_dir);

    // Load optional core-option overrides from <gameDir>/Flycast.opt, before retro_load_game (the
    // core queries GET_VARIABLE during load/update_variables). Falls back to systemDir, then defaults.
    {
        char dir[1024]; snprintf(dir, sizeof(dir), "%s", game_path);
        char* slash = strrchr(dir, '/');
        if (slash) *slash = 0;
        const char* optDir = slash ? dir : s_systemDir;
        snprintf(s_gameDir, sizeof(s_gameDir), "%s", optDir);   // for boot-state transplant files
        load_core_options(optDir);
        if (s_coreOptions.empty() && slash) load_core_options(s_systemDir);

        // Boot policy (shipped default): use the HLE (reios) BIOS unless a Flycast.opt overrides it,
        // so end users never have to supply a real Dreamcast dc_boot.bin. HLE boots straight into the
        // game and brings the AICA sound driver up correctly here; the real-BIOS path does not — a
        // frontend-specific failure (RetroArch's real-BIOS boot plays fine, verified 2026-07-03). A
        // per-cabinet Flycast.opt with reicast_hle_bios can still force either mode.
        if (s_coreOptions.find("reicast_hle_bios") == s_coreOptions.end()) {
            s_coreOptions["reicast_hle_bios"] = "enabled";
            LOGI("[opt] boot policy: reicast_hle_bios defaulted to 'enabled' (HLE) — no BIOS required");
        }

        // Per-cabinet overrides pushed from C# via pdlr_set_option (description.yaml `environment:`),
        // layered on top of the Flycast.opt defaults — YAML wins; options it doesn't name are left
        // at their default value. Applied last so it can also override the boot-policy default above.
        for (const auto& kv : s_optOverrides) {
            s_coreOptions[kv.first] = kv.second;
            LOGI("[opt] override %s = \"%s\" (from yaml environment)", kv.first.c_str(), kv.second.c_str());
        }

        // Audio back-pressure target in ms — knob <optDir>/backpressure.txt (a number 0..500; 0 = off).
        // Absent = built-in default. Lets us A/B the RA-style pacing on device without a rebuild.
        char fpath[1088]; snprintf(fpath, sizeof(fpath), "%s/backpressure.txt", optDir);
        FILE* bf = fopen(fpath, "r");
        bool backpressureKnobSet = false;
        if (bf) { int v = -1; if (fscanf(bf, "%d", &v) == 1 && v >= 0 && v <= 500) { s_backpressureMs = v; backpressureKnobSet = true; } fclose(bf); }
        // Governor toggle (A/B the boot race): governor.txt=0 disables the wall-clock producer sleeps.
        snprintf(fpath, sizeof(fpath), "%s/governor.txt", optDir);
        FILE* gf = fopen(fpath, "r");
        if (gf) { int v = -1; if (fscanf(gf, "%d", &v) == 1) s_governorOn = (v != 0); fclose(gf); }
        LOGI("[gov] governor %s (knob governor.txt)", s_governorOn ? "ON" : "OFF");
        // Display-lock toggle (A/B vs the old wall-clock pacer): displaylock.txt=0 disables it.
        snprintf(fpath, sizeof(fpath), "%s/displaylock.txt", optDir);
        FILE* pl = fopen(fpath, "r");
        if (pl) { int v = -1; if (fscanf(pl, "%d", &v) == 1) s_displayLock = (v != 0); fclose(pl); }
        // Display-lock is the pacer now, so back-pressure (which blocks INSIDE retro_run — its wait
        // counts as retro_run time) is redundant AND harmful: measured 2026-07-06, bp=48 inflated
        // retro_run 11-14ms and forced Flycast to frame-skip to 30fps in bursts; bp=0 dropped it to
        // ~1ms and made the output eye-confirmed smooth. So when display-lock is on, default
        // back-pressure OFF. An explicit backpressure.txt still wins (A/B); the legacy wall-clock
        // path (displaylock.txt=0) keeps the old back-pressure default, where it was the pacer.
        if (s_displayLock && !backpressureKnobSet) s_backpressureMs = 0;
        LOGI("[pace] display-lock %s (knob displaylock.txt), displayHz=%.2f, back-pressure=%dms%s",
             s_displayLock ? "ON" : "OFF", s_displayHz, s_backpressureMs,
             (s_displayLock && !backpressureKnobSet) ? " (auto-off: display-lock is the pacer)" : "");

        // How much of the core's log to capture into the ring the frontend drains — verbose.txt
        // holding 0=DEBUG 1=INFO 2=WARN 3=ERROR. Absent leaves whatever the frontend set via
        // pdlr_set_log_verbosity (default WARN+). A tester chasing a boot failure can drop a
        // verbose.txt next to the game and get the core's whole boot chatter in flycast.log, with
        // no rebuild — the same trick as the pacing knobs above.
        snprintf(fpath, sizeof(fpath), "%s/verbose.txt", optDir);
        FILE* vf = fopen(fpath, "r");
        if (vf) { int v = -1; if (fscanf(vf, "%d", &v) == 1) pdlr_set_log_verbosity(v); fclose(vf); }
        LOGI("[diag] core-log capture level=%d (0=DEBUG 1=INFO 2=WARN 3=ERROR), ring=%zu lines",
             s_diagMinLevel.load(), kDiagLines);
    }

    install_callbacks();

    // Flycast allocates its nvmem/fastmem backing in addrspace::reserve() during retro_init, using
    // a writable data path that the libretro shell doesn't configure until retro_load_game. In an
    // embedded (non-RetroArch) process the cwd is '/' (not writable), so the file-backed alloc fails
    // with EACCES, nvmem is disabled, and fastmem later segfaults. RetroArch's cwd is the app's
    // writable dir — which is why it works there. So point the process at a writable dir for the
    // init+load window, then restore Unity's cwd.
    char oldcwd[1024]; oldcwd[0] = 0;
    if (!getcwd(oldcwd, sizeof(oldcwd))) oldcwd[0] = 0;
    if (s_systemDir[0]) {
        setenv("HOME",   s_systemDir, 1);
        setenv("TMPDIR", s_systemDir, 1);
        if (chdir(s_systemDir) == 0) LOGI("pdlr_start: chdir to writable '%s' (for nvmem alloc)", s_systemDir);
        else                         LOGE("pdlr_start: chdir('%s') failed errno=%d", s_systemDir, errno);
    }

    if (!g.inited) { LOGI("pdlr_start: retro_init"); g.retro_init(); g.inited = true; }

    // Flycast reports need_fullpath=1: pass the path, not the bytes.
    struct retro_game_info gi; memset(&gi, 0, sizeof(gi));
    gi.path = game_path;
    LOGI("pdlr_start: retro_load_game('%s')", game_path);
    bool loaded = g.retro_load_game(&gi);
    if (oldcwd[0]) { chdir(oldcwd); LOGI("pdlr_start: restored cwd '%s'", oldcwd); }
    if (!loaded) {
        // The core already told us why, on its log callback and/or SET_MESSAGE — surface it instead
        // of the bare "FAILED" that used to be the only trace of a missing BIOS or a bad romset.
        std::string reason = diag_core_reason();
        if (!reason.empty()) SET_ERR("retro_load_game('%s') failed — the core said: %s", game_path, reason.c_str());
        else                 SET_ERR("retro_load_game('%s') failed and the core gave no reason", game_path);
        return -1;
    }
    g.gameLoaded = true;
    LOGI("pdlr_start: retro_load_game OK; haveHwcb=%d haveNego=%d", vk.haveHwcb, vk.haveNego);

    // Read the core's declared A/V timing (native video rate + audio sample rate). For Dreamcast
    // NTSC this is ~59.94 Hz — the rate the emulator should be ticked at (matters for audio sync).
    if (g.retro_get_system_av_info) {
        struct retro_system_av_info av; memset(&av, 0, sizeof(av));
        g.retro_get_system_av_info(&av);
        if (av.timing.fps > 0.0) { s_fps = av.timing.fps; s_sampleRate = av.timing.sample_rate; }
        LOGI("pdlr_start: av_info fps=%.4f sample_rate=%.1f base=%ux%u max=%ux%u",
             av.timing.fps, av.timing.sample_rate, av.geometry.base_width, av.geometry.base_height,
             av.geometry.max_width, av.geometry.max_height);
    }

    // Connect Dreamcast controllers on ALL 4 ports — exact RetroArch parity. RetroArch calls
    // retro_set_controller_port_device(port, JOYPAD) for EVERY core port at content load; with
    // no call at all Flycast builds an EMPTY maple bus (no controller/VMU/Purupuru), which we
    // proved gates the game's whole sound driver (channels never keyed). Port 0 alone fixed
    // channel key-on but MVOL/music stayed dead and the BIOS chime never keys channels either,
    // so ports 1-3 (previously NONE) are the last maple difference from RetroArch — it runs 4
    // pads with VMUs A1-D1. The per-port VMU/Purupuru expansions attach via update_variables()
    // from the fed reicast_device_portN_slotM options once all 4 ports are set.
    // NOTE: throwaway debug wiring, same scope as pdlr_set_input; the real AoJ control layer
    // folds in at Phase 6.
    if (g.retro_set_controller_port_device) {
        for (unsigned p = 0; p < 4; ++p)
            g.retro_set_controller_port_device(p, s_portDevice[p]);
        LOGI("pdlr_start: controller ports set (%u,%u,%u,%u) — full RetroArch maple parity",
             s_portDevice[0], s_portDevice[1], s_portDevice[2], s_portDevice[3]);
    }

    if (create_vulkan_context() != 0) { SET_ERR("pdlr_start: Vulkan context setup FAILED (see the [vk] lines in the captured log)"); return -1; }

    // Boot-race transplant: unless <gameDir>/no_state.txt exists, restore a captured good boot state
    // so the ARM7 sound driver comes up pre-initialized (SCIEB!=0 = music) instead of rolling the
    // init race. Done here, single-threaded, before the pump starts. A miss falls through to a normal
    // (racy) boot. no_state.txt lets us deliberately boot fresh (to capture a NEW good state).
    if (s_gameDir[0]) {
        char np[1088]; snprintf(np, sizeof(np), "%s/no_state.txt", s_gameDir);
        FILE* nf = fopen(np, "r");
        if (nf) { fclose(nf); LOGI("[state] no_state.txt present — skipping load, booting fresh (capture mode)"); }
        else { char sp[1088]; snprintf(sp, sizeof(sp), "%s/boot.state", s_gameDir); do_load_state(sp); }
    }

    // Start the dedicated emu pump (declaration near the top explains it).
    s_pumpStop = false; s_pumpPaused = false;
    if (pthread_create(&s_pumpThread, nullptr, pump_thread_fn, nullptr) == 0) s_pumpActive = true;
    else LOGE("pdlr_start: pump thread create FAILED — falling back to C#-driven pdlr_run ticks");

    LOGI("pdlr_start OK — Flycast running on its own Vulkan device (pump=%d)", (int)s_pumpActive);
    return 0;
}

static double ms_since(const struct timespec& a, const struct timespec& b)
{ return (b.tv_sec - a.tv_sec) * 1000.0 + (b.tv_nsec - a.tv_nsec) / 1e6; }

// Display-locked pacer (see s_displayFrame block up top). Blocks the pump until Unity's render
// cadence has granted one emulated frame of credit, then consumes it — producing the phase-stable
// 5:6 (60-on-72) cadence. `per` = core frames per display tick (≈0.832). A hitch that banks >2
// frames of credit is capped (drop backlog, never fast-forward). If Unity stops ticking (editor /
// stall), the 50 ms timeout grants a frame so audio/video never hard-freeze.
static void pace_to_display(double fps)
{
    double per = (fps > 1.0 ? fps : 60.0) / (s_displayHz > 1.0 ? s_displayHz : 72.0);
    std::unique_lock<std::mutex> lk(s_displayMutex);
    if (s_paceResync.exchange(false)) {
        s_paceLastDisplay = s_displayFrame.load(std::memory_order_relaxed);
        s_paceCredit = 0.0;
    }
    while (!s_pumpStop && !s_pumpPaused.load(std::memory_order_relaxed)) {
        uint64_t now = s_displayFrame.load(std::memory_order_relaxed);
        s_paceCredit += (double)(now - s_paceLastDisplay) * per;
        s_paceLastDisplay = now;
        if (s_paceCredit >= 1.0) break;
        if (s_displayCv.wait_for(lk, std::chrono::milliseconds(50)) == std::cv_status::timeout)
            s_paceCredit += 1.0;   // Unity not ticking → keep alive (~20 fps) rather than freeze
    }
    if (s_paceCredit > 2.0) s_paceCredit = 2.0;   // drop backlog after a hitch (no catch-up burst)
    s_paceCredit -= 1.0;
}

// Dedicated emu pump (see declarations near the top): retro_run + blit at the core's native rate,
// paced either display-locked (default, s_displayLock) or on an absolute wall-clock deadline.
// Falling behind (guest hiccup, app pause) resyncs and DROPS the backlog — never fast-forwards.
static void* pump_thread_fn(void*)
{
    pthread_setname_np(pthread_self(), "pdlr-pump");
    LOGI("[pump] emu pump thread up");
    struct timespec next; clock_gettime(CLOCK_MONOTONIC, &next);

    // Per-session state — locals, not statics. This thread is created by pdlr_start and joined by
    // pdlr_shutdown, so a local is scoped to exactly one game. Statics would survive into the next
    // game while the counters they track (vk.imageCount, s_prodFrames) are reset by pdlr_shutdown,
    // making the first delta of every 2nd+ game a huge unsigned underflow.
    time_t   lastChk = 0;                                // save_now.txt poll gate (1 Hz)
    double   accRun = 0, accBlit = 0, maxRun = 0;        // [speed] accumulators
    int      calls = 0;
    time_t   lastLog = 0;
    uint64_t lastImg = 0, lastProd = 0;

    while (!s_pumpStop) {
        if (s_pumpPaused.load(std::memory_order_relaxed)) {
            struct timespec ts { 0, 10000000 };   // 10 ms poll while paused
            nanosleep(&ts, nullptr);
            clock_gettime(CLOCK_MONOTONIC, &next);   // hold the wall-clock schedule at 'now' → clean resume
            s_paceResync.store(true);                // re-anchor the display-lock credit window on resume
            continue;
        }
        double fps = (s_fps > 1.0) ? s_fps : 60.0;   // live: SET_SYSTEM_AV_INFO updates s_fps mid-run

        struct timespec t0, t1, t2;
        clock_gettime(CLOCK_MONOTONIC, &t0);
        g.retro_run();
        clock_gettime(CLOCK_MONOTONIC, &t1);
        if (s_zeroCopy) blit_frame();   // GPU copy core frame -> AHB (Unity samples it)
        else            readback_frame();
        clock_gettime(CLOCK_MONOTONIC, &t2);

        // Capture trigger: if <gameDir>/save_now.txt appears, serialize the current (hopefully
        // music-alive) state to boot.state and consume the knob. Runs BETWEEN retro_run calls on this
        // same thread → safe. Lets us grab a good boot headlessly (drop the knob when SCIEB!=0).
        {
            time_t nowt = time(nullptr);
            if (nowt != lastChk && s_gameDir[0]) {
                lastChk = nowt;
                char kp[1088]; snprintf(kp, sizeof(kp), "%s/save_now.txt", s_gameDir);
                FILE* kf = fopen(kp, "r");
                if (kf) { fclose(kf);
                    char sp[1088]; snprintf(sp, sizeof(sp), "%s/boot.state", s_gameDir);
                    do_save_state(sp); remove(kp);
                }
            }
        }

        // Speed diagnostic (the "VF3 too fast" hunt). Per wall-second, log the pump call rate, the
        // core's produced-frame rate (set_image), and — the ground truth — audio frames produced/s.
        // realtime = audioFrames/s ÷ sample_rate: 1.00 = real time, >1 = the guest runs fast (and by
        // how much). coreFrames/s outrunning pump/s would mean the core's emu thread free-runs ahead
        // of our retro_run pacing. The three together localize any overspeed.
        double runMs = ms_since(t0, t1), blitMs = ms_since(t1, t2);
        accRun += runMs; accBlit += blitMs; if (runMs > maxRun) maxRun = runMs; calls++;
        if (t2.tv_sec != lastLog) {
            lastLog = t2.tv_sec;
            uint64_t img = vk.imageCount, prod = s_prodFrames.load(std::memory_order_relaxed);
            double sr = (s_sampleRate > 1.0) ? s_sampleRate : 44100.0;
            LOGI("[speed] pump=%d/s coreFrames=%llu/s (fps=%.2f)  audio=%llu/s (sr=%.0f) realtime=%.2fx  "
                 "retro_run avg=%.2f max=%.2fms blit=%.2fms",
                 calls, (unsigned long long)(img - lastImg), fps,
                 (unsigned long long)(prod - lastProd), sr, (double)(prod - lastProd) / sr,
                 accRun / (calls ? calls : 1), maxRun, accBlit / (calls ? calls : 1));
            lastImg = img; lastProd = prod;
            calls = 0; accRun = accBlit = maxRun = 0;
        }

        if (s_displayLock) {
            pace_to_display(fps);   // phase-lock to Unity's render cadence (default)
        } else {
            // Legacy free-running wall-clock deadline (displaylock.txt=0 for A/B).
            next.tv_nsec += (long)(1e9 / fps);
            while (next.tv_nsec >= 1000000000L) { next.tv_nsec -= 1000000000L; next.tv_sec++; }
            struct timespec now; clock_gettime(CLOCK_MONOTONIC, &now);
            if (ms_since(next, now) > 3.0 * (1000.0 / fps))
                next = now;                                                // >3 ticks late → resync, drop backlog
            else
                clock_nanosleep(CLOCK_MONOTONIC, TIMER_ABSTIME, &next, nullptr);
        }
    }
    LOGI("[pump] emu pump thread down");
    return nullptr;
}

int pdlr_run(void)
{
    if (!g.inited || !g.gameLoaded) return -1;
    if (s_pumpActive) {   // the pump thread drives retro_run; Unity's per-frame tick is a no-op
        static bool logged = false;
        if (!logged) { logged = true; LOGI("pdlr_run: pump thread active — C# tick calls are no-ops"); }
        return 0;
    }

    // DEBUG perf: time retro_run (emu.render — may block on the emu thread / a GPU fence) vs the
    // AHB blit, and count how often we're actually called. Logged ~1/sec. Pinpoints the 49fps stall.
    struct timespec t0, t1, t2;
    clock_gettime(CLOCK_MONOTONIC, &t0);
    g.retro_run();
    clock_gettime(CLOCK_MONOTONIC, &t1);
    if (s_zeroCopy) blit_frame();   // GPU copy core frame -> AHB (Unity samples it)
    else            readback_frame();
    clock_gettime(CLOCK_MONOTONIC, &t2);

    static double accRun = 0, accBlit = 0, maxRun = 0; static int calls = 0; static time_t lastLog = 0;
    double runMs = ms_since(t0, t1), blitMs = ms_since(t1, t2);
    accRun += runMs; accBlit += blitMs; if (runMs > maxRun) maxRun = runMs; calls++;
    if (t2.tv_sec != lastLog) {
        lastLog = t2.tv_sec;
        LOGI("[perf] pdlr_run=%d/s  retro_run avg=%.2f max=%.2fms  blit avg=%.2fms",
             calls, accRun / (calls ? calls : 1), maxRun, accBlit / (calls ? calls : 1));
        calls = 0; accRun = accBlit = maxRun = 0;
    }
    return 0;
}

void pdlr_set_paused(int paused)
{
    bool p = (paused != 0);
    if (s_pumpPaused.exchange(p) != p) LOGI("[pump] %s (app %s)", p ? "suspended" : "running", p ? "paused" : "resumed");
    if (!p) { s_govResync.store(true); s_paceResync.store(true); }   // don't let the guest catch up on resume
}

// Display-locked pacing: Unity calls this once per rendered frame (the pump paces retro_run off it).
void pdlr_notify_display_frame(void)
{
    s_displayFrame.fetch_add(1, std::memory_order_relaxed);
    s_displayCv.notify_one();
}
// Unity's actual display refresh (Hz) — sets the display-lock ratio. Call once at start (default 72).
void pdlr_set_display_hz(double hz) { if (hz > 1.0) s_displayHz = hz; }

// --- diagnostics accessors (see the diagnostics block at the top) -------------------------------

// The most specific reason the last pdlr_start failed. "" if it succeeded or was never called.
// The returned pointer stays valid until the next pdlr_last_error() call.
const char* pdlr_last_error(void)
{
    static std::string out;
    std::lock_guard<std::mutex> lk(s_diagMx);
    out = s_diagLastError;
    return out.c_str();
}

// The captured boot trace: core log lines at/above the capture level plus our own errors, one per
// line, oldest first. Valid until the next pdlr_recent_log() call.
const char* pdlr_recent_log(void)
{
    std::lock_guard<std::mutex> lk(s_diagMx);
    s_diagJoined.clear();
    for (const auto& l : s_diagRing) { s_diagJoined += l; s_diagJoined += '\n'; }
    return s_diagJoined.c_str();
}

void pdlr_set_log_verbosity(int min_level)
{
    if (min_level < RETRO_LOG_DEBUG) min_level = RETRO_LOG_DEBUG;
    if (min_level > RETRO_LOG_ERROR) min_level = RETRO_LOG_ERROR;
    s_diagMinLevel.store(min_level, std::memory_order_relaxed);
}

void pdlr_set_zero_copy(int enabled) { s_zeroCopy = (enabled != 0); }
int  pdlr_zero_copy_active(void)     { return s_zeroCopy ? 1 : 0; }   // false if pdlr_start auto-fell-back
double pdlr_frame_fps(void)          { return s_fps; }                // core's native video rate (Hz); 0 = unknown
double pdlr_sample_rate(void)        { return s_sampleRate; }         // core's audio sample rate (Hz); 0 = unknown

void pdlr_audio_set_output_rate(int hz) { if (hz > 0) s_audioOutRate = hz; }

// Drain up to `count` interleaved-stereo floats into dst; returns the number written. Call from
// Unity's OnAudioFilterRead (zero-fill the remainder for silence on underflow).
int pdlr_audio_read(float* dst, int count)
{
    if (!dst || count <= 0) return 0;
    std::lock_guard<std::mutex> lk(s_audioMutex);
    int n = (s_audioCount < count) ? s_audioCount : count;
    if (n > 0) {
        memcpy(dst, s_audio, (size_t)n * sizeof(float));
        int rem = s_audioCount - n;
        if (rem > 0) memmove(s_audio, s_audio + n, (size_t)rem * sizeof(float));
        s_audioCount = rem;
        s_audioSpaceCv.notify_one();   // freed space → wake a back-pressure-blocked producer
    }
    return n;
}
int  pdlr_unity_image_ready(void)    { return unityImagesReady ? 1 : 0; }   // all N buffers imported
int  pdlr_buffer_count(void)         { return kNumBuf; }
int  pdlr_ready_buffer_index(void)   { return readyIdx; }                   // which buffer C# should display (-1 = none yet)
const void* pdlr_get_unity_image_ptr_at(int idx)
{
    if (!unityImagesReady || idx < 0 || idx >= kNumBuf) return nullptr;
    return (const void*)&bufs[idx].unityImage;                              // VkImage* for CreateExternalTexture
}
void* pdlr_GetRenderEventFunc(void)  { return (void*)OnRenderEvent; }

int pdlr_frame_size(int* out_w, int* out_h)
{
    if (frameW <= 0 || frameH <= 0) return -1;
    if (out_w) *out_w = frameW;
    if (out_h) *out_h = frameH;
    return 0;
}

// The actual allocated AHB/external-texture dimensions (ahbW/ahbH): the kCeilW/kCeilH ceiling by
// default, or the game's native frame under s_nativeBuffer. C# sizes its external Texture2D to this and
// UV-crops the active sub-rect (pdlr_frame_size) within it — a no-op when native == active == buffer.
int pdlr_buffer_size(int* out_w, int* out_h)
{
    if (ahbW <= 0 || ahbH <= 0) return -1;
    if (out_w) *out_w = ahbW;
    if (out_h) *out_h = ahbH;
    return 0;
}

// Unity calls these when it loads/unloads the plugin (gives us IUnityInterfaces → Unity's VkDevice
// for the Step B import). If [UnityPluginLoad] never appears in logcat, the plugin was loaded too
// late for the interface and zero-copy import can't run (fall back to zeroCopy=false).
extern "C" UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API
UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
    s_interfaces = unityInterfaces;
    s_uvk = unityInterfaces ? unityInterfaces->Get<IUnityGraphicsVulkan>() : nullptr;
    LOGI("[UnityPluginLoad] called. interfaces=%p IUnityGraphicsVulkan=%p",
         (void*)s_interfaces, (void*)s_uvk);
    if (s_uvk) {
        UnityVulkanPluginEventConfig cfg{};
        cfg.renderPassPrecondition = kUnityVulkanRenderPass_EnsureOutside;
        cfg.graphicsQueueAccess    = kUnityVulkanGraphicsQueueAccess_Allow;
        cfg.flags = kUnityVulkanEventConfigFlag_EnsurePreviousFrameSubmission |
                    kUnityVulkanEventConfigFlag_ModifiesCommandBuffersState;
        s_uvk->ConfigureEvent(PDLR_EVENT_IMPORT_AHB, &cfg);
        LOGI("[UnityPluginLoad] ConfigureEvent(IMPORT_AHB) done");
    } else {
        LOGE("[UnityPluginLoad] IUnityGraphicsVulkan unavailable (is the app on Vulkan?)");
    }
}

extern "C" UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API UnityPluginUnload()
{
    LOGI("[UnityPluginUnload]");
    s_uvk = nullptr;
    s_interfaces = nullptr;
}

int pdlr_get_frame(const void** out_pixels, int* out_width, int* out_height)
{
    if (!rbReady || rbMapped == nullptr) return -1;
    if (out_pixels) *out_pixels = rbMapped;
    if (out_width)  *out_width  = rbW;
    if (out_height) *out_height = rbH;
    return 0;
}

int pdlr_frame_count(void) { return (int)vk.imageCount; }

void pdlr_set_input(uint32_t buttons, int16_t lx, int16_t ly, int16_t lt, int16_t rt)
{
    // DEBUG: log every button edge so we can confirm C# is actually delivering gamepad input (a
    // release build swallows Debug.Log, but this native line always reaches logcat). Absence of any
    // [input] line while pressing == Gamepad.current is null on the C# side.
    static uint32_t s_lastInputLogged = 0xffffffffu;
    if (buttons != s_lastInputLogged) {
        LOGI("[input] rx buttons=0x%04x lx=%d ly=%d lt=%d rt=%d (positional RetroPad)",
             buttons, (int)lx, (int)ly, (int)lt, (int)rt);
        s_lastInputLogged = buttons;
    }
    s_buttons  = buttons;
    s_analogLX = lx;
    s_analogLY = ly;
    s_triggerL = lt;
    s_triggerR = rt;
}

// Twin-stick variant: adds the right analog stick (rx/ry). The 5-arg pdlr_set_input above is kept for
// Flycast (and any caller that predates this) and simply implies rx=ry=0 — see the forwarder below.
void pdlr_set_input2(uint32_t buttons, int16_t lx, int16_t ly, int16_t rx, int16_t ry, int16_t lt, int16_t rt)
{
    pdlr_set_input(buttons, lx, ly, lt, rt);   // buttons + left stick + triggers + the edge log
    s_analogRX = rx;
    s_analogRY = ry;
}

void pdlr_set_port_device(unsigned port, unsigned device)
{
    if (port < 4) s_portDevice[port] = device;
}

// Frontend-pushed core-option override for the NEXT pdlr_start (from description.yaml `environment:`).
// Call BEFORE pdlr_start — the core reads options during retro_load_game. Overlays the Flycast.opt
// defaults (this wins); options left unset keep their default. Cleared on pdlr_shutdown.
void pdlr_set_option(const char* key, const char* value)
{
    if (!key || !*key || !value) return;
    s_optOverrides[key] = value;
    LOGI("[opt] queued override %s = \"%s\"", key, value);
}

void pdlr_set_lightgun(int16_t x, int16_t y, int offscreen, uint32_t buttons)
{
    s_lgX = x;
    s_lgY = y;
    s_lgOffscreen = offscreen;
    s_lgButtons = buttons;
}

const char* pdlr_core_name(void)    { return g.handle ? g.name    : nullptr; }
const char* pdlr_core_version(void) { return g.handle ? g.version : nullptr; }

void pdlr_shutdown(void)
{
    // Stop the pump first — it drives retro_run and touches the Vk resources torn down below.
    if (s_pumpActive)  { s_pumpStop = true;  s_audioSpaceCv.notify_all(); s_displayCv.notify_all(); pthread_join(s_pumpThread,  nullptr); s_pumpActive  = false; }
    s_prodFrames.store(0, std::memory_order_relaxed);

    // Step B: free the Unity-device imports first — they live on Unity's VkDevice, not the core's.
    // (C# must have destroyed its external Texture2Ds before calling this.)
    if (s_uvk) {
        UnityVulkanInstance uvi = s_uvk->Instance();
        if (uvi.device != VK_NULL_HANDLE) {
            vkDeviceWaitIdle(uvi.device);
            for (int i = 0; i < kNumBuf; ++i) {
                if (bufs[i].unityImage) vkDestroyImage(uvi.device, bufs[i].unityImage, nullptr);
                if (bufs[i].unityMem)   vkFreeMemory(uvi.device, bufs[i].unityMem, nullptr);
            }
        }
    }
    for (int i = 0; i < kNumBuf; ++i) { bufs[i].unityImage = VK_NULL_HANDLE; bufs[i].unityMem = VK_NULL_HANDLE; }
    unityImagesReady = false;

    // Read-back + AHB/blit resources (on the core's device, before we tear the device down).
    if (vk.device != VK_NULL_HANDLE) {
        vkDeviceWaitIdle(vk.device);
        if (rbFence) { vkDestroyFence(vk.device, rbFence, nullptr); rbFence = VK_NULL_HANDLE; }
        if (rbPool)  { vkDestroyCommandPool(vk.device, rbPool, nullptr); rbPool = VK_NULL_HANDLE; rbCmd = VK_NULL_HANDLE; }
        if (rbBuf)   { vkDestroyBuffer(vk.device, rbBuf, nullptr); rbBuf = VK_NULL_HANDLE; }
        if (rbMem)   { vkFreeMemory(vk.device, rbMem, nullptr); rbMem = VK_NULL_HANDLE; }
        if (blitFence) { vkDestroyFence(vk.device, blitFence, nullptr); blitFence = VK_NULL_HANDLE; }
        if (blitPool)  { vkDestroyCommandPool(vk.device, blitPool, nullptr); blitPool = VK_NULL_HANDLE; blitCmd = VK_NULL_HANDLE; }
        for (int i = 0; i < kNumBuf; ++i) {
            if (bufs[i].image) { vkDestroyImage(vk.device, bufs[i].image, nullptr); bufs[i].image = VK_NULL_HANDLE; }
            if (bufs[i].mem)   { vkFreeMemory(vk.device, bufs[i].mem, nullptr); bufs[i].mem = VK_NULL_HANDLE; }
        }
    }
    for (int i = 0; i < kNumBuf; ++i) {
        if (bufs[i].ahb) { AHardwareBuffer_release(bufs[i].ahb); bufs[i].ahb = nullptr; }
        bufs[i].firstUse = true;
    }
    fpGetAhbProps = nullptr; ahbW = ahbH = 0; writeIdx = 0; readyIdx = -1;
    blitPending = false; blitPendingIdx = -1;   // pipelined-fence state — next game starts fresh
    lastBlitImageCount = 0; lastRbImageCount = 0;
    rbMapped = nullptr; rbReady = false; rbW = rbH = 0; frameW = frameH = 0;

    if (g.gameLoaded && g.retro_unload_game) { LOGI("pdlr_shutdown: retro_unload_game"); g.retro_unload_game(); }
    // HW-render contract: fire the core's context_destroy while the VkDevice is still alive so it
    // releases its Vulkan objects now. Skipping this leaves them inside the core's statics, and the
    // dlclose-time destructors then call vkDestroy* on a dead device → SIGSEGV in the driver.
    if (vk.contextReady && vk.hwcb.context_destroy) { LOGI("pdlr_shutdown: context_destroy"); vk.hwcb.context_destroy(); }
    if (vk.haveNego && vk.nego.destroy_device) vk.nego.destroy_device();
    if (vk.device   != VK_NULL_HANDLE) { vkDeviceWaitIdle(vk.device); vkDestroyDevice(vk.device, nullptr); }
    if (vk.instance != VK_NULL_HANDLE) vkDestroyInstance(vk.instance, nullptr);
    if (g.inited && g.retro_deinit) { LOGI("pdlr_shutdown: retro_deinit"); g.retro_deinit(); }
    if (g.handle) { dlclose(g.handle); LOGI("pdlr_shutdown: dlclose"); }
    g = Core{};

    // Reset vk field-by-field (it holds a std::mutex, so it isn't assignable).
    vk.instance = VK_NULL_HANDLE; vk.phys = VK_NULL_HANDLE; vk.device = VK_NULL_HANDLE;
    vk.queue = VK_NULL_HANDLE; vk.queueFamily = 0; vk.contextReady = false;
    vk.haveHwcb = false; vk.haveNego = false;
    memset(&vk.hwcb, 0, sizeof(vk.hwcb));
    memset(&vk.nego, 0, sizeof(vk.nego));
    memset(&vk.iface, 0, sizeof(vk.iface));
    memset(&vk.lastImage, 0, sizeof(vk.lastImage));
    vk.haveImage = false; vk.imageCount = 0;
    lastBlitImageCount = 0; lastRbImageCount = 0;   // "seen" marks derived from imageCount — reset with it
    vk.semaphores = nullptr; vk.numSemaphores = 0; vk.srcQueueFamily = 0;

    s_systemDir[0] = 0; s_saveDir[0] = 0; s_gameDir[0] = 0;
    s_buttons = 0; s_analogLX = 0; s_analogLY = 0; s_analogRX = 0; s_analogRY = 0; s_triggerL = 0; s_triggerR = 0;
    s_optOverrides.clear();   // next game re-declares its own overrides before pdlr_start
    for (unsigned p = 0; p < 4; ++p) s_portDevice[p] = RETRO_DEVICE_JOYPAD;   // next game starts pad-only
    s_lgX = -0x7fff; s_lgY = -0x7fff; s_lgOffscreen = 1; s_lgButtons = 0;
    s_fps = 0.0; s_sampleRate = 0.0;
    s_govStartMs = -1; s_govBaseFrames = 0; s_govResync.store(false);
    { std::lock_guard<std::mutex> lk(s_audioMutex); s_audioCount = 0; }
}
