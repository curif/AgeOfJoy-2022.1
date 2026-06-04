/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// MR cabinet layout (mr-layout.yaml). Phase 2b: load/save, spawn, delete.
/// Placement Position/Rotation are anchor-local when AnchorUuid is set (v3+), otherwise world space (v2).
/// VR registry.yaml is never modified by this class.
/// </summary>
public class MRLayoutRegistry : MonoBehaviour
{
    const string LogPrefix = "[MRLayoutRegistry]";
    public const string LayoutFileName = "mr-layout.yaml";
    public const int WorldSpaceLayoutVersion = 2;
    public const int AnchorRelativeLayoutVersion = 3;

    public static MRLayoutRegistry Instance { get; private set; }

    readonly Dictionary<string, GameObject> spawnedById = new Dictionary<string, GameObject>();
    MRLayout layout;

    public string LayoutFilePath => Path.Combine(ConfigManager.CabinetsDB, LayoutFileName);

    public int SpawnedCount => spawnedById.Count;

    public IReadOnlyList<MRCabinetPlacement> Placements =>
        layout != null ? layout.GetCabinets() : System.Array.Empty<MRCabinetPlacement>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        MRLibretroWarmup.EnsureOnMainThread();
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

        layout = MRLayout.LoadOrCreate(LayoutFilePath);
        MigrateLayoutWorldFallback(layout, LayoutFilePath);
        ConfigManager.WriteConsole($"{LogPrefix} layout loaded ({layout.Cabinets.Count} entries)");
    }

    /// <summary>Cache live world transforms before MR→VR hide/destroy (anchor UUID may not resolve on next entry).</summary>
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

            MRCabinetPlacement placement = layout.FindById(entry.Key);
            if (placement == null)
                continue;

            Vector3 displayPos = entry.Value.transform.position;
            Quaternion rot = entry.Value.transform.rotation;
            Vector3 storagePos = WorldPositionForStorage(placement.SurfaceType, displayPos);
            Guid anchorUuid = Guid.Empty;
            if (!string.IsNullOrEmpty(placement.AnchorUuid))
                Guid.TryParse(placement.AnchorUuid, out anchorUuid);

            WriteStoredPose(placement, placement.SurfaceType, storagePos, rot, anchorUuid);
            changed = true;
            MRTransitionLog.Log(
                $"SnapshotWorldPose {placement.DisplayLabel} display={displayPos} storage={storagePos} anchor={placement.AnchorUuid ?? "none"}");
        }

        if (changed)
        {
            layout.Save(LayoutFilePath);
            MRTransitionLog.Log($"SnapshotSpawnedWorldPosesToLayout count={spawnedById.Count}");
        }
    }

    static void MigrateLayoutWorldFallback(MRLayout loaded, string layoutFilePath)
    {
        if (loaded == null)
            return;

        bool changed = false;
        foreach (MRCabinetPlacement placement in loaded.GetCabinets())
        {
            if (placement?.WorldPosition != null || placement?.Position == null)
                continue;
            if (!string.IsNullOrEmpty(placement.AnchorUuid))
                continue;

            placement.WorldPosition = placement.Position;
            placement.WorldRotation = placement.Rotation;
            changed = true;
        }

        if (changed)
            loaded.Save(layoutFilePath);
    }

    /// <summary>Spawn all entries from mr-layout.yaml under MRSpaceOrigin.</summary>
    public void SpawnAll(Transform mrSpaceOrigin)
    {
        DespawnAll();
        EnsureLayoutLoaded();

        if (mrSpaceOrigin == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} SpawnAll skipped — MRSpaceOrigin is null");
            return;
        }

        if (layout.Cabinets.Count == 0)
        {
            ConfigManager.WriteConsole($"{LogPrefix} SpawnAll: layout empty ({LayoutFilePath})");
            return;
        }

        EnsureWorldSpaceLayout(mrSpaceOrigin);

        int index = 0;
        foreach (MRCabinetPlacement placement in layout.GetCabinets())
        {
            if (placement == null || string.IsNullOrEmpty(placement.Id))
                continue;

            if (TrySpawnPlacement(placement, mrSpaceOrigin, index))
                index++;
        }

        ConfigManager.WriteConsole($"{LogPrefix} SpawnAll done ({spawnedById.Count} cabinets)");
    }

    public void DespawnAll(bool stopLibretroFirst = true)
    {
        foreach (GameObject root in CollectAllMrCabinetRoots())
        {
            if (root == null)
                continue;

            if (stopLibretroFirst)
                StopLibretroOnCabinet(root);

            var replace = root.GetComponent<CabinetReplace>();
            if (replace != null)
                Destroy(replace.gameObject);
            else
                Destroy(root);
        }

        spawnedById.Clear();
        ConfigManager.WriteConsole($"{LogPrefix} DespawnAll done (stopLibretro={stopLibretroFirst})");
    }

    /// <summary>Instant hide for every MR cabinet (registered, transient, validation).</summary>
    public void HideAllMrCabinetsImmediate()
    {
        int hidden = HideAllMrCabinetsImmediateCount();
        if (hidden > 0)
            ConfigManager.WriteConsole($"{LogPrefix} hid {hidden} MR cabinet(s)");
    }

    public int HideAllMrCabinetsImmediateCount()
    {
        var roots = CollectAllMrCabinetRoots();
        int hidden = 0;
        foreach (GameObject root in roots)
        {
            if (root == null || !root.activeSelf)
                continue;
            root.SetActive(false);
            hidden++;
        }

        MRTransitionLog.Log($"HideAllMrCabinetsImmediate roots={roots.Count} hidden={hidden} spawnedById={spawnedById.Count}");
        return hidden;
    }

    /// <summary>End Libretro on MR cabinets while screen components still exist (before hide/destroy).</summary>
    public void StopAllMrLibretroGames()
    {
        foreach (GameObject root in CollectAllMrCabinetRoots())
            StopLibretroOnCabinet(root);
    }

    /// <summary>Restart attract-mode loops after MR spawn (Start may have run before wiring).</summary>
    public void EnsureAttractPlaybackOnSpawned()
    {
        foreach (GameObject root in spawnedById.Values)
        {
            if (root == null)
                continue;

            foreach (LibretroScreenController screen in root.GetComponentsInChildren<LibretroScreenController>(true))
                screen.EnsureAttractLoopRunning();
        }
    }

    /// <summary>Hide then destroy MR cabinets over several frames so Libretro OnDestroy does not block VR reload.</summary>
    public IEnumerator DespawnAllAsync(bool stopLibretroFirst = false)
    {
        var roots = CollectAllMrCabinetRoots();
        MRTransitionLog.LogStep("DespawnAllAsync", $"begin roots={roots.Count} stopLibretro={stopLibretroFirst}");
        foreach (GameObject root in roots)
        {
            if (root != null)
                root.SetActive(false);
        }

        yield return null;
        yield return null;
        MRTransitionLog.LogStep("DespawnAllAsync", "after 2 hide frames");

        int destroyed = 0;
        foreach (GameObject root in roots)
        {
            if (root == null)
                continue;

            MRTransitionLog.Log($"DespawnAllAsync destroying {root.name}");
            if (stopLibretroFirst)
                StopLibretroOnCabinet(root);

            var replace = root.GetComponent<CabinetReplace>();
            if (replace != null)
                Destroy(replace.gameObject);
            else
                Destroy(root);

            destroyed++;
            yield return null;
        }

        spawnedById.Clear();
        MRTransitionLog.LogStep("DespawnAllAsync", $"done destroyed={destroyed}");
        ConfigManager.WriteConsole($"{LogPrefix} DespawnAllAsync done ({roots.Count} cabinets)");
    }

    List<GameObject> CollectAllMrCabinetRoots()
    {
        var roots = new HashSet<GameObject>();

        foreach (GameObject root in spawnedById.Values)
        {
            if (root != null)
                roots.Add(root);
        }

        foreach (MRPlacedCabinet placed in FindObjectsOfType<MRPlacedCabinet>(true))
        {
            if (placed == null)
                continue;
            roots.Add(ResolveCabinetRoot(placed.gameObject));
        }

        foreach (CabinetReplace replace in FindObjectsOfType<CabinetReplace>(true))
        {
            if (replace == null || replace.game == null)
                continue;
            if (replace.game.Room != MixedRealityManager.MrRoomName)
                continue;
            roots.Add(replace.gameObject);
        }

        return roots.ToList();
    }

    static GameObject ResolveCabinetRoot(GameObject go)
    {
        if (go == null)
            return null;

        CabinetReplace replace = go.GetComponentInParent<CabinetReplace>();
        return replace != null ? replace.gameObject : go;
    }

    public bool RemovePlacement(string placementId)
    {
        EnsureLayoutLoaded();
        if (string.IsNullOrEmpty(placementId))
            return false;

        MRCabinetPlacement placement = layout.FindById(placementId);
        if (placement == null)
            return false;

        DestroySpawnedInstance(placementId);
        layout.RemoveById(placementId);
        layout.Save(LayoutFilePath);
        ConfigManager.WriteConsole($"{LogPrefix} removed {placement.DisplayLabel} ({placementId})");
        return true;
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

    /// <summary>True when the cabinet has an entry in mr-layout (placed in MR space).</summary>
    public bool IsCabinetInScene(string cabinetDBName) =>
        FindPlacementByCabinetDBName(cabinetDBName) != null;

    public MRCabinetPlacement FindPlacementByCabinetDBName(string cabinetDBName)
    {
        EnsureLayoutLoaded();
        return layout?.FindByCabinetDBName(cabinetDBName);
    }

    public bool TryGetPlacementById(string placementId, out MRCabinetPlacement placement)
    {
        EnsureLayoutLoaded();
        placement = layout?.FindById(placementId);
        return placement != null;
    }

    /// <summary>All cabinet folders under cabinetsdb (same rule as GameRegistry: every subfolder).</summary>
    public static List<string> GetCatalogCabinetNames()
    {
        var names = new List<string>();
        string dbPath = ConfigManager.CabinetsDB;

        ConfigManager.CreateFolder(ConfigManager.BaseDir);
        ConfigManager.CreateFolder(dbPath);

        GameRegistry.ReloadCabinetDirectoriesFromDisk();
        if (GameRegistry.cabinetDirectories != null && GameRegistry.cabinetDirectories.Length > 0)
        {
            names.AddRange(GameRegistry.cabinetDirectories);
            ConfigManager.WriteConsole($"{LogPrefix} catalog {names.Count} cabinet(s) (GameRegistry) from {dbPath}");
            return names;
        }

        if (!Directory.Exists(dbPath))
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} CabinetsDB missing: {dbPath}");
            return names;
        }

        foreach (string dir in Directory.GetDirectories(dbPath).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            string folderName = Path.GetFileName(dir);
            if (!IsCatalogFolderName(folderName))
                continue;
            names.Add(folderName);
        }

        ConfigManager.WriteConsole($"{LogPrefix} catalog {names.Count} cabinet(s) (scan) from {dbPath}");
        return names;
    }

    static bool IsCatalogFolderName(string folderName)
    {
        if (string.IsNullOrEmpty(folderName))
            return false;
        if (folderName.StartsWith("."))
            return false;
        if (string.Equals(folderName, Path.GetFileNameWithoutExtension(LayoutFileName), StringComparison.OrdinalIgnoreCase))
            return false;
        return true;
    }

    /// <summary>Add to mr-layout.yaml and spawn in the MR space (MVP: one instance per cabinetDBName).</summary>
    public bool TryAddCabinetToScene(
        string cabinetDBName,
        Transform mrSpaceOrigin,
        Vector3 worldPosition,
        Quaternion worldRotation,
        Guid anchorUuid = default)
    {
        EnsureLayoutLoaded();
        if (layout == null || string.IsNullOrEmpty(cabinetDBName) || mrSpaceOrigin == null)
            return false;

        if (FindPlacementByCabinetDBName(cabinetDBName) != null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} already in layout: {cabinetDBName}");
            return false;
        }

        var placement = new MRCabinetPlacement
        {
            Id = $"{cabinetDBName}-{Guid.NewGuid():N}".Substring(0, Mathf.Min(48, cabinetDBName.Length + 33)),
            CabinetDBName = cabinetDBName,
            Scale = 1f,
            SurfaceType = PlacementSurfaceType.Floor
        };
        WriteStoredPose(
            placement,
            PlacementSurfaceType.Floor,
            WorldPositionForStorage(PlacementSurfaceType.Floor, worldPosition),
            worldRotation,
            anchorUuid);
        layout.AddPlacement(placement);
        layout.Version = AnchorRelativeLayoutVersion;
        layout.Save(LayoutFilePath);

        int index = spawnedById.Count;
        if (!TrySpawnPlacement(placement, mrSpaceOrigin, index))
        {
            layout.RemoveById(placement.Id);
            layout.Save(LayoutFilePath);
            return false;
        }

        ConfigManager.WriteConsole($"{LogPrefix} added {cabinetDBName} ({placement.Id})");
        return true;
    }

    /// <summary>Spawn a game cabinet for floor placement ray — not saved to mr-layout until finalized.</summary>
    public bool TrySpawnTransientCabinet(
        string cabinetDBName,
        Transform mrSpaceOrigin,
        Vector3 worldPosition,
        Quaternion worldRotation,
        out GameObject spawnedRoot)
    {
        spawnedRoot = null;
        if (string.IsNullOrEmpty(cabinetDBName) || mrSpaceOrigin == null)
            return false;

        if (FindPlacementByCabinetDBName(cabinetDBName) != null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} already in layout: {cabinetDBName}");
            return false;
        }

        int index = spawnedById.Count;
        if (!TrySpawnCabinetAtWorldPose(
                cabinetDBName,
                mrSpaceOrigin,
                worldPosition,
                worldRotation,
                index,
                registerSpawned: false,
                out spawnedRoot))
            return false;

        ConfigManager.WriteConsole($"{LogPrefix} transient spawn {cabinetDBName} for placement ray");
        return true;
    }

    /// <summary>TestMRmanager / editor: spawn without mr-layout entry or duplicate check.</summary>
    public bool TrySpawnValidationCabinet(
        string cabinetDBName,
        Transform mrSpaceOrigin,
        Vector3 worldPosition,
        Quaternion worldRotation,
        out GameObject spawnedRoot)
    {
        spawnedRoot = null;
        if (string.IsNullOrEmpty(cabinetDBName) || mrSpaceOrigin == null)
            return false;

        return TrySpawnCabinetAtWorldPose(
            cabinetDBName,
            mrSpaceOrigin,
            worldPosition,
            worldRotation,
            spawnedById.Count,
            registerSpawned: false,
            out spawnedRoot);
    }

    /// <summary>Commit a transient cabinet after floor placement ray confirm.</summary>
    public bool TryFinalizeTransientCabinetAdd(
        string cabinetDBName,
        GameObject root,
        Transform mrSpaceOrigin,
        Vector3 worldPosition,
        Quaternion worldRotation,
        Guid anchorUuid = default)
    {
        EnsureLayoutLoaded();
        if (layout == null || string.IsNullOrEmpty(cabinetDBName) || root == null || mrSpaceOrigin == null)
            return false;

        if (FindPlacementByCabinetDBName(cabinetDBName) != null)
        {
            DestroyTransientCabinet(root);
            ConfigManager.WriteConsoleWarning($"{LogPrefix} finalize skipped, already in layout: {cabinetDBName}");
            return false;
        }

        var placement = new MRCabinetPlacement
        {
            Id = $"{cabinetDBName}-{Guid.NewGuid():N}".Substring(0, Mathf.Min(48, cabinetDBName.Length + 33)),
            CabinetDBName = cabinetDBName,
            Scale = 1f,
            SurfaceType = PlacementSurfaceType.Floor,
            FacingAxis = PlacementFacingAxis.PositiveZ
        };
        WriteStoredPose(
            placement,
            PlacementSurfaceType.Floor,
            WorldPositionForStorage(PlacementSurfaceType.Floor, worldPosition),
            worldRotation,
            anchorUuid);

        layout.AddPlacement(placement);
        layout.Version = AnchorRelativeLayoutVersion;
        layout.Save(LayoutFilePath);

        root.transform.localScale = Vector3.one * GetEffectiveCabinetScale(placement);

        MRPlacedCabinet marker = root.GetComponent<MRPlacedCabinet>();
        if (marker == null)
            marker = root.AddComponent<MRPlacedCabinet>();
        marker.Initialize(placement.Id, cabinetDBName);

        spawnedById[placement.Id] = root;
        ConfigManager.WriteConsole($"{LogPrefix} added {cabinetDBName} ({placement.Id}) after placement ray");
        return true;
    }

    public void DestroyTransientCabinet(GameObject root)
    {
        if (root == null)
            return;

        StopLibretroOnCabinet(root);

        CabinetReplace replace = root.GetComponent<CabinetReplace>();
        if (replace != null)
            Destroy(replace.gameObject);
        else
            Destroy(root);

        ConfigManager.WriteConsole($"{LogPrefix} destroyed transient cabinet {root.name}");
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

        MRCabinetPlacement placement = layout.FindById(placementId);
        if (placement == null)
            return false;

        Vector3 storedPosition = WorldPositionForStorage(placement.SurfaceType, worldPosition);
        WriteStoredPose(placement, placement.SurfaceType, storedPosition, worldRotation, anchorUuid);
        layout.Version = AnchorRelativeLayoutVersion;
        layout.Save(LayoutFilePath);

        if (TryGetSpawnedRoot(placementId, out GameObject root))
        {
            root.transform.SetPositionAndRotation(worldPosition, worldRotation);
            if (placement.SurfaceType == PlacementSurfaceType.Floor)
                root.transform.localScale = Vector3.one * GetEffectiveCabinetScale(placement);
        }

        ConfigManager.WriteConsole($"{LogPrefix} updated pose {placement.DisplayLabel} ({placementId})");
        return true;
    }

    /// <summary>Remove from layout and despawn if present.</summary>
    public bool TryRemoveCabinetFromScene(string cabinetDBName)
    {
        EnsureLayoutLoaded();
        MRCabinetPlacement placement = FindPlacementByCabinetDBName(cabinetDBName);
        if (placement == null || string.IsNullOrEmpty(placement.Id))
            return false;

        return RemovePlacement(placement.Id);
    }

    bool TrySpawnPlacement(MRCabinetPlacement placement, Transform mrSpaceOrigin, int index)
    {
        if (string.IsNullOrEmpty(placement.CabinetDBName))
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} skip entry {placement.Id}: missing cabinetDBName");
            return false;
        }

        if (!TryReadWorldPose(placement, out Vector3 worldPos, out Quaternion worldRot))
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} defer spawn {placement.DisplayLabel} — anchor pose not ready yet");
            return false;
        }

        ApplyFloorCabinetDisplayOffset(placement.SurfaceType, ref worldPos);

        if (!TrySpawnCabinetAtWorldPose(
                placement.CabinetDBName,
                mrSpaceOrigin,
                worldPos,
                worldRot,
                index,
                registerSpawned: true,
                out GameObject root,
                placement.FacingAxis))
            return false;

        root.transform.localScale = Vector3.one * GetEffectiveCabinetScale(placement);

        MRPlacedCabinet marker = root.GetComponent<MRPlacedCabinet>();
        if (marker == null)
            marker = root.AddComponent<MRPlacedCabinet>();
        marker.Initialize(placement.Id, placement.CabinetDBName);

        spawnedById[placement.Id] = root;
        BackfillWorldPoseCache(placement, worldPos, worldRot);
        ConfigManager.WriteConsole($"{LogPrefix} spawned {placement.DisplayLabel} at {worldPos}");
        MRTransitionLog.Log(
            $"Spawn {placement.DisplayLabel} pos={worldPos} rotY={worldRot.eulerAngles.y:F1} anchor={placement.AnchorUuid ?? "none"}");
        return true;
    }

    static void BackfillWorldPoseCache(MRCabinetPlacement placement, Vector3 displayWorldPos, Quaternion worldRot)
    {
        if (placement == null || placement.WorldPosition != null)
            return;

        Vector3 storagePos = WorldPositionForStorage(placement.SurfaceType, displayWorldPos);
        placement.WorldPosition = MRVector3.From(storagePos);
        placement.WorldRotation = MRQuaternion.From(worldRot);
    }

    bool TrySpawnCabinetAtWorldPose(
        string cabinetDBName,
        Transform mrSpaceOrigin,
        Vector3 worldPos,
        Quaternion worldRot,
        int index,
        bool registerSpawned,
        out GameObject spawnedRoot,
        PlacementFacingAxis facingAxis = PlacementFacingAxis.PositiveZ)
    {
        spawnedRoot = null;
        if (string.IsNullOrEmpty(cabinetDBName) || mrSpaceOrigin == null)
            return false;

        CabinetInformation cabInfo;
        try
        {
            cabInfo = CabinetInformation.fromYaml(Path.Combine(ConfigManager.CabinetsDB, cabinetDBName));
        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleException($"{LogPrefix} yaml load failed for {cabinetDBName}", e);
            return false;
        }

        if (cabInfo == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} no description for {cabinetDBName}");
            return false;
        }

        MRLibretroWarmup.EnsureOnMainThread();

        Cabinet cabinet;
        try
        {
            cabinet = CabinetFactory.fromInformation(
                cabInfo,
                MixedRealityManager.MrRoomName,
                index,
                worldPos,
                worldRot,
                null,
                agentPlayerPositions: new List<AgentScenePosition>(),
                backgroundSoundController: null);
        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleException($"{LogPrefix} spawn failed {cabinetDBName}", e);
            return false;
        }

        if (cabinet == null)
            return false;

        ApplyCabinetSkinning(cabinet, cabInfo);

        spawnedRoot = cabinet.gameObject;
        DisableAutoFloorSnap(spawnedRoot);
        spawnedRoot.transform.localScale = Vector3.one * MRAdjustmentsSettings.CabinetScale;
        MRGameCabinetAttractSetup.AttachAttractZone(
            spawnedRoot, cabinet, cabInfo, cabinetDBName, index, facingAxis);

        if (!registerSpawned)
            ConfigManager.WriteConsole($"{LogPrefix} spawned transient {cabinetDBName} at {worldPos}");

        return true;
    }

    static void ApplyCabinetSkinning(Cabinet cabinet, CabinetInformation cabInfo)
    {
        if (cabinet == null || cabInfo?.Parts == null || cabInfo.Parts.Count == 0)
            return;

        try
        {
            CabinetFactory.skinFromInformation(cabinet, cabInfo);
            ConfigManager.WriteConsole($"{LogPrefix} skinned {cabInfo.name} ({cabInfo.Parts.Count} parts)");
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"{LogPrefix} skinning failed for {cabInfo.name}", e);
        }
    }

    public void ApplyGlobalAdjustmentsToSpawnedFloorCabinets()
    {
        RefreshAllSpawnedPosesFromLayout(floorOnly: true);
    }

    /// <summary>Re-resolve layout poses after MRUK anchors register (e.g. second MR entry).</summary>
    public void RefreshAllSpawnedPosesFromLayout(bool floorOnly = false)
    {
        MRAdjustmentsSettings.EnsureLoaded();
        if (layout == null)
            return;

        EnsureLayoutLoaded();
        int refreshed = 0;
        int spawned = 0;

        foreach (MRCabinetPlacement placement in layout.GetCabinets())
        {
            if (placement == null || string.IsNullOrEmpty(placement.Id))
                continue;

            if (floorOnly && placement.SurfaceType != PlacementSurfaceType.Floor)
                continue;

            if (!TryReadWorldPose(placement, out Vector3 worldPos, out Quaternion worldRot))
                continue;

            ApplyFloorCabinetDisplayOffset(placement.SurfaceType, ref worldPos);

            if (spawnedById.TryGetValue(placement.Id, out GameObject root) && root != null)
            {
                Vector3 before = root.transform.position;
                root.transform.SetPositionAndRotation(worldPos, worldRot);
                root.transform.localScale = Vector3.one * GetEffectiveCabinetScale(placement);
                if (Vector3.Distance(before, worldPos) > 0.02f)
                {
                    MRTransitionLog.Log(
                        $"Refresh {placement.DisplayLabel} {before} -> {worldPos} anchor={placement.AnchorUuid ?? "none"}");
                }

                refreshed++;
                continue;
            }

            int index = spawnedById.Count;
            if (TrySpawnPlacement(placement, MixedRealityManager.Instance?.MRSpaceOrigin, index))
                spawned++;
        }

        if (refreshed > 0 || spawned > 0)
            ConfigManager.WriteConsole($"{LogPrefix} refreshed {refreshed} spawned {spawned} cabinet pose(s)");
    }

    public static float GetEffectiveCabinetScale(MRCabinetPlacement placement)
    {
        MRAdjustmentsSettings.EnsureLoaded();
        float baseScale = placement != null && placement.Scale > 0f ? placement.Scale : 1f;
        return baseScale * MRAdjustmentsSettings.CabinetScale;
    }

    public static Vector3 WorldPositionForStorage(PlacementSurfaceType surfaceType, Vector3 worldPosition)
    {
        MRAdjustmentsSettings.EnsureLoaded();
        if (surfaceType == PlacementSurfaceType.Floor)
            worldPosition.y -= MRAdjustmentsSettings.FloorCabinetYOffset;
        return worldPosition;
    }

    public static void ApplyFloorCabinetDisplayOffset(PlacementSurfaceType surfaceType, ref Vector3 worldPosition)
    {
        MRAdjustmentsSettings.EnsureLoaded();
        if (surfaceType == PlacementSurfaceType.Floor)
            worldPosition.y += MRAdjustmentsSettings.FloorCabinetYOffset;
    }

    static void WriteStoredPose(
        MRCabinetPlacement placement,
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

    static bool TryReadWorldFallback(MRCabinetPlacement placement, out Vector3 worldPosition, out Quaternion worldRotation)
    {
        worldPosition = Vector3.zero;
        worldRotation = Quaternion.identity;
        if (placement?.WorldPosition == null || placement.WorldRotation == null)
            return false;

        worldPosition = placement.WorldPosition.ToVector3();
        worldRotation = placement.WorldRotation.ToQuaternion();
        return true;
    }

    static bool TryReadWorldPose(MRCabinetPlacement placement, out Vector3 worldPosition, out Quaternion worldRotation)
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
            {
                ConfigManager.WriteConsoleWarning(
                    $"{LogPrefix} {placement.DisplayLabel} anchor unresolved — using world fallback {worldPosition}");
                MRTransitionLog.LogWarning($"{placement.DisplayLabel} anchor unresolved — world fallback {worldPosition}");
                return true;
            }

            MRTransitionLog.LogWarning($"{placement.DisplayLabel} pose unresolved (no anchor, no fallback)");
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

    static void ReadWorldPose(MRCabinetPlacement placement, out Vector3 worldPosition, out Quaternion worldRotation)
    {
        if (!TryReadWorldPose(placement, out worldPosition, out worldRotation))
        {
            worldPosition = placement?.Position != null ? placement.Position.ToVector3() : Vector3.zero;
            worldRotation = placement?.Rotation != null ? placement.Rotation.ToQuaternion() : Quaternion.identity;
        }
    }

    void EnsureWorldSpaceLayout(Transform mrSpaceOrigin)
    {
        if (layout == null || layout.Version >= WorldSpaceLayoutVersion)
            return;

        ConfigManager.WriteConsoleWarning(
            $"{LogPrefix} mr-layout version {layout.Version} uses legacy local coords — re-place cabinets once to fix saved poses");

        if (mrSpaceOrigin != null)
        {
            foreach (MRCabinetPlacement placement in layout.GetCabinets())
            {
                if (placement?.Position == null || placement.Rotation == null)
                    continue;

                Vector3 localPos = placement.Position.ToVector3();
                Quaternion localRot = placement.Rotation.ToQuaternion();
                Vector3 worldPos = mrSpaceOrigin.TransformPoint(localPos);
                Quaternion worldRot = mrSpaceOrigin.rotation * localRot;
                placement.AnchorUuid = null;
                placement.Position = MRVector3.From(worldPos);
                placement.Rotation = MRQuaternion.From(worldRot);
            }
        }

        layout.Version = WorldSpaceLayoutVersion;
        layout.Save(LayoutFilePath);
    }

    static void DisableAutoFloorSnap(GameObject cabinetRoot)
    {
        if (cabinetRoot == null)
            return;

        PutOnFloor[] floorSnaps = cabinetRoot.GetComponentsInChildren<PutOnFloor>(true);
        foreach (PutOnFloor floorSnap in floorSnaps)
        {
            if (floorSnap != null)
                Destroy(floorSnap);
        }
    }

    void DestroySpawnedInstance(string placementId)
    {
        if (!spawnedById.TryGetValue(placementId, out GameObject root) || root == null)
            return;

        StopLibretroOnCabinet(root);

        var replace = root.GetComponent<CabinetReplace>();
        if (replace != null)
            Destroy(replace.gameObject);
        else
            Destroy(root);

        spawnedById.Remove(placementId);
    }

    static void StopLibretroOnCabinet(GameObject cabinetRoot)
    {
        var screens = cabinetRoot.GetComponentsInChildren<LibretroScreenController>(true);
        foreach (LibretroScreenController screen in screens)
        {
            if (screen == null)
                continue;
            if (LibretroMameCore.isRunning(screen.ScreenName, screen.GameFile))
                LibretroMameCore.End(screen.ScreenName, screen.GameFile);
            screen.SuspendAttractAndPlaybackForTransition();
        }
    }
}
