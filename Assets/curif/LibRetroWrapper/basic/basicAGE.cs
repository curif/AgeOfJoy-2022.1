using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using AOJ.Managers;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CompilationException : Exception
{
    public int LineNumber;
    public string Program;
    public CompilationException(string program, int lineNumber, string message) : base(message)
    {
        LineNumber = lineNumber;
        Program = program;
    }

    public CompilationException(string program, int lineNumber,
                                string message, Exception innerException) : base(message, innerException)
    {
        LineNumber = lineNumber;
        Program = program;
    }

    public override string ToString()
    {
        string str = $"COMPILATION ERROR: {Program} \n line: {LineNumber}\n";
        str += $"Exception: {Message}\n";
        return str;
    }

    public void Show(ScreenGenerator scr)
    {
        scr.Clear();
        scr.Print(0, 0, "Compilation error");
        scr.Print(0, 1, $"Line: {LineNumber}");
        scr.Print(0, 3, Program);
        scr.Print(0, 6, Message);
        scr.DrawScreen();
    }
}

public class RuntimeException : Exception
{
    public int LineNumber;
    public string Program;
    public RuntimeException(string program, int lineNumber, string message) : base(message)
    {
        LineNumber = lineNumber;
        Program = program;
    }

    public RuntimeException(string program, int lineNumber,
                                string message, Exception innerException) : base(message, innerException)
    {
        LineNumber = lineNumber;
        Program = program;
    }
    public override string ToString()
    {
        string str = $"RUNTIME ERROR: {Program} \n line: {LineNumber}\n";
        str += $"Exception: {Message}\n";
        return str;
    }
}

public class basicAGE : MonoBehaviour
{
    //program list
    public Dictionary<string, AGEProgram> programs = new();
    private AGEProgram running;
    private Coroutine runningProgramCoroutine;

    public ConfigurationController ConfigurationController;
    public LibretroControlMap libretroControlMap;
    public ScreenGenerator ScreenGenerator;
    public GameRegistry GameRegistry;
    public CabinetsController CabinetsController;
    public Teleportation Teleportation;
    public DownloadManager DownloadManager;

    public SceneDatabase SceneDatabase = null;
    public MoviePosterController PostersController;
    public AudioMixer audioMixer; // Drag your Audio Mixer asset here in the Unity Editor
    public AOJ.Managers.EventManager EventManager;
    public PlayerController Player;
    public XROrigin PlayerOrigin;
    public GameObject PlayerControllerGameObject;


    [System.Serializable]
    public class AGEBasicEvent : UnityEvent<string> { }

    public AGEBasicEvent OnProgramStarted;
    public AGEBasicEvent OnProgramEnded;

    bool initialized = false;

    public enum ProgramStatus
    {
        None = 0,
        WaitingForStart,
        Running,
        CancelledWithError,
        CompilationError,
        Finished
        //Persistent
    }

    public ProgramStatus Status;

    [Tooltip("Maximum number of BASIC lines to execute in a single frame when CPU is at 100%.")]
    [SerializeField]
    public int MaxLinesPerFrame = 5;

    [Tooltip("Maximum time in milliseconds to spend executing BASIC code in a single frame to prevent FPS drops.")]
    [SerializeField]
    public double MaxMillisecondsPerFrame = 2.0;

    [Tooltip("Total maximum number of lines a program can execute before being forcibly stopped (safety fail-safe).")]
    [SerializeField]
    public int DefaultMaxExecutionLines = 5000000;

    [Tooltip("Minimum seconds between .debug file saves for the same program (throttles disk I/O when DebugMode is on). Error/shutdown saves bypass this.")]
    [SerializeField]
    public float DebugSaveMinIntervalSeconds = 3.0f;

    private Dictionary<string, float> lastDebugSaveTime = new();

#if UNITY_EDITOR
    [Tooltip("Displays the current execution speed (Lines Per Second) of the running program.")]
    [SerializeField]
    private float linesPerSecond;

    // Update the property in the editor when the program is running
    void Update()
    {
        if (running != null && Status == ProgramStatus.Running)
        {
            linesPerSecond = running.GetLinesPerSecond();
        }
        else
        {
            linesPerSecond = 0;
        }
    }
    
    public string nameToExecute;
    public string path;
#endif 

    ConfigurationCommands configCommands = new();
    double cpuPercentage;
    float calculatedDelay;
    public RuntimeException LastRuntimeException;

