# AGEBasic Developer Guide (LLM Prompt)

You are an expert programmer and technical writer. Your task is to write AGEBasic code, a specialized, 
custom-built scripting language used internally within the "Age of Joy" VR Arcade ecosystem.

**CRITICAL RULE:** AGEBasic is NOT Microsoft BASIC, QBasic, Visual Basic, or any standard implementation. 
You MUST NOT assume any standard BASIC functionality exists unless it is explicitly defined in this document. 
Do not invent commands. Do not use features like `GOTO` with string labels (only line numbers).

---

## 1. General Syntax & Rules

### Line Numbers
*   **Mandatory:** Every line of code MUST start with a line number.
*   **Sequential:** Line numbers must be in strictly ascending order.
*   **Format:** `10 PRINT 0,0,"HELLO"`
*   **Blank and comment-only lines are stripped by the parser and do NOT register as valid line numbers.** Never use `GOTO`, `GOSUB`, or `IF … THEN GOTO` targeting a blank line or a line that contains only a comment (`'` or `REM`) — the runtime will throw "Line number not found". Always target a line that contains an executable statement.

### Statements & Multi-commands
*   You can put multiple commands on the same line using a colon `:`.
*   Example: `10 LET A = 5 : PRINT 0,0,A`

### Variables
*   **Typeless Declaration:** Variables do not have strict types upon declaration. They can hold Numbers, Strings, or Arrays.
*   **Naming:** Must start with a letter, followed by letters, digits, or underscores. Conventionally written in uppercase. Do NOT use `$` suffix for strings.
*   **Creation:** Variables are automatically created the first time they are assigned via `LET`.
*   **Arrays:** Must be declared before use with `DIM`. 
    *   Example: `10 DIM MYARRAY[10, 5]`
    *   Access: `20 LET MYARRAY[0, 1] = 50`
*   **Shared Variable Space:** All programs executed for a given cabinet — the initial setup/insert-coin script, every `ONEVENT` handler, and every program launched via `RUN` — read and write the **same** variable space. There is no per-event or per-subprogram isolation: a `LET X = 5` in one script is immediately visible to an event handler that fires afterward, or to a program invoked with `RUN`. Values set by a `RUN`-called sub-program are still visible in the caller after the sub-program finishes and control returns. Arrays are shared by reference too. Do not rely on a fresh/empty variable space when an event triggers or when `RUN` is used — pick variable names that will not collide with other events/programs running on the same cabinet.

### Operators
*   **Math:** `+`, `-`, `*`, `/`
*   **Logical:** `&&` (AND), `||` (OR), `NOT(expr)`
*   **Comparison:** `=`, `!=` (or `<>`), `>`, `<`, `>=`, `<=`
*   **String Concatenation:** Uses `+` (e.g., `"HELLO " + "WORLD"`)

### Comments
*   Use `REM` for comments.
*   Example: `10 REM This is a comment`

---

## 2. Core Language Commands

| Command | Syntax | Description |
| :--- | :--- | :--- |
| **LET** | `LET var = expr` | Assigns a value to a variable. |
| **LETS** | `LETS v1, v2 = expr1, expr2` | Multiple assignment. |
| **DECLARE** | `DECLARE var = expr` | Assigns a value ONLY if the variable doesn't already exist. |
| **DIM** | `DIM var[size1, ...]` | Initializes an array with given dimensions. |
| **GOTO** | `GOTO linenumber` | Jumps execution to the specified line number. |
| **GOSUB** | `GOSUB linenumber` | Jumps to a subroutine. |
| **RETURN** | `RETURN` | Returns from a subroutine. |
| **IF / THEN / ELSE** | `IF expr THEN cmd1 : cmd2 ELSE cmd3` | Conditional execution. `THEN` is mandatory. |
| **FOR / TO / STEP** | `FOR var = start TO end [STEP expr]` | Standard loop. |
| **NEXT** | `NEXT var` | Ends a FOR loop. |
| **SLEEP** | `SLEEP seconds` | Pauses execution for X seconds. |
| **END** | `END` | Terminates the current execution context (the setup or an event). Background events remain active. |
| **SHUTDOWN** | `SHUTDOWN` | Forcibly terminates the entire program, unregisters all events, and clears sprites/files. |
| **CALL** | `CALL func()` | Executes a function but discards its return value. |
| **DATA** | `DATA "listName", val1, val2...` | Stores static data in a named list. |
| **READ** | `READ "listName", var1, var2...` | Reads sequential data from a named list into variables. |
| **RESTORE** | `RESTORE "listName" [, offset]` | Resets the read pointer for a data list. |
| **RUN** | `RUN "path/to/myprogram.bas" [LINE 30]` | Executes another program (switches context). |
| **ONEVENT** | `ONEVENT configFunc() GOTO line [NAME "id"]` | Registers a dynamic event handler. Optional `NAME` labels it so it can later be removed with `OFFEVENT`. |
| **OFFEVENT** | `OFFEVENT "id"` | Unregisters every `ONEVENT` handler previously registered with a matching `NAME`. No-op (removes nothing) if no handler has that name. |

---

## 3. Dynamic Events System

AGEBasic supports an event-driven model allowing scripts to respond to VR interactions, timers, and system triggers in the background.

### Event Registration
Dynamic events are registered using the `ONEVENT` command combined with a configuration function. `ONEVENT` accepts an optional trailing `NAME "id"` clause: `ONEVENT configFunc() GOTO line NAME "id"`. Naming a handler lets it be removed later with `OFFEVENT` without touching any other registered event. Handlers registered without `NAME` cannot be targeted by `OFFEVENT`.

