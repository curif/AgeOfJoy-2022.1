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

            List<string> pages = CollectOrderedPageFileNames(issueDir);
            if (pages.Count > 0)
                cachedIssuePages[issueName] = pages;
        }

        ConfigManager.WriteConsole(
            $"[MRMagazineCatalog] {cachedIssuePages.Count} issue(s) in {MRPaths.MagazinesDir}");
    }

    /// <summary>Supported image filenames in the issue folder, sorted for cover inference.</summary>
    public static List<string> CollectOrderedPageFileNames(string issueDir)
    {
        var pages = new List<string>();
        if (string.IsNullOrEmpty(issueDir) || !Directory.Exists(issueDir))
            return pages;

        foreach (string file in Directory.EnumerateFiles(issueDir, "*.*", SearchOption.TopDirectoryOnly))
        {
            if (!MRPostersCatalog.IsSupportedImageFile(file))
                continue;

            pages.Add(Path.GetFileName(file));
        }

        pages.Sort(ComparePageFileNames);
        return pages;
    }

    /// <summary>
    /// Cover roles from sorted page order:
    /// first = front, second = insideFront, penultimate = insideBack, last = back.
    /// </summary>
    public static bool TryInferCoverFileNames(
        IReadOnlyList<string> orderedPages,
        out string front,
        out string insideFront,
        out string insideBack,
        out string back)
    {
        front = null;
        insideFront = null;
        insideBack = null;
        back = null;

        if (orderedPages == null || orderedPages.Count == 0)
            return false;

        front = orderedPages[0];
        insideFront = orderedPages.Count > 1 ? orderedPages[1] : orderedPages[0];
        insideBack = orderedPages[orderedPages.Count - 2];
        back = orderedPages[orderedPages.Count - 1];
        return true;
    }

    public static List<MREnvironmentCatalogEntry> GetCatalogEntries()
    {
        if (cachedIssuePages == null)
            RefreshCache();

        var entries = new List<MREnvironmentCatalogEntry>();
        foreach (string issueName in GetIssueNames())
        {
            entries.Add(MREnvironmentCatalogEntry.FromMagazine(
                issueName,
                GetDisplayLabel(issueName)));
        }

        return entries;
    }

    public static string GetDisplayLabel(string issueName) =>
        string.IsNullOrEmpty(issueName) ? "(magazine)" : issueName;

    public static int GetPageCount(string issueName)
    {
        if (cachedIssuePages == null)
            RefreshCache();

        if (string.IsNullOrEmpty(issueName) || !cachedIssuePages.TryGetValue(issueName, out List<string> pages))
            return 0;

        return pages.Count;
    }

    /// <summary>Highest leading page number in filenames (76 from 76.jpg), not file count.</summary>
    public static int GetLogicalPageCount(string issueName)
    {
        if (cachedIssuePages == null)
            RefreshCache();

        if (string.IsNullOrEmpty(issueName) || !cachedIssuePages.TryGetValue(issueName, out List<string> pages))
            return 0;

        int maxPageNumber = 0;
        foreach (string page in pages)
        {
            int pageNumber = TryParseLeadingPageNumber(page);
            if (pageNumber > maxPageNumber)
                maxPageNumber = pageNumber;
        }

        return maxPageNumber;
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

        return LoadPageTextureFromFile(issueName, pageFileName);
    }

    /// <summary>Load by filename or stem (FrontCover → FrontCover.png in the issue folder).</summary>
    public static Texture2D LoadPageTextureByFileName(string issueName, string fileNameOrStem, int maxTextureSize = DefaultMaxPageTextureSize)
    {
        string pageFileName = ResolvePageFileName(issueName, fileNameOrStem);
        if (string.IsNullOrEmpty(pageFileName))
            return null;

        return LoadPageTextureFromFile(issueName, pageFileName, maxTextureSize);
    }

    public static string ResolvePageFileName(string issueName, string fileNameOrStem)
    {
        if (string.IsNullOrWhiteSpace(fileNameOrStem))
            return null;

        string trimmed = fileNameOrStem.Trim();
        string fullPath = MRPaths.ResolveMagazinePagePath(issueName, trimmed);
        if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
            return trimmed;

        if (cachedIssuePages == null)
            RefreshCache();

        if (!string.IsNullOrEmpty(issueName) && cachedIssuePages.TryGetValue(issueName, out List<string> pages))
        {
            foreach (string page in pages)
            {
                if (string.Equals(page, trimmed, StringComparison.OrdinalIgnoreCase))
                    return page;

                if (string.Equals(Path.GetFileNameWithoutExtension(page), trimmed, StringComparison.OrdinalIgnoreCase))
                    return page;
            }
        }

        foreach (string ext in new[] { ".png", ".jpg", ".jpeg", ".webp", ".bmp" })
        {
            string candidate = trimmed + ext;
            fullPath = MRPaths.ResolveMagazinePagePath(issueName, candidate);
            if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                return candidate;
        }

        return null;
    }

    public const int DefaultMaxPageTextureSize = 1536;

    static Texture2D LoadPageTextureFromFile(string issueName, string pageFileName, int maxTextureSize = DefaultMaxPageTextureSize)
    {
        if (string.IsNullOrEmpty(pageFileName))
            return null;

        string fullPath = MRPaths.ResolveMagazinePagePath(issueName, pageFileName);
        if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath))
            return null;

        try
        {
            byte[] bytes = File.ReadAllBytes(fullPath);
            // Same cap/mips path as Magazine pages — bookshelf covers used to keep full-res RGBA.
            int maxPageTextureSize = Mathf.Max(64, maxTextureSize);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: true);
            if (!texture.LoadImage(bytes))
            {
                UnityEngine.Object.Destroy(texture);
                ConfigManager.WriteConsoleError(
                    $"[MRMagazineCatalog] LoadImage failed for {issueName}/{pageFileName} " +
                    "(use .png or .jpg on Quest — .tif is often unsupported).");
                return null;
            }

            int maxDim = Mathf.Max(texture.width, texture.height);
            if (maxDim > maxPageTextureSize)
            {
                float scale = (float)maxPageTextureSize / maxDim;
                int dstW = Mathf.Max(1, Mathf.RoundToInt(texture.width * scale));
                int dstH = Mathf.Max(1, Mathf.RoundToInt(texture.height * scale));
                Color32[] src = texture.GetPixels32();
                var dst = new Color32[dstW * dstH];
                for (int y = 0; y < dstH; y++)
                {
                    int srcY = y * texture.height / dstH;
                    int srcRow = srcY * texture.width;
                    int dstRow = y * dstW;
                    for (int x = 0; x < dstW; x++)
                        dst[dstRow + x] = src[srcRow + (x * texture.width / dstW)];
                }

                UnityEngine.Object.Destroy(texture);
                texture = new Texture2D(dstW, dstH, TextureFormat.RGBA32, mipChain: true);
                texture.SetPixels32(dst);
                texture.Apply(updateMipmaps: true, makeNoLongerReadable: true);
            }

            texture.name = Path.GetFileNameWithoutExtension(pageFileName);
            texture.filterMode = FilterMode.Trilinear;
            texture.anisoLevel = 4;
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

    static bool IsCoverFileName(
        string candidate,
        string frontCover,
        string insideFrontCover,
        string insideBackCover,
        string backCover)
    {
        return NameEquals(candidate, frontCover)
            || NameEquals(candidate, insideFrontCover)
            || NameEquals(candidate, insideBackCover)
            || NameEquals(candidate, backCover);
    }

    static bool NameEquals(string a, string b) =>
        !string.IsNullOrEmpty(a) && !string.IsNullOrEmpty(b)
        && string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    /// <summary>Legacy — prefer <see cref="GetSpreadInteriorPageNumbers"/>.</summary>
    public static void GetSpreadPageNumbers(int spreadIndex, int pageCount, out int leftPageNumber, out int rightPageNumber)
    {
        GetSpreadInteriorPageNumbers(spreadIndex, pageCount, out leftPageNumber, out rightPageNumber);
    }

    /// <summary>Spread 0: cover int 2 + first right page 3. Spread 1+: 4|5, … last: N-1|—.</summary>
    public static void GetSpreadInteriorPageNumbers(int spreadIndex, int pageCount, out int readingLeft, out int readingRight)
    {
        readingLeft = 0;
        readingRight = 0;
        if (pageCount <= 0 || spreadIndex < 0)
            return;

        if (spreadIndex == 0)
        {
            readingLeft = 2;
            readingRight = 3;
            return;
        }

        int lastSpreadIndex = GetLastSpreadIndex(pageCount);
        if (spreadIndex >= lastSpreadIndex)
        {
            readingLeft = pageCount - 1;
            return;
        }

        readingLeft = 4 + (spreadIndex - 1) * 2;
        readingRight = 5 + (spreadIndex - 1) * 2;
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
