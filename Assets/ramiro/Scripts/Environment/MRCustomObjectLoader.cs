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

        yield return InstantiateDefinitionAtWorldPose(definition, worldPosition, worldRotation, result);
    }

    /// <summary>Dev/test: spawn from absolute YAML + GLB paths (Inspector drag-drop).</summary>
    public static IEnumerator InstantiateFromPaths(
        string yamlPath,
        string glbAbsolutePath,
        Vector3 worldPosition,
        Quaternion worldRotation,
        CustomObjectSpawnResult result)
    {
        result.Root = null;
        result.Success = false;

        if (!MRCustomObjectDefinition.TryLoadFromPaths(yamlPath, glbAbsolutePath, out MRCustomObjectDefinition definition))
            yield break;

        yield return InstantiateDefinitionAtWorldPose(definition, worldPosition, worldRotation, result);
    }

    static IEnumerator InstantiateDefinitionAtWorldPose(
        MRCustomObjectDefinition definition,
        Vector3 worldPosition,
        Quaternion worldRotation,
        CustomObjectSpawnResult result)
    {
        string packageName = definition.PackageName;
        string modelPath = definition.GetResolvedModelPath();
        if (string.IsNullOrEmpty(modelPath) || !File.Exists(modelPath))
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} GLB path missing for {packageName}");
            MRDebugLog.LogError($"Custom object '{packageName}': GLB path missing");
            yield break;
        }

        GameObject modelRoot = null;
        AnimationClip[] animationClips = null;
        yield return LoadGlbRoot(modelPath, (loaded, clips) =>
        {
            modelRoot = loaded;
            animationClips = clips;
        });

        if (modelRoot == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} GLB load failed: {modelPath}");
            MRDebugLog.LogError($"Custom object '{packageName}': GLB load failed ({Path.GetFileName(modelPath)})");
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
        ApplyPlacementAnchor(root, definition);
        TryAttachAudio(root, definition);

        animationClips = MRCustomObjectGlbSupport.CollectClips(modelRoot, animationClips);

        if (animationClips != null && animationClips.Length > 0)
        {
            MRCustomObjectGlbClips clipHolder = root.AddComponent<MRCustomObjectGlbClips>();
            clipHolder.Clips = animationClips;
        }

        MRCustomObjectComponentApplier.Apply(root, definition);

        if (definition.HasAnimatorComponent())
            MRCustomObjectGlbSupport.EnsureReadableWorldScale(root);

        LogSpawnDiagnostics(packageName, root, modelRoot, Path.GetFileName(modelPath));

        result.Root = root;
        result.Success = true;
        ConfigManager.WriteConsole($"{LogPrefix} spawned custom package {packageName}");
    }

    static void LogSpawnDiagnostics(string packageName, GameObject packageRoot, GameObject modelRoot, string modelFile)
    {
        int meshFilters = packageRoot.GetComponentsInChildren<MeshFilter>(true).Length;
        int skinnedMeshes = packageRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length;
        int meshRenderers = packageRoot.GetComponentsInChildren<MeshRenderer>(true).Length;

        ConfigManager.WriteConsole(
            $"{LogPrefix} {packageName}: source={modelFile} modelRoot='{modelRoot.name}' "
            + $"SkinnedMeshRenderer={skinnedMeshes} MeshFilter={meshFilters} MeshRenderer={meshRenderers}");

        if (skinnedMeshes == 0 && meshFilters == 0)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} {packageName}: GLB loaded but no visible mesh found — check export includes skinned mesh");
        }
    }

    static IEnumerator LoadGlbRoot(string modelFilePath, Action<GameObject, AnimationClip[]> onLoaded)
    {
        GameObject loaded = null;
        AnimationClip[] clips = null;
        ImportSettings importSettings = new ImportSettings();
        importSettings.shaderOverrides.CacheDefaultShaders();
        // Generic clips cannot receive SetCurve or SampleAnimation on device builds.
        importSettings.animationSettings.useLegacyClips = true;

        TaskCompletionSource<(GameObject go, AnimationClip[] animationClips)> tcs =
            new TaskCompletionSource<(GameObject, AnimationClip[])>();
        try
        {
            Importer.LoadFromFileAsync(
                modelFilePath,
                importSettings,
                (go, animationClips) => tcs.SetResult((go, animationClips)),
                null);
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"{LogPrefix} LoadFromFileAsync {modelFilePath}", e);
            MRDebugLog.LogError($"Custom object GLB load exception: {Path.GetFileName(modelFilePath)} ({e.Message})");
            onLoaded?.Invoke(null, null);
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
            onLoaded?.Invoke(null, null);
            yield break;
        }

        (loaded, clips) = tcs.Task.Result;
        if (loaded != null)
        {
            clips = MRCustomObjectGlbSupport.CollectClips(loaded, clips);
            if (clips != null && clips.Length > 0)
            {
                ConfigManager.WriteConsole(
                    $"{LogPrefix} GLB clips: {MRCustomObjectGlbSupport.DescribeClips(clips)}");
            }
        }

        if (loaded != null)
            loaded.SetActive(true);

        if (loaded != null)
        {
            MRCustomObjectMaterialFix.Apply(loaded, Path.GetFileNameWithoutExtension(modelFilePath));

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

        onLoaded?.Invoke(loaded, clips);
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

    static void ApplyPlacementAnchor(GameObject root, MRCustomObjectDefinition definition)
    {
        if (!definition.GetProvidesAnchor())
            return;

        string anchorTargetName = definition.GetAnchorTarget();
        Transform anchorTransform = string.IsNullOrEmpty(anchorTargetName)
            ? root.transform
            : MRObjectAnchorPoseResolver.FindChildByName(root.transform, anchorTargetName);

        if (anchorTransform == null)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} {definition.PackageName}: providesAnchor but anchorTarget '{anchorTargetName}' not found");
            return;
        }

        EnsureAnchorCollider(anchorTransform.gameObject, root);
        anchorTransform.gameObject.tag = MRObjectAnchorPoseResolver.AnchorTag;
        ConfigManager.WriteConsole(
            $"{LogPrefix} {definition.PackageName}: placement anchor on '{anchorTransform.name}'");
    }

    static void EnsureAnchorCollider(GameObject anchorGo, GameObject packageRoot)
    {
        if (anchorGo.GetComponent<Collider>() != null)
            return;

        Renderer renderer = anchorGo.GetComponent<Renderer>();
        if (renderer == null)
            renderer = packageRoot.GetComponentInChildren<Renderer>(true);

        if (renderer != null)
        {
            BoxCollider box = anchorGo.AddComponent<BoxCollider>();
            if (anchorGo.transform == renderer.transform)
            {
                box.center = renderer.bounds.center - anchorGo.transform.position;
                box.size = renderer.bounds.size;
            }
            else
            {
                Bounds bounds = renderer.bounds;
                box.size = new Vector3(bounds.size.x, 0.04f, bounds.size.z);
                Vector3 topCenter = new Vector3(bounds.center.x, bounds.max.y - 0.02f, bounds.center.z);
                box.center = anchorGo.transform.InverseTransformPoint(topCenter);
            }

            return;
        }

        BoxCollider fallback = anchorGo.AddComponent<BoxCollider>();
        fallback.size = new Vector3(0.4f, 0.04f, 0.4f);
        fallback.center = Vector3.zero;
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
