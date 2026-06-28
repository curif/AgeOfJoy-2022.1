/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

/// <summary>
/// Parsed room skin package — fixed <see cref="MRPaths.RoomSkinYamlFileName"/> per folder.
/// </summary>
[Serializable]
public class MRRoomSkinDefinition
{
    public const int SupportedYamlVersion = 1;
    public const string KindValue = "roomSkin";

    public int Version = 1;
    public string Kind;
    public string Name;
    public string DisplayName;
    public string Texture;
    public List<string> Surfaces;

    public string PackageName { get; private set; }
    public string PackageDir { get; private set; }
    public bool FromResources { get; private set; }
    public string ResolvedTexturePath { get; private set; }

    public string GetDisplayName() =>
        !string.IsNullOrEmpty(DisplayName) ? DisplayName
        : !string.IsNullOrEmpty(Name) ? Name
        : PackageName;

    /// <summary>CRT menu label, e.g. "Blue Sky - ceiling" or "Tiles - floor, walls".</summary>
    public string GetMenuLabel()
    {
        IReadOnlyList<MRRoomSkinSurface> surfaces = GetNormalizedSurfaces();
        if (surfaces.Count == 0)
            return GetDisplayName();

        var surfaceNames = new List<string>(surfaces.Count);
        foreach (MRRoomSkinSurface surface in surfaces)
            surfaceNames.Add(FormatSurfaceYamlName(surface));

        return $"{GetDisplayName()} - {string.Join(", ", surfaceNames)}";
    }

    public bool SharesAnySurfaceWith(MRRoomSkinDefinition other)
    {
        if (other == null)
            return false;

        IReadOnlyList<MRRoomSkinSurface> otherSurfaces = other.GetNormalizedSurfaces();
        foreach (MRRoomSkinSurface surface in GetNormalizedSurfaces())
        {
            if (otherSurfaces.Contains(surface))
                return true;
        }

        return false;
    }

    public static string FormatSurfaceYamlName(MRRoomSkinSurface surface) =>
        surface switch
        {
            MRRoomSkinSurface.Floor => "floor",
            MRRoomSkinSurface.Walls => "walls",
            MRRoomSkinSurface.Ceiling => "ceiling",
            MRRoomSkinSurface.Table => "table",
            MRRoomSkinSurface.Couch => "couch",
            MRRoomSkinSurface.Bed => "bed",
            MRRoomSkinSurface.Door => "door",
            MRRoomSkinSurface.Window => "window",
            MRRoomSkinSurface.GlobalMesh => "global",
            _ => surface.ToString().ToLowerInvariant()
        };

    public bool IsValid =>
        !string.IsNullOrEmpty(ResolvedTexturePath) && GetNormalizedSurfaces().Count > 0;

    public IReadOnlyList<MRRoomSkinSurface> GetNormalizedSurfaces()
    {
        var result = new List<MRRoomSkinSurface>();
        if (Surfaces == null)
            return result;

        foreach (string entry in Surfaces)
        {
            if (TryParseSurface(entry, out MRRoomSkinSurface surface)
                && !result.Contains(surface))
                result.Add(surface);
        }

        return result;
    }

    public static bool TryParseSurface(string value, out MRRoomSkinSurface surface)
    {
        surface = default;
        if (string.IsNullOrEmpty(value))
            return false;

        switch (value.Trim().ToLowerInvariant())
        {
            case "floor":
                surface = MRRoomSkinSurface.Floor;
                return true;
            case "wall":
            case "walls":
            case "wall_face":
                surface = MRRoomSkinSurface.Walls;
                return true;
            case "ceiling":
                surface = MRRoomSkinSurface.Ceiling;
                return true;
            case "table":
                surface = MRRoomSkinSurface.Table;
                return true;
            case "couch":
            case "sofa":
                surface = MRRoomSkinSurface.Couch;
                return true;
            case "bed":
                surface = MRRoomSkinSurface.Bed;
                return true;
            case "door":
            case "door_frame":
                surface = MRRoomSkinSurface.Door;
                return true;
            case "window":
            case "window_frame":
                surface = MRRoomSkinSurface.Window;
                return true;
            case "global":
            case "global_mesh":
                surface = MRRoomSkinSurface.GlobalMesh;
                return true;
            default:
                return false;
        }
    }

    public static bool TryLoad(string packageName, out MRRoomSkinDefinition definition)
    {
        definition = null;
        if (string.IsNullOrEmpty(packageName))
            return false;

        if (TryLoadFromResources(packageName, out definition))
            return true;

        return TryLoadFromDisk(packageName, out definition);
    }

    static bool TryLoadFromResources(string packageName, out MRRoomSkinDefinition definition)
    {
        definition = null;
        TextAsset yaml = Resources.Load<TextAsset>(MRRoomSkinCatalog.GetPackageYamlResourcePath(packageName));
        if (yaml == null)
            return false;

        if (!TryParseYaml(yaml.text, packageName, null, fromResources: true, out definition))
            return false;

        definition.FromResources = true;
        return true;
    }

