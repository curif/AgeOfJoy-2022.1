using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DebugConsole : MonoBehaviour
{
    private List<string> logMessages = new List<string>();
    private int maxLogMessages = 40;
    private static bool? debugEnabled = null;
    private static List<string> debugKeywords = new List<string>();
    private Text console;

    void Start()
    {
        console = GetComponent<Text>();
    }

    void OnEnable()
    {
        if (IsDebugEnabled())
        {
            Application.logMessageReceived += HandleLog;
        }
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (debugKeywords.Count == 0 || debugKeywords.Exists(keyword => logString.Contains(keyword)))
        {
            logMessages.Add(logString);
            if (logMessages.Count > maxLogMessages)
            {
                logMessages.RemoveAt(0);
            }
            console.text = string.Join("\n", logMessages);
        }
    }

#if UNITY_EDITOR
    void OnGUI()
    {
        int height = 20;
        int margin = 10;
        for (int i = 0; i < logMessages.Count; i++)
        {
            GUI.Label(new Rect(10, margin + (i * height), 1000, height), logMessages[i]);
        }
    }
#endif

    public static bool IsDebugEnabled()
    {
        if (!debugEnabled.HasValue)
        {
            string debugFilePath = Path.Combine(ConfigManager.BaseDir, "debug.txt");
            bool debugFileExists = File.Exists(debugFilePath);
            if (debugFileExists)
            {
                string[] lines = File.ReadAllLines(debugFilePath);
                foreach (var line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (!string.IsNullOrEmpty(trimmedLine) && !trimmedLine.StartsWith("#"))
                    {
                        debugKeywords.Add(trimmedLine);
                    }
                }
            }
            debugEnabled = debugFileExists;
        }
        return debugEnabled.Value;
    }
}
