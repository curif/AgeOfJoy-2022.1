/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

/// <summary>
/// Per Scene Capture room window skybox — persisted in
/// <c>MR/Room Skins/Window/window.yaml</c> (not PlayerPrefs / objects-layout).
/// Empty <c>image</c> after CRT Rem = portal off. Missing room entry on first open
/// auto-sets <see cref="MRRuntimeSettings.DefaultSkyboxImage"/> into window.yaml.
/// </summary>
public static class MRSkyboxSettings
{
    const string LogPrefix = "[MRSkyboxSettings]";
    const string WindowFolderName = "Window";
    const int YamlVersion = 1;

    /// <summary>Legacy PlayerPrefs — migrated once into window.yaml when present.</summary>
    const string LegacySelectedKey = "MR.Skybox.SelectedRelativePath";
    const string LegacyYawKey = "MR.Skybox.YawDegrees";

    public const float YawStepDegrees = 5f;
    public const float MinYawDegrees = 0f;
    public const float MaxYawDegrees = 355f;

    static bool loaded;
    static string loadedForRoomId = string.Empty;
    /// <summary>Catalog key <c>Window/file.png</c>, or empty when portal is off.</summary>
    static string selectedRelativePath = string.Empty;
    static float yawDegrees;
    static MRWindowYamlFile cachedFile;

    public static string SelectedRelativePath
    {
        get
        {
            EnsureLoaded();
            return selectedRelativePath;
        }
    }

    /// <summary>True when window.yaml has an image for the bound Scene Capture room.</summary>
    public static bool HasActiveWindow
    {
        get
        {
            EnsureLoaded();
            return !string.IsNullOrEmpty(selectedRelativePath);
        }
    }

    public static float YawDegrees
    {
        get
        {
            EnsureLoaded();
            return yawDegrees;
        }
    }

    /// <summary>True when no Window image is selected for the bound room.</summary>
    public static bool UsesBuiltInDefault
    {
        get
        {
            EnsureLoaded();
            return string.IsNullOrEmpty(selectedRelativePath);
        }
    }

    /// <summary>1 when <paramref name="catalogKey"/> is the active Window image for this room.</summary>
    public static int GetActiveInstanceCount(string catalogKey)
    {
        EnsureLoaded();
        if (string.IsNullOrEmpty(catalogKey) || string.IsNullOrEmpty(selectedRelativePath))
            return 0;

        return string.Equals(
            NormalizeWindowKey(catalogKey),
            NormalizeWindowKey(selectedRelativePath),
            StringComparison.OrdinalIgnoreCase)
            ? 1
            : 0;
    }

    public static bool IsActiveCatalogKey(string catalogKey) =>
        GetActiveInstanceCount(catalogKey) > 0;

    /// <summary>Call when <see cref="MRActiveRoom"/> binds or clears.</summary>
    public static void OnActiveRoomChanged()
    {
        loaded = false;
        cachedFile = null;
        EnsureLoaded();
    }

    public static void EnsureLoaded()
    {
        string roomId = CurrentRoomScopeId();
        if (loaded && string.Equals(loadedForRoomId, roomId, StringComparison.OrdinalIgnoreCase))
            return;

        MRPaths.EnsureFolders();
        MRRoomSkinCatalog.SeedBuiltInPackagesToDevice();
        MRWindowYamlFile file = LoadYamlFile();
        MRWindowYamlRoomEntry entry = FindRoomEntry(file, roomId);

        if (entry == null)
        {
            // One-shot migrate legacy PlayerPrefs into yaml for this room.
            TryMigrateLegacyPlayerPrefs(roomId, file, out entry);
        }

        // First open for this Scene Capture room: seed MRRuntimeSettings.defaultSkyboxImage.
        if (entry == null)
            TryAssignRuntimeDefaultForNewRoom(roomId, file, out entry);

        if (entry != null)
        {
            selectedRelativePath = ToCatalogKey(entry.Image);
            yawDegrees = entry.Yaw;
        }
        else
        {
            selectedRelativePath = string.Empty;
            yawDegrees = 0f;
        }

        yawDegrees = Mathf.Clamp(yawDegrees, MinYawDegrees, MaxYawDegrees);
        yawDegrees = SnapYaw(yawDegrees);
        selectedRelativePath = NormalizeWindowKey(selectedRelativePath);

        loadedForRoomId = roomId;
        loaded = true;

        ConfigManager.WriteConsole(
            string.IsNullOrEmpty(roomId)
                ? $"{LogPrefix} loaded global yaw={yawDegrees:0}° selected={(string.IsNullOrEmpty(selectedRelativePath) ? "none" : selectedRelativePath)} ({MRPaths.WindowYamlPath})"
                : $"{LogPrefix} loaded room={roomId} yaw={yawDegrees:0}° selected={(string.IsNullOrEmpty(selectedRelativePath) ? "none" : selectedRelativePath)} ({MRPaths.WindowYamlPath})");
    }

