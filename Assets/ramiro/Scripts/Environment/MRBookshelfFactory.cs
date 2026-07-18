/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>Instantiates a bookshelf prefab populated with up to 9 magazine issues.</summary>
public static class MRBookshelfFactory
{
    const string LogPrefix = "[MRBookshelfFactory]";
    // The bookshelf model has 9 authored magazine roots (Magazine1..Magazine9).
    public const int MaxShelfMagazineCount = 9;
    public const string ShelfSlotsParentName = "PlacesBook";
    public const string ShelfSlotNamePrefix = "Place";

    public static bool TryInstantiate(
        IReadOnlyList<string> issueNames,
        Vector3 worldPos,
        Quaternion worldRot,
        out GameObject spawnedRoot)
    {
        spawnedRoot = null;

        List<string> validIssues = CollectValidIssues(issueNames, MaxShelfMagazineCount);
        if (validIssues.Count == 0)
            return false;

        GameObject bookshelfPrefab = MREnvironmentCatalog.LoadPrefab(MREnvironmentCatalog.BookshelfPrefabName);
        if (bookshelfPrefab == null)
        {
            ConfigManager.WriteConsoleError(
                $"{LogPrefix} bookshelf prefab missing: {MREnvironmentCatalog.BookshelfPrefabName}");
            MRDebugLog.LogError($"{LogPrefix} bookshelf prefab missing: {MREnvironmentCatalog.BookshelfPrefabName}");
            return false;
        }

        spawnedRoot = UnityEngine.Object.Instantiate(bookshelfPrefab, worldPos, worldRot);
        spawnedRoot.name = MREnvironmentCatalog.BookshelfPrefabName;
        spawnedRoot.transform.SetParent(null, worldPositionStays: true);

        if (TryPopulateBookshelf(spawnedRoot, validIssues))
            return true;

        UnityEngine.Object.Destroy(spawnedRoot);
        spawnedRoot = null;
        return false;
    }

