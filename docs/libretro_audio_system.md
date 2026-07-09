# Libretro Audio System — Architecture, Instrumentation, and the "Frying" Investigation

*Last updated: 2026-07-03. Written during the investigation of crackling ("frying potatoes")
audio in libretro games. Documents how game audio flows through the system, the
instrumentation added to diagnose it, everything measured and ruled out so far, and the
current standing hypothesis.*

---

## 1. Audio pipeline overview

Game audio travels through seven layers, each with its own clock, buffer, and thread:

```
MAME core (.so)                      emulated sound chips, core-native sample rate
      │  retro_audio_sample_batch
      ▼
Native wrapper ring buffer           8192 floats (4096 stereo frames ≈ 85ms @48kHz)
      │  guarded by AudioBufferLock (C# object, taken via native → managed callbacks)
      ▼
LibretroMameCore.MoveAudioStreamTo   C#, runs on Unity's audio (FMOD mixer) thread
      │  Marshal.Copy + wrapper_audio_consume_buffer
      ▼
OnAudioFilterRead                    LibretroScreenController / PortableGames
      │  procedural source: AudioSource playing with no clip
      ▼
FMOD mixer                           mixes all voices, applies mixer groups
      │
Meta XR Audio spatializer            (bypassed for game audio: spatialize = false)
      │
Android audio stream (AAudio/OpenSL) OS-side buffering and resampling
      ▼
Quest hardware                       native 48 kHz output
```

### Key components

| Component | File | Role |
|---|---|---|
| `MoveAudioStreamTo(float[], int channels)` | `LibretroMameCore.cs` | Drains the native ring buffer into Unity's DSP buffer on the audio thread. Frame-aligns the copy, zero-fills on underrun, never blocks more than 4 ms on the lock. |
| `AudioLockCB` / `AudioUnlockCB` | `LibretroMameCore.cs` | Managed callbacks the native wrapper calls around its ring-buffer writes (from the emulator run thread). Both sides synchronize on `AudioBufferLock`. |
| `OnAudioFilterRead` | `LibretroScreenController.cs`, `PortableGames.cs` | Unity's DSP filter callback. Guarded by `isRunning(ScreenName, GameFile)` so only the screen that owns the running game consumes the buffer. |
| Run thread | `LibretroMameCore.StartRunThread()` | Dedicated `LongRunning` thread at `ThreadPriority.Highest`. Paces `wrapper_run()` at the core-reported fps via `FpsControlNoUnity` (sleeps when it has slack; it previously busy-spun at 100% of a core). |
| `Speaker` | static `AudioSource` | The running cabinet's AudioSource. `priority = 0` while a game runs (restored to 128 on exit) so FMOD can never virtualize/steal its voice. |
| `SIDPlayer` | `SIDPlayer.cs` | SID music via a second `OnAudioFilterRead` on the same GameObject (additive mix). No longer force-starts the shared AudioSource on init — only when SID music actually plays. |

### Sample rates

- Most MAME cores output **exactly 48000 Hz** (galaga, mspacman, starforce all report it).
- `LibretroMameCore.Start()` reads `AudioSettings.GetConfiguration().sampleRate` and passes
  it to the wrapper (`QuestAudioFrequency`), which resamples core output to that rate.
- The project ran at **22050 Hz** for years (CPU savings). It is now **48000 Hz**: Quest
  hardware mixes at 48 kHz natively (so 22050 was resampled *twice* — wrapper down, OS back
  up), and the Meta XR Audio plugin officially supports only 48 kHz. At 48 kHz the wrapper's
  resampler is a 1:1 passthrough for most games.

### The shared AudioSource problem

One `AudioSource` per cabinet screen serves four masters:

1. **Game audio** — procedural via `OnAudioFilterRead` (no clip; source must be `Play()`ing).
2. **Attract clip** — `GameAudioPlayer` assigns/plays audio files.
3. **Attract video audio** — `VideoPlayer` with `audioOutputMode = AudioSource`.
4. **SID music** — `SIDPlayer`'s additive filter (installs a silent looping clip if needed).