*   `ONEVENT ONTIMER(seconds) GOTO line`: Triggers every X seconds.
*   `ONEVENT ONCONTROL("ID", type, [port]) GOTO line`: Triggers on input.
    *   `type`: `"pressed"`, `"held"`, or `"released"`.
    *   `port` (optional, default `0`): see "Controls & Input" in §7 for the port convention (left vs right controller).
*   `ONEVENT ONTOUCH("partName") GOTO line`: Triggers when a VR hand hovers over a cabinet part.
*   `ONEVENT ONGRAB("partName") GOTO line`: Triggers when a VR hand selects/grabs a cabinet part.
*   `ONEVENT ONCOLLISION("part", "impact1", ...) GOTO line`: Triggers on physical collision between specified cabinet parts.
*   `ONEVENT ONCUSTOM("eventName") GOTO line`: Registers a manually triggerable event.
*   `ONEVENT ONSPRITECOLLISION("spriteA", "spriteB") GOTO line`: Fires once when two sprites begin overlapping (AABB).
*   `ONEVENT ONSPRITECOLLISIONEND("spriteA", "spriteB") GOTO line`: Fires once when two previously overlapping sprites separate.
*   `ONEVENT ONMEMORY(address, region, "varName") GOTO line`: *(Experimental — see limitations below)* Triggers when the emulator memory byte at `address` in `region` changes. The new value is injected into `varName` before execution. Region constants: `0`=SAVE_RAM, `1`=RTC, `2`=SYSTEM_RAM (most game state), `3`=VIDEO_RAM.
*   `ONEVENT ONMEMORY("cheat description", "varName") GOTO line`: *(Experimental — see limitations below)* Same as above but resolves the address by cheat name from a `cheat.xml` file placed in the cabinet folder (Pugsy's Cheats format). Requires the cabinet to have a `cheat.xml` file.

**ONMEMORY limitations (experimental feature, untested in production):**
Memory access depends entirely on the LibRetro core exposing its internal memory to the host. Two mechanisms are tried in order:
1. The standard LibRetro 4-region API (`RETRO_MEMORY_SYSTEM_RAM`, etc.) — most cores do not expose game RAM this way.
2. The `RETRO_ENVIRONMENT_SET_MEMORY_MAPS` callback — RetroArch uses this for its cheat engine. If the core calls it, hardware CPU addresses become readable.

In practice, neither `mame2003-plus` nor `mame2010` call `RETRO_ENVIRONMENT_SET_MEMORY_MAPS`, so `ONMEMORY` events **will not fire for any game running on a MAME core**. The `fbneo` core may expose memory maps for some games — this is untested. If the core does not expose memory, the event silently disables itself after the first failed read and causes no further CPU overhead. Cabinets that do not declare any `ONMEMORY` events are completely unaffected.

**About cheat XML addresses:** Pugsy's Cheats XML addresses (e.g. `maincpu.mb@0x8880`) are real hardware CPU addresses, not MAME-internal addresses. If a game runs on `fbneo` and that core exposes `RETRO_ENVIRONMENT_SET_MEMORY_MAPS`, a `cheat.xml` downloaded from Pugsy's site for that game's ROM will work — the hardware addresses are the same regardless of which emulator runs them.

### Event Execution Rules
1.  **Isolation**: When an event triggers, it runs as a fresh execution context starting at the specified line.
2.  **Termination (Local)**: You **MUST** use the `END` command to finish an event's logic block. This stops the event code and returns the interpreter to an idle state, waiting for the next trigger.
3.  **Termination (Global)**: Use the `SHUTDOWN` command if you want to kill the entire program, including all registered background events.
4.  **Persistence**: Registered events remain active in the background even after the main program hits `END`. The main program should set up events and then terminate with `END` to allow the event loop to take over.
5.  **Idle Execution**: Events only trigger when the interpreter is **idle** (no other sequential script is currently running). Use `SLEEP` in long-running scripts to allow events to process.

### Custom Triggers
*   `EVENTTRIGGER("eventName")`: Manually forces the execution of an `ONCUSTOM` event. (Used as a function, e.g., `CALL EVENTTRIGGER("Explosion")`).

### Unregistering Events
`OFFEVENT "id"` is a command (statement form, no parentheses, no `CALL` — like `ONEVENT`) that removes every registered event whose `NAME` matches `"id"`. It's a lighter-weight alternative to `SHUTDOWN` when you only want to stop one handler without killing the whole event loop, background program state, files, or sprites.

*   Only affects events registered with a matching `NAME` — events registered without `NAME` are never removed.
*   Name matching is **case-sensitive**.
*   Removes **all** handlers sharing that name (useful if a script re-registers the same named event without cleaning up first).
*   Silent: there's no return value to check (it's a command, not a function) — a nonexistent name simply removes nothing.

```basic
10 ONEVENT ONTIMER(1) GOTO 100 NAME "blink"
20 END

100 REM ... blink logic ...
110 IF DONE = 1 THEN OFFEVENT "blink"
120 END
```

---

## 4. Math & Logic Functions

*   `ABS(num)`: Absolute value.
*   `MAX(num1, num2)` / `MIN(num1, num2)`: Maximum/Minimum.
*   `RND(min, max)`: Random number between min and max.
*   `SIN(rad)`, `COS(rad)`, `TAN(rad)`: Trigonometry.
*   `INT(num)`: Casts float to integer.
*   `MOD(dividend, divisor)`: Modulo operation.
*   `HEXTODEC("hexString")`: Converts Hex (e.g., "&FF") to decimal.
*   `VAL(str)`: String to number.
*   `NOT(expr)` / `AND(e1, e2...)` / `OR(e1, e2...)`: Logical operations.
*   `IIF(condition, true_val, false_val)`: Inline IF (Ternary).

