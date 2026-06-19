/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// MR environment props layout (MR/objects-layout.yaml). Build prefabs + Custom Objects packages.
/// </summary>
public class MREnvironmentRegistry : MonoBehaviour
{
    const string LogPrefix = "[MREnvironmentRegistry]";
    public const string LayoutFileName = "objects-layout.yaml";

    public static MREnvironmentRegistry Instance { get; private set; }

    readonly Dictionary<string, GameObject> spawnedById = new Dictionary<string, GameObject>();
    MREnvironmentLayout layout;

    public string LayoutFilePath => MRPaths.ResolveObjectsLayoutPath();

    public int SpawnedCount => spawnedById.Count;

    public IReadOnlyList<MREnvironmentPlacement> Placements =>
        layout != null ? layout.GetProps() : Array.Empty<MREnvironmentPlacement>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void EnsureLayoutLoaded()
    {
        if (layout != null)
            return;

        MRPaths.EnsureFolders();
        layout = MREnvironmentLayout.LoadOrCreate(LayoutFilePath);
        ConfigManager.WriteConsole($"{LogPrefix} layout loaded ({layout.Props.Count} entries)");
    }

    public void SpawnAll(Transform mrSpaceOrigin)
    {
        if (!isActiveAndEnabled)
            return;

        StartCoroutine(SpawnAllAsync(mrSpaceOrigin));
    }

    public IEnumerator SpawnAllAsync(Transform mrSpaceOrigin)
    {
        DespawnAll();
        EnsureLayoutLoaded();

        if (mrSpaceOrigin == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} SpawnAllAsync skipped — MRSpaceOrigin is null");
            yield break;
        }

        int index = 0;
        foreach (MREnvironmentPlacement placement in layout.GetProps())
        {
            if (placement == null || string.IsNullOrEmpty(placement.Id))
                continue;

            if (!placement.HasValidCatalogReference())
                continue;

            yield return TrySpawnPlacementAsync(placement, mrSpaceOrigin, index, spawned =>
            {
                if (spawned)
                    index++;
            });
        }

