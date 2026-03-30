# Configuration Controller — Developer Guide

## Overview

`ConfigurationController.cs` (`Assets/curif/UI/`) is the in-VR configuration room UI for Age of Joy. It runs as a Unity MonoBehaviour coroutine that drives a **Fluid Behavior Tree** (CleverCrow.Fluid.BTs). The screen is a virtual CRT rendered by `ScreenGenerator`. All interaction is controller-based — no mouse, no touch.

The system handles:
- Global and per-room arcade settings (audio, graphics, locomotion, player, lights, NPC)
- Cabinet replacement and graphics configuration
- Controller mapping
- Teleportation between rooms
- AGEBasic script execution
- Debug logging and bug report generation

---

## Architecture at a Glance

```
ConfigurationController (MonoBehaviour)
│
├── run() coroutine ──────────────────────────────────────────────
│   │  Tick → ResetInputValues → Wait → UpdateInputValues → Tick…
│   └── Builds and ticks a Fluid BehaviorTree
│
├── StatusOptions enum  (state machine)
│
├── GenericWidgetContainers  (one per menu screen)
│   └── Each contains GenericWidget subclasses
│
├── LibretroControlMap  (physical input → inputDictionary)
├── ScreenGenerator     (character-grid CRT renderer)
├── ConfigurationHelper (YAML persistence)
└── BugReportManager    (debug logging + ZIP export)
```

---

## State Machine

The controller is driven by a `StatusOptions` enum. Each value corresponds to exactly one Behavior Tree sequence:

| Status | Menu |
|---|---|
| `init` | Presentation / insert-coin screen |
| `waitingForCoin` | Waiting for coin slot |
| `onBoot` | Animated boot sequence |
| `onMainMenu` | Main configuration menu |
| `onNPCMenu` | NPC behaviour configuration |
| `onAudio` | Audio volume / mute |
| `onChangeMode` | Switch global ↔ room config |
| `onChangeController` | Controller button mapping |
| `onChangeCabinets` | Replace cabinets in room |
| `onTeleport` | Teleport to another room |
| `onReset` | Reset configuration to defaults |
| `onChangeLocomotion` | Player movement settings |
| `onChangePlayer` | Player height / skin colour |
| `onRunAGEBasic` | AGEBasic program selector |
| `onRunAGEBasicRunning` | AGEBasic executing |
| `onCabinet` | Cabinet graphics settings |
| `onLights` | Light colour / intensity |
| `onDebugMenu` | Debug logging + bug report |
| `exit` | Exit configuration room |

State transitions happen inside BT Process nodes. The BT `Selector` at the root runs the first sequence whose condition matches the current status.

---

## Main Coroutine Loop

```csharp
// Simplified structure of run()
inputDictionary initialised;
screen initialised;
widgets created;
status = init;

tree = buildBT();
while (true)
{
    tree.Tick();              // BT reads inputDictionary, updates state
    ResetInputValues();       // Clear all inputs to false

    if (status == init || status == waitingForCoin)
        yield return new WaitForSeconds(2f);          // no input sampling
    else if (status == onBoot)
        yield return new WaitForSeconds(0.25f);       // no input sampling
    else
    {
        yield return new WaitForSeconds(1f / 6f);     // ~166 ms
        UpdateInputValues();  // sample physical controller → inputDictionary
    }
}
```

**Critical ordering:** inputs are sampled at the END of each iteration (after the wait), so they are available on the NEXT `Tick()`. This means each physical button press is consumed in exactly one tick. The `ResetInputValues()` call guarantees no input leaks between ticks.

---

## Input System

### The Input Dictionary

```csharp
Dictionary<string, bool> inputDictionary;
// keys: "up", "down", "left", "right", "action", "x", "y", "a"
```

### UpdateInputValues — accumulation with OR

