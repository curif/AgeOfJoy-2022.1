# Cabinet Attraction Video System — Developer Documentation

This document describes how cabinet **attraction videos** work end to end: player proximity detection, video playback/lifecycle, the global decoder budget that caps how many videos can play at once, and how everything interacts with cabinet suspend/resume and AGEBasic game sessions.

For the AGEBasic scripting surface (`VIDEOPLAY`, `VIDEOLOAD`, `VIDEOSTATUS()`, etc.) see [`agebasic_video.md`](./agebasic_video.md) — this document covers the engine side underneath those commands.

---

## Overview

Cabinets with a screen show an **attraction video** (a looping demo clip) when nobody is playing them, to lure players in. Two screen controller types drive this, both implementing `ISuspendableCabinetScreen`:

- **`AGEBasicScreenController.cs`** — cabinets of type `crt: type: 19i-agebasic`, where the whole game is an AGEBasic program (no MAME/LibRetro emulation).
- **`LibretroScreenController.cs`** — MAME/LibRetro-emulated cabinets, where the attraction video plays only when no game is loaded.

Both own a `GameVideoPlayer` component that wraps Unity's `VideoPlayer`, and both tick a Behavior Tree (Fluid Behavior Trees) roughly once a second that decides whether the video should play, pause, or stop based on player proximity and gaze.

Because cabinets are never destroyed once the room loads them (they are only *suspended*), and every cabinet screen owns its own hardware video decoder, standing near several cabinets at once can spin up several simultaneous decoders — expensive on Quest hardware. A global budget (`AttractVideoBudget`) caps how many can actually be playing/prepared at a time.

---

## Player proximity and gaze detection

Two independent layers feed the video-control Behavior Tree:

### 1. Trigger-volume presence — `AgentScenePosition.cs`

Each cabinet has one or more `AgentScenePosition` components — `BoxCollider`s set as triggers around player standing spots. `OnTriggerEnter`/`OnTriggerExit` set `IsPlayerPresent` when the player (tag `Player` / object name `OVRPlayerControllerGalery`) enters/leaves. An optional `playerStayDurationTimeSecs` adds a dwell timer before `IsPlayerPresent` flips true, to avoid flicker for players just passing by.

`AGEBasicScreenController.playerIsInSomePosition()` is `AgentPlayerPositions.Any(asp => asp.IsPlayerPresent)` — true when the player occupies any of the cabinet's registered positions.

### 2. Distance + gaze checks

- **`AGEBasicScreenController.IsNearPlayer()`** — `Vector3.Distance(...) <= DistanceMinToPlayerToActivate` (default **4 m**).
- **`AGEBasicScreenController.isPlayerLookingAtScreen4()`** — viewport test plus a `Physics.Linecast` on the CRT layer to confirm the screen isn't occluded by geometry between the player and the cabinet.
- **`LibretroScreenController`** caches `distanceToPlayer` and `playerInTheZone` once per BT tick (`runBT`), and separates the video and audio activation radii: `DistanceMaxToPlayerToActivateVideo` (default **2.5 m**) and `DistanceMaxToPlayerToActivateAudio` (default **3.5 m**) — so a player can be close enough to hear a game's attract audio before they're close enough to trigger the (more expensive) video decode. Gaze uses `isPlayerLookingAtScreenZone()`.

---

## `GameVideoPlayer.cs` — per-screen playback engine

`Assets/curif/LibRetroWrapper/GameVideoPlayer.cs`. One instance per cabinet screen (`RequireComponent(VideoPlayer)`, `RequireComponent(TextureCache)`). There is **no decoder pooling** — every screen owns its own Unity `VideoPlayer`.

### State

| Field | Meaning |
|---|---|
| `videoPath` | path of the currently loaded clip |
| `isPreparing` | true while Unity's async `Prepare()` is in flight |
| `isReady` | true once `PrepareCompleted` has fired |
| `loopEnabled` | desired loop state (default `true`) |
| `desired` (`Stopped`/`Paused`/`Playing`) | the *latest* intention expressed by a caller (BT tick or AGEBasic command) |

`desired` exists so an async `PrepareCompleted` callback — which can fire well after the `Play()` call that triggered it — always honors whatever the caller most recently asked for, instead of blindly resuming playback. Without it, a `Stop()`/`Pause()` that arrives while a prepare is still in flight could get overridden the moment the prepare finishes.

### Lifecycle