    /// <summary>
    /// First visit to a room (no yaml row): copy default from _global or
    /// <see cref="MRRuntimeSettings.DefaultSkyboxImage"/> and persist.
    /// </summary>
    static void TryAssignRuntimeDefaultForNewRoom(
        string roomId,
        MRWindowYamlFile file,
        out MRWindowYamlRoomEntry entry)
    {
        entry = null;
        if (file == null)
            return;

        // Prefer existing _global default if it already has an image.
        MRWindowYamlRoomEntry global = FindRoomEntry(file, "_global");
        string imageFile = global != null && !string.IsNullOrWhiteSpace(global.Image)
            ? Path.GetFileName(global.Image.Trim())
            : null;
        float yaw = global != null ? global.Yaw : 0f;

        if (string.IsNullOrEmpty(imageFile))
            imageFile = MRRoomSkinCatalog.EnsureDefaultWindowImageFromRuntimeSettings();

        if (string.IsNullOrEmpty(imageFile))
            return;

        string id = string.IsNullOrEmpty(roomId) ? "_global" : roomId;
        // Avoid duplicating _global when that is what we are loading.
        entry = FindRoomEntry(file, id);
        if (entry == null)
        {
            entry = new MRWindowYamlRoomEntry { RoomId = id };
            file.Rooms.Add(entry);
        }

        entry.Image = imageFile;
        entry.Yaw = SnapYaw(yaw);
        SaveYamlFile(file);

        ConfigManager.WriteConsole(
            $"{LogPrefix} first open room={id} → default image={imageFile} (MRRuntimeSettings / _global)");
    }

    /// <summary>
    /// Set active Window image for the bound room. Pass null/empty to disable the portal.
    /// Accepts <c>City360.png</c> or <c>Window/City360.png</c>.
    /// </summary>
    public static void SetSelectedRelativePath(string relativePath)
    {
        EnsureLoaded();
        selectedRelativePath = NormalizeWindowKey(relativePath);
        PersistCurrentRoom();
        ConfigManager.WriteConsole(
            string.IsNullOrEmpty(selectedRelativePath)
                ? $"{LogPrefix} selected=none (portal off) room={ScopeLabel()}"
                : $"{LogPrefix} selected={selectedRelativePath} room={ScopeLabel()}");
    }

    public static float AdjustYaw(int direction)
    {
        EnsureLoaded();
        if (direction == 0)
            return yawDegrees;

        yawDegrees = SnapYaw(yawDegrees + direction * YawStepDegrees);
        if (yawDegrees > MaxYawDegrees)
            yawDegrees = MinYawDegrees;
        if (yawDegrees < MinYawDegrees)
            yawDegrees = MaxYawDegrees;

        PersistCurrentRoom();
        ConfigManager.WriteConsole($"{LogPrefix} yaw={yawDegrees:0}° room={ScopeLabel()}");
        return yawDegrees;
    }

    public static void SetYawDegrees(float degrees)
    {
        EnsureLoaded();
        yawDegrees = SnapYaw(degrees);
        PersistCurrentRoom();
    }

