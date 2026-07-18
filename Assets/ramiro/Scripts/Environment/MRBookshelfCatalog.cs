/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

/// <summary>
/// Builds the single Bookshelf menu entry from MR/Magazines issues activated
/// in MR/Magazines/magazines.yaml (issue name: true/false; missing file or
/// missing issue counts as active). Only one bookshelf can exist in the scene,
/// so the catalog never offers more than one entry.
/// </summary>
public static class MRBookshelfCatalog
{
    public static List<MREnvironmentCatalogEntry> GetCatalogEntries()
    {
        MRMagazineCatalog.RefreshCache();

        List<string> issues = MRBookshelfFactory.CollectValidIssues(GetActiveIssueNames());
        var entries = new List<MREnvironmentCatalogEntry>();
        if (issues.Count == 0)
            return entries;

        int count = Math.Min(MRBookshelfFactory.MaxShelfMagazineCount, issues.Count);
        List<string> shelfIssues = issues.GetRange(0, count);

        entries.Add(MREnvironmentCatalogEntry.FromBookshelf(
            "bookshelf-01",
            GetDisplayLabel(shelfIssues),
            shelfIssues));
        return entries;
    }

    /// <summary>Issue names filtered by the magazines.yaml activation flags.</summary>
    public static List<string> GetActiveIssueNames()
    {
        Dictionary<string, bool> activation = LoadActivationMap();
        var active = new List<string>();
        foreach (string issueName in MRMagazineCatalog.GetIssueNames())
        {
            if (activation.TryGetValue(issueName, out bool enabled) && !enabled)
                continue;

            active.Add(issueName);
        }

        return active;
    }

    /// <summary>Parses MR/Magazines/magazines.yaml (name: true/false). Empty on any failure.</summary>
    static Dictionary<string, bool> LoadActivationMap()
    {
        var activation = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        string yamlPath = MRPaths.MagazinesYamlPath;
        if (string.IsNullOrEmpty(yamlPath) || !File.Exists(yamlPath))
            return activation;

        try
        {
            string text = File.ReadAllText(yamlPath);
            if (string.IsNullOrWhiteSpace(text))
                return activation;

            var deserializer = new DeserializerBuilder().Build();
            var parsed = deserializer.Deserialize<Dictionary<string, bool>>(text);
            if (parsed == null)
                return activation;

            foreach (KeyValuePair<string, bool> pair in parsed)
            {
                if (!string.IsNullOrWhiteSpace(pair.Key))
                    activation[pair.Key.Trim()] = pair.Value;
            }
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException(
                $"[MRBookshelfCatalog] failed to parse {yamlPath}; showing all issues", e);
            activation.Clear();
        }

        return activation;
    }

    public static string GetDisplayLabel(IReadOnlyList<string> issueNames, int shelfIndex = -1)
    {
        int count = issueNames?.Count ?? 0;
        string shelfLabel = shelfIndex >= 0 ? $"Bookshelf {shelfIndex + 1}" : "Bookshelf";
        return $"{shelfLabel} ({count})";
    }
}