- **`setVideo(path, shader, invertX, invertY)`** — wires this screen's video source. Stores the shader reference, then calls `textureCache.Init(path)` to asynchronously load the on-disk cached first-frame thumbnail (`path + ".png"`) if one exists. Called once per screen (guarded by `videoInitialized` in the controllers) — re-calling it on every resume would race the thumbnail load against a live video frame.
- **`Play()`** — the core state machine:
  1. Sets `desired = Playing`. If a prepare is already in flight, returns immediately (the eventual `PrepareCompleted` will honor `desired`).
  2. If not yet prepared: asks `AttractVideoBudget.RequestSlot(this)` for a decoder slot. **Denied → shows the cached fallback texture and returns** (the caller — normally a BT tick — retries next cycle). Granted → calls `PrepareVideo()` (`videoPlayer.Prepare()`), and shows the cached thumbnail (or the generic standby texture) while preparing.
  3. If already prepared and ready but not playing: requests a slot again (free if already held), then actually calls `videoPlayer.Play()`, binds the video texture to the shader, and applies loop/skip-on-drop settings.
  4. Once playing, if no first-frame thumbnail is cached yet, kicks off `textureCache.Load(videoPlayer.texture)` to capture and persist one (see below).
- **`Pause()`** — sets `desired = Paused`. Unity's `VideoPlayer.Pause()` is a no-op while `isLooping = true`, so this sets `isLooping = false` before pausing (restored by `Play()`). The decoder slot is **kept** while paused — pausing doesn't free hardware resources, only playing does.
- **`Stop()`** — sets `desired = Stopped`, clears `isPreparing` (important: see "Prepare/Stop race" below), calls `videoPlayer.Stop()` (releases the decoder), releases the budget slot, and shows the cached fallback texture.
- **`StopAndReset()`** — `Stop()` plus clears `videoPath`/`isReady`/loop state entirely, so a later `Play()` is a no-op. Used when leaving a cabinet with no attraction video configured, so the BT's video loop can't accidentally restart an AGEBasic-loaded clip from the previous session.
- **`ChangeVideo(path, invertX, invertY)`** — swaps the clip (used by AGEBasic's `VIDEOLOAD`), releasing the current slot and resetting prepared/ready state so the next `Play()` re-prepares the new file.
- **`PrepareCompleted`** — clears `isPreparing`, sets `isReady`, then honors `desired`: `Playing` → calls `Play()` to actually start; `Stopped` → stops and releases the slot (defensive — normally `Stop()` already canceled the prepare before this fires); `Paused` → does nothing, stays prepared and ready with its slot intact.
- **`ErrorReceived`** — clears `isPreparing`/`isReady`, releases the slot, falls back to the cached thumbnail if one exists.

### Prepare/Stop race (fixed)

Calling `Stop()` while a Unity `Prepare()` is in flight cancels the prepare — `PrepareCompleted` then never fires. Previously this left `isPreparing == true` forever, and every future `Play()` call bailed out at the top-level guard, permanently killing that cabinet's attraction video until the whole room reloaded. `Stop()` and `ErrorReceived` now explicitly clear `isPreparing`, so a subsequent `Play()` re-prepares normally. This was the single biggest source of "erratic" attraction-video behavior — walking away from a cabinet mid-prepare (a roughly one-second window) reliably triggered it.

### Fallback texture when not playing

When a video is stopped, denied a slot, or evicted, the screen shows:
1. The cached first-frame thumbnail (`TextureCache.CachedTexture`), if one has been captured for this clip, or
2. `ShaderScreenBase.StandByTexture` (`Resources/Cabinets/OutOfOrder/Prefab/CRTOff`) — the generic "CRT off" image — if nothing is cached yet.

The first frame is captured lazily: the first time a clip actually plays and no thumbnail exists yet, `Play()` triggers `TextureCache.Load()`, which does an `AsyncGPUReadback` of the live video texture, encodes it to PNG, and writes it to disk (`videoPath + ".png"`) so future visits show a real frame instead of the standby image while the video decodes.

---

## `AttractVideoBudget.cs` — global decoder cap

`Assets/curif/LibRetroWrapper/AttractVideoBudget.cs`. A static manager (same shape as `CabinetTextureCache`'s LRU: static, lazily configured, no `Update()` loop — see [`texture_caching_and_compression.md`](./texture_caching_and_compression.md)) that limits how many `GameVideoPlayer`s can hold an active decoder ("slot") at once.

### Why

Every attraction-video screen owns its own Unity `VideoPlayer`, and cabinets stay resident (only suspended, never destroyed) once a room loads them. Without a cap, standing near many video cabinets spins up many simultaneous hardware decoders — a real source of erratic playback and performance problems on Quest.

### Policy

- **Cap**: 5 concurrent slots on Quest 3, 3 on Quest 2/other devices (`DeviceController.IsQ3`), or a configured value (`cabinet.max-attract-videos` in the global yaml, 0 = auto, clamped 1–10). Same auto-by-device pattern `CabinetTextureCache` uses for its MB budget.
- **A slot is held from the moment a video starts preparing until it's stopped** — not just while actively playing. This is what actually limits concurrent codec pressure; a paused-but-prepared video still occupies hardware resources.
- **Granting**: a request is granted for free if the requester already holds a slot (so BT retries are cheap), always granted if the requester is *pinned* (see below), granted immediately if under the cap, and otherwise attempts to **evict** the worst current holder.
- **Eviction target**: among non-pinned holders, paused videos are evicted before playing ones; ties are broken by distance — farthest from the player loses.
- **Hysteresis**, to prevent slot thrashing as the player moves: eviction only happens if the requester is at least **0.75 m** closer to the player than the victim, *and* the victim has held its slot for at least **3 seconds**.
- **Denied/evicted** players show their cached fallback texture (`GameVideoPlayer.ShowFallbackTexture()`) and simply retry next BT tick (roughly 1 s later) — there's no separate retry mechanism, the existing polling behavior tree already does it.
- **No player found** (editor/desktop, no `OVRPlayerControllerGalery` in the scene): the budget degrades to first-come-first-served up to the cap and never evicts, so nothing breaks outside VR.

### Pinning — AGEBasic sessions always win

When a player inserts a coin and an AGEBasic program takes over a screen, that screen's `GameVideoPlayer` is **pinned** (`videoPlayer.BudgetPin(true)` in `AGEBasicScreenController`'s "Run main program" BT node). A pinned player:
- is always granted a slot, even if that pushes the active count one over the cap (only one program session can run at a time, since only one coin slot can be active), and
- is never chosen as an eviction victim.

The rationale: attraction videos are ambient decoration, but AGEBasic-controlled video (`VIDEOLOAD`/`VIDEOPLAY` inside a paid session) is game content the player is actively interacting with — it must never lose to a nearby cabinet's idle attract loop. The pin is released (`BudgetPin(false)`) when the program ends (the BT's "END Program" node) or the cabinet is suspended for a room transition (`SuspendAttractAndPlaybackForTransition()`).