```csharp
void UpdateInputValues()
{
    inputDictionary["up"]     = inputDictionary["up"]     || ControlActive(LC.JOYPAD_UP);
    inputDictionary["down"]   = inputDictionary["down"]   || ControlActive(LC.JOYPAD_DOWN);
    inputDictionary["left"]   = inputDictionary["left"]   || ControlActive(LC.JOYPAD_LEFT);
    inputDictionary["right"]  = inputDictionary["right"]  || ControlActive(LC.JOYPAD_RIGHT);
    inputDictionary["action"] = inputDictionary["action"] || ControlActive(LC.JOYPAD_B);
    inputDictionary["x"]      = inputDictionary["x"]      || ControlActive(LC.JOYPAD_X);
    inputDictionary["y"]      = inputDictionary["y"]      || ControlActive(LC.JOYPAD_Y);
    inputDictionary["a"]      = inputDictionary["a"]      || ControlActive(LC.JOYPAD_A);
}
```

The OR accumulation ensures that a brief press during the wait window is not lost. Inputs remain true until `ResetInputValues()` clears them after the next tick.

### Control Mapping

`setupActionMap()` creates a `DefaultControlMap` (from `ControlMapConfiguration.cs`) and in the Unity Editor adds keyboard aliases:

| Input | VR Controller | Editor Key |
|---|---|---|
| `JOYPAD_UP` / `DOWN` / `LEFT` / `RIGHT` | Left thumbstick (axis) | W / S / A / D |
| `JOYPAD_B` (action / select) | B button | Enter |
| `JOYPAD_X` | X button | Escape |
| `JOYPAD_Y` | Y button | Y |
| `JOYPAD_A` | A button | Space |

**Important:** `JOYPAD_B` must NOT share a key with any direction. In `DefaultControlMap`, `KEYBOARD_W` was historically in `JOYPAD_B`'s binding list alongside `VR_CONTROLLER_B`. This caused the "navigate up accidentally toggles a bool" bug because pressing W fired both `JOYPAD_UP` and `JOYPAD_B` in the same tick, moving the cursor onto a `GenericBool` and triggering its toggle simultaneously. The fix was to remove `KEYBOARD_W` from `JOYPAD_B`'s default mapping.

### changeContainerSelection

All menu Process nodes delegate directional navigation through this helper:

```csharp
private void changeContainerSelection(GenericWidgetContainer gwc)
{
    if (inputDictionary["up"])
        gwc.PreviousOption();
    else if (inputDictionary["down"])
        gwc.NextOption();
    else if (inputDictionary["left"])
        gwc.GetSelectedWidget()?.PreviousOption();
    else if (inputDictionary["right"])
        gwc.GetSelectedWidget()?.NextOption();
}
```

- `up` / `down` — move the container cursor (which widget is selected).
- `left` / `right` — change the value of the currently selected widget (e.g., cycle a `GenericOptions` list).
- Directions are mutually exclusive per tick (`else if` chain).

---

## Widget System

### Class Hierarchy

```
GenericWidget  (abstract)
├── GenericLabel            — static/dynamic text, non-selectable
│   ├── GenericButton       — selectable, highlighted when active
│   ├── GenericLabelOnOff   — text that can be hidden
│   └── GenericTimedLabel   — text that auto-clears after N seconds
├── GenericBool             — checkbox toggle  ( ) / (*)
├── GenericOptions          — left/right value selector
│   ├── GenericOptionsInteger   — integer range (min–max)
│   └── GenericOptionsDecimal   — decimal range (min–max, step)
└── GenericWindow           — decorative box frame with title

GenericWidgetContainer      — ordered list of widgets with cursor
```

### GenericWidget (base)

Every widget exposes:

| Member | Purpose |
|---|---|
| `int x, y` | Character-grid position |
| `string name` | Unique identifier for lookup |
| `bool isSelectable` | Whether cursor can land here |
| `bool enabled` | Whether widget is rendered and interactive |
| `Draw()` | Render to screen |
| `Action()` | Execute (toggle, confirm, etc.) |
| `NextOption()` | Move to next value (for value widgets) |
| `PreviousOption()` | Move to previous value |

### GenericWidgetContainer

The container manages the cursor (selection index) among its children. Only widgets with `isSelectable=true` AND `enabled=true` participate in navigation.

```csharp
container.Add(new GenericWindow(...))    // frame — not selectable
         .Add(new GenericLabel(...))     // label — not selectable
         .Add(new GenericBool(...))      // toggle — selectable  ← cursor starts here
         .Add(new GenericButton(...))    // button — selectable
```

