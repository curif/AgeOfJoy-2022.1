/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections;
using System.Threading.Tasks;
using Meta.XR.MRUtilityKit;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Loads MRUK scene data when entering MR. Uses the official MRUK prefab (required on Quest).
/// Flow on device: wait OVRManager ready -> await LoadSceneFromDevice(true,true) -> log rooms/anchors.
/// Never ClearScene() before loading: MRUK manages internal state and that can cause NoRoomsFound.
/// </summary>
public class MRSceneBootstrap : MonoBehaviour
{
    const string LogPrefix = "[MRSceneBootstrap]";
    const string MrukResourcesPath = "MR/MRUK";

    [SerializeField] float deviceLoadTimeoutSeconds = 180f;
    [SerializeField] float ovrReadyTimeoutSeconds = 20f;
    [SerializeField] float anchorsReadyTimeoutSeconds = 12f;
    [SerializeField] GameObject runtimeMrukPrefab;
#if UNITY_EDITOR
    [SerializeField] GameObject editorTestRoomPrefab;
    [SerializeField] string editorTestRoomPrefabName = "Office_Small";
#endif

    MRUK mruk;
    GameObject mrukObject;
    bool sceneLoaded;
    bool sceneLoadedEventFired;
    bool roomCreatedHooked;
    bool sceneLoadedHooked;

    public bool HasScene => sceneLoaded && MRUK.Instance != null && MRUK.Instance.GetCurrentRoom() != null;
    public MRUKRoom CurrentRoom => HasScene ? MRUK.Instance.GetCurrentRoom() : null;

    public IEnumerator EnsureSceneLoaded()
    {
        sceneLoaded = false;
        sceneLoadedEventFired = false;
        MRSceneLoadState.Reset();

        mruk = EnsureMrukComponent();
        if (mruk == null)
        {
            MRSceneLoadState.LastLoadResult = MRUK.LoadDeviceResult.FailureDataIsInvalid;
            MRSceneLoadState.LastFaultDetail = "MRUK prefab ausente — gere Resources/MR/MRUK.prefab";
            MRSceneLoadState.LastLoadMessage = MRSceneLoadState.Describe(MRSceneLoadState.LastLoadResult);
            ConfigManager.WriteConsoleError($"{LogPrefix} {MRSceneLoadState.LastFaultDetail}");
            yield break;
        }

        HookEvents();

        if (HasRoomWithAnchors())
        {
            ConfigManager.WriteConsole($"{LogPrefix} MRUK already has a current room; reusing");
            MarkLoaded();
            LogRoomDetails(MRUK.Instance.GetCurrentRoom());
            yield break;
        }

#if UNITY_EDITOR
        if (Application.isEditor)
        {
            yield return LoadEditorTestRoom();
            yield break;
        }
#endif

        if (!MRScenePermissions.IsGranted)
            ConfigManager.WriteConsoleWarning($"{LogPrefix} USE_SCENE not granted before device load");

        yield return LoadFromDevice();
    }

#if UNITY_EDITOR
    IEnumerator LoadEditorTestRoom()
    {
        GameObject roomPrefab = ResolveEditorTestRoomPrefab();
        if (roomPrefab == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} editor test room prefab not found");
            yield break;
        }

        ConfigManager.WriteConsole($"{LogPrefix} loading editor test room '{roomPrefab.name}'");
        MRUK.Instance.LoadSceneFromPrefab(roomPrefab);

        yield return WaitForRoomReady(5f);

        if (HasRoomWithAnchors())
        {
            MarkLoaded();
            LogRoomDetails(MRUK.Instance.GetCurrentRoom());
        }
        else
        {
            MRSceneLoadState.LastLoadResult = MRUK.LoadDeviceResult.NoRoomsFound;
            MRSceneLoadState.LastLoadMessage = MRSceneLoadState.Describe(MRSceneLoadState.LastLoadResult);
        }
    }