The behavior tree in `LibretroScreenController` arbitrates: it stops clip/video playback
before a game starts and switches the mixer group (`audioMixerAttractMode` ↔
`audioMixerGame`, `spatialize` true ↔ false). Because the source is shared, any component
that force-starts or reconfigures it affects all the others — see §4 (SIDPlayer fix).

---

## 2. The failure mode: ring-buffer overflow (not underrun)

The intuitive assumption — crackling = buffer underruns (consumer starving) — was **wrong**.
Instrumentation proved the opposite:

- The producer is *perfect*: `runs/s` matches the core fps to two decimals.
- Underruns are ~zero; ring occupancy sits **pegged at its 8192 ceiling**.
- Unity's audio thread consumes at only **~84% of nominal** (39.4 callbacks/s of 1024-frame
  blocks where 46.9/s are required at 48 kHz).

So the wrapper produces real-time audio, Unity drains slower than real time, the ring
buffer overflows, and the wrapper **drops chunks**. Every drop seam is a crackle; the
surviving stream is time-compressed. This produced a diagnostic signature worth remembering:

| Observation | Why |
|---|---|
| WAV capture of the stream sounds **slightly fast** | dropped chunks compress time |
| In-headset tempo sounds **correct** | slow consumption (~84%) cancels the compression |
| In-headset pitch is slightly **low** (reported on mspacman) | 48 kHz content played at the effective ~40 kHz consumption rate |
| Crackling everywhere | discontinuities at every dropped-chunk seam |

**Rule of thumb: fast-but-crackly = overflow (producer side fine); slow/gappy-but-clean-pitch
= underrun (consumer side fine).**

---

## 3. Instrumentation (what exists now and how to read it)

### `[AudioStats]` — logged every 5 s while a game runs

Emitted by `LibretroScreenController.Update()` via `LibretroMameCore.GetAndResetAudioStats()`:

```
[AudioStats] elapsed: 5.0s callbacks/s: 39.4 consumed floats/s: 80629 expected: 96000 (84.0%)
underruns: 0 missing: 0 occupancy min/max: 2240/8192 gaps: 36 maxGap: 51.5ms
lockWait avg/max: 0.02/3.80ms lockMisses: 0 runs/s: 60.55 expectedFps: 60.61 playingAudioSources: 13
```

| Field | Meaning | Healthy value |
|---|---|---|
| `callbacks/s` | `OnAudioFilterRead` service rate | sampleRate ÷ dspBufferSize (46.9 at 48k/1024) |
| `consumed floats/s` vs `expected` | actual drain rate vs `QuestAudioFrequency × 2` | ≈100%. Below ⇒ overflow+drops; above ⇒ underruns |
| `underruns` / `missing` | callbacks that found too little data / floats short | 0 |
| `occupancy min/max` | ring-buffer fill range (floats, capacity 8192) | mid-range; pegged at 8192 ⇒ overflow, 0 ⇒ starvation |
| `gaps` / `maxGap` | callbacks arriving >1.5× the block duration apart | 0 / ≈ block duration (21.3 ms at 48k/1024) |
| `lockWait avg/max`, `lockMisses` | audio-thread wait on `AudioBufferLock`; misses gave up after 4 ms | ≈0 / <1 ms / 0 |
| `runs/s` vs `expectedFps` | emulator pacing | equal |
| `playingAudioSources` | scene-wide playing voices | context-dependent |

### WAV capture — `#define _debug_audio_` (top of `LibretroMameCore.cs`)

Records **exactly what `MoveAudioStreamTo` hands to Unity** — the last point we control —
into `<GameSaveDir>/audio_capture.wav` (16-bit PCM stereo):

- Armed on every game start; skips leading silence (boot); captures 15 s of audible audio.
- Flushed to disk on a background task when full, or force-flushed (partial) on game exit.
- Logs `AUDIO CAPTURE saved: <path>` (or `... failed: <reason>`).
- Pull with `adb pull /sdcard/Android/data/com.curif.AgeOfJoy/save/audio_capture.wav`.

**Interpretation:** WAV fries ⇒ corruption is at/before our copy (wrapper/core). WAV clean
but headset fries ⇒ corruption is downstream (FMOD/plugin/OS). It localized the bug upstream
of FMOD in one test. Comparing against RetroArch on PC separates *core-authentic* sounds
from real artifacts (starforce's "pip" is authentic; galaga's frying is not).

