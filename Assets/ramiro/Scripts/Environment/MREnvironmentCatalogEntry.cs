/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections.Generic;

/// <summary>Unified catalog row for build prefabs and custom object packages.</summary>
public readonly struct MREnvironmentCatalogEntry : IEquatable<MREnvironmentCatalogEntry>
{
    public MREnvironmentObjectSource Source { get; }
    public string Key { get; }
    public string DisplayLabel { get; }
    readonly string[] groupedKeys;
    public IReadOnlyList<string> GroupedKeys => groupedKeys;

    MREnvironmentCatalogEntry(
        MREnvironmentObjectSource source,
        string key,
        string displayLabel,
        IReadOnlyList<string> groupedKeys = null)
    {
        Source = source;
        Key = key ?? string.Empty;
        DisplayLabel = string.IsNullOrEmpty(displayLabel) ? key : displayLabel;
        if (groupedKeys == null || groupedKeys.Count == 0)
        {
            this.groupedKeys = Array.Empty<string>();
            return;
        }

        this.groupedKeys = new string[groupedKeys.Count];
        for (int i = 0; i < groupedKeys.Count; i++)
            this.groupedKeys[i] = groupedKeys[i];
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

    public static MREnvironmentCatalogEntry FromMagazine(string issueName, string displayLabel) =>
        new MREnvironmentCatalogEntry(MREnvironmentObjectSource.Magazine, issueName, displayLabel);

    public static MREnvironmentCatalogEntry FromBookshelf(
        string shelfKey,
        string displayLabel,
        IReadOnlyList<string> issueNames) =>
        new MREnvironmentCatalogEntry(MREnvironmentObjectSource.Bookshelf, shelfKey, displayLabel, issueNames);

    public string MenuPrefix => Source switch
    {
        MREnvironmentObjectSource.Custom => "[C] ",
        MREnvironmentObjectSource.RoomSkin => "[R] ",
        MREnvironmentObjectSource.Magazine => "[M] ",
        MREnvironmentObjectSource.Bookshelf => "[B] ",
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

        if (Source == MREnvironmentObjectSource.Magazine)
            return placement.IsMagazineSource
                && string.Equals(placement.MagazineIssueName, Key, StringComparison.OrdinalIgnoreCase);

        if (Source == MREnvironmentObjectSource.Bookshelf)
            return placement.IsBookshelfSource
                && SequenceEqualsIgnoreCase(placement.BookshelfIssueNames, groupedKeys);

        return string.Equals(placement.PrefabName, Key, StringComparison.OrdinalIgnoreCase);
    }

    public bool Equals(MREnvironmentCatalogEntry other)
    {
        if (Source != other.Source)
            return false;

        if (Source == MREnvironmentObjectSource.Bookshelf)
            return SequenceEqualsIgnoreCase(groupedKeys, other.groupedKeys);

        return string.Equals(Key, other.Key, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object obj) => obj is MREnvironmentCatalogEntry other && Equals(other);

    public override int GetHashCode()
    {
        if (Source != MREnvironmentObjectSource.Bookshelf)
            return HashCode.Combine(Source, Key?.ToLowerInvariant());

        int hash = (int)Source;
        for (int i = 0; i < groupedKeys.Length; i++)
            hash = HashCode.Combine(hash, groupedKeys[i]?.ToLowerInvariant());
        return hash;
    }

    public override string ToString() =>
        Source == MREnvironmentObjectSource.Bookshelf
            ? $"{Source}:{DisplayLabel}"
            : $"{Source}:{Key}";

    static bool SequenceEqualsIgnoreCase(IReadOnlyList<string> a, IReadOnlyList<string> b)
    {
        int countA = a?.Count ?? 0;
        int countB = b?.Count ?? 0;
        if (countA != countB)
            return false;

        for (int i = 0; i < countA; i++)
        {
            if (!string.Equals(a[i], b[i], StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

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
    RoomSkin = 4,
    Magazine = 5,
    Bookshelf = 6
}
