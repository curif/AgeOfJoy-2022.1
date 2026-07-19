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
        ConfigManager.WriteConsole(
            $"{LogPrefix} layout loaded ({layout.Props.Count} entries, room={MRActiveRoom.BoundRoomId ?? "global"}) path={LayoutFilePath}");
    }

    /// <summary>Drop in-memory layout so the next EnsureLayoutLoaded reads the current path (per-room bind).</summary>
    public void UnloadLayoutMemory()
    {
        layout = null;
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
        var spawnFirst = new List<MREnvironmentPlacement>();
        var spawnAfterParents = new List<MREnvironmentPlacement>();
        foreach (MREnvironmentPlacement placement in layout.GetProps())
        {
            if (placement == null || string.IsNullOrEmpty(placement.Id))
                continue;

            if (!placement.HasValidCatalogReference())
                continue;

            if (placement.SurfaceType == PlacementSurfaceType.Object
                && !string.IsNullOrEmpty(placement.AnchorPlacementId))
                spawnAfterParents.Add(placement);
            else
                spawnFirst.Add(placement);
        }

        foreach (MREnvironmentPlacement placement in spawnFirst)
        {
            yield return TrySpawnPlacementAsync(placement, mrSpaceOrigin, index, spawned =>
            {
                if (spawned)
                    index++;
            });
        }

        foreach (MREnvironmentPlacement placement in spawnAfterParents)
        {
            yield return TrySpawnPlacementAsync(placement, mrSpaceOrigin, index, spawned =>
            {
                if (spawned)
                    index++;
            });
        }

        ConfigManager.WriteConsole($"{LogPrefix} SpawnAllAsync done ({spawnedById.Count} props)");
    }

    public IEnumerator ReapplyRoomSkinsWhenReady()
    {
        yield return ReapplyActiveRoomSkinsFromLayout();
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
        MRRoomSurfaceSkin.Clear();
    }

    /// <summary>Delete in-memory layout and despawn all props (after layout YAML wipe).</summary>
    public void ClearLayoutAndDespawn()
    {
        DespawnAll();
        layout = MREnvironmentLayout.LoadOrCreate(LayoutFilePath);
        ConfigManager.WriteConsole($"{LogPrefix} layout cleared ({LayoutFilePath})");
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
        MRRoomSurfaceSkin.Clear();
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

            MRPlacementConfirmAnchor anchor = BuildAnchorFromPlacement(placement);

            Vector3 displayPos = entry.Value.transform.position;
            Quaternion rot = entry.Value.transform.rotation;
            WriteStoredPose(
                placement,
                placement.SurfaceType,
                displayPos,
                rot,
                anchor);
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

    /// <summary>
    /// Re-resolve layout poses after tracking discontinuity (e.g. Meta system menu).
    /// Pass 1: MRUK/world anchors. Pass 2: object-anchored props (need parent pose first).
    /// </summary>
    public void RefreshAllSpawnedPosesFromLayout(bool anchorsOnly = false)
    {
        if (layout == null)
            return;

        EnsureLayoutLoaded();
        int refreshed = RefreshSpawnedPosesPass(objectAnchorsOnly: false, anchorsOnly);
        refreshed += RefreshSpawnedPosesPass(objectAnchorsOnly: true, anchorsOnly);

        if (refreshed > 0)
            ConfigManager.WriteConsole($"{LogPrefix} refreshed {refreshed} env prop pose(s)");
    }

    public void ForEachSpawnedRoot(System.Action<Transform> action)
    {
        if (action == null)
            return;

        foreach (GameObject root in spawnedById.Values)
        {
            if (root != null)
                action(root.transform);
        }
    }

    int RefreshSpawnedPosesPass(bool objectAnchorsOnly, bool anchorsOnly)
    {
        int refreshed = 0;
        foreach (MREnvironmentPlacement placement in layout.GetProps())
        {
            if (placement == null || string.IsNullOrEmpty(placement.Id))
                continue;

            bool isObjectAnchor = placement.SurfaceType == PlacementSurfaceType.Object
                && !string.IsNullOrEmpty(placement.AnchorPlacementId);
            if (objectAnchorsOnly != isObjectAnchor)
                continue;

            if (!TryGetSpawnedRoot(placement.Id, out GameObject root) || root == null)
                continue;

            if (!TryReadWorldPose(placement, out Vector3 worldPos, out Quaternion worldRot, anchorsOnly))
                continue;

            Vector3 before = root.transform.position;
            root.transform.SetPositionAndRotation(worldPos, worldRot);
            ApplyRootScaleAfterSpawn(root, placement);
            NotifyPortableGamesPlacementUpdated(root);
            if (Vector3.Distance(before, worldPos) > 0.02f)
            {
                MRTransitionLog.Log(
                    $"Refresh env {placement.DisplayLabel} {before} -> {worldPos} anchor={placement.AnchorUuid ?? placement.AnchorPlacementId ?? "none"}");
            }

            refreshed++;
        }

        return refreshed;
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

    /// <summary>Total placed bookshelves in the MR layout (menu allows at most one).</summary>
    public int GetBookshelfInstanceCount()
    {
        EnsureLayoutLoaded();
        if (layout == null)
            return 0;

        int count = 0;
        foreach (MREnvironmentPlacement placement in layout.GetProps())
        {
            if (placement != null && placement.IsBookshelfSource)
                count++;
        }

        return count;
    }

    /// <summary>First placed bookshelf regardless of its stored issue list.</summary>
    public MREnvironmentPlacement FindFirstBookshelfPlacement()
    {
        EnsureLayoutLoaded();
        if (layout == null)
            return null;

        foreach (MREnvironmentPlacement placement in layout.GetProps())
        {
            if (placement != null && placement.IsBookshelfSource)
                return placement;
        }

        return null;
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

        bool wasRoomSkin = placement.IsRoomSkinSource;
        DestroySpawnedInstance(placementId);
        layout.RemoveById(placementId);
        layout.Save(LayoutFilePath);
        if (wasRoomSkin)
            ReapplyActiveRoomSkinsFromLayoutSync();

        ConfigManager.WriteConsole($"{LogPrefix} removed {placement.DisplayLabel} ({placementId})");
        return true;
    }

    public bool TryFinalizeRoomSkinInstant(MREnvironmentCatalogEntry entry)
    {
        EnsureLayoutLoaded();
        if (layout == null || string.IsNullOrEmpty(entry.Key))
            return false;

        if (!MRRoomSkinDefinition.TryLoad(entry.Key, out MRRoomSkinDefinition newDefinition))
            return false;

        RemoveOverlappingRoomSkinPlacements(newDefinition);

        string idSeed = entry.Key;
        var placement = new MREnvironmentPlacement
        {
            Id = $"{idSeed}-{Guid.NewGuid():N}".Substring(0, Mathf.Min(48, idSeed.Length + 33)),
            Source = "roomSkin",
            PackageName = entry.Key,
            Scale = 1f,
            Position = MRVector3.From(Vector3.zero),
            Rotation = MRQuaternion.From(Quaternion.identity),
            WorldPosition = MRVector3.From(Vector3.zero),
            WorldRotation = MRQuaternion.From(Quaternion.identity)
        };

        layout.AddPlacement(placement);
        layout.Save(LayoutFilePath);

        GameObject marker = new GameObject($"RoomSkin_{placement.Id}");
        marker.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        MRPlacedEnvironment placedMarker = marker.AddComponent<MRPlacedEnvironment>();
        placedMarker.Initialize(placement.Id, entry);
        spawnedById[placement.Id] = marker;

        ReapplyActiveRoomSkinsFromLayoutSync();

        ConfigManager.WriteConsole($"{LogPrefix} room skin applied {entry} ({placement.Id})");
        return true;
    }

    void RemoveOverlappingRoomSkinPlacements(MRRoomSkinDefinition newDefinition)
    {
        if (layout == null || newDefinition == null)
            return;

        var idsToRemove = new List<string>();
        foreach (MREnvironmentPlacement existing in layout.GetProps())
        {
            if (existing == null || !existing.IsRoomSkinSource)
                continue;

            if (!MRRoomSkinDefinition.TryLoad(existing.PackageName, out MRRoomSkinDefinition existingDefinition))
                continue;

            if (newDefinition.SharesAnySurfaceWith(existingDefinition))
                idsToRemove.Add(existing.Id);
        }

        foreach (string id in idsToRemove)
        {
            DestroySpawnedInstance(id);
            layout.RemoveById(id);
        }
    }

    public IEnumerator ReapplyActiveRoomSkinsFromLayout()
    {
        EnsureLayoutLoaded();
        if (layout == null)
            yield break;

        List<string> packageNames = CollectActiveRoomSkinPackageNames();
        if (packageNames.Count == 0)
        {
            if (MRRoomSurfaceSkin.IsActive)
                MRRoomSurfaceSkin.Clear();
            yield break;
        }

        MRRoomSurfaceSkin.Clear();

        MREffectMeshController controller = MixedRealityManager.Instance != null
            ? MixedRealityManager.Instance.GetComponent<MREffectMeshController>()
            : null;
        if (controller != null)
            yield return controller.WaitForAnchorMeshReady();
        else
            yield return null;

        foreach (string packageName in packageNames)
            MRRoomSurfaceSkin.TryApplyPackage(packageName);
    }

    void ReapplyActiveRoomSkinsFromLayoutSync()
    {
        EnsureLayoutLoaded();
        if (layout == null)
            return;

        List<string> packageNames = CollectActiveRoomSkinPackageNames();
        if (packageNames.Count == 0)
        {
            if (MRRoomSurfaceSkin.IsActive)
                MRRoomSurfaceSkin.Clear();
            return;
        }

        MRRoomSurfaceSkin.Clear();
        foreach (string packageName in packageNames)
            MRRoomSurfaceSkin.TryApplyPackage(packageName);
    }

    List<string> CollectActiveRoomSkinPackageNames()
    {
        var packageNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (layout == null)
            return new List<string>();

        foreach (MREnvironmentPlacement placement in layout.GetProps())
        {
            if (placement == null || !placement.IsRoomSkinSource)
                continue;

            if (string.IsNullOrEmpty(placement.PackageName))
                continue;

            packageNames.Add(placement.PackageName);
        }

        return new List<string>(packageNames);
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

        if (entry.Source == MREnvironmentObjectSource.Bookshelf)
            return TrySpawnBookshelfAtWorldPose(entry.GroupedKeys, worldPosition, worldRotation, out spawnedRoot);

        if (entry.Source == MREnvironmentObjectSource.Magazine)
            return TrySpawnMagazineAtWorldPose(entry.Key, worldPosition, worldRotation, out spawnedRoot);

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

        if (entry.Source == MREnvironmentObjectSource.Bookshelf)
        {
            if (TrySpawnBookshelfAtWorldPose(entry.GroupedKeys, worldPosition, worldRotation, out GameObject bookshelfRoot))
            {
                result.Root = bookshelfRoot;
                result.Success = true;
            }

            yield break;
        }

        if (entry.Source == MREnvironmentObjectSource.Magazine)
        {
            if (TrySpawnMagazineAtWorldPose(entry.Key, worldPosition, worldRotation, out GameObject magazineRoot))
            {
                result.Root = magazineRoot;
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
        MRPlacementConfirmAnchor anchor = default) =>
        TryFinalizeTransientCatalogEntry(
            MREnvironmentCatalogEntry.FromBuild(prefabName, prefabName),
            root,
            mrSpaceOrigin,
            worldPosition,
            worldRotation,
            anchor);

    public bool TryFinalizeTransientCatalogEntry(
        MREnvironmentCatalogEntry entry,
        GameObject root,
        Transform mrSpaceOrigin,
        Vector3 worldPosition,
        Quaternion worldRotation,
        MRPlacementConfirmAnchor anchor = default)
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
        PlacementSurfaceType surfaceType = anchor.IsObjectAnchor
            ? PlacementSurfaceType.Object
            : profile != null
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
                MREnvironmentObjectSource.RoomSkin => "roomSkin",
                MREnvironmentObjectSource.Bookshelf => "bookshelf",
                MREnvironmentObjectSource.Magazine => "magazine",
                _ => "build"
            },
            Scale = ResolveScaleFromRoot(root),
            SurfaceType = surfaceType,
            FacingAxis = facingAxis
        };

        if (entry.Source == MREnvironmentObjectSource.Custom)
            placement.PackageName = entry.Key;
        else if (entry.Source == MREnvironmentObjectSource.RoomSkin)
            placement.PackageName = entry.Key;
        else if (entry.Source == MREnvironmentObjectSource.Poster)
        {
            placement.TextureFile = entry.Key;
            placement.PrefabName = MRPosterFactory.PosterPrefabId;
        }
        else if (entry.Source == MREnvironmentObjectSource.Bookshelf)
        {
            placement.PrefabName = MREnvironmentCatalog.BookshelfPrefabName;
            placement.BookshelfIssueNames = entry.GroupedKeys != null
                ? new List<string>(entry.GroupedKeys)
                : new List<string>();
        }
        else if (entry.Source == MREnvironmentObjectSource.Magazine)
        {
            placement.MagazineIssueName = entry.Key;
            placement.PrefabName = MREnvironmentCatalog.MagazinePrefabName;
        }
        else
            placement.PrefabName = entry.Key;

        if (entry.Source == MREnvironmentObjectSource.Light)
            CaptureLightSettingsFromRoot(placement, root);

        WriteStoredPose(placement, surfaceType, worldPosition, worldRotation, anchor);

        layout.AddPlacement(placement);
        layout.Save(LayoutFilePath);

        ApplyRootScaleAfterFinalize(root, placement);
        MRShadowCasterPolicy.Apply(root);
        // Placement ray moves the root after grab Configure/Start captured home — refresh dock pose
        // so returnOnRelease snaps to the confirmed position, not the initial spawn pose.
        NotifyPortableGamesPlacementUpdated(root);

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
        MRPlacementConfirmAnchor anchor = default)
    {
        EnsureLayoutLoaded();
        if (layout == null || string.IsNullOrEmpty(placementId) || mrSpaceOrigin == null)
            return false;

        MREnvironmentPlacement placement = layout.FindById(placementId);
        if (placement == null)
            return false;

        PlacementSurfaceType surfaceType = anchor.IsObjectAnchor
            ? PlacementSurfaceType.Object
            : placement.SurfaceType;
        placement.SurfaceType = surfaceType;
        WriteStoredPose(placement, surfaceType, worldPosition, worldRotation, anchor);
        layout.Save(LayoutFilePath);

        if (TryGetSpawnedRoot(placementId, out GameObject root))
        {
            if (placement.IsPosterSource || placement.IsCustomSource)
            {
                float liveScale = ResolveScaleFromRoot(root);
                placement.Scale = placement.IsPosterSource
                    ? MRPosterPlacement.SnapScale(liveScale)
                    : MRCustomObjectPlacement.SnapScale(liveScale);
            }

            root.transform.SetPositionAndRotation(worldPosition, worldRotation);
            ApplyRootScaleAfterSpawn(root, placement);
            NotifyPortableGamesPlacementUpdated(root);
        }

        ConfigManager.WriteConsole(
            $"{LogPrefix} updated pose {placement.DisplayLabel} ({placementId}) scale={placement.Scale:F2}");
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
        else if (placement.IsBookshelfSource)
        {
            if (!TrySpawnBookshelfAtWorldPose(placement.BookshelfIssueNames, worldPos, worldRot, out root))
            {
                MRDebugLog.LogError($"Environment spawn failed: bookshelf '{placement.DisplayLabel}' ({placement.Id})");
                onComplete?.Invoke(false);
                yield break;
            }
        }
        else if (placement.IsMagazineSource)
        {
            if (!TrySpawnMagazineAtWorldPose(placement.MagazineIssueName, worldPos, worldRot, out root))
            {
                MRDebugLog.LogError($"Environment spawn failed: magazine '{placement.MagazineIssueName}' ({placement.Id})");
                onComplete?.Invoke(false);
                yield break;
            }
        }
        else if (placement.IsRoomSkinSource)
        {
            if (!TrySpawnRoomSkinMarker(placement, entry: MREnvironmentCatalogEntry.FromRoomSkin(
                    placement.PackageName,
                    placement.DisplayLabel),
                out root))
            {
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
        MRShadowCasterPolicy.Apply(root);
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
                    : placement.IsRoomSkinSource
                        ? MREnvironmentCatalogEntry.FromRoomSkin(placement.PackageName, placement.DisplayLabel)
                        : placement.IsBookshelfSource
                            ? MREnvironmentCatalogEntry.FromBookshelf(
                                placement.PrefabName,
                                placement.DisplayLabel,
                                placement.BookshelfIssueNames)
                        : placement.IsMagazineSource
                            ? MREnvironmentCatalogEntry.FromMagazine(placement.MagazineIssueName, placement.DisplayLabel)
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

    bool TrySpawnMagazineAtWorldPose(
        string issueName,
        Vector3 worldPos,
        Quaternion worldRot,
        out GameObject spawnedRoot)
    {
        spawnedRoot = null;
        if (string.IsNullOrEmpty(issueName) || !MRMagazineCatalog.IssueExists(issueName))
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} magazine issue missing page images: {issueName}");
            MRDebugLog.LogError($"Magazine issue has no page images: {issueName}");
            return false;
        }

        GameObject prefab = MREnvironmentCatalog.LoadMagazinePrefab();
        if (prefab == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} magazine prefab missing: {MREnvironmentCatalog.MagazinePrefabName}");
            MRDebugLog.LogError($"Magazine prefab missing: {MREnvironmentCatalog.MagazinePrefabName}");
            return false;
        }

        spawnedRoot = Instantiate(prefab, worldPos, worldRot);
        spawnedRoot.name = $"{MREnvironmentCatalog.MagazinePrefabName}_{issueName}";
        spawnedRoot.transform.SetParent(null, worldPositionStays: true);

        Magazine magazine = spawnedRoot.GetComponent<Magazine>();
        magazine?.ConfigureIssue(issueName);

        NotifyPortableGamesPlacementUpdated(spawnedRoot);
        return true;
    }

    bool TrySpawnBookshelfAtWorldPose(
        IReadOnlyList<string> issueNames,
        Vector3 worldPos,
        Quaternion worldRot,
        out GameObject spawnedRoot)
    {
        spawnedRoot = null;
        if (!MRBookshelfFactory.TryInstantiate(issueNames, worldPos, worldRot, out spawnedRoot))
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} bookshelf failed to spawn");
            MRDebugLog.LogError("Bookshelf failed to spawn");
            return false;
        }

        NotifyPortableGamesPlacementUpdated(spawnedRoot);
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

    public bool TryGetPosterScale(string placementId, out float scale)
    {
        EnsureLayoutLoaded();
        scale = MRPosterPlacement.DefaultScale;
        if (layout == null || string.IsNullOrEmpty(placementId))
            return false;

        MREnvironmentPlacement placement = layout.FindById(placementId);
        if (placement == null || !placement.IsPosterSource)
            return false;

        scale = placement.Scale > 0f ? placement.Scale : MRPosterPlacement.DefaultScale;
        return true;
    }

    public bool TryUpdatePosterScale(string placementId, float scale)
    {
        EnsureLayoutLoaded();
        if (layout == null || string.IsNullOrEmpty(placementId))
            return false;

        MREnvironmentPlacement placement = layout.FindById(placementId);
        if (placement == null || !placement.IsPosterSource)
            return false;

        placement.Scale = MRPosterPlacement.SnapScale(scale);
        layout.Save(LayoutFilePath);

        if (TryGetSpawnedRoot(placement.Id, out GameObject root))
            MRPosterPlacement.ApplyUserScale(root, placement.Scale);

        ConfigManager.WriteConsole(
            $"{LogPrefix} poster scale {placement.DisplayLabel} yz={placement.Scale:F2}");
        return true;
    }

    public bool TryGetCustomObjectScale(string placementId, out float scale)
    {
        EnsureLayoutLoaded();
        scale = MRCustomObjectPlacement.DefaultScale;
        if (layout == null || string.IsNullOrEmpty(placementId))
            return false;

        MREnvironmentPlacement placement = layout.FindById(placementId);
        if (placement == null || !placement.IsCustomSource)
            return false;

        scale = ResolveSpawnScale(placement);
        return true;
    }

    public bool TryUpdateCustomObjectScale(string placementId, float scale)
    {
        EnsureLayoutLoaded();
        if (layout == null || string.IsNullOrEmpty(placementId))
            return false;

        MREnvironmentPlacement placement = layout.FindById(placementId);
        if (placement == null || !placement.IsCustomSource)
            return false;

        placement.Scale = MRCustomObjectPlacement.SnapScale(scale);
        layout.Save(LayoutFilePath);

        if (TryGetSpawnedRoot(placement.Id, out GameObject root))
        {
            MRCustomObjectPlacement.ApplyUserScale(root, placement.Scale);
            NotifyPortableGamesPlacementUpdated(root);
        }

        ConfigManager.WriteConsole(
            $"{LogPrefix} custom object scale {placement.DisplayLabel} xyz={placement.Scale:F2}");
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

        MagazineGrab magazineGrab = root.GetComponentInChildren<MagazineGrab>(true);
        magazineGrab?.NotifyPlacementPoseUpdated();

        MRCustomObjectGrab customGrab = root.GetComponentInChildren<MRCustomObjectGrab>(true);
        if (magazineGrab == null)
            customGrab?.NotifyPlacementPoseUpdated();
    }

    static bool TrySpawnRoomSkinMarker(MREnvironmentPlacement placement, MREnvironmentCatalogEntry entry, out GameObject root)
    {
        root = null;
        if (placement == null || string.IsNullOrEmpty(placement.PackageName))
            return false;

        root = new GameObject($"RoomSkin_{placement.Id}");
        root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        MRPlacedEnvironment marker = root.AddComponent<MRPlacedEnvironment>();
        marker.Initialize(placement.Id, entry);
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

    static MRPlacementConfirmAnchor BuildAnchorFromPlacement(MREnvironmentPlacement placement)
    {
        if (placement != null
            && placement.SurfaceType == PlacementSurfaceType.Object
            && !string.IsNullOrEmpty(placement.AnchorPlacementId))
        {
            return MRPlacementConfirmAnchor.FromObject(
                placement.AnchorPlacementId,
                placement.AnchorPoint);
        }

        if (placement != null
            && !string.IsNullOrEmpty(placement.AnchorUuid)
            && Guid.TryParse(placement.AnchorUuid, out Guid anchorUuid))
            return MRPlacementConfirmAnchor.FromMruk(anchorUuid);

        return default;
    }

    static void WriteStoredPose(
        MREnvironmentPlacement placement,
        PlacementSurfaceType surfaceType,
        Vector3 worldPosition,
        Quaternion worldRotation,
        MRPlacementConfirmAnchor anchor)
    {
        placement.WorldPosition = MRVector3.From(worldPosition);
        placement.WorldRotation = MRQuaternion.From(worldRotation);
        placement.SurfaceType = surfaceType;

        if (anchor.IsObjectAnchor
            && Instance != null
            && Instance.TryGetSpawnedRoot(anchor.ObjectPlacementId, out GameObject parentRoot))
        {
            if (MRObjectAnchorPoseResolver.TryWriteAnchorRelativePose(
                    parentRoot,
                    anchor.ObjectAnchorPoint,
                    worldPosition,
                    worldRotation,
                    out string storedAnchorPoint,
                    out MRVector3 storedPosition,
                    out MRQuaternion storedRotation))
            {
                placement.AnchorPlacementId = anchor.ObjectPlacementId;
                placement.AnchorPoint = storedAnchorPoint;
                placement.AnchorUuid = null;
                placement.Position = storedPosition;
                placement.Rotation = storedRotation;
                return;
            }
        }

        placement.AnchorPlacementId = null;
        placement.AnchorPoint = null;

        Meta.XR.MRUtilityKit.MRUKRoom room = MREnvironmentSurfaces.Instance?.CurrentRoom;
        if (MRAnchorPoseResolver.TryWriteAnchorRelativePose(
                room,
                surfaceType,
                worldPosition,
                worldRotation,
                anchor.MrukUuid,
                out string anchorUuidText,
                out MRVector3 mrukStoredPosition,
                out MRQuaternion mrukStoredRotation))
        {
            placement.AnchorUuid = anchorUuidText;
            placement.Position = mrukStoredPosition;
            placement.Rotation = mrukStoredRotation;
            return;
        }

        placement.AnchorUuid = null;
        placement.Position = MRVector3.From(worldPosition);
        placement.Rotation = MRQuaternion.From(worldRotation);
    }

    static bool TryReadWorldPose(
        MREnvironmentPlacement placement,
        out Vector3 worldPosition,
        out Quaternion worldRotation,
        bool anchorsOnly = false)
    {
        worldPosition = Vector3.zero;
        worldRotation = Quaternion.identity;

        if (placement == null)
            return false;

        if (placement.SurfaceType == PlacementSurfaceType.Object
            && !string.IsNullOrEmpty(placement.AnchorPlacementId)
            && Instance != null
            && Instance.TryGetSpawnedRoot(placement.AnchorPlacementId, out GameObject parentRoot))
        {
            Vector3 storedPosition = placement.Position != null ? placement.Position.ToVector3() : Vector3.zero;
            Quaternion storedRotation = placement.Rotation != null ? placement.Rotation.ToQuaternion() : Quaternion.identity;
            if (MRObjectAnchorPoseResolver.TryResolveWorldPose(
                    parentRoot,
                    placement.AnchorPoint,
                    storedPosition,
                    storedRotation,
                    out worldPosition,
                    out worldRotation))
                return true;

            if (anchorsOnly)
                return false;

            if (TryReadWorldFallback(placement, out worldPosition, out worldRotation))
            {
                ConfigManager.WriteConsoleWarning(
                    $"{LogPrefix} {placement.DisplayLabel} object anchor unresolved — using world fallback {worldPosition}");
                return true;
            }

            return false;
        }

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

            if (anchorsOnly)
                return false;

            if (TryReadWorldFallback(placement, out worldPosition, out worldRotation))
                return true;

            return false;
        }

        if (anchorsOnly)
            return false;

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
        if (root != null)
        {
            MRWallPoster poster = root.GetComponent<MRWallPoster>();
            if (poster != null)
                return poster.UserScale;
        }

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
                poster.SetUserScale(placement.Scale > 0f ? placement.Scale : MRPosterPlacement.DefaultScale);
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
                poster.SetUserScale(ResolveSpawnScale(placement));
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
