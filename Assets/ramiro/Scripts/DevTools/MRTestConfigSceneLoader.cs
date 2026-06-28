/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// TestConfig scene: loads the MRUK editor test room (default) or legacy SimulateRoom primitives,
/// then spawns cabinets and environment props from MR YAML via <see cref="MixedRealityManager.EnterTestSceneMr"/>.
/// </summary>
[DefaultExecutionOrder(-200)]
public class MRTestConfigSceneLoader : MonoBehaviour
{
    public const string TestSceneName = "TestConfig";

    const string LogPrefix = "[MRTestConfigSceneLoader]";
    const string SimulateRoomName = "SimulateRoom";
    const string FloorChildName = "Floor";

    static MRTestConfigSceneLoader instance;

    [Tooltip("Default: MRUK Office_Small — same anchors/labels as Quest MR.")]
    [SerializeField] bool useMrukEditorRoom = true;
    [SerializeField] bool hideSimulateRoomWhenUsingMruk = true;
    [SerializeField] float startDelaySeconds = 0.2f;
    [SerializeField] float eyeHeightMeters = 1.6f;
    [SerializeField] float viewBackOffsetMeters = 1.2f;

    public static bool UseMrukEditorRoom =>
        instance != null ? instance.useMrukEditorRoom : true;

    void Awake()
    {
        if (SceneManager.GetActiveScene().name != TestSceneName)
            return;

        instance = this;

        if (useMrukEditorRoom)
        {
            EnsureSceneMruk();
            if (hideSimulateRoomWhenUsingMruk)
                HideSimulateRoom();
        }
        else if (!TryFindRoom(out Transform simulateRoom, out Transform floor))
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} SimulateRoom fallback missing {SimulateRoomName}/{FloorChildName}");
            return;
        }
        else
        {
            PrepareSimulateRoomColliders(simulateRoom);
            PositionViewCameraOnFloor(floor);
        }

        MRTestConfigEditorCamera.EnsureOnMainCamera();
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name != TestSceneName)
            return;

        StartCoroutine(LoadLayoutWhenReady());
    }

    IEnumerator LoadLayoutWhenReady()
    {
        yield return new WaitForSeconds(startDelaySeconds);

        MRCatalogBootstrap.PrepareCatalog(seedExampleInEditor: true);

        while (MixedRealityManager.Instance == null)
            yield return null;

        Vector3 originHint = Vector3.zero;
        if (!useMrukEditorRoom && TryFindRoom(out _, out Transform floor))
            originHint = GetFloorSurfaceCenter(floor);

        MixedRealityManager.Instance.EnterTestSceneMr(originHint, Quaternion.identity);

        string roomSource = useMrukEditorRoom ? "MRUK editor room" : "SimulateRoom";
        ConfigManager.WriteConsole($"{LogPrefix} loading MR layouts ({roomSource})");
        ConfigManager.WriteConsole(
            $"{LogPrefix} CRT: Enter/A | arrows=nav | M=menu | placement: mouse+RMB | Esc=cancel");
    }

    /// <summary>After MRUK probe, place the editor fly camera inside the loaded room.</summary>
    public static void PositionViewCameraFromSurfaces(MREnvironmentSurfaces surfaces)
    {
        if (instance == null || !UseMrukEditorRoom || surfaces == null || !surfaces.HasFloor)
            return;

        Camera cam = Camera.main;
        if (cam == null)
            return;

        Vector3 floorPoint = surfaces.PlayerFloorPoint;
        Vector3 eye = floorPoint + Vector3.up * instance.eyeHeightMeters - Vector3.forward * instance.viewBackOffsetMeters;
        cam.transform.SetPositionAndRotation(eye, Quaternion.LookRotation(Vector3.forward, Vector3.up));
        MRCameraRigShim.Align();
        ConfigManager.WriteConsole($"{LogPrefix} camera in MRUK room at {eye}");
    }

    public static bool TryGetSimulateRoomSurfaces(out Transform floor, out Transform ceiling)
    {
        floor = null;
        ceiling = null;
        if (UseMrukEditorRoom || !TryFindRoom(out Transform simulateRoom, out Transform floorTransform))
            return false;

        floor = floorTransform;
        ceiling = simulateRoom.Find("Ceiling");
        return true;
    }

    static void HideSimulateRoom()
    {
        GameObject roomGo = GameObject.Find(SimulateRoomName);
        if (roomGo == null || !roomGo.activeSelf)
            return;

        roomGo.SetActive(false);
        ConfigManager.WriteConsole($"{LogPrefix} hidden {SimulateRoomName} — using MRUK editor room");
    }

    static void EnsureSceneMruk()
    {
        if (MRUK.Instance != null || Object.FindObjectOfType<MRUK>() != null)
            return;

        GameObject prefab = Resources.Load<GameObject>("ramiro/MRUK");
        if (prefab == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} Resources/ramiro/MRUK.prefab missing");
            return;
        }

        Object.Instantiate(prefab);
        ConfigManager.WriteConsole($"{LogPrefix} instantiated MRUK from Resources (RoomPrefabs configured on prefab)");
    }

    static bool TryFindRoom(out Transform simulateRoom, out Transform floor)
    {
        GameObject roomGo = GameObject.Find(SimulateRoomName);
        simulateRoom = roomGo != null ? roomGo.transform : null;
        floor = simulateRoom != null ? simulateRoom.Find(FloorChildName) : null;
        return simulateRoom != null && floor != null;
    }

    static void PrepareSimulateRoomColliders(Transform simulateRoom)
    {
        int floorLayer = LayerMask.NameToLayer("floor");
        if (floorLayer < 0)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} layer 'floor' missing — placement ray may fail");
            return;
        }

        foreach (Collider col in simulateRoom.GetComponentsInChildren<Collider>(true))
            col.gameObject.layer = floorLayer;
    }

    void PositionViewCameraOnFloor(Transform floor)
    {
        Camera cam = Camera.main;
        if (cam == null)
            return;

        Bounds floorBounds = GetWorldBounds(floor);
        Vector3 eye = new Vector3(
            floorBounds.center.x,
            floorBounds.max.y + eyeHeightMeters,
            floorBounds.center.z - viewBackOffsetMeters);

        cam.transform.SetPositionAndRotation(eye, Quaternion.LookRotation(Vector3.forward, Vector3.up));
    }

    static Vector3 GetFloorSurfaceCenter(Transform floor)
    {
        Bounds bounds = GetWorldBounds(floor);
        return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
    }

    static Bounds GetWorldBounds(Transform target)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
            return renderer.bounds;

        Collider collider = target.GetComponent<Collider>();
        if (collider != null)
            return collider.bounds;

        return new Bounds(target.position, Vector3.one * 0.15f);
    }
}
