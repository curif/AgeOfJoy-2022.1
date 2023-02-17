using UnityEngine;
using System.Collections;
using System.IO;
using System;
using UnityEngine.Events;

public class FileMonitor : MonoBehaviour
{
  public string ConfigFileName = ""; // change this to the path of the file you want to monitor
  public float Interval = 2f; // interval in seconds to check for changes
  public UnityEvent OnFileChanged;

  private DateTime lastWriteTime;

  private IEnumerator Start()
  {
    // get the initial last write time of the file

    string filePath = ConfigManager.ConfigDir + "/" + ConfigFileName;
    ConfigManager.WriteConsole($"[FileMonitor]: monitoring file {filePath} ");
    FileInfo fileInfo = new FileInfo(filePath);
    lastWriteTime = fileInfo.LastWriteTime;

    // start the coroutine to monitor the file
    while (true)
    {
        yield return new WaitForSeconds(Interval);

        // get the current last write time of the file
        fileInfo = new FileInfo(filePath);
        DateTime currentLastWriteTime = fileInfo.LastWriteTime;

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