    static void PersistCurrentRoom()
    {
        MRPaths.EnsureFolders();
        MRWindowYamlFile file = LoadYamlFile();
        string roomId = CurrentRoomScopeId();
        string imageFile = ToImageFileName(selectedRelativePath);

        MRWindowYamlRoomEntry entry = FindRoomEntry(file, roomId);
        if (entry == null)
        {
            entry = new MRWindowYamlRoomEntry { RoomId = string.IsNullOrEmpty(roomId) ? "_global" : roomId };
            file.Rooms.Add(entry);
        }

        entry.Image = imageFile ?? string.Empty;
        entry.Yaw = yawDegrees;

        // Drop room rows with no image and default yaw to keep the file tidy? Keep yaw even if image empty.
        SaveYamlFile(file);
    }

    static void TryMigrateLegacyPlayerPrefs(string roomId, MRWindowYamlFile file, out MRWindowYamlRoomEntry entry)
    {
        entry = null;
        string selectedKey = ScopedLegacyKey(LegacySelectedKey, roomId);
        string yawKey = ScopedLegacyKey(LegacyYawKey, roomId);

        bool hasSelected = PlayerPrefs.HasKey(selectedKey);
        string legacyPath = hasSelected
            ? (PlayerPrefs.GetString(selectedKey, string.Empty) ?? string.Empty)
            : string.Empty;
        if (hasSelected && string.IsNullOrEmpty(legacyPath))
        {
            PlayerPrefs.DeleteKey(selectedKey);
            hasSelected = false;
        }

        if (!hasSelected && !string.IsNullOrEmpty(roomId) && PlayerPrefs.HasKey(LegacySelectedKey))
        {
            legacyPath = PlayerPrefs.GetString(LegacySelectedKey, string.Empty) ?? string.Empty;
            hasSelected = !string.IsNullOrEmpty(legacyPath);
        }

        float legacyYaw = 0f;
        bool hasYaw = PlayerPrefs.HasKey(yawKey);
        if (hasYaw)
            legacyYaw = PlayerPrefs.GetFloat(yawKey, 0f);
        else if (!string.IsNullOrEmpty(roomId) && PlayerPrefs.HasKey(LegacyYawKey))
        {
            legacyYaw = PlayerPrefs.GetFloat(LegacyYawKey, 0f);
            hasYaw = true;
        }

        legacyPath = NormalizeWindowKey(legacyPath);
        if (!hasSelected && !hasYaw)
            return;

        entry = new MRWindowYamlRoomEntry
        {
            RoomId = string.IsNullOrEmpty(roomId) ? "_global" : roomId,
            Image = ToImageFileName(legacyPath) ?? string.Empty,
            Yaw = SnapYaw(legacyYaw)
        };
        file.Rooms.Add(entry);
        SaveYamlFile(file);

        if (hasSelected)
            PlayerPrefs.DeleteKey(selectedKey);
        if (hasYaw)
            PlayerPrefs.DeleteKey(yawKey);
        PlayerPrefs.Save();

        ConfigManager.WriteConsole(
            $"{LogPrefix} migrated PlayerPrefs → window.yaml room={entry.RoomId} image={entry.Image}");
    }

