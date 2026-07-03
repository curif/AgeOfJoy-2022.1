/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Magazine test scene: spawn the Resources prefab at the disabled scene marker pose (production flow).
/// Editor issue path: %UserProfile%/cabs/MR/Magazines/&lt;issue&gt;/magazine.yaml
/// </summary>
public class MRTestMagazineSpawn : MonoBehaviour
{
    public const string TestSceneName = "Magazine";

    const string LogPrefix = "[MRTestMagazineSpawn]";
    const string SpawnRootName = "SpawnedMagazine";
    const string PlacementMarkerName = "Magazine";

    [SerializeField] string issueName = "Dezembro_1997_45";
    [SerializeField] float startDelaySeconds = 0.25f;
    [SerializeField] float fallbackSpawnDistanceMeters = 4f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void InstallForTestScene()
    {
        Scene active = SceneManager.GetActiveScene();
        if (active.name != TestSceneName)
            return;

        if (Object.FindObjectOfType<MRTestMagazineSpawn>() != null)
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

    IEnumerator SpawnMagazineWhenReady()
    {
        yield return new WaitForSeconds(startDelaySeconds);

        MRPaths.EnsureFolders();
        MRMagazineCatalog.RefreshCache();

        string issue = string.IsNullOrWhiteSpace(issueName) ? null : issueName.Trim();
        if (string.IsNullOrEmpty(issue) || !MRMagazineCatalog.IssueHasYaml(issue))
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} issue missing {MRPaths.MagazineYamlFileName}: {issue} in {MRPaths.MagazinesDir}");
            yield break;
        }

        GameObject prefab = MREnvironmentCatalog.LoadMagazinePrefab();
        if (prefab == null)
        {
            ConfigManager.WriteConsoleError(
                $"{LogPrefix} prefab missing: Resources/{MREnvironmentCatalog.ResourcesPath}/{MREnvironmentCatalog.MagazinePrefabName}");
            yield break;
        }

        ResolveSpawnPose(out Vector3 worldPos, out Quaternion worldRot);

        GameObject spawnedRoot = Instantiate(prefab, worldPos, worldRot);
        spawnedRoot.name = $"{MREnvironmentCatalog.MagazinePrefabName}_{issue}";
        spawnedRoot.transform.SetParent(EnsureSpawnRoot(), true);

        Magazine magazine = spawnedRoot.GetComponent<Magazine>();
        magazine?.ConfigureIssue(issue);

        ConfigManager.WriteConsole(
            $"{LogPrefix} spawned {spawnedRoot.name} from Resources at {worldPos} (issue {issue})");
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

    void ResolveSpawnPose(out Vector3 worldPos, out Quaternion worldRot)
    {
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

        PlayerController pc = Object.FindObjectOfType<PlayerController>();
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
}
