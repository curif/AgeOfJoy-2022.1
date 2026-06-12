/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Siccity.GLTFUtility;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Loads custom object GLB packages from MR/Custom Objects.</summary>
public static class MRCustomObjectLoader
{
    const string LogPrefix = "[MRCustomObjectLoader]";

    public static IEnumerator InstantiateAtWorldPose(
        string packageName,
        Vector3 worldPosition,
        Quaternion worldRotation,
        CustomObjectSpawnResult result)
    {
        result.Root = null;
        result.Success = false;

        if (!MRCustomObjectDefinition.TryLoad(packageName, out MRCustomObjectDefinition definition))
            yield break;

        string modelPath = Path.Combine(definition.PackageDir, definition.GetModelFileName());
        GameObject modelRoot = null;
        yield return LoadGlbRoot(modelPath, loaded => modelRoot = loaded);

        if (modelRoot == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} GLB load failed: {modelPath}");
            MRDebugLog.LogError($"Custom object '{packageName}': GLB load failed ({definition.GetModelFileName()})");
            yield break;
        }

        GameObject root = new GameObject(packageName);
        root.transform.SetPositionAndRotation(worldPosition, worldRotation);
        modelRoot.transform.SetParent(root.transform, worldPositionStays: false);
        modelRoot.transform.localPosition = definition.GetModelLocalOffset();
        modelRoot.transform.localRotation = Quaternion.Euler(definition.GetModelLocalEuler());

        float scale = definition.GetModelScale();
        root.transform.localScale = Vector3.one * scale;

        ApplyPlacementProfile(root, definition);
        EnsureCollider(root, definition);
        TryAttachAudio(root, definition);
        MRCustomObjectComponentApplier.Apply(root, definition);

        result.Root = root;
        result.Success = true;
        ConfigManager.WriteConsole($"{LogPrefix} spawned custom package {packageName}");
    }

    static IEnumerator LoadGlbRoot(string modelFilePath, Action<GameObject> onLoaded)
    {
        GameObject loaded = null;
        ImportSettings importSettings = new ImportSettings();
        importSettings.shaderOverrides.CacheDefaultShaders();

        TaskCompletionSource<GameObject> tcs = new TaskCompletionSource<GameObject>();
        try
        {
            Importer.LoadFromFileAsync(modelFilePath, importSettings, (go, _) => tcs.SetResult(go), null);
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"{LogPrefix} LoadFromFileAsync {modelFilePath}", e);
            MRDebugLog.LogError($"Custom object GLB load exception: {Path.GetFileName(modelFilePath)} ({e.Message})");
            onLoaded?.Invoke(null);
            yield break;
        }

        yield return new WaitUntil(() => tcs.Task.IsCompleted);

        if (tcs.Task.IsFaulted)
        {
            Exception baseEx = tcs.Task.Exception?.GetBaseException();
            ConfigManager.WriteConsoleException(
                $"{LogPrefix} LoadFromFileAsync {modelFilePath}",
                baseEx);
            MRDebugLog.LogError(
                $"Custom object GLB load failed: {Path.GetFileName(modelFilePath)} ({baseEx?.Message ?? "unknown"})");
            onLoaded?.Invoke(null);
            yield break;
        }

        loaded = tcs.Task.Result;
        if (loaded != null)
            loaded.SetActive(true);

        if (loaded != null)
        {
            foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (go.name == "Root"
                    && go != loaded
                    && go.transform.childCount == 0
                    && go.GetComponents<Component>().Length == 1)
                {
                    UnityEngine.Object.Destroy(go);
                }
            }
        }

        onLoaded?.Invoke(loaded);
    }

    static void ApplyPlacementProfile(GameObject root, MRCustomObjectDefinition definition)
    {
        MRPlacementProfile profile = root.GetComponent<MRPlacementProfile>();
        if (profile == null)
            profile = root.AddComponent<MRPlacementProfile>();

        profile.displayName = definition.GetDisplayName();
        profile.surfaceType = definition.GetSurfaceType();
        profile.facingAxis = definition.GetFacingAxis();
        profile.allowStickRotation = definition.GetAllowStickRotation();
        profile.stickRotationAxis = definition.GetStickRotationAxis();
        profile.stickRotationSpeed = definition.GetStickRotationSpeed();
    }

    static void EnsureCollider(GameObject root, MRCustomObjectDefinition definition)
    {
        MRCustomObjectCollisionMode mode = definition.GetCollisionMode();
        if (mode == MRCustomObjectCollisionMode.None)
            return;

        if (root.GetComponentInChildren<Collider>(true) != null)
            return;

        if (mode == MRCustomObjectCollisionMode.Box)
        {
            AddBoxColliderFromBounds(root);
            return;
        }

        MeshFilter meshFilter = root.GetComponentInChildren<MeshFilter>(true);
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            MeshCollider meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = meshFilter.sharedMesh;
            meshCollider.convex = definition.GetCollisionConvex();
            return;
        }

        AddBoxColliderFromBounds(root);
    }

    static void AddBoxColliderFromBounds(GameObject root)
    {
        Renderer renderer = root.GetComponentInChildren<Renderer>(true);
        if (renderer == null)
            return;

        BoxCollider box = renderer.gameObject.AddComponent<BoxCollider>();
        box.center = renderer.bounds.center - renderer.transform.position;
        box.size = renderer.bounds.size;
    }

    static void TryAttachAudio(GameObject root, MRCustomObjectDefinition definition)
    {
        if (definition.Audio == null || string.IsNullOrEmpty(definition.Audio.File))
            return;

        string audioPath = Path.Combine(definition.PackageDir, definition.Audio.File);
        if (!File.Exists(audioPath))
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} audio missing: {audioPath}");
            return;
        }

        AudioSource audioSource = root.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = root.AddComponent<AudioSource>();

        audioSource.loop = definition.Audio.Loop;
        audioSource.volume = definition.Audio.Volume;
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;
        if (definition.Audio.Distance != null)
        {
            audioSource.minDistance = definition.Audio.Distance.Min;
            audioSource.maxDistance = definition.Audio.Distance.Max;
        }

        CabinetPartAudioController audioCtrl = root.GetComponent<CabinetPartAudioController>();
        if (audioCtrl == null)
            audioCtrl = root.AddComponent<CabinetPartAudioController>();

        audioCtrl.cabPathBase = definition.PackageDir.Replace("\\", "/");
        audioCtrl.AssignAudioClip(definition.Audio.File);
        if (definition.Audio.PlayOnAwake)
            audioCtrl.PlayAudio();
    }
}

public class CustomObjectSpawnResult
{
    public bool Success;
    public GameObject Root;
}
