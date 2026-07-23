# AGEBasic Implementation Documentation

Based on the provided source code, AGEBasic is a custom-built, interpreted scripting language designed specifically for the "Age of Joy" VR Arcade ecosystem. It allows users to write BASIC-like scripts to interact with virtual arcade cabinets, the VR environment, and minigames, while safely running within the Unity engine without degrading VR performance.

## 1. Architecture & Core Components

The implementation is structured around several key C# classes that separate the engine, program state, parsing, and execution.

### `basicAGE.cs` (The Engine)
This Unity `MonoBehaviour` serves as the execution engine and environment manager.
- **Context Injection:** It collects references to Unity game objects and Age of Joy systems (e.g., `ScreenGenerator`, `CabinetsController`, `Teleportation`, `PlayerController`) and packages them into a `ConfigurationCommands` object. This object acts as the bridge between the AGEBasic scripts and the Unity world.
- **Execution Loop:** Execution is handled via a Unity Coroutine (`runProgram`). To maintain high frame rates in VR, it strictly budgets execution time. It limits the number of lines executed per frame based on a CPU multiplier (`MaxLinesPerFrame * cpuPercentage`) and enforcing a hard time limit (`MaxMillisecondsPerFrame`, default 2.0ms).
- **Safety & UI Integration:** Programs can be executed natively via cabinets or triggered via UI management systems like the `ConfigurationController`. The `basicAGE` engine publishes UnityEvents (`OnProgramStarted`, `OnProgramEnded`) which are safely invoked (`?.Invoke()`) since non-cabinet UI runners might not instantiate these listeners. The UI behavior tree tracks engine execution state via `IsRunning()` and `IsRunningInBackground()` (detecting if `eventCoroutine` is active).
- **Sub-program execution (RUN command):** To support running sub-programs via the `RUN` command, `basicAGE` implements a `ProgramContext` stack (`programContextStack`). When a new program is called, the current program's state (variables, call stack, execution line) is pushed to the stack, and execution immediately switches to the new script. If the sub-script doesn't throw an error, execution returns to the parent program once the sub-script finishes.

### `basicAGEProgram.cs` (The Program Representation)
Represents a single parsed `.bas` script.
- **Parsing:** Reads the file line by line, stripping comments (`'`) early. It enforces strictly ascending line numbers. Lines not starting with a number are treated as continuations of the previous logical line.
- **Lexical Analysis:** Uses a Regular Expression (`Regex.Matches`) to tokenize the code into strings, numbers, identifiers, operators, and punctuation.
- **Compilation for Fast Execution:** After parsing lines into a `SortedDictionary`, it compiles them into flat arrays (`parsedLineNumbers` and `parsedCommands`). This optimizes the runtime loop by avoiding dictionary lookups.
- **Jumps:** When a jump occurs (`GOTO`, `GOSUB`), it uses a highly efficient `Array.BinarySearch` on the compiled line numbers array to locate the target index.
- **Profiling:** Uses `CodeExecutionTracker` to monitor execution speed (lines per second) and total executed lines.

### `basicTokens.cs` (The Tokenizer)
The `TokenConsumer` class is a sequential token reader used during the parsing of individual statements. It provides helper methods (`Next()`, `ConsumeIf()`, `TheNextIs()`) that allow individual command classes to validate and parse their specific grammar structures.

### `basicCommands.cs` & `functionBase.cs` (Command Registry)
- **Registry:** `Commands` acts as a static factory that maps string tokens (e.g., `"PRINT"`, `"LET"`, `"ABS"`) to their concrete C# implementations (e.g., `CommandPRINT`, `CommandFunctionABS`).
- **Base Classes:** All commands implement the `ICommandBase` interface. Functions inherit from `CommandFunctionBase`, which is further refined into `CommandFunctionExpressionListBase` and `CommandFunctionSingleExpressionBase` to standardize parameter parsing (e.g., demanding enclosing parentheses).
- **Validation Utility:** `FunctionHelper` provides static methods to validate runtime arguments (e.g., `ExpectedNumber`, `ExpectedArraySize`) and convert types (like converting a 3-element BasicValue array into a Unity `Color32`).

