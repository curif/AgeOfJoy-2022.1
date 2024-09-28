using UnityEngine;
using System.Collections;
using System.IO;
using System;
using UnityEngine.Events;
using System.Threading;


public class FileMonitor : MonoBehaviour
{
    public string ConfigFileName = ""; // change this to the path of the file you want to monitor
    public string ConfigPath = "";
    public float Interval = 2f; // interval in seconds to check for changes
    public UnityEvent OnFileChanged;

    private DateTime lastWriteTime;

    private FileSystemWatcher fileWatcher;

    private object lockFile = new();
    void Start()
    {
        if (string.IsNullOrEmpty(ConfigPath))
            ConfigPath = ConfigManager.ConfigDir;

        ///filePath = Path.Combine(ConfigPath, ConfigFileName);
        //ConfigManager.WriteConsole($"[FileMonitor.monitor]: file {filePath} ");

        StartMonitor(ConfigPath, ConfigFileName);
        // Initialize the FileSystemWatcher
        
        // get the initial last write time of the file
        //StartCoroutine(monitor());
    }
    public void StartMonitor(string path, string fileName)
    {
        if (fileWatcher != null)
            fileWatcher.Dispose();

        ConfigPath = path;
        ConfigFileName = fileName;
        fileWatcher = new FileSystemWatcher
        {
            Path = ConfigPath,
            Filter = ConfigFileName, // Specify the file name
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName //changed or created
        };

        // Add event handlers
        fileWatcher.Changed += OnChanged;
        fileWatcher.Created += OnChanged;

        // Begin watching
        fileWatcher.EnableRaisingEvents = true;
    }
    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        ConfigManager.WriteConsole($"[FileMonitor]: changed {ConfigFileName} ");
        OnFileChanged.Invoke();
    }

    private void OnDestroy()
    {
        // Clean up the FileSystemWatcher
        if (fileWatcher != null)
        {
            fileWatcher.EnableRaisingEvents = false;
            fileWatcher.Changed -= OnChanged;
            fileWatcher.Created -= OnChanged;
            fileWatcher.Dispose();
        }

    }

    public void fileLock()
    {
        Monitor.Enter(lockFile);
    }
    public void fileUnlock()
    {
        Monitor.Exit(lockFile);
    }
    /*
    IEnumerator monitor()
    {
        DateTime currentLastWriteTime;
        FileInfo fileInfo;

        while (!Init.PermissionGranted)
        {
            yield return new WaitForSeconds(1f);
        }

        filePath = Path.Combine(ConfigPath, ConfigFileName);
        ConfigManager.WriteConsole($"[FileMonitor.monitor]: file {filePath} ");

        lock (lockFile)
        {
            fileInfo = new FileInfo(filePath);
            lastWriteTime = fileInfo.LastWriteTime;
        }

        // start the coroutine to monitor the file
        while (true)
        {
            yield return new WaitForSeconds(Interval);

            lock (lockFile)
            {
                // get the current last write time of the file
                fileInfo.Refresh();
                currentLastWriteTime = fileInfo.LastWriteTime;
            }
            // compare the current last write time with the previous time
            if (currentLastWriteTime != lastWriteTime)
            {
                // the file has been modified, do something
                ConfigManager.WriteConsole($"[FileMonitor]: changed {filePath} ");
                OnFileChanged.Invoke();

                // update the last write time
                lastWriteTime = currentLastWriteTime;
            }
        }
    }*/
}

