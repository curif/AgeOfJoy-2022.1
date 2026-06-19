/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// TestCustomObjects scene: spawn custom object packages and MR/Posters images for local testing.
/// Editor paths: %UserProfile%/cabs/MR/Custom Objects/ and %UserProfile%/cabs/MR/Posters/
/// </summary>
public class MRTestCustomObjectSpawn : MonoBehaviour
{
    public const string TestSceneName = "TestCustomObjects";

    const string LogPrefix = "[MRTestCustomObjectSpawn]";
    const string SpawnRootName = "SpawnedCustomObjects";

    [SerializeField] float startDelaySeconds = 0.25f;
    [SerializeField] float spawnDistanceMeters = 4f;
    [SerializeField] float spawnHeightMeters;
    [SerializeField] float columnSpacingMeters = 2f;
    [SerializeField] float rowSpacingMeters = 2.5f;
    [SerializeField] int columns = 4;
    [SerializeField] bool createReferenceFloor = true;

    Transform spawnRoot;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void InstallForTestScene()
    {
        Scene active = SceneManager.GetActiveScene();
        if (active.name != TestSceneName)
            return;

        if (Object.FindObjectOfType<MRTestCustomObjectSpawn>() != null)
            return;

        var host = new GameObject("TestCustomObjectSpawner");
        host.AddComponent<MRTestCustomObjectSpawn>();
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name != TestSceneName)
            return;

        StartCoroutine(SpawnAllPackagesWhenReady());
    }

    IEnumerator SpawnAllPackagesWhenReady()
    {
        yield return new WaitForSeconds(startDelaySeconds);

        MRPaths.EnsureFolders();
        MRCustomObjectCatalog.RefreshCache();
        IReadOnlyList<string> packages = MRCustomObjectCatalog.GetPackageNames();

        if (packages.Count == 0)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} no custom object packages in {MRPaths.CustomObjectsDir}");
            yield break;
        }

        EnsureSpawnRoot();
        if (createReferenceFloor)
            EnsureReferenceFloor();

        ResolveGridFrame(out Vector3 anchor, out Vector3 rowDir, out Vector3 columnDir);
        int columnCount = Mathf.Max(1, columns);

        ConfigManager.WriteConsole(
            $"{LogPrefix} spawning {packages.Count} package(s) from {MRPaths.CustomObjectsDir}");

        int spawned = 0;
        for (int i = 0; i < packages.Count; i++)
        {
            string packageName = packages[i];
            int col = i % columnCount;
            int row = i / columnCount;
            float colOffset = (col - (columnCount - 1) * 0.5f) * columnSpacingMeters;

            Vector3 worldPos = anchor + columnDir * colOffset + rowDir * (row * rowSpacingMeters);
            Quaternion worldRot = Quaternion.LookRotation(-columnDir, Vector3.up);

            var result = new CustomObjectSpawnResult();
            yield return MRCustomObjectLoader.InstantiateAtWorldPose(
                packageName, worldPos, worldRot, result);

            if (!result.Success || result.Root == null)
            {
                ConfigManager.WriteConsoleWarning($"{LogPrefix} spawn failed for {packageName}");
                continue;
            }

            result.Root.transform.SetParent(spawnRoot, true);
            spawned++;
            ConfigManager.WriteConsole(
                $"{LogPrefix} [{spawned}/{packages.Count}] {packageName} at {worldPos}");
        }

        ConfigManager.WriteConsole($"{LogPrefix} done — spawned {spawned}/{packages.Count}");
    }

    void EnsureSpawnRoot()
    {
        GameObject existing = GameObject.Find(SpawnRootName);
        if (existing != null)
        {
            spawnRoot = existing.transform;
            return;
        }

        var rootGo = new GameObject(SpawnRootName);
        spawnRoot = rootGo.transform;
        spawnRoot.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    void EnsureReferenceFloor()
    {
        if (GameObject.Find("TestCustomObjects_Floor") != null)
            return;

        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "TestCustomObjects_Floor";
        floor.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        floor.transform.localScale = new Vector3(4f, 1f, 4f);
    }

    void ResolveGridFrame(out Vector3 anchor, out Vector3 rowDir, out Vector3 columnDir)
    {
        Transform view = ResolveViewTransform();
        Vector3 forward = view != null ? view.forward : Vector3.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;
        forward.Normalize();

        columnDir = Vector3.Cross(Vector3.up, forward).normalized;
        if (columnDir.sqrMagnitude < 0.001f)
            columnDir = Vector3.right;

        rowDir = -forward;
        Vector3 origin = view != null ? view.position : Vector3.zero;
        anchor = origin + forward * spawnDistanceMeters;
        anchor.y = spawnHeightMeters;
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
}
