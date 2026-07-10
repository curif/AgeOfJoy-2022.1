/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Persistent MR configuration / custom-object error log for the ConfigurationCabinet DEBUG screen.
/// File: ConfigManager.DebugDir/mr-config-debug.log
/// </summary>
public static class MRDebugLog
{
    const string LogFileName = "mr-config-debug.log";
    const int MaxLogBytes = 512 * 1024;
    const int MaxEntriesLoad = 300;

    static readonly object FileLock = new object();
    static string logFilePath;
    static bool initialized;

    public struct Entry
    {
        public DateTime Timestamp;
        public string Level;
        public string Message;
    }

    public struct DisplayLine
    {
        public bool IsDateHeader;
        public int EntryIndex;
        public string Text;
    }

    public static bool TryGetEntry(int entryIndex, out Entry entry)
    {
        entry = default;
        if (entryIndex < 0)
            return false;

        List<Entry> entries = LoadEntriesNewestFirst();
        if (entryIndex >= entries.Count)
            return false;

        entry = entries[entryIndex];
        return true;
    }

    public static string LogFilePath
    {
        get
        {
            EnsureInitialized();
            return logFilePath ?? "(not initialized)";
        }
    }

    public static void LogError(string message) => Write("ERROR", message);

    public static void LogWarning(string message) => Write("WARN", message);

    public static List<Entry> LoadEntriesNewestFirst()
    {
        EnsureInitialized();
        var entries = new List<Entry>();

        try
        {
            if (string.IsNullOrEmpty(logFilePath) || !File.Exists(logFilePath))
                return entries;

            foreach (string rawLine in File.ReadAllLines(logFilePath, Encoding.UTF8))
            {
                if (!TryParseLine(rawLine, out Entry entry))
                    continue;

                entries.Add(entry);
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"[MRDebugLog] read failed: {ex.Message}");
        }

        entries.Sort((a, b) => b.Timestamp.CompareTo(a.Timestamp));

        if (entries.Count > MaxEntriesLoad)
            entries.RemoveRange(MaxEntriesLoad, entries.Count - MaxEntriesLoad);

        return entries;
    }

    public static List<DisplayLine> BuildDisplayLines()
    {
        List<Entry> entries = LoadEntriesNewestFirst();
        var lines = new List<DisplayLine>();

        if (entries.Count == 0)
            return lines;

        string lastDate = null;
        for (int i = 0; i < entries.Count; i++)
        {
            Entry entry = entries[i];
            string date = entry.Timestamp.ToString("yyyy-MM-dd");
            if (!string.Equals(date, lastDate, StringComparison.Ordinal))
            {
                lines.Add(new DisplayLine { IsDateHeader = true, EntryIndex = -1, Text = date });
                lastDate = date;
            }

            string level = string.IsNullOrEmpty(entry.Level) ? "INFO" : entry.Level;
            lines.Add(new DisplayLine
            {
                IsDateHeader = false,
                EntryIndex = i,
                Text = $"{entry.Timestamp:HH:mm} {level} {entry.Message}"
            });
        }

        return lines;
    }

    static void Write(string level, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        string sanitized = message.Replace('|', '/').Replace('\r', ' ').Replace('\n', ' ');
        string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}|{level}|{sanitized}";
        UnityEngine.Debug.Log($"[MRDebugLog] {line}");

        lock (FileLock)
        {
            EnsureInitialized();
            AppendLine(line);
        }
    }

    static void AppendLine(string line)
    {
        try
        {
            File.AppendAllText(logFilePath, line + Environment.NewLine, Encoding.UTF8);
            TrimIfNeeded();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"[MRDebugLog] file write failed: {ex.Message}");
        }
    }

    static void EnsureInitialized()
    {
        if (initialized)
            return;

        try
        {
            ConfigManager.CreateFolder(ConfigManager.DebugDir);
            logFilePath = Path.Combine(ConfigManager.DebugDir, LogFileName);
        }
        catch (Exception ex)
        {
            logFilePath = Path.Combine(Application.persistentDataPath, LogFileName);
            UnityEngine.Debug.LogWarning($"[MRDebugLog] DebugDir fallback: {ex.Message} -> {logFilePath}");
        }

        initialized = true;
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

            File.WriteAllText(
                logFilePath,
                $"--- trimmed {DateTime.Now:O} ---{Environment.NewLine}{tail}",
                Encoding.UTF8);
        }
        catch
        {
            // best effort
        }
    }

    static bool TryParseLine(string rawLine, out Entry entry)
    {
        entry = default;
        if (string.IsNullOrWhiteSpace(rawLine) || rawLine.StartsWith("--- trimmed", StringComparison.Ordinal))
            return false;

        int firstSep = rawLine.IndexOf('|');
        if (firstSep <= 0)
            return false;

        int secondSep = rawLine.IndexOf('|', firstSep + 1);
        if (secondSep <= firstSep)
            return false;

        string timestampText = rawLine.Substring(0, firstSep);
        if (!DateTime.TryParse(timestampText, out DateTime timestamp))
            return false;

        entry.Timestamp = timestamp;
        entry.Level = rawLine.Substring(firstSep + 1, secondSep - firstSep - 1);
        entry.Message = rawLine.Substring(secondSep + 1);
        return !string.IsNullOrEmpty(entry.Message);
    }
}