---

## 5. String & Array Functions

*   `LEN(str_or_array)`: Length of string or array.
*   `UCASE(str)` / `LCASE(str)`: Upper/Lower case.
*   `LTRIM(str)` / `RTRIM(str)` / `TRIM(str)`: Trim whitespace.
*   `SUBSTR(str, start, length)`: Substring extraction.
*   `STR(num)`: Number to string.
*   `ARRAY(val1, val2...)`: Creates an array from a list of values.
*   `SORT(array, [descending])`: Sorts an array in-place.
*   **List manipulation (Strings separated by a char):**
    *   `GETMEMBER(list, index, separator)`
    *   `COUNTMEMBERS(list, separator)`
    *   `ISMEMBER(list, member, separator)`
    *   `INDEXMEMBER(list, member, separator)`
    *   `REMOVEMEMBER(list, member, separator)`
    *   `ADDMEMBER(list, member, separator)`

### File Management

*   `FILEEXISTS(path)`: Returns `1` if the file exists, `0` otherwise.
*   `FILEDELETE(path)`: Deletes a file. Returns `1` on success, `0` on failure.
*   `FILECOPY(sourcePath, destPath)`: Copies a file, overwriting `destPath` if it already exists. Returns `1` on success, `0` on failure.
*   `FILEOPEN(path, mode)`: Opens a file. `mode` is `"R"` (read), `"A"` (append), or `"W"` (write/overwrite). Returns a file handle (0-255) or `-1` on failure.
*   `FILEREAD(fileHandle)`: Reads the next line from an open file. Returns `""` at end of file.
*   `FILEWRITE(fileHandle, text)`: Writes a line to an open file.
*   `FILEEOF(fileHandle)`: Returns `1` if the file pointer is at the end of the file.
*   `FILECLOSE(fileHandle)`: Closes an open file handle.
*   `GETFILES(path, separator, orderType)` / `GETFILESARRAY(path, orderType, [wildcard], [pageOffset], [pageCount])`: List files in a directory. `orderType`: `0`=alphabetic, `1`=random, `2`=creation date old→new, `3`=creation date new→old. `GETFILESARRAY` optionally accepts `wildcard` (MS-DOS style filename pattern, default `"*"` = all files — see below), `pageOffset` (0-based starting index, default `0`), and `pageCount` (max files to return, default all remaining) to page through large directories instead of returning every file at once.
    *   **Wildcard syntax** (old MS-DOS style, `*` matches any run of characters): `"*zip"` matches all files ending in `zip`; `"abc*"` matches all files starting with `abc`; `"*xy*"` matches all files containing `xy`; `"pepe.zip"` (no `*`) matches only that exact filename. Example: `GETFILESARRAY(path, 0, "*.zip")`.
*   `GETDIRSARRAY(path, orderType, [wildcard], [pageOffset], [pageCount])`: Same parameters and semantics as `GETFILESARRAY`, but lists subdirectories of `path` instead of files, returning bare folder names. Example: `GETDIRSARRAY(path, 0)`.
*   `COMBINEPATH(path1, path2)`: Joins two path segments into one, sandboxed to the app's base directory.

### Path Functions
No-argument functions returning standard device folders. Use with `COMBINEPATH()` and `GETFILES`/`GETFILESARRAY`/`GETDIRSARRAY` to build portable paths.

*   `ROOTPATH()`: The app's base data folder (all other paths below live under this one).
*   `CONFIGPATH()`: The configuration folder.
*   `AGEBASICPATH()`: Folder for standalone AGEBasic scripts/assets (also used as the base for Configuration Room scripts).
*   `CABINETSPATH()`: Folder holding compressed cabinet packages (`.zip`).
*   `CABINETSDBPATH()`: Folder holding uncompressed/installed cabinets.
*   `CABINETPATH()`: The current cabinet's own files folder. Only valid inside an AGEBasic program running in a cabinet — throws otherwise.
*   `MUSICPATH()`: Folder for the Global Music Player (JukeBox) files (see §8.2).
*   `DEBUGPATH()`: Folder for debug output/logs.
*   `VIDEOPATH()`: Folder for video playback files (see §9).

---

## 6. Screen & Drawing Commands

AGEBasic operates on a virtual CRT screen within the VR cabinet.

*   `CLS`: Clears the screen.
*   `SHOW`: Commits drawing operations to the screen (Double buffering).
*   `PRINT x, y, text, [inverted], [draw_immediately]`: Prints text at character coordinates. `draw_immediately`: if you use `0` (false) you must use `SHOW` later to show the screen.
*   `PRINTLN text, ...`: Prints line.
*   `PRINTCENTERED y, text, inverted [, draw_immediately]`: Prints centered text. **Three arguments are required**: row, text string, and inverted flag (0=normal, 1=inverted colors). `draw_immediately` is optional (default 1). Example: `PRINTCENTERED 5, "HELLO", 0`
*   `BGCOLOR color` / `FGCOLOR color `: Sets background/foreground color. Color can be a name (e.g., "red") or RGB (`R, G, B`). **Named colors only work for `BGCOLOR`/`FGCOLOR`** — see "Named Colors" below.
*   `RESETCOLOR` / `INVERTCOLOR`
*   `SETCOLORSPACE "name"`: Selects the palette used for named-color lookups in `BGCOLOR`/`FGCOLOR`. See "Named Colors" below for valid space names and their color lists.
*   `DPSET x, y, color, [draw_immediately] `: Draw pixel. `color` is an `R, G, B` triplet — **string names are NOT supported here.**
*   `DLINE corner1[2], corner2[2], color, [draw_immediately]`: Draw line. `color` is `R, G, B` only.
*   `DOVAL corner[2], radX, radY, color, [fill], [fillcolor], [draw_immediately]`: Draw oval. `color`/`fillcolor` are `R, G, B` only.
*   `DCIRCLE corner[2], radius, color...`: Draw circle. `color` is `R, G, B` only.
*   `DBOX corner1[2], size[2], color...`: Draw rectangle. `color` is `R, G, B` only.

