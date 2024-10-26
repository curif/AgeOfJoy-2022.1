using UnityEngine;
using System.IO;
using System;
using UnityEngine.Events;
using System.Threading;


public class FileMonitor : MonoBehaviour
{
    public string FileName = ""; // change this to the path of the file you want to monitor
    public string Path = "";
    public UnityEvent OnFileChanged;
    public float checkInterval = 2f; // Check every 2 seconds
    
    private float nextCheckTime = 0f;
    private DateTime lastWriteTime = DateTime.MinValue;
    private string filePath;
    private object lockFile = new();
    bool started;
    void Start()
    {
        if (string.IsNullOrEmpty(Path))
            Path = ConfigManager.ConfigDir;

        if (!string.IsNullOrEmpty(FileName))
            StartMonitor(Path, FileName);
    }

    void Update()
    {
        if (started && Time.time >= nextCheckTime)
        {
            nextCheckTime = Time.time + checkInterval; // Set next check time

            if (!File.Exists(filePath)) return;

            // Check the last write time of the file
            DateTime currentWriteTime = File.GetLastWriteTime(filePath);
            if (currentWriteTime != lastWriteTime)
            {
                lastWriteTime = currentWriteTime; // Update the last write time
                OnFileChanged.Invoke();           // Invoke the UnityEvent on the main thread
                ConfigManager.WriteConsole($"[FileMonitor]: File changed: {filePath}");
            }
        }
    }

    public void StartMonitor(string path, string name)
    {
        filePath = System.IO.Path.Combine(path, name);

        if (File.Exists(filePath))
            lastWriteTime = File.GetLastWriteTime(filePath);
        else
            lastWriteTime = DateTime.MinValue;

        nextCheckTime = 0;
        started = true;
        ConfigManager.WriteConsole($"[FileMonitor]: Started for File: {filePath}");
    }

    public void fileLock()
    {
        Monitor.Enter(lockFile);
    }
    public void fileUnlock()
    {
        Monitor.Exit(lockFile);
    }
 }