### API

```csharp
AttractVideoBudget.Configure(int maxFromConfig);     // 0 = auto by device
AttractVideoBudget.RequestSlot(GameVideoPlayer p);   // true = may prepare/play
AttractVideoBudget.ReleaseSlot(GameVideoPlayer p);   // idempotent
AttractVideoBudget.Pin(GameVideoPlayer p);
AttractVideoBudget.Unpin(GameVideoPlayer p);
AttractVideoBudget.ActiveCount;                      // for diagnostics
AttractVideoBudget.Status();                         // one WriteConsole line
```

`GameVideoPlayer` never calls these directly except through its own `RequestSlot`/`ReleaseSlot`/`BudgetPin` integration — controllers only ever call `videoPlayer.BudgetPin(bool)`, never touch `AttractVideoBudget` directly.

### Configuration

```yaml
cabinet:
  max-attract-videos: 5   # 0 or omitted = auto (5 on Quest 3, 3 otherwise), max 10
```

### Diagnostics

Every grant, denial, eviction, pin, and unpin logs a `[AttractVideoBudget]` line via `ConfigManager.WriteConsole`. `AttractVideoBudget.Status()` is also called from `Init.cs`'s existing 15-second periodic memory snapshot (alongside `ResourceCacheManager.LogAllCacheStatus`), so `active: N/cap pinned: N` shows up in every device log capture without extra instrumentation. On a Quest device: `adb logcat -s Unity | grep AttractVideoBudget`.

---

## The Behavior Tree video-control nodes

Both controllers tick a "video control" sequence roughly every second (`runBT()` coroutine, `yield return new WaitForSeconds(1f)`).

### `AGEBasicScreenController` — "Video Player control"

```
.Selector()
  .Sequence()
    .Condition("Player not in the zone?", () => !playerIsInSomePosition())
    .Do("Stop video player", () => videoPlayer.Stop())
  .End()
  .Sequence()
    .Condition("A program is not running?", () => !cabinetAGEBasic.AGEBasic.IsRunning())
    .Condition("... (background)?", () => !cabinetAGEBasic.AGEBasic.IsRunningInBackground())
    .Condition("Is Player looking the screen", () => isPlayerLookingAtScreen4())
    .Do("Play video player", () => videoPlayer.Play())
  .End()
  .Do("Pause video player", () => videoPlayer.Pause())
.End()
```

Player not in any registered position → stop. In a position, no AGEBasic program running, and looking at the screen → play. Otherwise (in position but not looking, or a program is running) → pause.

### `LibretroScreenController` — "Video/Audio Player control"