### Named Colors (`BGCOLOR` / `FGCOLOR` only)

`BGCOLOR`/`FGCOLOR` can take a string color name instead of `R, G, B`. The name is looked up **case-insensitively** in the currently active color space, selected with `SETCOLORSPACE "name"`. Pixel-drawing commands (`DPSET`, `DLINE`, `DOVAL`, `DCIRCLE`, `DBOX`) do **not** accept named colors — always pass `R, G, B` to those.

| Space | Names |
| :--- | :--- |
| `ibmpc` | black, blue, green, cyan, red, magenta, brown, light_gray, dark_gray, light_blue, light_green, light_cyan, light_red, light_magenta, yellow, white |
| `c64` | black, white, red, cyan, violet, green, blue, yellow, orange, brown, light_red, darkgrey, grey, light_green, light_blue, light_grey |
| `zx` | black, blue, red, magenta, green, cyan, yellow, white, bright_black, bright_blue, bright_red, bright_magenta, bright_green, bright_cyan, bright_yellow, bright_white |
| `appleii` | black, red, blue, purple, green, gray, medium_blue, light_blue, brown, orange, light_gray, pink, light_green, yellow, aqua, white |
| `atari2600` | black, white, red, cyan, purple, green, blue, yellow |
| `msx` / `msx_mono` | transparent, black, green, "light green", blue, "light blue", "dark red", cyan, red, "light red", yellow, "light yellow", "dark green", magenta, gray, white |
| `to7` | black, red, green, yellow, blue, pink, cyan, white |
| `cpc` / `cpc_mono` | black, blue, bright_blue, red, magenta, mauve, bright_red, purple, bright_magenta, green, cyan, sky_blue, yellow, white, pastel_blue, orange, pink, pastel_magenta, bright_green, sea_green, bright_cyan, lime, pastel_green, pastel_cyan, bright_yellow, pastel_yellow, bright_white |

**Note:** MSX color names use spaces (e.g. `"light green"`), while every other space uses underscores (e.g. `"light_green"`) — this inconsistency comes from the source data and is not a typo.
*   `SCREENWIDTH()`, `SCREENHEIGHT()`: Returns character grid dimensions.
*   `DSCREENWIDTH()`, `DSCREENHEIGHT()`: Returns pixel dimensions.

### Sprites (Software-composited)
Sprites are drawn over the background and retain their Z-order. Loading is asynchronous.
*   `SPRITELOAD "name", "path/to/image.png"`: Starts loading a PNG texture into the sprite cache.
*   `SPRITESTATUS("name")`: Returns 1 if the sprite is fully loaded and ready to use, 0 otherwise.
*   `SPRITE "name", x, y, z`: Draws/Updates a sprite at the specified pixel coordinates and Z-index layer.
*   `SPRITEREMOVE "name"`: Removes a sprite from the screen.
*   `SPRITECOLLISIONCOUNT("nameA", "nameB")`: Returns the number of colliding cell pairs from the last collision check between two sprites.
*   `SPRITECOLLISIONDATA("nameA", "nameB")`: Returns a flat array `[colA, rowA, colB, rowB, ...]` of cell-coordinate pairs for every collision point. Call inside a `ONSPRITECOLLISION` handler. Cell coordinates are sprite-local (4×4 px cells, 0,0 = top-left of sprite).

---

## 7. VR & Cabinet Specific Functions

AGEBasic can interact directly with the Age of Joy 3D environment.

### Controls & Input
*   `CONTROLACTIVE("ID", [port])`: Returns true if a specific Libretro button (e.g., `CONTROLACTIVE("JOYPAD_UP")`, `CONTROLACTIVE("JOYPAD_B", 1)`) is pressed. Includes automatic 250ms debouncing.
*   `CONTROLRUMBLE("ID", amplitude, duration)`: Triggers controller haptics.

**The `port` concept:** `port` is an optional argument (default `0`) passed separately from the button `"ID"` — it is **not** appended to the ID string yourself. For the directional pad (`JOYPAD_UP`/`DOWN`/`LEFT`/`RIGHT`), `port` selects which physical thumbstick/D-pad drives it: **port `0` = left controller stick, port `1` = right controller stick**. For every other button ID (`JOYPAD_A`, `JOYPAD_B`, etc.) port is normally left at its default `0`. Same `port` argument applies to `ONCONTROL("ID", type, [port])`.

**Available control IDs** (from `LibretroControlMapDictionnary.cs`), for use with `CONTROLACTIVE`/`ONCONTROL`:

