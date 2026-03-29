# Controllers

This document describes how player input is implemented in AGE of Joy — from physical hardware to the emulated game — covering the data model, the class hierarchy, the configuration layers, and how to customize mappings.

## Overview

AGE of Joy bridges three different control vocabularies:

| Layer | Examples |
|---|---|
| **Physical hardware** | Meta Quest controller buttons, gamepad sticks, keyboard keys |
| **Unity Input System** | `<XRController>{RightHand}/primaryButton`, `<Gamepad>/leftStick` |
| **Libretro / MAME** | `JOYPAD_B`, `JOYPAD_UP`, `MOUSE_X`, `LIGHTGUN_TRIGGER` |

The control pipeline is:

```
Physical device
    → Unity Input System (InputAction / InputBinding)
        → LibretroControlMap.Active(mameControl, port)
            → LibRetro core callback
                → emulated game
```

The mapping between layers is driven entirely by `ControlMapConfiguration` objects loaded from YAML files or built in code.

---

## Key Classes

### `ControlMapPathDictionary` (`ControlMapPathDictionary.cs`)

A static dictionary that maps **friendly control names** (used in YAML and code) to **Unity Input System paths**.

```
"quest-a"           →  "<XRController>{RightHand}/primaryButton"
"gamepad-left-stick"→  "<Gamepad>/leftStick"
"keyboard-w"        →  "<keyboard>/w"
"mouse"             →  "<Mouse>/delta"
```

This is the single source of truth for all supported physical controls. It also exposes `GetBehavior(control)` which returns `"axis"` for thumbsticks and `"button"` for everything else.

### `LibretroControlMapDictionnary` (`LibretroControlMapDictionnary.cs`)

String constants for every libretro control ID the engine recognises:

```
JOYPAD_A, JOYPAD_B, JOYPAD_X, JOYPAD_Y
JOYPAD_START, JOYPAD_SELECT
JOYPAD_UP, JOYPAD_DOWN, JOYPAD_LEFT, JOYPAD_RIGHT
JOYPAD_L, JOYPAD_R, JOYPAD_L2, JOYPAD_R2, JOYPAD_L3, JOYPAD_R3
JOYPAD_LEFT_RUMBLE, JOYPAD_RIGHT_RUMBLE
EXIT, INSERT, MODIFIER
MOUSE_X, MOUSE_Y, MOUSE_LEFT, MOUSE_RIGHT, MOUSE_MIDDLE
MOUSE_WHEELUP, MOUSE_WHEELDOWN, MOUSE_HORIZ_WHEELUP, MOUSE_HORIZ_WHEELDOWN
MOUSE_BUTTON_4, MOUSE_BUTTON_5
LIGHTGUN_TRIGGER, LIGHTGUN_RELOAD, LIGHTGUN_AUX_A/B/C
LIGHTGUN_START, LIGHTGUN_SELECT
LIGHTGUN_DPAD_UP/DOWN/LEFT/RIGHT
KEYB_UP, KEYB_DOWN, KEYB_LEFT, KEYB_RIGHT   (editor-only)
```

### `ControlMapConfiguration` (`ControlMapConfiguration.cs`)

The data model. Holds a list of `Maps` entries, each describing one libretro control ID and which physical controls activate it.

```
ControlMapConfiguration
  ├── invertx : bool            ← invert analog x-axis
  ├── inverty : bool            ← invert analog y-axis
  └── mapList : List<Maps>
        └── Maps
              ├── mameControl : string   (libretro-id, e.g. "JOYPAD_B")
              ├── port        : int      (player port, default 0)
              ├── behavior    : string   ("button" or "axis")
              └── controlMaps : List<ControlMap>
                    └── ControlMap
                          ├── RealControl : string  (e.g. "quest-b")
                          └── Path        : string  (Unity input path)
```

`Merge(other)` overlays another configuration on top of this one: matching `mameControl+port` entries are replaced, new entries are added, and `invertx`/`inverty` are OR-merged (if either side is `true`, the result is `true`).

