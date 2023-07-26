using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class basicAGE : MonoBehaviour
{
    //program list
    public Dictionary<string, AGEProgram> programs = new();
    private AGEProgram running;

    public ConfigurationController ConfigurationController;
    public ScreenGenerator ScreenGenerator;
    public GameRegistry GameRegistry;
    public CabinetsController CabinetsController;
    public SceneDatabase SceneDatabase = null;

#if UNITY_EDITOR
    public string nameToExecute;
    public string path;
#endif 

    ConfigurationCommands configCommands = new();

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

        configCommands.ConfigurationController = ConfigurationController;
        configCommands.ScreenGenerator = ScreenGenerator;
        configCommands.SceneDatabase = SceneDatabase;
        configCommands.CabinetsController = CabinetsController;
        configCommands.GameRegistry = GameRegistry;
    }

    public void ParseFiles(string folderPath)
    {
        string[] files = Directory.GetFiles(folderPath, "*.bas");
        programs = new();

        foreach (string filePath in files)
        {
            ConfigManager.WriteConsole($"[basicAge.ProcessFiles] {filePath}");
            ParseFile(filePath);
        }
    }

    private void ParseFile(string filePath)
    {
        string name = Path.GetFileName(filePath);
        AGEProgram prg = new(name);
        try
        {
            prg.Parse(filePath, configCommands);
        }
        catch (Exception e)
        {
            if (configCommands.ScreenGenerator != null)
            {
                ScreenGenerator scr = configCommands.ScreenGenerator;
                scr.Clear();
                scr.Print(0, 0, "Compilation error");
                scr.Print(0, 1, filePath);
                scr.Print(0, 2, $"Line: {prg.LastLineNumberParsed}");
                scr.Print(0, 3, e.Message);
                scr.DrawScreen();
            }
            ConfigManager.WriteConsoleException($"reading {filePath}\n Line: {prg.LastLineNumberParsed}", e);
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

    public bool NotRunning()
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
                    showRuntimeExceptionOnScreen(e, running.Name);
                    running = null;
                }
            }
            ConfigManager.WriteConsole($"{running.Name} END.");
            running = null;
        }

        return;
    }

    private void showRuntimeExceptionOnScreen(Exception e, string prgName)
    {
        if (configCommands.ScreenGenerator == null)
            return;
        ScreenGenerator scr = configCommands.ScreenGenerator;
        string strerror = errorMessage(running, e);
        scr.Clear();
        scr.Print(0, 0, "runtime Exception");
        scr.Print(0, 1, prgName);
        scr.Print(0, 2, $"line: {configCommands.LineNumber}");
        scr.Print(0, 3, e.Message);
        scr.DrawScreen();
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
                showRuntimeExceptionOnScreen(e, running.Name);
                running = null;
                yield break;
            }
            yield return new WaitForSeconds(0.01f);
        }
        ConfigManager.WriteConsole($"{running.Name} END. {running.ContLinesExecuted} lines executed.");
        running = null;
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