#endif

    IEnumerator LoadFromDevice()
    {
        yield return WaitForOvrReady();

        if (MRUK.Instance == null)
        {
            MRSceneLoadState.LastLoadResult = MRUK.LoadDeviceResult.FailureDataIsInvalid;
            MRSceneLoadState.LastFaultDetail = "MRUK.Instance ausente após esperar OVRManager";
            MRSceneLoadState.LastLoadMessage = MRSceneLoadState.Describe(MRSceneLoadState.LastLoadResult);
            ConfigManager.WriteConsoleError($"{LogPrefix} {MRSceneLoadState.LastFaultDetail}");
            yield break;
        }

        // MRUK SDK 72 requires _cameraRig.trackingSpace inside LoadSceneFromDevice
        // (pose.ComputeWorldPosition). Age of Joy uses XROrigin (no OVRCameraRig in scene),
        // so attach a shim before loading or we get a NullReferenceException.
        if (!MRCameraRigShim.EnsureAttachedTo(MRUK.Instance))
        {
            MRSceneLoadState.LastLoadResult = MRUK.LoadDeviceResult.FailureDataIsInvalid;
            MRSceneLoadState.LastFaultDetail = "OVRCameraRig shim falhou (verifique XROrigin na cena)";
            MRSceneLoadState.LastLoadMessage = MRSceneLoadState.Describe(MRSceneLoadState.LastLoadResult);
            ConfigManager.WriteConsoleError($"{LogPrefix} {MRSceneLoadState.LastFaultDetail}");
            yield break;
        }

        ConfigManager.WriteConsole(
            $"{LogPrefix} LoadSceneFromDevice begin (rooms_before={MRUK.Instance.Rooms.Count}, anchors={MRUK.Instance.GetCurrentRoom()?.Anchors.Count ?? 0})");

        Task<MRUK.LoadDeviceResult> loadTask = MRUK.Instance.LoadSceneFromDevice(
            requestSceneCaptureIfNoDataFound: true,
            removeMissingRooms: true);

        float remaining = deviceLoadTimeoutSeconds;
        while (!loadTask.IsCompleted && remaining > 0f)
        {
            remaining -= Time.unscaledDeltaTime;
            yield return null;
        }

        MRUK.LoadDeviceResult result = InterpretLoadTask(loadTask);

        MRSceneLoadState.LastLoadResult = result;
        MRSceneLoadState.LastLoadMessage = MRSceneLoadState.Describe(result);
        ConfigManager.WriteConsole(
            $"{LogPrefix} LoadSceneFromDevice result={result} rooms={MRUK.Instance.Rooms.Count} currentRoom={(MRUK.Instance.GetCurrentRoom() != null ? "yes" : "no")}");

        if (result == MRUK.LoadDeviceResult.Success || HasRoomWithAnchors())
        {
            yield return WaitForRoomReady(anchorsReadyTimeoutSeconds);
        }

        if (HasRoomWithAnchors())
        {
            MarkLoaded();
            LogRoomDetails(MRUK.Instance.GetCurrentRoom());
        }
        else
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} no usable room after load (result={result}, sceneLoadedEvent={sceneLoadedEventFired})");
        }

        RefreshSurfaceDebug();
    }

    static MRUK.LoadDeviceResult InterpretLoadTask(Task<MRUK.LoadDeviceResult> task)
    {
        if (!task.IsCompleted)
        {
            MRSceneLoadState.LastFaultDetail = "timeout no LoadSceneFromDevice";
            return MRUK.LoadDeviceResult.FailureInsufficientResources;
        }

        if (task.IsFaulted)
        {
            Exception ex = task.Exception?.GetBaseException();
            MRSceneLoadState.LastFaultDetail = ex != null ? ex.Message : "exceção sem mensagem";
            ConfigManager.WriteConsoleError(
                $"{LogPrefix} LoadSceneFromDevice exception: {MRSceneLoadState.LastFaultDetail}");

            return HasRoomWithAnchors()
                ? MRUK.LoadDeviceResult.Success
                : MRUK.LoadDeviceResult.FailureDataIsInvalid;
        }

        MRUK.LoadDeviceResult result = task.Result;
        if (result == MRUK.LoadDeviceResult.FailureDataIsInvalid)
            MRSceneLoadState.LastFaultDetail = "UpdateScene rejeitou dados do dispositivo";
        else if (result == MRUK.LoadDeviceResult.NoRoomsFound)
            MRSceneLoadState.LastFaultDetail = "MRUK não encontrou sala (verifique Space Setup)";

        return result;
    }

    IEnumerator WaitForRoomReady(float timeoutSeconds)
    {
        float remaining = timeoutSeconds;
        while (remaining > 0f)
        {
            if (HasRoomWithAnchors())
                yield break;
            remaining -= Time.unscaledDeltaTime;
            yield return null;
        }
    }

    IEnumerator WaitForOvrReady()
    {
        float remaining = ovrReadyTimeoutSeconds;
        while (remaining > 0f)
        {
            if (OVRManager.OVRManagerinitialized && OVRPlugin.initialized && MRUK.Instance != null)
            {
                ConfigManager.WriteConsole($"{LogPrefix} OVRManager + MRUK ready");
                yield break;
            }
            remaining -= Time.unscaledDeltaTime;
            yield return null;
        }

        ConfigManager.WriteConsoleWarning(
            $"{LogPrefix} OVRManager/MRUK not ready after {ovrReadyTimeoutSeconds:F0}s — continuing anyway");
    }

    static bool HasRoomWithAnchors()
    {
        MRUKRoom room = MRUK.Instance != null ? MRUK.Instance.GetCurrentRoom() : null;
        return room != null && room.Anchors != null && room.Anchors.Count > 0;
    }

    void MarkLoaded()
    {
        sceneLoaded = true;
        MRSceneLoadState.LastLoadResult = MRUK.LoadDeviceResult.Success;
        MRSceneLoadState.LastFaultDetail = "";
        MRSceneLoadState.LastLoadMessage = MRSceneLoadState.Describe(MRUK.LoadDeviceResult.Success);
    }

    static void LogRoomDetails(MRUKRoom room)
    {
        if (room == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} room null — nothing to log");
            return;
        }

        MRUKAnchor floor = room.FloorAnchor;
        MRUKAnchor ceiling = room.CeilingAnchor;
        int walls = room.WallAnchors != null ? room.WallAnchors.Count : 0;

        ConfigManager.WriteConsole(
            $"{LogPrefix} room='{room.gameObject.name}' anchors={room.Anchors.Count} walls={walls} " +
            $"floor={(floor != null ? "Y=" + floor.GetAnchorCenter().y.ToString("F2") : "null")} " +
            $"ceiling={(ceiling != null ? "Y=" + ceiling.GetAnchorCenter().y.ToString("F2") : "null")}");

        foreach (MRUKAnchor anchor in room.Anchors)
        {
            if (anchor == null)
                continue;
            ConfigManager.WriteConsole(
                $"{LogPrefix}   anchor label={anchor.Label} pos={anchor.transform.position} " +
                $"plane={(anchor.PlaneRect.HasValue ? anchor.PlaneRect.Value.size.ToString() : "no")} " +
                $"volume={(anchor.VolumeBounds.HasValue ? anchor.VolumeBounds.Value.size.ToString() : "no")}");
        }
    }

    void HookEvents()
    {
        if (MRUK.Instance == null)
            return;

        if (!roomCreatedHooked)
        {
            MRUK.Instance.RoomCreatedEvent.AddListener(HandleRoomCreated);
            roomCreatedHooked = true;
        }

        if (!sceneLoadedHooked)
        {
            MRUK.Instance.SceneLoadedEvent.AddListener(HandleSceneLoadedEvent);
            sceneLoadedHooked = true;
        }
    }

    void UnhookEvents()
    {
        if (MRUK.Instance == null)
            return;

        if (roomCreatedHooked)
        {
            MRUK.Instance.RoomCreatedEvent.RemoveListener(HandleRoomCreated);
            roomCreatedHooked = false;
        }

        if (sceneLoadedHooked)
        {
            MRUK.Instance.SceneLoadedEvent.RemoveListener(HandleSceneLoadedEvent);
            sceneLoadedHooked = false;
        }
    }

    void HandleRoomCreated(MRUKRoom room)
    {
        if (room == null)
            return;

        ConfigManager.WriteConsole($"{LogPrefix} RoomCreatedEvent anchors={room.Anchors.Count}");
        if (HasRoomWithAnchors())
        {
            MarkLoaded();
            LogRoomDetails(room);
        }
        RefreshSurfaceDebug();
    }

    void HandleSceneLoadedEvent()
    {
        sceneLoadedEventFired = true;
        ConfigManager.WriteConsole(
            $"{LogPrefix} SceneLoadedEvent rooms={MRUK.Instance?.Rooms?.Count ?? 0}");

        if (HasRoomWithAnchors())
        {
            MarkLoaded();
            LogRoomDetails(MRUK.Instance.GetCurrentRoom());
        }
        RefreshSurfaceDebug();
    }

    static void RefreshSurfaceDebug()
    {
        if (MRUK.Instance == null)
            return;

        MRRoomInfoUI.Instance?.RefreshContent();
        if (MRRuntimeSettings.ShowRoomAnchorInfoCanvas
            && MixedRealityManager.Instance != null
            && MixedRealityManager.Instance.CurrentMode != ExperienceMode.VR)
            MRRoomInfoUI.Instance?.Show();
    }

    public void ClearScene()
    {
        UnhookEvents();
        sceneLoaded = false;
        MRSceneLoadState.Reset();
        // Não chamamos MRUK.Instance.ClearScene() aqui: a sala carregada do dispositivo
        // pode ser reutilizada na próxima entrada em MR sem precisar recarregar.
        ConfigManager.WriteConsole($"{LogPrefix} scene cleared (MRUK rooms preserved)");
    }

    void OnDestroy()
    {
        UnhookEvents();
    }

    MRUK EnsureMrukComponent()
    {
        if (MRUK.Instance != null)
        {
            mruk = MRUK.Instance;
            mrukObject = mruk.gameObject;
            ConfigureMrukForManualLoad(mruk);
            return mruk;
        }

        MRUK existing = FindObjectOfType<MRUK>();
        if (existing != null)
        {
            mruk = existing;
            mrukObject = existing.gameObject;
            ConfigureMrukForManualLoad(mruk);
            return mruk;
        }

        GameObject prefab = ResolveRuntimeMrukPrefab();
        if (prefab == null)
            return null;

        mrukObject = Instantiate(prefab);
        mrukObject.name = "MRUK";
        mruk = mrukObject.GetComponent<MRUK>();
        ConfigureMrukForManualLoad(mruk);
        ConfigManager.WriteConsole($"{LogPrefix} MRUK instantiated from prefab '{prefab.name}'");
        return mruk;
    }

    GameObject ResolveRuntimeMrukPrefab()
    {
        if (runtimeMrukPrefab != null)
            return runtimeMrukPrefab;

        GameObject fromResources = Resources.Load<GameObject>(MrukResourcesPath);
        if (fromResources != null)
            return fromResources;

#if UNITY_EDITOR
        return LoadMrukPrefabFromPackage();
#else
        return null;
#endif
    }

    static void ConfigureMrukForManualLoad(MRUK target)
    {
        if (target == null)
            return;

        if (target.SceneSettings == null)
            target.SceneSettings = new MRUK.MRUKSettings();

        target.SceneSettings.DataSource = MRUK.SceneDataSource.Device;
        target.SceneSettings.LoadSceneOnStartup = false;
        // World Lock disabled: we use a custom OVRCameraRig shim (XROrigin-driven),
        // so MRUK.Update should NOT try to mutate its trackingSpace (would do nothing useful
        // and could re-introduce a NullReferenceException on disabled GameObjects).
        target.EnableWorldLock = false;
    }

