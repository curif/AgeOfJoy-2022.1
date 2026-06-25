/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.IO;

/// <summary>
/// On-disk MR folder layout under ConfigManager.BaseDir (Quest: .../com.curif.AgeOfJoy/MR/).
/// </summary>
public static class MRPaths
{
    public const string MrFolderName = "MR";
    public const string CustomObjectsFolderName = "Custom Objects";
    public const string PostersFolderName = "Posters";
    public const string RoomSkinsFolderName = "Room Skins";
    public const string CabinetsLayoutFileName = "cabinets-layout.yaml";
    public const string ObjectsLayoutFileName = "objects-layout.yaml";
    public const string CustomObjectYamlFileName = "object.yaml";
    public const string RoomSkinYamlFileName = "roomskin.yaml";
    /// <summary>Default image filename when <c>texture</c> is omitted in roomskin.yaml.</summary>
    public const string RoomSkinDefaultTextureFileName = "texture.png";

    public const string LegacyCabinetsLayoutFileName = "mr-layout.yaml";
    public const string LegacyObjectsLayoutFileName = "mr-environment-layout.yaml";

    public static string MrDir => Path.Combine(ConfigManager.BaseDir, MrFolderName);
    public static string CustomObjectsDir => Path.Combine(MrDir, CustomObjectsFolderName);
    public static string PostersDir => Path.Combine(MrDir, PostersFolderName);
    public static string RoomSkinsDir => Path.Combine(MrDir, RoomSkinsFolderName);
    public static string CabinetsLayoutPath => Path.Combine(MrDir, CabinetsLayoutFileName);
    public static string ObjectsLayoutPath => Path.Combine(MrDir, ObjectsLayoutFileName);

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
        ConfigManager.CreateFolder(RoomSkinsDir);
        SeedExampleCustomObjectIfNeeded();
        SeedPostersReadmeIfNeeded();
        MRRoomSkinCatalog.SeedBuiltInPackagesToDevice();
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
#
#rotator:
#  target: Blades
#  axis: y
#  speed: 180
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
        return CabinetsLayoutPath;
    }

    public static string ResolveObjectsLayoutPath()
    {
        EnsureFolders();
        MigrateLegacyLayoutIfNeeded(
            Path.Combine(ConfigManager.CabinetsDB, LegacyObjectsLayoutFileName),
            ObjectsLayoutPath);
        return ObjectsLayoutPath;
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
