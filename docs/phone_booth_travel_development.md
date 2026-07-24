# Phone Booth VR ↔ MR Travel — Developer Guide

## Overview

The phone booth (`PF_Payphone`) is the in-game portal between the **VR arcade** (fully rendered virtual world) and **MR mode** (Meta Quest passthrough with virtual objects anchored to the real room). The player grabs the handset, an immersive travel sequence plays, and the experience transitions seamlessly.

Two directions exist:
- **VR → MR**: player grabs handset in the arcade exterior → MR passthrough world
- **MR → VR**: player grabs handset inside the MR phone booth → VR arcade

---

## Prefab and Scene Setup

- **Prefab**: `Resources/Decoration/PhoneBooth/PF_Payphone`
- **Scene booth**: lives in `IntroGalleryExterior` scene (the VR arcade exterior)
- **Traveler instance**: when the transition begins, the booth is reparented to `MixedRealityManager` (DontDestroyOnLoad) and becomes the "traveler" — it persists across scene loads. It is destroyed after arrival.
- **MR booth**: in MR the traveler is placed on the real room floor via `PlaceOnMrFloor`. Its pose is saved to `PlayerPrefs` as `MR.Booth.Position/Rotation`.

**Required child objects on `PF_Payphone`:**

| Child name | Purpose |
|---|---|
| `SM_Payphone_Handset` | XR-grabbable handset — has `PayphoneHandsetGrab` |
| `ZoneBlackout` | `BoxCollider` (trigger) — head-fade zone. Camera blackout ramps as the player's head exits this volume. |
| `DoorPhonebooth` | Travel door (disabled in prefab, enabled during journey) |
| `AudioCue` | `AudioSource` — plays when handset is first grabbed |
| `AudioSpaceshipEngine` | `AudioSource` — plays during the travel journey |
| `AudioExplosion` | `AudioSource` — plays on arrival |
| `ParticleSmoke` | `ParticleSystem` — burst on arrival |

---

## Key Scripts

| Script | Location | Role |
|---|---|---|
| `MRPhoneBoothPortal` | `Scripts/PhoneBooth/` | Main controller: owns travel state, coroutines, VFX coordination |
| `PayphoneHandsetGrab` | `Scripts/PhoneBooth/` | XRGrabInteractable wrapper; notifies portal on grab/release |
| `MRPhoneBoothTravelHeadFade` | `Scripts/PhoneBooth/` | Per-frame head-position monitoring against ZoneBlackout; drives `cullingMask=0` + black clear color |
| `MRPhoneBoothTravelVfx` | `Scripts/PhoneBooth/` | Opaque glass swap, interior glow pulse, cabinet shake coroutine |
| `MRPhoneBoothSettings` | `Scripts/PhoneBooth/` | `MR/phone-booth.yaml` — visibility, auto-hide, MR pose |
| `MRPhoneBoothVisibility` | `Scripts/PhoneBooth/` | Show/hide the MR booth |
| `PhoneBoothTravelState` | `Scripts/PhoneBooth/` | Serializable snapshot of player pose relative to booth root at travel start |
| `PhoneBoothJourneyDirection` | `Scripts/PhoneBooth/` | Enum: `ToMR` / `ToVR` |
| `MixedRealityManager` | `Scripts/Core/` | Owns VR↔MR mode transitions; called by portal after travel sequence |
| `MRPassthroughController` | `Scripts/Core/` | Manages `OVRPassthroughLayer`, camera clear flags, fade sphere |
| `MRSceneTransition` | `Scripts/Core/` | Loads/unloads VR additive scenes |
| `MRVrSystemsGate` | `Scripts/Core/` | Suspends/resumes LibRetro, cabinet controllers, locomotion |
| `MRPhoneBoothTransitionSequence` | `Scripts/Configuration/` | Static step-order enums; validated against each other |
| `MRRuntimeSettings` | `Scripts/Configuration/` | Inspector-configurable timings and step lists (FixedScene GameObject) |

---

## VR → MR Travel: Step-by-Step

### 1. Handset grab

