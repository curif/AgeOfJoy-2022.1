/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Placeable MR props from Resources/ramiro/PrefabsEnvironment.
/// ConfigurationCabinetMiniMR is excluded — it spawns automatically via MRConfigurationCabinetController.
/// </summary>
public static class MREnvironmentCatalog
{
    public const string ResourcesPath = "ramiro/PrefabsEnvironment";
    public const string ExcludedPrefabName = "ConfigurationCabinetMiniMR";

    static List<string> cachedNames;

    public static IReadOnlyList<string> GetPlaceablePrefabNames()
    {
        if (cachedNames == null)
            RefreshCache();
        return cachedNames;
    }

    public static void RefreshCache()
    {
        cachedNames = new List<string>();
        GameObject[] prefabs = Resources.LoadAll<GameObject>(ResourcesPath);
        foreach (GameObject prefab in prefabs.OrderBy(entry => entry.name, StringComparer.OrdinalIgnoreCase))
        {
            if (prefab == null)
                continue;

            if (string.Equals(prefab.name, ExcludedPrefabName, StringComparison.OrdinalIgnoreCase))
                continue;

            cachedNames.Add(prefab.name);
        }

        ConfigManager.WriteConsole(
            $"[MREnvironmentCatalog] {cachedNames.Count} placeable prefab(s) in {ResourcesPath} (excluded {ExcludedPrefabName})");
    }

    public static GameObject LoadPrefab(string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName))
            return null;

        if (string.Equals(prefabName, ExcludedPrefabName, StringComparison.OrdinalIgnoreCase))
            return null;

        return Resources.Load<GameObject>($"{ResourcesPath}/{prefabName}");
    }

    public static string GetDisplayLabel(string prefabName)
    {
        GameObject prefab = LoadPrefab(prefabName);
        if (prefab == null)
            return prefabName;

        MRPlacementProfile profile = prefab.GetComponent<MRPlacementProfile>();
        return profile != null ? profile.GetDisplayName() : prefabName;
    }
}
