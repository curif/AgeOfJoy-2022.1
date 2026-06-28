/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>MR light prefabs from Resources/ramiro/Lights (root must have MRPlacementProfile).</summary>
public static class MRLightsCatalog
{
    public const string ResourcesPath = "ramiro/Lights";

    static List<string> cachedPrefabNames;

    public static IReadOnlyList<string> GetPrefabNames()
    {
        if (cachedPrefabNames == null)
            RefreshCache();
        return cachedPrefabNames;
    }

    public static void RefreshCache()
    {
        cachedPrefabNames = new List<string>();
        GameObject[] prefabs = Resources.LoadAll<GameObject>(ResourcesPath);
        foreach (GameObject prefab in prefabs.OrderBy(entry => entry.name, StringComparer.OrdinalIgnoreCase))
        {
            if (prefab == null)
                continue;

            cachedPrefabNames.Add(prefab.name);
        }

        ConfigManager.WriteConsole(
            $"[MRLightsCatalog] {cachedPrefabNames.Count} light prefab(s) in {ResourcesPath}");
    }

    public static GameObject LoadPrefab(string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName))
            return null;

        return Resources.Load<GameObject>($"{ResourcesPath}/{prefabName}");
    }

    public static string GetDisplayLabel(string prefabName)
    {
        GameObject prefab = LoadPrefab(prefabName);
        if (prefab == null)
            return prefabName;

        MRPlacementProfile profile = MRPlacementProfile.Resolve(prefab);
        return profile != null ? profile.GetDisplayName() : prefabName;
    }

    public static List<MREnvironmentCatalogEntry> GetCatalogEntries()
    {
        var entries = new List<MREnvironmentCatalogEntry>();
        foreach (string prefabName in GetPrefabNames())
        {
            entries.Add(MREnvironmentCatalogEntry.FromLight(
                prefabName,
                GetDisplayLabel(prefabName)));
        }

        return entries;
    }
}