### `CabinetAGEBasic.cs` (Event Handling and Linking)
This class manages the lifecycle of AGEBasic scripts attached to a specific arcade cabinet.
- **Initialization:** Reads configuration from `description.yaml` (via `CabinetAGEBasicInformation`). It pre-loads variables using `IngestVariables` and registers all declarative events defined in the YAML by adding them to the `basicAGE` event list.
- **Event Coroutine (`RunEvents`):** Manages a continuous background loop that checks conditions and executes mapped scripts based on cabinet interactions. It supports dynamic registration of events at runtime. If an event script is triggered, it invokes a new `runProgram` coroutine and prevents overlapping runs by sequentially yielding (`while (IsRunning()) yield return null;`).
- **SHUTDOWN vs END:** The engine makes a hard behavioral distinction between `END` and `SHUTDOWN`. Calling `END` sets `config.stop = true`, gracefully exiting the local program context but allowing the `eventCoroutine` loop to persist. Calling `SHUTDOWN` sets `config.shutdown = true` and `config.stop = true`, which forcefully kills the local program context, executes a `StopCoroutine` on the background `eventCoroutine`, and fully clears the registered events queue and file pointers. Any host code that needs to detect "the program is done" must account for this — see *"Detecting Program Completion (END vs SHUTDOWN)"* under section 2.
- **Event Types:** Implements an extensible event system through the `Event` base class and its derived forms:
  - `OnTimer`, `OnAlways` (Time-based triggers)
  - `OnControlActivePressed`, `OnControlActiveHeld`, `OnControlActiveReleased` (MAME input mapping)
  - `OnInsertCoin` (Hooked to `CoinSlotController`)
  - `OnLightGunStart`, `OnLightGunStay`, `OnLightGunExit` (Lightgun targeting)
  - `OnCollisionStart`, `OnCollisionStay`, `OnCollisionEnd` (Physical interactions)
  - `OnPlayerTouchStartEvent`, `OnPlayerTouchEndEvent` (VR Hand interactions - Hover)
  - `OnPlayerGrabStartEvent`, `OnPlayerGrabEndEvent` (VR Hand interactions - Select)
  - `OnCustom` (Programmatically triggered events)
- **Condition Evaluation:** Before an event script runs, its optional `when` clause is evaluated, supporting logical structures (`and`, `or`, and variable comparisons).
- **Shared variable space, isolated control flow:** When an event is triggered, it runs its specified script using a dedicated `Event.PrepareToRun()` call, which gives it its own execution pointer and call stack (`Gosub`), but the `BasicVars` reference is not copied — it is the *same* `BasicVars` instance held by `CabinetAGEBasic.vars`. So execution state (current line, `GOSUB` stack) is isolated per event/program, but variable values are shared with every other program running for that cabinet. See "Shared Variable Space Across Executions" below.

### `AGEBasicScreenController.cs` and `AGEBasicCabinetController.cs`
Both scripts share a lot of DNA, specifically in how they connect the basicAGE engine to Unity's ecosystem. However, they serve two distinct types of arcade cabinets, which leads to several key operational differences.

#### `AGEBasicCabinetController.cs`
This `MonoBehaviour` binds the AGEBasic engine to standard interactive arcade cabinets. It allows AGEBasic to run alongside or in place of Libretro emulation.
Usage on `screen19i` gameobjects serie and `noScreen`
- **Hardware Integration:** Connects the script engine to physical cabinet inputs (`LibretroControlMap`), the coin slot (`CoinSlotController`), and VR peripherals like lightguns (`LightGunTarget`).
- **Lifecycle Management:** Uses a BehaviorTree (`runBT`, `buildScreenBT`) that ticks continuously. It waits for the player to insert a coin, subsequently executing the `cabinetAGEBasic.ExecInsertCoinBas()` hook, changing VR hands/models (e.g., swapping to a lightgun), and mapping the control scheme.
- **Execution Monitoring:** Continually evaluates if the script is still running (or running in the background via events) and listens for user interruptions (like pressing the EXIT button). When execution concludes, it cleanly unbinds the controls and calls `cabinetAGEBasic.ExecAfterLeaveBas()`.

#### `AGEBasicScreenController.cs`
Designed for custom arcade cabinets dedicated *exclusively* to running AGEBasic programs (i.e., non-Libretro cabinets).
- **Attraction Mode Video:** Integrates a `GameVideoPlayer` to loop a designated attraction video when no script is active. It actively monitors player proximity (`DistanceMinToPlayerToActivate`) and line-of-sight (`isPlayerLookingAtScreen4`) to pause or play the video to conserve VR performance.
- **Seamless Handoff:** Once a coin is inserted or a program triggered, the controller immediately pauses the video player, configures custom CRT and video shaders, activates the `basicAGE` context, and runs `cabinetAGEBasic.ExecInsertCoinBas()`.
- **Dedicated Hardware Maps:** Similar to `AGEBasicCabinetController`, it handles the controller mappings pulling from global control maps, the game database, or a cabinet's assigned control scheme to ensure AGEBasic logic correctly maps to physical inputs.

