/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

/// <summary>
/// Spawns ConfigurationCabinetMiniMR on the floor and drives MR menus on the CRT via MRConfigurationController.
/// </summary>
public class MRConfigurationCabinetController : MonoBehaviour
{
    const string LogPrefix = "[MRConfigurationCabinetController]";
    const string DefaultCabinetResourcesPath = "ramiro/PrefabsEnvironment/ConfigurationCabinetMiniMR";
    const string LegacyWallUiResourcesPath = "MR/MRConfigurationUI";
    const string SavedPoseFlagKey = "MR.ConfigurationCabinetMiniMR.PoseSaved";
    const string SavedPosePositionKey = "MR.ConfigurationCabinetMiniMR.Position";
    const string SavedPoseRotationKey = "MR.ConfigurationCabinetMiniMR.Rotation";
    const string SavedPoseAnchorUuidKey = "MR.ConfigurationCabinetMiniMR.AnchorUuid";
    const string SavedWorldPositionKey = "MR.ConfigurationCabinetMiniMR.WorldPosition";
    const string SavedWorldRotationKey = "MR.ConfigurationCabinetMiniMR.WorldRotation";
    const int SavedPoseSchemaWorld = 2;
    const int SavedPoseSchemaAnchorRelative = 3;
    const bool EnableSavedPoseLoad = true;
    static readonly Vector3 CabinetFootprint = new Vector3(0.45f, 0.5f, 0.35f);

    public static MRConfigurationCabinetController Instance { get; private set; }

    [SerializeField] GameObject configurationCabinetPrefab;
    [SerializeField] float spawnDistanceMeters = 1.4f;
    [SerializeField] float spawnYawOffsetDegrees = 0f;
    [Tooltip("Wall-only fine adjust after auto-facing fix. Keep near zero.")]
    [Range(-30f, 30f)]
    [SerializeField] float wallMountYawOffsetDegrees = 0f;
    [Tooltip("Fallback when prefab has no MRPlacementProfile.")]
    [SerializeField] PlacementFacingAxis wallFacingAxis = PlacementFacingAxis.NegativeX;
    [Tooltip("If true, always instantiate a new cabinet and ignore scene templates.")]
    [SerializeField] bool alwaysInstantiateCabinet = true;
#if UNITY_EDITOR
    [SerializeField] bool editorAutoSpawnInVr = false;
    [SerializeField] float editorAutoSpawnDelaySeconds = 2f;
    [Tooltip("Editor only: re-place cabinet every frame (move MRUK room to test).")]
    [SerializeField] bool editorContinuousFloorSnap = true;
    [Tooltip("Editor only (test scenes): enables the camera already configured inside cabinet prefab.")]
    [SerializeField] bool editorAlignCameraAfterInstantiate = true;
    [SerializeField] bool editorDisableSceneMainCamera = true;
    [Tooltip("Editor only: auto insert coin right after cabinet is spawned/adopted.")]
    [SerializeField] bool editorAutoInsertCoinOnSpawn = true;
    [Tooltip("Editor only: auto open CRT menu after spawn.")]
    [SerializeField] bool editorAutoOpenEditOnSpawn = true;
#endif

    GameObject cabinetInstance;
    MRConfigurationController crtController;
    MRPlacementRayController placementRay;
    CoinSlotController boundCoinSlot;
    UnityAction onCoinInsertedHandler;
    bool isEditOpen;
    bool cabinetWasInstantiated;
    bool initialPlacementRequested;
    Coroutine initialPlacementRayCoroutine;

    public bool IsEditOpen => isEditOpen;
    public bool HasCabinet => cabinetInstance != null;

    /// <summary>True when the config cabinet is visible (hidden instances kept for re-entry do not count).</summary>
    public bool HasVisibleCabinet => cabinetInstance != null && cabinetInstance.activeInHierarchy;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    void OnEnable()
    {
        if (MixedRealityManager.Instance != null)
            MixedRealityManager.Instance.OnModeChanged += HandleModeChanged;
    }

    void Start()
    {
        DestroyStrayWallUi();

        if (IsTestScene())
            StartCoroutine(PlaceCabinetOnFloorWhenReady());

        TrySpawnIfMrActive();

#if UNITY_EDITOR
        if (editorAutoSpawnInVr)
            StartCoroutine(EditorAutoSpawnWhenReady());
#endif
    }

    void TrySpawnIfMrActive()
    {
        if (HasCabinet || MixedRealityManager.Instance == null)
            return;

        ExperienceMode mode = MixedRealityManager.Instance.CurrentMode;
        if (mode == ExperienceMode.MR || mode == ExperienceMode.MR_EDIT)
            SpawnAtMrOrigin();
    }

