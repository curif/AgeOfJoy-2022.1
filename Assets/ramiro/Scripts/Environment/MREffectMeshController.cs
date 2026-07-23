/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Enables MRUK EffectMesh objects placed under FixedScene/MR (disabled by default).
/// Uses Meta MR Utility Kit package prefabs in scene; Resources/ramiro is runtime fallback only.
/// </summary>
public class MREffectMeshController : MonoBehaviour
{
    const string LogPrefix = "[MREffectMeshController]";
    const string AnchorMeshResourcesPath = "ramiro/EffectMesh";
    const string GlobalMeshResourcesPath = "ramiro/EffectMeshGlobalMesh";
    const string OccluderAndShadowMaterialFallbackPath = "ramiro/Materials/MROccluderAndShadow";
    const string RoomBoxEffectsMaterialPath = "ramiro/Materials/RoomBoxEffects";

    GameObject anchorMeshRoot;
    GameObject globalMeshRoot;
    EffectMesh anchorEffectMesh;
    EffectMesh globalEffectMesh;
    Coroutine spawnRoutine;
    bool anchorMeshFromScene;
    bool globalMeshFromScene;

    static Material cachedOccluderMaterial;
    static Material cachedRoomBoxEffectsMaterial;

    public bool IsSpawned => anchorMeshRoot != null || globalMeshRoot != null;
    public bool IsAnchorMeshSpawned => anchorMeshRoot != null && anchorMeshRoot.activeSelf;
    public bool IsGlobalMeshSpawned => globalMeshRoot != null && globalMeshRoot.activeSelf;

    public EffectMesh GetAnchorEffectMesh() => anchorEffectMesh;

    public void RestoreDefaultMeshMaterials()
    {
        if (MRRoomSurfaceSkin.IsActive)
            return;

        ApplyColorTint();
    }

    public void Spawn()
    {
        ApplySettings();
    }

    /// <summary>Spawn or despawn mesh layers according to <see cref="MREffectMeshSettings"/>.</summary>
    public void ApplySettings()
    {
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);

        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    /// <summary>Refresh EffectMesh materials (RoomBoxEffects vs occluder) without respawning.</summary>
    public void ApplyColorTint()
    {
        if (MRRoomSurfaceSkin.IsActive)
            return;

        if (MREffectMeshSettings.AnchorMeshEnabled
            && anchorEffectMesh != null
            && !HasEffectMeshGeometry(anchorEffectMesh))
        {
            TrySpawnAnchorMesh();
        }

        ApplyMeshMaterials(anchorEffectMesh);
        ApplyMeshMaterials(globalEffectMesh);
    }

    public IEnumerator WaitForAnchorMeshReady(float timeoutSeconds = 15f)
    {
        float remaining = timeoutSeconds;
        while (remaining > 0f)
        {
            if (anchorEffectMesh != null && HasEffectMeshGeometry(anchorEffectMesh))
                yield break;

            remaining -= Time.unscaledDeltaTime;
            yield return null;
        }
    }