*   **Joypad buttons:** `JOYPAD_A`, `JOYPAD_B`, `JOYPAD_X`, `JOYPAD_Y`, `JOYPAD_START`, `JOYPAD_SELECT`, `JOYPAD_UP`, `JOYPAD_DOWN`, `JOYPAD_LEFT`, `JOYPAD_RIGHT`, `JOYPAD_L`, `JOYPAD_R`, `JOYPAD_L2`, `JOYPAD_R2`, `JOYPAD_L3`, `JOYPAD_R3`
*   **Rumble:** `JOYPAD_LEFT_RUMBLE`, `JOYPAD_RIGHT_RUMBLE`
*   **Cabinet:** `EXIT`, `INSERT`
*   **Mouse:** `MOUSE_X`, `MOUSE_Y`, `MOUSE_LEFT`, `MOUSE_RIGHT`, `MOUSE_MIDDLE`, `MOUSE_WHEELUP`, `MOUSE_WHEELDOWN`, `MOUSE_HORIZ_WHEELUP`, `MOUSE_HORIZ_WHEELDOWN`, `MOUSE_BUTTON_4`, `MOUSE_BUTTON_5`
*   **Lightgun:** `LIGHTGUN_AUX_A`, `LIGHTGUN_AUX_B`, `LIGHTGUN_AUX_C`, `LIGHTGUN_DPAD_UP`, `LIGHTGUN_DPAD_DOWN`, `LIGHTGUN_DPAD_LEFT`, `LIGHTGUN_DPAD_RIGHT`, `LIGHTGUN_START`, `LIGHTGUN_SELECT`, `LIGHTGUN_TRIGGER`, `LIGHTGUN_RELOAD`
*   **Other:** `MODIFIER`, `KEYB-UP`, `KEYB-DOWN`, `KEYB-LEFT`, `KEYB-RIGHT`

### Player & Room
*   `PLAYERGETHEIGHT()` / `PLAYERSETHEIGHT(h)`
*   `PLAYERGETCOORDINATE("X"|"Z")` / `PLAYERSETCOORDINATE("X"|"Z", val)`
*   `PLAYERLOOKAT(partName)`: Forces the player camera to look at a specific cabinet part.
*   `PLAYERTELEPORT(roomName)`: Teleports player.
*   `ROOMNAME()` / `ROOMCOUNT()` / `ROOMGETNAME(idx)`

### Cabinet Parts Manipulation
*   `CABPARTSCOUNT()` / `CABPARTSNAME(idx)` / `CABPARTSPOSITION("name")`
*   `CABPARTSENABLE(part, bool)`: Enables/Disables a 3D model part.
*   `CABPARTSSETCOORDINATE(part, "X|Y|Z", val)`
*   `CABPARTSSETROTATION(part, "X|Y|Z", angle)`
*   `CABPARTSSETTRANSPARENCY(part, percent)`
*   `CABPARTSSETCOLOR(part, R, G, B)`
*   `CABPARTSEMISSION(part, bool)`
*   `CABPARTSAUDIOPLAY(part)` / `CABPARTSAUDIOSTOP(part)`

`CABPARTSROTATE`/`CABPARTSSETROTATION` (and their `GLOBAL` counterparts) always require a `part` argument —
there is no `part` value that means "the whole cabinet," since parts are resolved from the cabinet's
first-level children only. To rotate the cabinet **as a single unit** (its root object, carrying every part
with it), use the whole-cabinet commands instead:

*   `CABSETROTATION("X|Y|Z", angle)`: Sets the whole cabinet's local rotation on an axis, relative to its placement rotation (absolute, not additive).
*   `CABROTATE("X|Y|Z", angle)`: Rotates the whole cabinet locally by a relative angle.
*   `CABGETROTATION("X|Y|Z")`: Reads the whole cabinet's current local rotation delta (degrees) on an axis, relative to its placement rotation.
*   `CABSETGLOBALROTATION("X|Y|Z", angle)` / `CABGETGLOBALROTATION("X|Y|Z")`: Same, but in world space.

### Cabinet Registry & Replacement
AGEBasic can inspect and change which cabinet game occupies each position in a room, and swap the live 3D cabinet without going through the in-VR Configuration Room UI.

**Room-scoped (affects only the currently running room):**
*   `CABROOMCOUNT()`: Number of cabinet positions in the current room.
*   `CABROOMGETNAME(position)`: Cabinet DB name currently loaded at `position` in this room (`""` if none).
*   `CABROOMREPLACE(position, cabinetName)`: Immediately swaps the **live 3D cabinet** at `position` in the current room to `cabinetName` (must exist in the cabinet DB). Runs asynchronously (fire-and-forget) — returns `1` once the swap has started, `0` if the room or cabinet name is invalid. **Does not persist the change** — the swap reverts the next time the room reloads unless you also call `CABDBASSIGN` + `CABDBSAVE` for the same room/position.

