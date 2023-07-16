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

    public ConfigurationController configurationController;
    public ScreenGenerator screenGenerator;

#if UNITY_EDITOR
    public string nameToExecute;
    public string path;
#endif 

    ConfigurationCommands configCommands = new();

    public void Start()
    {
        if (configurationController == null)
            configurationController = GetComponent<ConfigurationController>();
        if (screenGenerator == null)
            screenGenerator = GetComponent<ScreenGenerator>();
        configCommands.ConfigurationController = configurationController;
        configCommands.ScreenGenerator = screenGenerator;
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
        AGEProgram prg = new();

        string name = Path.GetFileName(filePath);
        try
        {
            prg.Name = name;
            prg.Parse(filePath, configCommands);
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"reading {filePath} Line number: {prg.LastLineNumberParsed}", e);
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

    string errorMessage(AGEProgram prg, Exception exception)
    {
        string str = $"ERROR: PRG {prg.Name} line: {prg.LastLineNumberExecuted}\n";
        str += $"Exception: {exception.Message}\n";
        return str;
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
                    ConfigManager.WriteConsole(errorMessage(running, e));
                    ConfigManager.WriteConsoleException($"running {running.Name} line: {running.LastLineNumberExecuted}", e);
                    running = null;
                }
            }
            ConfigManager.WriteConsole($"{running.Name} END.");
            running = null;
        }

        return;
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
                ConfigManager.WriteConsoleException($"running {running.Name} line: {running.LastLineNumberExecuted}", e);
                running = null;
                yield break;
            }
            yield return new WaitForSeconds(0.01f);
        }
        ConfigManager.WriteConsole($"{running.Name} END.");
        running = null;
    }

#if UNITY_EDITOR
    public void ExecuteInEditorMode()
    {
        Run(nameToExecute);
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
    }
}
#endif