    private Coroutine eventCoroutine;
    public List<Event> events = new();

    public bool DebugMode
    {
        get
        {
            return configCommands.DebugMode;
        }
        set
        {
            configCommands.DebugMode = value;
        }
    }

    public ConfigurationCommands ConfigCommands {
        get {return configCommands; }
    }

    public void InitComponents()
    {
        if (initialized)
            return;

        GameObject roomInit = GameObject.Find("FixedObject");
        if (roomInit != null)
        {
            if (SceneDatabase == null)
                SceneDatabase = roomInit.GetComponent<SceneDatabase>();
            if (GameRegistry == null)
                GameRegistry = roomInit.GetComponent<GameRegistry>();
            if (DownloadManager == null)
                DownloadManager = roomInit.GetComponent<DownloadManager>();
        }

        //if (ConfigurationController == null)
        //    ConfigurationController = GetComponent<ConfigurationController>();
        if (libretroControlMap == null)
            libretroControlMap = GetComponent<LibretroControlMap>();
        if (ScreenGenerator == null)
            ScreenGenerator = GetComponent<ScreenGenerator>();
        if (CabinetsController == null && ConfigurationController != null)
            CabinetsController = ConfigurationController.cabinetsController;
        if (Teleportation == null)
            Teleportation = GetComponent<Teleportation>();

        if (PlayerControllerGameObject == null)
            PlayerControllerGameObject = GameObject.Find("OVRPlayerControllerGalery");
        if (PlayerControllerGameObject != null)
        {
            if (Player == null)
                Player = PlayerControllerGameObject.GetComponent<PlayerController>();
            if (PlayerOrigin == null)
                PlayerOrigin = PlayerControllerGameObject.GetComponent<XROrigin>();
            if (EventManager == null)
                EventManager = PlayerControllerGameObject.GetComponent<EventManager>();
        }

        configCommands.ConfigurationController = ConfigurationController;
        //configCommands.DownloadManager = DownloadManager;
        configCommands.ControlMap = libretroControlMap;
        configCommands.ScreenGenerator = ScreenGenerator;
        configCommands.SceneDatabase = SceneDatabase;
        configCommands.CabinetsController = CabinetsController;
        configCommands.GameRegistry = GameRegistry;
        configCommands.Teleportation = Teleportation;
        configCommands.PostersController = PostersController;
        // configCommands.Audio = Audio;
        configCommands.audioMixer = audioMixer;
        configCommands.Player = Player;
        configCommands.PlayerGameObject = PlayerControllerGameObject;
        configCommands.PlayerOrigin = PlayerOrigin;
        configCommands.EventManager = EventManager;

        configCommands.ageBasic = this;

        // Expose the existing AudioSource (guaranteed by [RequireComponent] on screen/cabinet controllers).
        // Will be null when basicAGE runs in the ConfigurationController context (no cabinet AudioSource).
        configCommands.AudioSource = GetComponent<AudioSource>();

        // Attach SIDPlayer to this same GameObject so OnAudioFilterRead routes through the cabinet's AudioSource.
        if (configCommands.AudioSource != null)
        {
            SIDPlayer sidPlayer = GetComponent<SIDPlayer>();
            if (sidPlayer == null)
                sidPlayer = gameObject.AddComponent<SIDPlayer>();
            configCommands.SIDPlayer = sidPlayer;
        }

        GameObject musicPlayer = GameObject.Find("JukeBox");
        if (musicPlayer != null)
            configCommands.MusicPlayerQueue = musicPlayer.GetComponent<MusicPlayer>();

        configCommands.events = events;

        initialized = true;
    }

    public void ParseFiles(string folderPath)
    {
        string[] files = Directory.GetFiles(folderPath, "*.bas");
        programs = new();

        foreach (string filePath in files)
        {
            ConfigManager.WriteConsole($"[basicAge.ProcessFiles] {filePath}");
            ParseFile(filePath);
            // if (configCommands.ScreenGenerator != null)
            // ce.Show(configCommands.ScreenGenerator);
        }
    }

    public void SetCabinet(Cabinet cabinet)
    {
        configCommands.Cabinet = cabinet;
    }

    public void SetCoinSlot(CoinSlotController coinSlot)
    {
        configCommands.CoinSlot = coinSlot;
    }