**Registry-scoped (persisted database, `registry.yaml`, survives room reloads):**
*   `CABDBCOUNT()`: Total number of cabinets available in the cabinet DB folder.
*   `CABDBCOUNTINROOM(room)`: Number of registry entries assigned to `room`.
*   `CABDBGETNAME(index)`: Cabinet directory name at `index` in the sorted list of all cabinets on disk (unrelated to room assignment).
*   `CABDBGETINFO(cabinetName, path)`: Reads a field from `cabinetName`'s `description.yaml` at the given dotted/bracketed field `path` (case-insensitive), e.g. `CABDBGETINFO("pacman", "year")`, `CABDBGETINFO("pacman", "crt.type")`, `CABDBGETINFO("pacman", "parts[3].art.file")` (list indices accept either `[n]` or `.n`). A trailing `.count` segment returns a list's length, e.g. `CABDBGETINFO("pacman", "parts.count")`. **Fails soft, unlike other `CABDB*` functions**: if the cabinet doesn't exist, the path doesn't match a field, or the path resolves to a list/object instead of a leaf value, it logs the problem to the console and returns `""` — it does NOT throw and does NOT stop the running program. A missing/empty list also returns `""` for `.count`, so check for `""` before comparing the result numerically. Note: `description.yaml` currently has no `author`/`description` metadata field — this function only exposes fields that actually exist on `CabinetInformation` (`name`, `year`, `style`, `core`, `crt.*`, `model.*`, `parts[n].*`, etc.).
*   `CABDBSETINFO(cabinetName, path, value)`: Writes `value` into `cabinetName`'s `description.yaml` at the same dotted/bracketed `path` syntax as `CABDBGETINFO`. Missing intermediate objects (e.g. an absent `color:` block) are auto-created; a list index equal to the list's current length appends a new element (e.g. `CABDBSETINFO("test", "parts[2].name", "newpart")` when only 2 parts exist, or `CABDBSETINFO("test", "roms[1]", "pacman.zip")` to add a rom). Booleans accept `0`/`1` or `"true"`/`"false"`. Returns `1` on success, `0` on failure — like `CABDBGETINFO` it never throws; failures are logged to the console. **Caveats**: rewrites the entire yaml file, so comments, original key order, and any unrecognized keys are lost, and fields left at their C# default are written out explicitly; dictionary-valued paths (e.g. `crt.screen.properties.*`) are not supported; writing does **not** change the live 3D cabinet — for the workshop's test cabinet, follow with `WORKSHOPRELOAD()` (see below) to see the change.
*   `CABDBSEARCH(namePart, separator)`: Cabinet names starting with `namePart`, joined with `separator`.
*   `CABDBSEARCHARRAY(namePart)`: Same search, returned as an array.
*   `CABDBGETASSIGNED(room, position)`: Cabinet name assigned to `room`/`position` in the registry (`""` if unassigned).
*   `CABDBADD(room, position, cabinetName)`: Adds a **new** registry entry. Throws a runtime error if `room`/`position` is already occupied — use `CABDBASSIGN` to overwrite instead.
*   `CABDBASSIGN(room, position, cabinetName)`: Assigns `cabinetName` to `room`/`position`, creating the entry if it doesn't exist or overwriting it if it does.
*   `CABDBDELETE(room, position)`: Removes the registry entry at `room`/`position`. Throws a runtime error if nothing is assigned there.
*   `CABDBSAVE()`: Persists all in-memory registry changes (`CABDBADD`/`CABDBASSIGN`/`CABDBDELETE`) to `registry.yaml`. **Required** — those three only mutate memory; without a matching `CABDBSAVE()` the changes are lost the next time the room reloads.
*   `CABDBRELOAD()`: Re-reads `registry.yaml` from disk into memory, discarding any unsaved in-memory changes, and re-scans the cabinet DB folder for unassigned cabinets. **Also reconciles the currently loaded room**: any position whose live 3D cabinet no longer matches what the reloaded registry assigns is swapped in-place (same swap used by `CABROOMREPLACE`). Positions with no registry entry are left untouched. Use when the registry file was modified outside the running script (e.g. by another process or a manual edit) and the script needs to see the current on-disk state, including an immediately updated room.

**Typical pattern to durably swap a cabinet from a script** (mirrors what the in-VR Configuration Room UI does internally):
```basic
10 LET ROOM = ROOMNAME()
20 CALL CABDBASSIGN(ROOM, 3, "SpaceInvaders")
30 CALL CABDBSAVE()
40 CALL CABROOMREPLACE(3, "SpaceInvaders")
50 END
```

### Cross-Cabinet Part Manipulation (`CABROOMPARTS*`)

The `CABPARTS*` functions (§7 "Cabinet Parts Manipulation") only ever act on the cabinet whose own AGEBasic
program is currently running — there is no way to reach a different cabinet with them, and they throw if
called from a standalone runner (Workshop, Configuration Room) with no bound cabinet.