`PayphoneHandsetGrab.OnGrabbed` → `MRPhoneBoothPortal.NotifyHandsetGrabbedForTravel` → `TryStartTravelFromHandset` → `BeginTravelToMR`.

`BeginTravelToMR` captures `PhoneBoothTravelState` (player position/rotation relative to booth root) and calls `MixedRealityManager.StartPhoneBoothVrToMrTravel(portal, travelState)`.

### 2. Scan gate (room scan check)

`MixedRealityManager.PhoneBoothVrToMrTravelWithScanGateRoutine` immediately calls `portal.StartImmersiveTravelToMrAfterScanGate()` — the MRUK room probe is deferred to the `BeginJourneyVisuals` step (see below).

### 3. Immersive travel sequence — `PlayTravelThen`

`MRPhoneBoothPortal` runs `PlayTravelThen(() => MixedRealityManager.EnterMRFromPhoneBooth(this))`. The configurable step list (`MRRuntimeSettings.ImmersiveTravelSteps`, defaulting to `MRPhoneBoothTransitionSequence.DefaultImmersive`) runs:

| Step | What happens |
|---|---|
| `HandsetAudioCue` | Plays `AudioCue` source; waits `immersiveHandsetCueStepDurationSeconds`. After this step, releasing the handset no longer cancels travel. |
| `BeginJourneyVisuals` | `MRPhoneBoothTravelVfx.BeginJourneyVisuals` — swaps to opaque glass, increases glow, starts cabinet shake, opens travel door. Also calls `EnsureMrukReadyAtJourneyStart()` (MRUK room probe / Space Setup if needed) and `SuspendPlayerLocomotionForPhoneBoothVrToMrTravel()`. |
| `FadeSphereIn` | Optional hold (`immersiveFadeSphereStepDurationSeconds`). The SM_FadeSphere fade-in animation may run here if triggered. |
| `SpaceshipEngine` | Plays `AudioSpaceshipEngine`; waits `immersiveSpaceshipEngineStepDurationSeconds`. |
| `EndJourneyVisuals` | `MRPhoneBoothTravelVfx.EndJourneyVisuals(ToMR)` — stops shake, restores glow. Glass and door are kept opaque until passthrough is confirmed. Optional hold (`immersiveEndJourneyStepDurationSeconds`). |

**Head fade** (`MRPhoneBoothTravelHeadFade`) runs in parallel from the start of `PlayTravelThen`. It monitors the camera position against the `ZoneBlackout` BoxCollider every frame. While the player's head is outside the zone:
- `xrCamera.cullingMask = 0` (renders nothing)
- `xrCamera.clearFlags = SolidColor`, `backgroundColor = black`

This creates a smooth blackout as the player "steps out" of the booth during travel.

**`finally` block of `PlayTravelThen`**: calls `StopTravelHeadFade(force: false)` → `EndMonitoring()` → `RestoreCameraState()`. This restores the camera to its pre-monitoring state and sets `activeTravelFade = null` (`IsTravelBlackoutActive = false`). The travel callback (`MixedRealityManager.EnterMRFromPhoneBooth`) has already been started as a new coroutine before this finally runs.

### 4. MR transition — `EnterMRFromPhoneBoothCoroutine`

Runs on `MixedRealityManager` (DontDestroyOnLoad). Sequence:

