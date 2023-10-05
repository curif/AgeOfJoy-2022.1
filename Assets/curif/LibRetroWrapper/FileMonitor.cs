using UnityEngine;
using System.Collections;
using System.IO;
using System;
using UnityEngine.Events;
using System.Threading;


public class FileMonitor : MonoBehaviour
{
    public string ConfigFileName = ""; // change this to the path of the file you want to monitor
    public float Interval = 2f; // interval in seconds to check for changes
    public UnityEvent OnFileChanged;

    private DateTime lastWriteTime;
    private string filePath;

    private object lockFile = new();
    void Start()
    {
        // get the initial last write time of the file

        filePath = ConfigManager.ConfigDir + "/" + ConfigFileName;
        ConfigManager.WriteConsole($"[FileMonitor]: monitoring file {filePath} ");
        StartCoroutine(monitor());
    }
    public void fileLock()
    {
        Monitor.Enter(lockFile);
    }
    public void fileUnlock()
    {
        Monitor.Exit(lockFile);
    }

    IEnumerator monitor()
    {
        DateTime currentLastWriteTime;
        FileInfo fileInfo;

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
    }
}

