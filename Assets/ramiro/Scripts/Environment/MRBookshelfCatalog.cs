/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections.Generic;

/// <summary>Builds Bookshelf menu entries from MR/Magazines batches of up to 8 issues.</summary>
public static class MRBookshelfCatalog
{
    public static List<MREnvironmentCatalogEntry> GetCatalogEntries()
    {
        MRMagazineCatalog.RefreshCache();

        List<string> issues = MRBookshelfFactory.CollectValidIssues(MRMagazineCatalog.GetIssueNames());
        var entries = new List<MREnvironmentCatalogEntry>();

        int shelfIndex = 0;
        for (int start = 0; start < issues.Count; start += MRBookshelfFactory.MaxShelfMagazineCount)
        {
            int count = Math.Min(MRBookshelfFactory.MaxShelfMagazineCount, issues.Count - start);
            var shelfIssues = new List<string>(count);
            for (int i = 0; i < count; i++)
                shelfIssues.Add(issues[start + i]);

            string shelfKey = $"bookshelf-{shelfIndex + 1:D2}";
            entries.Add(MREnvironmentCatalogEntry.FromBookshelf(
                shelfKey,
                GetDisplayLabel(shelfIssues, shelfIndex),
                shelfIssues));
            shelfIndex++;
        }

        return entries;
    }

    public static string GetDisplayLabel(IReadOnlyList<string> issueNames, int shelfIndex = -1)
    {
        int count = issueNames?.Count ?? 0;
        string shelfLabel = shelfIndex >= 0 ? $"Bookshelf {shelfIndex + 1}" : "Bookshelf";
        return $"{shelfLabel} ({count})";
    }
}