    static bool TryLoadFromDisk(string packageName, out MRRoomSkinDefinition definition)
    {
        definition = null;
        string packageDir = MRPaths.GetRoomSkinPackageDir(packageName);
        if (string.IsNullOrEmpty(packageDir))
            return false;

        string yamlPath = Path.Combine(packageDir, MRPaths.RoomSkinYamlFileName);
        if (!File.Exists(yamlPath))
            return false;

        try
        {
            return TryParseYaml(File.ReadAllText(yamlPath), packageName, packageDir, fromResources: false, out definition);
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[MRRoomSkinDefinition] load {yamlPath}", e);
            return false;
        }
    }

    static bool TryParseYaml(
        string yamlText,
        string packageName,
        string packageDir,
        bool fromResources,
        out MRRoomSkinDefinition definition)
    {
        definition = null;
        if (string.IsNullOrEmpty(yamlText))
            return false;

        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            definition = deserializer.Deserialize<MRRoomSkinDefinition>(yamlText);
            if (definition == null)
                return false;

            definition.PackageName = packageName;
            definition.PackageDir = packageDir;
            definition.FromResources = fromResources;

            if (definition.Version <= 0)
                definition.Version = 1;

            if (string.IsNullOrEmpty(definition.Name))
                definition.Name = packageName;

            if (!string.Equals(definition.Kind, KindValue, StringComparison.OrdinalIgnoreCase))
            {
                ConfigManager.WriteConsoleWarning(
                    $"[MRRoomSkinDefinition] {packageName}: kind must be {KindValue}");
                return false;
            }

            if (!definition.TryResolveTexturePath())
            {
                ConfigManager.WriteConsoleWarning(
                    $"[MRRoomSkinDefinition] {packageName}: texture missing or not found");
                return false;
            }

            if (definition.GetNormalizedSurfaces().Count == 0)
            {
                ConfigManager.WriteConsoleWarning(
                    $"[MRRoomSkinDefinition] {packageName}: surfaces list empty");
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[MRRoomSkinDefinition] parse {packageName}", e);
            return false;
        }
    }

    bool TryResolveTexturePath()
    {
        if (!string.IsNullOrEmpty(Texture))
        {
            ResolvedTexturePath = MRRoomSkinCatalog.NormalizeRelativePath(Texture);
            return TextureFileExists(ResolvedTexturePath);
        }

        if (!FromResources && !string.IsNullOrEmpty(PackageDir))
        {
            string defaultPath = MRPaths.RoomSkinDefaultTextureFileName;
            if (TextureFileExists(defaultPath))
            {
                ResolvedTexturePath = defaultPath;
                return true;
            }

            string soleImage = FindSoleImageInPackageDir();
            if (!string.IsNullOrEmpty(soleImage))
            {
                ResolvedTexturePath = soleImage;
                return true;
            }
        }

        return false;
    }

    bool TextureFileExists(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return false;

        if (FromResources)
        {
            string fileWithoutExt = Path.GetFileNameWithoutExtension(relativePath);
            string subPath = MRRoomSkinCatalog.NormalizeRelativePath(
                Path.GetDirectoryName(relativePath)?.Replace('\\', '/'));
            string resourcePath = string.IsNullOrEmpty(subPath)
                ? $"{MRRoomSkinCatalog.ResourcesPath}/{PackageName}/{fileWithoutExt}"
                : $"{MRRoomSkinCatalog.ResourcesPath}/{PackageName}/{subPath}/{fileWithoutExt}";
            return Resources.Load<Texture2D>(resourcePath) != null;
        }

        if (string.IsNullOrEmpty(PackageDir))
            return false;

        string fullPath = Path.GetFullPath(
            Path.Combine(PackageDir, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        string packageRoot = Path.GetFullPath(PackageDir);
        return fullPath.StartsWith(packageRoot, StringComparison.OrdinalIgnoreCase)
               && File.Exists(fullPath)
               && MRPostersCatalog.IsSupportedImageFile(fullPath);
    }

    string FindSoleImageInPackageDir()
    {
        if (string.IsNullOrEmpty(PackageDir) || !Directory.Exists(PackageDir))
            return null;

        List<string> images = Directory
            .EnumerateFiles(PackageDir, "*.*", SearchOption.TopDirectoryOnly)
            .Where(MRPostersCatalog.IsSupportedImageFile)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList();

        return images.Count == 1 ? images[0] : null;
    }
}

public enum MRRoomSkinSurface
{
    Floor,
    Walls,
    Ceiling,
    Table,
    Couch,
    Bed,
    Door,
    Window,
    GlobalMesh
}
