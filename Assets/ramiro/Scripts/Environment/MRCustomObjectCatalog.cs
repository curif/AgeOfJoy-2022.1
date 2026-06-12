/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>Scans MR/Custom Objects/*/object.yaml packages.</summary>
public static class MRCustomObjectCatalog
{
    static List<string> cachedPackageNames;

    public static IReadOnlyList<string> GetPackageNames()
    {
        if (cachedPackageNames == null)
            RefreshCache();
        return cachedPackageNames;
    }

    public static void RefreshCache()
    {
        cachedPackageNames = new List<string>();
        MRPaths.EnsureFolders();

        if (!Directory.Exists(MRPaths.CustomObjectsDir))
            return;

        foreach (string dir in Directory.GetDirectories(MRPaths.CustomObjectsDir).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            string packageName = Path.GetFileName(dir);
            if (string.IsNullOrEmpty(packageName) || packageName.StartsWith("."))
                continue;

            string yamlPath = Path.Combine(dir, MRPaths.CustomObjectYamlFileName);
            if (!File.Exists(yamlPath))
                continue;

            cachedPackageNames.Add(packageName);
        }

        ConfigManager.WriteConsole($"[MRCustomObjectCatalog] {cachedPackageNames.Count} custom package(s) in {MRPaths.CustomObjectsDir}");
    }

    public static string GetDisplayLabel(string packageName)
    {
        if (!MRCustomObjectDefinition.TryLoad(packageName, out MRCustomObjectDefinition definition))
            return packageName;

        return definition.GetDisplayName();
    }
}
