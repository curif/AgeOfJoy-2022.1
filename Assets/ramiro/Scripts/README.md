# Assets/ramiro/Scripts — Mixed Reality Subsystem

All C# scripts for the Age of Joy **Mixed Reality (MR)** subsystem live here,
organised by feature domain. Each folder is a self-contained concern; scripts
in one folder may reference scripts in other folders freely — there are no
assembly-definition boundaries between them.

**Design docs:** [`MIXED_REALITY_DESIGN.md`](../../../MIXED_REALITY_DESIGN.md) · [`MR_PHONE_BOOTH_TRANSITION.md`](../../../MR_PHONE_BOOTH_TRANSITION.md)

---

## Folder Map

### `Core/` — Orchestration & Scene Management

The backbone of MR mode: state machine (`VR` / `MR` / `MR_EDIT`), passthrough,
scene loading, MRUK scan flow, and player rig bridging.

| Script | Role |
|--------|------|
| `MixedRealityManager` | Singleton orchestrator; owns `CurrentMode`, fires `OnModeChanged` |
| `ExperienceMode` | Enum: `VR`, `MR`, `MR_EDIT` |
| `MixedRealityBootstrap` | `[RuntimeInitializeOnLoadMethod]` — installs the MR system on scene load |
| `MRSceneBootstrap` | Wires scene-level MR objects on startup |
| `MRSceneHost` | Hosts MR scene lifecycle (FixedScene boot path) |
| `MRSceneTransition` | Async scene load/unload for VR↔MR travel |
| `MRSceneScanRequest` / `MRSceneScanState` | MRUK room-scan request and progress |
| `MRPassthroughController` | Meta passthrough underlay, fade sphere, safe Editor fallback |
| `MRVrSystemsGate` | Suspends VR locomotion and LibRetro while MR is active |
| `MRCameraRigShim` | Bridges `XROrigin` / `OVRPlayerController` between modes |
| `MRCameraRigAlignLog` | Debug log for floor / rig alignment after booth travel |
| `MRSceneLoadState` | Tracks current scene-load phase |
| `MRScenePermissions` | Runtime platform permission checks |
| `MRTransitionLog` | Structured log helper for mode-transition events |
| `MRAgentPlayerPresence` | Keeps the player avatar consistent across mode switches |
| `MRRoomInfoUI` | Overlay with room / anchor debug info |
| `MREditMenuInput` | Input for the in-MR CRT configuration menus |
| `MREditorInput` | Editor-only MR input helpers |
| `MRPaths` | On-disk MR folder layout under `ConfigManager.BaseDir` |
| `MRDebugLog` | MR DEBUG screen log buffer (CRT menu) |

---

### `Layout/` — Cabinet Registry & Spawn

Manages cabinet placement YAML: reads, writes, and spawns arcade cabinets in MR space.

| Script | Role |
|--------|------|
| `MRLayoutRegistry` | Loads layout YAML; spawn / move / remove cabinets; skinning + adjustments |
| `MRPlacedCabinet` | Runtime data for a single cabinet placed in MR |
| `MRLibretroWarmup` | Pre-warms LibRetro cores on MR entry (reduces first-launch stutter) |
| `MRGameCabinetAttractSetup` | Attract-mode behaviour for MR-placed cabinets |

---

### `Environment/` — Props, Surfaces, Skins & Lighting

Environment objects (`objects-layout.yaml`), custom user objects, room skins,
posters, MRUK effect mesh, and MR lighting.

**Registry & catalog**

| Script | Role |
|--------|------|
| `MREnvironmentRegistry` | Loads `objects-layout.yaml`; spawn / remove placed props |
| `MREnvironmentCatalog` | Built-in prop prefabs under `Resources/ramiro/PrefabsEnvironment/` |
| `MREnvironmentUnifiedCatalog` | Merges official + custom entries for CRT menus |
| `MRCatalogBootstrap` | Initialises catalogs at startup |
| `MREnvironmentCatalogEntry` | Single catalog row (prefab, placement profile, display name) |
| `MRPlacedEnvironment` | Runtime data for one placed environment prop |
| `MREnvironmentSurfaces` | MRUK surface queries (floor, wall, ceiling) |

**Custom objects** (`{BaseDir}/MR/Custom Objects/`)