#### `AGEBasicScreenController.cs` versus `AGEBasicCabinetController.cs`
 Architectural & Operational Focus
   * AGEBasicScreenController is designed for traditional video-based arcade cabinets (i.e. those with a screen). It actively manages a display
     (Renderer), video playback (attraction mode), and dynamic shader effects (e.g., CRT, neon). It uses player gaze and proximity to trigger events.
   * AGEBasicCabinetController is a lightweight version designed for non-video, logic-driven, or mechanical cabinets (e.g., electro-mechanical games,
     pinball). It completely omits the video rendering pipeline and focuses solely on running the underlying AGEBasic logic and managing physical
     interactions (coins, lightguns, inputs).


### `ConfigurationController.cs`
Serves as the central user interface (UI) manager inside the virtual configuration room. Crucially, it acts as a standalone runner for AGEBasic programs outside the context of an arcade cabinet.
- **In-VR Runner:** Provides an interactive UI menu to load, compile (`AGEBasic.ParseFiles`), and execute `.bas` scripts globally.
- **Diagnostics:** Offers visual widgets to inspect the last thrown compilation error (`CompilationException`) or runtime error (`LastRuntimeException`), making it an essential tool for script developers debugging inside VR.
- **Autostart:** Automatically executes a designated startup script (defined via `config.agebasic.afterLoad` in the global configuration) as soon as the configuration room is fully initialized.
- **Global Systems Management:** Maps deeply into the global ecosystem, handling non-scripting tasks such as setting audio levels, configuring NPC behaviors, managing player locomotion, tweaking screen shader attributes, and adjusting environmental lighting.
- **Test-run completion detection:** The "AGEBasic > run" screen (`onRunAGEBasicRunning` BT state) and the editor-only `EditorWaitAGEBasicTestFinished()` helper decide when a manually-run script is "done" using the same rule described in *"Detecting Program Completion"* below — see that section before touching either of these.



### `basicConfigurationCommands.cs` (The Shared State Object)
Passed to almost every object in the interpreter. Holds the current state of execution.
- Contains references to global dependencies (`ScreenGenerator`, `CabinetsController`, `ControlMap`, etc.).
- Maintains the Call Stack (`Gosub`).
- Controls program flow interrupts (`stop`, `shutdown`, `JumpTo`, `JumpNextTo`, `SleepTime`).
- File I/O pointers (`filepointer` array).
- Signals for sub-program execution (`RunSubProgramPath`, `RunSubProgramLine`).
- Holds the list of active `Event` instances.

## 2. Execution Model

1. **Parsing Phase:** When loaded, the script is immediately parsed. Syntax errors throw a `CompilationException`. Multiple commands on a single line separated by `:` are handled sequentially during this phase.
2. **Initialization:** Calling `PrepareToRun()` resets the execution pointer, clears or injects variables (`BasicVars`), and resets the call stack (`Gosub`).
3. **Coroutine Batching:** The `runProgram` Coroutine loops through commands. In a single Unity frame, it executes commands sequentially until:
   - The batch limit is reached (determined by `SETCPU`).
   - The time budget (ms) is exceeded.
   - A command explicitly yields (e.g., `SLEEP` returns a `WaitForSeconds`).
   - The program ends or faults.
4. **Variable State:** Variables (`BasicValue`) are dynamically typed and can hold Numbers (doubles), Strings, or Arrays of `BasicValue`. They are stored in a `BasicVars` dictionary and implicitly declared upon first assignment (arrays require `DIM`).

### Shared Variable Space Across Executions

**All AGEBasic programs running for a given cabinet share a single `BasicVars` instance — there is no per-program or per-event variable isolation.**

`CabinetAGEBasic` owns exactly one `BasicVars vars` field, created once (`CabinetAGEBasic.cs`) and reused for the lifetime of the cabinet (only repopulated by `IngestVariables`, never replaced). Every execution path shares it by reference rather than copying it:

