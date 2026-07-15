/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.IO;

/// <summary>
/// On-disk MR folder layout under ConfigManager.BaseDir (Quest: .../com.curif.AgeOfJoy/MR/).
/// Per-room layouts: MR/Rooms/{roomId}/cabinets-layout.yaml + objects-layout.yaml.
/// </summary>
public static class MRPaths
{
    public const string MrFolderName = "MR";
    public const string CustomObjectsFolderName = "Custom Objects";
    public const string PostersFolderName = "Posters";
    public const string MagazinesFolderName = "Magazines";
    public const string RoomSkinsFolderName = "Room Skins";
    public const string RoomsFolderName = "Rooms";
    public const string CabinetsLayoutFileName = "cabinets-layout.yaml";
    public const string ObjectsLayoutFileName = "objects-layout.yaml";
    public const string RoomMetaFileName = "meta.yaml";
    public const string CustomObjectYamlFileName = "object.yaml";
    public const string RoomSkinYamlFileName = "roomskin.yaml";
    public const string MagazineYamlFileName = "magazine.yaml";
    /// <summary>Default image filename when <c>texture</c> is omitted in roomskin.yaml.</summary>
    public const string RoomSkinDefaultTextureFileName = "texture.png";
    /// <summary>Marker: global MR/*.yaml layouts were copied into the first room folder once.</summary>
    public const string LegacyGlobalMigratedMarkerFileName = ".legacy-global-migrated";

    public const string LegacyCabinetsLayoutFileName = "mr-layout.yaml";
    public const string LegacyObjectsLayoutFileName = "mr-environment-layout.yaml";

    public static string MrDir => Path.Combine(ConfigManager.BaseDir, MrFolderName);
    public static string CustomObjectsDir => Path.Combine(MrDir, CustomObjectsFolderName);
    public static string PostersDir => Path.Combine(MrDir, PostersFolderName);
    public static string MagazinesDir => Path.Combine(MrDir, MagazinesFolderName);
    public static string RoomSkinsDir => Path.Combine(MrDir, RoomSkinsFolderName);
    public static string RoomsDir => Path.Combine(MrDir, RoomsFolderName);
    public static string CabinetsLayoutPath => Path.Combine(MrDir, CabinetsLayoutFileName);
    public static string ObjectsLayoutPath => Path.Combine(MrDir, ObjectsLayoutFileName);
    public static string LegacyGlobalMigratedMarkerPath =>
        Path.Combine(RoomsDir, LegacyGlobalMigratedMarkerFileName);

    public const string ExamplePackageName = "Example";

    static bool foldersEnsured;

    /// <summary>Creates MR/ subfolders and one-time seeds. Safe to call repeatedly (no-op after first run).</summary>
    public static void EnsureFolders()
    {
        if (foldersEnsured)
            return;

        foldersEnsured = true;
        ConfigManager.CreateFolder(MrDir);
        ConfigManager.CreateFolder(CustomObjectsDir);
        ConfigManager.CreateFolder(PostersDir);
        ConfigManager.CreateFolder(MagazinesDir);
        ConfigManager.CreateFolder(RoomSkinsDir);
        ConfigManager.CreateFolder(RoomsDir);
        SeedExampleCustomObjectIfNeeded();
        SeedPostersReadmeIfNeeded();
        SeedMagazineReadmeIfNeeded();
        SeedExampleMagazineIfNeeded();
        MRRoomSkinCatalog.SeedBuiltInPackagesToDevice();
    }

    public static string GetRoomDir(string roomId)
    {
        if (string.IsNullOrEmpty(roomId))
            return null;

        string safeId = SanitizeRoomIdForPath(roomId);
        if (string.IsNullOrEmpty(safeId))
            return null;

        return Path.Combine(RoomsDir, safeId);
    }

    public static string GetRoomCabinetsLayoutPath(string roomId)
    {
        string roomDir = GetRoomDir(roomId);
        return string.IsNullOrEmpty(roomDir) ? null : Path.Combine(roomDir, CabinetsLayoutFileName);
    }

    public static string GetRoomObjectsLayoutPath(string roomId)
    {
        string roomDir = GetRoomDir(roomId);
        return string.IsNullOrEmpty(roomDir) ? null : Path.Combine(roomDir, ObjectsLayoutFileName);
    }

    public static string GetRoomMetaPath(string roomId)
    {
        string roomDir = GetRoomDir(roomId);
        return string.IsNullOrEmpty(roomDir) ? null : Path.Combine(roomDir, RoomMetaFileName);
    }

