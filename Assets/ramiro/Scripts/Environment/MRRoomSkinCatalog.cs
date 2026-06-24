/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Room skin packages: built-in under Resources/ramiro/roomskin + user folders MR/Room Skins/.
/// </summary>
public static class MRRoomSkinCatalog
{
    const string LogPrefix = "[MRRoomSkinCatalog]";

    public const string ResourcesPath = "ramiro/roomskin";
    public static string YamlFileName => MRPaths.RoomSkinYamlFileName;

    /// <summary>Resources.Load path for roomskin.yaml (extension omitted — Unity requirement).</summary>
    public static string GetPackageYamlResourcePath(string packageName) =>
        $"{ResourcesPath}/{packageName}/{Path.GetFileNameWithoutExtension(YamlFileName)}";

    static readonly string[] BuiltInResourcePackages = { "galaxy" };

    static List<string> cachedPackageNames;

    public static IReadOnlyList<string> GetPackageNames()
    {
        if (cachedPackageNames == null)
            RefreshCache();
        return cachedPackageNames;
    }

    public static void RefreshCache()
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string packageName in BuiltInResourcePackages)
        {
            if (MRRoomSkinDefinition.TryLoad(packageName, out _))
                names.Add(packageName);
        }

        ConfigManager.CreateFolder(MRPaths.RoomSkinsDir);
        if (Directory.Exists(MRPaths.RoomSkinsDir))
        {
            foreach (string dir in Directory.GetDirectories(MRPaths.RoomSkinsDir))
            {
                string packageName = Path.GetFileName(dir);
                if (string.IsNullOrEmpty(packageName))
                    continue;

                if (MRRoomSkinDefinition.TryLoad(packageName, out _))
                    names.Add(packageName);
            }
        }

        cachedPackageNames = names.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
        ConfigManager.WriteConsole($"{LogPrefix} {cachedPackageNames.Count} room skin package(s)");
    }

    public static List<MREnvironmentCatalogEntry> GetCatalogEntries()
    {
        var entries = new List<MREnvironmentCatalogEntry>();
        foreach (string packageName in GetPackageNames())
        {
            if (!MRRoomSkinDefinition.TryLoad(packageName, out MRRoomSkinDefinition definition))
                continue;

            entries.Add(MREnvironmentCatalogEntry.FromRoomSkin(
                packageName,
                definition.GetMenuLabel()));
        }

        return entries;
    }

    public static string GetDisplayLabel(string packageName)
    {
        if (!MRRoomSkinDefinition.TryLoad(packageName, out MRRoomSkinDefinition definition))
            return packageName;

        return definition.GetDisplayName();
    }

    public static Texture2D LoadTexture(MRRoomSkinDefinition definition, string relativeFile)
    {
        if (definition == null || string.IsNullOrEmpty(relativeFile))
            return null;

        string normalized = NormalizeRelativePath(relativeFile);
        if (definition.FromResources)
            return LoadTextureFromResources(definition.PackageName, normalized);

        return LoadTextureFromDisk(definition.PackageDir, normalized);
    }

    public static Texture2D LoadPackageTexture(MRRoomSkinDefinition definition)
    {
        if (definition == null || string.IsNullOrEmpty(definition.ResolvedTexturePath))
            return null;

        return LoadTexture(definition, definition.ResolvedTexturePath);
    }

    static Texture2D LoadTextureFromResources(string packageName, string relativeFile)
    {
        string normalized = NormalizeRelativePath(relativeFile);
        string fileWithoutExt = Path.GetFileNameWithoutExtension(normalized);
        if (string.IsNullOrEmpty(fileWithoutExt))
            return null;

        string subDir = Path.GetDirectoryName(normalized)?.Replace('\\', '/');
        string resourcePath = string.IsNullOrEmpty(subDir)
            ? $"{ResourcesPath}/{packageName}/{fileWithoutExt}"
            : $"{ResourcesPath}/{packageName}/{subDir}/{fileWithoutExt}";

        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} missing Resources/{resourcePath}");
            return null;
        }

        return InstantiateReadableTexture(texture, fileWithoutExt);
    }

    static Texture2D LoadTextureFromDisk(string packageDir, string relativeFile)
    {
        if (string.IsNullOrEmpty(packageDir))
            return null;

        string fullPath = Path.GetFullPath(
            Path.Combine(packageDir, relativeFile.Replace('/', Path.DirectorySeparatorChar)));
        string packageRoot = Path.GetFullPath(packageDir);
        if (!fullPath.StartsWith(packageRoot, StringComparison.OrdinalIgnoreCase))
            return null;

        if (!File.Exists(fullPath) || !MRPostersCatalog.IsSupportedImageFile(fullPath))
            return null;

        try
        {
            byte[] bytes = File.ReadAllBytes(fullPath);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(bytes))
            {
                UnityEngine.Object.Destroy(texture);
                return null;
            }

            texture.name = Path.GetFileNameWithoutExtension(fullPath);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Repeat;
            return texture;
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"{LogPrefix} load {fullPath}", e);
            return null;
        }
    }

    static Texture2D InstantiateReadableTexture(Texture2D source, string name)
    {
        if (source == null)
            return null;

        var copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        copy.name = name;
        copy.filterMode = FilterMode.Bilinear;
        copy.wrapMode = TextureWrapMode.Repeat;

        RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(source, rt);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;
        copy.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
        copy.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);
        return copy;
    }

    public static string NormalizeRelativePath(string relativePath) =>
        string.IsNullOrEmpty(relativePath)
            ? string.Empty
            : relativePath.Replace('\\', '/').TrimStart('/');

    static bool seedInProgress;

    /// <summary>Copies built-in Resources packages to MR/Room Skins/ on device/editor data dir.</summary>
    public static void SeedBuiltInPackagesToDevice()
    {
        if (seedInProgress)
            return;

        seedInProgress = true;
        try
        {
            ConfigManager.CreateFolder(MRPaths.RoomSkinsDir);
            foreach (string packageName in BuiltInResourcePackages)
            {
                if (!MRRoomSkinDefinition.TryLoad(packageName, out MRRoomSkinDefinition definition))
                {
                    ConfigManager.WriteConsoleWarning(
                        $"{LogPrefix} seed skipped '{packageName}': built-in package failed to load from Resources");
                    continue;
                }

                string targetDir = Path.Combine(MRPaths.RoomSkinsDir, packageName);
                string targetYaml = Path.Combine(targetDir, YamlFileName);
                if (File.Exists(targetYaml))
                    continue;

                try
                {
                    ConfigManager.CreateFolder(targetDir);
                    WritePackageToDisk(definition, targetDir);
                    ConfigManager.WriteConsole($"{LogPrefix} seeded built-in package '{packageName}' -> {targetDir}");
                }
                catch (Exception e)
                {
                    ConfigManager.WriteConsoleException($"{LogPrefix} seed failed for {packageName}", e);
                }
            }

            cachedPackageNames = null;
        }
        finally
        {
            seedInProgress = false;
        }
    }

    static void WritePackageToDisk(MRRoomSkinDefinition definition, string targetDir)
    {
        TextAsset yaml = Resources.Load<TextAsset>(GetPackageYamlResourcePath(definition.PackageName));
        if (yaml != null)
            File.WriteAllText(Path.Combine(targetDir, YamlFileName), yaml.text);
        else
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} seed missing Resources/{GetPackageYamlResourcePath(definition.PackageName)}");

        WriteTextureFileToDisk(definition, definition.ResolvedTexturePath, targetDir);
    }

    static void WriteTextureFileToDisk(MRRoomSkinDefinition definition, string relativeFile, string targetDir)
    {
        if (string.IsNullOrEmpty(relativeFile))
            return;

        string normalized = NormalizeRelativePath(relativeFile);
        string destPath = Path.Combine(targetDir, normalized.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(destPath))
            return;

        Texture2D texture = LoadTextureFromResources(definition.PackageName, normalized);
        if (texture == null)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} seed missing texture Resources/{ResourcesPath}/{definition.PackageName}/{Path.GetFileNameWithoutExtension(normalized)}");
            return;
        }

        try
        {
            string destDir = Path.GetDirectoryName(destPath);
            if (!string.IsNullOrEmpty(destDir))
                ConfigManager.CreateFolder(destDir);

            File.WriteAllBytes(destPath, texture.EncodeToPNG());
        }
        finally
        {
            UnityEngine.Object.Destroy(texture);
        }
    }
}