`Add()` returns `this` for fluent chaining. The first selectable/enabled widget added becomes the initial cursor position. `DrawAll()` renders every widget and paints `>` before the selected one.

### GenericBool

```csharp
new GenericBool(screen, "name", "Label text: ", initialValue, x, y)
```

Renders as `Label text:  ( )` with `*` inside when true. `Action()` calls `Toggle()`, which flips `value` and redraws. The container Process node must explicitly call `Action()` — navigation alone never changes the value.

### GenericOptions / GenericOptionsInteger / GenericOptionsDecimal

```csharp
new GenericOptionsInteger(screen, "speed", "Speed:", minVal, maxVal, x, y, format: "D2")
new GenericOptionsDecimal(screen, "intensity", "Intensity:", 0.0, 2.5, 0.1, x, y, format: "N2")
```

Displays `Label: < VALUE >`. Left/right arrows cycle through the option list. `GetSelectedOption()` returns the current value (typed for Integer/Decimal subclasses). `SetCurrent(value)` positions the selector programmatically on load.

### GenericTimedLabel

```csharp
label.label = "Saved!";
label.SetSecondsAndDraw(2);   // show for 2 seconds then auto-clear
```

The label draws itself for the specified duration. Each Process node calls `label.Draw()` every tick to let the timer count down and eventually clear the text.

### GenericWindow

Non-selectable decorative frame. Drawn once in the Init node. Position and size are specified in characters. Title is centred in the top border.

---

## Menu Screens — Reference

### Main Menu

Built by `SetMainMenuWidgets()`. Uses `GenericMenu` (not `GenericWidgetContainer`), which centres options and shows a help line below the selection. Options are added conditionally:

| Option | Condition |
|---|---|
| AGEBasic | always |
| Audio configuration | `CanConfigureAudio()` |
| NPC configuration | `canChangeNPC` |
| controllers | `CanConfigureControllers()` |
| cabinets (replace) | `CanConfigureCabinets()` AND room mode |
| configure cabinets (graphics) | global mode only |
| locomotion | global mode only |
| player | global mode only |
| debug & bug report | global mode only |
| lights | always |
| change mode | always |
| reset | always |
| teleport | `canTeleport` |
| exit | always |

### Global vs Room Configuration

Every save/load call passes `isGlobalConfigurationWidget.value` to `ConfigurationHelper`:

- `true` → global configuration file (applies to all rooms)
- `false` → room-specific configuration file

The widget is only interactive when `CanConfigureRoom()` returns true. When the player is in the configuration room (not inside a regular room), only the global config is available.

### Debug & Bug Report Menu

Built by `SetDebugWidgets()`. Contains:

1. `GenericBool` "debugToggle" — enables `Application.logMessageReceivedThreaded` capture via `BugReportManager`.
2. Five `GenericLabel` instruction lines — workflow guide for users.
3. `GenericButton` "generate" — triggers `GenerateBugReport()`, an async operation that creates a ZIP in the AgeOfJoy base directory.
4. `GenericButton` "exit".
5. `GenericTimedLabel` "statusLabel" — shows "Logging started", "Logging stopped", ZIP result, or error.

ZIP generation is blocked if no log file exists (`BugReportManager.HasLogFile()` returns false) — this prevents generating an empty report when the user never enabled logging.

### Controller Mapping Menu

The most complex screen. It exposes a 5-slot mapping table for a selected cabinet game and MAME control. Notably, this sequence supplements `inputDictionary` with direct `ControlActive(LC.KEYB_UP/DOWN)` calls, providing a keyboard fallback exclusive to this screen. Left/right variable names are intentionally swapped in the source to achieve the correct directional behaviour for the selector.

### AGEBasic Menu

Allows running any `.bas` script from the AGEBasic programs directory. The `onRunAGEBasicRunning` status monitors execution; a 30-minute timeout prevents the configuration room from being locked indefinitely. After execution, any runtime error is surfaced on screen.

---

## Behavior Tree Pattern

Every menu follows the same two-node Sequence:

