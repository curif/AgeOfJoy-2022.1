# LED Interface (MAMEhooker) and MAMEcheat Memory Hooking

This document covers two related but distinct mechanisms for reacting to in-game events in AGE of Joy: the **LibRetro LED interface** (the working implementation, analogous to MAMEhooker) and **memory-change watching via MAMEcheat XML** (implemented but effectively non-functional with current cores).

---

## 1. Background — What MAMEhooker Does

[MAMEhooker](http://mamehooker.net/) is a Windows utility that connects to a running MAME process and reads its output signals in real time — things like Player Start lamps, coin counters, ticket dispensers, solenoids, and fans. These output signals come from the arcade PCB's actual I/O ports, decoded by MAME from the ROM's port writes. MAMEhooker uses them to drive real hardware attached to a cabinet (serial-controlled LED boards, contactors, etc.).

In AGE of Joy the same concept applies, but instead of driving real hardware we drive virtual cabinet parts: emission colors, audio, haptics, animations. The two LibRetro mechanisms explored for this are:

| Mechanism | MAMEhooker analogy | Status |
|---|---|---|
| `RETRO_ENVIRONMENT_GET_LED_INTERFACE` | Output signals (lamp, counter, solenoid) | **Working** |
| `RETRO_ENVIRONMENT_SET_MEMORY_MAPS` + MAMEcheat XML | Direct memory read (lives, score, etc.) | **Implemented but not working with current cores** |

---

## 2. LED Interface — The Working Path

### How it works

Real arcade PCBs drove output lamps via dedicated I/O port writes. In `mame2003-plus`, game drivers call `set_led_status(int num, int on)`. The core accumulates these into an 8-bit bitmask and fires a callback once per video frame for each LED whose state changed (edge-triggered, not level). Up to 8 LEDs (indices 0–7).

The LibRetro contract:

```c
typedef void (RETRO_CALLCONV *retro_set_led_state_t)(int led, int state);

struct retro_led_interface {
    retro_set_led_state_t set_led_state;
};
```

During `retro_load_game`, the core calls:
```c
environ_cb(RETRO_ENVIRONMENT_GET_LED_INTERFACE, &ledintf);
```

The frontend fills `ledintf.set_led_state` with its own function pointer. The core then calls that function every frame when LEDs change.

### Call chain in mame2003-plus

1. A game driver calls `set_led_status(int num, int on)` (e.g. Player 1 Start button should light up).
2. `mame.c` accumulates into `leds_status` bitmask.
3. Each frame, `video.c` XORs current vs previous bitmask; for each changed bit it calls `led_state_cb(led, state)`.
4. `led_state_cb` is our registered function pointer.

Source: `src/mame2003/core_options.c` (registers callback during `update_variables()`) and `src/mame2003/video.c` (per-frame dispatch).

### Core support matrix

| Core | LED interface | Notes |
|------|---|---|
| **mame2003-plus** | **Yes** | ~28 game drivers call `set_led_status`. Confirmed in source. |
| mame2010 | No | Has internal output system, never bridges to LibRetro LED interface. |
| fbneo | No | No LED interface support found in source. |

The implementation in AGE of Joy is **core-agnostic** — the callback handler is registered unconditionally for every cabinet. If the core never calls `RETRO_ENVIRONMENT_GET_LED_INTERFACE`, the LED state array stays all-zero and `OnLedChange` events seed and then silently never trigger. No filtering, no per-core logic.

### Implementation in AGE of Joy

**`cwrapper/environment.c`:**
- `led_state[8]` static array holds current LED states.
- `wrapper_set_led_state(int led, int state)` — callback registered with the core.
- Handler for `RETRO_ENVIRONMENT_GET_LED_INTERFACE` fills `led_if->set_led_state`.
- `wrapper_get_led_state(int led)` — exported C function, returns 0/1/-1.
- `wrapper_led_reset()` — called from `wrapper_unload_game()`, zeros the array.

**`LibretroMameCore.cs`:**
- P/Invoke for both `wrapper_get_led_state` and `wrapper_led_reset`.
- `getLedState(int led)` public accessor — returns -1 if `!GameLoaded`.
- `wrapper_led_reset()` called in `ClearAll()`.

**`basicEvents.cs` — `OnLedChange` class:**
- Follows the same pattern as `OnMemoryChange`.
- `seeded` flag: first call records initial state without triggering.
- `permanentlyFailed` flag: if `getLedState` returns -1, event self-disables with zero ongoing CPU cost.
- Edge-triggered: only fires when state changes from previous poll.
- Injects the named variable with the new state (0 or 1) before jumping to the handler line.

**`CabinetAGEBasic.cs`:**
- `"on-led-change"` in `validEvents[]`.
- `ledIndex` field on `EventInformation` (YAML alias: `led`).

**`basic/functions/led.cs`:**
- `CommandFunctionONLED` — `ONEVENT ONLED(index, "varName") GOTO line`
- `CommandFunctionLEDSTATE` — `LET S = LEDSTATE(index)`

**`basic/Commands/ONEVENT.cs`:**
- `"on-led-change"` case maps `configVal[2]` → `ledIndex`, `configVal[3]` → `varName`.

### Cabinet YAML syntax

```yaml
agebasic:
  events:
    - event: on-led-change
      led: 0
      var: LED0
      program: hooks.bas
      goto: 1000
```

### AGEBasic syntax

```vb
' Event-driven (YAML or runtime):
ONEVENT ONLED(0, "P1_LAMP") GOTO 1000

' Direct polling:
LET S = LEDSTATE(0)   ' returns 0, 1, or -1
```

### Verified log markers (mame2003-plus, Star Wars)

```
[RETRO_ENVIRONMENT_GET_LED_INTERFACE] registering LED callback
[OnLedChange] Init: led=0 var='LED0' program=starwars-hooks.bas line=1000
[OnLedChange] TRIGGER led=0 1 → 0 → jumping to line 1000
```

---

## 3. Game Compatibility Reference

Based on `mame2003-plus` source inspection. LED index semantics are game-specific.

### Games with LEDs

| Game | ROM | LEDs | Notes |
|------|-----|------|-------|
| Star Wars | `starwars.zip` | 0, 1, 2 | 3 LEDs from output port writes |
| Missile Command | `missile.zip` | 0, 1 | Coin/game signal lamps |
| Berzerk | `berzerk.zip` | 0 | Toggled on specific memory reads |
| Battles (Xevious variant) | `battles.zip` | 0, 1 | P1/P2 Start |
| Ajax | `ajax.zip` | 0–7 | Start, super weapon, power-up lamps |

### Games without LEDs

| Game | ROM | Reason |
|------|-----|--------|
| Juno First | `junofrst.zip` | Uses `coin_counter_w` only, no `set_led_status` calls |

### Discovering LEDs empirically

```vb
10 ONEVENT ONTIMER(1) GOTO 100
20 END
100 FOR I = 0 TO 7
110   LET S = LEDSTATE(I)
120   IF S > 0 THEN PRINT "LED "; I; " = "; S
130 NEXT I
140 END
```

Run this on any mame2003-plus game and interact with it (insert coin, press Start). Active LEDs will appear in the log.

---

## 4. MAMEcheat Memory Hooking — Why It Doesn't Work

### The idea

MAMEhooker can also read raw MAME process memory to detect game state changes (lives, score, level). The community-maintained [Pugsy's Cheats](https://www.mamecheat.co.uk/) database (`cheat.xml`) provides named addresses for thousands of games in the format `maincpu.mb@0x8880` (chip, byte size, CPU address).

The equivalent LibRetro mechanism is `RETRO_ENVIRONMENT_SET_MEMORY_MAPS` (env cmd 36), which lets a core push its memory layout to the frontend. RetroArch uses this for its built-in cheat engine.

The AGE of Joy implementation:
- Parses Pugsy-format `cheat.xml` from the cabinet folder.
- Translates `chip.size@address` → `(region, offset)`.
- `OnMemoryChange` event polls the address each frame and triggers on value change.
- `ONMEMORY(address, region, "var")` or `ONMEMORY("cheat name", "var")` for YAML or runtime registration.

### Why it fails

**Neither mame2003-plus nor mame2010 call `RETRO_ENVIRONMENT_SET_MEMORY_MAPS`.**

Unlike `GET_LED_INTERFACE` (which the core calls because *it* wants to push to the frontend), `SET_MEMORY_MAPS` would require the core to proactively expose its memory layout. MAME cores never implemented this bridge — the internal memory system is complex (banked, address-translated, multi-CPU) and exposing it cleanly to LibRetro was apparently never prioritized.

Without `SET_MEMORY_MAPS` being called, `getMemorySize(region)` returns 0 for all regions except SAVE_RAM. The fallback `wrapper_read_memory_map()` never has any descriptors to work with. All `PEEK()` calls into SYSTEM_RAM return garbage or zero. `OnMemoryChange` events seed with a value and then never trigger because the memory is never updated.

### When it could work

The `fbneo` (Final Burn Neo) core may call `SET_MEMORY_MAPS` for some games. If confirmed, `on-memory-change` events would work for those games — the Pugsy addresses would be valid because both MAME and FBNeo emulate the same physical hardware, so CPU addresses are identical. This remains **untested**.

SAVE_RAM (region 0) via `PEEK(offset)` does work, but game logic is not stored in SRAM — only save data. Practically unusable for MAMEhooker-style event detection.

### What was implemented (safe to leave in)

All code is in place and safe to ship:
- `MameCheatXmlParser.cs` — parses Pugsy cheat XML.
- `OnMemoryChange` in `basicEvents.cs` — polls memory, self-disables cleanly if memory is unavailable.
- `ONMEMORY` function in `basic/functions/events.cs`.
- `PEEK(offset [, region])` extended to support all 4 LibRetro memory regions.
- `wrapper_read_memory_map()` in `environment.c` as fallback for region 2.

Cabinets that do not declare `on-memory-change` events are completely unaffected. If memory is not exposed by the core, the event self-disables silently.

---

## 5. Future Work

### LED interface

- Test more mame2003-plus games to expand the compatibility list.
- If mame2010 ever gains LED interface support, it will work automatically (no code changes needed).
- Consider documenting LED indices per-game in the content vault as the community discovers them.

### Memory hooking

- Test `fbneo` with a known game and `SET_MEMORY_MAPS` logging to confirm whether it fires.
- If confirmed, write cabinet examples and update documentation to note fbneo as a supported core.
- Consider a diagnostic AGEBasic program that dumps memory map descriptors when `SET_MEMORY_MAPS` fires.

---

## 6. Key Files Reference

| File | Role |
|------|------|
| `Assets/curif/LibRetroWrapper/cwrapper/environment.c` | C wrapper: LED callback registration, `wrapper_get_led_state`, `wrapper_led_reset`, `wrapper_read_memory_map` |
| `Assets/curif/LibRetroWrapper/cwrapper/environment.h` | Declarations for both LED and memory exports |
| `Assets/curif/LibRetroWrapper/LibretroMameCore.cs` | P/Invoke, `getLedState()` accessor, `ClearAll()` cleanup |
| `Assets/curif/LibRetroWrapper/basic/basicEvents.cs` | `OnLedChange` and `OnMemoryChange` event classes, `EventsFactory` |
| `Assets/curif/LibRetroWrapper/CabinetAGEBasic.cs` | `EventInformation` fields (`ledIndex`), `validEvents[]` |
| `Assets/curif/LibRetroWrapper/basic/Commands/ONEVENT.cs` | `"on-led-change"` and `"on-memory-change"` cases |
| `Assets/curif/LibRetroWrapper/basic/functions/led.cs` | `ONLED`, `LEDSTATE` functions |
| `Assets/curif/LibRetroWrapper/basic/functions/events.cs` | `ONMEMORY` function |
| `Assets/curif/LibRetroWrapper/MameCheatXmlParser.cs` | Pugsy cheat XML parser |
| `Assets/curif/LibRetroWrapper/basic/basicCommands.cs` | Function registry for all of the above |