    /// <summary>
    /// Creates MR/Rooms/{roomId}/, optional meta.yaml, and one-shot migrates global layouts
    /// into this room when the global→room migration has not run yet.
    /// </summary>
    public static void EnsureRoomLayouts(string roomId)
    {
        EnsureFolders();
        // CabinetsDB legacy → global MR/ (before room copy).
        MigrateLegacyLayoutIfNeeded(
            Path.Combine(ConfigManager.CabinetsDB, LegacyCabinetsLayoutFileName),
            CabinetsLayoutPath);
        MigrateLegacyLayoutIfNeeded(
            Path.Combine(ConfigManager.CabinetsDB, LegacyObjectsLayoutFileName),
            ObjectsLayoutPath);

        string roomDir = GetRoomDir(roomId);
        if (string.IsNullOrEmpty(roomDir))
            return;

        ConfigManager.CreateFolder(roomDir);
        EnsureRoomMeta(roomId);

        string roomCabinets = GetRoomCabinetsLayoutPath(roomId);
        string roomObjects = GetRoomObjectsLayoutPath(roomId);
        TryMigrateGlobalLayoutsIntoRoomOnce(roomCabinets, roomObjects);
    }

    static void EnsureRoomMeta(string roomId)
    {
        string metaPath = GetRoomMetaPath(roomId);
        if (string.IsNullOrEmpty(metaPath) || File.Exists(metaPath))
            return;

        try
        {
            string stamp = DateTime.UtcNow.ToString("o");
            string yaml =
                "version: 1\n" +
                $"roomId: \"{SanitizeRoomIdForPath(roomId)}\"\n" +
                "displayName: \"\"\n" +
                $"createdAt: \"{stamp}\"\n" +
                $"lastUsed: \"{stamp}\"\n";
            File.WriteAllText(metaPath, yaml);
            ConfigManager.WriteConsole($"[MRPaths] created room meta {metaPath}");
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[MRPaths] failed to write room meta for {roomId}", e);
        }
    }

    /// <summary>
    /// Copy global MR/cabinets-layout.yaml + objects-layout.yaml into the first room only
    /// (marker under Rooms/). Further rooms start empty.
    /// </summary>
    static void TryMigrateGlobalLayoutsIntoRoomOnce(string roomCabinetsPath, string roomObjectsPath)
    {
        if (string.IsNullOrEmpty(roomCabinetsPath) || string.IsNullOrEmpty(roomObjectsPath))
            return;

        if (File.Exists(LegacyGlobalMigratedMarkerPath))
            return;

        bool copiedAny = false;
        if (!File.Exists(roomCabinetsPath) && File.Exists(CabinetsLayoutPath))
        {
            TryCopyFile(CabinetsLayoutPath, roomCabinetsPath);
            copiedAny = true;
        }

        if (!File.Exists(roomObjectsPath) && File.Exists(ObjectsLayoutPath))
        {
            TryCopyFile(ObjectsLayoutPath, roomObjectsPath);
            copiedAny = true;
        }

        // Mark migrated even if global files were missing — avoid repeating on every new room.
        try
        {
            ConfigManager.CreateFolder(RoomsDir);
            File.WriteAllText(
                LegacyGlobalMigratedMarkerPath,
                copiedAny
                    ? $"migratedAt: {DateTime.UtcNow:o}\n"
                    : $"skippedEmptyAt: {DateTime.UtcNow:o}\n");
            ConfigManager.WriteConsole(
                copiedAny
                    ? $"[MRPaths] migrated global layouts into room folder (marker {LegacyGlobalMigratedMarkerPath})"
                    : $"[MRPaths] no global layouts to migrate (marker {LegacyGlobalMigratedMarkerPath})");
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException("[MRPaths] failed to write global migration marker", e);
        }
    }

    static void TryCopyFile(string source, string dest)
    {
        try
        {
            string destDir = Path.GetDirectoryName(dest);
            if (!string.IsNullOrEmpty(destDir))
                ConfigManager.CreateFolder(destDir);
            File.Copy(source, dest, overwrite: false);
            ConfigManager.WriteConsole($"[MRPaths] migrated layout {source} -> {dest}");
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[MRPaths] migrate failed {source} -> {dest}", e);
        }
    }