    public void SetLightGunTarget(LightGunTarget lightGunTarget)
    {
        configCommands.lightGunTarget = lightGunTarget;
    }
    public bool Exists(string name)
    {
        return programs.ContainsKey(name);
    }
    public AGEProgram ParseFile(string filePath)
    {
        InitComponents();

        string name = Path.GetFileName(filePath);
        string directory = Path.GetDirectoryName(filePath);

        configCommands.ProgramPath = directory;
        configCommands.ProgramName = name;

        ConfigManager.WriteConsole($"[basicAGE.ParseFile] [{name}] {filePath} ");

        AGEProgram prg = new(name);
        try
        {
            prg.Parse(filePath, configCommands);
        }
        catch (Exception e)
        {
            CompilationException ce = new CompilationException(
                filePath,
                prg.LastLineNumberParsed,
                e.Message, e
            );

            if (configCommands.DebugMode)
                SaveDebug(name, ce, bypassThrottle: true);

            Status = ProgramStatus.CompilationError;

            throw ce;
        }
        programs[name] = prg;
        return prg;
    }

    public void ListPrograms()
    {
        foreach (KeyValuePair<string, AGEProgram> kvp in programs)
        {
            ConfigManager.WriteConsole($" {kvp.Key}");
        }
    }


    public List<string> GetParsedPrograms()
    {
        return new List<string>(programs.Keys);
    }

    string errorMessage(AGEProgram prg, Exception exception)
    {
        string str = $"ERROR: {prg.Name} line: {configCommands.LineNumber}\n";
        str += $"Exception: {exception.Message}\n";
        return str;
    }

    public bool IsRunning()
    {
        return running != null || Status == ProgramStatus.Running || Status == ProgramStatus.WaitingForStart;
    }
    public bool IsRunningInBackground()
    {
        return eventCoroutine != null && events.Count > 0;
    }

    public bool IsEventLoopActive => eventCoroutine != null;
    public int RegisteredEventsCount => events.Count;

    public bool IsRunning(string name)
    {
        return (!String.IsNullOrEmpty(running?.Name) && running.Name == name);
    }

    public bool ExceptionOccurred()
    {
        return LastRuntimeException != null;
    }

    public class ProgramContext
    {
        public AGEProgram Program;
        public Stack<double> GosubStack;
        public double LineNumber;
        public string ProgramPath;
        public string ProgramName;
    }
    private Stack<ProgramContext> programContextStack = new Stack<ProgramContext>();

    // Helper to pop the state
    private void PopProgramState()
    {
        ProgramContext ctx = programContextStack.Pop();
        running = ctx.Program;
        configCommands.Gosub = ctx.GosubStack;
        configCommands.LineNumber = ctx.LineNumber;
        configCommands.ageProgram = running;
        configCommands.ProgramPath = ctx.ProgramPath;
        configCommands.ProgramName = ctx.ProgramName;
    }

    public void IngestVariables(List<AGEBasicVariable> variables, BasicVars targetVars)
    {
        if (variables == null || variables.Count == 0 || targetVars == null)
            return;

        foreach (AGEBasicVariable var in variables)
        {
            BasicValue bv;
            if (string.IsNullOrEmpty(var?.type) || var.type.ToUpper() == "STRING")
                bv = new BasicValue(var.value, forceType: BasicValue.BasicValueType.String);
            else if (var.type.ToUpper() == "NUMBER")
                bv = new BasicValue(var.value, forceType: BasicValue.BasicValueType.Number);
            else
                throw new Exception($"AGEBasic variable injection error var: {var.name} value type unknown: {var.type}");

            targetVars.SetValue(var.name, bv);
        }
    }

