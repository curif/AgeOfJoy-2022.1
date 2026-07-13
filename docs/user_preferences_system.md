# User preferences system

How player-facing settings (audio volumes, player height, locomotion, NPC behavior, room mood light, system skin, …) are stored, loaded, edited from VR, and applied to the running game.

## Overview

Preferences live in YAML files on the device. There are two tiers:

- **Global** — applies to the whole app. One file: `configuration.yaml`.
- **Per-room** — overrides global for a single room scene. One file per scene: `Room001.yaml`, `Room002.yaml`, …

The player edits both tiers from the **in-VR configuration cabinet**. A "global vs room" toggle inside the cabinet picks which file each save targets. Outside the cabinet, code reads merged values via [RoomConfiguration.cs](../Assets/curif/LibRetroWrapper/RoomConfiguration.cs) (for room-aware consumers) or [GlobalConfiguration.cs](../Assets/curif/LibRetroWrapper/GlobalConfiguration.cs) (for global-only consumers).

The whole pipeline is **reactive**: writing a YAML file triggers a poll-based file watcher, which reloads, re-merges, and fires Unity events. Consumers subscribe to those events and reapply.

## Where files live

All preference files sit under `ConfigManager.ConfigDir`:

| Platform | Path |
|---|---|
| Quest (release) | `/sdcard/Android/data/com.curif.AgeOfJoy/configuration/` |
| Unity Editor | `<UserProfile>/cabs/configuration/` |