        ConfigManager.WriteConsole($"{LogPrefix} SpawnAllAsync done ({spawnedById.Count} props)");
    }

    public void DespawnAll()
    {
        foreach (GameObject root in spawnedById.Values)
        {
            if (root != null)
                Destroy(root);
        }

        foreach (MRPlacedEnvironment placed in FindObjectsOfType<MRPlacedEnvironment>(true))
        {
            if (placed != null && placed.gameObject != null)
                Destroy(placed.gameObject);
        }

        spawnedById.Clear();
    }

    public int HideAllImmediateCount()
    {
        int hidden = 0;
        foreach (GameObject root in spawnedById.Values)
        {
            if (root == null || !root.activeSelf)
                continue;

            root.SetActive(false);
            hidden++;
        }

        return hidden;
    }

    public IEnumerator DespawnAllAsync()
    {
        var roots = new List<GameObject>(spawnedById.Values);
        foreach (GameObject root in roots)
        {
            if (root != null)
                root.SetActive(false);
        }

        yield return null;

        foreach (GameObject root in roots)
        {
            if (root != null)
                Destroy(root);
            yield return null;
        }

        spawnedById.Clear();
        ConfigManager.WriteConsole($"{LogPrefix} DespawnAllAsync done ({roots.Count} props)");
    }

    public void SnapshotSpawnedWorldPosesToLayout()
    {
        EnsureLayoutLoaded();
        if (layout == null || spawnedById.Count == 0)
            return;

        bool changed = false;
        foreach (KeyValuePair<string, GameObject> entry in spawnedById)
        {
            if (entry.Value == null)
                continue;

            MREnvironmentPlacement placement = layout.FindById(entry.Key);
            if (placement == null)
                continue;

            Guid anchorUuid = Guid.Empty;
            if (!string.IsNullOrEmpty(placement.AnchorUuid))
                Guid.TryParse(placement.AnchorUuid, out anchorUuid);

            Vector3 displayPos = entry.Value.transform.position;
            Quaternion rot = entry.Value.transform.rotation;
            WriteStoredPose(
                placement,
                placement.SurfaceType,
                displayPos,
                rot,
                anchorUuid);
            changed = true;
            MRTransitionLog.Log(
                $"SnapshotWorldPose {placement.DisplayLabel} pos={displayPos} rotY={rot.eulerAngles.y:F1} anchor={placement.AnchorUuid ?? "none"}");
        }

        if (changed)
        {
            layout.Save(LayoutFilePath);
            MRTransitionLog.Log($"SnapshotSpawnedWorldPosesToLayout env count={spawnedById.Count}");
        }
    }

    public MREnvironmentPlacement FindPlacementById(string placementId)
    {
        EnsureLayoutLoaded();
        return layout?.FindById(placementId);
    }

    public MREnvironmentPlacement FindPlacementByPrefabName(string prefabName)
    {
        EnsureLayoutLoaded();
        return layout?.FindByPrefabName(prefabName);
    }

    public MREnvironmentPlacement FindPlacementByCatalogEntry(MREnvironmentCatalogEntry entry)
    {
        EnsureLayoutLoaded();
        return layout?.FindByCatalogEntry(entry);
    }

    public IReadOnlyList<MREnvironmentPlacement> FindAllPlacementsByCatalogEntry(MREnvironmentCatalogEntry entry)
    {
        EnsureLayoutLoaded();
        if (layout == null)
            return Array.Empty<MREnvironmentPlacement>();

        return layout.FindAllByCatalogEntry(entry);
    }

    public int GetInstanceCount(MREnvironmentCatalogEntry entry)
    {
        EnsureLayoutLoaded();
        return layout?.CountByCatalogEntry(entry) ?? 0;
    }

    public bool CanAddAnotherInstance(MREnvironmentCatalogEntry entry) =>
        !string.IsNullOrEmpty(entry.Key);

    public bool IsPrefabInScene(string prefabName) =>
        IsCatalogEntryInScene(MREnvironmentCatalogEntry.FromBuild(prefabName, prefabName));

    public bool IsCatalogEntryInScene(MREnvironmentCatalogEntry entry)
    {
        EnsureLayoutLoaded();
        return layout?.FindByCatalogEntry(entry) != null;
    }

    public bool TryGetSpawnedRoot(string placementId, out GameObject root)
    {
        if (string.IsNullOrEmpty(placementId))
        {
            root = null;
            return false;
        }

        return spawnedById.TryGetValue(placementId, out root) && root != null;
    }

    public bool TryRemovePrefabFromScene(string prefabName) =>
        TryRemoveCatalogEntryFromScene(MREnvironmentCatalogEntry.FromBuild(prefabName, prefabName));

    public bool TryRemoveCatalogEntryFromScene(MREnvironmentCatalogEntry entry)
    {
        EnsureLayoutLoaded();
        MREnvironmentPlacement placement = layout?.FindByCatalogEntry(entry);
        if (placement == null || string.IsNullOrEmpty(placement.Id))
            return false;

        return RemovePlacement(placement.Id);
    }

    public bool RemovePlacement(string placementId)
    {
        EnsureLayoutLoaded();
        if (string.IsNullOrEmpty(placementId))
            return false;

        MREnvironmentPlacement placement = layout.FindById(placementId);
        if (placement == null)
            return false;

        DestroySpawnedInstance(placementId);
        layout.RemoveById(placementId);
        layout.Save(LayoutFilePath);
        ConfigManager.WriteConsole($"{LogPrefix} removed {placement.DisplayLabel} ({placementId})");
        return true;
    }

    public bool TrySpawnTransientProp(
        string prefabName,
        Transform mrSpaceOrigin,
        Vector3 worldPosition,
        Quaternion worldRotation,
        out GameObject spawnedRoot) =>
        TrySpawnTransientCatalogEntry(
            MREnvironmentCatalogEntry.FromBuild(prefabName, prefabName),
            mrSpaceOrigin,
            worldPosition,
            worldRotation,
            out spawnedRoot);

    public bool TrySpawnTransientCatalogEntry(
        MREnvironmentCatalogEntry entry,
        Transform mrSpaceOrigin,
        Vector3 worldPosition,
        Quaternion worldRotation,
        out GameObject spawnedRoot)
    {
        spawnedRoot = null;
        if (mrSpaceOrigin == null || string.IsNullOrEmpty(entry.Key))
            return false;

        if (!CanAddAnotherInstance(entry))
            return false;

        if (entry.Source == MREnvironmentObjectSource.Custom)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} custom transient spawn requires async ({entry.Key})");
            return false;
        }

        if (entry.Source == MREnvironmentObjectSource.Light)
            return TrySpawnLightPrefabAtWorldPose(entry.Key, worldPosition, worldRotation, out spawnedRoot);

        if (entry.Source == MREnvironmentObjectSource.Poster)
            return TrySpawnPosterAtWorldPose(entry.Key, worldPosition, worldRotation, out spawnedRoot);

        return TrySpawnBuildPrefabAtWorldPose(entry.Key, worldPosition, worldRotation, out spawnedRoot);
    }

    public IEnumerator TrySpawnTransientCatalogEntryAsync(
        MREnvironmentCatalogEntry entry,
        Transform mrSpaceOrigin,
        Vector3 worldPosition,
        Quaternion worldRotation,
        CustomObjectSpawnResult result)
    {
        result.Success = false;
        result.Root = null;

        if (mrSpaceOrigin == null || string.IsNullOrEmpty(entry.Key))
            yield break;

        if (!CanAddAnotherInstance(entry))
            yield break;

        if (entry.Source == MREnvironmentObjectSource.Build)
        {
            if (TrySpawnBuildPrefabAtWorldPose(entry.Key, worldPosition, worldRotation, out GameObject buildRoot))
            {
                result.Root = buildRoot;
                result.Success = true;
            }

            yield break;
        }

        if (entry.Source == MREnvironmentObjectSource.Light)
        {
            if (TrySpawnLightPrefabAtWorldPose(entry.Key, worldPosition, worldRotation, out GameObject lightRoot))
            {
                result.Root = lightRoot;
                result.Success = true;
            }

            yield break;
        }

        if (entry.Source == MREnvironmentObjectSource.Poster)
        {
            if (TrySpawnPosterAtWorldPose(entry.Key, worldPosition, worldRotation, out GameObject posterRoot))
            {
                result.Root = posterRoot;
                result.Success = true;
            }

            yield break;
        }

        yield return MRCustomObjectLoader.InstantiateAtWorldPose(entry.Key, worldPosition, worldRotation, result);
    }

    public bool TryFinalizeTransientAdd(
        string prefabName,
        GameObject root,
        Transform mrSpaceOrigin,
        Vector3 worldPosition,
        Quaternion worldRotation,
        Guid anchorUuid = default) =>
        TryFinalizeTransientCatalogEntry(
            MREnvironmentCatalogEntry.FromBuild(prefabName, prefabName),
            root,
            mrSpaceOrigin,
            worldPosition,
            worldRotation,
            anchorUuid);

    public bool TryFinalizeTransientCatalogEntry(
        MREnvironmentCatalogEntry entry,
        GameObject root,
        Transform mrSpaceOrigin,
        Vector3 worldPosition,
        Quaternion worldRotation,
        Guid anchorUuid = default)
    {
        EnsureLayoutLoaded();
        if (layout == null || string.IsNullOrEmpty(entry.Key) || root == null || mrSpaceOrigin == null)
            return false;

        if (!CanAddAnotherInstance(entry))
        {
            DestroyTransientProp(root);
            return false;
        }

        MRPlacementProfile profile = MRPlacementProfile.Resolve(root);
        PlacementSurfaceType surfaceType = profile != null
            ? profile.surfaceType
            : PlacementSurfaceType.Floor;
        PlacementFacingAxis facingAxis = profile != null
            ? profile.facingAxis
            : PlacementFacingAxis.PositiveZ;

        string idSeed = entry.Key;
        var placement = new MREnvironmentPlacement
        {
            Id = $"{idSeed}-{Guid.NewGuid():N}".Substring(0, Mathf.Min(48, idSeed.Length + 33)),
            Source = entry.Source switch
            {
                MREnvironmentObjectSource.Custom => "custom",
                MREnvironmentObjectSource.Light => "light",
                MREnvironmentObjectSource.Poster => "poster",
                _ => "build"
            },
            Scale = ResolveScaleFromRoot(root),
            SurfaceType = surfaceType,
            FacingAxis = facingAxis
        };

        if (entry.Source == MREnvironmentObjectSource.Custom)
            placement.PackageName = entry.Key;
        else if (entry.Source == MREnvironmentObjectSource.Poster)
        {
            placement.TextureFile = entry.Key;
            placement.PrefabName = MRPosterFactory.PosterPrefabId;
        }
        else
            placement.PrefabName = entry.Key;

        if (entry.Source == MREnvironmentObjectSource.Light)
            CaptureLightSettingsFromRoot(placement, root);

        WriteStoredPose(placement, surfaceType, worldPosition, worldRotation, anchorUuid);

        layout.AddPlacement(placement);
        layout.Save(LayoutFilePath);

        ApplyRootScaleAfterFinalize(root, placement);

        MRPlacedEnvironment marker = root.GetComponent<MRPlacedEnvironment>();
        if (marker == null)
            marker = root.AddComponent<MRPlacedEnvironment>();
        marker.Initialize(placement.Id, entry);

        spawnedById[placement.Id] = root;
        ConfigManager.WriteConsole($"{LogPrefix} added {entry} ({placement.Id})");
        MRTransitionLog.Log(
            $"Finalize env {entry} pos={worldPosition} rotY={worldRotation.eulerAngles.y:F1} anchor={placement.AnchorUuid ?? "none"}");
        return true;
    }

    public void DestroyTransientProp(GameObject root)
    {
        if (root != null)
            Destroy(root);
    }

    public bool TryUpdatePlacementPose(
        string placementId,
        Transform mrSpaceOrigin,
        Vector3 worldPosition,
        Quaternion worldRotation,
        Guid anchorUuid = default)
    {
        EnsureLayoutLoaded();
        if (layout == null || string.IsNullOrEmpty(placementId) || mrSpaceOrigin == null)
            return false;

        MREnvironmentPlacement placement = layout.FindById(placementId);
        if (placement == null)
            return false;

        WriteStoredPose(placement, placement.SurfaceType, worldPosition, worldRotation, anchorUuid);
        layout.Save(LayoutFilePath);

        if (TryGetSpawnedRoot(placementId, out GameObject root))
        {
            root.transform.SetPositionAndRotation(worldPosition, worldRotation);
            ApplyRootScaleAfterSpawn(root, placement);
            NotifyPortableGamesPlacementUpdated(root);
        }

        ConfigManager.WriteConsole($"{LogPrefix} updated pose {placement.DisplayLabel} ({placementId})");
        return true;
    }

    IEnumerator TrySpawnPlacementAsync(
        MREnvironmentPlacement placement,
        Transform mrSpaceOrigin,
        int index,
        Action<bool> onComplete)
    {
        bool spawned = false;
        if (placement == null || !placement.HasValidCatalogReference())
        {
            onComplete?.Invoke(false);
            yield break;
        }

        if (!TryReadWorldPose(placement, out Vector3 worldPos, out Quaternion worldRot))
        {
            onComplete?.Invoke(false);
            yield break;
        }

        placement.NormalizeLegacySource();
        GameObject root = null;

        if (placement.IsCustomSource)
        {
            var result = new CustomObjectSpawnResult();
            yield return MRCustomObjectLoader.InstantiateAtWorldPose(
                placement.PackageName,
                worldPos,
                worldRot,
                result);
            if (!result.Success)
            {
                MRDebugLog.LogError($"Environment spawn failed: custom '{placement.PackageName}' ({placement.Id})");
                onComplete?.Invoke(false);
                yield break;
            }

            root = result.Root;
        }
        else if (placement.IsLightSource)
        {
            if (!TrySpawnLightPrefabAtWorldPose(placement.PrefabName, worldPos, worldRot, out root))
            {
                MRDebugLog.LogError($"Environment spawn failed: light '{placement.PrefabName}' ({placement.Id})");
                onComplete?.Invoke(false);
                yield break;
            }
        }
        else if (placement.IsPosterSource)
        {
            if (!TrySpawnPosterAtWorldPose(placement.TextureFile, worldPos, worldRot, out root))
            {
                MRDebugLog.LogError($"Environment spawn failed: poster '{placement.TextureFile}' ({placement.Id})");
                onComplete?.Invoke(false);
                yield break;
            }
        }
        else if (!TrySpawnBuildPrefabAtWorldPose(placement.PrefabName, worldPos, worldRot, out root))
        {
            MRDebugLog.LogError($"Environment spawn failed: build '{placement.PrefabName}' ({placement.Id})");
            onComplete?.Invoke(false);
            yield break;
        }

        ApplyRootScaleAfterSpawn(root, placement);
        ApplyStoredLightSettings(root, placement);
        NotifyPortableGamesPlacementUpdated(root);

        MRPlacedEnvironment marker = root.GetComponent<MRPlacedEnvironment>();
        if (marker == null)
            marker = root.AddComponent<MRPlacedEnvironment>();

        MREnvironmentCatalogEntry entry = placement.IsCustomSource
            ? MREnvironmentCatalogEntry.FromCustom(placement.PackageName, placement.DisplayLabel)
            : placement.IsLightSource
                ? MREnvironmentCatalogEntry.FromLight(placement.PrefabName, placement.DisplayLabel)
                : placement.IsPosterSource
                    ? MREnvironmentCatalogEntry.FromPoster(placement.TextureFile, placement.DisplayLabel)
                    : MREnvironmentCatalogEntry.FromBuild(placement.PrefabName, placement.DisplayLabel);
        marker.Initialize(placement.Id, entry);

        spawnedById[placement.Id] = root;
        BackfillWorldPoseCache(placement, worldPos, worldRot);
        ConfigManager.WriteConsole($"{LogPrefix} spawned {placement.DisplayLabel} at {worldPos}");
        spawned = true;
        onComplete?.Invoke(spawned);
    }

    bool TrySpawnPosterAtWorldPose(
        string textureRelativePath,
        Vector3 worldPos,
        Quaternion worldRot,
        out GameObject spawnedRoot)
    {
        spawnedRoot = null;
        if (!MRPosterFactory.TryInstantiate(textureRelativePath, worldPos, worldRot, out spawnedRoot))
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} poster missing or failed: {textureRelativePath}");
            MRDebugLog.LogError($"Poster missing or failed: {textureRelativePath}");
            return false;
        }

        return true;
    }

    bool TrySpawnBuildPrefabAtWorldPose(
        string prefabName,
        Vector3 worldPos,
        Quaternion worldRot,
        out GameObject spawnedRoot)
    {
        spawnedRoot = null;
        GameObject prefab = MREnvironmentCatalog.LoadPrefab(prefabName);
        if (prefab == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} prefab missing: {prefabName}");
            MRDebugLog.LogError($"Environment prefab missing: {prefabName}");
            return false;
        }

        spawnedRoot = Instantiate(prefab, worldPos, worldRot);
        spawnedRoot.name = prefabName;
        spawnedRoot.transform.SetParent(null, worldPositionStays: true);
        NotifyPortableGamesPlacementUpdated(spawnedRoot);
        return true;
    }

    public bool TryGetLightSettings(
        string placementId,
        out float intensity,
        out float range,
        out float temperature)
    {
        intensity = 0f;
        range = 0f;
        temperature = 0f;
        EnsureLayoutLoaded();
        MREnvironmentPlacement placement = layout?.FindById(placementId);
        if (placement == null || !placement.IsLightSource)
            return false;

        intensity = placement.LightIntensity;
        range = placement.LightRange;
        temperature = placement.LightTemperature;
        ResolveMissingLightSettings(
            placement,
            out float resolvedIntensity,
            out float resolvedRange,
            out float resolvedTemperature);
        if (Mathf.Approximately(intensity, 0f)
            && Mathf.Approximately(range, 0f)
            && Mathf.Approximately(temperature, 0f))
        {
            intensity = resolvedIntensity;
            range = resolvedRange;
            temperature = resolvedTemperature;
        }
        else
        {
            if (Mathf.Approximately(range, 0f))
                range = resolvedRange;
            if (Mathf.Approximately(temperature, 0f))
                temperature = resolvedTemperature;
        }

        return true;
    }

    public bool TryUpdateLightSettings(
        string placementId,
        float intensity,
        float range,
        float temperature)
    {
        EnsureLayoutLoaded();
        if (layout == null)
            return false;

        MREnvironmentPlacement placement = layout.FindById(placementId);
        if (placement == null || !placement.IsLightSource)
            return false;

        placement.LightIntensity = MRLightPlacement.SnapTune(
            intensity,
            MRLightPlacement.MinIntensity,
            MRLightPlacement.MaxIntensity);
        placement.LightRange = MRLightPlacement.SnapTune(
            range,
            MRLightPlacement.MinRange,
            MRLightPlacement.MaxRange);
        placement.LightTemperature = MRLightPlacement.SnapTemperature(temperature);
        layout.Save(LayoutFilePath);

        if (TryGetSpawnedRoot(placement.Id, out GameObject root))
            MRLightPlacement.ApplyAllLights(
                root,
                placement.LightIntensity,
                placement.LightRange,
                placement.LightTemperature);

        ConfigManager.WriteConsole(
            $"{LogPrefix} light tune {placement.DisplayLabel} intensity={placement.LightIntensity:F1} range={placement.LightRange:F1} temp={placement.LightTemperature:F0}K");
        return true;
    }

    static void CaptureLightSettingsFromRoot(MREnvironmentPlacement placement, GameObject root)
    {
        if (placement == null || root == null)
            return;

        if (!MRLightPlacement.TryReadInstanceValues(root, out float intensity, out float range, out float temperature))
            return;

        placement.LightIntensity = intensity;
        placement.LightRange = range;
        placement.LightTemperature = temperature;
    }

    static void ApplyStoredLightSettings(GameObject root, MREnvironmentPlacement placement)
    {
        if (root == null || placement == null || !placement.IsLightSource)
            return;

        ResolveMissingLightSettings(
            placement,
            out float intensity,
            out float range,
            out float temperature);
        MRLightPlacement.ApplyAllLights(root, intensity, range, temperature);
    }

    static void ResolveMissingLightSettings(
        MREnvironmentPlacement placement,
        out float intensity,
        out float range,
        out float temperature)
    {
        intensity = placement.LightIntensity;
        range = placement.LightRange;
        temperature = placement.LightTemperature;

        GameObject prefab = MRLightsCatalog.LoadPrefab(placement.PrefabName);
        if (!MRLightPlacement.TryReadPrefabDefaults(
                prefab,
                out float prefabIntensity,
                out float prefabRange,
                out float prefabTemperature))
            return;

        if (Mathf.Approximately(intensity, 0f)
            && Mathf.Approximately(range, 0f)
            && Mathf.Approximately(temperature, 0f))
        {
            intensity = prefabIntensity;
            range = prefabRange;
            temperature = prefabTemperature;
            return;
        }

        if (Mathf.Approximately(range, 0f))
            range = prefabRange;
        if (Mathf.Approximately(temperature, 0f))
            temperature = prefabTemperature;
    }

    bool TrySpawnLightPrefabAtWorldPose(
        string prefabName,
        Vector3 worldPos,
        Quaternion worldRot,
        out GameObject spawnedRoot)
    {
        spawnedRoot = null;
        GameObject prefab = MRLightsCatalog.LoadPrefab(prefabName);
        if (prefab == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} light prefab missing: {prefabName}");
            MRDebugLog.LogError($"Light prefab missing: {prefabName}");
            return false;
        }

        spawnedRoot = Instantiate(prefab, worldPos, worldRot);
        spawnedRoot.name = prefabName;
        spawnedRoot.transform.SetParent(null, worldPositionStays: true);
        NotifyPortableGamesPlacementUpdated(spawnedRoot);
        return true;
    }

    static void NotifyPortableGamesPlacementUpdated(GameObject root)
    {
        if (root == null)
            return;

        PortableGamesTwoHandGrab portableGrab = root.GetComponentInChildren<PortableGamesTwoHandGrab>(true);
        portableGrab?.NotifyPlacementPoseUpdated();

        MRCustomObjectGrab customGrab = root.GetComponentInChildren<MRCustomObjectGrab>(true);
        customGrab?.NotifyPlacementPoseUpdated();
    }

    void DestroySpawnedInstance(string placementId)
    {
        if (!spawnedById.TryGetValue(placementId, out GameObject root))
            return;

        spawnedById.Remove(placementId);
        if (root != null)
            Destroy(root);
    }

    static void BackfillWorldPoseCache(MREnvironmentPlacement placement, Vector3 worldPos, Quaternion worldRot)
    {
        if (placement == null || placement.WorldPosition != null)
            return;

        placement.WorldPosition = MRVector3.From(worldPos);
        placement.WorldRotation = MRQuaternion.From(worldRot);
    }

    static void WriteStoredPose(
        MREnvironmentPlacement placement,
        PlacementSurfaceType surfaceType,
        Vector3 worldPosition,
        Quaternion worldRotation,
        Guid anchorUuid)
    {
        placement.WorldPosition = MRVector3.From(worldPosition);
        placement.WorldRotation = MRQuaternion.From(worldRotation);

        Meta.XR.MRUtilityKit.MRUKRoom room = MREnvironmentSurfaces.Instance?.CurrentRoom;
        if (MRAnchorPoseResolver.TryWriteAnchorRelativePose(
                room,
                surfaceType,
                worldPosition,
                worldRotation,
                anchorUuid,
                out string anchorUuidText,
                out MRVector3 storedPosition,
                out MRQuaternion storedRotation))
        {
            placement.AnchorUuid = anchorUuidText;
            placement.Position = storedPosition;
            placement.Rotation = storedRotation;
            return;
        }

        placement.AnchorUuid = null;
        placement.Position = MRVector3.From(worldPosition);
        placement.Rotation = MRQuaternion.From(worldRotation);
    }

    static bool TryReadWorldPose(MREnvironmentPlacement placement, out Vector3 worldPosition, out Quaternion worldRotation)
    {
        worldPosition = Vector3.zero;
        worldRotation = Quaternion.identity;

        if (placement == null)
            return false;

        if (!string.IsNullOrEmpty(placement.AnchorUuid))
        {
            Vector3 storedPosition = placement.Position != null ? placement.Position.ToVector3() : Vector3.zero;
            Quaternion storedRotation = placement.Rotation != null ? placement.Rotation.ToQuaternion() : Quaternion.identity;
            Meta.XR.MRUtilityKit.MRUKRoom room = MREnvironmentSurfaces.Instance?.CurrentRoom;
            if (MRAnchorPoseResolver.TryResolveWorldPose(
                    room,
                    placement.AnchorUuid,
                    storedPosition,
                    storedRotation,
                    placement.SurfaceType,
                    out worldPosition,
                    out worldRotation))
                return true;

            if (TryReadWorldFallback(placement, out worldPosition, out worldRotation))
                return true;

            return false;
        }

        if (TryReadWorldFallback(placement, out worldPosition, out worldRotation))
            return true;

        if (placement.Position != null && placement.Rotation != null)
        {
            worldPosition = placement.Position.ToVector3();
            worldRotation = placement.Rotation.ToQuaternion();
            return true;
        }

        return false;
    }

    static bool TryReadWorldFallback(MREnvironmentPlacement placement, out Vector3 worldPosition, out Quaternion worldRotation)
    {
        worldPosition = Vector3.zero;
        worldRotation = Quaternion.identity;
        if (placement?.WorldPosition == null || placement.WorldRotation == null)
            return false;

        worldPosition = placement.WorldPosition.ToVector3();
        worldRotation = placement.WorldRotation.ToQuaternion();
        return true;
    }

    static float ResolveScaleFromRoot(GameObject root)
    {
        float scale = root != null ? root.transform.localScale.x : 1f;
        return scale > 0f ? scale : 1f;
    }

    static void ApplyRootScaleAfterFinalize(GameObject root, MREnvironmentPlacement placement)
    {
        if (root == null || placement == null)
            return;

        if (placement.IsPosterSource)
        {
            MRWallPoster poster = root.GetComponent<MRWallPoster>();
            if (poster != null)
            {
                poster.ApplyTextureAndScale();
                return;
            }
        }

        root.transform.localScale = Vector3.one * placement.Scale;
    }

    static void ApplyRootScaleAfterSpawn(GameObject root, MREnvironmentPlacement placement)
    {
        if (root == null || placement == null)
            return;

        if (placement.IsPosterSource)
        {
            MRWallPoster poster = root.GetComponent<MRWallPoster>();
            if (poster != null)
            {
                poster.ApplyTextureAndScale();
                return;
            }
        }

        root.transform.localScale = Vector3.one * ResolveSpawnScale(placement);
    }

    static float ResolveSpawnScale(MREnvironmentPlacement placement)
    {
        if (placement == null)
            return 1f;

        placement.NormalizeLegacySource();
        float layoutScale = placement.Scale > 0f ? placement.Scale : 1f;

        if (placement.IsCustomSource
            && MRCustomObjectDefinition.TryLoad(placement.PackageName, out MRCustomObjectDefinition definition))
        {
            float yamlScale = definition.GetModelScale();
            if (!Mathf.Approximately(layoutScale, 1f))
                return layoutScale;
            return yamlScale;
        }

        return layoutScale;
    }
}
