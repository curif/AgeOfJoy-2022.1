/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// MR environment props layout (mr-environment-layout.yaml). Prefabs from Resources/ramiro/PrefabsEnvironment.
/// </summary>
public class MREnvironmentRegistry : MonoBehaviour
{
    const string LogPrefix = "[MREnvironmentRegistry]";
    public const string LayoutFileName = "mr-environment-layout.yaml";

    public static MREnvironmentRegistry Instance { get; private set; }

    readonly Dictionary<string, GameObject> spawnedById = new Dictionary<string, GameObject>();
    MREnvironmentLayout layout;

    public string LayoutFilePath => Path.Combine(ConfigManager.CabinetsDB, LayoutFileName);

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

        ConfigManager.CreateFolder(ConfigManager.CabinetsDB);
        layout = MREnvironmentLayout.LoadOrCreate(LayoutFilePath);
        ConfigManager.WriteConsole($"{LogPrefix} layout loaded ({layout.Props.Count} entries)");
    }

    public void SpawnAll(Transform mrSpaceOrigin)
    {
        DespawnAll();
        EnsureLayoutLoaded();

        if (mrSpaceOrigin == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} SpawnAll skipped — MRSpaceOrigin is null");
            return;
        }

        int index = 0;
        foreach (MREnvironmentPlacement placement in layout.GetProps())
        {
            if (placement == null || string.IsNullOrEmpty(placement.Id))
                continue;

            if (TrySpawnPlacement(placement, mrSpaceOrigin, index))
                index++;
        }

        ConfigManager.WriteConsole($"{LogPrefix} SpawnAll done ({spawnedById.Count} props)");
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

            WriteStoredPose(
                placement,
                placement.SurfaceType,
                entry.Value.transform.position,
                entry.Value.transform.rotation,
                anchorUuid);
            changed = true;
        }

        if (changed)
            layout.Save(LayoutFilePath);
    }

    public MREnvironmentPlacement FindPlacementByPrefabName(string prefabName)
    {
        EnsureLayoutLoaded();
        return layout?.FindByPrefabName(prefabName);
    }

    public bool IsPrefabInScene(string prefabName)
    {
        EnsureLayoutLoaded();
        return layout?.FindByPrefabName(prefabName) != null;
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

    public bool TryRemovePrefabFromScene(string prefabName)
    {
        EnsureLayoutLoaded();
        MREnvironmentPlacement placement = layout?.FindByPrefabName(prefabName);
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
        out GameObject spawnedRoot)
    {
        spawnedRoot = null;
        if (string.IsNullOrEmpty(prefabName) || mrSpaceOrigin == null)
            return false;

        if (IsPrefabInScene(prefabName))
            return false;

        return TrySpawnPrefabAtWorldPose(prefabName, mrSpaceOrigin, worldPosition, worldRotation, out spawnedRoot);
    }

    public bool TryFinalizeTransientAdd(
        string prefabName,
        GameObject root,
        Transform mrSpaceOrigin,
        Vector3 worldPosition,
        Quaternion worldRotation,
        Guid anchorUuid = default)
    {
        EnsureLayoutLoaded();
        if (layout == null || string.IsNullOrEmpty(prefabName) || root == null || mrSpaceOrigin == null)
            return false;

        if (layout.FindByPrefabName(prefabName) != null)
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

        var placement = new MREnvironmentPlacement
        {
            Id = $"{prefabName}-{Guid.NewGuid():N}".Substring(0, Mathf.Min(48, prefabName.Length + 33)),
            PrefabName = prefabName,
            Scale = 1f,
            SurfaceType = surfaceType,
            FacingAxis = facingAxis
        };
        WriteStoredPose(placement, surfaceType, worldPosition, worldRotation, anchorUuid);

        layout.AddPlacement(placement);
        layout.Save(LayoutFilePath);

        root.transform.localScale = Vector3.one * placement.Scale;

        MRPlacedEnvironment marker = root.GetComponent<MRPlacedEnvironment>();
        if (marker == null)
            marker = root.AddComponent<MRPlacedEnvironment>();
        marker.Initialize(placement.Id, prefabName);

        spawnedById[placement.Id] = root;
        ConfigManager.WriteConsole($"{LogPrefix} added {prefabName} ({placement.Id})");
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
            root.transform.localScale = Vector3.one * placement.Scale;
        }

        ConfigManager.WriteConsole($"{LogPrefix} updated pose {placement.DisplayLabel} ({placementId})");
        return true;
    }

    bool TrySpawnPlacement(MREnvironmentPlacement placement, Transform mrSpaceOrigin, int index)
    {
        if (string.IsNullOrEmpty(placement.PrefabName))
            return false;

        if (!TryReadWorldPose(placement, out Vector3 worldPos, out Quaternion worldRot))
            return false;

        if (!TrySpawnPrefabAtWorldPose(placement.PrefabName, mrSpaceOrigin, worldPos, worldRot, out GameObject root))
            return false;

        root.transform.localScale = Vector3.one * (placement.Scale > 0f ? placement.Scale : 1f);

        MRPlacedEnvironment marker = root.GetComponent<MRPlacedEnvironment>();
        if (marker == null)
            marker = root.AddComponent<MRPlacedEnvironment>();
        marker.Initialize(placement.Id, placement.PrefabName);

        spawnedById[placement.Id] = root;
        BackfillWorldPoseCache(placement, worldPos, worldRot);
        ConfigManager.WriteConsole($"{LogPrefix} spawned {placement.DisplayLabel} at {worldPos}");
        return true;
    }

    bool TrySpawnPrefabAtWorldPose(
        string prefabName,
        Transform mrSpaceOrigin,
        Vector3 worldPos,
        Quaternion worldRot,
        out GameObject spawnedRoot)
    {
        spawnedRoot = null;
        GameObject prefab = MREnvironmentCatalog.LoadPrefab(prefabName);
        if (prefab == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} prefab missing: {prefabName}");
            return false;
        }

        spawnedRoot = Instantiate(prefab, worldPos, worldRot);
        spawnedRoot.name = prefabName;
        spawnedRoot.transform.SetParent(null, worldPositionStays: true);
        return true;
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
}