### `ControlMapInputAction` (`ControlMapInputAction.cs`)

A static factory. `inputActionMapFromConfiguration(conf, name)` converts a `ControlMapConfiguration` into a Unity `InputActionMap` — one `InputAction` per `Maps` entry (typed `Button` or `Value`), with one `InputBinding` per physical control.

### `LibretroControlMap` (`LibretroControlMap.cs`)

A Unity `MonoBehaviour` component attached to the CRT screen object. Holds the live `InputActionMap` and is queried every frame by the libretro callback.

Key members:

| Member | Purpose |
|---|---|
| `InvertX` / `InvertY` | Set from the configuration; flips analog axes at query time |
| `CreateFromConfiguration(conf)` | Builds the `InputActionMap`, reads `invertx`/`inverty` from `conf` |
| `Active(mameControl, port)` | Returns 0 or non-zero; handles button presses and axis thresholds |
| `Enable(bool)` | Enables/disables the entire action map (off while not playing) |
| `Clean()` | Disposes the action map when the game session ends |

`Active()` logic for axis controls:
- Reads `Vector2` or `float` from the action
- Applies `InvertX`/`InvertY` by negating the vector components
- Checks directional thresholds (`> 0.5`, `< -0.5`) for joystick directions
- Scales mouse delta to a signed integer for `MOUSE_X`/`MOUSE_Y`
- Checks wheel delta threshold for scroll inputs

---

## Configuration Class Hierarchy

The configuration classes form a merge chain. Each level applies on top of the previous one:

```
DefaultControlMap          ← hard-coded defaults for all controls (Quest + gamepad + keyboard)
    └── GlobalControlMap   ← merges configuration/controllers/global.yaml (player-wide overrides)
          └── GameControlMap      ← merges configuration/controllers/<game>.yaml (per-game user overrides)
          └── ControlSchemeControlMap  ← merges configuration/controllers/schemes/<scheme>.yaml
    └── CustomControlMap   ← merges any ControlMapConfiguration directly (used for cabinet-embedded maps)
```

`DefaultControlMap` is a singleton (`DefaultControlMap.Instance`) used as the fallback when nothing else is configured.

### File locations on device

```
<sdcard>/AgeOfJoy/configuration/controllers/global.yaml          ← GlobalControlMap
<sdcard>/AgeOfJoy/configuration/controllers/<cabDBName>.yaml      ← GameControlMap
<sdcard>/AgeOfJoy/configuration/controllers/schemes/<name>.yaml  ← ControlSchemeControlMap
```

---

## Default Control Mapping

The following table is the built-in default (`DefaultControlMap`). All three hardware types (Quest VR controller, gamepad, keyboard) are mapped simultaneously.

