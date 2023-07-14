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

#if UNITY_EDITOR
    public string nameToExecute;
#endif 

    public void Start()
    {
        ProcessFiles(ConfigManager.AGEBasicDir);
    }

    public void ProcessFiles(string folderPath)
    {
        string[] files = Directory.GetFiles(folderPath, "*.bas");
        programs = new();

        foreach (string filePath in files)
        {
            ConfigManager.WriteConsole($"[basicAge.ProcessFiles] {filePath}");
            AGEProgram prg = new();
            try
            {
                prg.Parse(filePath);
            }
            catch (Exception e)
            {
                ConfigManager.WriteConsoleException($"reading {filePath} Line number: {prg.LastLineNumberParsed}", e);
            }
            string name = Path.GetFileName(filePath);
            prg.Name = name;
            programs.Add(name, prg);
        }
    }


    public void ListPrograms()
    {
        foreach (KeyValuePair<string, AGEProgram> kvp in programs)
        {
            ConfigManager.WriteConsole($" {kvp.Key}");
        }
    }

    public void run(string name)
    {

        if (!programs.ContainsKey(name))
            throw new Exception($"program {name} doesn't exists");

        if (running != null)
            throw new Exception($"you can't run {name}, {running.Name} is runnig");

        running = programs[name];
        running.PrepareToRun();

        StartCoroutine(runProgram());

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
        run(nameToExecute);
    }
    public void Log()
    {
        ConfigManager.WriteConsole(programs[nameToExecute].Log());
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
        if(GUILayout.Button("Execute"))
        {
          myScript.ExecuteInEditorMode();
        }
        if(GUILayout.Button("Reprocess path"))
        {
          myScript.Start();
        }
        if(GUILayout.Button("LOG"))
        {
          myScript.Log();
        }
    }
}
#endif