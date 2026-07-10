/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections.Generic;

/// <summary>Build prefabs (Resources) + custom object packages (MR/Custom Objects).</summary>
public static class MREnvironmentUnifiedCatalog
{
    public static List<MREnvironmentCatalogEntry> GetCatalogEntries()
    {
        var entries = new List<MREnvironmentCatalogEntry>();

        MREnvironmentCatalog.RefreshCache();
        foreach (string prefabName in MREnvironmentCatalog.GetPlaceablePrefabNames())
        {
            entries.Add(MREnvironmentCatalogEntry.FromBuild(
                prefabName,
                MREnvironmentCatalog.GetDisplayLabel(prefabName)));
        }

        MRCustomObjectCatalog.RefreshCache();
        foreach (string packageName in MRCustomObjectCatalog.GetPackageNames())
        {
            entries.Add(MREnvironmentCatalogEntry.FromCustom(
                packageName,
                MRCustomObjectCatalog.GetDisplayLabel(packageName)));
        }

        return entries;
    }
}
