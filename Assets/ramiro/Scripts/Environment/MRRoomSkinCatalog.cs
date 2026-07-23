/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Room skins from <c>MR/Room Skins/{Wall|Ceiling|Floor|Window}/*.png</c>.
/// Window active image per Scene Capture room: <c>MR/Room Skins/Window/window.yaml</c>.
/// Legacy yaml packages (Resources/ramiro/roomskin) still load when present.
/// </summary>
public static class MRRoomSkinCatalog
{
    const string LogPrefix = "[MRRoomSkinCatalog]";

    public const string ResourcesPath = "ramiro/roomskin";
    public static string YamlFileName => MRPaths.RoomSkinYamlFileName;

    public static readonly string[] SurfaceFolderNames =
    {
        "Wall",
        "Ceiling",
        "Floor",
        "Window"
    };

    /// <summary>Resources.Load path for roomskin.yaml (extension omitted — Unity requirement).</summary>
    public static string GetPackageYamlResourcePath(string packageName) =>
        $"{ResourcesPath}/{packageName}/{Path.GetFileNameWithoutExtension(YamlFileName)}";

    static readonly string[] BuiltInResourcePackages = { "galaxy" };

    static List<string> cachedKeys;

    public static IReadOnlyList<string> GetPackageNames()
    {
        if (cachedKeys == null)
            RefreshCache();
        return cachedKeys;
    }

    public static void RefreshCache()
    {
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        MRPaths.EnsureFolders();

        foreach (string surfaceFolder in SurfaceFolderNames)
        {
            string dir = Path.Combine(MRPaths.RoomSkinsDir, surfaceFolder);
            if (!Directory.Exists(dir))
                continue;

            foreach (string file in Directory
                         .EnumerateFiles(dir, "*.*", SearchOption.TopDirectoryOnly)
                         .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                if (!MRPostersCatalog.IsSupportedImageFile(file))
                    continue;

                string fileName = Path.GetFileName(file);
                if (string.IsNullOrEmpty(fileName))
                    continue;

                keys.Add($"{surfaceFolder}/{fileName}");
            }
        }

        // Legacy yaml packages (optional).
        foreach (string packageName in BuiltInResourcePackages)
        {
            if (MRRoomSkinDefinition.TryLoadYamlPackage(packageName, out _))
                keys.Add(packageName);
        }

        if (Directory.Exists(MRPaths.RoomSkinsDir))
        {
            foreach (string dir in Directory.GetDirectories(MRPaths.RoomSkinsDir))
            {
                string packageName = Path.GetFileName(dir);
                if (string.IsNullOrEmpty(packageName) || IsSurfaceFolderName(packageName))
                    continue;

                if (MRRoomSkinDefinition.TryLoadYamlPackage(packageName, out _))
                    keys.Add(packageName);
            }
        }

        cachedKeys = keys.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
        ConfigManager.WriteConsole($"{LogPrefix} {cachedKeys.Count} room skin(s)");
    }

    public static List<MREnvironmentCatalogEntry> GetCatalogEntries()
    {
        var entries = new List<MREnvironmentCatalogEntry>();
        foreach (string key in GetPackageNames())
        {
            if (!MRRoomSkinDefinition.TryLoad(key, out MRRoomSkinDefinition definition))
                continue;

            entries.Add(MREnvironmentCatalogEntry.FromRoomSkin(key, definition.GetMenuLabel()));
        }

        return entries;
    }

    public static List<MREnvironmentCatalogEntry> GetCatalogEntriesForSurfaceFolder(string surfaceFolder)
    {
        var entries = new List<MREnvironmentCatalogEntry>();
        if (string.IsNullOrEmpty(surfaceFolder))
            return entries;

        string prefix = surfaceFolder.Trim().TrimEnd('/') + "/";
        foreach (MREnvironmentCatalogEntry entry in GetCatalogEntries())
        {
            if (entry.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                entries.Add(entry);
        }

        return entries;
    }

    public static string GetDisplayLabel(string packageName)
    {
        if (!MRRoomSkinDefinition.TryLoad(packageName, out MRRoomSkinDefinition definition))
            return packageName;

        return definition.GetDisplayName();
    }

    public static bool IsSurfaceFolderName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        foreach (string folder in SurfaceFolderNames)
        {
            if (string.Equals(folder, name, StringComparison.OrdinalIgnoreCase))
                return true;
            // Accept Walls as alias for Wall.
            if (string.Equals(folder, "Wall", StringComparison.OrdinalIgnoreCase)
                && string.Equals(name, "Walls", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public static bool TryParseSurfaceFolder(string folderName, out MRRoomSkinSurface surface)
    {
        surface = default;
        if (string.IsNullOrEmpty(folderName))
            return false;

        switch (folderName.Trim().ToLowerInvariant())
        {
            case "wall":
            case "walls":
                surface = MRRoomSkinSurface.Walls;
                return true;
            case "ceiling":
                surface = MRRoomSkinSurface.Ceiling;
                return true;
            case "floor":
                surface = MRRoomSkinSurface.Floor;
                return true;
            case "window":
                surface = MRRoomSkinSurface.Window;
                return true;
            default:
                return false;
        }
    }

    public static string GetSurfaceFolderName(MRRoomSkinSurface surface) =>
        surface switch
        {
            MRRoomSkinSurface.Walls => "Wall",
            MRRoomSkinSurface.Ceiling => "Ceiling",
            MRRoomSkinSurface.Floor => "Floor",
            MRRoomSkinSurface.Window => "Window",
            _ => surface.ToString()
        };

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

    /// <summary>
    /// High-quality load for window skybox equirect (no mips).
    /// Falls back to package/Resources load when the primary disk path fails.
    /// </summary>
    public static Texture2D LoadSkyboxTexture(MRRoomSkinDefinition definition)
    {
        if (definition == null || string.IsNullOrEmpty(definition.ResolvedTexturePath))
            return null;

        if (!string.IsNullOrEmpty(definition.PackageDir))
        {
            string fullPath = Path.GetFullPath(Path.Combine(
                definition.PackageDir,
                definition.ResolvedTexturePath.Replace('/', Path.DirectorySeparatorChar)));
            if (File.Exists(fullPath))
            {
                Texture2D fromDisk = LoadSkyboxTextureFromFile(fullPath);
                if (fromDisk != null)
                    return fromDisk;
            }
        }

        // Resources / legacy yaml packages, or disk path missing after seed race.
        Texture2D fallback = LoadPackageTexture(definition);
        if (fallback != null)
        {
            fallback.filterMode = FilterMode.Bilinear;
            fallback.anisoLevel = 0;
            fallback.wrapModeU = TextureWrapMode.Repeat;
            fallback.wrapModeV = TextureWrapMode.Clamp;
            fallback.wrapModeW = TextureWrapMode.Clamp;
        }

        return fallback;
    }

    static Texture2D LoadSkyboxTextureFromFile(string fullPath)
    {
        try
        {
            byte[] bytes = File.ReadAllBytes(fullPath);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
            // Keep CPU copy so wrap/filter can be adjusted safely on Quest.
            if (!texture.LoadImage(bytes, markNonReadable: false))
            {
                UnityEngine.Object.Destroy(texture);
                return null;
            }

            texture.name = Path.GetFileNameWithoutExtension(fullPath);
            texture.filterMode = FilterMode.Bilinear;
            texture.anisoLevel = 0;
            texture.wrapModeU = TextureWrapMode.Repeat;
            texture.wrapModeV = TextureWrapMode.Clamp;
            texture.wrapModeW = TextureWrapMode.Clamp;
            texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
            return texture;
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"{LogPrefix} skybox load {fullPath}", e);
            return null;
        }
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
            ConfigManager.WriteConsoleWarning($"{LogPrefix} missing Resources/{resourcePath}");
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

    /// <summary>Ensures surface folders exist and seeds built-in Ceiling/galaxy.png when missing.</summary>
    public static void SeedBuiltInPackagesToDevice()
    {
        if (seedInProgress)
            return;

        seedInProgress = true;
        try
        {
            MRPaths.EnsureRoomSkinSurfaceFolders();
            SeedBuiltInCeilingGalaxy();
            SeedBuiltInWindowDefault();
        }
        finally
        {
            seedInProgress = false;
        }
    }

    static void SeedBuiltInCeilingGalaxy()
    {
        string ceilingDir = Path.Combine(MRPaths.RoomSkinsDir, "Ceiling");
        ConfigManager.CreateFolder(ceilingDir);

        // Prefer a direct image; skip if any ceiling image already exists.
        foreach (string file in Directory.GetFiles(ceilingDir, "*.*", SearchOption.TopDirectoryOnly))
        {
            if (MRPostersCatalog.IsSupportedImageFile(file))
                return;
        }

        Texture2D galaxy = Resources.Load<Texture2D>($"{ResourcesPath}/galaxy/galaxy");
        if (galaxy == null)
            return;

        string targetPath = Path.Combine(ceilingDir, "galaxy.png");
        try
        {
            Texture2D readable = InstantiateReadableTexture(galaxy, "galaxy");
            if (readable == null)
                return;

            byte[] png = readable.EncodeToPNG();
            UnityEngine.Object.Destroy(readable);
            if (png == null || png.Length == 0)
                return;

            File.WriteAllBytes(targetPath, png);
            ConfigManager.WriteConsole($"{LogPrefix} seeded Ceiling/galaxy.png");
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"{LogPrefix} seed Ceiling/galaxy.png", e);
        }
    }

    /// <summary>
    /// Ensures <see cref="MRRuntimeSettings.DefaultSkyboxImage"/> exists under
    /// <c>MR/Room Skins/Window/</c> and returns its file name (e.g. City360.png).
    /// Falls back to City360.* or the first Window image.
    /// </summary>
    public static string EnsureDefaultWindowImageFromRuntimeSettings()
    {
        string windowDir = Path.Combine(MRPaths.RoomSkinsDir, "Window");
        ConfigManager.CreateFolder(windowDir);

        Texture2D fromSettings = MRRuntimeSettings.DefaultSkyboxImage;
        if (fromSettings != null)
        {
            string fileName = SanitizeWindowImageFileName(fromSettings.name);
            string targetPath = Path.Combine(windowDir, fileName);
            if (!File.Exists(targetPath))
            {
                if (!TryWriteTexturePng(fromSettings, targetPath, Path.GetFileNameWithoutExtension(fileName)))
                {
                    // Fall through to folder scan if encode fails.
                }
                else
                    ConfigManager.WriteConsole(
                        $"{LogPrefix} seeded Window/{fileName} from MRRuntimeSettings.defaultSkyboxImage");
            }

            if (File.Exists(targetPath))
                return fileName;
        }

        // Resources City360 when folder empty / settings missing.
        SeedBuiltInWindowDefault();

        if (TryGetDefaultWindowSkyboxKey(out string relativeKey))
            return Path.GetFileName(relativeKey);

        return null;
    }

    static string SanitizeWindowImageFileName(string textureName)
    {
        string baseName = string.IsNullOrWhiteSpace(textureName) ? "City360" : textureName.Trim();
        foreach (char c in Path.GetInvalidFileNameChars())
            baseName = baseName.Replace(c, '_');
        if (string.IsNullOrWhiteSpace(baseName))
            baseName = "City360";
        if (!baseName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
            && !baseName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
            && !baseName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
            baseName += ".png";
        return baseName;
    }

    static bool TryWriteTexturePng(Texture2D source, string targetPath, string debugName)
    {
        if (source == null || string.IsNullOrEmpty(targetPath))
            return false;

        try
        {
            Texture2D readable = InstantiateReadableTexture(source, debugName);
            if (readable == null)
                return false;

            byte[] png = readable.EncodeToPNG();
            UnityEngine.Object.Destroy(readable);
            if (png == null || png.Length == 0)
                return false;

            File.WriteAllBytes(targetPath, png);
            return true;
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"{LogPrefix} write {targetPath}", e);
            return false;
        }
    }

    /// <summary>Seeds MR/Room Skins/Window/City360.png from Resources / runtime settings when the folder has no images.</summary>
    static void SeedBuiltInWindowDefault()
    {
        string windowDir = Path.Combine(MRPaths.RoomSkinsDir, "Window");
        ConfigManager.CreateFolder(windowDir);

        foreach (string file in Directory.GetFiles(windowDir, "*.*", SearchOption.TopDirectoryOnly))
        {
            if (MRPostersCatalog.IsSupportedImageFile(file))
                return;
        }

        Texture2D city = Resources.Load<Texture2D>($"{ResourcesPath}/Window/City360");
        if (city == null)
            city = MRRuntimeSettings.DefaultSkyboxImage;
        if (city == null)
            return;

        string targetPath = Path.Combine(windowDir, "City360.png");
        if (TryWriteTexturePng(city, targetPath, "City360"))
            ConfigManager.WriteConsole($"{LogPrefix} seeded Window/City360.png");
    }

    /// <summary>
    /// Default window skybox: MRRuntimeSettings image file if present, else City360.*, else first image.
    /// </summary>
    public static bool TryGetDefaultWindowSkyboxKey(out string relativeKey)
    {
        relativeKey = null;
        MRPaths.EnsureRoomSkinSurfaceFolders();
        string windowDir = Path.Combine(MRPaths.RoomSkinsDir, "Window");
        if (!Directory.Exists(windowDir))
            return false;

        Texture2D fromSettings = MRRuntimeSettings.DefaultSkyboxImage;
        if (fromSettings != null)
        {
            string preferredFile = SanitizeWindowImageFileName(fromSettings.name);
            string preferredPath = Path.Combine(windowDir, preferredFile);
            if (File.Exists(preferredPath))
            {
                relativeKey = $"Window/{preferredFile}";
                return true;
            }
        }

        string preferred = null;
        string first = null;
        foreach (string file in Directory
                     .EnumerateFiles(windowDir, "*.*", SearchOption.TopDirectoryOnly)
                     .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            if (!MRPostersCatalog.IsSupportedImageFile(file))
                continue;

            string name = Path.GetFileName(file);
            if (string.IsNullOrEmpty(name))
                continue;

            if (first == null)
                first = name;

            if (Path.GetFileNameWithoutExtension(name)
                    .Equals("City360", StringComparison.OrdinalIgnoreCase)
                || Path.GetFileNameWithoutExtension(name)
                    .Equals("default", StringComparison.OrdinalIgnoreCase))
            {
                preferred = name;
                break;
            }
        }

        string chosen = preferred ?? first;
        if (string.IsNullOrEmpty(chosen))
            return false;

        relativeKey = $"Window/{chosen}";
        return true;
    }
}
