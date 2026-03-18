# AGEBasic Implementation Documentation

Based on the provided source code, AGEBasic is a custom-built, interpreted scripting language designed specifically for the "Age of Joy" VR Arcade ecosystem. It allows users to write BASIC-like scripts to interact with virtual arcade cabinets, the VR environment, and minigames, while safely running within the Unity engine without degrading VR performance.

## 1. Architecture & Core Components

The implementation is structured around several key C# classes that separate the engine, program state, parsing, and execution.

### `basicAGE.cs` (The Engine)
This Unity `MonoBehaviour` serves as the execution engine and environment manager.
- **Context Injection:** It collects references to Unity game objects and Age of Joy systems (e.g., `ScreenGenerator`, `CabinetsController`, `Teleportation`, `PlayerController`) and packages them into a `ConfigurationCommands` object. This object acts as the bridge between the AGEBasic scripts and the Unity world.
- **Execution Loop:** Execution is handled via a Unity Coroutine (`runProgram`). To maintain high frame rates in VR, it strictly budgets execution time. It limits the number of lines executed per frame based on a CPU multiplier (`MaxLinesPerFrame * cpuPercentage`) and enforcing a hard time limit (`MaxMillisecondsPerFrame`, default 2.0ms).
- **Safety:** It implements a fail-safe limit on total execution lines (`DefaultMaxExecutionLines`) to prevent infinite loops from locking the application, and catches `RuntimeException`s safely.
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
- **Initialization:** Reads configuration from `description.yaml` (via `CabinetAGEBasicInformation`) and pre-loads variables using `IngestVariables`.
- **Event Coroutine (`RunEvents`):** Manages a continuous background loop that checks conditions and executes mapped scripts based on cabinet interactions. It supports dynamic registration of events at runtime.
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
- **Sub-script isolation:** When an event is triggered, it runs its specified script using a dedicated `Event.PrepareToRun()` call, ensuring isolated execution environments based on the cabinet's `BasicVars` state.

### `basicConfigurationCommands.cs` (The Shared State Object)
Passed to almost every object in the interpreter. Holds the current state of execution.
- Contains references to global dependencies (`ScreenGenerator`, `CabinetsController`, `ControlMap`, etc.).
- Maintains the Call Stack (`Gosub`).
- Controls program flow interrupts (`stop`, `stopAllEvents`, `JumpTo`, `JumpNextTo`, `SleepTime`).
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
- `GOTO 100`: Uses `config.JumpTo`. The program searches exactly for line number 100.
- Internally, a jump can use `config.JumpNextTo`. This is used when a line number might not exist (e.g. exiting a `FOR` loop), telling the engine to find the *first line number greater than or equal to* the requested number.
- `ONEVENT`: Registers a new `Event` at runtime. The command parses the event type and parameters, creates an `EventInformation` object, and uses `EventsFactory` to instantiate the appropriate `Event` subclass. This instance is added to `config.events` and initialized.

## 5. Extending AGEBasic

To add new functionality to AGEBasic, developers follow these patterns:

### Adding a New Command
1. Create a new class in `Assets/curif/LibRetroWrapper/basic/Commands/` inheriting from `CommandBase` (or specialized bases like `CommandSingleExpressionBase`).
2. Implement `Parse(TokenConsumer tokens)` to handle the syntax.
3. Implement `Execute(BasicVars vars)` to perform the action.
4. Register the new command in `Commands` static constructor in `basicCommands.cs`.

### Adding a New Function
1. Create a new class in `Assets/curif/LibRetroWrapper/basic/functions/` inheriting from `CommandFunctionBase` (or similar).
2. Implement `Execute(BasicVars vars)` to return a `BasicValue`.
3. Register the new function in `Commands` static constructor in `basicCommands.cs`.

### Adding a New Event Type
1. Define a new `Event` subclass in `CabinetAGEBasic.cs`.
2. Update `EventsFactory.Factory` to handle the new event ID.
3. Update `CommandONEVENT.Execute` in `ONEVENT.cs` to correctly map BASIC parameters to the `EventInformation` fields.

## Conclusion

The AGEBasic implementation is a pragmatic, domain-specific interpreter. Instead of building a fully abstract syntax tree (AST), it compiles directly to a list of executable command objects. This design choice, combined with array-based binary searching for jumps and time-budgeted coroutine execution, makes it exceptionally fast to parse and safe to run alongside a demanding VR rendering pipeline. Dynamic event registration via `ONEVENT` further extends its flexibility, allowing scripts to respond to a wide range of VR and system interactions without being hardcoded in configuration files.