    static string SanitizeRoomIdForPath(string roomId)
    {
        if (string.IsNullOrEmpty(roomId))
            return null;

        char[] invalid = Path.GetInvalidFileNameChars();
        var chars = roomId.Trim().ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (Array.IndexOf(invalid, chars[i]) >= 0)
                chars[i] = '-';
        }

        string safe = new string(chars);
        return string.IsNullOrWhiteSpace(safe) ? null : safe;
    }

    /// <summary>Writes MR/Posters/README.txt when the folder is new or empty.</summary>
    public static void SeedPostersReadmeIfNeeded()
    {
        if (!Directory.Exists(PostersDir))
            return;

        if (HasAnyPosterImage())
            return;

        string readmePath = Path.Combine(PostersDir, "README.txt");
        if (File.Exists(readmePath))
            return;

        try
        {
            File.WriteAllText(readmePath, PostersReadmeText);
            ConfigManager.WriteConsole($"[MRPaths] created posters readme at {readmePath}");
        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleException($"[MRPaths] failed to seed {readmePath}", e);
        }
    }

    static bool HasAnyPosterImage()
    {
        if (!Directory.Exists(PostersDir))
            return false;

        foreach (string file in Directory.GetFiles(PostersDir, "*.*", SearchOption.AllDirectories))
        {
            if (MRPostersCatalog.IsSupportedImageFile(file))
                return true;
        }

        return false;
    }

    public static string ResolvePosterFilePath(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return null;

        string normalized = MRPostersCatalog.NormalizeRelativePath(relativePath);
        string fullPath = Path.GetFullPath(Path.Combine(PostersDir, normalized.Replace('/', Path.DirectorySeparatorChar)));
        string postersRoot = Path.GetFullPath(PostersDir);
        if (!fullPath.StartsWith(postersRoot, System.StringComparison.OrdinalIgnoreCase))
            return null;

        return fullPath;
    }

    public static string GetMagazineIssueDir(string issueName)
    {
        if (string.IsNullOrEmpty(issueName))
            return null;

        return Path.Combine(MagazinesDir, issueName);
    }

    public static string GetMagazineIssueYamlPath(string issueName)
    {
        string issueDir = GetMagazineIssueDir(issueName);
        return string.IsNullOrEmpty(issueDir) ? null : Path.Combine(issueDir, MagazineYamlFileName);
    }

    public static string ResolveMagazinePagePath(string issueName, string pageFileName)
    {
        if (string.IsNullOrEmpty(issueName) || string.IsNullOrEmpty(pageFileName))
            return null;

        string issueDir = GetMagazineIssueDir(issueName);
        string fullPath = Path.GetFullPath(Path.Combine(issueDir, pageFileName));
        string magazineRoot = Path.GetFullPath(MagazinesDir);
        if (!fullPath.StartsWith(magazineRoot, System.StringComparison.OrdinalIgnoreCase))
            return null;

        return fullPath;
    }

    /// <summary>Writes MR/Magazines/README.txt when the folder is new or empty.</summary>
    public static void SeedMagazineReadmeIfNeeded()
    {
        if (!Directory.Exists(MagazinesDir))
            return;

        string readmePath = Path.Combine(MagazinesDir, "README.txt");
        if (File.Exists(readmePath))
            return;

        if (HasAnyMagazineIssue())
            return;

        try
        {
            ConfigManager.CreateFolder(Path.Combine(MagazinesDir, ExamplePackageName));
            File.WriteAllText(readmePath, MagazineReadmeText);
            ConfigManager.WriteConsole($"[MRPaths] created magazine readme at {readmePath}");
        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleException($"[MRPaths] failed to seed {readmePath}", e);
        }
    }

    /// <summary>Writes MR/Magazines/Example/magazine.yaml when missing.</summary>
    public static void SeedExampleMagazineIfNeeded()
    {
        if (!Directory.Exists(MagazinesDir))
            return;

        string exampleDir = Path.Combine(MagazinesDir, ExamplePackageName);
        string yamlPath = Path.Combine(exampleDir, MagazineYamlFileName);
        if (File.Exists(yamlPath))
            return;

        try
        {
            ConfigManager.CreateFolder(exampleDir);
            File.WriteAllText(yamlPath, ExampleMagazineYaml);
            ConfigManager.WriteConsole($"[MRPaths] created example magazine yaml at {yamlPath}");
        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleException($"[MRPaths] failed to seed {yamlPath}", e);
        }
    }

    static bool HasAnyMagazineIssue()
    {
        if (!Directory.Exists(MagazinesDir))
            return false;

        foreach (string issueDir in Directory.GetDirectories(MagazinesDir))
        {
            foreach (string file in Directory.GetFiles(issueDir, "*.*", SearchOption.TopDirectoryOnly))
            {
                if (MRPostersCatalog.IsSupportedImageFile(file))
                    return true;
            }
        }

        return false;
    }

    const string MagazineReadmeText = @"Age of Joy — MR magazines