- **Insert-coin / setup script:** `ExecInsertCoinBas()` → `execute()` calls `AGEBasic.Run(prgName, vars, ...)`, passing the cabinet's `vars` directly.
- **`ONEVENT` handlers:** `RegisterYamlEvents()` constructs each `Event` via `EventsFactory.Factory(info, vars, AGEBasic)`, storing the same `vars` reference on the `Event`. When the event fires, `basicAGE.RunEvents()` runs the event's program with that same `Event.vars` object — not a copy.
- **`RUN` command:** In `basicAGE.runNextLineCurrentProgram`, the sub-program is prepared with `newProg.PrepareProgramToRun(running.Vars, line)`. Inside `AGEProgram.PrepareProgramToRun`, when a non-null `pvars` is passed, the program's `vars` field is simply reassigned to that object (`vars = pvars;`) rather than cloned. A fresh `BasicVars()` is only created when `Run()` is invoked with `pvars == null` (top-level/standalone execution with no explicit vars, e.g. editor-mode test runs).

**Practical consequences:**
- `LET X = 5` in the setup script is immediately visible to any `ONEVENT` handler that fires afterward, and to any program invoked via `RUN`.
- A `RUN`-called sub-program can set a variable and the caller will see the new value once `RUN` returns — popping the program context (`PopProgramState`) restores the call stack and line pointer, but never touches `vars`.
- Arrays (`DIM`) are shared by reference as well, since they live in the same underlying dictionary.
- Because of this sharing, event handlers and `RUN` sub-programs must be written defensively: use distinct variable names to avoid accidental collisions with other events/programs on the same cabinet, and don't assume a variable starts uninitialized just because a new event fired or a new program was `RUN`.
- `Shutdown()`/`ResetState(true)` clears the program-context stack and registered events but does **not** clear `vars` — stale values can persist across an `afterInsertCoin` restart unless variables are explicitly reset via YAML re-ingestion (`IngestVariables`).

### Detecting Program Completion (END vs SHUTDOWN) — a recurring pitfall

Any host code that runs an AGEBasic program and needs to know when it's "finished" (to redraw a menu, restore input control, free a cabinet, etc.) must **not** simply wait for both `basicAGE.IsRunning()` and `basicAGE.IsRunningInBackground()` to become false. That pattern looks intuitive but is wrong, and has caused real freezes in `ConfigurationController.cs`'s AGEBasic test-run screen.

- `IsRunning()` reflects the **foreground program** (`running != null` or `Status` is `Running`/`WaitingForStart`).
- `IsRunningInBackground()` reflects whether the **event loop** is alive (`eventCoroutine != null && events.Count > 0`) — this is true for as long as *any* `ONEVENT` handler remains registered, regardless of whether one is actively executing right now.

The two commands that end a foreground program mean different things for the event loop, and host code must branch on which one happened:

| How the program stopped | Events still registered? | Meaning |
|---|---|---|
| `END` | Yes | **Not finished.** The script intentionally left handlers running and is waiting on them — `END` only exits the current program's scope, it never touches the event loop. Treat this as "still running." |
| `END` | No | Finished. Nothing left to do. |
| `SHUTDOWN` | (always none — `Shutdown()` clears `events` itself) | Finished. Equivalent to the "no events" case. |
| Uncaught runtime exception | Yes or no | Finished — but treat any registered events as leftover, incomplete state from the crash (e.g. the script died partway through its own `ONEVENT` setup), not an intentional wait. Clean them up (`Shutdown()`) before reporting completion. |

The correct completion check (see `ConfigurationController.cs`, `onRunAGEBasicRunning` BT state, and `EditorWaitAGEBasicTestFinished()` for the reference implementation):

```csharp
if (AGEBasic.IsRunning())
    return Continue; // still executing

// Foreground stopped. An END that left events registered means the program is
// intentionally still active, waiting on those events — not finished.
if (AGEBasic.LastRuntimeException == null && AGEBasic.IsRunningInBackground())
    return Continue;

// A crash is different: leftover events are incomplete setup, not an intentional wait.
if (AGEBasic.LastRuntimeException != null && AGEBasic.IsRunningInBackground())
    AGEBasic.Shutdown();

// truly finished: report result, hand control back.
```

Getting this wrong has two failure modes, both observed in practice:
1. **Waiting for `IsRunningInBackground()` unconditionally** — a script that does normal, intentional `ONEVENT` + `END` (e.g. an interactive picker that waits for further input events) never lets the background flag clear, so the host screen hangs forever waiting for "completion" that was never coming.
2. **Force-`Shutdown()`ing as soon as the foreground ends, regardless of why** — kills a legitimately-still-running script's event handlers, so an interactive program that intentionally ended its setup phase via `END` gets cut off mid-interaction (input events silently stop firing) instead of continuing to run.

