/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>Equirectangular skybox images from {BaseDir}/MR/Skyboxes/ (flat or subfolders).</summary>
public static class MRSkyboxesCatalog
{
    static readonly string[] SupportedExtensions =
    {
        ".png", ".jpg", ".jpeg", ".webp", ".bmp"
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

        if (!Directory.Exists(MRPaths.SkyboxesDir))
            return;

        foreach (string file in Directory
                     .EnumerateFiles(MRPaths.SkyboxesDir, "*.*", SearchOption.AllDirectories)
                     .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            if (!IsSupportedImageFile(file))
                continue;

            string relative = GetRelativePath(file);
            if (!string.IsNullOrEmpty(relative))
                cachedRelativePaths.Add(relative);
        }

        ConfigManager.WriteConsole(
            $"[MRSkyboxesCatalog] {cachedRelativePaths.Count} skybox(es) in {MRPaths.SkyboxesDir}");
    }

    public static string GetDisplayLabel(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return "(default)";

        string fileName = Path.GetFileNameWithoutExtension(relativePath);
        return string.IsNullOrEmpty(fileName) ? relativePath : fileName;
    }

    public static bool Exists(string relativePath)
    {
        string fullPath = MRPaths.ResolveSkyboxFilePath(relativePath);
        return !string.IsNullOrEmpty(fullPath) && File.Exists(fullPath);
    }

    public static Texture2D LoadTexture(string relativePath)
    {
        string fullPath = MRPaths.ResolveSkyboxFilePath(relativePath);
        if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath))
            return null;

        try
        {
            byte[] bytes = File.ReadAllBytes(fullPath);
            // No mip chain — sky samples at infinity; mips look extremely soft through the portal.
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
            if (!texture.LoadImage(bytes, markNonReadable: true))
            {
                UnityEngine.Object.Destroy(texture);
                string extension = Path.GetExtension(fullPath);
                ConfigManager.WriteConsoleError(
                    $"[MRSkyboxesCatalog] LoadImage failed for {relativePath} ({extension}). " +
                    "On Quest use .png or .jpg equirect (2:1), ideally 4K+.");
                return null;
            }

            texture.name = Path.GetFileNameWithoutExtension(fullPath);
            texture.filterMode = FilterMode.Bilinear;
            texture.anisoLevel = 0;
            texture.wrapModeU = TextureWrapMode.Repeat;
            texture.wrapModeV = TextureWrapMode.Clamp;
            texture.wrapModeW = TextureWrapMode.Clamp;
            return texture;
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[MRSkyboxesCatalog] load failed: {fullPath}", e);
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

        string skyboxesRoot = Path.GetFullPath(MRPaths.SkyboxesDir);
        string fullPath = Path.GetFullPath(absolutePath);
        if (!fullPath.StartsWith(skyboxesRoot, StringComparison.OrdinalIgnoreCase))
            return null;

        string relative = fullPath.Substring(skyboxesRoot.Length).TrimStart(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);
        return NormalizeRelativePath(relative);
    }
}