| Script | Role |
|--------|------|
| `MRCustomObjectCatalog` | Scans custom-object packages (`object.yaml` + GLB) |
| `MRCustomObjectDefinition` | Parsed `object.yaml` model |
| `MRCustomObjectLoader` | Instantiates GLB + applies components |
| `MRCustomObjectComponentApplier` | Adds grab, video, collision from YAML |
| `MRCustomObjectGrab` | XR grab + dock return |
| `MRCustomObjectVideo` | Video playback on a named mesh (`Screen`, etc.) |
| `MRCustomObjectRotator` | Stick rotation for placed custom props |
| `MRVideoRemuxUtility` | Remuxes user video for Quest playback when needed |

**Room skins** (`{BaseDir}/MR/Room Skins/`)

| Script | Role |
|--------|------|
| `MRRoomSkinCatalog` | Scans `roomskin.yaml` packages |
| `MRRoomSkinDefinition` | Parsed room-skin metadata |
| `MRRoomSurfaceSkin` | Applies per-anchor textures via `MRRoomSkin` material |

**Posters** (`{BaseDir}/MR/Posters/`)

| Script | Role |
|--------|------|
| `MRPostersCatalog` | User poster image catalog |
| `MRPosterFactory` | Spawns poster prefab from image path |
| `MRPosterPlacement` | Placement + persistence for wall posters |
| `MRWallPoster` | Runtime poster renderer (`MRWallPoster` material) |

**Effect mesh & scan visuals**

| Script | Role |
|--------|------|
| `MREffectMeshController` | EffectMesh materials (`RoomBoxEffects` / `MROccluderAndShadow`) |
| `MREffectMeshSettings` | Scan mesh visibility and colour prefs (PlayerPrefs) |
| `MREffectMeshScanColors` | SCAN COLORS tuning in CRT **MESH** menu |
| `MREffectMeshVisibility` | Show/hide anchor and global effect meshes |

**Lights**

| Script | Role |
|--------|------|
| `MRLightsCatalog` | MR light prop catalog |
| `MRLightPlacement` | Spawn / tune placed lamps |
| `MRAutoLightingSettings` | Global sun on/off + intensity (PlayerPrefs; CRT **CONFIG → GLOBAL LIGHT**) |
| `MRAutoLightingVisibility` | Applies global sun without affecting manual lamps |
| `MRMrEnvironmentLighting` | Cool white global fill (ambient + key/fill directionals) |

---

### `Configuration/` — CRT Edit Menu & Placement

In-headset CRT configuration: cabinets, props, skins, posters, lights, placement
ray, global scale / floor offset, and anchor resolution.

| Script | Role |
|--------|------|
| `MRConfigurationController` | Main CRT menu controller (`GenericMenu` on config cabinet) |
| `MRConfigurationCabinetController` | Repositions the configuration cabinet itself |
| `MRConfigurationUI` | Legacy uGUI bindings — **not used in production** |
| `MRPlacementRayController` | Right-hand placement ray (Floor / Wall / Ceiling) |
| `MRPlacementProfile` | Per-prefab placement rules |
| `PlacementOrientation` | Enum: floor / wall / ceiling facing |
| `MRAdjustmentsSettings` | Global scale + floor Y offset (PlayerPrefs, step 0.01) |
| `MRAnchorPoseResolver` | 6-DOF poses from MRUK `OVRAnchor.Uuid` |
| `MRRuntimeSettings` | Scriptable travel / booth timing overrides |
| `MRPhoneBoothTransitionSequence` | Shared booth transition step helpers |

**CRT menu tree (summary)**

```
MR CONFIGURATION
  CABINETS        → arcade catalog; Add/Rem; Move when placed
  OFFICIAL        → built-in props (PortableGames, lights, …)
  CUSTOM          → user packages from MR/Custom Objects/
  ROOM SKIN       → per-anchor textures
  POSTERS         → wall images
  LIGHTS          → lamps + auto-light toggle
  MESH            → effect-mesh visibility + scan colours
  MOVE CONFIG     → reposition ConfigurationCabinetMiniMR
  ADJUSTMENTS     → global scale + floor Y
  PHONE BOOTH     → show/hide payphone cabine
  DEBUG / HELP / EXIT
```

---

### `PhoneBooth/` — VR ↔ MR Immersive Travel

Phone booth journey: handset grab, travel VFX, head fade, passthrough timing,
and portal logic.