| Libretro ID | Port | Behavior | Quest | Gamepad | Keyboard |
|---|---|---|---|---|---|
| `MODIFIER` | 0 | button | left-grip | — | — |
| `JOYPAD_B` | 0 | button | B, right-trigger | B | Enter, W, 1 |
| `JOYPAD_A` | 0 | button | A | A | Q, 0 |
| `JOYPAD_X` | 0 | button | X | X | E, 2 |
| `JOYPAD_Y` | 0 | button | Y | Y | R, 3 |
| `JOYPAD_START` | 0 | button | start | start | I |
| `JOYPAD_START` | 2 | button | — | — | 7 |
| `JOYPAD_SELECT` | 0 | button | select | select | U, 6 |
| `JOYPAD_UP` | 0 | axis | left-thumbstick | left-thumbstick | — |
| `JOYPAD_DOWN` | 0 | axis | left-thumbstick | left-thumbstick | — |
| `JOYPAD_LEFT` | 0 | axis | left-thumbstick | left-thumbstick | — |
| `JOYPAD_RIGHT` | 0 | axis | left-thumbstick | left-thumbstick | — |
| `JOYPAD_UP` | 0 | button | — | dpad-up | A, Z |
| `JOYPAD_DOWN` | 0 | button | — | dpad-down | S, X |
| `JOYPAD_LEFT` | 0 | button | — | dpad-left | D, C |
| `JOYPAD_RIGHT` | 0 | button | — | dpad-right | F, V |
| `JOYPAD_UP` | 1 | axis | right-thumbstick | right-thumbstick | — |
| `JOYPAD_DOWN` | 1 | axis | right-thumbstick | right-thumbstick | — |
| `JOYPAD_LEFT` | 1 | axis | right-thumbstick | right-thumbstick | — |
| `JOYPAD_RIGHT` | 1 | axis | right-thumbstick | right-thumbstick | — |
| `JOYPAD_L` | 0 | button | left-trigger | left-trigger | Y, 5 |
| `JOYPAD_R` | 0 | button | right-trigger | right-trigger | O, 8 |
| `JOYPAD_L2` | 0 | button | left-grip | left-bumper | T, 4 |
| `JOYPAD_R2` | 0 | button | right-grip | right-bumper | P, 9 |
| `JOYPAD_L3` | 0 | button | left-thumbstick-press | left-thumbstick-press | — |
| `JOYPAD_R3` | 0 | button | right-thumbstick-press | right-thumbstick-press | — |
| `JOYPAD_LEFT_RUMBLE` | 0 | button | left-haptic-device | — | — |
| `JOYPAD_RIGHT_RUMBLE` | 0 | button | right-haptic-device | — | — |
| `EXIT` | 0 | button | right-grip | left-bumper | Esc |
| `INSERT` | 0 | button | — | select | U, 6 |
| `MOUSE_X` | 0 | axis | right-thumbstick | right-thumbstick | mouse-delta |
| `MOUSE_Y` | 0 | axis | right-thumbstick | right-thumbstick | mouse-delta |
| `MOUSE_LEFT` | 0 | button | B | B | mouse-left |
| `MOUSE_RIGHT` | 0 | button | A | A | mouse-right |
| `MOUSE_MIDDLE` | 0 | button | X | X | mouse-middle |
| `MOUSE_WHEELUP` | 0 | axis | left-thumbstick | left-thumbstick | scroll-up |
| `MOUSE_WHEELDOWN` | 0 | axis | left-thumbstick | left-thumbstick | scroll-down |
| `MOUSE_HORIZ_WHEELUP` | 0 | axis | left-thumbstick | left-thumbstick | scroll-left |
| `MOUSE_HORIZ_WHEELDOWN` | 0 | axis | left-thumbstick | left-thumbstick | scroll-right |
| `MOUSE_BUTTON_4` | 0 | button | left-thumbstick-press | left-thumbstick-press | mouse-button-4 |
| `MOUSE_BUTTON_5` | 0 | button | right-thumbstick-press | right-thumbstick-press | mouse-button-5 |
| `LIGHTGUN_TRIGGER` | 0 | button | right-trigger | right-trigger | — |
| `LIGHTGUN_RELOAD` | 0 | button | start | start | — |
| `LIGHTGUN_AUX_A` | 0 | button | A | A | — |
| `LIGHTGUN_AUX_B` | 0 | button | B, right-grip | B | Enter |
| `LIGHTGUN_AUX_C` | 0 | button | X | X | — |
| `LIGHTGUN_START` | 0 | button | start | start | — |
| `LIGHTGUN_SELECT` | 0 | button | select | select | — |
| `LIGHTGUN_DPAD_*` | 0 | axis | left-thumbstick | left-thumbstick | — |
| `LIGHTGUN_DPAD_*` | 1 | axis | right-thumbstick | right-thumbstick | — |

> Note: `EXIT` requires `MODIFIER` (left-grip) to be held simultaneously and must be held for `SecondsToWaitToExitGame` seconds (default 2) before the game exits.

---

## Configuration Layers and Priority

When a game starts, `LibretroScreenController` (or `AGEBasicScreenController`) selects a configuration using this priority order:

