# AGEBasic Video Control — Developer Documentation

This document describes the implementation of the AGEBasic video control feature (issue #851). It is intended for developers who need to understand, debug, or extend the system.

---

## Overview

Cabinets of type `crt: type: 19i-agebasic` are managed by `AGEBasicScreenController`. These cabinets have a `GameVideoPlayer` component for video playback. The video control feature exposes that player to AGEBasic scripts through a set of commands and functions, allowing cabinet scripts to load, play, pause, stop, seek, and loop video files.

---

## Architecture

### Key classes and their roles

#### `GameVideoPlayer.cs`
The Unity `MonoBehaviour` wrapping Unity's `VideoPlayer`. It manages the full lifecycle of a single video clip.

**State fields:**
- `videoPath` — path of the currently loaded clip
- `isPreparing` — true while Unity's async `Prepare()` is in progress
- `isReady` — true once `PrepareCompleted` has fired
- `loopEnabled` — desired loop state (default `true`); applied to `videoPlayer.isLooping` at play time

**Key methods added for AGEBasic:**

| Method | Purpose |
|---|---|
| `ChangeVideo(path, invertX, invertY)` | Changes the clip without touching the shader. Resets `isPreparing`/`isReady`, stops the current video. |
| `SeekTo(seconds)` | Sets `videoPlayer.time`. Only works when prepared. |
| `GetCurrentTime()` | Returns `videoPlayer.time`. |
| `GetDuration()` | Returns `videoPlayer.length`. |
| `GetStatus()` | Returns 0=not loaded, 1=stopped/ready, 2=playing, 3=paused. |
| `SetLoop(bool)` | Sets `loopEnabled`; applies immediately if prepared and not paused. |
| `GetLoopStatus()` | Returns `loopEnabled`. |

**`setVideo()` — shader initialization fix:**
`setVideo()` is called from `AGEBasicScreenController.runBT()` with the attraction video path and `videoShader`. When no attraction video is configured the path is empty and the method previously returned early without storing `shader`, causing a null reference crash in `Play()`. The fix: `this.shader = shader` is now assigned before the empty-path guard so the shader is always available.

**`PrepareCompleted` — async playback fix:**
Unity's video preparation is asynchronous. `VIDEOPLAY` calls `Play()`, which triggers `PrepareVideo()` and returns immediately — the AGEBasic script ends before preparation completes. The `PrepareCompleted` callback was previously calling `vp.Pause()` (designed for the attraction video BT loop which retried `Play()` each tick). For AGEBasic one-shot calls this meant the video would prepare but never actually start. The fix: `PrepareCompleted` now calls `this.Play()` directly, which switches the renderer to the video shader and starts playback as soon as the clip is ready.

**Pause workaround:**
Unity's `VideoPlayer.Pause()` has no effect when `isLooping = true`. `Pause()` sets `isLooping = false` before calling `videoPlayer.Pause()`, then `Play()` restores `isLooping = loopEnabled`.

---

#### `ConfigurationCommands` (`basicConfigurationCommands.cs`)
The shared execution context passed to every command and function. Two fields were added:

```csharp
public GameVideoPlayer VideoPlayer;   // set by CabinetAGEBasic.SetVideoConfig()
public ShaderScreenBase GameShader;   // the CRT/game shader (not the video shader)
```

`VideoPlayer` is `null` in non-video cabinets (e.g., `AGEBasicCabinetController`). Every video command and function guards against this with a warning.

---

#### `CabinetAGEBasic.cs`
Added `SetVideoConfig(GameVideoPlayer videoPlayer, ShaderScreenBase gameShader)` which writes both fields into `AGEBasic.ConfigCommands`. Called from `AGEBasicScreenController.runBT()` during initialization.

---

#### `AGEBasicScreenController.cs`

**Initialization (`runBT`):**
```csharp
videoPlayer.setVideo(VideoFile, videoShader, VideoInvertX, VideoInvertY); // stores videoShader even if VideoFile empty
cabinetAGEBasic.ActivateShader(shader);                                    // activates game/CRT shader
cabinetAGEBasic.SetVideoConfig(videoPlayer, shader);                       // wires into ConfigCommands
```

**`StartPlayerActivities()` call order fix:**
`setupActionMap()` must run before `PreparePlayerToRunPrograms()` because it lazily initializes `libretroControlMap` via `GetComponent`. Previously the order was reversed, causing a null reference on `libretroControlMap.Enable()`.

---

### Shader switching

There are two shader instances on the same renderer:

| Shader | When active | What is visible |
|---|---|---|
| `videoShader` (e.g. `crtlod`) | During `VIDEOPLAY` | Video frame |
| `shader` (e.g. `crt`) | After `VIDEOPAUSE` / `VIDEOSTOP` | AGEBasic `PRINT`/`SHOW` output |

Switching is done by calling `ScreenGenerator.ActivateShader(shader)`, which:
1. Sets `display.materials[position] = shader.material` (swaps the renderer's material)
2. Calls `shader.Activate(screenTexture)` (binds the ScreenGenerator's texture)

> **Important:** `ShaderScreenBase.ApplyConfiguration()` only modifies material properties — it does **not** swap the material on the renderer. Always use `ScreenGenerator.ActivateShader()` to switch the active shader.

`VIDEOPLAY` activates the video shader inside `GameVideoPlayer.Play()` via `shader.Activate(videoPlayer.texture)`.
`VIDEOPAUSE` and `VIDEOSTOP` both call `config.ScreenGenerator?.ActivateShader(config.GameShader)` to restore the CRT shader.

**Overlay limitation:** AGEBasic drawing (`PRINT`, `SHOW`) is never visible during playback. The renderer can only show one texture at a time, and the CRT shader's software compositing pipeline (`ScreenGenerator.baseTexture`) is a separate CPU-side `Texture2D` from the video's GPU texture. A composite shader would be needed to overlay text on video, which was deemed too expensive given the multi-pass CRT shader requirement.

---

## New commands

All commands live in `Assets/curif/LibRetroWrapper/basic/Commands/` and are registered in `basicCommands.cs`.

### `VIDEOLOAD` (`VIDEOLOAD.cs`)
Inherits `CommandExpressionListBase`. Accepts 1–3 expressions: `path [, invertX [, invertY]]`.
Calls `config.VideoPlayer.ChangeVideo(path, invertX, invertY)`.

### `VIDEOPLAY` (`VIDEOPLAY.cs`)
Inherits `CommandNoExpressionBase`.
Calls `config.VideoPlayer.Play()`. If the clip is not yet prepared, `Play()` starts async preparation; `PrepareCompleted` will call `Play()` again automatically when ready.

### `VIDEOPAUSE` (`VIDEOPAUSE.cs`)
Inherits `CommandNoExpressionBase`.
Calls `config.VideoPlayer.Pause()` then `config.ScreenGenerator?.ActivateShader(config.GameShader)`. The CRT shader is restored so the HUD timer can draw over it.

### `VIDEOSTOP` (`VIDEOSTOP.cs`)
Inherits `CommandNoExpressionBase`.
Calls `config.VideoPlayer.Stop()` then `config.ScreenGenerator?.ActivateShader(config.GameShader)`.

### `VIDEOSEEK` (`VIDEOSEEK.cs`)
Inherits `CommandSingleExpressionBase`. Accepts one numeric expression (seconds).
Calls `config.VideoPlayer.SeekTo(value)`. No-op if the video is not prepared.

### `VIDEOLOOP` (`VIDEOLOOP.cs`)
Inherits `CommandSingleExpressionBase`. Accepts `1` (loop on) or `0` (loop off).
Calls `config.VideoPlayer.SetLoop(value != 0)`.

---

## New functions

All functions live in `Assets/curif/LibRetroWrapper/basic/functions/` and are registered in `basicCommands.cs`.

### `video.cs` — `VIDEOTIME`, `VIDEODURATION`, `VIDEOSTATUS`, `VIDEOLOOPSTATUS`
All inherit `CommandFunctionNoExpressionBase` (zero-argument functions).

| Function | Implementation |
|---|---|
| `VIDEOTIME()` | `config.VideoPlayer.GetCurrentTime()` |
| `VIDEODURATION()` | `config.VideoPlayer.GetDuration()` |
| `VIDEOSTATUS()` | `config.VideoPlayer.GetStatus()` → 0/1/2/3 |
| `VIDEOLOOPSTATUS()` | `config.VideoPlayer.GetLoopStatus() ? 1 : 0` |

### `VIDEOPATH()` (`filemngmnt.cs`)
Added as a case in `PathFunctionsBase.Execute()`.
Returns `ConfigManager.VideoDir` — the device path where video files are stored.

---

## Video folder

`ConfigManager.VideoDir` is defined as:
```csharp
public static string VideoDir = Path.Combine(BaseDir, "video");
```
The folder is created automatically in `ConfigManager.createFolders()`. On Quest the resolved path is `/sdcard/Android/data/com.curif.AgeOfJoy/files/video/`.

---

## `PRINTCENTERED` bug fix

During development a pre-existing bug in `CommandPRINTCENTERED.Execute()` was found and fixed. The method was reading `vals[1]` as `invertedFlag` and using `vals[0].GetString()` as the text content, when the correct signature is `y, text, invertedFlag [, drawFlag]`. Fixed indices: `vals[1]` = text, `vals[2]` = invertedFlag, `vals[3]` = drawFlag.

---

## Reference cabinet: `videoplayer`

Located at `AgeOfJoyCabinets/videoplayer`. Use it as a working reference and integration test for all video commands.

| File | Purpose |
|---|---|
| `description.yaml` | Cabinet config: `crt: type: 19i-agebasic`, all button/timer events |
| `after_load.bas` | Idle screen shown before coin insert |
| `insert_coin.bas` | Scans video folder, loads first file, starts playback |
| `player.bas` | All playback controls (play/pause/stop/seek/next/prev/restart/loop) |
| `hud.bas` | Timer-driven HUD; redraws when status is stopped (1) or paused (3) |

The `FILES` array is populated dynamically in `insert_coin.bas` via `GETFILESARRAY(VIDEOPATH(), 0)` and is not declared in `description.yaml`. Only `CURR_FILE` and `FILE_COUNT` are declared there (needed for `when:` condition guards before the coin is inserted).

---

## Common failure modes

| Symptom | Likely cause |
|---|---|
| `NullReferenceException` in `Play()` at `shader.Invert` | `shader` not set — `setVideo()` called with empty path before this fix |
| Video loads but never plays | `PrepareCompleted` calling `Pause()` instead of `Play()` — old behavior |
| HUD not visible after pause/stop | Using `ApplyConfiguration()` instead of `ActivateShader()` to restore shader |
| `PRINTCENTERED` compilation error "3 expected" | Calling with fewer than 3 arguments: `PRINTCENTERED y, text, invertedFlag` required |
| Line order compilation error in `.bas` | Edit tool inserted lines mid-file — always verify ascending line numbers after edits |
| `NullReferenceException` on `libretroControlMap.Enable` | `setupActionMap()` was called after `PreparePlayerToRunPrograms()` — order matters |