| Script | Role |
|--------|------|
| `MRPhoneBoothPortal` | Coroutine orchestrator for the full VR↔MR travel sequence |
| `PayphoneHandsetGrab` | XR grab; cradle pick-up / auto-return |
| `MRPhoneBoothTravelVfx` | Door VFX, glass swap, shake, passthrough during travel |
| `MRPhoneBoothTravelHeadFade` | Head fade via gallery `ZoneBlackout` |
| `MRPhoneBoothInteriorVolumeLayout` | Interior volume / spawn alignment inside booth |
| `MRPhoneBoothSettings` | `MR/phone-booth.yaml` — visibility, auto-hide after travel, MR pose |
| `MRPhoneBoothVisibility` | CRT **PHONE BOOTH** show/hide |
| `PhoneBoothJourneyDirection` | Enum: `VrToMr`, `MrToVr` |
| `PhoneBoothTravelState` | Travel sequence phase state |

---

### `PortableGames/` — Handheld Libretro Device

Portable Games prop: CRT menu (cores → ROM list) and Libretro gameplay on the
device screen. Prefab: `Resources/ramiro/PrefabsEnvironment/PortableGames`.

| Script | Role |
|--------|------|
| `PortableGames` | Menu flow, screen shader, ROM launch on portable mesh |
| `PortableShaderScreen` | CRT shader bound to custom screen material |
| `PortableGamesTwoHandGrab` | Two-hand grab + dock return |

---

### `DevTools/` — Development & Testing

Editor and test-scene utilities. Not required for production player builds.

| Script | Role |
|--------|------|
| `MREditorMrSimulator` | Simulates MR in the Unity Editor (no headset) |
| `MRTestConfigSceneLoader` | `TestConfig`: MRUK room + load YAML layouts |
| `MRTestConfigEditorCamera` | Fly camera in `TestConfig` (RMB + WASD) |
| `MRTestGameCabinetSpawn` | Spawns a test cabinet in `TestMRmanager` |
| `MRTestCustomObjectSpawn` | Spawns a custom object package for QA |
| `MRTestPosterSpawn` | Spawns a test wall poster |
| `MRExampleCabinetSeed` | Seeds layout registry with example data |
| `TestPhoneBoothHeadFadeBootstrap` | Boots head-fade test scene |
| `TestPhoneBoothHeadFadeEditorCamera` | Editor camera for head-fade tests |

---

### `Data/` — Data Models

Plain C# types for serialised layout data (no `MonoBehaviour`).

| Script | Role |
|--------|------|
| `MRLayout` | Data model for cabinet entries in layout YAML (v3 schema) |

---

## On-Disk Paths (`MRPaths`)

Under `ConfigManager.BaseDir` (Editor: `%UserProfile%/cabs`, Quest: `.../com.curif.AgeOfJoy`):

| Path | Purpose |
|------|---------|
| `MR/cabinets-layout.yaml` | Cabinet placement (current) |
| `MR/objects-layout.yaml` | Environment / custom / poster / skin placement |
| `MR/Custom Objects/{name}/` | User object packages (`object.yaml` + GLB + assets) |
| `MR/Posters/` | Poster images |
| `MR/Room Skins/{name}/` | Room skin packages (`roomskin.yaml` + texture) |

Legacy filenames (`mr-layout.yaml`, `mr-environment-layout.yaml` in `cabinetsdb/`) are
still read when the new `MR/` files are absent.

**Shipped templates:** `Assets/ramiro/CustomObjectTemplate/` (copy to `MR/Custom Objects/`).

---

## Key Asset References

| Asset | Purpose |
|-------|---------|
| `Resources/ramiro/PrefabsEnvironment/ConfigurationCabinetMiniMR` | CRT configuration cabinet |
| `Resources/ramiro/PrefabsEnvironment/PortableGames` | Handheld Libretro device |
| `Resources/Decoration/PhoneBooth/PF_Payphone` | Phone booth prefab |
| `Resources/ramiro/Materials/` | Runtime-loaded MR materials (skins, posters, effect mesh) |
| `Assets/ramiro/materials/` | Materials referenced directly by MR prefabs (not `Resources.Load`) |

---

## Related Code Outside This Folder

| Location | Role |
|----------|------|
| `Assets/curif/LibRetroWrapper/` | Cabinet factory, `ShaderScreen`, core emulation |
| `Assets/geometrizer/scripts/PassthroughTriggerHandler.cs` | Gallery **zone** passthrough (not full MR) |
| `CabinetFactory` + `registry.yaml` | VR room placement (unchanged by MR layout) |