A host-side safety-net timeout (e.g. `ConfigurationController`'s 30-minute `AGEBasicRunTimeout`) is still worth keeping regardless — it protects against a script that legitimately never calls `SHUTDOWN` and is simply abandoned by the developer testing it.

**Known call sites applying this rule** (check these first before adding a new one elsewhere):
- `ConfigurationController.cs` — `onRunAGEBasicRunning` BT state, and `EditorWaitAGEBasicTestFinished()` (editor-only).
- `AGEBasicCabinetController.cs` — the `"not running anymore?"` condition inside `buildScreenBT()`'s `RepeatUntilSuccess("Until player or program exit")`. Same rule, applied to a live gameplay cabinet rather than a one-shot test: an `END` that leaves `ONEVENT` handlers registered (e.g. lightgun/collision/timer handlers meant to run for the whole play session) means the game is still going, not over. The subsequent `"END Program"` step always calls `cabinetAGEBasic.Stop()` (→ `AGEBasic.Shutdown()`) regardless of *why* the condition tripped, so it's safe for that condition to fire promptly on a crash. (`SuspendAttractAndPlaybackForTransition()` in the same file ORs `IsRunning()`/`IsRunningInBackground()` too, but only to decide *whether* to bother running cleanup, not as a wait-loop exit condition — over-triggering there is harmless, so it doesn't need this treatment.)

## 3. Unity & VR Integration

AGEBasic achieves its capabilities by acting as a safe proxy to complex Unity components. The `ConfigurationCommands` context allows commands to:
- Modify `GameObject` properties (position, rotation, transparency, emission) via `CabinetsController`.
- Draw pixels, lines, and text to virtual CRT screens via `ScreenGenerator`.
- Read controller inputs and trigger haptics via Libretro wrappers.
- Manage Audio and Player Teleportation.

### Sprite Rendering (`ScreenGenerator.cs`)
AGEBasic supports hardware-accelerated, software-composited 2D sprites.
- **Background Buffer:** Standard drawing commands (`PRINT`, `DLINE`, `DPSET`) draw permanently into a `baseTexture`.
- **Compositing:** The `SHOW` command (via `DrawScreen`) acts as a compositor. It takes the `baseTexture`, paints all active sprites over it (sorted by Z-index), and pushes the final result to the screen. Moving a sprite does not permanently overwrite the background, allowing smooth, trail-free animation.
- **Caching:** Sprite textures (`SPRITELOAD`) integrate with the game's `CabinetTextureCache` and `TextureDiskCache`. They are requested asynchronously and must be kept CPU-readable (`makeNoLongerReadable: false`) for the software compositor to perform Alpha blending.

## 4. Control Flow Internals
- **Fractional Jumps (IF/THEN/Multi-commands):** AGEBasic handles inline statement blocks (e.g. `THEN` or `:` separated commands) by leveraging fractional line numbers. Commands appended on the same logical line are inserted into the `SortedDictionary` by incrementing the floating-point line number by a tiny delta (`AGEProgram.MinJump`). This clever hack ensures commands execute sequentially on the same "line" without necessitating complex nested AST structures.
- **Exact Jumps:** `GOTO 100`: Uses `config.JumpTo`. The program searches exactly for line number 100.
- **Fuzzy Jumps:** Internally, a jump can use `config.JumpNextTo`. This is used when a line number might not exist (e.g. exiting a `FOR` loop), telling the engine to find the *first line number greater than or equal to* the requested number.
- **Dynamic Events:** `ONEVENT`: Registers a new `Event` at runtime. The command parses the event type and parameters, creates an `EventInformation` object, and uses `EventsFactory` to instantiate the appropriate `Event` subclass. This instance is added to `config.events` and initialized.

## 5. Extending AGEBasic

To add new functionality to AGEBasic, developers follow these patterns:

### Adding a New Command
1. Create a new class in `Assets/curif/LibRetroWrapper/basic/Commands/` inheriting from `CommandBase` (or specialized bases like `CommandSingleExpressionBase`).
2. Implement `Parse(TokenConsumer tokens)` to handle the syntax.
3. Implement `Execute(BasicVars fractional)` to perform the action.
4. Register the new command in `Commands` static constructor in `basicCommands.cs`.

### Adding a New Function
1. Create a new class in `Assets/curif/LibRetroWrapper/basic/functions/` inheriting from `CommandFunctionBase` (or similar).
2. Implement `Execute(BasicVars vars)` to return a `BasicValue`.
3. Register the new function in `Commands` static constructor in `basicCommands.cs`.

### Adding a New Event Type
1. Define a new `Event` subclass in `basicEvents.cs`.
2. Update `EventsFactory.Factory` in `basicEvents.cs` to handle the new event ID string.
3. Update `CommandONEVENT.Execute` in `ONEVENT.cs` to correctly map BASIC parameters to the `EventInformation` fields.
4. Add the new event ID to `validEvents[]` in `CabinetAGEBasic.cs` if it should be declarable in `description.yaml`.

### Memory-change events (`on-memory-change`) — Experimental

> **Status: experimental and untested in production.** The feature is fully implemented and safe to ship — cabinets that do not declare `on-memory-change` events (or `ONMEMORY`) are completely unaffected. However, the happy path (an event actually firing) has not been verified on a real device with a core that exposes memory maps.

The `OnMemoryChange` event class (`basicEvents.cs`) polls a single emulator memory byte each frame and triggers when the value changes. The new value is injected into the named AGEBasic variable before the handler runs.

**Address resolution order:**
1. If `EventInformation.cheat` is set → look up address/region from `ConfigurationCommands.CheatAddresses` (populated by `MameCheatXmlParser` from `cheat.xml`).
2. Otherwise use `EventInformation.address` and `EventInformation.region` directly.

**Memory access fallback:** `LibretroMameCore.getMemory(region, offset)` first tries the standard LibRetro 4-region API (`wrapper_get_memory_size`/`wrapper_get_memory_data`). If the region returns size=0 it falls back to `wrapper_read_memory_map()` in the native C layer, which searches the descriptors registered via `RETRO_ENVIRONMENT_SET_MEMORY_MAPS` (cmd 36 | RETRO_ENVIRONMENT_EXPERIMENTAL). This is the same mechanism RetroArch uses for its cheat engine.

**Why MAME cores don't work:** Neither `mame2003-plus` nor `mame2010` call `RETRO_ENVIRONMENT_SET_MEMORY_MAPS`. This is because Pugsy's Cheats are designed for MAME's *internal* native cheat engine, which has direct access to the emulated CPU address space — a completely separate system from the LibRetro memory map API. The MAME LibRetro cores simply do not bridge these two systems.

**When it can work:** The `fbneo` core (Final Burn Neo) may call `RETRO_ENVIRONMENT_SET_MEMORY_MAPS` for some games. If it does, `on-memory-change` events will function correctly for those games. The addresses in Pugsy's Cheats XML are real hardware CPU addresses (e.g. `maincpu.mb@0x8880` means "byte at CPU address 0x8880"), not MAME-internal addresses. Since both MAME and FBNeo emulate the same physical hardware, those addresses are identical in both emulators. A `cheat.xml` from Pugsy's site will therefore work with `fbneo` if the core exposes its memory maps — no address translation needed.

**Failure handling:** If `getMemory()` throws on the first call (memory not available), the event sets a `permanentlyFailed` flag. `Condition()` returns `false` permanently, stopping all polling with zero ongoing CPU overhead. One error message is logged.

**Event loop startup:** `ExecInsertCoinBas()` in `CabinetAGEBasic.cs` calls `AGEBasic.StartEventLoop()` when no `after-insert-coin` program is configured but events are registered, ensuring memory-watch events run for pure-YAML cabinets. `LoadCheatXml()` is also called here (not at cabinet load time) to avoid parsing XML for games the player never inserts a coin into.

**Mamecheat XML (`MameCheatXmlParser.cs`):** Parses Pugsy's Cheats XML format. For each `<cheat>` element, finds the first `state="run"` action whose script matches `chip.size@hexoffset`. Chip names (`maincpu`, `soundcpu`, etc.) map to region 2 (SYSTEM_RAM). Returns a case-insensitive `Dictionary<string, CheatAddress>` keyed by cheat description.

## Conclusion

The AGEBasic implementation is a pragmatic, domain-specific interpreter. Instead of building a fully abstract syntax tree (AST), it compiles directly to a list of executable command objects. This design choice, combined with array-based binary searching for jumps and time-budgeted coroutine execution, makes it exceptionally fast to parse and safe to run alongside a demanding VR rendering pipeline. Dynamic event registration via `ONEVENT` further extends its flexibility, allowing scripts to respond to a wide range of VR and system interactions without being hardcoded in configuration files.