Disable the define before release builds.

---

## 4. Suspects investigated and ruled out

Each was a plausible cause; each was tested and measured. Keep this list — it prevents
re-investigating dead ends.

| Suspect | Test | Verdict |
|---|---|---|
| DSP buffer size (512 vs 1024) | frying already present at 1024 ("Best performance") | **Not the cause** |
| Consumer starvation / CPU load | underruns ≈ 0, occupancy pegged high | **Wrong direction** — it's overflow |
| Run-thread busy-spin & scheduling | replaced with sleeping `LongRunning` thread, `Highest` priority; `runs/s` exact | Good hygiene; **didn't change frying** |
| Wrapper resampler (48000→22050) | moved Unity to 48 kHz (wrapper 1:1 passthrough) | **Exonerated** — same deficit at both rates |
| Unity mixer/spatializer corrupting downstream | WAV capture already fries | **Downstream exonerated** |
| Voice stealing/virtualization | `Speaker.priority = 0` | No change |
| Virtualize Effects (silent-input filter bypass) | unchecked in Project Settings → Audio | No change |
| Meta XR Audio plugin (spatializer + ambisonic) | set both to None, rebuilt | **No change** — same 39.4/s cadence |
| Too many playing AudioSources | SIDPlayer no longer force-starts every cabinet's source; `playingAudioSources: 13` measured | **Not load-related** |
| Emulator pacing too fast | `runs/s` 60.55–60.63 vs expected 60.61 | **Producer exonerated** |

### Correctness fixes kept regardless (all in `LibretroMameCore.cs` unless noted)