==============================

Copy one folder per issue under MR/Magazines/ (Quest: Android/data/com.curif.AgeOfJoy/MR/Magazines/).

Each issue folder must contain numbered page images (sorted by leading number):

  MR/Magazines/MyIssue/1.jpg    — front cover (outside, left)
  MR/Magazines/MyIssue/2.jpg    — inside front cover
  MR/Magazines/MyIssue/3.jpg    — first interior page
  ...
  MR/Magazines/MyIssue/N-1.jpg  — inside back cover
  MR/Magazines/MyIssue/N.jpg    — back cover (outside, right)

Cover roles are inferred from sort order (1st, 2nd, penultimate, last).
Optional magazine.yaml can override cover filenames.

Supported formats: .png .jpg .jpeg .webp .bmp
On Quest prefer .png or .jpg (.tif often does not load).

Bookshelves in MR CONFIGURATION group up to 8 issues per shelf.
";

    const string ExampleMagazineYaml = @"# Optional override — covers are inferred from sorted page images when absent.
# Copy numbered images into this folder (1.jpg, 2.jpg, ... N.jpg).

version: 1

covers:
  front: 1.jpg
  insideFront: 2.jpg
  insideBack: 75.jpg
  back: 76.jpg
";

    const string PostersReadmeText = @"Age of Joy — MR wall posters
================================

Copy poster images here, then open MR CONFIGURATION → POSTERS on the cabinet CRT.

Supported formats: .png .jpg .jpeg .webp .bmp
On Quest prefer .png or .jpg (.tif often does not load).
Subfolders are allowed (e.g. Posters/80/my-movie.png).

Placed posters are saved in MR/objects-layout.yaml.
";

    /// <summary>Writes Custom Objects/Example/object.yaml when the folder has no packages yet.</summary>
    public static void SeedExampleCustomObjectIfNeeded()
    {
        if (!Directory.Exists(CustomObjectsDir) || HasAnyCustomObjectPackage())
            return;

        string exampleDir = Path.Combine(CustomObjectsDir, ExamplePackageName);
        string yamlPath = Path.Combine(exampleDir, CustomObjectYamlFileName);
        if (File.Exists(yamlPath))
            return;

        try
        {
            ConfigManager.CreateFolder(exampleDir);
            File.WriteAllText(yamlPath, ExampleObjectYaml);
            ConfigManager.WriteConsole(
                $"[MRPaths] created example custom object at {yamlPath} — add example.glb to spawn");
        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleException($"[MRPaths] failed to seed {yamlPath}", e);
        }
    }

    static bool HasAnyCustomObjectPackage()
    {
        if (!Directory.Exists(CustomObjectsDir))
            return false;

        foreach (string dir in Directory.GetDirectories(CustomObjectsDir))
        {
            if (File.Exists(Path.Combine(dir, CustomObjectYamlFileName)))
                return true;
        }

        return false;
    }

    const string ExampleObjectYaml = @"# Age of Joy — MR custom object example (schema v1)
# Spec: CUSTOM_OBJECT_YAML.md in the repo (Assets/ramiro/)
#
# 1. Copy or rename this folder under MR/Custom Objects/
# 2. Add your mesh as example.glb (or change model.file below)
# 3. Place via MR CRT menu → CUSTOM OBJECTS → Example

version: 1

name: Example
displayName: Example object

model:
  file: example.glb
  scale: 1.0

# placement.surfaceType: 0=Floor 1=Wall 2=Ceiling 3=Free3D 4=Table 5=Object
# placement.facingAxis: 0=+Z 1=-Z 2=+X 3=-X
# placement.stickRotationAxis: 0=WorldYaw 1=WorldPitch 2=WorldRoll
placement:
  surfaceType: 0
  facingAxis: 0
  allowStickRotation: true
  stickRotationAxis: 0
  stickRotationSpeed: 90
  providesAnchor: false
  # anchorTarget: Top

collision:
  mode: mesh
  convex: true

# Optional — uncomment to enable spatial audio (add example.wav)
#audio:
#  file: example.wav
#  loop: true
#  volume: 0.6
#  playOnAwake: true
#  distance:
#    min: 0.5
#    max: 4.0

