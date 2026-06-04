# Assets/ramiro/Scripts — Mixed Reality Subsystem

All C# scripts for the Age of Joy **Mixed Reality (MR)** subsystem live here,
organised by feature domain. Each folder is a self-contained concern; scripts
in one folder may reference scripts in other folders freely — there are no
assembly-definition boundaries between them.

---

## Folder Map

### `Core/` — Orchestration & Scene Management
The backbone of the MR mode. Controls the VR↔MR state machine, passthrough,
scene loading, and player presence.

| Script | Role |
|--------|------|
| `MixedRealityManager` | Singleton orchestrator; owns `CurrentMode`, fires `OnModeChanged` |
| `ExperienceMode` | Enum: `VR` / `MR` |
| `MixedRealityBootstrap` | `[RuntimeInitializeOnLoadMethod]` — installs the MR system on scene load |
| `MRSceneBootstrap` | Wires up scene-level MR objects on startup |
| `MRSceneTransition` | Handles async scene load/unload for VR↔MR travel |
| `MRPassthroughController` | Enables / disables Meta passthrough layer |
| `MRVrSystemsGate` | Suspends VR locomotion while MR is active |
| `MRCameraRigShim` | Bridges XROrigin / OVRPlayerController between modes |
| `MRSceneLoadState` | Tracks current scene-load phase |
| `MRScenePermissions` | Checks required platform permissions at runtime |
| `MRTransitionLog` | Structured log helper for mode-transition events |
| `MRAgentPlayerPresence` | Keeps the player avatar consistent across mode switches |
| `MRRoomInfoUI` | Overlay that shows room / anchor debug info |
| `MRModeInput` | Input actions that switch VR↔MR mode |
| `MREditMenuInput` | Input actions for the in-MR edit / configuration menu |

---

### `Layout/` — Cabinet Registry & Spawn
Manages the `mr-layout.yaml` v3 file: reads, writes, and spawns physical
arcade cabinets in MR space.

| Script | Role |
|--------|------|
| `MRLayoutRegistry` | Loads `mr-layout.yaml`; spawns / moves / removes cabinets |
| `MRPlacedCabinet` | Runtime data for a single cabinet placed in MR |
| `MRLibretroWarmup` | Pre-warms LibRetro cores when entering MR (reduces first-launch stutter) |
| `MRGameCabinetAttractSetup` | Configures attract-mode behaviour for MR-placed cabinets |

---

### `Environment/` — Props, Surfaces & Lighting
Manages environment props (`mr-environment-layout.yaml`), real-world surface
detection, and MR-specific lighting adjustments.

| Script | Role |
|--------|------|
| `MREnvironmentRegistry` | Loads `mr-environment-layout.yaml`; spawns / removes props |
| `MREnvironmentCatalog` | Asset catalog of available environment prop prefabs |
| `MRCatalogBootstrap` | Initialises the catalog at startup |
| `MREnvironmentSurfaces` | MRUK surface queries (floor, wall, ceiling detection) |
| `MRMrEnvironmentLighting` | Adjusts ambient lighting to match the real room |
| `MRPlacedEnvironment` | Runtime data for a single environment prop placed in MR |

---

### `Configuration/` — CRT Edit Menu & Placement
All scripts for the in-headset CRT-style configuration menus: adjusting cabinet
positions, placement ray, global scale / floor offset, and anchor resolution.

| Script | Role |
|--------|------|
| `MRConfigurationController` | Main controller for the CRT configuration screens |
| `MRConfigurationCabinetController` | Per-cabinet configuration screen logic |
| `MRConfigurationUI` | UI bindings and rendering for the configuration menus |
| `MRPlacementRayController` | Raycast-based placement tool (Floor / Wall / Ceiling) |
| `MRPlacementProfile` | Stores placement constraints for a cabinet or prop type |
| `PlacementOrientation` | Enum: `Floor`, `Wall`, `Ceiling` |
| `MRAdjustmentsSettings` | Global scale + Y-floor offset; persisted via PlayerPrefs |
| `MRAnchorPoseResolver` | Resolves 6-DOF anchor poses from MRUK / OpenXR |

---

### `PhoneBooth/` — VR ↔ MR Immersive Travel
Everything related to the phone booth journey mechanic: grabbing the handset,
playing travel VFX, managing travel state, and the VR↔MR portal logic.

| Script | Role |
|--------|------|
| `MRPhoneBoothPortal` | Coroutine orchestrator for the full VR↔MR travel sequence |
| `PayphoneHandsetGrab` | XR Grab Interactable extension; detects cradle pick-up / return |
| `MRPhoneBoothTravelVfx` | Door VFX, glass opacity swap, shake, passthrough control during travel |
| `MRPhoneBoothSettings` | ScriptableObject / static config (audio clips, timing values) |
| `MRPhoneBoothVisibility` | Shows / hides the CRT **PHONE BOOTH** button based on MR state |
| `PhoneBoothJourneyDirection` | Enum: `VrToMr`, `MrToVr` |
| `PhoneBoothTravelState` | State machine enum for the travel sequence phases |

---

### `DevTools/` — Development & Testing Utilities
Scripts used only during development and QA. **Not shipped in release builds**
(wrap any editor-only calls with `#if UNITY_EDITOR` if needed).

| Script | Role |
|--------|------|
| `MREditorMrSimulator` | Simulates MR environment in the Unity Editor (no headset required) |
| `MRTestGameCabinetSpawn` | Spawns a test cabinet in the `TestMRmanager` scene at runtime |
| `MRExampleCabinetSeed` | Seeds the layout registry with example cabinet data for quick tests |

---

### `Data/` — Data Models
Plain C# data classes and structs that represent serialised layout data.
No `MonoBehaviour` or Unity-specific attributes here.

| Script | Role |
|--------|------|
| `MRLayout` | Data model for a single entry in `mr-layout.yaml` v3 |

---

## Key External References

| File | Purpose |
|------|---------|
| `cabinetsdb/mr-layout.yaml` | Persisted cabinet placement (v3 schema) |
| `cabinetsdb/mr-environment-layout.yaml` | Persisted environment prop placement (v1 schema) |
| `Resources/ramiro/PrefabsEnvironment/ConfigurationCabinetMiniMR` | CRT configuration cabinet prefab |
| `Resources/Decoration/PhoneBooth/PF_Payphone` | Phone booth prefab |
| `MIXED_REALITY_DESIGN.md` | Full MR design specification |
| `MR_PHONE_BOOTH_TRANSITION.md` | Phone booth travel spec |