- `MoveAudioStreamTo`: frame-aligned `toCopy` (no L/R interleave shift), zero-filled tail on
  partial copies (pop → brief silence), 4 ms lock timeout (mixer can't be stalled by the producer).
- Native callback delegates rooted in static fields (GC could previously collect them while
  native code held the function pointers — random-crash risk).
- `AudioBufferLock` never reassigned (`ClearAll` used to replace it mid-teardown).
- Run thread: dedicated thread, sleeps instead of burning a core.
- `SIDPlayer.cs`: no `EnsureAudioRunning()` on `Start()` — the shared cabinet AudioSource is
  only forced into playing state when SID music actually plays.

---

## 5. Root cause (found 2026-07-04) and the fix

Lock contention was disproved (`lockWait 0.00ms, lockMisses 0`). The decisive measurement
chain (each step one build):

1. DSP blocks 1024 → 39.4 callbacks/s; blocks 512 → 78.8 callbacks/s — but **consumption
   identical: ~80,650 floats/s = 40,320 frames/s** in both. It's a throughput cap, not a
   callback-count cap.
2. The cap follows a precise law across every configuration: **the Unity audio thread
   loses ~4 µs of wall time per audio frame** — delivered rate = 1/(1/fs + 4µs). At
   48000 that's 40,320 (84%); it also retro-predicts the earlier "22050-era" measurement
   exactly (the AudioManager 22050 setting never actually took effect on device — FMOD ran
   48000/1024 all along, which is why both eras measured the same 39.4 callbacks/s).
3. Production measured via conservation (`consumed + discarded + Δoccupancy`): **~94–96k
   floats/s = 48,000 frames/s always**, regardless of the rate passed from C#.

Then the wrapper source turned up **inside this repo** — `Assets/curif/LibRetroWrapper/
cwrapper/audio.c` (it compiles into the core .so, the "AGE of Joy test mod" build) — with
the final piece:

```c
#define QUEST_AUDIO_FREQUENCY 48000   // resampler output rate, hardcoded
```

The rate C# passes to `wrapper_environment_open` only sets the *core generation* rate
(served as core option `mame2003-plus_sample_rate`); the wrapper always resamples the
core's output back to hardcoded 48000. So:

> **Root cause:** the wrapper unconditionally produces 48,000 frames/s while Unity's audio
> thread on Quest can only drain ~40,320 frames/s (~4 µs/frame pacing tax in the
> FMOD/Android output layer, external to this codebase). The permanent ~16% surplus
> overflows the ring buffer and every dropped chunk is a crackle. This was true in every
> historical configuration; only the surplus percentage varied.

### The fix: calibrated output rate

The Unity-side tax can't be removed from app code, so production is matched to measured
consumption:

- **`cwrapper/audio.c`**: `QUEST_AUDIO_FREQUENCY` replaced by a runtime-settable
  `OutputSampleRate` (new export `wrapper_audio_set_output_rate(double)`, declared in
  `audio.h`). Default 48000 = legacy behavior.
- **`LibretroMameCore.cs`**: measures the real drain rate from healthy stats windows
  (underruns == 0), persists it in `PlayerPrefs["AudioCalibratedOutputRate"]` on game exit
  (`SaveAudioCalibration()`, needs ≥15 s of playback), and applies it at every game start.
  First-ever session runs uncalibrated (legacy frying); every session after is matched.
  Old core builds without the export are tolerated (EntryPointNotFound → legacy behavior).
- The ring **recenter** (§3 additions) plus underrun **zero-fill** guard both ends against
  residual jitter drift.

**Deployment caveat: the core `.so` and the APK must be rebuilt together** — cwrapper
changes ship inside the core, and cores are sideloaded per-device (`CoresDir` →
`InternalCoresDir`), so a stale core on the device silently reverts to legacy behavior
(the C# logs a warning).

**Why not just read the game's declared sample rate instead of measuring?** `audio.c`
already reads `wrapper_environment_get_sample_rate()` (`av_info.timing.sample_rate`, the
core's own reported audio-generation rate) as the resampler *input* rate. It's tempting to
ask why the *output* rate can't be set from the same source. It can't, because the two
numbers describe unrelated things: the declared rate is a property of the specific MAME
driver's sound-chip clock divider (fixed per game), while the true output constraint is
Unity's audio-thread drain capacity on this device/OS/Unity version (fixed per install,
independent of game content). They are unrelated axes that can coincidentally collide —
confirmed on-device: Galaga declares `sample_rate: 40320.000000` (a quirk of its driver's
clock divider) which happens to match the measured Quest drain rate almost exactly, while
mspacman, starforce, and gyruss all declare the generic MAME default `sample_rate:
48000.000000`. Using the declared rate as the output target would have "fixed" Galaga by
luck while leaving every other game overflowing at their real declared 48000 — the
calibration has to be measured from actual consumption, not read from game metadata.

### Open questions

- The ~4 µs/frame output tax itself: Unity version dependent? (project: 2021.3.22f1 — an
  LTS patch upgrade may change it; the calibration self-corrects either way, including back
  to 48000 if the tax ever disappears.)
- Whether the OS inserts audible silence at the FMOD→AAudio boundary independent of our
  ring drops. If frying persists after calibration with clean stats (`recenters: 0`,
  `underruns: 0`, WAV clean), the definitive escalation is **native AAudio output in the
  wrapper** (bypass Unity audio for game sound entirely, like RetroArch — which is clean on
  the same device).

---

## 6. Why this class of bug is hard (lessons)

1. **Push meets pull with no backpressure.** The emulator pushes on its clock; FMOD pulls on
   its own. The only coupling is a fixed-size ring buffer, so any rate mismatch *must*
   surface as drops or gaps — silently.
2. **`OnAudioFilterRead` is a side door.** It requires a playing AudioSource, a clip-less
   source looks "silent" to engine optimizers, and nothing documents when the engine may
   skip calling it. Procedural audio inherits all these quirks.
3. **Configured ≠ actual.** Unity reports the sample rate and buffer size it was *asked*
   for. The measured callback cadence is the only truth. All key facts in this
   investigation came from counters we added, not from any Unity API or profiler.
4. **Audio bugs degrade instead of crashing.** No exception, no log — just frying. The
   audio thread must never block/allocate/log, and nothing enforces it.
5. **Measure before fixing.** Three "obvious" fixes (buffer size, priorities, spatializer)
   were all reasonable and all wrong. The WAV capture and the rate counters each eliminated
   half the search space in a single test. Instrument first.
