/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Magazine test scene: spawn a populated Bookshelf at the scene placeholder or marker pose.
/// Editor issue path: %UserProfile%/cabs/MR/Magazines/&lt;issue&gt;/ (numbered page images)
/// </summary>
public class MRTestMagazineSpawn : MonoBehaviour
{
    public const string TestSceneName = "Magazine";

    const string LogPrefix = "[MRTestMagazineSpawn]";
    const string SpawnRootName = "SpawnedBookshelves";
    const string PlacementMarkerName = "Magazine";
    const string BookshelfObjectName = "Bookshelf";

    [SerializeField] string issueName = "Dezembro_1997_45";
    [SerializeField] float startDelaySeconds = 0.25f;
    [SerializeField] float fallbackSpawnDistanceMeters = 4f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void InstallForTestScene()
    {
        Scene active = SceneManager.GetActiveScene();
        if (active.name != TestSceneName)
            return;

        if (UnityEngine.Object.FindObjectOfType<MRTestMagazineSpawn>() != null)
            return;

        var host = new GameObject("TestMagazineSpawner");
        host.AddComponent<MRTestMagazineSpawn>();
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name != TestSceneName)
            return;

        StartCoroutine(SpawnMagazineWhenReady());
    }

    void Update()
    {
#if UNITY_EDITOR
        if (SceneManager.GetActiveScene().name != TestSceneName)
            return;

        if (MREditorInput.WasAnyPressed(KeyCode.G))
            TryGrabFirstShelfMagazineInEditor();
#endif
    }

    IEnumerator SpawnMagazineWhenReady()
    {
        yield return new WaitForSeconds(startDelaySeconds);

        MRPaths.EnsureFolders();
        MRMagazineCatalog.RefreshCache();

        List<string> issues = ResolveIssueList();
        if (issues.Count == 0)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} no magazine issues with page images in {MRPaths.MagazinesDir}");
            MRDebugLog.LogWarning($"{LogPrefix} no magazine issues with page images in {MRPaths.MagazinesDir}");
            yield break;
        }

        Transform spawnRoot = EnsureSpawnRoot();
        ResolveBookshelfSpawnPose(out Vector3 worldPos, out Quaternion worldRot);
        GameObject sceneBookshelfTemplate = FindSceneObjectIncludingInactive(BookshelfObjectName);
        SpawnBookshelf(issues, worldPos, worldRot, spawnRoot, sceneBookshelfTemplate);
    }

    List<string> ResolveIssueList()
    {
        var issues = MRMagazineCatalog.GetIssueNames().ToList();

        if (issues.Count > 0)
            return issues;

        string fallback = string.IsNullOrWhiteSpace(issueName) ? null : issueName.Trim();
        if (!string.IsNullOrEmpty(fallback) && MRMagazineCatalog.IssueExists(fallback))
            issues.Add(fallback);

        return issues;
    }

    void SpawnBookshelf(
        IReadOnlyList<string> issues,
        Vector3 worldPos,
        Quaternion worldRot,
        Transform spawnRoot,
        GameObject sceneBookshelfTemplate)
    {
        GameObject spawnedRoot = null;

        if (sceneBookshelfTemplate != null)
        {
            bool wasActive = sceneBookshelfTemplate.activeSelf;
            sceneBookshelfTemplate.SetActive(true);
            sceneBookshelfTemplate.transform.SetPositionAndRotation(worldPos, worldRot);

            if (MRBookshelfFactory.TryPopulateBookshelf(sceneBookshelfTemplate, issues))
            {
                sceneBookshelfTemplate.transform.SetParent(spawnRoot, true);
                spawnedRoot = sceneBookshelfTemplate;
            }
            else
            {
                sceneBookshelfTemplate.SetActive(wasActive);
            }
        }

        if (spawnedRoot == null
            && !MRBookshelfFactory.TryInstantiate(issues, worldPos, worldRot, out spawnedRoot))
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} failed to spawn populated bookshelf");
            MRDebugLog.LogError($"{LogPrefix} failed to spawn populated bookshelf");
            return;
        }

        spawnedRoot.transform.SetParent(spawnRoot, true);
        ConfigManager.WriteConsole(
            $"{LogPrefix} spawned {spawnedRoot.name} at {worldPos} with up to {MRBookshelfFactory.MaxShelfMagazineCount} issue(s)");
    }

    Transform EnsureSpawnRoot()
    {
        GameObject existing = GameObject.Find(SpawnRootName);
        if (existing != null)
            return existing.transform;

        var rootGo = new GameObject(SpawnRootName);
        rootGo.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        return rootGo.transform;
    }

    void ResolveBookshelfSpawnPose(out Vector3 worldPos, out Quaternion worldRot)
    {
        GameObject bookshelf = FindSceneObjectIncludingInactive(BookshelfObjectName);
        if (bookshelf != null)
        {
            worldPos = bookshelf.transform.position;
            worldRot = bookshelf.transform.rotation;
            return;
        }

        GameObject marker = FindSceneObjectIncludingInactive(PlacementMarkerName);
        if (marker != null)
        {
            worldPos = marker.transform.position;
            worldRot = marker.transform.rotation;
            return;
        }

        Transform view = ResolveViewTransform();
        Vector3 forward = view != null ? view.forward : Vector3.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;
        forward.Normalize();

        Vector3 origin = view != null ? view.position : Vector3.zero;
        worldPos = origin + forward * fallbackSpawnDistanceMeters;
        worldRot = Quaternion.LookRotation(-forward, Vector3.up);
    }

    static Transform ResolveViewTransform()
    {
        if (Camera.main != null)
            return Camera.main.transform;

        PlayerController pc = UnityEngine.Object.FindObjectOfType<PlayerController>();
        if (pc != null && pc.xrorigin != null && pc.xrorigin.Camera != null)
            return pc.xrorigin.Camera.transform;

        return null;
    }

    static GameObject FindSceneObjectIncludingInactive(string objectName)
    {
        Scene scene = SceneManager.GetActiveScene();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (string.Equals(root.name, objectName, System.StringComparison.Ordinal))
                return root;

            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (string.Equals(child.name, objectName, System.StringComparison.Ordinal))
                    return child.gameObject;
            }
        }

        return null;
    }