    void OnDisable()
    {
        if (MixedRealityManager.Instance != null)
            MixedRealityManager.Instance.OnModeChanged -= HandleModeChanged;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

#if UNITY_EDITOR
    void LateUpdate()
    {
        if (!Application.isEditor || !editorContinuousFloorSnap || cabinetInstance == null)
            return;

        if (MixedRealityManager.Instance == null)
            return;

        ExperienceMode mode = MixedRealityManager.Instance.CurrentMode;
        if (mode != ExperienceMode.MR && mode != ExperienceMode.MR_EDIT)
            return;

        if (placementRay != null && placementRay.IsActive)
            return;

        if (initialPlacementRequested)
            return;

        if (HasValidSavedPose())
            return;

        ApplyPoseToCabinet(cabinetInstance);
    }
#endif

    void HandleModeChanged(ExperienceMode mode)
    {
        if (mode != ExperienceMode.VR)
            return;

        // EnterVRCoroutine owns teardown — sync Despawn here blocked on Libretro Destroy.
        if (MixedRealityManager.Instance != null && MixedRealityManager.Instance.TransitionInProgress)
            return;

        Despawn();
    }

    public void ToggleEdit()
    {
        if (isEditOpen)
            CloseEdit();
        else
            OpenEdit();
    }

    public void OpenEdit()
    {
        if (isEditOpen)
            return;

        if (MixedRealityManager.Instance == null)
            return;

        ExperienceMode mode = MixedRealityManager.Instance.CurrentMode;
        if (mode != ExperienceMode.MR && mode != ExperienceMode.MR_EDIT)
        {
#if UNITY_EDITOR
            if (mode == ExperienceMode.VR)
            {
                if (!HasCabinet)
                    SpawnForEditorTest();
                // In editor test flow, allow opening CRT session even while still in VR mode.
            }
            else
                return;
#else
            return;
#endif
        }

        if (!HasCabinet)
            SpawnAtMrOrigin();

        if (crtController == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} open failed — MRConfigurationController missing on cabinet");
            return;
        }

        cabinetInstance.SetActive(true);
        crtController.BeginSession();

        if (mode == ExperienceMode.MR)
            MixedRealityManager.Instance.EnterMREdit();

        isEditOpen = true;
        ConfigManager.WriteConsole($"{LogPrefix} CRT configuration open");
    }

    public void CloseEdit()
    {
        if (!isEditOpen)
            return;

        crtController?.EndSession();

        isEditOpen = false;

        if (MixedRealityManager.Instance != null
            && MixedRealityManager.Instance.CurrentMode == ExperienceMode.MR_EDIT)
            MixedRealityManager.Instance.ExitMREdit();

        ConfigManager.WriteConsole($"{LogPrefix} CRT configuration closed");
    }

    /// <summary>Idle CRT and empty coin slot before game-cabinet placement ray (Add / Move in ADDED).</summary>
    public void SuspendEditForGameCabinetPlacement()
    {
        if (crtController != null && crtController.IsSessionActive)
            crtController.SuspendForExternalPlacement();

        if (boundCoinSlot != null)
            boundCoinSlot.clean();

        isEditOpen = false;

        if (MixedRealityManager.Instance != null
            && MixedRealityManager.Instance.CurrentMode == ExperienceMode.MR_EDIT)
            MixedRealityManager.Instance.ExitMREdit();

        ConfigManager.WriteConsole(
            $"{LogPrefix} suspended for cabinet placement — insert coin to reopen CRT");
    }

    public void ForceCloseEdit()
    {
        if (isEditOpen || (crtController != null && crtController.IsSessionActive))
            crtController?.EndSession();
        isEditOpen = false;
    }

    /// <summary>Hide config cabinet immediately on MR exit — avoid sync Destroy before coroutine runs.</summary>
    public void HideForMrExit()
    {
        CancelActivePlacementRay();
        if (initialPlacementRayCoroutine != null)
        {
            StopCoroutine(initialPlacementRayCoroutine);
            initialPlacementRayCoroutine = null;
        }

        if (cabinetInstance != null)
        {
            SaveWorldFallbackPose(cabinetInstance.transform.position, cabinetInstance.transform.rotation);
            cabinetInstance.SetActive(false);
        }

        UnbindCoinInsert();
        initialPlacementRequested = false;

        MRTransitionLog.Log($"HideForMrExit hasCabinet={cabinetInstance != null} wasInstantiated={cabinetWasInstantiated}");
    }

    /// <summary>MR→VR: no-op if HideForMrExit already ran in BeginMrExitImmediateSync.</summary>
    public void ReleaseForVrTransition()
    {
        MRTransitionLog.LogStep("MRConfigurationCabinetController.ReleaseForVrTransition", "skip — sync exit already done");
    }

    public void SpawnAtMrOrigin()
    {
        if (cabinetInstance != null)
        {
            cabinetInstance.SetActive(true);
            EnsureCoinSlotBound();
            ApplyPoseToExistingCabinet();
            RequestInitialPlacementRayWhenReady();
            return;
        }

        DestroyStrayWallUi();

        if (!alwaysInstantiateCabinet)
        {
            GameObject sceneCabinet = FindSceneCabinet();
            if (sceneCabinet != null)
            {
                AdoptCabinetInstance(sceneCabinet, wasInstantiated: false);
                ConfigManager.WriteConsole($"{LogPrefix} adopted scene cabinet at {cabinetInstance.transform.position}");
                return;
            }
        }
        else
            DisableSceneCabinetTemplates();

        GameObject prefab = ResolveCabinetPrefab();
        if (prefab == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} prefab not found at Resources/{DefaultCabinetResourcesPath}");
            return;
        }

