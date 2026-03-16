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
        Finished,
        Persistent
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

    public void SetCabinetEvents(List<Event> events)
    {
        this.events = events;
        configCommands.events = events;
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
                SaveDebug(name, ce);

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
        return running != null || Status == ProgramStatus.Running;
    }
    public bool IsPersistentRunning()
    {
        return running != null || Status == ProgramStatus.Persistent;
    }

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

    public void startEventCoroutine()
    {
        if (eventCoroutine == null)
            eventCoroutine = StartCoroutine(RunEvents());
    }

    public void stopEventCoroutine()
    {
        if (eventCoroutine != null)
        {
            StopCoroutine(eventCoroutine);
            eventCoroutine = null;
        }
    }

    public bool IsEventCoroutineRunning()
    {
        return eventCoroutine != null;
    }

    private IEnumerator RunEvents()
    {
        while (true)
        {
            if (IsRunning())
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
                    ConfigManager.WriteConsole($"[basicAGE.RunEvents] starting event:{evt.eventInformation.name} prg:{evt.eventInformation.program} goto:{evt.eventInformation.line} ");

                    // Variables logic from CabinetAGEBasic
                    if (evt.eventInformation.Variables != null && evt.eventInformation.Variables.Count > 0)
                    {
                        foreach (AGEBasicVariable var in evt.eventInformation.Variables)
                        {
                            BasicValue bv;
                            if (string.IsNullOrEmpty(var?.type) || var.type.ToUpper() == "STRING")
                                bv = new BasicValue(var.value, forceType: BasicValue.BasicValueType.String);
                            else if (var.type.ToUpper() == "NUMBER")
                                bv = new BasicValue(var.value, forceType: BasicValue.BasicValueType.Number);
                            else
                                throw new Exception($"AGEBasic variable injection error var: {var.name} value type unknown: {var.type}");

                            evt.vars.SetValue(var.name, bv);
                        }
                    }

                    int maxExecLines = DefaultMaxExecutionLines;
                    if (configCommands.Cabinet != null && configCommands.Cabinet.gameObject != null) 
                    {
                        CabinetAGEBasic cabAge = configCommands.Cabinet.gameObject.GetComponent<CabinetAGEBasic>();
                        if (cabAge != null && cabAge.AGEInfo.maxExecutionLines != -1)
                            maxExecLines = cabAge.AGEInfo.maxExecutionLines;
                    }

                    PrepareToRun(evt.eventInformation.program, evt.vars, 
                                    maxExecutionLinesAllowed: maxExecLines, 
                                    lineNumber: evt.eventInformation.line);

                    runningProgramCoroutine = StartCoroutine(runProgram());
                    
                    while (IsRunning())
                        yield return null;

                    evt.Finish();
                }
            }
            yield return null;
        }
    }

    public void Stop()
    {
        if (Status == ProgramStatus.Persistent)
        {
            ForceStop();
            return;
        }

        if (running == null)
            return;
        configCommands.stop = true;
        Status = ProgramStatus.Finished;
    }

    public void ForceStop()
    {
        programContextStack.Clear();

        if (runningProgramCoroutine != null)
            StopCoroutine(runningProgramCoroutine);

        if (running != null)
        {
            if (configCommands.DebugMode)
                SaveDebug(running.Name, compEx: null, runEx: LastRuntimeException);

            ConfigManager.WriteConsole($"[ForceStop] {running.Name} forced to END. {running.ContLinesExecuted} lines executed. ERROR: {LastRuntimeException}");
        }

        configCommands.stop = true;
        configCommands.CloseFiles();
        configCommands.ScreenGenerator?.ClearSprites();
        
        // GLOBAL ENFORCEMENT: Kill ALL registered events when a stop is requested
        events.Clear();
        configCommands.events = events;

        runningProgramCoroutine = null;
        running = null;
        Status = ProgramStatus.Finished;
    }

    public void Run(string programName, BasicVars pvars = null,
                        int maxExecutionLinesAllowed = -1)
    {
        // Ensure only one program context exists. 
        // If a program was persistent in the background, this kills it.
        ForceStop();

        if (maxExecutionLinesAllowed == -1)
            maxExecutionLinesAllowed = DefaultMaxExecutionLines;

        PrepareToRun(programName, pvars, maxExecutionLinesAllowed);

        runningProgramCoroutine = StartCoroutine(runProgram());
    }

    public void PrepareToRun(string name, BasicVars pvars, int maxExecutionLinesAllowed = 0, int lineNumber = 0)
    {
        InitComponents();

        // If no events are registered yet, but we have an events list, let's make sure the loop is running
        if (events.Count > 0 && eventCoroutine == null)
            startEventCoroutine();

        ConfigManager.WriteConsole($"[BasicAGE.PrepareToRun] starting {name} goto line: {lineNumber}.");

        Status = ProgramStatus.WaitingForStart;

        if (!programs.ContainsKey(name))
            throw new Exception($"program {name} doesn't exists or was erroneous");

        if (running != null)
            throw new Exception($"you can't run {name}, {running.Name} is runnig");

        running = null;
        LastRuntimeException = null;
        running = programs[name];
        running.PrepareToRun(pvars, lineNumber: lineNumber);
        running.MaxExecutionLinesAllowed = maxExecutionLinesAllowed;
        cpuPercentage = configCommands.cpuPercentage;
        calculatedDelay = CalculateDelay(cpuPercentage);
    }

    public void SaveDebug(string prgName, CompilationException compEx = null, RuntimeException runEx = null)
    {
        string filePathDebug = Path.Combine(ConfigManager.AGEBasicDir, prgName + ".debug");
        ConfigManager.WriteConsole($"[BasicAGE.SaveDebug] {filePathDebug}");
        try
        {
            // Create a new StreamWriter instance to write to the file
            using (StreamWriter writer = new StreamWriter(filePathDebug, false))
            {
                // Write the text to the file
                writer.WriteLine($"Program: {prgName}");
                if (compEx != null)
                    writer.WriteLine(compEx.ToString());
                if (runEx != null)
                    writer.WriteLine(runEx.ToString());

                if (Exists(prgName))
                {
                    writer.WriteLine("---PROGRAM STATUS ---");
                    writer.WriteLine(programs[prgName].Log());
                    writer.WriteLine("---");
                }
            }
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException("Error writing to file: " + filePathDebug, e);
        }
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
        PreRunTasks();

        ConfigManager.WriteConsole($"[BasicAGE.runProgram] START {running.Name}");

        bool moreLines = true;
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

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

        PostRunTasks();
        
        yield break;
    }

    // to run inmediatly before a program runs
    public void PreRunTasks()
    {
        InitComponents();
        OnProgramStarted.Invoke(running.Name);

        Status = ProgramStatus.Running;

    }

    // to run inmediatly after a program runs
    public void PostRunTasks()
    {
        bool engineHasEvents = events != null && events.Count > 0;
        bool currentProgramIsPersistent = IsPersistenceRequired();

        // If a STOP command specifically requested to kill all events
        if (configCommands.stopAllEvents)
        {
            ConfigManager.WriteConsole($"[PostRunTasks] {running?.Name} executed STOP. Clearing all registered events.");
            events?.Clear();
            configCommands.events = events;
            engineHasEvents = false;
            currentProgramIsPersistent = false;
        }

        if (!engineHasEvents)
        {
            if (running != null)
                ConfigManager.WriteConsole($"[PostRunTasks] {running.Name} No active events in the engine. Performing full cleanup.");
            
            configCommands.CloseFiles();
            configCommands.ScreenGenerator?.ClearSprites();
        }
        else
        {
            ConfigManager.WriteConsole($"[PostRunTasks] {running?.Name} Engine remains active due to registered events.");
        }

        if (running != null)
        {
            // Only fire the 'Ended' signal if this specific program isn't waiting for events
            if (!currentProgramIsPersistent)
                OnProgramEnded.Invoke(running.Name);
        }

        // Update the engine status based on what remains
        if (currentProgramIsPersistent)
            Status = ProgramStatus.Persistent;
        else if (engineHasEvents)
            Status = ProgramStatus.Persistent; // Engine is still persistent for other programs
        else
            Status = ProgramStatus.Finished;
        
        running = null;
    }

    private bool IsPersistenceRequired()
    {
        if (events == null || running == null) return false;
        foreach (Event evt in events)
        {
            if (evt.eventInformation.program == running.Name)
                return true;
        }
        return false;
    }

    //run just one line of the current loaded program
    public YieldInstruction runNextLineCurrentProgram(ref bool moreLines)
    {
        if (running == null || configCommands.stop)
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
                newProg.PrepareToRun(running.Vars, line);
                
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
                SaveDebug(running.Name, compEx: null, runEx: LastRuntimeException);

            ConfigManager.WriteConsole($"[BasicAGE.runNextLineCurrentProgram] {running.Name} #{configCommands.LineNumber} no more lines or END. {running.ContLinesExecuted} lines executed. ERROR: {LastRuntimeException}");

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
        
        this.Stop();

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
          myScript.Stop();
        }
    }
}
#endif