#if UNITY_EDITOR
    void TryGrabFirstShelfMagazineInEditor()
    {
        if (IsAnyShelfMagazineHeldInEditor())
            return;

        MRBookshelfMagazineProxy[] proxies = FindObjectsOfType<MRBookshelfMagazineProxy>(includeInactive: false)
            .Where(proxy => proxy != null)
            .OrderBy(proxy => GetMagazineSlotSortKey(proxy.transform))
            .ToArray();

        foreach (MRBookshelfMagazineProxy proxy in proxies)
        {
            if (proxy.TrySpawnHeldMagazineForEditor())
            {
                ConfigManager.WriteConsole($"{LogPrefix} editor grabbed shelf magazine {proxy.name}");
                return;
            }
        }

        ConfigManager.WriteConsoleWarning($"{LogPrefix} no shelf magazine available for editor grab");
    }

    static int GetMagazineSlotSortKey(Transform proxyTransform)
    {
        if (proxyTransform == null)
            return int.MaxValue;

        int nameOrder = GetMagazineSlotNameOrder(proxyTransform.name);
        if (nameOrder != int.MaxValue)
            return nameOrder;

        return int.MaxValue - 1;
    }

    static bool IsAnyShelfMagazineHeldInEditor()
    {
        foreach (MRSpawnedShelfMagazine shelfMagazine in FindObjectsOfType<MRSpawnedShelfMagazine>(includeInactive: false))
        {
            if (shelfMagazine == null)
                continue;

            MagazineGrab grab = shelfMagazine.GetComponent<MagazineGrab>();
            if (grab != null && grab.IsHeldNow())
                return true;
        }

        return false;
    }

    static int GetMagazineSlotNameOrder(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
            return int.MaxValue;

        if (!objectName.StartsWith("Magazine", StringComparison.OrdinalIgnoreCase))
            return int.MaxValue;

        if (objectName.Length <= "Magazine".Length)
            return 0;

        string suffix = objectName.Substring("Magazine".Length);
        return int.TryParse(suffix, out int index) ? index : int.MaxValue;
    }
#endif
}
