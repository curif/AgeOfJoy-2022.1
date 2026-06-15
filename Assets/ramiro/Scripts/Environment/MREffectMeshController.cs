/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Spawns MRUK EffectMesh prefabs from Resources/ramiro after the scene is loaded.
/// Uses transparent occluder shader: depth hides props behind walls; receives shadows only (no cast).
/// </summary>
public class MREffectMeshController : MonoBehaviour
{
    const string LogPrefix = "[MREffectMeshController]";
    const string AnchorMeshResourcesPath = "ramiro/EffectMesh";
    const string GlobalMeshResourcesPath = "ramiro/EffectMeshGlobalMesh";
    const string OccluderAndShadowMaterialFallbackPath = "ramiro/Materials/MROccluderAndShadow";

    GameObject anchorMeshRoot;
    GameObject globalMeshRoot;
    EffectMesh anchorEffectMesh;
    EffectMesh globalEffectMesh;
    Coroutine spawnRoutine;
    MaterialPropertyBlock tintPropertyBlock;

    static readonly int TintEnabledId = Shader.PropertyToID("_TintEnabled");
    static readonly int TintColorId = Shader.PropertyToID("_TintColor");

    public bool IsSpawned => anchorMeshRoot != null || globalMeshRoot != null;
    public bool IsAnchorMeshSpawned => anchorMeshRoot != null;
    public bool IsGlobalMeshSpawned => globalMeshRoot != null;

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

    /// <summary>Refresh mesh tint color without respawning EffectMesh instances.</summary>
    public void ApplyColorTint()
    {
        ConfigureOccluderRenderers(anchorEffectMesh);
        ConfigureOccluderRenderers(globalEffectMesh);
    }

    public void Despawn()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        TearDown(ref anchorMeshRoot, ref anchorEffectMesh);
        TearDown(ref globalMeshRoot, ref globalEffectMesh);
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
            {
                GameObject anchorPrefab = Resources.Load<GameObject>(AnchorMeshResourcesPath);
                if (anchorPrefab == null)
                {
                    ConfigManager.WriteConsoleError(
                        $"{LogPrefix} missing prefab Resources/{AnchorMeshResourcesPath}");
                }
                else
                {
                    anchorMeshRoot = SpawnEffectMeshInstance(anchorPrefab, parent, "MREffectMesh_Anchors");
                    anchorEffectMesh = anchorMeshRoot != null
                        ? anchorMeshRoot.GetComponent<EffectMesh>()
                        : null;
                }
            }
        }
        else
        {
            TearDown(ref anchorMeshRoot, ref anchorEffectMesh);
        }

        if (MREffectMeshSettings.GlobalMeshEnabled)
        {
            if (globalMeshRoot == null)
            {
                GameObject globalPrefab = Resources.Load<GameObject>(GlobalMeshResourcesPath);
                if (globalPrefab == null)
                {
                    ConfigManager.WriteConsoleWarning(
                        $"{LogPrefix} missing prefab Resources/{GlobalMeshResourcesPath}");
                }
                else
                {
                    globalMeshRoot = SpawnEffectMeshInstance(globalPrefab, parent, "MREffectMesh_Global");
                    globalEffectMesh = globalMeshRoot != null
                        ? globalMeshRoot.GetComponent<EffectMesh>()
                        : null;
                }
            }

            TrySpawnGlobalMesh();
        }
        else
        {
            TearDown(ref globalMeshRoot, ref globalEffectMesh);
        }

        yield return ApplyOccluderRenderers();

        MRTransitionLog.LogStep("MREffectMeshController",
            $"layers anchor={(anchorEffectMesh != null)} global={(globalEffectMesh != null)} " +
            $"prefs anchor={MREffectMeshSettings.AnchorMeshEnabled} global={MREffectMeshSettings.GlobalMeshEnabled}");
        ConfigManager.WriteConsole($"{LogPrefix} applied settings (transparent occluder, receive shadow)");
        spawnRoutine = null;
    }

    static GameObject SpawnEffectMeshInstance(
        GameObject prefab,
        Transform parent,
        string instanceName)
    {
        GameObject instance = Instantiate(prefab, parent);
        instance.name = instanceName;
        instance.SetActive(false);

        EffectMesh effectMesh = instance.GetComponent<EffectMesh>();
        if (effectMesh != null)
        {
            if (effectMesh.MeshMaterial == null)
            {
                Material fallback = Resources.Load<Material>(OccluderAndShadowMaterialFallbackPath);
                if (fallback != null)
                    effectMesh.MeshMaterial = fallback;
                else
                    ConfigManager.WriteConsoleWarning(
                        $"{LogPrefix} {instanceName} has no MeshMaterial and fallback missing");
            }

            effectMesh.CastShadow = false;
        }

        instance.SetActive(true);
        return instance;
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

    void ConfigureOccluderRenderers(EffectMesh effectMesh)
    {
        if (effectMesh == null)
            return;

        effectMesh.CastShadow = false;
        MREffectMeshSettings.EnsureLoaded();
        bool scanColorsEnabled = MREffectMeshSettings.ScanDebugColorsEnabled;
        int configured = 0;

        if (tintPropertyBlock == null)
            tintPropertyBlock = new MaterialPropertyBlock();

        foreach (var pair in effectMesh.EffectMeshObjects)
        {
            MRUKAnchor anchor = pair.Key;
            EffectMesh.EffectMeshObject meshObject = pair.Value;
            if (meshObject?.effectMeshGO == null)
                continue;

            MeshRenderer renderer = meshObject.effectMeshGO.GetComponent<MeshRenderer>();
            if (renderer == null)
                continue;

            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = true;

            Color tintColor = Color.clear;
            bool tintEnabled = scanColorsEnabled
                && MREffectMeshScanColors.TryGetColor(anchor, out tintColor);
            tintPropertyBlock.SetFloat(TintEnabledId, tintEnabled ? 1f : 0f);
            tintPropertyBlock.SetColor(TintColorId, tintColor);
            renderer.SetPropertyBlock(tintPropertyBlock);
            configured++;
        }

        if (configured > 0)
            ConfigManager.WriteConsole(
                $"{LogPrefix} {effectMesh.name}: {configured} renderer(s) scanColors={scanColorsEnabled}");
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

        globalEffectMesh.CreateMesh(room);
        ConfigManager.WriteConsole($"{LogPrefix} global mesh created for room '{room.name}'");
        ApplyColorTint();
    }

    static void TearDown(ref GameObject root, ref EffectMesh effect)
    {
        if (effect != null)
            effect.DestroyMesh();

        if (root != null)
            Destroy(root);

        root = null;
        effect = null;
    }
}
