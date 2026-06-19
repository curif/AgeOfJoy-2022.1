/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// TestCustomObjects scene: spawn every image under {BaseDir}/MR/Posters/ on a reference wall grid.
/// Editor: %UserProfile%/cabs/MR/Posters/
/// </summary>
public class MRTestPosterSpawn : MonoBehaviour
{
    public const string TestSceneName = MRTestCustomObjectSpawn.TestSceneName;

    const string LogPrefix = "[MRTestPosterSpawn]";
    const string SpawnRootName = "SpawnedPosters";
    const string WallName = "TestCustomObjects_PosterWall";

    [SerializeField] float startDelaySeconds = 0.35f;
    [SerializeField] float wallDistanceMeters = 4f;
    [SerializeField] float wallHeightMeters = 1.55f;
    [SerializeField] float columnSpacingMeters = 0.62f;
    [SerializeField] float rowSpacingMeters = 0.82f;
    [SerializeField] int columns = 4;
    [SerializeField] bool createReferenceWall = true;
    [SerializeField] float surfaceOffsetMeters = 0.012f;

    Transform spawnRoot;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void InstallForTestScene()
    {
        Scene active = SceneManager.GetActiveScene();
        if (active.name != TestSceneName)
            return;

        if (Object.FindObjectOfType<MRTestPosterSpawn>() != null)
            return;

        var host = new GameObject("TestPosterSpawner");
        host.AddComponent<MRTestPosterSpawn>();
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name != TestSceneName)
            return;

        StartCoroutine(SpawnAllPostersWhenReady());
    }

    IEnumerator SpawnAllPostersWhenReady()
    {
        yield return new WaitForSeconds(startDelaySeconds);

        MRPaths.EnsureFolders();
        MRPostersCatalog.RefreshCache();
        IReadOnlyList<string> posterPaths = MRPostersCatalog.GetRelativePaths();

        if (posterPaths.Count == 0)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} no poster images in {MRPaths.PostersDir} (.png/.jpg recommended)");
            yield break;
        }

        EnsureSpawnRoot();
        ResolveWallFrame(out Vector3 wallCenter, out Quaternion wallRotation, out Vector3 wallRight, out Vector3 wallUp);
        if (createReferenceWall)
            EnsureReferenceWall(wallCenter, wallRotation);

        int columnCount = Mathf.Max(1, columns);
        ConfigManager.WriteConsole(
            $"{LogPrefix} spawning {posterPaths.Count} poster(s) from {MRPaths.PostersDir}");

        int spawned = 0;
        for (int i = 0; i < posterPaths.Count; i++)
        {
            string relativePath = posterPaths[i];
            int col = i % columnCount;
            int row = i / columnCount;
            float colOffset = (col - (columnCount - 1) * 0.5f) * columnSpacingMeters;
            float rowOffset = row * rowSpacingMeters;

            Vector3 worldPos = wallCenter
                + wallRight * colOffset
                + wallUp * rowOffset
                + wallRotation * Vector3.forward * surfaceOffsetMeters;
            Quaternion worldRot = wallRotation;

            if (!MRPosterFactory.TryInstantiate(relativePath, worldPos, worldRot, out GameObject root))
            {
                ConfigManager.WriteConsoleWarning($"{LogPrefix} spawn failed for {relativePath}");
                continue;
            }

            root.transform.SetParent(spawnRoot, true);
            spawned++;
            ConfigManager.WriteConsole(
                $"{LogPrefix} [{spawned}/{posterPaths.Count}] {relativePath} at {worldPos}");
            yield return null;
        }

        ConfigManager.WriteConsole($"{LogPrefix} done — spawned {spawned}/{posterPaths.Count}");
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

    void EnsureReferenceWall(Vector3 center, Quaternion rotation)
    {
        if (GameObject.Find(WallName) != null)
            return;

        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Quad);
        wall.name = WallName;
        Object.Destroy(wall.GetComponent<Collider>());
        wall.transform.SetPositionAndRotation(center, rotation);
        wall.transform.localScale = new Vector3(6f, 3.5f, 1f);

        Renderer renderer = wall.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.82f, 0.8f, 0.76f, 1f);
            renderer.sharedMaterial = material;
        }
    }

    void ResolveWallFrame(
        out Vector3 wallCenter,
        out Quaternion wallRotation,
        out Vector3 wallRight,
        out Vector3 wallUp)
    {
        Transform view = ResolveViewTransform();
        Vector3 forward = view != null ? view.forward : Vector3.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;
        forward.Normalize();

        Vector3 intoRoom = -forward;
        wallRotation = Quaternion.LookRotation(intoRoom, Vector3.up);
        wallRight = wallRotation * Vector3.right;
        wallUp = Vector3.up;

        Vector3 origin = view != null ? view.position : Vector3.zero;
        wallCenter = origin + forward * wallDistanceMeters;
        wallCenter.y = wallHeightMeters;
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