```
MRScenePermissions.EnsureGranted()          — camera/microphone permission
MRSceneHost.PrepareForMr()                  — enable OVRManager + MRUK in FixedScene
MRVrSystemsGate.SuspendForMR()              — stop LibRetro, disable CabinetsControllers, suspend locomotion
WaitForShutdownBeforeSceneUnload()          — 3 frames: let audio threads settle

EnablePassthroughAdoptBoothThenUnload():
  BeginTransitionBlackout()                 — camera: SolidColor + black
  EnablePassthroughWhenReady(keepCameraBlack:true)
    ├── WaitForPassthroughSystemReady()     — poll OVRManager.IsInsightPassthroughInitialized()
    ├── ApplyPassthroughRendering()         — enable OVRPassthroughLayer (Underlay), color=clear
    ├── [keepCameraBlack] force black       — ← BUG FIX: overrides Color.clear while VR scenes still loaded
    └── WaitUntilPassthroughLayerVisible()  — wait for passthroughLayerResumed event (up to 5 s)
  AdoptAsTraveler(portal, MRManager)        — reparent booth to DDOL; travelHeadFade moves with it
  UnloadVrScenes()                          — unload IntroGalleryExterior + IntroGallery (scene list order)
  RefreshPassthroughAfterSceneUnload()      — re-applies passthrough rendering; now sets Color.clear safely

EnsureMRSpaceOrigin()                       — create world-space origin for MR layout
environmentSurfaces.ProbeWhenReady()        — MRUK floor/ceiling detection
SetMode(ExperienceMode.MR)                  — ApplyMrColocatedScale (1:1 tracking scale)

mrLighting.Spawn()                          — MR scene lighting
mrEffectMesh.Spawn()                        — effect mesh (wall/floor boundaries)
layoutRegistry.SpawnAllAsync()              — spawn MR cabinets from objects-layout.yaml
environmentRegistry.SpawnAllAsync()         — spawn MR env props
MRConfigurationCabinetController.SpawnAtMrOrigin()

RefreshMrPosesWhenReady()                   — up to 20 frames: sync spawned poses to MRUK anchors
FinalizeEnvironmentAfterEnterMr()           — wait for usable MRUK room, apply room skins

portal.PlaceOnMrFloor()                     — position traveler booth on real room floor
portal.PlayTravelArrivalExplosionAndRestoreGlassDoor():
  ├── PlayArrivalExplosionSmoke()           — particle burst
  ├── AudioExplosion source plays
  ├── WaitAfterArrivalExplosionBeforeGlassDoor()
  ├── RestoreTravelGlassAndDoor()           — swap back transparent glass, close travel door
  └── EndTravelBlackout()                   — StopTravelHeadFade(force:true) → RestoreCameraState

portal.NotifyHandsetsTravelComplete()       — handset snaps back to cradle
```

> **Critical: `keepCameraBlack` flag** — `EnablePassthroughWhenReady` is called with `keepCameraBlack: true` in this path (`EnablePassthroughAdoptBoothThenUnload`). `ApplyPassthroughRendering` sets `xrCamera.backgroundColor = Color.clear` when `IsTravelBlackoutActive = false` (which is the case here, since `StopTravelHeadFade` already ran). Without the override, VR geometry would render on top of the passthrough underlay before `UnloadVrScenes` completes, making the arcade visible through the real world. The black is released when `RefreshPassthroughAfterSceneUnload` runs after VR scenes are gone.

---

## MR → VR Travel: Step-by-Step

### 1. Handset grab in MR

Same grab path → `BeginTravelToVR`. Immediately calls `MRVrSystemsGate.SilenceAllCabinetScreensForPhoneBoothTravelToVr()` (stops LibRetro, suspends attract loops).

### 2. Immersive travel sequence

Same `PlayTravelThen` (direction = `ToVR`). The same `ImmersiveTravelStep` list runs, with the difference that `BeginJourneyVisuals` calls `MixedRealityManager.DisablePassthroughForPhoneBoothTravel()` (disables the passthrough layer immediately, so you see the opaque glass instead of the real room).

### 3. VR transition — `EnterVRFromPhoneBoothCoroutine`

Runs the configurable `MrToVrReturnStep` list (`MRRuntimeSettings.MrToVrReturnSteps`), default order:

| Step | What happens |
|---|---|
| `CacheArrivalExplosionClip` | Resolve the explosion `AudioClip` while traveler still exists |
| `RestoreGalleryPlayerPose` | Restore VR player position saved before the VR→MR travel |
| `RefreshCameraOffset` | Optional `AdjustCameraYOffset` (disabled by default — causes Y drift) |
| `ApplyTravelStateFallback` | If pose restore failed: apply `PhoneBoothTravelState` to place player near booth |
| `FinalizeHandsetOnSceneBooth` | Move handset from traveler to scene booth; find scene portal |
| `WaitFramesBeforeArrivalEffects` | Optional delay (`secondsBeforeArrivalExplosion`) |
| `ArrivalExplosionAndSmoke` | Play explosion on scene booth; restore glass + door; `EndTravelBlackoutEverywhere()` |
| `MrEnvironmentCleanup` | Despawn MR cabinets, env props, effect mesh, lighting; clear MRUK scene; destroy MRSpaceOrigin |
| `ReloadVrScenes` | Additive load IntroGalleryExterior + IntroGallery; `SilenceCabinetScreensAfterVrSceneReload()` |
| `EnableVrModeAndLocomotion` | `RebindCameraAndDisablePassthrough()`, `SetMode(VR)`, `ResumeVrSystemsExceptLocomotion()`, re-enable locomotion after optional delay |
| `FinalPassthroughRebind` | Safety: `RebindCameraAndDisablePassthrough()` again |