    public void Despawn()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        TearDown(ref anchorMeshRoot, ref anchorEffectMesh, ref anchorMeshFromScene);
        TearDown(ref globalMeshRoot, ref globalEffectMesh, ref globalMeshFromScene);
        MRTransitionLog.LogStep("MREffectMeshController", "despawned");
    }

    IEnumerator SpawnRoutine()
    {
        MREffectMeshSettings.EnsureLoaded();

        if (MRUK.Instance == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} spawn skipped — MRUK.Instance null");
            spawnRoutine = null;
            yield break;
        }

        Transform parent = MRUK.Instance.transform;

        if (MREffectMeshSettings.AnchorMeshEnabled)
        {
            if (anchorMeshRoot == null)
                ResolveAnchorMesh(parent);

            TrySpawnAnchorMesh();
        }
        else
        {
            TearDown(ref anchorMeshRoot, ref anchorEffectMesh, ref anchorMeshFromScene);
        }

        if (MREffectMeshSettings.GlobalMeshEnabled)
        {
            if (globalMeshRoot == null)
                ResolveGlobalMesh(parent);

            TrySpawnGlobalMesh();
        }
        else
        {
            TearDown(ref globalMeshRoot, ref globalEffectMesh, ref globalMeshFromScene);
        }

        yield return ApplyOccluderRenderers();

        MRTransitionLog.LogStep("MREffectMeshController",
            $"layers anchor={(anchorEffectMesh != null)} global={(globalEffectMesh != null)} " +
            $"scene anchor={anchorMeshFromScene} scene global={globalMeshFromScene} " +
            $"prefs anchor={MREffectMeshSettings.AnchorMeshEnabled} global={MREffectMeshSettings.GlobalMeshEnabled}");
        ConfigManager.WriteConsole($"{LogPrefix} applied settings (mesh material per scan colors pref)");
        spawnRoutine = null;
    }

    void ResolveAnchorMesh(Transform parent)
    {
        EffectMesh sceneMesh = MRSceneHost.GetSceneEffectMesh(MRSceneHost.EffectMeshChildName);
        if (sceneMesh != null)
        {
            anchorMeshRoot = sceneMesh.gameObject;
            anchorEffectMesh = sceneMesh;
            anchorMeshFromScene = true;
            PrepareSceneMeshInstance(anchorMeshRoot, parent);
            ConfigManager.WriteConsole($"{LogPrefix} anchor mesh from FixedScene/{MRSceneHost.RootObjectName}");
            return;
        }

        GameObject anchorPrefab = Resources.Load<GameObject>(AnchorMeshResourcesPath);
        if (anchorPrefab == null)
        {
            ConfigManager.WriteConsoleError(
                $"{LogPrefix} missing prefab Resources/{AnchorMeshResourcesPath}");
            return;
        }

        anchorMeshRoot = SpawnEffectMeshInstance(anchorPrefab, parent, "MREffectMesh_Anchors");
        anchorEffectMesh = anchorMeshRoot != null ? anchorMeshRoot.GetComponent<EffectMesh>() : null;
        anchorMeshFromScene = false;
    }

    void ResolveGlobalMesh(Transform parent)
    {
        EffectMesh sceneMesh = MRSceneHost.GetSceneEffectMesh(MRSceneHost.EffectMeshGlobalChildName);
        if (sceneMesh != null)
        {
            globalMeshRoot = sceneMesh.gameObject;
            globalEffectMesh = sceneMesh;
            globalMeshFromScene = true;
            PrepareSceneMeshInstance(globalMeshRoot, parent);
            ConfigManager.WriteConsole($"{LogPrefix} global mesh from FixedScene/{MRSceneHost.RootObjectName}");
            return;
        }

        GameObject globalPrefab = Resources.Load<GameObject>(GlobalMeshResourcesPath);
        if (globalPrefab == null)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} missing prefab Resources/{GlobalMeshResourcesPath}");
            return;
        }

        globalMeshRoot = SpawnEffectMeshInstance(globalPrefab, parent, "MREffectMesh_Global");
        globalEffectMesh = globalMeshRoot != null ? globalMeshRoot.GetComponent<EffectMesh>() : null;
        globalMeshFromScene = false;
    }

    static void PrepareSceneMeshInstance(GameObject instance, Transform parent)
    {
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        ConfigureEffectMeshComponent(instance.GetComponent<EffectMesh>(), instance.name);
        instance.SetActive(true);
    }

    static GameObject SpawnEffectMeshInstance(
        GameObject prefab,
        Transform parent,
        string instanceName)
    {
        GameObject instance = Instantiate(prefab, parent);
        instance.name = instanceName;
        instance.SetActive(false);
        ConfigureEffectMeshComponent(instance.GetComponent<EffectMesh>(), instanceName);
        instance.SetActive(true);
        return instance;
    }

    static void ConfigureEffectMeshComponent(EffectMesh effectMesh, string instanceName)
    {
        if (effectMesh == null)
            return;

        Material material = ResolveMeshMaterial(out string materialName);
        if (material != null)
            effectMesh.MeshMaterial = material;
        else
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} {instanceName} has no MeshMaterial ({materialName} missing)");

        effectMesh.CastShadow = false;
        // Cut holes in walls for doors/windows; portal owns WINDOW_FRAME (do not mesh it).
        effectMesh.CutHoles = MRUKAnchor.SceneLabels.DOOR_FRAME | MRUKAnchor.SceneLabels.WINDOW_FRAME;
        effectMesh.Labels &= ~MRUKAnchor.SceneLabels.WINDOW_FRAME;
    }

    static Material ResolveMeshMaterial(out string materialName)
    {
        MREffectMeshSettings.EnsureLoaded();
        if (MREffectMeshSettings.ScanDebugColorsEnabled)
        {
            materialName = "RoomBoxEffects";
            return LoadRoomBoxEffectsMaterial();
        }

        materialName = "MROccluderAndShadow";
        return LoadOccluderMaterial();
    }

    static Material LoadOccluderMaterial()
    {
        if (cachedOccluderMaterial == null)
            cachedOccluderMaterial = Resources.Load<Material>(OccluderAndShadowMaterialFallbackPath);
        return cachedOccluderMaterial;
    }

    static Material LoadRoomBoxEffectsMaterial()
    {
        if (cachedRoomBoxEffectsMaterial == null)
            cachedRoomBoxEffectsMaterial = Resources.Load<Material>(RoomBoxEffectsMaterialPath);
        return cachedRoomBoxEffectsMaterial;
    }

    IEnumerator ApplyOccluderRenderers()
    {
        const float timeoutSeconds = 3f;
        float remaining = timeoutSeconds;
        while (remaining > 0f)
        {
            if (AreEnabledMeshLayersReady())
                break;

            remaining -= Time.unscaledDeltaTime;
            yield return null;
        }

        ApplyColorTint();

        yield return null;
        ApplyColorTint();
    }

    bool AreEnabledMeshLayersReady()
    {
        MREffectMeshSettings.EnsureLoaded();

        bool anchorReady = !MREffectMeshSettings.AnchorMeshEnabled
            || anchorEffectMesh == null
            || HasEffectMeshGeometry(anchorEffectMesh);
        bool globalReady = !MREffectMeshSettings.GlobalMeshEnabled
            || globalEffectMesh == null
            || HasEffectMeshGeometry(globalEffectMesh);

        return anchorReady && globalReady;
    }

    static bool HasEffectMeshGeometry(EffectMesh effectMesh)
    {
        return effectMesh != null && effectMesh.EffectMeshObjects.Count > 0;
    }

    void ApplyMeshMaterials(EffectMesh effectMesh)
    {
        if (effectMesh == null)
            return;

        MREffectMeshSettings.EnsureLoaded();
        Material material = ResolveMeshMaterial(out string materialName);
        if (material == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} ApplyMeshMaterials skipped — {materialName} missing");
            return;
        }

        effectMesh.CastShadow = false;

        if (effectMesh.MeshMaterial != material)
            effectMesh.MeshMaterial = material;

        if (!HasEffectMeshGeometry(effectMesh))
            return;

        effectMesh.OverrideEffectMaterial(material);

        int configured = 0;
        foreach (var pair in effectMesh.EffectMeshObjects)
        {
            EffectMesh.EffectMeshObject meshObject = pair.Value;
            if (meshObject?.effectMeshGO == null)
                continue;

            MeshRenderer renderer = meshObject.effectMeshGO.GetComponent<MeshRenderer>();
            if (renderer == null)
                continue;

            renderer.SetPropertyBlock(null);
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = !MREffectMeshSettings.ScanDebugColorsEnabled;
            configured++;
        }

        if (configured > 0)
        {
            ConfigManager.WriteConsole(
                $"{LogPrefix} {effectMesh.name}: {configured} renderer(s) material={materialName} " +
                $"scanColors={MREffectMeshSettings.ScanDebugColorsEnabled}");
        }
    }

    void TrySpawnAnchorMesh()
    {
        if (anchorEffectMesh == null || MRUK.Instance == null)
            return;

        if (HasEffectMeshGeometry(anchorEffectMesh))
            return;

        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} anchor mesh skipped — no current room");
            return;
        }

        Material material = ResolveMeshMaterial(out _);
        if (material != null)
            anchorEffectMesh.MeshMaterial = material;

        anchorEffectMesh.CreateMesh(room);
        ConfigManager.WriteConsole($"{LogPrefix} anchor mesh created for room '{room.name}'");
        ApplyColorTint();
    }

    void TrySpawnGlobalMesh()
    {
        if (globalEffectMesh == null || MRUK.Instance == null)
            return;

        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room == null || room.GlobalMeshAnchor == null)
        {
            ConfigManager.WriteConsole(
                $"{LogPrefix} global mesh skipped — no GlobalMeshAnchor in current room");
            return;
        }

        Material material = ResolveMeshMaterial(out _);
        if (material != null)
            globalEffectMesh.MeshMaterial = material;

        globalEffectMesh.CreateMesh(room);
        ConfigManager.WriteConsole($"{LogPrefix} global mesh created for room '{room.name}'");
        ApplyColorTint();
    }

    static void TearDown(ref GameObject root, ref EffectMesh effect, ref bool fromScene)
    {
        if (effect != null)
            effect.DestroyMesh();

        if (root != null)
        {
            if (fromScene)
                MRSceneHost.StashEffectMeshUnderRoot(root);
            else
                Destroy(root);
        }

        root = null;
        effect = null;
        fromScene = false;
    }
}