        AdoptCabinetInstance(Instantiate(prefab), wasInstantiated: true);
        cabinetInstance.name = "ConfigurationCabinetMiniMR";
#if UNITY_EDITOR
        TryAlignEditorCameraToScreen();
#endif
        ConfigManager.WriteConsole(
            $"{LogPrefix} spawned (floor) at {cabinetInstance.transform.position} rot={cabinetInstance.transform.eulerAngles}");
    }

    public void Despawn()
    {
        ForceCloseEdit();
        CancelActivePlacementRay();
        if (initialPlacementRayCoroutine != null)
        {
            StopCoroutine(initialPlacementRayCoroutine);
            initialPlacementRayCoroutine = null;
        }
        UnbindCoinInsert();
        if (cabinetWasInstantiated && cabinetInstance != null)
            Destroy(cabinetInstance);
        cabinetInstance = null;
        crtController = null;
        cabinetWasInstantiated = false;
        initialPlacementRequested = false;
        placementRay = null;
    }

    void ApplyPoseToExistingCabinet()
    {
        if (cabinetInstance == null)
            return;

        if (placementRay != null && placementRay.IsActive)
            return;

        if (HasValidSavedPose() && TryLoadSavedPose(out Vector3 savedPos, out Quaternion savedRot))
        {
            if (Vector3.Distance(cabinetInstance.transform.position, savedPos) > 0.02f
                || Quaternion.Angle(cabinetInstance.transform.rotation, savedRot) > 0.5f)
            {
                cabinetInstance.transform.SetPositionAndRotation(savedPos, savedRot);
                ConfigManager.WriteConsole($"{LogPrefix} pose (saved) pos={savedPos} rot={savedRot.eulerAngles}");
                MRTransitionLog.Log($"config cabinet pose (saved) pos={savedPos} rotY={savedRot.eulerAngles.y:F1}");
            }

            return;
        }

        if (TryLoadWorldFallbackPose(out Vector3 fallbackPos, out Quaternion fallbackRot))
        {
            if (Vector3.Distance(cabinetInstance.transform.position, fallbackPos) > 0.02f
                || Quaternion.Angle(cabinetInstance.transform.rotation, fallbackRot) > 0.5f)
            {
                cabinetInstance.transform.SetPositionAndRotation(fallbackPos, fallbackRot);
                ConfigManager.WriteConsole($"{LogPrefix} pose (world fallback) pos={fallbackPos} rot={fallbackRot.eulerAngles}");
                MRTransitionLog.Log($"config cabinet pose (world fallback) pos={fallbackPos} rotY={fallbackRot.eulerAngles.y:F1}");
            }

            return;
        }

        if (NeedsInitialPlacementRay())
        {
            PlaceCabinetNearViewForInitialRay(cabinetInstance);
            ConfigManager.WriteConsole($"{LogPrefix} awaiting initial placement ray (re-entry)");
            return;
        }

        ApplyPoseToCabinet(cabinetInstance);
    }

    /// <summary>Re-apply saved or MRUK pose after MR re-entry (MRUK may register anchors a frame later).</summary>
    public void RefreshPoseForMrReentry()
    {
        if (cabinetInstance == null || !cabinetInstance.activeInHierarchy)
            return;

        ApplyPoseToExistingCabinet();
    }

    void CancelActivePlacementRay()
    {
        MRPlacementRayController activeRay = placementRay != null
            ? placementRay
            : FindObjectOfType<MRPlacementRayController>();
        activeRay?.CancelActive();
    }

    GameObject ResolveCabinetPrefab()
    {
        if (configurationCabinetPrefab != null)
        {
            if (IsWallUiPrefab(configurationCabinetPrefab))
            {
                ConfigManager.WriteConsoleError(
                    $"{LogPrefix} Inspector prefab is MRConfigurationUI (wall). Assign ConfigurationCabinetMiniMR.");
                configurationCabinetPrefab = null;
            }
            else
                return configurationCabinetPrefab;
        }

        configurationCabinetPrefab = Resources.Load<GameObject>(DefaultCabinetResourcesPath);
        if (configurationCabinetPrefab == null)
            configurationCabinetPrefab = Resources.Load<GameObject>("ramiro/ConfigurationCabinetMiniMR");
        if (configurationCabinetPrefab == null)
            configurationCabinetPrefab = Resources.Load<GameObject>("UICabinet/ConfigurationCabinetMini");

        if (IsWallUiPrefab(configurationCabinetPrefab))
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} refusing wall UI prefab; need ConfigurationCabinetMiniMR");
            configurationCabinetPrefab = null;
        }

        return configurationCabinetPrefab;
    }

    static bool IsWallUiPrefab(GameObject prefab)
    {
        if (prefab == null)
            return false;
        return prefab.GetComponent<MRConfigurationUI>() != null
            || prefab.GetComponentInChildren<MRConfigurationUI>(true) != null;
    }

    void AdoptCabinetInstance(GameObject root, bool wasInstantiated)
    {
        cabinetInstance = root;
        cabinetWasInstantiated = wasInstantiated;
        cabinetInstance.transform.SetParent(null, true);
        PrepareCabinetInstance(cabinetInstance);
        ApplyPoseToCabinet(cabinetInstance);
        cabinetInstance.SetActive(true);
#if UNITY_EDITOR
        TryAutoInsertCoinForEditor();
#endif
        RequestInitialPlacementRayWhenReady();
    }

    bool NeedsInitialPlacementRay() =>
        !EnableSavedPoseLoad || !HasValidSavedPose();

    bool HasValidSavedPose()
    {
        if (!EnableSavedPoseLoad || !TryLoadSavedPose(out _, out _))
            return false;

        int schema = PlayerPrefs.GetInt(SavedPoseFlagKey, 0);
        if (schema != SavedPoseSchemaAnchorRelative)
            return true;

        string anchorUuid = PlayerPrefs.GetString(SavedPoseAnchorUuidKey, string.Empty);
        return !string.IsNullOrEmpty(anchorUuid);
    }

    void RequestInitialPlacementRayWhenReady()
    {
        if (!NeedsInitialPlacementRay() || cabinetInstance == null)
            return;

        if (initialPlacementRayCoroutine != null)
            StopCoroutine(initialPlacementRayCoroutine);

        initialPlacementRayCoroutine = StartCoroutine(InitialPlacementRayWhenReadyCoroutine());
    }

    IEnumerator InitialPlacementRayWhenReadyCoroutine()
    {
        const int maxAttempts = 90;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (cabinetInstance == null)
                yield break;

            if (initialPlacementRequested || !NeedsInitialPlacementRay())
                yield break;

            if (placementRay != null && placementRay.IsActive)
                yield break;

            MixedRealityManager mgr = MixedRealityManager.Instance;
            if (mgr == null)
            {
                yield return null;
                continue;
            }

            ExperienceMode mode = mgr.CurrentMode;
            if (mode != ExperienceMode.MR && mode != ExperienceMode.MR_EDIT)
            {
                yield return null;
                continue;
            }

            MREnvironmentSurfaces surfaces = MREnvironmentSurfaces.Instance;
            if (surfaces != null && !surfaces.IsReady)
            {
                Transform player = FindPlayerTransform();
                yield return surfaces.ProbeWhenReady(player);
                yield return null;
                continue;
            }

            if (BeginRepositionWithRay(isInitialPlacement: true))
            {
                ConfigManager.WriteConsole($"{LogPrefix} initial placement ray started");
                initialPlacementRayCoroutine = null;
                yield break;
            }

            yield return null;
        }

        initialPlacementRayCoroutine = null;
        ConfigManager.WriteConsoleWarning($"{LogPrefix} initial placement ray could not start");
    }

    static GameObject FindSceneCabinet()
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root.name.Contains("ConfigurationCabinetMini"))
                return root;
        }

        return null;
    }

    static void DisableSceneCabinetTemplates()
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (!root.name.Contains("ConfigurationCabinetMini"))
                continue;
            if (!root.activeSelf)
                continue;

            root.SetActive(false);
            ConfigManager.WriteConsole($"{LogPrefix} disabled scene cabinet template: {root.name}");
        }
    }

    static void DestroyStrayWallUi()
    {
        MRConfigurationUI[] wallPanels = Object.FindObjectsOfType<MRConfigurationUI>(true);
        foreach (MRConfigurationUI ui in wallPanels)
        {
            if (ui == null)
                continue;
            ConfigManager.WriteConsole($"{LogPrefix} removing legacy wall UI: {ui.gameObject.name}");
            Object.Destroy(ui.gameObject);
        }

        GameObject named = GameObject.Find("MRConfigurationUI");
        if (named != null)
            Object.Destroy(named);
    }

    static bool IsTestScene()
    {
        string scene = SceneManager.GetActiveScene().name;
        return scene == "TestMRmanager" || scene == "TestUI";
    }

    IEnumerator PlaceCabinetOnFloorWhenReady()
    {
        yield return null;

        MREnvironmentSurfaces surfaces = MREnvironmentSurfaces.Instance;
        Transform player = FindPlayerTransform();
        if (surfaces != null && player != null && !surfaces.IsReady)
            yield return surfaces.ProbeWhenReady(player);

        DestroyStrayWallUi();

        if (!HasCabinet)
            SpawnAtMrOrigin();
        else
        {
            ApplyPoseToCabinet(cabinetInstance);
            RequestInitialPlacementRayWhenReady();
        }
    }

    void PrepareCabinetInstance(GameObject root)
    {
        DisableLegacyComponents(root);

        ScreenGenerator screenGenerator = FindScreenGenerator(root);
        if (screenGenerator == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} ScreenGenerator not found under cabinet");
            return;
        }

        LibretroControlMap controlMap = screenGenerator.GetComponent<LibretroControlMap>();
        if (controlMap == null)
            controlMap = screenGenerator.gameObject.AddComponent<LibretroControlMap>();

        CoinSlotController coinSlot = FindCoinSlot(root);
        BindCoinInsert(coinSlot);

        crtController = screenGenerator.GetComponent<MRConfigurationController>();
        if (crtController == null)
            crtController = screenGenerator.gameObject.AddComponent<MRConfigurationController>();

        crtController.PrepareForCabinet(screenGenerator, controlMap, coinSlot);

        foreach (AudioSource audio in root.GetComponentsInChildren<AudioSource>(true))
        {
            if (coinSlot != null && audio.gameObject == coinSlot.gameObject)
                continue;
            audio.enabled = false;
        }
    }

    static void DisableLegacyComponents(GameObject root)
    {
        foreach (ConfigurationController legacy in root.GetComponentsInChildren<ConfigurationController>(true))
            legacy.enabled = false;

        foreach (MRConfigurationUI ui in root.GetComponentsInChildren<MRConfigurationUI>(true))
            ui.enabled = false;

        foreach (Teleportation teleport in root.GetComponentsInChildren<Teleportation>(true))
            teleport.enabled = false;

        foreach (basicAGE age in root.GetComponentsInChildren<basicAGE>(true))
            age.enabled = false;
    }

    static ScreenGenerator FindScreenGenerator(GameObject root)
    {
        foreach (ScreenGenerator sg in root.GetComponentsInChildren<ScreenGenerator>(true))
        {
            if (sg.gameObject.CompareTag("screenControlCabinet") || sg.gameObject.name.Contains("screen"))
                return sg;
        }

        return root.GetComponentInChildren<ScreenGenerator>(true);
    }

    static CoinSlotController FindCoinSlot(GameObject root)
    {
        CoinSlotController onRoot = root.GetComponentInChildren<CoinSlotController>(true);
        return onRoot;
    }

    void BindCoinInsert(CoinSlotController coinSlot)
    {
        if (boundCoinSlot == coinSlot && onCoinInsertedHandler != null)
            return;

        UnbindCoinInsert();
        if (coinSlot == null)
            return;

        if (onCoinInsertedHandler == null)
            onCoinInsertedHandler = HandleCoinInserted;

        coinSlot.OnInsertCoin?.AddListener(onCoinInsertedHandler);
        boundCoinSlot = coinSlot;
        MRTransitionLog.Log("config cabinet coin slot bound");
    }

    /// <summary>Re-bind coin listener after MR re-entry (HideForMrExit unbinds).</summary>
    void EnsureCoinSlotBound()
    {
        if (cabinetInstance == null)
            return;

        if (crtController == null)
        {
            PrepareCabinetInstance(cabinetInstance);
            return;
        }

        BindCoinInsert(FindCoinSlot(cabinetInstance));
    }

    void UnbindCoinInsert()
    {
        if (boundCoinSlot == null || onCoinInsertedHandler == null)
            return;

        boundCoinSlot.OnInsertCoin?.RemoveListener(onCoinInsertedHandler);
        boundCoinSlot = null;
    }

    void HandleCoinInserted()
    {
        if (isEditOpen)
            return;

        if (crtController != null && crtController.IsSessionActive)
            return;

        if (placementRay != null && placementRay.IsActive)
            return;

        MRTransitionLog.Log("config cabinet coin inserted — opening edit");
        OpenEdit();
    }

    void ApplyPoseToCabinet(GameObject root)
    {
        if (root == null)
            return;

        if (HasValidSavedPose() && TryLoadSavedPose(out Vector3 savedPos, out Quaternion savedRot))
        {
            root.transform.SetPositionAndRotation(savedPos, savedRot);
            ConfigManager.WriteConsole($"{LogPrefix} pose (saved) pos={savedPos} rot={savedRot.eulerAngles}");
            return;
        }

        if (NeedsInitialPlacementRay())
        {
            PlaceCabinetNearViewForInitialRay(root);
            ConfigManager.WriteConsole($"{LogPrefix} awaiting initial placement ray");
            return;
        }

        Transform player = FindPlayerTransform();
        MREnvironmentSurfaces surfaces = MREnvironmentSurfaces.Instance;

        Vector3 worldPos;
        Quaternion worldRot;
        string poseSource;
        bool useWallMount = GetPlacementSurfaceType() == PlacementSurfaceType.Wall;

        if (useWallMount && surfaces != null && player != null
            && surfaces.TryGetWallMountedFramePose(
                player, spawnDistanceMeters, CabinetFootprint.z, out worldPos, out worldRot, GetPlacementFacingAxis()))
        {
            poseSource = "wall mount";
            worldRot *= Quaternion.Euler(0f, spawnYawOffsetDegrees + wallMountYawOffsetDegrees, 0f);
        }
        else if (surfaces != null && player != null
            && surfaces.TryGetConfigurationCabinetPose(
                player, spawnDistanceMeters, CabinetFootprint, out worldPos, out worldRot))
        {
            poseSource = "MRUK floor";
            worldRot *= Quaternion.Euler(0f, spawnYawOffsetDegrees, 0f);
        }
        else
        {
            poseSource = "floor fallback";
            Vector3 forward = ResolveViewForward(player);
            Vector3 eye = ResolveEyePosition(player);
            worldPos = eye + forward * spawnDistanceMeters;
            worldPos.y = ResolveFloorY(surfaces, worldPos, player);
            Vector3 faceDir = player != null
                ? player.position - worldPos
                : forward;
            faceDir.y = 0f;
            if (faceDir.sqrMagnitude < 0.001f)
                faceDir = forward;
            worldRot = Quaternion.LookRotation(faceDir.normalized, Vector3.up)
                * Quaternion.Euler(0f, spawnYawOffsetDegrees, 0f);
        }

        root.transform.SetPositionAndRotation(worldPos, worldRot);

        BoxCollider box = root.GetComponentInChildren<BoxCollider>();
        if (box != null)
        {
            float bottomY = PlaceOnFloorFromBoxCollider.CalculateLowerPointY(root.transform, box);
            float pivotToBottom = root.transform.position.y - bottomY;
            root.transform.position = new Vector3(worldPos.x, worldPos.y + pivotToBottom, worldPos.z);
        }

        ConfigManager.WriteConsole(
            $"{LogPrefix} floor pose ({poseSource}) pos={root.transform.position} rot={root.transform.eulerAngles}");
    }

    public bool BeginRepositionWithRay(bool isInitialPlacement = false)
    {
        if (cabinetInstance == null)
            return false;

        placementRay = EnsurePlacementRayController();
        if (placementRay == null || placementRay.IsActive)
        {
            if (placementRay != null && placementRay.IsActive)
                ConfigManager.WriteConsole($"{LogPrefix} placement ray busy — skipped");
            return false;
        }

        bool reopenEdit = isEditOpen;
        ForceCloseEdit();

        placementRay.BeginMove(
            cabinetInstance,
            GetPlacementSurfaceType(),
            GetPlacementFacingAxis(),
            confirmCallback: (worldPos, worldRot, anchorUuid) =>
            {
                SavePose(worldPos, worldRot, anchorUuid);
                initialPlacementRequested = true;
                if (reopenEdit)
                    OpenEdit();
            },
            cancelCallback: () =>
            {
                if (isInitialPlacement)
                    initialPlacementRequested = false;
                if (reopenEdit)
                    OpenEdit();
            });
        return true;
    }

    static void PlaceCabinetNearViewForInitialRay(GameObject root)
    {
        if (root == null)
            return;

        Transform player = FindPlayerTransform();
        Transform view = Camera.main != null ? Camera.main.transform : player;
        if (view == null)
            return;

        Vector3 forward = view.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;
        forward.Normalize();

        Vector3 startPos = view.position + forward * 1.2f;
        startPos.y = view.position.y;
        root.transform.SetPositionAndRotation(startPos, Quaternion.LookRotation(forward, Vector3.up));
    }

    MRPlacementProfile ResolvePlacementProfile() =>
        cabinetInstance != null ? MRPlacementProfile.Resolve(cabinetInstance) : null;

    PlacementSurfaceType GetPlacementSurfaceType()
    {
        MRPlacementProfile profile = ResolvePlacementProfile();
        return profile != null ? profile.surfaceType : PlacementSurfaceType.Wall;
    }

    PlacementFacingAxis GetPlacementFacingAxis()
    {
        MRPlacementProfile profile = ResolvePlacementProfile();
        return profile != null ? profile.facingAxis : wallFacingAxis;
    }

    MRPlacementRayController EnsurePlacementRayController()
    {
        MRPlacementRayController existing = FindObjectOfType<MRPlacementRayController>();
        if (existing != null)
            return existing;

        GameObject host = new GameObject("MRPlacementRayController");
        return host.AddComponent<MRPlacementRayController>();
    }

    bool TryLoadSavedPose(out Vector3 worldPosition, out Quaternion worldRotation) =>
        TryLoadSavedPose(GetPlacementSurfaceType(), out worldPosition, out worldRotation);

    static bool TryLoadSavedPose(PlacementSurfaceType surface, out Vector3 worldPosition, out Quaternion worldRotation)
    {
        worldPosition = Vector3.zero;
        worldRotation = Quaternion.identity;
        int schema = PlayerPrefs.GetInt(SavedPoseFlagKey, 0);
        if (schema != SavedPoseSchemaWorld && schema != SavedPoseSchemaAnchorRelative && schema != 1)
            return false;

        string posJson = PlayerPrefs.GetString(SavedPosePositionKey, string.Empty);
        string rotJson = PlayerPrefs.GetString(SavedPoseRotationKey, string.Empty);
        if (string.IsNullOrEmpty(posJson) || string.IsNullOrEmpty(rotJson))
        {
            ClearSavedPose();
            return false;
        }

        MRVector3 pos = JsonUtility.FromJson<MRVector3>(posJson);
        MRQuaternion rot = JsonUtility.FromJson<MRQuaternion>(rotJson);
        if (pos == null || rot == null)
        {
            ClearSavedPose();
            return false;
        }

        Vector3 storedPosition = pos.ToVector3();
        Quaternion storedRotation = rot.ToQuaternion();

        if (schema == SavedPoseSchemaAnchorRelative)
        {
            string anchorUuidText = PlayerPrefs.GetString(SavedPoseAnchorUuidKey, string.Empty);
            Meta.XR.MRUtilityKit.MRUKRoom room = MREnvironmentSurfaces.Instance?.CurrentRoom;
            if (!MRAnchorPoseResolver.TryResolveWorldPose(
                    room,
                    anchorUuidText,
                    storedPosition,
                    storedRotation,
                    surface,
                    out worldPosition,
                    out worldRotation))
                return false;

            return true;
        }

        worldPosition = storedPosition;
        worldRotation = storedRotation;
        return true;
    }

    static void ClearSavedPose()
    {
        PlayerPrefs.DeleteKey(SavedPoseFlagKey);
        PlayerPrefs.DeleteKey(SavedPosePositionKey);
        PlayerPrefs.DeleteKey(SavedPoseRotationKey);
        PlayerPrefs.DeleteKey(SavedPoseAnchorUuidKey);
        PlayerPrefs.DeleteKey(SavedWorldPositionKey);
        PlayerPrefs.DeleteKey(SavedWorldRotationKey);
        PlayerPrefs.Save();
    }

    static void SaveWorldFallbackPose(Vector3 worldPosition, Quaternion worldRotation)
    {
        PlayerPrefs.SetString(SavedWorldPositionKey, JsonUtility.ToJson(MRVector3.From(worldPosition)));
        PlayerPrefs.SetString(SavedWorldRotationKey, JsonUtility.ToJson(MRQuaternion.From(worldRotation)));
        PlayerPrefs.Save();
    }

    static bool TryLoadWorldFallbackPose(out Vector3 worldPosition, out Quaternion worldRotation)
    {
        worldPosition = Vector3.zero;
        worldRotation = Quaternion.identity;

        string posJson = PlayerPrefs.GetString(SavedWorldPositionKey, string.Empty);
        string rotJson = PlayerPrefs.GetString(SavedWorldRotationKey, string.Empty);
        if (string.IsNullOrEmpty(posJson) || string.IsNullOrEmpty(rotJson))
            return false;

        MRVector3 pos = JsonUtility.FromJson<MRVector3>(posJson);
        MRQuaternion rot = JsonUtility.FromJson<MRQuaternion>(rotJson);
        if (pos == null || rot == null)
            return false;

        worldPosition = pos.ToVector3();
        worldRotation = rot.ToQuaternion();
        return true;
    }

    static bool HasSavedPose() => PlayerPrefs.HasKey(SavedPoseFlagKey);

    void SavePose(Vector3 worldPosition, Quaternion worldRotation, System.Guid anchorUuid = default)
    {
        PlacementSurfaceType surfaceType = GetPlacementSurfaceType();
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
            PlayerPrefs.SetString(SavedPosePositionKey, JsonUtility.ToJson(storedPosition));
            PlayerPrefs.SetString(SavedPoseRotationKey, JsonUtility.ToJson(storedRotation));
            PlayerPrefs.SetString(SavedPoseAnchorUuidKey, anchorUuidText);
            PlayerPrefs.SetInt(SavedPoseFlagKey, SavedPoseSchemaAnchorRelative);
        }
        else
        {
            PlayerPrefs.SetString(SavedPosePositionKey, JsonUtility.ToJson(MRVector3.From(worldPosition)));
            PlayerPrefs.SetString(SavedPoseRotationKey, JsonUtility.ToJson(MRQuaternion.From(worldRotation)));
            PlayerPrefs.DeleteKey(SavedPoseAnchorUuidKey);
            PlayerPrefs.SetInt(SavedPoseFlagKey, SavedPoseSchemaWorld);
        }

        SaveWorldFallbackPose(worldPosition, worldRotation);
        PlayerPrefs.Save();
    }

    static float ResolveFloorY(MREnvironmentSurfaces surfaces, Vector3 near, Transform player)
    {
        if (surfaces != null && surfaces.TryGetFloorPointAt(near, out Vector3 floorPoint))
            return floorPoint.y;

        if (surfaces != null && surfaces.HasFloor)
            return surfaces.FloorHeight;

        int floorMask = LayerMask.GetMask("floor");
        if (floorMask != 0)
        {
            Vector3 rayStart = new Vector3(near.x, (player != null ? player.position.y : near.y) + 2f, near.z);
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 8f, floorMask))
                return hit.point.y;
        }

        if (player != null)
            return player.position.y - 1.6f;

        return near.y;
    }