    public static bool TryPopulateBookshelf(GameObject bookshelfRoot, IReadOnlyList<string> issueNames)
    {
        if (bookshelfRoot == null)
            return false;

        List<string> validIssues = CollectValidIssues(issueNames, MaxShelfMagazineCount);
        if (validIssues.Count == 0)
            return false;

        List<Transform> authoredMagazineRoots = CollectAuthoredMagazineRoots(bookshelfRoot.transform);
        List<Transform> slotAnchors = authoredMagazineRoots.Count == 0
            ? CollectShelfSlots(bookshelfRoot.transform)
            : new List<Transform>();

        if (authoredMagazineRoots.Count == 0 && slotAnchors.Count == 0)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} no authored magazines or shelf slots found on '{bookshelfRoot.name}'");
            MRDebugLog.LogWarning($"{LogPrefix} no authored magazines or shelf slots found on '{bookshelfRoot.name}'");
            return false;
        }

        GameObject magazinePrefab = MREnvironmentCatalog.LoadMagazinePrefab();
        if (magazinePrefab == null)
        {
            ConfigManager.WriteConsoleError(
                $"{LogPrefix} magazine prefab missing: {MREnvironmentCatalog.MagazinePrefabName}");
            MRDebugLog.LogError($"{LogPrefix} magazine prefab missing: {MREnvironmentCatalog.MagazinePrefabName}");
            return false;
        }

        int capacity = authoredMagazineRoots.Count > 0 ? authoredMagazineRoots.Count : slotAnchors.Count;
        int spawnCount = Math.Min(validIssues.Count, Math.Min(capacity, MaxShelfMagazineCount));
        for (int i = 0; i < spawnCount; i++)
        {
            string issueName = validIssues[i];

            bool configured = authoredMagazineRoots.Count > 0
                ? TryConfigureExistingProxyMagazineRoot(authoredMagazineRoots[i], issueName)
                : TryCreateShelfProxyMagazine(bookshelfRoot.transform, slotAnchors[i], magazinePrefab, issueName);

            if (!configured)
            {
                ConfigManager.WriteConsoleWarning($"{LogPrefix} failed to configure proxy for issue {issueName}");
                MRDebugLog.LogWarning($"{LogPrefix} failed to configure proxy for issue {issueName}");
            }
        }

        if (authoredMagazineRoots.Count > spawnCount)
            DestroyUnusedAuthoredMagazineRoots(authoredMagazineRoots, spawnCount);

        ConfigManager.WriteConsole(
            $"{LogPrefix} populated bookshelf with {spawnCount} issue(s)");
        return spawnCount > 0;
    }

    public static List<string> CollectValidIssues(IReadOnlyList<string> issueNames, int maxCount = int.MaxValue)
    {
        var valid = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (issueNames == null)
            return valid;

        for (int i = 0; i < issueNames.Count && valid.Count < maxCount; i++)
        {
            string issueName = issueNames[i];
            if (string.IsNullOrWhiteSpace(issueName))
                continue;

            issueName = issueName.Trim();
            if (!seen.Add(issueName))
                continue;

            if (!MRMagazineCatalog.IssueExists(issueName))
                continue;

            valid.Add(issueName);
        }

        return valid;
    }

    static List<Transform> CollectShelfSlots(Transform bookshelfRoot)
    {
        Transform slotsParent = FindChildByName(bookshelfRoot, ShelfSlotsParentName);
        var slots = new List<Transform>();
        if (slotsParent == null)
            return slots;

        foreach (Transform child in slotsParent)
        {
            if (child != null
                && child.name.StartsWith(ShelfSlotNamePrefix, StringComparison.Ordinal))
            {
                slots.Add(child);
            }
        }

        slots.Sort((a, b) => ShelfSlotSortKey(a.name).CompareTo(ShelfSlotSortKey(b.name)));
        return slots;
    }

    static List<Transform> CollectAuthoredMagazineRoots(Transform bookshelfRoot)
    {
        var magazineRoots = new List<Transform>();

        magazineRoots.AddRange(CollectNamedMagazineChildren(bookshelfRoot));
        if (magazineRoots.Count > 0)
            return magazineRoots;

        foreach (Magazine magazine in bookshelfRoot.GetComponentsInChildren<Magazine>(true))
        {
            if (magazine == null || magazine.transform == bookshelfRoot)
                continue;

            magazineRoots.Add(magazine.transform);
        }

        if (magazineRoots.Count == 0)
            magazineRoots.AddRange(CollectMagazineRendererCandidates(bookshelfRoot));

        magazineRoots.Sort(CompareHierarchyOrder);
        return magazineRoots;
    }

    static List<Transform> CollectNamedMagazineChildren(Transform bookshelfRoot)
    {
        var namedChildren = new List<Transform>();
        if (bookshelfRoot == null)
            return namedChildren;

        foreach (Transform child in bookshelfRoot)
        {
            if (child == null || !IsNamedMagazineChild(child.name))
                continue;

            namedChildren.Add(child);
        }

        namedChildren.Sort(CompareMagazineNameOrder);
        return namedChildren;
    }

    static bool IsNamedMagazineChild(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
            return false;

        if (!objectName.StartsWith("Magazine", StringComparison.OrdinalIgnoreCase))
            return false;

        if (objectName.Length == "Magazine".Length)
            return true;

        string suffix = objectName.Substring("Magazine".Length);
        return int.TryParse(suffix, out _);
    }

    static int CompareMagazineNameOrder(Transform a, Transform b)
    {
        int orderA = GetMagazineNameOrder(a?.name);
        int orderB = GetMagazineNameOrder(b?.name);
        if (orderA != orderB)
            return orderA.CompareTo(orderB);

        return CompareHierarchyOrder(a, b);
    }

    static int GetMagazineNameOrder(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
            return int.MaxValue;

        if (objectName.Length <= "Magazine".Length)
            return 0;

        string suffix = objectName.Substring("Magazine".Length);
        return int.TryParse(suffix, out int index) ? index : int.MaxValue;
    }

    static List<Transform> CollectMagazineRendererCandidates(Transform bookshelfRoot)
    {
        var candidates = new List<Transform>();
        if (!TryGetWorldBoundsVolume(bookshelfRoot, out float shelfVolume))
            shelfVolume = float.MaxValue;

        foreach (Renderer renderer in bookshelfRoot.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null || renderer.transform == bookshelfRoot)
                continue;

            Material[] materials = renderer.sharedMaterials;
            if (materials == null || materials.Length < 2)
                continue;

            if (!IsLikelyMagazineRenderer(renderer, shelfVolume))
                continue;

            if (!candidates.Contains(renderer.transform))
                candidates.Add(renderer.transform);
        }

        return candidates;
    }

    static bool IsLikelyMagazineRenderer(Renderer renderer, float shelfVolume)
    {
        if (renderer == null)
            return false;

        string lowerName = renderer.transform.name.ToLowerInvariant();
        if (lowerName.Contains("mag") || lowerName.Contains("book"))
            return true;

        Bounds bounds = renderer.bounds;
        float volume = Mathf.Max(bounds.size.x * bounds.size.y * bounds.size.z, 0f);
        if (volume <= 0f)
            return false;

        // Proxy magazines should be much smaller than the whole bookshelf.
        return volume < shelfVolume * 0.15f;
    }

    static bool TryGetWorldBoundsVolume(Transform root, out float volume)
    {
        volume = 0f;
        bool hasBounds = false;
        Bounds bounds = default;
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null)
                continue;

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        if (!hasBounds)
            return false;

        volume = Mathf.Max(bounds.size.x * bounds.size.y * bounds.size.z, 0f);
        return true;
    }

    static int CompareHierarchyOrder(Transform a, Transform b)
    {
        if (a == b)
            return 0;
        if (a == null)
            return 1;
        if (b == null)
            return -1;

        string pathA = a.GetHierarchyPath();
        string pathB = b.GetHierarchyPath();
        return string.Compare(pathA, pathB, StringComparison.Ordinal);
    }

    static bool TryCreateShelfProxyMagazine(
        Transform bookshelfRoot,
        Transform slot,
        GameObject magazinePrefab,
        string issueName)
    {
        if (slot == null || magazinePrefab == null || string.IsNullOrEmpty(issueName))
            return false;

        GameObject proxyRoot = UnityEngine.Object.Instantiate(magazinePrefab, slot.position, slot.rotation, slot);
        proxyRoot.name = $"{MREnvironmentCatalog.MagazinePrefabName}_{issueName}_Proxy";
        proxyRoot.transform.localScale = Vector3.one;

        Magazine magazine = proxyRoot.GetComponent<Magazine>();
        if (magazine != null)
        {
            magazine.ConfigureIssue(issueName);
            // The visible shelf copy is only a textured mesh. Prevent Start()
            // from building pages or decoding magazine content.
            magazine.enabled = false;
        }

        XRGrabInteractable proxyGrabInteractable = proxyRoot.GetComponent<XRGrabInteractable>();
        if (proxyGrabInteractable != null)
            proxyGrabInteractable.enabled = false;

        MRCustomObjectGrab customGrab = proxyRoot.GetComponent<MRCustomObjectGrab>();
        if (customGrab != null)
            customGrab.enabled = false;

        MagazineGrab magazineGrab = proxyRoot.GetComponent<MagazineGrab>();
        if (magazineGrab != null)
            magazineGrab.enabled = false;

        Rigidbody body = proxyRoot.GetComponent<Rigidbody>();
        if (body != null)
        {
            body.isKinematic = true;
            body.useGravity = false;
        }

        MRSpawnedShelfMagazine spawnedMarker = proxyRoot.GetComponent<MRSpawnedShelfMagazine>();
        if (spawnedMarker != null)
            UnityEngine.Object.Destroy(spawnedMarker);

        MRBookshelfMagazineProxy proxy = proxyRoot.GetComponent<MRBookshelfMagazineProxy>();
        if (proxy == null)
            proxy = proxyRoot.AddComponent<MRBookshelfMagazineProxy>();
        proxy.Configure(issueName);
        return true;
    }

    static bool TryConfigureExistingProxyMagazineRoot(Transform proxyRoot, string issueName)
    {
        if (proxyRoot == null || string.IsNullOrEmpty(issueName))
            return false;

        Magazine magazine = proxyRoot.GetComponent<Magazine>();
        if (magazine != null)
        {
            magazine.ConfigureIssue(issueName);
            // Authored shelf magazines are deliberately dumb visual proxies.
            magazine.enabled = false;
        }

        XRGrabInteractable proxyGrabInteractable = proxyRoot.GetComponent<XRGrabInteractable>();
        if (proxyGrabInteractable != null)
            proxyGrabInteractable.enabled = false;

        MRCustomObjectGrab customGrab = proxyRoot.GetComponent<MRCustomObjectGrab>();
        if (customGrab != null)
            customGrab.enabled = false;

        MagazineGrab magazineGrab = proxyRoot.GetComponent<MagazineGrab>();
        if (magazineGrab != null)
            magazineGrab.enabled = false;

        Rigidbody body = proxyRoot.GetComponent<Rigidbody>();
        if (body != null)
        {
            body.isKinematic = true;
            body.useGravity = false;
        }

        MRSpawnedShelfMagazine spawnedMarker = proxyRoot.GetComponent<MRSpawnedShelfMagazine>();
        if (spawnedMarker != null)
            UnityEngine.Object.Destroy(spawnedMarker);

        MRBookshelfMagazineProxy proxy = proxyRoot.GetComponent<MRBookshelfMagazineProxy>();
        if (proxy == null)
            proxy = proxyRoot.gameObject.AddComponent<MRBookshelfMagazineProxy>();
        proxy.Configure(issueName);
        return true;
    }

    static void DestroyUnusedAuthoredMagazineRoots(List<Transform> authoredMagazineRoots, int keepCount)
    {
        if (authoredMagazineRoots == null)
            return;

        for (int i = keepCount; i < authoredMagazineRoots.Count; i++)
        {
            Transform proxyRoot = authoredMagazineRoots[i];
            if (proxyRoot != null)
                UnityEngine.Object.Destroy(proxyRoot.gameObject);
        }
    }

    static Transform FindChildByName(Transform root, string childName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (string.Equals(child.name, childName, StringComparison.Ordinal))
                return child;
        }

        return null;
    }

    static int ShelfSlotSortKey(string slotName)
    {
        if (string.IsNullOrEmpty(slotName) || slotName.Length <= ShelfSlotNamePrefix.Length)
            return int.MaxValue;

        string suffix = slotName.Substring(ShelfSlotNamePrefix.Length);
        return int.TryParse(suffix, out int index) ? index : int.MaxValue;
    }
}

static class MRBookshelfFactoryTransformExtensions
{
    public static string GetHierarchyPath(this Transform transform)
    {
        if (transform == null)
            return string.Empty;

        string path = transform.name;
        Transform current = transform.parent;
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }
}
