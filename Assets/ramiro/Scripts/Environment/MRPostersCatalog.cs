/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>Wall poster images from {BaseDir}/MR/Posters/ (flat or subfolders).</summary>
public static class MRPostersCatalog
{
    static readonly string[] SupportedExtensions =
    {
        ".png", ".jpg", ".jpeg", ".tif", ".tiff", ".webp", ".bmp"
    };

    static List<string> cachedRelativePaths;

    public static IReadOnlyList<string> GetRelativePaths()
    {
        if (cachedRelativePaths == null)
            RefreshCache();
        return cachedRelativePaths;
    }

    public static void RefreshCache()
    {
        cachedRelativePaths = new List<string>();
        MRPaths.EnsureFolders();

        if (!Directory.Exists(MRPaths.PostersDir))
            return;

        foreach (string file in Directory
                     .EnumerateFiles(MRPaths.PostersDir, "*.*", SearchOption.AllDirectories)
                     .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            if (!IsSupportedImageFile(file))
                continue;

            string relative = GetRelativePath(file);
            if (!string.IsNullOrEmpty(relative))
                cachedRelativePaths.Add(relative);
        }

        ConfigManager.WriteConsole(
            $"[MRPostersCatalog] {cachedRelativePaths.Count} poster(s) in {MRPaths.PostersDir}");
    }

    public static List<MREnvironmentCatalogEntry> GetCatalogEntries()
    {
        var entries = new List<MREnvironmentCatalogEntry>();
        foreach (string relativePath in GetRelativePaths())
        {
            entries.Add(MREnvironmentCatalogEntry.FromPoster(
                relativePath,
                GetDisplayLabel(relativePath)));
        }

        return entries;
    }

    public static string GetDisplayLabel(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return "(poster)";

        string fileName = Path.GetFileNameWithoutExtension(relativePath);
        return string.IsNullOrEmpty(fileName) ? relativePath : fileName;
    }

    public static bool Exists(string relativePath)
    {
        string fullPath = MRPaths.ResolvePosterFilePath(relativePath);
        return !string.IsNullOrEmpty(fullPath) && File.Exists(fullPath);
    }

    public static Texture2D LoadTexture(string relativePath)
    {
        string fullPath = MRPaths.ResolvePosterFilePath(relativePath);
        if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath))
            return null;

        try
        {
            byte[] bytes = File.ReadAllBytes(fullPath);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(bytes))
            {
                UnityEngine.Object.Destroy(texture);
                string extension = Path.GetExtension(fullPath);
                ConfigManager.WriteConsoleError(
                    $"[MRPostersCatalog] LoadImage failed for {relativePath} ({extension}). " +
                    "On Quest use .png or .jpg — .tif is often unsupported.");
                return null;
            }

            texture.name = Path.GetFileNameWithoutExtension(fullPath);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            return texture;
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[MRPostersCatalog] load failed: {fullPath}", e);
            return null;
        }
    }

    public static string NormalizeRelativePath(string relativePath) =>
        string.IsNullOrEmpty(relativePath)
            ? string.Empty
            : relativePath.Replace('\\', '/').TrimStart('/');

    public static bool IsSupportedImageFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;

        string extension = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(extension))
            return false;

        foreach (string supported in SupportedExtensions)
        {
            if (string.Equals(extension, supported, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    static string GetRelativePath(string absolutePath)
    {
        if (string.IsNullOrEmpty(absolutePath))
            return null;

        string postersRoot = Path.GetFullPath(MRPaths.PostersDir);
        string fullPath = Path.GetFullPath(absolutePath);
        if (!fullPath.StartsWith(postersRoot, StringComparison.OrdinalIgnoreCase))
            return null;

        string relative = fullPath.Substring(postersRoot.Length).TrimStart(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);
        return NormalizeRelativePath(relative);
    }
}