    private IEnumerator RunEvents()
    {
        CabinetAGEBasic cabinet = null;
        int maxExecLines = DefaultMaxExecutionLines;
        if (configCommands.Cabinet != null && configCommands.Cabinet.gameObject != null)
        {
            cabinet = configCommands.Cabinet.gameObject.GetComponentInChildren<CabinetAGEBasic>();
            if (cabinet != null && cabinet.AGEInfo.maxExecutionLines != -1)
                maxExecLines = cabinet.AGEInfo.maxExecutionLines;
        }

        while (true)
        {
            if (IsRunning() || events.Count == 0)
            {
                yield return null;
                continue;
            }

            Event[] currentEvents = events.ToArray();

            foreach (Event evt in currentEvents)
            {
                if (!evt.Initialized)
                    evt.Init();

                try
                {
                    if (evt.Condition())
                        evt.EvaluateTrigger();
                }
                catch (Exception e)
                {
                    ConfigManager.WriteConsoleException($"[basicAGE.RunEvents] evaluate exception event:{evt.eventInformation.name} / {evt.eventInformation.eventId}", e);
                }
            }

            foreach (Event evt in currentEvents)
            {
                if (evt.WasTriggered())
                {
                    try
                    {
                        ConfigManager.WriteConsole($"[basicAGE.RunEvents] starting event:{evt.eventInformation.name} prg:{evt.eventInformation.program} goto:{evt.eventInformation.line} ");
                        
                        // Use unified IngestVariables for event-specific variables
                        IngestVariables(evt.eventInformation.Variables, evt.vars);

                        // Ensure program is compiled before running
                        if (!Exists(evt.eventInformation.program))
                        {
                            string basePath;
                            if (cabinet != null)
                                basePath = cabinet.pathBase;
                            else if (!string.IsNullOrEmpty(configCommands.ProgramPath))
                                basePath = configCommands.ProgramPath;
                            else
                                basePath = ConfigManager.AGEBasicDir;

                            ParseFile(Path.Combine(basePath, evt.eventInformation.program));
                        }

                        Run(evt.eventInformation.program, evt.vars,
                                        maxExecutionLinesAllowed: maxExecLines,
                                        lineNumber: evt.eventInformation.line);
                    }
                    catch (Exception e)
                    {
                        ConfigManager.WriteConsoleException($"[basicAGE.RunEvents] Error executing event program {evt.eventInformation.program}: ", e);
                        running = null;
                        Status = ProgramStatus.None;
                    }

                    while (IsRunning())
                        yield return null;

                    evt.Finish();

                }
            }
            yield return null;
        }
    }

    //finish running program
    public void EndRunningProgram()
    {
        if (running != null)
        {
            OnProgramEnded?.Invoke(running.Name);
        }

        running = null;
        Status = ProgramStatus.Finished;

        if (runningProgramCoroutine != null)
        {
            StopCoroutine(runningProgramCoroutine);
            runningProgramCoroutine = null;
        }

        //stop event loop when don't needed.
        if (events != null && events.Count == 0 && eventCoroutine != null)
        {
            StopCoroutine(eventCoroutine);
            eventCoroutine = null;
        }
    }

    public void Shutdown()
    {
        ConfigManager.WriteConsole($"[BasicAGE.Shutdown] {running?.Name} SHUTDOWN.");

        AGEProgram lastRunning = running;
        programContextStack.Clear();

        //force stop event loop.
        if (eventCoroutine != null)
        {
            StopCoroutine(eventCoroutine);
            eventCoroutine = null;
        }
        if (events != null)
        {
            foreach (Event evt in events)
            {
                try
                {
                    evt.Dispose();
                }
                catch (Exception e)
                {
                    ConfigManager.WriteConsoleException($"[BasicAGE.Shutdown] error disposing event:{evt.eventInformation.name} / {evt.eventInformation.eventId}", e);
                }
            }
            events.Clear();
            configCommands.events = events;
        }
        
        configCommands.CloseFiles();
        configCommands.ScreenGenerator?.ClearSprites();

        if (lastRunning != null)
        {
            if (configCommands.DebugMode)
                SaveDebug(lastRunning.Name, compEx: null, runEx: LastRuntimeException, bypassThrottle: true);

            ConfigManager.WriteConsole($"[ForceStop] {lastRunning.Name} forced to END. {lastRunning.ContLinesExecuted} lines executed. ERROR: {LastRuntimeException}");
            
        }

        configCommands.stop = false;
        configCommands.shutdown = false;
        configCommands.CloseFiles();
        configCommands.ScreenGenerator?.ClearSprites();

        EndRunningProgram();
    }