    static MRWindowYamlFile LoadYamlFile()
    {
        if (cachedFile != null)
            return cachedFile;

        string path = MRPaths.WindowYamlPath;
        MRPaths.SeedWindowYamlIfNeeded();

        if (!File.Exists(path))
        {
            cachedFile = new MRWindowYamlFile { Version = YamlVersion };
            return cachedFile;
        }

        try
        {
            string text = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(text))
            {
                cachedFile = new MRWindowYamlFile { Version = YamlVersion };
                return cachedFile;
            }

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            cachedFile = deserializer.Deserialize<MRWindowYamlFile>(text) ?? new MRWindowYamlFile();
            if (cachedFile.Rooms == null)
                cachedFile.Rooms = new List<MRWindowYamlRoomEntry>();
            if (cachedFile.Version <= 0)
                cachedFile.Version = YamlVersion;
            return cachedFile;
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"{LogPrefix} failed to read {path}", e);
            cachedFile = new MRWindowYamlFile { Version = YamlVersion };
            return cachedFile;
        }
    }

    static void SaveYamlFile(MRWindowYamlFile file)
    {
        if (file == null)
            return;

        file.Version = YamlVersion;
        if (file.Rooms == null)
            file.Rooms = new List<MRWindowYamlRoomEntry>();

        string path = MRPaths.WindowYamlPath;
        try
        {
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                ConfigManager.CreateFolder(dir);

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            string yaml = serializer.Serialize(file);
            File.WriteAllText(path, yaml);
            cachedFile = file;
            ConfigManager.WriteConsole($"{LogPrefix} saved {path} ({file.Rooms.Count} room(s))");
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"{LogPrefix} failed to write {path}", e);
        }
    }

    static MRWindowYamlRoomEntry FindRoomEntry(MRWindowYamlFile file, string roomId)
    {
        if (file?.Rooms == null)
            return null;

        string want = string.IsNullOrEmpty(roomId) ? "_global" : roomId;
        string wantSafe = SanitizeRoomId(want);

        for (int i = 0; i < file.Rooms.Count; i++)
        {
            MRWindowYamlRoomEntry entry = file.Rooms[i];
            if (entry == null || string.IsNullOrEmpty(entry.RoomId))
                continue;

            if (string.Equals(entry.RoomId, want, StringComparison.OrdinalIgnoreCase))
                return entry;
            if (string.Equals(SanitizeRoomId(entry.RoomId), wantSafe, StringComparison.OrdinalIgnoreCase))
                return entry;
        }

        return null;
    }

    /// <summary><c>Window/a.png</c> or <c>a.png</c> → <c>Window/a.png</c>.</summary>
    static string NormalizeWindowKey(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        string normalized = path.Replace('\\', '/').Trim().TrimStart('/');
        if (normalized.StartsWith(WindowFolderName + "/", StringComparison.OrdinalIgnoreCase))
            return WindowFolderName + "/" + Path.GetFileName(normalized);

        // Bare filename from yaml.
        if (!normalized.Contains("/"))
            return WindowFolderName + "/" + normalized;

        // Legacy Skyboxes/ paths kept as-is for ApplySelectedSkybox fallback.
        return MRSkyboxesCatalog.NormalizeRelativePath(normalized);
    }

    static string ToCatalogKey(string imageFileName)
    {
        if (string.IsNullOrWhiteSpace(imageFileName))
            return string.Empty;
        return NormalizeWindowKey(imageFileName.Trim());
    }

    static string ToImageFileName(string catalogOrFile)
    {
        if (string.IsNullOrWhiteSpace(catalogOrFile))
            return string.Empty;
        string key = NormalizeWindowKey(catalogOrFile);
        if (string.IsNullOrEmpty(key))
            return string.Empty;
        return Path.GetFileName(key);
    }

    static float SnapYaw(float degrees)
    {
        float snapped = Mathf.Round(degrees / YawStepDegrees) * YawStepDegrees;
        while (snapped < 0f)
            snapped += 360f;
        while (snapped >= 360f)
            snapped -= 360f;
        return snapped;
    }

    static string CurrentRoomScopeId() =>
        MRActiveRoom.HasBoundRoom ? MRActiveRoom.BoundRoomId : string.Empty;

    static string ScopeLabel() =>
        string.IsNullOrEmpty(CurrentRoomScopeId()) ? "_global" : CurrentRoomScopeId();

    static string ScopedLegacyKey(string baseKey, string roomId)
    {
        if (string.IsNullOrEmpty(roomId))
            return baseKey;
        return $"{baseKey}.Room.{SanitizeRoomId(roomId)}";
    }

    static string SanitizeRoomId(string roomId)
    {
        if (string.IsNullOrEmpty(roomId))
            return "unknown";

        var chars = roomId.Trim().ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            if (char.IsLetterOrDigit(c) || c == '-' || c == '_')
                continue;
            chars[i] = '_';
        }

        return new string(chars);
    }
}

[Serializable]
public class MRWindowYamlFile
{
    public int Version = 1;
    public List<MRWindowYamlRoomEntry> Rooms = new List<MRWindowYamlRoomEntry>();
}

[Serializable]
public class MRWindowYamlRoomEntry
{
    /// <summary>Scene Capture / MRUK room id (same as <c>MR/Rooms/{id}</c>).</summary>
    public string RoomId;
    /// <summary>Image file under <c>MR/Room Skins/Window/</c>. Empty = portal off.</summary>
    public string Image;
    public float Yaw;
}