> **Step order constraints** (validated by `MRPhoneBoothTransitionSequence.ValidateMrToVr`):
> - `ReloadVrScenes` must be **after** `MrEnvironmentCleanup` (prevents gallery cabinet audio during travel)
> - `CacheArrivalExplosionClip` must be before `FinalizeHandsetOnSceneBooth` (traveler is destroyed after finalize)
> - `FinalizeHandsetOnSceneBooth` must be before `ArrivalExplosionAndSmoke`
> - `ApplyTravelStateFallback` must be after `RestoreGalleryPlayerPose`

---

## Passthrough Rendering Details

`MRPassthroughController` manages the `OVRPassthroughLayer` component, which is added dynamically to the XR camera.

| State | `overlayType` | `backgroundColor` | `cullingMask` |
|---|---|---|---|
| VR (normal) | — (layer disabled/destroyed) | Skybox | All layers |
| Transition blackout | — | SolidColor + black | All layers |
| Head fade active | — | SolidColor + black | 0 (render nothing) |
| MR passthrough (active) | `Underlay` | SolidColor + `Color.clear` | All layers |

**Why Underlay?** The passthrough renders *behind* Unity's frame buffer. `Color.clear` (alpha=0) background makes the buffer transparent where no geometry renders, revealing the passthrough. Opaque MR objects render on top, occluding the real world where intended.

**`SM_FadeSphere`**: a sphere around the player used for fade animations (FadeIn/FadeOut triggers via `Animator`). During passthrough, all its renderers are disabled (not destroyed) by `ClearFadeSphereForPassthrough()`. They are re-enabled on VR return by `RestoreFadeSphereForVr()`.

**`EnsureInsightPassthroughEnabled()`**: sets `OVRManager.instance.isInsightPassthroughEnabled = true`. Must be called before the passthrough layer can initialize. The `OVRManager` child in the `MR` scene root (FixedScene) is activated by `MRSceneHost.PrepareForMr()`.

---

## Head Fade (`MRPhoneBoothTravelHeadFade`)

Attached to `PF_Payphone` at travel start via `EnsureTravelHeadFade()`.

Two modes:
- **Monitoring** (`BeginMonitoring`): evaluates `DistanceOutsideFadeZone(eyePosition)` every frame; cubic ease-in ramp over `exteriorFadeRangeMeters`. Used for VR→MR.
- **Forced** (`EngageForcedTravelBlackout`): fixed fade=1 regardless of head position. Used when locking the blackout explicitly.

`ApplyCameraBlackout(fade)`:
- `fade > 0.01`: `cullingMask = 0`, `clearFlags = SolidColor`, `backgroundColor = black`
- `fade ≤ 0.01`: `RestoreCameraState()` (restores saved values from before monitoring started)

`IsTravelBlackoutActive` (static): `activeTravelFade != null && (forcedTravelBlackout || (monitoring && currentFade > 0.01f))`

> **Ownership transfer**: the head fade component lives on the portal's `GameObject`. When `AdoptAsTraveler` reparents the portal to `MixedRealityManager` (DDOL), the head fade moves with it and keeps running. `EndTravelBlackoutEverywhere()` → `MRPhoneBoothTravelHeadFade.EndActiveTravelFade()` → `EndMonitoring()` is the final cleanup, called from `PlayTravelArrivalExplosionAndRestoreGlassDoor`.

---

## Scene Management

`MRSceneTransition` tracks which scenes were unloaded for reload on MR→VR return.