#if UNITY_EDITOR
    static GameObject LoadMrukPrefabFromPackage()
    {
        string[] paths =
        {
            "Packages/com.meta.xr.mrutilitykit/Core/Prefabs/MRUK.prefab",
            "Packages/com.meta.xr.mrutilitykit/Core/Tools/MRUK.prefab",
        };

        foreach (string path in paths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
                return prefab;
        }

        foreach (string guid in AssetDatabase.FindAssets("MRUK t:Prefab"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.Contains("mrutilitykit", StringComparison.OrdinalIgnoreCase))
                continue;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null && prefab.GetComponent<MRUK>() != null)
                return prefab;
        }

        return null;
    }

    GameObject ResolveEditorTestRoomPrefab()
    {
        if (editorTestRoomPrefab != null)
            return editorTestRoomPrefab;

        string[] candidatePaths =
        {
            $"Packages/com.meta.xr.mrutilitykit/Core/Rooms/Prefabs/{editorTestRoomPrefabName}.prefab",
            $"Packages/com.meta.xr.mrutilitykit/Core/Rooms/Prefabs/Rooms/{editorTestRoomPrefabName}.prefab",
        };

        foreach (string path in candidatePaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
                return prefab;
        }

        foreach (string guid in AssetDatabase.FindAssets($"{editorTestRoomPrefabName} t:Prefab"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.Contains("mrutilitykit", StringComparison.OrdinalIgnoreCase))
                continue;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
                return prefab;
        }

        return null;
    }
#endif
}
