/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.IO;
using System.Text;
using AOJ.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MR↔VR transition diagnostics — always appends to a file on device (Quest: debug folder under app data).
/// Pull via: adb pull /sdcard/Android/data/com.curif.AgeOfJoy/debug/mr-transition.log
/// </summary>
public static class MRTransitionLog
{
    const string LogFileName = "mr-transition.log";
    const string LatestFileName = "mr-transition-latest.log";
    const int MaxLogBytes = 2 * 1024 * 1024;

    static readonly object FileLock = new object();
    static string logFilePath;
    static string latestFilePath;
    static bool initialized;
    static int sessionId;

    public static string LogFilePath => logFilePath ?? "(not initialized)";

    public static void EnsureSession(string reason)
    {
        lock (FileLock)
        {
            if (!initialized)
                InitializePaths();

            sessionId++;
            string banner =
                $"{NewLine}========== MR TRANSITION LOG session #{sessionId} {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} =========={NewLine}" +
                $"reason={reason}{NewLine}" +
                $"unity={Application.unityVersion} platform={Application.platform} device={SystemInfo.deviceModel}{NewLine}" +
                $"logFile={logFilePath}{NewLine}" +
                $"latestCopy={latestFilePath}{NewLine}" +
                $"baseDir={ConfigManager.BaseDir}{NewLine}" +
                $"debugDir={ConfigManager.DebugDir}{NewLine}";

            WriteRaw(banner);
            LogScenes("session-start");
            LogManagerState("session-start");
        }
    }

    public static void Log(string message)
    {
        WriteLine("INFO", message);
    }

    public static void LogWarning(string message)
    {
        WriteLine("WARN", message);
    }

    public static void LogError(string message)
    {
        WriteLine("ERROR", message);
    }

    public static void LogException(string context, Exception ex)
    {
        WriteLine("EXC", $"{context}: {ex.GetType().Name}: {ex.Message}{NewLine}{ex.StackTrace}");
    }

    public static void LogStep(string phase, string detail = null)
    {
        string line = string.IsNullOrEmpty(detail)
            ? $"STEP [{phase}] t={Time.realtimeSinceStartup:F3}s unscaled={Time.unscaledTime:F3}s frame={Time.frameCount}"
            : $"STEP [{phase}] {detail} | t={Time.realtimeSinceStartup:F3}s frame={Time.frameCount}";
        WriteLine("STEP", line);
    }

    public static void LogManagerState(string tag)
    {
        var sb = new StringBuilder();
        sb.Append($"tag={tag}");

        if (MixedRealityManager.Instance != null)
        {
            MixedRealityManager mgr = MixedRealityManager.Instance;
            sb.Append($" mode={mgr.CurrentMode}");
            sb.Append($" mrActive={mgr.IsMrEnvironmentActive()}");
            sb.Append($" mrOrigin={(mgr.MRSpaceOrigin != null ? mgr.MRSpaceOrigin.position.ToString("F2") : "null")}");
        }
        else
        {
            sb.Append(" manager=null");
        }

        if (MRLayoutRegistry.Instance != null)
            sb.Append($" spawned={MRLayoutRegistry.Instance.SpawnedCount}");

        sb.Append($" libretroLoaded={LibretroMameCore.GameLoaded}");
        sb.Append($" eventPassthrough={(EventManager.Instance != null && EventManager.Instance.IsPassthrough)}");

        Transform player = FindPlayer();
        if (player != null)
            sb.Append($" playerPos={player.position.ToString("F2")} rotY={player.rotation.eulerAngles.y:F1}");

        WriteLine("STATE", sb.ToString());
    }

    public static void LogScenes(string tag)
    {
        var sb = new StringBuilder();
        sb.Append($"tag={tag} count={SceneManager.sceneCount} active={SceneManager.GetActiveScene().name}");
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            sb.Append($" | [{i}] {scene.name} loaded={scene.isLoaded} valid={scene.IsValid()}");
        }

        WriteLine("SCENES", sb.ToString());
    }

    public static void LogPassthrough(string tag, MRPassthroughController controller)
    {
        if (controller == null)
        {
            WriteLine("PT", $"{tag} controller=null");
            return;
        }

        WriteLine("PT",
            $"{tag} enabled={controller.IsPassthroughEnabled} active={controller.IsPassthroughActive} " +
            $"systemReady={controller.PassthroughSystemReady}");
    }

    static void InitializePaths()
    {
        try
        {
            ConfigManager.CreateFolder(ConfigManager.DebugDir);
            logFilePath = Path.Combine(ConfigManager.DebugDir, LogFileName);
            latestFilePath = Path.Combine(ConfigManager.DebugDir, LatestFileName);
        }
        catch (Exception ex)
        {
            logFilePath = Path.Combine(Application.persistentDataPath, LogFileName);
            latestFilePath = Path.Combine(Application.persistentDataPath, LatestFileName);
            UnityEngine.Debug.LogWarning($"[MRTransitionLog] DebugDir fallback: {ex.Message} -> {logFilePath}");
        }

        initialized = true;
        TrimIfNeeded();
    }

    static void WriteLine(string level, string message)
    {
        string line = $"{DateTime.Now:HH:mm:ss.fff} [{level}] {message}";
        UnityEngine.Debug.Log($"[MRTransitionLog] {line}");

        lock (FileLock)
        {
            if (!initialized)
                InitializePaths();
            WriteRaw(line + NewLine);
        }
    }

    static void WriteRaw(string text)
    {
        try
        {
            File.AppendAllText(logFilePath, text, Encoding.UTF8);
            File.AppendAllText(latestFilePath, text, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"[MRTransitionLog] file write failed: {ex.Message}");
        }
    }

    static void TrimIfNeeded()
    {
        try
        {
            if (!File.Exists(logFilePath))
                return;

            var info = new FileInfo(logFilePath);
            if (info.Length <= MaxLogBytes)
                return;

            string tail = File.ReadAllText(logFilePath, Encoding.UTF8);
            if (tail.Length > MaxLogBytes / 2)
                tail = tail.Substring(tail.Length - MaxLogBytes / 2);

            File.WriteAllText(logFilePath, $"--- trimmed {DateTime.Now:O} ---{NewLine}{tail}", Encoding.UTF8);
        }
        catch
        {
            // best effort
        }
    }

    static Transform FindPlayer()
    {
        PlayerController pc = UnityEngine.Object.FindObjectOfType<PlayerController>();
        if (pc != null && pc.PlayerControllerGameObject != null)
            return pc.PlayerControllerGameObject.transform;
        if (pc != null)
            return pc.transform;

        GameObject tagged = GameObject.FindGameObjectWithTag("Player");
        return tagged != null ? tagged.transform : null;
    }

    static string NewLine => Environment.NewLine;
}
