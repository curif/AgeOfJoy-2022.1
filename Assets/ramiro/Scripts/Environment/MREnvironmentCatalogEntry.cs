/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;

/// <summary>Unified catalog row for build prefabs and custom object packages.</summary>
public readonly struct MREnvironmentCatalogEntry : IEquatable<MREnvironmentCatalogEntry>
{
    public MREnvironmentObjectSource Source { get; }
    public string Key { get; }
    public string DisplayLabel { get; }

    MREnvironmentCatalogEntry(MREnvironmentObjectSource source, string key, string displayLabel)
    {
        Source = source;
        Key = key ?? string.Empty;
        DisplayLabel = string.IsNullOrEmpty(displayLabel) ? key : displayLabel;
    }

    public static MREnvironmentCatalogEntry FromBuild(string prefabName, string displayLabel) =>
        new MREnvironmentCatalogEntry(MREnvironmentObjectSource.Build, prefabName, displayLabel);

    public static MREnvironmentCatalogEntry FromCustom(string packageName, string displayLabel) =>
        new MREnvironmentCatalogEntry(MREnvironmentObjectSource.Custom, packageName, displayLabel);

    public static MREnvironmentCatalogEntry FromLight(string prefabName, string displayLabel) =>
        new MREnvironmentCatalogEntry(MREnvironmentObjectSource.Light, prefabName, displayLabel);

    public static MREnvironmentCatalogEntry FromPoster(string textureRelativePath, string displayLabel) =>
        new MREnvironmentCatalogEntry(
            MREnvironmentObjectSource.Poster,
            MRPostersCatalog.NormalizeRelativePath(textureRelativePath),
            displayLabel);

    public static MREnvironmentCatalogEntry FromRoomSkin(string packageName, string displayLabel) =>
        new MREnvironmentCatalogEntry(MREnvironmentObjectSource.RoomSkin, packageName, displayLabel);

    public string MenuPrefix => Source switch
    {
        MREnvironmentObjectSource.Custom => "[C] ",
        MREnvironmentObjectSource.RoomSkin => "[R] ",
        _ => string.Empty
    };

    public string MenuLabel => MenuPrefix + Truncate(DisplayLabel, 10);

    /// <summary>All environment/light catalog rows allow unlimited instances. Game cabinets use <see cref="MRLayoutRegistry"/> (one per game).</summary>
    public bool AllowsMultipleInstances => true;

    public bool MatchesPlacement(MREnvironmentPlacement placement)
    {
        if (placement == null)
            return false;

        placement.NormalizeLegacySource();
        if (Source == MREnvironmentObjectSource.Custom)
            return string.Equals(placement.PackageName, Key, StringComparison.OrdinalIgnoreCase);

        if (Source == MREnvironmentObjectSource.Light)
            return placement.IsLightSource
                && string.Equals(placement.PrefabName, Key, StringComparison.OrdinalIgnoreCase);

        if (Source == MREnvironmentObjectSource.Poster)
            return placement.IsPosterSource
                && string.Equals(placement.TextureFile, Key, StringComparison.OrdinalIgnoreCase);

        if (Source == MREnvironmentObjectSource.RoomSkin)
            return placement.IsRoomSkinSource
                && string.Equals(placement.PackageName, Key, StringComparison.OrdinalIgnoreCase);

        return string.Equals(placement.PrefabName, Key, StringComparison.OrdinalIgnoreCase);
    }

    public bool Equals(MREnvironmentCatalogEntry other) =>
        Source == other.Source && string.Equals(Key, other.Key, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object obj) => obj is MREnvironmentCatalogEntry other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(Source, Key?.ToLowerInvariant());

    public override string ToString() => $"{Source}:{Key}";

    static string Truncate(string value, int max)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= max)
            return value;
        return value.Substring(0, max - 1) + "…";
    }
}

public enum MREnvironmentObjectSource
{
    Build = 0,
    Custom = 1,
    Light = 2,
    Poster = 3,
    RoomSkin = 4
}
