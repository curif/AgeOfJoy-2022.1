using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Audio;

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

    public ConfigurationController ConfigurationController;
    public ScreenGenerator ScreenGenerator;
    public GameRegistry GameRegistry;
    public CabinetsController CabinetsController;
    public Teleportation Teleportation;

    public SceneDatabase SceneDatabase = null;
    public MoviePosterController PostersController;
    public AudioMixer audioMixer; // Drag your Audio Mixer asset here in the Unity Editor

    public PlayerController Player;
    public XROrigin PlayerOrigin;
    public GameObject PlayerControllerGameObject;

#if UNITY_EDITOR
    public string nameToExecute;
    public string path;
#endif 

    ConfigurationCommands configCommands = new();

    public RuntimeException LastRuntimeException;

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

    public void Start()
    {
        GameObject roomInit = GameObject.Find("FixedObject");

        if (ConfigurationController == null)
            ConfigurationController = GetComponent<ConfigurationController>();
        if (ScreenGenerator == null)
            ScreenGenerator = GetComponent<ScreenGenerator>();
        if (CabinetsController == null && ConfigurationController != null)
            CabinetsController = ConfigurationController.cabinetsController;
        if (SceneDatabase == null && roomInit != null)
            SceneDatabase = roomInit.GetComponent<SceneDatabase>();
        if (GameRegistry == null && roomInit != null)
            GameRegistry = roomInit.GetComponent<GameRegistry>();
        if (Teleportation == null)
            Teleportation = GetComponent<Teleportation>();
        if (PlayerControllerGameObject == null)
        {
            PlayerControllerGameObject = GameObject.Find("OVRPlayerControllerGalery");
            if (PlayerControllerGameObject != null)
            {
                if (Player == null)
                    Player = PlayerControllerGameObject.GetComponent<PlayerController>();
                if (PlayerOrigin == null)
                    PlayerOrigin = PlayerControllerGameObject.GetComponent<XROrigin>();
            }
        }
        // if (Audio == null)
        //     Audio = GetComponent<AudioSource>();

        configCommands.ConfigurationController = ConfigurationController;
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

    public void SetCabinet(GameObject cabinet)
    {
        configCommands.Cabinet = cabinet;
    }

    public void SetCoinSlot(CoinSlotController coinSlot)
    {
        configCommands.CoinSlot = coinSlot;
    }

    public bool Exists(string name)
    {
        return programs.ContainsKey(name);
    }
    public AGEProgram ParseFile(string filePath)
    {
        string name = Path.GetFileName(filePath);

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
        return running != null;
    }
    public bool IsRunning(string name)
    {
        return (running != null && running.Name == name);
    }
    public bool ExceptionOccurred()
    {
        return LastRuntimeException != null;
    }


    public void Stop()
    {
        if (running == null)
            return;
        configCommands.stop = true;
    }
    public void Run(string name, bool blocking = false, BasicVars pvars = null)
    {
        if (!programs.ContainsKey(name))
            throw new Exception($"program {name} doesn't exists");

        if (running != null)
            throw new Exception($"you can't run {name}, {running.Name} is runnig");

        PrepareToRun();
        running = programs[name];
        running.PrepareToRun(pvars);

        if (!blocking)
        {
            StartCoroutine(runProgram());
            return;
        }

        bool moreLines = true;
        while (moreLines)
        {
            moreLines = RunALine();
        }

        if (configCommands.DebugMode)
            SaveDebug(running.Name, null, LastRuntimeException);

        running = null;
        return;
    }

    public void SaveDebug(string prgName, CompilationException compEx = null, RuntimeException runEx = null)
    {
        string filePathDebug = Path.Combine(ConfigManager.AGEBasicDir, prgName + ".debug");
        ConfigManager.WriteConsole($"[SaveDebug] {filePathDebug}");
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

    public void PrepareToRun()
    {
        running = null;
        LastRuntimeException = null;
    }


    IEnumerator runProgram()
    {
        bool moreLines = true;
        LastRuntimeException = null;
        while (moreLines)
        {
            moreLines = RunALine();
            yield return new WaitForSeconds(0.01f);
        }

        ConfigManager.WriteConsole($"[runProgram] {running.Name} END. {running.ContLinesExecuted} lines executed. ERROR: {LastRuntimeException}");

        if (configCommands.DebugMode)
            SaveDebug(running.Name, compEx: null, runEx: LastRuntimeException);

        running = null;
    }

    //run a line of an started program
    public bool RunALine()
    {
        try
        {
            return running.runNextLine();
        }
        catch (Exception e)
        {
            string strerror = errorMessage(running, e);
            ConfigManager.WriteConsoleError($"[RunALine] {strerror}");
            LastRuntimeException = new(running.Name, configCommands.LineNumber, e.Message, e);
            return false;
        }
    }

#if UNITY_EDITOR
    public void ExecuteInEditorMode()
    {
        Run(nameToExecute, false);

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
        ParseFile(path + "\\" + nameToExecute);
    }
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
        if(GUILayout.Button("Run tests in Path"))
        {
          myScript.RunTests();
        }
        if(GUILayout.Button("Stop"))
        {
          myScript.Stop();
        }
    }
}
#endif