`CABROOMPARTS*` functions provide the same manipulations, but target **any cabinet in the current room by
position** (0-based, same convention as `CABROOMGETNAME`/`CABROOMREPLACE` — room is always the current one,
never an explicit parameter). This lets a Workshop script, a Configuration Room script, or one cabinet's
program manipulate a *different* cabinet's parts. Every function takes the cabinet `position` as its first
argument, followed by the same arguments as its `CABPARTS*` counterpart. All of them throw if no cabinet is
currently loaded at that position (e.g. it's still showing an "out of order" placeholder).

*   `CABROOMPARTSCOUNT(position)` / `CABROOMPARTSNAME(position, idx)`: Enumerate parts of the target cabinet.
*   `CABROOMPARTSENABLE(position, part, bool)`: Enables/disables a part.
*   `CABROOMPARTSGETCOORDINATE(position, part, "X|Y|Z")` / `CABROOMPARTSSETCOORDINATE(position, part, "X|Y|Z", val)`: Local position.
*   `CABROOMPARTSGETGLOBALCOORDINATE(position, part, "X|Y|Z")` / `CABROOMPARTSSETGLOBALCOORDINATE(position, part, "X|Y|Z", val)`: World position.
*   `CABROOMPARTSSETROTATION(position, part, "X|Y|Z", angle)`: Sets local rotation on an axis relative to the part's origin.
*   `CABROOMPARTSROTATE(position, part, "X|Y|Z", angle)`: Rotates the part locally by a relative angle.
*   `CABROOMPARTSGETROTATION(position, part, "X|Y|Z")`: Reads current local rotation on an axis.
*   `CABROOMPARTSSETGLOBALROTATION(position, part, "X|Y|Z", angle)` / `CABROOMPARTSGETGLOBALROTATION(position, part, "X|Y|Z")`: World rotation.
*   `CABROOMPARTSGETTRANSPARENCY(position, part)` / `CABROOMPARTSSETTRANSPARENCY(position, part, percent)`: Transparency 0-100.
*   `CABROOMPARTSSETCOLOR(position, part, R, G, B)`: Sets base color.
*   `CABROOMPARTSEMISSION(position, part, bool)` / `CABROOMPARTSSETEMISSIONCOLOR(position, part, R, G, B)`: Emission.

Example — rotate a neighboring cabinet's door from any running program:
```basic
10 LET DOOR_POS = 3
20 CALL CABROOMPARTSSETROTATION(DOOR_POS, "door", "Y", 90)
30 END
```

**Whole-cabinet rotation by room position** — the `CABROOM*` equivalents of `CAB{SET,GET}{,GLOBAL}ROTATION`/`CABROTATE`
(§7), for rotating a *different* cabinet's root object (not one of its parts) from any running program:

*   `CABROOMSETROTATION(position, "X|Y|Z", angle)` / `CABROOMROTATE(position, "X|Y|Z", angle)`: Set (absolute, from placement) or rotate (relative) the target cabinet's local rotation.
*   `CABROOMGETROTATION(position, "X|Y|Z")`: Reads the target cabinet's current local rotation delta.
*   `CABROOMSETGLOBALROTATION(position, "X|Y|Z", angle)` / `CABROOMGETGLOBALROTATION(position, "X|Y|Z")`: Same, in world space.

### Workshop
*   `WORKSHOPRELOAD([cabinetName$])`: **Workshop room only.** No argument (or the same name as whatever cabinet is currently deployed) redeploys the workshop's cabinet from its on-disk `cabinetsdb/<name>` folder — the intended way to iterate after editing its `description.yaml` with `CABDBSETINFO`. A *different* `cabinetName$` (must already exist under `cabinetsdb/`) summons that cabinet into the workshop slot instead, replacing whatever was there, and persists the choice so it's still there after an app restart. Returns `1` if the request was accepted (the (re)deploy happens within a couple of seconds and rewrites `test.log` with any compile/deploy problems), or `0` if there is no active workshop test-cabinet loader (e.g. called from a different room) or the named cabinet doesn't exist on disk. Once a cabinet is summoned, the workshop slot also auto-reloads on its own after any file changes under that cabinet's folder (a texture, `description.yaml`, a `.bas` file, ...) — no explicit call needed for that case. This check runs every ~15 seconds and debounces: after the first detected change it waits for one more 15s interval with no further changes before reloading, so editing/saving several files in a row triggers one reload, not one per save (worst case ~30s after your last save). Call `WORKSHOPRELOAD()` explicitly if you don't want to wait. **Warning**: never call `WORKSHOPRELOAD()` unconditionally from `debug.bas` — the workshop debug console reruns `debug.bas` whenever `test.log` changes, and an unconditional call there creates an infinite reload loop.

### Emulation & System
*   `GAMEISRUNNING()`: True if a ROM is currently loaded.
*   `PEEK(offset [, region])` / `POKE(offset, val)`: Direct memory access to emulator memory. `region`: `0`=SAVE_RAM (default, backward-compatible), `1`=RTC, `2`=SYSTEM_RAM, `3`=VIDEO_RAM. Example: `PEEK(0x42, 2)` reads offset 0x42 from SYSTEM_RAM.
*   `CABINSERTCOIN()`: Triggers the coin slot logic.
*   `SETCPU(multiplier)`: "Overclocks" execution speed (e.g., `CALL SETCPU(500)` runs 500 lines per frame. Default is `1`).

---

## 8. Audio

AGEBasic exposes four independent audio systems. See `docs/agebasic_audio.md` for the full reference.

### 8.1 Global Audio Mixer (dB buses)

Controls master volume for three Unity audio buses. Values are decibels: `0` = full, `-80` = silence.

*   `AUDIOGAMEGETVOLUME()` / `AUDIOGAMESETVOLUME(db)`
*   `AUDIOMUSICGETVOLUME()` / `AUDIOMUSICSETVOLUME(db)`
*   `AUDIOAMBIENCEGETVOLUME()` / `AUDIOAMBIENCESETVOLUME(db)`

### 8.2 Global Music Player (JukeBox playlist)

Sequential MP3/OGG playlist running across the whole arcade. Files are resolved relative to `<config>/music/`.

*   **Queue:** `MUSICADD(file)`, `MUSICREMOVE(file)`, `MUSICEXIST(file)`, `MUSICCLEAR()`, `MUSICCOUNT()`, `MUSICGETLIST(sep)`, `MUSICADDLIST(list, sep)`, `MUSICADDLISTARRAY(array)`
*   **Playback:** `MUSICPLAY()`, `MUSICNEXT()`, `MUSICPREVIOUS()`, `MUSICRESET()`, `MUSICLOOP(bool)`, `MUSICLOOPSTATUS()`

### 8.3 Cabinet Parts Audio

Plays spatialized sounds from individual cabinet parts declared with a `speaker` block in `description.yaml`.

*   `CABPARTSAUDIOFILE(part, path)` — assign audio file
*   `CABPARTSAUDIOPLAY(part)` / `CABPARTSAUDIOSTOP(part)` / `CABPARTSAUDIOPAUSE(part)`
*   `CABPARTSAUDIOVOLUME(part, 0.0–1.0)`
*   `CABPARTSAUDIOLOOP(part, bool)`
*   `CABPARTSAUDIODISTANCE(part, min, max)` — 3D rolloff distances

### 8.4 SID Player (C64 chiptune)

Plays `.sid` files (PSID/RSID v1–v4) through the cabinet's AudioSource. Supports **multiple named instances** simultaneously. Not available in the Configuration Room context.

**Commands (no parentheses, statement form):**

| Command | Syntax | Description |
| :--- | :--- | :--- |
| `SIDLOAD` | `SIDLOAD name, path` | Load a `.sid` file. Call during setup, not in the game loop. |
| `SIDLOADDATA` | `SIDLOADDATA name, storage` | Load SID from a `DATA` list (embedded bytes). |
| `SIDPLAY` | `SIDPLAY name [, song]` | Start/restart playback. `song` is 1-based. Re-calling restarts — useful for one-shot SFX. |
| `SIDSTOP` | `SIDSTOP name` | Stop (instance stays loaded). |
| `SIDPAUSE` | `SIDPAUSE name` | Pause at current position. |
| `SIDRESUME` | `SIDRESUME name` | Resume from paused position. |
| `SIDUNLOAD` | `SIDUNLOAD name` | Free the instance. Always unload when done. |
| `SIDVOLUME` | `SIDVOLUME name, vol` | Set volume 0–100. |

**Functions (require parentheses):**

| Function | Returns | Description |
| :--- | :--- | :--- |
| `SIDSTATUS(name)` | number | `0`=not loaded · `1`=loaded/stopped · `2`=playing · `3`=paused |
| `SIDTITLE(name)` | string | Title from SID header |
| `SIDAUTHOR(name)` | string | Composer from SID header |
| `SIDRELEASED(name)` | string | Release/copyright from SID header |
| `SIDCOUNT(name)` | number | Total sub-songs in the file |
| `SIDDEFAULTSONG(name)` | number | Default starting sub-song (1-based) |

**Quick example — background music + one-shot SFX:**
```basic
10 SIDLOAD "music", COMBINEPATH(AGEBASICPATH(), "music/theme.sid")
20 SIDLOAD "sfx",   COMBINEPATH(AGEBASICPATH(), "music/zap.sid")
30 SIDVOLUME "music", 70 : SIDVOLUME "sfx", 60
40 SIDPLAY "music"
50 IF CONTROLACTIVE("JOYPAD_B_0") THEN SIDPLAY "sfx", 1
60 SLEEP 0.05 : GOTO 50
```

**Notes:**
- At most 3 active SIDs recommended (CPU budget on Quest).
- Use `AGEBASICPATH()` + `COMBINEPATH()` to build portable paths to `.sid` files.
- Place `.sid` files in `<agebasicpath>/music/` or any accessible path.

---

## 9. Video Playback

Available **only** in cabinets with `crt: type: 19i-agebasic` in `description.yaml`. These cabinets have a `GameVideoPlayer` component. All video commands are no-ops (with a console warning) in other cabinet types.

Video files are stored in the device video folder, returned by `VIDEOPATH()`. On Quest: `/sdcard/Android/data/com.curif.AgeOfJoy/files/video/`.

### Shader behavior

During playback the video shader is active and fills the screen — `PRINT`/`SHOW` output is not visible. Calling `VIDEOPAUSE` or `VIDEOSTOP` restores the CRT shader so AGEBasic drawing commands work normally again.

### Commands

| Command | Syntax | Description |
| :--- | :--- | :--- |
| `VIDEOLOAD` | `VIDEOLOAD path [, invertX [, invertY]]` | Load a video file. `invertX`/`invertY` are 0 or 1 (default 0). Resets playback state. |
| `VIDEOPLAY` | `VIDEOPLAY` | Start or resume playback. If the clip is not yet prepared, preparation begins asynchronously and playback starts automatically when ready — no retry needed. |
| `VIDEOPAUSE` | `VIDEOPAUSE` | Pause at current position. Restores the CRT shader so the HUD is visible. |
| `VIDEOSTOP` | `VIDEOSTOP` | Stop and rewind to position 0. Restores the CRT shader. |
| `VIDEOSEEK` | `VIDEOSEEK seconds` | Jump to the given position in seconds. No-op if the video is not prepared. |
| `VIDEOLOOP` | `VIDEOLOOP 1\|0` | Enable (`1`) or disable (`0`) looping. Default is on. |

### Functions

| Function | Returns | Description |
| :--- | :--- | :--- |
| `VIDEOPATH()` | string | Path to the video folder on the device. |
| `VIDEOTIME()` | number | Current playback position in seconds. |
| `VIDEODURATION()` | number | Total clip duration in seconds. |
| `VIDEOSTATUS()` | number | `0`=not loaded · `1`=stopped/ready · `2`=playing · `3`=paused |
| `VIDEOLOOPSTATUS()` | number | `1` if looping is enabled, `0` if not. |

### Typical usage pattern

```basic
10 REM Load file list on coin insert
20 LET FILES = GETFILESARRAY(VIDEOPATH(), 0)
30 LET FILE_COUNT = LEN(FILES)
40 IF FILE_COUNT = 0 THEN END
50 LET CURR_FILE = 0
60 VIDEOLOAD COMBINEPATH(VIDEOPATH(), FILES[CURR_FILE])
70 VIDEOPLAY
80 END

100 REM Stop (Y button) — shows HUD
110 VIDEOSTOP
120 CLS
130 PRINT 0, 5, "[ STOPPED ]"
140 SHOW
150 END

200 REM Next file (D-pad Down)
210 LET CURR_FILE = MOD(CURR_FILE + 1, FILE_COUNT)
220 VIDEOLOAD COMBINEPATH(VIDEOPATH(), FILES[CURR_FILE])
230 VIDEOPLAY
240 END
```

### Notes

- `VIDEOSTATUS()` returns `1` (stopped/ready) both when the video has never played and when `VIDEOSTOP` was called. Use it in timer events to decide whether to draw the HUD.
- `VIDEOLOOP` takes effect on the next `VIDEOPLAY` call. If the video is already prepared and not paused, it applies immediately.
- `VIDEOSEEK` only works after the clip is prepared. Call it after `VIDEOPLAY` has had time to prepare, or from a button event after playback has started.
- Supported formats depend on the platform. `.mp4` (H.264) works reliably on both Windows (editor) and Android (Quest). `.mkv` may work on Quest but is not supported in the Unity editor.
- See `docs/agebasic_video.md` for the full developer/implementation reference.
