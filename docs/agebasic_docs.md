# AGEBasic Implementation Documentation

Based on the provided source code, AGEBasic is a custom-built, interpreted scripting language designed specifically for the "Age of Joy" VR Arcade ecosystem. It allows users to write BASIC-like scripts to interact with virtual arcade cabinets, the VR environment, and minigames, while safely running within the Unity engine without degrading VR performance.

## 1. Architecture & Core Components

The implementation is structured around several key C# classes that separate the engine, program state, parsing, and execution.

### `basicAGE.cs` (The Engine)
This Unity `MonoBehaviour` serves as the execution engine and environment manager.
- **Context Injection:** It collects references to Unity game objects and Age of Joy systems (e.g., `ScreenGenerator`, `CabinetsController`, `Teleportation`, `PlayerController`) and packages them into a `ConfigurationCommands` object. This object acts as the bridge between the AGEBasic scripts and the Unity world.
- **Execution Loop:** Execution is handled via a Unity Coroutine (`runProgram`). To maintain high frame rates in VR, it strictly budgets execution time. It limits the number of lines executed per frame based on a CPU multiplier (`MaxLinesPerFrame * cpuPercentage`) and enforcing a hard time limit (`MaxMillisecondsPerFrame`, default 2.0ms).
- **Safety:** It implements a fail-safe limit on total execution lines (`DefaultMaxExecutionLines`) to prevent infinite loops from locking the application, and catches `RuntimeException`s safely.

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

## Conclusion

The AGEBasic implementation is a pragmatic, domain-specific interpreter. Instead of building a fully abstract syntax tree (AST), it compiles directly to a list of executable command objects. This design choice, combined with array-based binary searching for jumps and time-budgeted coroutine execution, makes it exceptionally fast to parse and safe to run alongside a demanding VR rendering pipeline.