```
.Selector()
  .Sequence()
    .Condition("Running any game or Player not in the zone?",
               () => LibretroMameCore.GameLoaded || !playerInTheZone)
    .Do("Stop video and audio player", () => { videoPlayer.Stop(); audioPlayer.Stop(); })
  .End()
  .Sequence()
    .Condition("Player in the zone?", () => playerInTheZone)
    .Condition("Not running any game", () => !LibretroMameCore.GameLoaded)
    .Selector()
      .Sequence()
        .Condition(() => distanceToPlayer <= DistanceMaxToPlayerToActivateVideo)
        .Condition(() => isPlayerLookingAtScreenZone())
        .Do("Play video", () => { audioPlayer.Stop(); videoPlayer.Play(); })
      .End()
      .Sequence()
        .Condition(() => distanceToPlayer <= DistanceMaxToPlayerToActivateAudio)
        .Do("Play audio clip", () => { videoPlayer.Pause(); audioPlayer.Play(); })
      .End()
```

A game running (MAME/LibRetro loaded) or the player out of the zone → stop both. Close enough and looking → play video (and stop the separate ambient audio clip). Close enough to hear but not to trigger video → pause video, play audio only.

Since `GameVideoPlayer.Play()`/`Pause()`/`Stop()` are all idempotent now, calling them every tick regardless of current state is safe and is the intended usage pattern — the budget's `RequestSlot` free-retry-for-holders design relies on this polling behavior.

---

## Cabinet suspend/resume interaction

`CabinetsController.cs`'s `run()` coroutine polls every `TimeToWaitBetweenChecks` (0.5 s) and calls `SuspendReplacement()`/`ResumeReplacement()` based on `AgentScenePosition` "load"/"unload" trigger volumes. These call every `ISuspendableCabinetScreen`'s `SuspendAttractAndPlaybackForTransition()` / `EnsureAttractLoopRunning()`.

Cabinets are **never destroyed** once loaded into a room — "unload" only stops the BT coroutine and the video/game, freeing decoder and CPU resources without tearing down the GameObject (texture memory is separately managed by `CabinetTextureCache`'s LRU — see [`texture_caching_and_compression.md`](./texture_caching_and_compression.md)). `SuspendAttractAndPlaybackForTransition()` calls `videoPlayer.Stop()` unconditionally, which also releases the `AttractVideoBudget` slot — so suspended cabinets never hold a slot hostage while off-screen.

---

## Common failure modes

| Symptom | Cause | Status |
|---|---|---|
| A cabinet's attraction video never plays again after the player walks away and back | Prepare/Stop race: `Stop()` mid-prepare left `isPreparing` stuck `true` forever | Fixed — `Stop()`/`ErrorReceived` now clear `isPreparing` |
| Screen frozen on the cached thumbnail while video audio keeps playing | Async thumbnail load (`textureCache.OnTextureLoaded`) fired after playback started and stomped the live video texture | Fixed — the callback now checks `IsActuallyPlaying` before activating |
| Video plays and immediately gets paused/stopped, or flickers, when many cabinets are near | No cap on concurrent decoders; BT calls racing `PrepareCompleted`'s auto-`Play()` | Fixed — `AttractVideoBudget` caps concurrency; `desired` state resolves the BT-vs-PrepareCompleted race |
| An AGEBasic game's video gets evicted by an attraction video from a neighboring cabinet | (would only happen without pinning) | Prevented — game sessions pin their screen via `BudgetPin(true)` |
| `[AttractVideoBudget]` shows `active` above the configured cap | Expected when a pinned session is active — the cap only bounds non-pinned slots | Not a bug — check the `pinned` count in the same log line |

---

## Summary of key files

- **`Assets/curif/LibRetroWrapper/GameVideoPlayer.cs`** — per-screen Unity `VideoPlayer` wrapper; owns the play/pause/stop state machine and the fallback-texture logic.
- **`Assets/curif/LibRetroWrapper/AttractVideoBudget.cs`** — the global decoder slot manager (cap, eviction, pinning).
- **`Assets/curif/LibRetroWrapper/TextureCache.cs`** — bridges a video's on-disk first-frame PNG thumbnail into the `CabinetTextureCache` LRU.
- **`Assets/curif/LibRetroWrapper/AgentScenePosition.cs`** — trigger-volume player presence detection.
- **`Assets/curif/LibRetroWrapper/AGEBasicScreenController.cs`** / **`LibretroScreenController.cs`** — per-cabinet Behavior Trees driving proximity/gaze checks into `Play`/`Pause`/`Stop`, and the pin/unpin calls around AGEBasic sessions.
- **`Assets/curif/LibRetroWrapper/CabinetsController.cs`** / **`ISuspendableCabinetScreen.cs`** — room-level suspend/resume that starts/stops each cabinet's BT and releases video resources when the player leaves a cabinet's load/unload zone.
- **`Assets/curif/LibRetroWrapper/ConfigInformation.cs`** — `cabinet.max-attract-videos` yaml key.