#if UNITY_EDITOR
    IEnumerator EditorAutoSpawnWhenReady()
    {
        yield return new WaitForSeconds(editorAutoSpawnDelaySeconds);

        MREnvironmentSurfaces surfaces = MREnvironmentSurfaces.Instance;
        if (surfaces != null && !surfaces.IsReady)
            yield return surfaces.ProbeWhenReady(FindPlayerTransform());

        if (HasCabinet)
            yield break;

        if (MixedRealityManager.Instance != null
            && MixedRealityManager.Instance.CurrentMode == ExperienceMode.MR)
        {
            SpawnAtMrOrigin();
            yield break;
        }

        SpawnForEditorTest();
        OpenEdit();
    }

    void SpawnForEditorTest()
    {
        if (HasCabinet)
            return;

        GameObject prefab = ResolveCabinetPrefab();
        if (prefab == null)
            return;

        AdoptCabinetInstance(Instantiate(prefab), wasInstantiated: true);
        cabinetInstance.name = "ConfigurationCabinetMiniMR_EditorTest";
#if UNITY_EDITOR
        TryAlignEditorCameraToScreen();
#endif
        ConfigManager.WriteConsole($"{LogPrefix} editor test cabinet at {cabinetInstance.transform.position}");
    }

    void TryAlignEditorCameraToScreen()
    {
        if (!Application.isEditor || !IsTestScene() || !editorAlignCameraAfterInstantiate)
            return;
        if (cabinetInstance == null)
            return;

        Camera sceneMain = Camera.main;
        Camera cabinetCamera = cabinetInstance.GetComponentInChildren<Camera>(true);
        if (cabinetCamera == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} editor camera setup skipped (no Camera under cabinet prefab)");
            return;
        }

        if (!cabinetCamera.gameObject.activeSelf)
            cabinetCamera.gameObject.SetActive(true);
        cabinetCamera.enabled = true;

        if (editorDisableSceneMainCamera && sceneMain != null && sceneMain != cabinetCamera)
            sceneMain.gameObject.SetActive(false);

        ConfigManager.WriteConsole(
            $"{LogPrefix} editor enabled cabinet camera '{cabinetCamera.name}'");
    }

    void TryAutoInsertCoinForEditor()
    {
        if (!Application.isEditor || !editorAutoInsertCoinOnSpawn || cabinetInstance == null)
            return;

        CoinSlotController coinSlot = FindCoinSlot(cabinetInstance);
        if (coinSlot == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} editor auto-coin skipped (CoinSlotController missing)");
            return;
        }

        StartCoroutine(EditorInsertCoinWhenReady(coinSlot));
    }

    IEnumerator EditorInsertCoinWhenReady(CoinSlotController coinSlot)
    {
        const int maxFrames = 8;
        for (int i = 0; i < maxFrames; i++)
        {
            if (coinSlot == null)
                yield break;

            if (!coinSlot.isActiveAndEnabled)
            {
                yield return null;
                continue;
            }

            bool shouldRetry = false;
            try
            {
                coinSlot.insertCoin();
                ConfigManager.WriteConsole($"{LogPrefix} editor auto-coin inserted on spawn");
                if (editorAutoOpenEditOnSpawn)
                    StartCoroutine(EditorOpenEditWhenReady());
                yield break;
            }
            catch (System.NullReferenceException)
            {
                // CoinSlotController initializes dependencies in Start(); retry on next frame.
                shouldRetry = true;
            }

            if (shouldRetry)
                yield return null;
        }

        ConfigManager.WriteConsoleWarning($"{LogPrefix} editor auto-coin failed (CoinSlotController not ready)");
        if (editorAutoOpenEditOnSpawn)
            StartCoroutine(EditorOpenEditWhenReady());
    }

    IEnumerator EditorOpenEditWhenReady()
    {
        const int maxFrames = 20;
        for (int i = 0; i < maxFrames; i++)
        {
            if (!Application.isEditor || cabinetInstance == null)
                yield break;

            if (isEditOpen)
                yield break;

            if (MixedRealityManager.Instance == null || crtController == null)
            {
                yield return null;
                continue;
            }

            OpenEdit();
            if (isEditOpen)
            {
                ConfigManager.WriteConsole($"{LogPrefix} editor auto-opened CRT session");
                yield break;
            }

            yield return null;
        }

        ConfigManager.WriteConsoleWarning($"{LogPrefix} editor auto-open failed (timed out)");
    }
#endif

    static Transform FindPlayerTransform()
    {
        var pc = FindObjectOfType<PlayerController>();
        if (pc != null && pc.PlayerControllerGameObject != null)
            return pc.PlayerControllerGameObject.transform;
        if (pc != null)
            return pc.transform;

        var tagged = GameObject.FindGameObjectWithTag("Player");
        return tagged != null ? tagged.transform : null;
    }

    static Vector3 ResolveEyePosition(Transform player)
    {
        if (Camera.main != null)
            return Camera.main.transform.position;

        var pc = FindObjectOfType<PlayerController>();
        if (pc != null && pc.xrorigin != null && pc.xrorigin.Camera != null)
            return pc.xrorigin.Camera.transform.position;

        Vector3 basePos = player != null ? player.position : Vector3.zero;
        return basePos + Vector3.up * 1.6f;
    }

    static Vector3 ResolveViewForward(Transform player)
    {
        Vector3 forward;
        if (Camera.main != null)
            forward = Camera.main.transform.forward;
        else
            forward = player != null ? player.forward : Vector3.forward;

        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;
        return forward.normalized;
    }

}
