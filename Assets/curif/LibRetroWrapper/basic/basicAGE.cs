using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
        string str = $"COMPILATION ERROR: {Program} \n line: {LineNumber}\n";
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
        GameObject roomInit = GameObject.Find("RoomInit");

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

        configCommands.ConfigurationController = ConfigurationController;
        configCommands.ScreenGenerator = ScreenGenerator;
        configCommands.SceneDatabase = SceneDatabase;
        configCommands.CabinetsController = CabinetsController;
        configCommands.GameRegistry = GameRegistry;
        configCommands.Teleportation = Teleportation;

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
    public void ParseFile(string filePath)
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

    public bool Running()
    {
        return running == null;
    }
    public void Stop()
    {
        if (running == null)
            return;
        configCommands.stop = true;
    }
    public void Run(string name, bool blocking = false)
    {
        if (!programs.ContainsKey(name))
            throw new Exception($"program {name} doesn't exists");

        if (running != null)
            throw new Exception($"you can't run {name}, {running.Name} is runnig");

        running = programs[name];
        running.PrepareToRun();

        if (!blocking)
            StartCoroutine(runProgram());
        else
        {
            bool moreLines = true;
            while (moreLines)
            {
                try
                {
                    moreLines = running.runNextLine();
                }
                catch (Exception e)
                {
                    string strerror = errorMessage(running, e);
                    ConfigManager.WriteConsoleError(strerror);
                    LastRuntimeException = new(running.Name, configCommands.LineNumber, e.Message, e);
                    running = null;
                    if (configCommands.DebugMode)
                        SaveDebug(name, null, LastRuntimeException);
                }
            }
            ConfigManager.WriteConsole($"{running.Name} END.");
            running = null;
            LastRuntimeException = null;
            if (configCommands.DebugMode)
                SaveDebug(name);
        }
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
                    writer.WriteLine("PROGRAM STATUS ---");
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

    IEnumerator runProgram()
    {
        bool moreLines = true;
        while (moreLines)
        {
            try
            {
                moreLines = running.runNextLine();
            }
            catch (Exception e)
            {
                string strerror = errorMessage(running, e);
                ConfigManager.WriteConsoleError(strerror);
                LastRuntimeException = new(running.Name, configCommands.LineNumber, e.Message, e);
                running = null;
                if (configCommands.DebugMode)
                    SaveDebug(name, null, LastRuntimeException);

                yield break;
            }
            yield return new WaitForSeconds(0.01f);
        }
        ConfigManager.WriteConsole($"{running.Name} END. {running.ContLinesExecuted} lines executed.");

        if (configCommands.DebugMode)
            SaveDebug(running.Name);
        
        running = null;
        LastRuntimeException = null;

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