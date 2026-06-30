/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>Magazine page images from {BaseDir}/MR/Magazines/{issue}/ (ordered by filename).</summary>
public static class MRMagazineCatalog
{
    static Dictionary<string, List<string>> cachedIssuePages;

    public static IReadOnlyList<string> GetIssueNames()
    {
        if (cachedIssuePages == null)
            RefreshCache();

        return cachedIssuePages.Keys.OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public static void RefreshCache()
    {
        cachedIssuePages = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        MRPaths.EnsureFolders();

        if (!Directory.Exists(MRPaths.MagazinesDir))
            return;

        foreach (string issueDir in Directory.GetDirectories(MRPaths.MagazinesDir)
                     .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            string issueName = Path.GetFileName(issueDir);
            if (string.IsNullOrEmpty(issueName))
                continue;

            var pages = new List<string>();
            foreach (string file in Directory.EnumerateFiles(issueDir, "*.*", SearchOption.TopDirectoryOnly))
            {
                if (!MRPostersCatalog.IsSupportedImageFile(file))
                    continue;

                pages.Add(Path.GetFileName(file));
            }

            pages.Sort(ComparePageFileNames);

            if (pages.Count > 0)
                cachedIssuePages[issueName] = pages;
        }

        ConfigManager.WriteConsole(
            $"[MRMagazineCatalog] {cachedIssuePages.Count} issue(s) in {MRPaths.MagazinesDir}");
    }

    public static int GetPageCount(string issueName)
    {
        if (cachedIssuePages == null)
            RefreshCache();

        if (string.IsNullOrEmpty(issueName) || !cachedIssuePages.TryGetValue(issueName, out List<string> pages))
            return 0;

        return pages.Count;
    }

    public static bool IssueExists(string issueName)
    {
        if (cachedIssuePages == null)
            RefreshCache();

        return !string.IsNullOrEmpty(issueName) && cachedIssuePages.ContainsKey(issueName);
    }

    public static string GetPageFileName(string issueName, int pageIndex)
    {
        if (cachedIssuePages == null)
            RefreshCache();

        if (string.IsNullOrEmpty(issueName) || !cachedIssuePages.TryGetValue(issueName, out List<string> pages))
            return null;

        if (pageIndex < 0 || pageIndex >= pages.Count)
            return null;

        return pages[pageIndex];
    }

    public static Texture2D LoadPageTexture(string issueName, int pageIndex)
    {
        string pageFileName = GetPageFileName(issueName, pageIndex);
        if (string.IsNullOrEmpty(pageFileName))
            return null;

        string fullPath = MRPaths.ResolveMagazinePagePath(issueName, pageFileName);
        if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath))
            return null;

        try
        {
            byte[] bytes = File.ReadAllBytes(fullPath);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(bytes))
            {
                UnityEngine.Object.Destroy(texture);
                ConfigManager.WriteConsoleError(
                    $"[MRMagazineCatalog] LoadImage failed for {issueName}/{pageFileName} " +
                    "(use .png or .jpg on Quest — .tif is often unsupported).");
                return null;
            }

            texture.name = Path.GetFileNameWithoutExtension(pageFileName);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            return texture;
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException(
                $"[MRMagazineCatalog] load failed: {issueName}/{pageFileName}", e);
            return null;
        }
    }

    /// <summary>Legacy — prefer <see cref="GetSpreadInteriorPageNumbers"/>.</summary>
    public static void GetSpreadPageNumbers(int spreadIndex, int pageCount, out int leftPageNumber, out int rightPageNumber)
    {
        GetSpreadInteriorPageNumbers(spreadIndex, pageCount, out leftPageNumber, out rightPageNumber);
    }

    /// <summary>Interior faces (0 = hidden). Spread 0: 2|3, then 4|5, … last: N-1|—.</summary>
    public static void GetSpreadInteriorPageNumbers(int spreadIndex, int pageCount, out int readingLeft, out int readingRight)
    {
        readingLeft = 0;
        readingRight = 0;
        if (pageCount <= 0 || spreadIndex < 0)
            return;

        int lastSpreadIndex = GetLastSpreadIndex(pageCount);
        if (spreadIndex >= lastSpreadIndex)
        {
            readingLeft = pageCount - 1;
            return;
        }

        readingLeft = 2 + spreadIndex * 2;
        readingRight = 3 + spreadIndex * 2;
    }

    public static int GetLastSpreadIndex(int pageCount)
    {
        if (pageCount < 4)
            return 0;

        return (pageCount - 2) / 2;
    }

    public static int GetSpreadCount(int pageCount)
    {
        if (pageCount <= 0)
            return 0;

        return GetLastSpreadIndex(pageCount) + 1;
    }

    static int ComparePageFileNames(string a, string b)
    {
        int pageA = TryParseLeadingPageNumber(a);
        int pageB = TryParseLeadingPageNumber(b);
        if (pageA >= 0 && pageB >= 0 && pageA != pageB)
            return pageA.CompareTo(pageB);

        return StringComparer.OrdinalIgnoreCase.Compare(a, b);
    }

    /// <summary>Leading digits in the filename (e.g. 12 from 12.png or 12-cover.jpg).</summary>
    public static int TryParseLeadingPageNumber(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return -1;

        string name = Path.GetFileNameWithoutExtension(fileName);
        if (string.IsNullOrEmpty(name))
            return -1;

        int digitLength = 0;
        while (digitLength < name.Length && char.IsDigit(name[digitLength]))
            digitLength++;

        if (digitLength == 0)
            return -1;

        return int.TryParse(name.Substring(0, digitLength), out int pageNumber) ? pageNumber : -1;
    }
}