1. **Cabinet-embedded map** (`controllers:` block in `description.yaml`) — highest priority
2. **Per-game user override** (`configuration/controllers/<cabDBName>.yaml` on the headset)
3. **Control scheme** (`control-scheme:` key in `description.yaml` → `configuration/controllers/schemes/<name>.yaml`)
4. **Global user override** (`configuration/controllers/global.yaml` on the headset)
5. **Built-in defaults** (`DefaultControlMap`) — always the base

Every level uses `Merge()` on top of `DefaultControlMap`, so you only need to specify the entries you want to change.

---

## Configuring Controllers in `description.yaml`

### Remapping buttons

The `controllers:` block in a cabinet's `description.yaml` can override any default mapping.

```yaml
controllers:
  maps:
  - libretro-id: JOYPAD_B
    maps-to:
    - control: quest-b
    - control: gamepad-b
    - control: quest-right-trigger
```

- `libretro-id`: the libretro control name (from the table above)
- `port`: player port, defaults to `0`
- `maps-to`: list of physical controls to bind; each `control:` is a friendly name from the list below

The entire default binding for that `libretro-id` + `port` pair is **replaced** (not appended) by the cabinet map.

### Control axis inversion

```yaml
controllers:
  invertx: true
  inverty: false
```

Flips the analog input axes for the whole cabinet. Applies to all axis-type controls: thumbstick joystick directions (`JOYPAD_UP/DOWN/LEFT/RIGHT`), mouse movement (`MOUSE_X`/`MOUSE_Y`), and scroll wheel axes. Useful for games where the default axis direction feels reversed — for example FPS-style camera or trackball games.

Both keys are optional and default to `false`. They can be combined with `maps:` in the same `controllers:` block.

### Control scheme

```yaml
control-scheme: 6-buttons
```

Points to a file at `configuration/controllers/schemes/6-buttons.yaml` on the headset. Schemes are reusable mapping presets — a cabinet author declares the scheme name; the player installs the corresponding YAML file. See the Control Schemes documentation for details.

---

## Supported Physical Control Names

These are the values valid for `control:` entries in YAML and for `AddMap()` calls in code.

### Meta Quest VR Controllers

| Name | Description |
|---|---|
| `quest-a` | A button (right controller) |
| `quest-b` | B button (right controller) |
| `quest-x` | X button (left controller) |
| `quest-y` | Y button (left controller) |
| `quest-start` | Start / menu button |
| `quest-select` | Select / Oculus button (right controller) |
| `quest-left-grip` | Left grip button |
| `quest-right-grip` | Right grip button |
| `quest-left-trigger` | Left trigger |
| `quest-right-trigger` | Right trigger |
| `quest-left-thumbstick` | Left thumbstick (axis) |
| `quest-right-thumbstick` | Right thumbstick (axis) |
| `quest-left-thumbstick-press` | Left thumbstick click |
| `quest-right-thumbstick-press` | Right thumbstick click |
| `quest-left-haptic-device` | Left controller haptic (rumble output) |
| `quest-right-haptic-device` | Right controller haptic (rumble output) |

### Gamepad

| Name | Description |
|---|---|
| `gamepad-a` | South button |
| `gamepad-b` | East button |
| `gamepad-x` | West button |
| `gamepad-y` | North button |
| `gamepad-start` | Start button |
| `gamepad-select` | Select / back button |
| `gamepad-left-bumper` | Left shoulder button |
| `gamepad-right-bumper` | Right shoulder button |
| `gamepad-left-trigger` | Left trigger |
| `gamepad-right-trigger` | Right trigger |
| `gamepad-left-thumbstick` | Left stick (axis) |
| `gamepad-right-thumbstick` | Right stick (axis) |
| `gamepad-left-thumbstick-press` | Left stick click |
| `gamepad-right-thumbstick-press` | Right stick click |
| `gamepad-dpad-up` | D-pad up |
| `gamepad-dpad-down` | D-pad down |
| `gamepad-dpad-left` | D-pad left |
| `gamepad-dpad-right` | D-pad right |

### Keyboard