See [ConfigManager.cs:46](../Assets/curif/LibRetroWrapper/ConfigManager.cs#L46).

Files in that directory:

- `configuration.yaml` — the global file.
- `Room001.yaml` … `Room023.yaml` — one per room scene. The filename is set on the `FileMonitor` component inside each scene (e.g. [Room005.unity](../Assets/Scenes/Room005.unity) → `Room005.yaml`).
- `controllers/` and `controllers/schemes/` — controller mappings. **Not** part of `ConfigInformation`; managed independently. See [docs/controllers.md](controllers.md).

If a file doesn't exist when first loaded, the system writes a default ([GlobalConfiguration.Load](../Assets/curif/LibRetroWrapper/GlobalConfiguration.cs#L77)). Cabinet positions and controller mappings are stored elsewhere and are unaffected by preference resets — this is why the in-VR reset warning reads *"except controllers information and cabinets positions"*.

## Schema

The schema is [ConfigInformation.cs](../Assets/curif/LibRetroWrapper/ConfigInformation.cs). YamlDotNet serializes it directly, so the field names below are the YAML keys.

| Field | What it controls | Primary consumer |
|---|---|---|
| `audio.background` | Background-music volume + mute (out of game) | [BackgroundSoundController.cs](../Assets/curif/LibRetroWrapper/BackgroundSoundController.cs) |
| `audio.inGameBackground` (YAML: `in-game-background`) | Background-music volume + mute while playing a cabinet | [BackgroundSoundController.cs](../Assets/curif/LibRetroWrapper/BackgroundSoundController.cs) |
| `npc.status` | `enabled` / `static` / `disabled` | [NPCController.cs](../Assets/curif/LibRetroWrapper/NPCController.cs) |
| `player.height` | VR eye height; 0 = use default | [PlayerController.cs](../Assets/curif/LibRetroWrapper/PlayerController.cs) |
| `player.scale` | Player avatar scale (`Kid`/`Teen`/`Adult` → 0.6/0.75/0.9) | [PlayerController.cs](../Assets/curif/LibRetroWrapper/PlayerController.cs) |
| `player.skinColor` | `light` / `dark` (avatar hands) | [PlayerController.cs](../Assets/curif/LibRetroWrapper/PlayerController.cs) |
| `locomotion.teleportEnabled` (YAML: `teleport-enabled`) | Enable teleport interactor | [LocomotionConfigController.cs](../Assets/curif/LibRetroWrapper/LocomotionConfigController.cs) |
| `locomotion.moveSpeed` (YAML: `speed`) | Smooth-locomotion speed | [LocomotionConfigController.cs](../Assets/curif/LibRetroWrapper/LocomotionConfigController.cs) |
| `locomotion.turnSpeed` (YAML: `turn-speed`) | Smooth-turn speed | [LocomotionConfigController.cs](../Assets/curif/LibRetroWrapper/LocomotionConfigController.cs) |
| `locomotion.SnapTurnActive` / `SnapTurnAmount` | Snap-turn mode + degrees | [LocomotionConfigController.cs](../Assets/curif/LibRetroWrapper/LocomotionConfigController.cs) |
| `cabinet.insertCoinOnStartup` | Auto-coin when entering a cabinet | [LibretroScreenController.cs](../Assets/curif/LibRetroWrapper/LibretroScreenController.cs) |
| `cabinet.forcedShader` (YAML: `forced-shader`) | Override per-cabinet CRT shader | [LibretroScreenController.cs](../Assets/curif/LibRetroWrapper/LibretroScreenController.cs) |
| `cabinet.screenGlowIntensity` | CRT bloom intensity 0–5 | [LibretroScreenController.cs](../Assets/curif/LibRetroWrapper/LibretroScreenController.cs) |
| `cabinet.OriginalTextures` (YAML: `original-textures`) | Use cabinet's bundled textures vs. swapped ones | [Cabinet.cs](../Assets/curif/LibRetroWrapper/Cabinet.cs) |
| `cabinet.worldResolution` / `cabinet.ingameResolution` | XR resolution scale + foveated rendering level out-of-game vs in-cabinet | [DeviceController.cs](../Assets/curif/LibRetroWrapper/DeviceController.cs) |
| `light.color` + `light.intensity` | Per-room mood light tint and brightness | [UserLightManager.cs](../Assets/geometrizer/scripts/UserLightManager.cs) — see [docs/roomlight_system.md](roomlight_system.md) |
| `agebasic.active` / `agebasic.debug` / `agebasic.afterLoad` (YAML: `after-load`) | AGEBasic enable, debug output, post-load script path | [CabinetAGEBasic.cs](../Assets/curif/LibRetroWrapper/CabinetAGEBasic.cs), [AGEBasicScreenController.cs](../Assets/curif/LibRetroWrapper/AGEBasicScreenController.cs) |
| `system-skin` (`system_skin` in C#) | CRT text/menu skin name (default `c64`) | [ScreenGenerator.cs](../Assets/curif/UI/ScreenGenerator.cs) via `scr.Init` |

Defaults come from per-section static factories (`BackgroundDefault`, `BackgroundInGameDefault`, `PlayerDefault`, `CabinetDefault`) and the field initializers on `ConfigInformation`.

Validation runs after every load via `ConfigInformation.validate()` → per-section `IsValid()`. An invalid file falls back to defaults and a warning is logged.

## Lifecycle

```
                     ┌─────────────────────────────────┐
       (VR player)   │   in-VR configuration cabinet   │
       turns knob ──▶│   ConfigurationController       │
                     │   + ConfigurationHelper         │
                     └────────────────┬────────────────┘
                                      │ Save(isGlobal, cfg)
                                      ▼
        ┌─────────────────────┐   ┌────────────────────┐
        │  GlobalConfiguration│   │  RoomConfiguration │
        │  .Save()            │   │  .Save()           │
        └──────────┬──────────┘   └─────────┬──────────┘
                   │ ToYaml(yamlPath)        │ ToYaml(yamlPath)
                   ▼                         ▼
        configuration.yaml          Room0XX.yaml
                   │                         │
                   │ (mtime changes)         │
                   ▼                         ▼
        ┌─────────────────────────────────────────────┐
        │  FileMonitor — polls LastWriteTime every 2s │
        └──────────┬───────────────────────┬──────────┘
                   │                       │
                   ▼                       ▼
        GlobalConfiguration.Load   RoomConfiguration.Load
                   │                       │
                   │                       │  Merge(global, room)
                   │                       │  (room wins, see below)
                   ▼                       ▼
        OnGlobalConfigChanged       OnRoomConfigChanged
                   │                       │
                   ▼                       ▼
        PlayerController             NPCController
        LocomotionConfigController   BackgroundSoundController
        DeviceController             (reads merged values)
        (reads global only)
```

Key points:

- **The save path and the load path are decoupled.** `Save` writes the file. The watcher detects the write and triggers `Load`, which re-emits the event. This means external edits to the YAML files (e.g. from a desktop file manager while the headset is on the charger) propagate the same way as in-VR edits.
- `FileMonitor` polls on the main thread every 2 s ([FileMonitor.cs:13](../Assets/curif/LibRetroWrapper/FileMonitor.cs#L13)). Quest doesn't reliably surface `FileSystemWatcher` events from `/sdcard/`, so polling is intentional.
- `RoomConfiguration` listens to **both** its own file *and* `OnGlobalConfigChanged`, so flipping a global value also re-merges every loaded room.
- `GlobalConfiguration` lives on the `FixedGlobalConfiguration` GameObject in [FixedScene.unity](../Assets/Scenes/FixedScene.unity) — it persists across room loads. Each room scene carries its own `RoomConfiguration` instance.

## Merge rules

Implemented in `ConfigInformation.Merge(global, room)` ([ConfigInformation.cs:486](../Assets/curif/LibRetroWrapper/ConfigInformation.cs#L486)). Room values win whenever both sides are set.

Sections that **are merged** (room overrides global if present):

- `audio.background` and `audio.inGameBackground` (volume + muted)
- `npc.status`
- `player.height`, `player.scale` *(but not `player.skinColor` — see gotchas)*
- `agebasic` (cloned wholesale from whichever side is non-null)
- `light`

Sections that are **not merged** — i.e. consumers read straight from either `GlobalConfiguration.Configuration` (global-only) or the loaded room file:

- `locomotion.*` — consumed only by `LocomotionConfigController`, which subscribes to `OnGlobalConfigChanged`. Effectively **global-only**.
- `cabinet.*` — read by various cabinet/screen consumers from `GlobalConfiguration.Configuration` directly.
- `system_skin` — read once during scene init by `ScreenGenerator.Init`.

### Gotchas

- **`player.skinColor` is set by the UI but ignored by `Merge`** ([ConfigInformation.cs:500-505](../Assets/curif/LibRetroWrapper/ConfigInformation.cs#L500-L505) copies only `height` and `scale`). A room-level skin color change is written to `Room0XX.yaml` but lost the next time the room reloads. If you're touching this area, fix the merge.
- `PlayerController` and `LocomotionConfigController` subscribe to `OnGlobalConfigChanged` only — per-room player height / locomotion overrides will not take effect even though the UI lets you save them under "room" mode.
- `system_skin` is applied at scene init and never re-read. Changing it mid-session requires a room reload.

## How the player modifies preferences

The in-VR **configuration cabinet** is the only sanctioned UI ([UI/ConfigurationController.cs](../Assets/curif/UI/ConfigurationController.cs), ~106k chars; see [docs/configuration_controller_development.md](configuration_controller_development.md) for its internals). The flow:

1. Player walks up to the configuration cabinet and inserts a coin.
2. The first screen offers a **"working with global"** toggle ([ConfigurationController.cs:701](../Assets/curif/UI/ConfigurationController.cs#L701)).
   - In a gallery / hub room where `roomConfiguration` is wired up, both options are available.
   - In rooms where `roomConfiguration` is unassigned (`ConfigurationHelper.CanConfigureRoom() == false`), the toggle is locked to global.
3. From the main menu the player picks a category: Audio, NPC, Player, Locomotion, Cabinet, Light, Controllers, Reset, …
4. Each category is a `GenericContainer` of widgets (`GenericBool`, `GenericOptionsInteger`, `GenericOptions`) bound to fields on `ConfigInformation`. Editing a value updates only the in-memory widget; **save & exit** calls `ConfigurationHelper.Save(isGlobal, config)`, which writes the YAML and triggers the reload chain above.
5. **Reset** (`ConfigurationHelper.Reset(isGlobal)`) deletes the chosen YAML file and reloads — defaults are reinstated. Controllers and cabinet positions are not touched because they live elsewhere.

`ConfigurationHelper` is the thin facade between widgets and the two config singletons; it never writes a file itself, it just delegates to `GlobalConfiguration.Save` / `RoomConfiguration.Save`.

## Adding a new preference

Use this checklist when introducing a new preference (e.g. a new audio band, a new player option, a new cabinet toggle).

1. **Add the field to `ConfigInformation`** ([ConfigInformation.cs](../Assets/curif/LibRetroWrapper/ConfigInformation.cs)).
   - Put it on the appropriate inner class (`Player`, `Audio.Background`, `CabinetConfiguration`, …) or add a new inner class for a new section.
   - If the YAML key should differ from the C# field name, add `[YamlMember(Alias = "kebab-case-name", ApplyNamingConventions = false)]`.
   - Set a sensible default value at the field declaration.
   - If the value has a finite range, extend `IsValid()` on the owning class.
2. **Update defaults factories** if your new field needs an explicit default that differs from the field initializer (`BackgroundDefault`, `PlayerDefault`, `CabinetDefault`, etc.).
3. **Update `Merge`** ([ConfigInformation.cs:486](../Assets/curif/LibRetroWrapper/ConfigInformation.cs#L486)) **if the preference should be room-overridable**. Follow the existing pattern: `ret.foo.bar = ci2?.foo?.bar != null ? ci2.foo.bar : ci1?.foo?.bar;`. If the preference is global-only, skip this step — but verify no consumer reads it via `RoomConfiguration.Configuration` expecting a per-room value.
4. **Wire the widget into the configuration cabinet** ([ConfigurationController.cs](../Assets/curif/UI/ConfigurationController.cs)). For an existing category:
   - Create the widget in the matching `Set<Category>Widgets()` (e.g. `SetAudioWidgets`, `SetPlayerWidgets`).
   - Read the current value into the widget in `<category>Screen()` (e.g. `audioScreen`).
   - Write the widget value back to the config object in `<category>Save()` (e.g. `audioSave`), then `configHelper.Save(isGlobal, config)`.
   - For a brand-new category: add a menu entry in `mainMenu`, a new `GenericContainer`, a screen-draw method, a save method, and a draw entry in the controller's behavior tree.
5. **Subscribe a consumer.** Decide:
   - **Room-aware** (reacts to room or global edits): inject a `RoomConfiguration` reference, subscribe to `OnRoomConfigChanged`, read `roomConfiguration.Configuration.<your field>`. See `BackgroundSoundController` and `NPCController` for the pattern.
   - **Global-only** (no room override): inject a `GlobalConfiguration` reference, subscribe to `OnGlobalConfigChanged`, read `globalConfiguration.Configuration.<your field>`. See `PlayerController`, `LocomotionConfigController`, `DeviceController`.
   - Always implement `addListener` / `removeListener` and call them from `OnEnable` / `OnDisable` (existing controllers all use this pattern — copy it verbatim to avoid leaking listeners on scene transitions).
   - Apply the value once during your `Start`/init path so first-load works without waiting for the event.
6. **Test the reload path.** Save in VR (or hand-edit the YAML in the device path); confirm:
   - The file is created/updated under `ConfigManager.ConfigDir`.
   - The watcher reload prints the expected `[GlobalConfiguration]` / `[RoomConfiguration]` log lines (enable [docs/debug_mode.md](debug_mode.md)).
   - Your consumer's `On…ConfigChanged` fires and the value is applied.
   - Reset restores the default.

## Related docs

- [docs/configuration_controller_development.md](configuration_controller_development.md) — in-VR configuration cabinet internals (widget framework, behavior tree).
- [docs/controllers.md](controllers.md) — controller mapping files, stored separately under `configuration/controllers/`.
- [docs/roomlight_system.md](roomlight_system.md) — how `config.light` interacts with per-scene lights and the reflection probe.
- [docs/debug_mode.md](debug_mode.md) — enabling the logs cited in the testing checklist.