    public void Run(string progName, BasicVars pvars = null,
                        int maxExecutionLinesAllowed = -1, 
                        int lineNumber = 0)
    {
        ConfigManager.WriteConsole($"[BasicAGE.Run] starting {progName} goto line: {lineNumber}.");

        if (!programs.ContainsKey(progName))
            throw new Exception($"program {progName} doesn't exists or was erroneous");

        if (running != null)
            throw new Exception($"you can't run {progName}, {running.Name} is runnig");

        InitComponents();

        running = programs[progName];
        Status = ProgramStatus.WaitingForStart;
        LastRuntimeException = null;
        running.PrepareProgramToRun(pvars, 
                                    lineNumber: lineNumber, 
                                    maxExecutionLinesAllowed==-1? DefaultMaxExecutionLines: maxExecutionLinesAllowed);
        cpuPercentage = configCommands.cpuPercentage;
        calculatedDelay = CalculateDelay(cpuPercentage);

        runningProgramCoroutine = StartCoroutine(runProgram());

        if (eventCoroutine == null)
            eventCoroutine = StartCoroutine(RunEvents());
    }

    /// <summary>
    /// Starts the event loop coroutine if it isn't already running.
    /// Call this when events are registered but no program will be executed
    /// (e.g. a cabinet with YAML events but no after-insert-coin script).
    /// </summary>
    public void StartEventLoop()
    {
        InitComponents();
        if (eventCoroutine == null)
        {
            ConfigManager.WriteConsole("[basicAGE.StartEventLoop] Starting event loop (no program running).");
            eventCoroutine = StartCoroutine(RunEvents());
        }
    }

    public void SaveDebug(string prgName, CompilationException compEx = null, RuntimeException runEx = null, bool bypassThrottle = false)
    {
        if (!bypassThrottle)
        {
            float now = Time.realtimeSinceStartup;
            if (lastDebugSaveTime.TryGetValue(prgName, out float last) && (now - last) < DebugSaveMinIntervalSeconds)
                return;
            lastDebugSaveTime[prgName] = now;
        }

        string filePathDebug = Path.Combine(ConfigManager.AGEBasicDir, prgName + ".debug");

        string content = $"Program: {prgName}\n";
        if (compEx != null)
            content += compEx.ToString();
        if (runEx != null)
            content += runEx.ToString();
        if (Exists(prgName))
            content += "---PROGRAM STATUS ---\n" + programs[prgName].Log() + "---\n";

        ConfigManager.WriteConsole($"[BasicAGE.SaveDebug] {filePathDebug}");

        System.Threading.Tasks.Task.Run(() =>
        {
            try
            {
                File.WriteAllText(filePathDebug, content);
            }
            catch (Exception e)
            {
                ConfigManager.WriteConsoleException("Error writing to file: " + filePathDebug, e);
            }
        });
    }
    public float CalculateDelay(double cpuPercentageDouble)
    {
        float cpuPercentage = (float)cpuPercentageDouble;
        // Clamp the CPU percentage between 0 and 100 to avoid invalid input.
        cpuPercentage = Mathf.Clamp(cpuPercentage, 0f, 100f);

        // Calculate delay based on CPU percentage.
        // 100% CPU = 0 delay, 
        float delay = Mathf.Lerp(0.03f, 0f, cpuPercentage / 100f);

        return delay;
    }

    // run a complete program in a coroutine
    IEnumerator runProgram()
    {

        ConfigManager.WriteConsole($"[BasicAGE.runProgram] START {running.Name}");
        
        InitComponents(); //only if not initialized previously

        Status = ProgramStatus.Running;
        OnProgramStarted?.Invoke(running.Name);

        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        bool moreLines = true;
        while (moreLines)
        {
            // Determine lines to execute this frame. 
            // cpuPercentage acts as a direct multiplier to the Unity Inspector base MaxLinesPerFrame.
            // Default: MaxLinesPerFrame(1) * cpuPercentage(1) = 1 line per frame (Legacy speed).
            // Fast script: SETCPU 500 -> 1 * 500 = 500 lines per frame.
            int linesToExecute = (int)(MaxLinesPerFrame * cpuPercentage);
            if (linesToExecute < 1) linesToExecute = 1;

            stopwatch.Restart();

            for (int i = 0; i < linesToExecute && moreLines; i++)
            {
                YieldInstruction yieldInstruction = runNextLineCurrentProgram(ref moreLines);
                
                if (configCommands.shutdown)
                {
                    Shutdown();
                    yield break;
                }

                if (configCommands.stop)
                {
                    EndRunningProgram();
                    yield break;
                }

                // If a command specifically requested a sleep, break the batch and yield
                if (yieldInstruction != null)
                {
                    yield return yieldInstruction;
                    break;
                }

                // TIME BUDGET: Prevent FPS drops in VR.
                if (stopwatch.Elapsed.TotalMilliseconds > MaxMillisecondsPerFrame)
                {
                    break;
                }
            }

            // Yield once to let Unity render the frame
            if (moreLines)
                yield return null;
        }

        EndRunningProgram();
        yield break;
    }