| Name | Keys |
|---|---|
| `keyboard-a` `keyboard-w` `keyboard-s` `keyboard-d` | WASD |
| `keyboard-q` `keyboard-e` `keyboard-r` `keyboard-t` | QERT |
| `keyboard-y` `keyboard-u` `keyboard-i` `keyboard-o` `keyboard-p` | YUIOP |
| `keyboard-x` `keyboard-z` `keyboard-c` `keyboard-v` | XZCV |
| `keyboard-f` | F |
| `keyboard-0` … `keyboard-9` | Number row |
| `keyboard-enter` | Enter |
| `keyboard-esc` | Escape |
| `keyboard-space` | Space |

### Mouse

| Name | Description |
|---|---|
| `mouse` | Mouse delta (x/y movement, axis) |
| `mouse-left` | Left button |
| `mouse-right` | Right button |
| `mouse-middle` | Middle button |
| `mouse-scroll-up` | Scroll wheel up |
| `mouse-scroll-down` | Scroll wheel down |
| `mouse-scroll-left` | Horizontal scroll left |
| `mouse-scroll-right` | Horizontal scroll right |
| `mouse-button-4` | Extra button 4 |
| `mouse-button-5` | Extra button 5 |

---

## Input Devices

Some games require a non-standard libretro input device type (e.g. pointer, lightgun, analog). The `devices:` key in `description.yaml` assigns device types per player port:

```yaml
devices:
  - slot: 0
    type: pointer
```

Supported device type names: `empty`, `gamepad`, `mouse`, `mouse_pointer`, `keyboard`, `lightgun`, `analog`, `pointer`, `psx_standard`, `psx_analog`, `psx_dual_shock`, `psx_negcon`, `psx_guncon`, `psx_justifier`, `psx_mouse`, `snes_superscope`, `snes_justifier`, `snes_justifier_2`, `snes_macs_rifle`, `sega_phaser`, `sega_menacer`, `sega_justifiers`, `nes_zapper`, `nes_arkanoid`, `nes_powerpad_a`, `nes_powerpad_b`.

A numeric device ID can also be specified directly as `device_<uint>` for cores that expose custom device subtypes.

---

## User-Level Overrides (on the headset)

Players can override mappings without touching cabinet assets by placing YAML files on the headset's storage.

**Global override** — applies to every game:
```
<sdcard>/AgeOfJoy/configuration/controllers/global.yaml
```

**Per-game override** — applies to one specific game (filename = cabinet DB name):
```
<sdcard>/AgeOfJoy/configuration/controllers/dkong.yaml
```

The YAML format is the same as the `controllers:` block in `description.yaml`:

```yaml
maps:
- libretro-id: JOYPAD_B
  maps-to:
  - control: quest-b
  - control: gamepad-b
```

These files are loaded at game start and merged on top of the defaults, so only the entries listed are changed.

---

## Code Reference

| Class | File | Role |
|---|---|---|
| `ControlMapPathDictionary` | `ControlMapPathDictionary.cs` | Friendly name → Unity input path dictionary |
| `LibretroControlMapDictionnary` | `LibretroControlMapDictionnary.cs` | Libretro control ID string constants |
| `ControlMapConfiguration` | `ControlMapConfiguration.cs` | Data model; YAML serialization; `Merge()` |
| `DefaultControlMap` | `ControlMapConfiguration.cs` | Hard-coded default mappings (singleton) |
| `GlobalControlMap` | `ControlMapConfiguration.cs` | Loads `global.yaml` over defaults |
| `GameControlMap` | `ControlMapConfiguration.cs` | Loads per-game YAML over global |
| `ControlSchemeControlMap` | `ControlMapConfiguration.cs` | Loads scheme YAML over defaults |
| `CustomControlMap` | `ControlMapConfiguration.cs` | Merges any config over global (used by cabinets) |
| `ControlMapInputAction` | `ControlMapInputAction.cs` | Converts `ControlMapConfiguration` → Unity `InputActionMap` |
| `LibretroControlMap` | `LibretroControlMap.cs` | MonoBehaviour; queries input each frame; axis inversion |
| `LibretroInputDevice` | `LibretroInputDevice.cs` | Libretro device type constants and lookup |