```
Sequence("Menu Name")
  Condition(() => status == StatusOptions.onXxx)
  Do("Init", () =>
  {
      SetXxxWidgets();          // create widgets (no-op if already created)
      XxxWindowDraw();          // clear screen, draw container, draw hints
      scr.DrawScreen();
      return Success;           // runs once, then BT advances to Process
  })
  Do("Process", () =>
  {
      changeContainerSelection(xxxContainer);

      if (inputDictionary["action"])
      {
          GenericWidget w = xxxContainer.GetSelectedWidget();
          if (w.name == "exit")  { status = onMainMenu; return Success; }
          if (w.name == "save")  { XxxSave(); status = onMainMenu; return Success; }
          // widget-specific handling or generic fallthrough:
          w.Action();
      }

      scr.DrawScreen();
      return Continue;          // stays here until a Success is returned
  })
End()
```

**Init runs once** because BT sequences remember child state — when Init returns `Success`, the BT advances to Process and stays there on subsequent ticks.

**Process returns `Continue`** each frame except when transitioning out (change `status` and return `Success`). When a sequence returns `Success`, the BT root Selector re-evaluates from the top, matching the new status to a different sequence.

---

## Adding a New Menu Screen

1. **Add a status value** to `StatusOptions`.

2. **Declare widget fields** near the top of the class, following the existing naming pattern (`xxxContainer`, specific widget references you need to read back on save).

3. **Implement `SetXxxWidgets()`** — create the `GenericWidgetContainer` and populate it. Guard with `if (xxxContainer != null) return;` so it only builds once.

4. **Implement `XxxWindowDraw()`** — call `scr.Clear()`, `xxxContainer.Draw()`, and print any footer hints (`UDLR_TO_CHANGE`, `B_TO_SELECT`).

5. **Implement save/load helpers** following the existing save pattern (read `isGlobalConfigurationWidget.value`, call `configHelper.getConfigInformation(isGlobal)`, mutate, call `configHelper.Save(isGlobal, config)`).

6. **Add to the main menu** in `SetMainMenuWidgets()`, with the appropriate condition.

7. **Add a Behavior Tree sequence** in `buildBT()` following the Init + Process pattern above. Insert before the Exit sequence.

8. **Add the transition** from the main menu's Process node switch statement.

---

## Adding a New Widget Type

1. Subclass `GenericWidget`. Implement `Draw()`. Override `Action()`, `NextOption()`, and/or `PreviousOption()` as needed.
2. Set `isSelectable = false` in the constructor if the widget should never receive the cursor (labels, windows).
3. Register nothing — just instantiate and `Add()` to a `GenericWidgetContainer`.

---

## Persistence

`ConfigurationHelper` abstracts file I/O. Config data is modelled by `ConfigInformation` (YAML-serialisable). The save pattern is always:

```csharp
bool isGlobal = isGlobalConfigurationWidget.value;
ConfigInformation config = configHelper.getConfigInformation(isGlobal);
config.someSection = new SomeSection();
config.someSection.field = widget.GetSelectedOption();
configHelper.Save(isGlobal, config);
```

Never write directly to disk from a Process node.

---

## Known Pitfalls

### Input key collision
`JOYPAD_B` and any direction key must not share a keyboard binding. If they do, a single keypress fires both navigation and action in the same tick, causing a `GenericBool` to toggle when the cursor merely moves onto it. See the fix in `ControlMapConfiguration.cs` — `KEYBOARD_W` was removed from `JOYPAD_B`'s binding list.

### Widget creation is one-shot
`SetXxxWidgets()` guards against re-creation (`if (container != null) return`). Widget state (e.g. `GenericBool.value`) therefore persists for the lifetime of the configuration session. If you need to reset state on re-entry, do it explicitly before or after `SetXxxWidgets()`.

### Async operations
`GenerateBugReport()` and `SaveCabinetPositions()` are `async void`. They update a `GenericTimedLabel` on completion. The BT continues ticking while they run — the Process node simply redisplays the timed label each tick until it auto-clears.

### Controller mapping menu input
The controller mapping sequence reads `ControlActive(LC.KEYB_UP/DOWN)` directly alongside `inputDictionary`, giving it a keyboard fallback that other menus do not have. This is intentional for usability during controller remapping but means the pattern is not uniform across all sequences.

### Widget Y positioning
Some containers use absolute Y coordinates, others use `container.lastYAdded + N` for relative stacking. Be consistent within a container; mixing the two makes layout changes error-prone.