# Optional components (see CUSTOM_OBJECT_YAML.md)
#components:
#  - rotator
#  - grab
#  - animator
#
#rotator:
#  target: Blades
#  axis: y
#  speed: 180
#
#animator:
#  clip: Walk
#  target: Character
#  loop: true
#  playOnAwake: true
#
#grab:
#  twoHands: false
#  returnOnRelease: true
#  hideHands: true
";

    public static string ResolveCabinetsLayoutPath()
    {
        EnsureFolders();
        MigrateLegacyLayoutIfNeeded(
            Path.Combine(ConfigManager.CabinetsDB, LegacyCabinetsLayoutFileName),
            CabinetsLayoutPath);

        if (MRActiveRoom.HasBoundRoom)
        {
            EnsureRoomLayouts(MRActiveRoom.BoundRoomId);
            string roomPath = GetRoomCabinetsLayoutPath(MRActiveRoom.BoundRoomId);
            if (!string.IsNullOrEmpty(roomPath))
                return roomPath;
        }

        return CabinetsLayoutPath;
    }

    public static string ResolveObjectsLayoutPath()
    {
        EnsureFolders();
        MigrateLegacyLayoutIfNeeded(
            Path.Combine(ConfigManager.CabinetsDB, LegacyObjectsLayoutFileName),
            ObjectsLayoutPath);

        if (MRActiveRoom.HasBoundRoom)
        {
            EnsureRoomLayouts(MRActiveRoom.BoundRoomId);
            string roomPath = GetRoomObjectsLayoutPath(MRActiveRoom.BoundRoomId);
            if (!string.IsNullOrEmpty(roomPath))
                return roomPath;
        }

        return ObjectsLayoutPath;
    }

    /// <summary>
    /// Deletes every <c>.yaml</c>/<c>.yml</c> under <c>MR/</c> plus legacy layout files in CabinetsDB.
    /// Re-seeds default example packages afterward. Returns how many files were removed.
    /// </summary>
    public static int DeleteAllYamlFiles()
    {
        EnsureFolders();
        int deleted = 0;

        if (Directory.Exists(MrDir))
        {
            try
            {
                foreach (string path in Directory.GetFiles(MrDir, "*.yaml", SearchOption.AllDirectories))
                    deleted += TryDeleteFile(path);
                foreach (string path in Directory.GetFiles(MrDir, "*.yml", SearchOption.AllDirectories))
                    deleted += TryDeleteFile(path);
            }
            catch (System.Exception e)
            {
                ConfigManager.WriteConsoleException($"[MRPaths] failed scanning YAML under {MrDir}", e);
            }
        }

        deleted += TryDeleteFile(Path.Combine(ConfigManager.CabinetsDB, LegacyCabinetsLayoutFileName));
        deleted += TryDeleteFile(Path.Combine(ConfigManager.CabinetsDB, LegacyObjectsLayoutFileName));
        deleted += TryDeleteFile(LegacyGlobalMigratedMarkerPath);

        MRActiveRoom.Clear();

        // Allow EnsureFolders seeds to run again after wipe.
        foldersEnsured = false;
        EnsureFolders();

        ConfigManager.WriteConsole($"[MRPaths] deleted {deleted} YAML file(s) under MR/");
        return deleted;
    }

    static int TryDeleteFile(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            return 0;

        try
        {
            File.Delete(path);
            ConfigManager.WriteConsole($"[MRPaths] deleted YAML {path}");
            return 1;
        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleException($"[MRPaths] failed to delete {path}", e);
            return 0;
        }
    }

    public static string GetCustomObjectPackageDir(string packageName)
    {
        if (string.IsNullOrEmpty(packageName))
            return null;
        return Path.Combine(CustomObjectsDir, packageName);
    }

    public static string GetRoomSkinPackageDir(string packageName)
    {
        if (string.IsNullOrEmpty(packageName))
            return null;
        return Path.Combine(RoomSkinsDir, packageName);
    }

    static void MigrateLegacyLayoutIfNeeded(string legacyPath, string newPath)
    {
        if (File.Exists(newPath) || !File.Exists(legacyPath))
            return;

        try
        {
            File.Copy(legacyPath, newPath, overwrite: false);
            ConfigManager.WriteConsole($"[MRPaths] migrated layout {legacyPath} -> {newPath}");
        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleException($"[MRPaths] migrate failed {legacyPath}", e);
        }
    }
}