**Unload order** (VR→MR):
1. `IntroGalleryExterior` (contains the scene booth portal)
2. `IntroGallery` (main arcade hall)
3. All other additive VR scenes

**Reload order** (MR→VR): same list, additive load; `ActivateBestVrScene()` sets `IntroGallery` (or `IntroGalleryExterior` as fallback) as the active scene.

In the Unity Editor, `IntroGallery` is hidden (root GameObjects deactivated) instead of unloaded by default (`MRRuntimeSettings.EditorHideIntroGalleryInsteadOfUnload`). Set `editorForceFullVrSceneUnloadOnMrEnter = true` to test the full unload path.

---

## Configuration (`MRRuntimeSettings`)

Placed on a GameObject in `FixedScene`. DontDestroyOnLoad singleton.

| Field | Default | Purpose |
|---|---|---|
| `immersiveTravelSteps` | DefaultImmersive | Reorderable step list for handset journey |
| `mrToVrReturnSteps` | DefaultMrToVr | Reorderable step list for MR→VR return |
| `immersiveHandsetCueStepDurationSeconds` | 2.0 | Audio cue hold time |
| `immersiveSpaceshipEngineStepDurationSeconds` | 3.5 | Spaceship engine hold time |
| `arrivalExplosionStepDurationSeconds` | 1.5 | Explosion audio hold time |
| `secondsAfterArrivalExplosionBeforeGlassAndDoor` | 0.8 | Delay before glass/door restore |
| `rememberVrPoseOnPhoneBoothTravelToMr` | true | Save VR player pose before VR→MR |
| `restoreVrPoseOnPhoneBoothReturn` | true | Restore VR player pose on MR→VR |
| `refreshCameraOffsetAfterPhoneBoothReturn` | false | **Keep false** — causes ~6 cm Y drift |
| `autoEnterMrOnFixedSceneBoot` | false | Skip IntroGallery and enter MR on boot |

---

## Known Issues / Design Decisions

**Colocated scale**: VR uses a 0.9 player-scale factor (comfort); MRUK anchors are at 1:1. `SetMode(MR)` calls `PlayerController.EnterMrColocatedScale()` to force 1:1; `SetMode(VR)` restores it. Applying `ApplyPhoneBoothTravelState` (teleport player to booth) is intentionally disabled in the MR entry path — moving the rig would shift all virtual content relative to the passthrough.

**Glass kept opaque through passthrough setup**: `MRPhoneBoothTravelVfx.EndJourneyVisuals(ToMR)` does NOT restore the glass on VR→MR (only restores glow and stops shake). The glass is restored after arrival by `RestoreGlassAfterMrTransition()` inside `RestoreTravelGlassAndDoor()`.

**`StopTravelHeadFade` timing**: the head fade `finally` block in `PlayTravelThen` runs while `EnterMRFromPhoneBoothCoroutine` is already queued. This means `IsTravelBlackoutActive` becomes `false` before `ApplyPassthroughRendering` runs. The `keepCameraBlack: true` parameter on `EnablePassthroughWhenReady` compensates — without it, `xrCamera.backgroundColor = Color.clear` with VR scenes still loaded causes VR geometry to show through the passthrough underlay.

**`refreshCameraOffsetAfterPhoneBoothReturn` disabled**: `AdjustCameraYOffset` runs on the XR rig and was found to shift it ~6 cm in Y, conflicting with MRUK world-lock on phone booth MR→VR transitions. Left disabled; the comment in `MRRuntimeSettings.cs` documents the reason.

---

## Debugging

All key transition steps write to `MRTransitionLog` and `ConfigManager.WriteConsole`. On device, pipe logcat and filter for:
- `[MixedRealityManager]`
- `[MRPhoneBoothPortal]`
- `[MRPassthroughController]`
- `[MRPhoneBoothTravelHeadFade]`
- `[MRVrSystemsGate]`
- `[MRSceneTransition]`
- `[blackout]` — logs when head fade activates/deactivates

`MRTransitionLog.LogFilePath` is printed to the console on MR entry completion. It logs every named step with a timestamp for post-mortem analysis.