    //run just one line of the current loaded program
    public YieldInstruction runNextLineCurrentProgram(ref bool moreLines)
    {
        if (running == null || configCommands.stop || configCommands.shutdown)
        {
            moreLines = false;
            return null;
        }

        try
        {
            moreLines = running.runNextLine();
        }
        catch (Exception e)
        {
            string strerror = errorMessage(running, e);
            ConfigManager.WriteConsoleException($"[BasicAGE.runNextLineCurrentProgram] {strerror}",e);
            LastRuntimeException = new(running.Name, (int)configCommands.LineNumber, e.Message, e);
            moreLines = false;
        }

        // --- NEW: INTERCEPT SUB-PROGRAM SIGNAL ---
        if (configCommands.RunSubProgramPath != null)
        {
            if (programContextStack.Count > 20) // Prevent infinite recursion crashing game
                throw new Exception($"[BasicAGE.runNextLineCurrentProgram] RUN command stack overflow (max depth 20).");

            string path = configCommands.RunSubProgramPath;
            int line = configCommands.RunSubProgramLine;

            ConfigManager.WriteConsole($"[BasicAGE.runNextLineCurrentProgram] ready to run {path} on line {line} called by {name}");

            configCommands.RunSubProgramPath = null;
            configCommands.RunSubProgramLine = -1;

            // 1. Save current program state
            ProgramContext ctx = new ProgramContext {
                Program = running,
                GosubStack = configCommands.Gosub,
                LineNumber = configCommands.LineNumber,
                ProgramPath = configCommands.ProgramPath,
                ProgramName = configCommands.ProgramName
            };
            programContextStack.Push(ctx);

            try 
            {
                AGEProgram newProg;
                string progName = Path.GetFileName(path);

                // 2. Resolve and load the program
                if (!programs.ContainsKey(progName))
                {
                    // Resolve absolute path if needed
                    // C:\Users\curif\desarr\ageofjoy.0.5.1\AgeOfJoy-2022.1\AGEBasicTests\core test_run.bas
                    string fullPath = Path.Combine(configCommands.ProgramPath, path);
                    newProg = ParseFile(fullPath);
                }
                else
                {
                    newProg = programs[progName];
                }

                // 3. Prepare new program. CRITICAL: Pass running.Vars to share the exact same variable space!
                newProg.PrepareProgramToRun(running.Vars, line);
                
                // 4. Swap execution context
                running = newProg;
                configCommands.ageProgram = running;
                
                moreLines = true; // Force loop to continue immediately with new program
                return null;
            }
            catch (Exception e)
            {
                string strerror = errorMessage(running, e);
                ConfigManager.WriteConsoleException($"[BasicAGE.runNextLineCurrentProgram] Sub-program chaining failed: {configCommands.LineNumber} {strerror}", e);
                LastRuntimeException = new(running.Name, (int)configCommands.LineNumber, e.Message, e);
                moreLines = false;
                return null;
            }
        }

        if (!moreLines)
        {
            // --- NEW: RETURN TO CALLER PROGRAM ---
            // If no error occurred and there is a caller waiting on the stack
            if (LastRuntimeException == null && programContextStack.Count > 0)
            {
                PopProgramState();
                
                // If the sub-program hit an 'END' command, it sets stop=true. 
                // We must reset it so the parent program doesn't accidentally stop too.
                configCommands.stop = false; 
                
                moreLines = true; // Resume the parent program
                return null; 
            }

            if (configCommands.DebugMode)
                SaveDebug(running.Name, compEx: null, runEx: LastRuntimeException, bypassThrottle: LastRuntimeException != null);

            ConfigManager.WriteConsole($"[BasicAGE.runNextLineCurrentProgram] {running.Name} #{configCommands.LineNumber} no more lines or END. {running.ContLinesExecuted} lines executed. ERROR: [{LastRuntimeException}]");

            if (LastRuntimeException != null)
                Status = ProgramStatus.CancelledWithError;
            else
                Status = ProgramStatus.Finished; // Status will be refined in PostRunTasks
            
            return null;
        }

        if (configCommands.SleepTime > 0)
        {
            YieldInstruction delay = new WaitForSeconds(configCommands.SleepTime);
            configCommands.SleepTime = 0;
            return delay;
        }

        if (cpuPercentage != configCommands.cpuPercentage)
        {
            //user changes cpu control
            cpuPercentage = configCommands.cpuPercentage;
            // ConfigManager.WriteConsole($"[BasicAGE.runProgram] {running.Name} cpu: {cpuPercentage}% delay: {calculatedDelay} secs");
        }

        // Return null instead of WaitForSeconds(0) so the batch loop can continue
        return null;
    }


#if UNITY_EDITOR
    public void ExecuteInEditorMode()
    {
        Run(nameToExecute);

        AGEProgram program = programs[nameToExecute];
        if (program.Vars.Exists("ERROR"))
        {            
            BasicValue error = program.Vars.GetValue("ERROR");
            if (error.IsString() && error.GetValueAsString() != "")
            {
                ConfigManager.WriteConsoleError($"[ExecuteInEditorMode] {nameToExecute}: {error.GetValueAsString()}");
            }
        }
    }
    public void CheckTestsResults()
    {
        AGEProgram program = programs[nameToExecute];
        if (program == null)
            throw new Exception("Program didn't run yet (or was deleted)");
        
        EndRunningProgram();

        if (program.Vars.Exists("ERROR"))
        {            
            BasicValue error = program.Vars.GetValue("ERROR");
            if (error.IsString() && error.GetValueAsString() != "")
            {
                ConfigManager.WriteConsoleError($"[ExecuteInEditorMode] {nameToExecute}: {error.GetValueAsString()}");
                return;
            }
        }
        ConfigManager.WriteConsole("no error detected in: " + program.Name);
    }
    public void Log()
    {
        ConfigManager.WriteConsole(programs[nameToExecute].Log());
    }
    public void ProcessTestPath()
    {
        ParseFiles(path);
    }
    public void ProcessTheFile()
    {
        ParseFile(Path.Combine(path, nameToExecute));
    }
    public void LogStatus()
    {
        ConfigManager.WriteConsole(
            $"AGEBasic Log Status\n Running: program name: [{running?.Name}] Is running: {IsRunning()} \n Events: {IsRunningInBackground()} cantidad: {events.Count} coroutine: [{eventCoroutine}]");
    }
    /*
    public void RunTests()
    {
        ParseFiles(path);
        ConfigManager.WriteConsole($"[RunTests] START");
        foreach (KeyValuePair<string, AGEProgram> kvp in programs)
        {
            AGEProgram program = kvp.Value;
            ConfigManager.WriteConsole($"[RunTests] TEST {kvp.Key}");
            Run(kvp.Key, blocking: true);
            if (program.Vars.Exists("ERROR"))
            {            
                BasicValue error = program.Vars.GetValue("ERROR");
                if (error.IsString() && error.GetValueAsString() != "")
                {
                    ConfigManager.WriteConsoleError($"[RunTests] {kvp.Key}: {error.GetValueAsString()}");
                }
            }
            ConfigManager.WriteConsole($"[RunTests] {program.Log()}");
        }
        
        ConfigManager.WriteConsole($"[RunTests] END");
    }
    */
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(basicAGE))]
public class basicAGEEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        basicAGE myScript = (basicAGE)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Events Status", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Event Loop Active:", myScript.IsRunningInBackground() ? "YES" : "No");
        EditorGUILayout.LabelField("Registered Events:", myScript.events.Count.ToString());
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Commands", EditorStyles.boldLabel);
        if(GUILayout.Button("Show Programs"))
        {
          myScript.ListPrograms();
        }
        if(GUILayout.Button("Process and Execute the file"))
        {
          myScript.ProcessTheFile();
          myScript.ExecuteInEditorMode();
        }
        if(GUILayout.Button("Check tests results"))
        {
          myScript.CheckTestsResults();
        }
        if(GUILayout.Button("Process Test Path"))
        {
          myScript.ProcessTestPath();
        }
        if(GUILayout.Button("Last LOG"))
        {
          myScript.Log();
        }
        if(GUILayout.Button("Stop"))
        {
          myScript.EndRunningProgram();
        }
        if (GUILayout.Button("AGEBASIC Engine status"))
        {
            myScript.LogStatus();
        }
    }
}
#endif