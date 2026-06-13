/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;

/// <summary>
/// Builds Unity-friendly MP4 caches (video-only) when source files have broken audio metadata.
/// Requires ffmpeg on PATH (Editor/Windows dev) or pre-generated cache on device.
/// </summary>
public static class MRVideoRemuxUtility
{
    const string LogPrefix = "[MRVideoRemuxUtility]";
    const string CacheFolderName = ".unity-video-cache";

    public static string GetNoAudioCachePath(string sourceVideoPath)
    {
        if (string.IsNullOrEmpty(sourceVideoPath))
            return null;

        string packageDir = Path.GetDirectoryName(sourceVideoPath);
        if (string.IsNullOrEmpty(packageDir))
            return null;

        string baseName = Path.GetFileNameWithoutExtension(sourceVideoPath);
        return Path.Combine(packageDir, CacheFolderName, baseName + "_noaudio.mp4");
    }

    public static bool IsCacheFresh(string sourceVideoPath, string cachePath)
    {
        if (!File.Exists(cachePath) || !File.Exists(sourceVideoPath))
            return false;

        return File.GetLastWriteTimeUtc(cachePath) >= File.GetLastWriteTimeUtc(sourceVideoPath);
    }

    public static IEnumerator EnsureNoAudioCacheAsync(string sourceVideoPath, Action<string, string> onComplete)
    {
        string cachePath = GetNoAudioCachePath(sourceVideoPath);
        if (string.IsNullOrEmpty(cachePath))
        {
            onComplete?.Invoke(null, "invalid source path");
            yield break;
        }

        if (IsCacheFresh(sourceVideoPath, cachePath))
        {
            onComplete?.Invoke(cachePath, null);
            yield break;
        }

        string cacheDir = Path.GetDirectoryName(cachePath);
        try
        {
            if (!string.IsNullOrEmpty(cacheDir))
                ConfigManager.CreateFolder(cacheDir);
        }
        catch (Exception ex)
        {
            onComplete?.Invoke(null, ex.Message);
            yield break;
        }

        string ffmpeg = FindFfmpegExecutable();
        if (string.IsNullOrEmpty(ffmpeg))
        {
            onComplete?.Invoke(null,
                "ffmpeg not found on PATH — install with: winget install ffmpeg "
                + "or remux manually: ffmpeg -i input.mp4 -c:v copy -an -movflags +faststart output.mp4");
            yield break;
        }

        bool finished = false;
        string error = null;
        bool success = false;

        yield return RunFfmpegRemuxCoroutine(ffmpeg, sourceVideoPath, cachePath, (ok, err) =>
        {
            success = ok;
            error = err;
            finished = true;
        });

        if (!finished)
            error = "ffmpeg did not finish";

        if (success && File.Exists(cachePath))
        {
            ConfigManager.WriteConsole($"{LogPrefix} created cache {cachePath}");
            onComplete?.Invoke(cachePath, null);
        }
        else
        {
            onComplete?.Invoke(null, string.IsNullOrEmpty(error) ? "ffmpeg remux failed" : error);
        }
    }

    static IEnumerator RunFfmpegRemuxCoroutine(
        string ffmpegPath,
        string inputPath,
        string outputPath,
        Action<bool, string> onComplete)
    {
        string args =
            $"-hide_banner -loglevel error -y -i \"{inputPath}\" -c:v copy -an -movflags +faststart \"{outputPath}\"";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };

        string stderr = string.Empty;
        bool started = false;
        try
        {
            started = process.Start();
        }
        catch (Exception ex)
        {
            onComplete?.Invoke(false, ex.Message);
            yield break;
        }

        if (!started)
        {
            onComplete?.Invoke(false, "failed to start ffmpeg");
            yield break;
        }

        while (!process.HasExited)
            yield return null;

        try
        {
            stderr = process.StandardError.ReadToEnd();
        }
        catch
        {
            // best effort
        }

        bool ok = process.ExitCode == 0 && File.Exists(outputPath);
        onComplete?.Invoke(ok, ok ? null : TrimError(stderr));
    }

    static string FindFfmpegExecutable()
    {
        string pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(pathEnv))
        {
            foreach (string folder in pathEnv.Split(Path.PathSeparator))
            {
                if (string.IsNullOrEmpty(folder))
                    continue;

                string candidate = Path.Combine(folder.Trim(), "ffmpeg.exe");
                if (File.Exists(candidate))
                    return candidate;

                candidate = Path.Combine(folder.Trim(), "ffmpeg");
                if (File.Exists(candidate))
                    return candidate;
            }
        }

        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string[] common =
        {
            Path.Combine(localAppData, "Microsoft", "WinGet", "Links", "ffmpeg.exe"),
            @"C:\ffmpeg\bin\ffmpeg.exe",
            @"C:\Program Files\ffmpeg\bin\ffmpeg.exe"
        };

        foreach (string candidate in common)
        {
            if (File.Exists(candidate))
                return candidate;
        }

        string wingetPackages = Path.Combine(localAppData, "Microsoft", "WinGet", "Packages");
        if (Directory.Exists(wingetPackages))
        {
            try
            {
                foreach (string ffmpegExe in Directory.GetFiles(wingetPackages, "ffmpeg.exe", SearchOption.AllDirectories))
                {
                    if (ffmpegExe.IndexOf("\\bin\\", StringComparison.OrdinalIgnoreCase) >= 0)
                        return ffmpegExe;
                }
            }
            catch
            {
                // best effort
            }
        }

        return null;
    }

    static string TrimError(string stderr)
    {
        if (string.IsNullOrWhiteSpace(stderr))
            return "ffmpeg remux failed";

        stderr = stderr.Trim();
        if (stderr.Length > 240)
            stderr = stderr.Substring(0, 240) + "...";

        return stderr;
    }
}
