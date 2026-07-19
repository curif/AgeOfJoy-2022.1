/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections;
using AOJ.Managers;
using Meta.XR.MRUtilityKit;
using UnityEngine;

public class MixedRealityManager : MonoBehaviour
{
    public const string MrRoomName = "MR";
    const string LogPrefix = "[MixedRealityManager]";

    public static MixedRealityManager Instance { get; private set; }

    public ExperienceMode CurrentMode { get; private set; } = ExperienceMode.VR;
    public Transform MRSpaceOrigin { get; private set; }
    public event Action<ExperienceMode> OnModeChanged;

    MRPassthroughController passthrough;
    MRSceneTransition sceneTransition;
    MRLayoutRegistry layoutRegistry;
    MREnvironmentRegistry environmentRegistry;
    MRMrEnvironmentLighting mrLighting;
    MREffectMeshController mrEffectMesh;
    MREnvironmentSurfaces environmentSurfaces;
    bool transitionInProgress;
    Coroutine runningTransition;
    Coroutine mrukEnvironmentRefreshRoutine;
    Coroutine phoneBoothVrToMrScanRoutine;
    Coroutine reapplyPosesAfterFocusRoutine;
    Coroutine roomLayoutSwitchRoutine;
    bool roomLayoutSwitchInProgress;
    int transitionGeneration;
    int focusReturnGeneration;
    int lastTrackingRecenterCount = -1;
    OVRDisplay subscribedDisplay;
    bool hasFloorAnchorCache;
    Vector3 cachedFloorAnchorPosition;
    Quaternion cachedFloorAnchorRotation = Quaternion.identity;
    const float FloorAnchorJumpMeters = 0.12f;
    int lastDiscontinuityFrame = -1;
    float nextRoomLayoutPollTime;

    const float MrukEnvironmentRefreshDebounceSeconds = 0.75f;
    const float RoomLayoutPollIntervalSeconds = 0.5f;

    Vector3? savedVrPlayerPosition;
    Quaternion? savedVrPlayerRotation;
    /// <summary>
    /// XROrigin CameraFloorOffset local pose at app start. WorldLock pushes this transform in MR;
    /// restore it on VR exit or the player floats above the VR floor.
    /// </summary>
    Vector3? appStartCameraFloorLocalPosition;
    Quaternion? appStartCameraFloorLocalRotation;
    Vector3? appStartPlayerControllerLocalPosition;

    const string SavedMrPlayerPositionKey = "MR.LastSession.PlayerPosition";
    const string SavedMrPlayerRotationKey = "MR.LastSession.PlayerRotation";

    /// <summary>True when VR gallery rules (CabinetsController slots, registry rooms) should run.</summary>
    public static bool IsVrExperience =>
        Instance == null || Instance.CurrentMode == ExperienceMode.VR;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        MRPaths.EnsureFolders();

        passthrough = GetComponent<MRPassthroughController>();
        if (passthrough == null)
            passthrough = gameObject.AddComponent<MRPassthroughController>();

        sceneTransition = GetComponent<MRSceneTransition>();
        if (sceneTransition == null)
            sceneTransition = gameObject.AddComponent<MRSceneTransition>();

        layoutRegistry = GetComponent<MRLayoutRegistry>();
        if (layoutRegistry == null)
            layoutRegistry = gameObject.AddComponent<MRLayoutRegistry>();

        environmentRegistry = GetComponent<MREnvironmentRegistry>();
        if (environmentRegistry == null)
            environmentRegistry = gameObject.AddComponent<MREnvironmentRegistry>();

        mrLighting = GetComponent<MRMrEnvironmentLighting>();
        if (mrLighting == null)
            mrLighting = gameObject.AddComponent<MRMrEnvironmentLighting>();

        if (GetComponent<MRGameplayLightingGate>() == null)
            gameObject.AddComponent<MRGameplayLightingGate>();

        mrEffectMesh = GetComponent<MREffectMeshController>();
        if (mrEffectMesh == null)
            mrEffectMesh = gameObject.AddComponent<MREffectMeshController>();

        environmentSurfaces = GetComponent<MREnvironmentSurfaces>();
        if (environmentSurfaces == null)
            environmentSurfaces = gameObject.AddComponent<MREnvironmentSurfaces>();

        if (GetComponent<MRRoomInfoUI>() == null)
            gameObject.AddComponent<MRRoomInfoUI>();

        if (GetComponent<MREditMenuInput>() == null)
            gameObject.AddComponent<MREditMenuInput>();

        if (GetComponent<MRConfigurationCabinetController>() == null)
            gameObject.AddComponent<MRConfigurationCabinetController>();

        MRRuntimeSettings.LogMissingInstanceOnce("MixedRealityManager.Awake");

        if (GetComponent<MREditorMrSimulator>() == null)
            gameObject.AddComponent<MREditorMrSimulator>();

        passthrough.Initialize();
        ConfigManager.WriteConsole($"{LogPrefix} ready (mode={CurrentMode})");
        MRTransitionLog.EnsureSession("MixedRealityManager.Awake");
        MRTransitionLog.Log($"MixedRealityManager ready logFile={MRTransitionLog.LogFilePath}");
    }

    void OnEnable()
    {
        OVRManager.HMDMounted += OnHmdMounted;
        OVRManager.InputFocusAcquired += OnInputFocusAcquired;
        SubscribeDisplayRecenter();
    }

    void OnDisable()
    {
        OVRManager.HMDMounted -= OnHmdMounted;
        OVRManager.InputFocusAcquired -= OnInputFocusAcquired;
        UnsubscribeDisplayRecenter();
    }

    void Start()
    {
        SubscribeDisplayRecenter();
        if (OVRPlugin.initialized)
            lastTrackingRecenterCount = OVRPlugin.GetLocalTrackingSpaceRecenterCount();

        CaptureAppStartTrackingOffsetIfNeeded();
        if (!appStartCameraFloorLocalPosition.HasValue)
            StartCoroutine(CaptureAppStartTrackingOffsetWhenReady());

        if (ShouldAutoEnterMrOnFixedSceneBoot())
            StartCoroutine(AutoEnterMrOnFixedSceneBootRoutine());
    }

    IEnumerator CaptureAppStartTrackingOffsetWhenReady()
    {
        float timeout = 5f;
        while (!appStartCameraFloorLocalPosition.HasValue && timeout > 0f)
        {
            CaptureAppStartTrackingOffsetIfNeeded();
            if (appStartCameraFloorLocalPosition.HasValue)
                yield break;
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }
    }

    void CaptureAppStartTrackingOffsetIfNeeded()
    {
        if (appStartCameraFloorLocalPosition.HasValue)
            return;

        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc == null)
            return;

        if (pc.PlayerControllerGameObject != null)
            appStartPlayerControllerLocalPosition = pc.PlayerControllerGameObject.transform.localPosition;

        Transform floor = ResolveCameraFloorOffsetTransform(pc);
        if (floor == null)
            return;

        appStartCameraFloorLocalPosition = floor.localPosition;
        appStartCameraFloorLocalRotation = floor.localRotation;
        MRTransitionLog.Log(
            $"captured app start CameraFloorOffset localPos={appStartCameraFloorLocalPosition.Value}");
    }

    static Transform ResolveCameraFloorOffsetTransform(PlayerController pc)
    {
        if (pc == null)
            return null;
        if (pc.xrorigin != null && pc.xrorigin.CameraFloorOffsetObject != null)
            return pc.xrorigin.CameraFloorOffsetObject.transform;
        return pc.cameraOffset;
    }

    void LateUpdate()
    {
        if (!IsMrEnvironmentActive() || transitionInProgress)
        {
            hasFloorAnchorCache = false;
            return;
        }

        bool recenteredByCount = false;
        if (OVRPlugin.initialized)
        {
            int recenterCount = OVRPlugin.GetLocalTrackingSpaceRecenterCount();
            if (lastTrackingRecenterCount < 0)
                lastTrackingRecenterCount = recenterCount;
            else if (recenterCount != lastTrackingRecenterCount)
            {
                lastTrackingRecenterCount = recenterCount;
                recenteredByCount = true;
            }
        }

        if (!TryGetFloorAnchorPose(out Vector3 floorPos, out Quaternion floorRot))
        {
            if (recenteredByCount)
                HandleTrackingDiscontinuity("TrackingSpaceRecenterCount", applyFloorDelta: false);
            return;
        }

        bool floorJumped = hasFloorAnchorCache
            && Vector3.Distance(floorPos, cachedFloorAnchorPosition) >= FloorAnchorJumpMeters;

        if (recenteredByCount || floorJumped)
        {
            string reason = recenteredByCount ? "TrackingSpaceRecenterCount" : "FloorAnchorJump";
            HandleTrackingDiscontinuity(reason, applyFloorDelta: hasFloorAnchorCache, floorPos, floorRot);
        }

        cachedFloorAnchorPosition = floorPos;
        cachedFloorAnchorRotation = floorRot;
        hasFloorAnchorCache = true;

        PollActiveRoomLayoutSwitch();
    }

    /// <summary>
    /// Phase A: when the headset moves into another MRUK room, swap layout files and respawn.
    /// </summary>
    void PollActiveRoomLayoutSwitch()
    {
        if (Time.unscaledTime < nextRoomLayoutPollTime)
            return;

        nextRoomLayoutPollTime = Time.unscaledTime + RoomLayoutPollIntervalSeconds;

        if (roomLayoutSwitchRoutine != null || roomLayoutSwitchInProgress || transitionInProgress)
            return;

        if (MRPlacementRayController.AnyActive)
            return;

        if (!MRRoomIdentity.TryGetCurrentRoomId(out string roomId))
            return;

        if (string.Equals(roomId, MRActiveRoom.BoundRoomId, StringComparison.OrdinalIgnoreCase))
            return;

        roomLayoutSwitchRoutine = StartCoroutine(SwitchActiveRoomLayoutCoroutine(roomId));
    }

    IEnumerator SwitchActiveRoomLayoutCoroutine(string roomId)
    {
        if (roomLayoutSwitchInProgress)
            yield break;

        roomLayoutSwitchInProgress = true;
        try
        {
            ConfigManager.WriteConsole(
                $"{LogPrefix} room layout switch {MRActiveRoom.BoundRoomId ?? "(none)"} -> {roomId}");

            ActiveRegistry()?.SnapshotSpawnedWorldPosesToLayout();
            ActiveEnvironmentRegistry()?.SnapshotSpawnedWorldPosesToLayout();

            if (!MRActiveRoom.Bind(roomId))
                yield break;

            CancelActivePlacementRay();

            MRLayoutRegistry layout = ActiveRegistry();
            if (layout != null)
                yield return layout.SpawnAllAsync(MRSpaceOrigin);

            MREnvironmentRegistry environment = ActiveEnvironmentRegistry();
            if (environment != null)
                yield return environment.SpawnAllAsync(MRSpaceOrigin);

            layout?.EnsureAttractPlaybackOnSpawned();
            MRRoomInfoUI.Instance?.RefreshContent();
        }
        finally
        {
            roomLayoutSwitchInProgress = false;
            roomLayoutSwitchRoutine = null;
        }
    }

    /// <summary>Bind cabinets/objects YAML paths to the current scanned room (Phase A).</summary>
    public void EnsureLayoutPathsBoundForEditSession() => BindLayoutPathsToCurrentRoom();

    void BindLayoutPathsToCurrentRoom()
    {
        if (MRActiveRoom.TryBindFromDevice())
        {
            ConfigManager.WriteConsole(
                $"{LogPrefix} layout paths bound to room {MRActiveRoom.BoundRoomId}");
            return;
        }

        if (MRActiveRoom.HasBoundRoom)
        {
            ConfigManager.WriteConsole(
                $"{LogPrefix} keeping bound room {MRActiveRoom.BoundRoomId} (device room id unavailable)");
            return;
        }

        ConfigManager.WriteConsoleWarning(
            $"{LogPrefix} no room id — using global MR layout paths");
    }

    void HandleTrackingDiscontinuity(
        string reason,
        bool applyFloorDelta,
        Vector3 newFloorPos = default,
        Quaternion newFloorRot = default)
    {
        bool alreadyHandledThisFrame = lastDiscontinuityFrame == Time.frameCount;
        lastDiscontinuityFrame = Time.frameCount;

        // With WorldLock driving XROrigin, floor/wall anchors should stay colocated with
        // passthrough. Do not also shove spawned props by floor delta — that double-corrects.
        bool worldLockActive = MRUK.Instance != null && MRUK.Instance.IsWorldLockActive;
        if (applyFloorDelta && hasFloorAnchorCache && !alreadyHandledThisFrame && !worldLockActive)
        {
            ApplyFloorAnchorDeltaToMrContent(
                cachedFloorAnchorPosition,
                cachedFloorAnchorRotation,
                newFloorPos,
                newFloorRot);
            MRTransitionLog.LogStep(
                "TrackingDiscontinuity",
                $"{reason} applied floor delta move={Vector3.Distance(cachedFloorAnchorPosition, newFloorPos):F3}m");
        }
        else if (!alreadyHandledThisFrame)
        {
            MRTransitionLog.LogStep(
                "TrackingDiscontinuity",
                $"{reason} worldLockActive={worldLockActive} (skip floor delta, reapply anchors)");
        }

        // Let WorldLock settle on XROrigin, then re-resolve props from anchors only.
        MRCameraRigShim.Align();
        ScheduleReapplyMrPosesAfterFocusReturn(reason, settleSeconds: 0.45f, retryCount: 4, anchorsOnly: true);
    }

    static bool TryGetFloorAnchorPose(out Vector3 position, out Quaternion rotation)
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;

        MRUKRoom room = MREnvironmentSurfaces.Instance?.CurrentRoom;
        if (room == null && MRUK.Instance != null)
            room = MRUK.Instance.GetCurrentRoom();

        MRUKAnchor floor = room != null ? room.FloorAnchor : null;
        if (floor == null)
            return false;

        position = floor.transform.position;
        rotation = floor.transform.rotation;
        return true;
    }

    void ApplyFloorAnchorDeltaToMrContent(
        Vector3 oldFloorPos,
        Quaternion oldFloorRot,
        Vector3 newFloorPos,
        Quaternion newFloorRot)
    {
        Quaternion deltaRot = newFloorRot * Quaternion.Inverse(oldFloorRot);

        void TransformRoot(Transform root)
        {
            if (root == null)
                return;

            Vector3 local = Quaternion.Inverse(oldFloorRot) * (root.position - oldFloorPos);
            root.SetPositionAndRotation(
                newFloorPos + deltaRot * local,
                deltaRot * root.rotation);
        }

        ActiveRegistry()?.ForEachSpawnedRoot(TransformRoot);
        ActiveEnvironmentRegistry()?.ForEachSpawnedRoot(TransformRoot);

        GameObject configCabinet = MRConfigurationCabinetController.Instance != null
            ? MRConfigurationCabinetController.Instance.CabinetInstance
            : null;
        if (configCabinet != null)
            TransformRoot(configCabinet.transform);

        MRPhoneBoothPortal booth = MRPhoneBoothPortal.FindMrTravelerInstance(includeInactive: false);
        if (booth != null)
            TransformRoot(booth.transform);
    }

    void SubscribeDisplayRecenter()
    {
        if (subscribedDisplay != null)
            return;

        OVRDisplay display = OVRManager.display;
        if (display == null)
            return;

        subscribedDisplay = display;
        subscribedDisplay.RecenteredPose += OnTrackingRecentered;
    }

    void UnsubscribeDisplayRecenter()
    {
        if (subscribedDisplay == null)
            return;

        subscribedDisplay.RecenteredPose -= OnTrackingRecentered;
        subscribedDisplay = null;
    }

    static bool ShouldAutoEnterMrOnFixedSceneBoot()
    {
        if (!MRRuntimeSettings.AutoEnterMrOnFixedSceneBoot)
            return false;

        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == MRRuntimeSettings.FixedScene;
    }

    IEnumerator AutoEnterMrOnFixedSceneBootRoutine()
    {
        yield return null;

        if (CurrentMode == ExperienceMode.MR || CurrentMode == ExperienceMode.MR_EDIT)
            yield break;

        if (transitionInProgress)
            yield break;

        ConfigManager.WriteConsole($"{LogPrefix} auto EnterMR from FixedScene boot");
        EnterMRDirectFromBoot();
    }

    void OnDestroy()
    {
        OVRManager.HMDMounted -= OnHmdMounted;
        OVRManager.InputFocusAcquired -= OnInputFocusAcquired;
        UnsubscribeDisplayRecenter();
        if (Instance == this)
            Instance = null;
    }

    void OnTrackingRecentered()
    {
        if (OVRPlugin.initialized)
            lastTrackingRecenterCount = OVRPlugin.GetLocalTrackingSpaceRecenterCount();

        if (TryGetFloorAnchorPose(out Vector3 floorPos, out Quaternion floorRot) && hasFloorAnchorCache)
        {
            HandleTrackingDiscontinuity("RecenteredPose", applyFloorDelta: true, floorPos, floorRot);
            cachedFloorAnchorPosition = floorPos;
            cachedFloorAnchorRotation = floorRot;
            return;
        }

        ScheduleReapplyMrPosesAfterFocusReturn("RecenteredPose", settleSeconds: 0.35f, retryCount: 4, anchorsOnly: true);
    }

    void OnHmdMounted()
    {
        ScheduleReapplyMrPosesAfterFocusReturn("HMDMounted", settleSeconds: 0.55f, retryCount: 4, anchorsOnly: true);
    }

    void OnInputFocusAcquired()
    {
        ScheduleReapplyMrPosesAfterFocusReturn("InputFocusAcquired", settleSeconds: 0.25f, retryCount: 2, anchorsOnly: true);
    }

    void OnApplicationPause(bool paused)
    {
        // Do not snapshot on pause: Meta system menu / HMD remove / tracking discontinuity
        // can bake drifted world poses into YAML. Placement edits already save on confirm.
        if (!paused)
            ScheduleReapplyMrPosesAfterFocusReturn("OnApplicationPause-resume", settleSeconds: 0.4f, retryCount: 3, anchorsOnly: true);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            ScheduleReapplyMrPosesAfterFocusReturn("OnApplicationFocus", settleSeconds: 0.25f, retryCount: 2, anchorsOnly: true);
    }

    void OnApplicationQuit()
    {
        PersistMrLayoutPoses("OnApplicationQuit");
    }

    /// <summary>Write live transforms to MR/cabinets-layout.yaml and MR/objects-layout.yaml (MR exit or quit).</summary>
    public void PersistMrLayoutPoses(string reason)
    {
        if (CurrentMode != ExperienceMode.MR && CurrentMode != ExperienceMode.MR_EDIT)
            return;

        MRTransitionLog.LogStep("PersistMrLayoutPoses", reason);
        ActiveRegistry()?.SnapshotSpawnedWorldPosesToLayout();
        ActiveEnvironmentRegistry()?.SnapshotSpawnedWorldPosesToLayout();
    }

    void ScheduleReapplyMrPosesAfterFocusReturn(
        string reason,
        float settleSeconds = 0.25f,
        int retryCount = 2,
        bool anchorsOnly = false)
    {
        if (!IsMrEnvironmentActive() || transitionInProgress)
            return;

        focusReturnGeneration++;
        if (reapplyPosesAfterFocusRoutine != null)
            StopCoroutine(reapplyPosesAfterFocusRoutine);
        reapplyPosesAfterFocusRoutine = StartCoroutine(
            ReapplyMrPosesAfterFocusReturn(focusReturnGeneration, reason, settleSeconds, retryCount, anchorsOnly));
    }

    IEnumerator ReapplyMrPosesAfterFocusReturn(
        int generation,
        string reason,
        float settleSeconds,
        int retryCount,
        bool anchorsOnly)
    {
        // Let MRUK WorldLock / tracking settle after Meta Reset View or HMD remount.
        float waited = 0f;
        while (waited < settleSeconds)
        {
            if (generation != focusReturnGeneration)
            {
                reapplyPosesAfterFocusRoutine = null;
                yield break;
            }

            waited += Time.unscaledDeltaTime;
            yield return null;
        }

        int attempts = Mathf.Max(1, retryCount);
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            if (generation != focusReturnGeneration || !IsMrEnvironmentActive() || transitionInProgress)
                break;

            MRTransitionLog.LogStep(
                "ReapplyMrPosesAfterFocusReturn",
                $"{reason} attempt={attempt + 1}/{attempts} anchorsOnly={anchorsOnly}");
            MRCameraRigShim.Align();
            ActiveRegistry()?.RefreshAllSpawnedPosesFromLayout(floorOnly: false, anchorsOnly: anchorsOnly);
            ActiveEnvironmentRegistry()?.RefreshAllSpawnedPosesFromLayout(anchorsOnly: anchorsOnly);
            MRConfigurationCabinetController.Instance?.RefreshPoseForMrReentry();

            if (TryGetFloorAnchorPose(out Vector3 floorPos, out Quaternion floorRot))
            {
                cachedFloorAnchorPosition = floorPos;
                cachedFloorAnchorRotation = floorRot;
                hasFloorAnchorCache = true;
            }

            if (attempt + 1 >= attempts)
                break;

            float retryWait = 0f;
            while (retryWait < 0.35f)
            {
                if (generation != focusReturnGeneration)
                {
                    reapplyPosesAfterFocusRoutine = null;
                    yield break;
                }

                retryWait += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        ConfigManager.WriteConsole($"{LogPrefix} reapplied MR poses after focus return ({reason})");
        reapplyPosesAfterFocusRoutine = null;
    }

    public bool CanToggleMode() => !transitionInProgress && (sceneTransition == null || !sceneTransition.IsTransitionRunning);

    public bool TransitionInProgress => transitionInProgress;

    /// <summary>MR→VR phone booth travel — turn off passthrough before opaque cabin VFX.</summary>
    public void DisablePassthroughForPhoneBoothTravel()
    {
        if (!IsMrEnvironmentActive())
            return;

        passthrough?.DisablePassthrough(playFadeOut: false);
    }

    /// <summary>Keep HMD black during phone-booth travel (camera cull + solid clear).</summary>
    public void HoldPhoneBoothTravelBlackout()
    {
        passthrough?.BeginTransitionBlackout(triggerFadeInAnimator: false, restoreFadeSphere: false);
        MRPhoneBoothTravelHeadFade.ReassertActiveTravelBlackout();
    }

    /// <summary>True when MR visuals/content are active (uses runtime state, not only CurrentMode).</summary>
    public bool IsMrEnvironmentActive()
    {
        if (CurrentMode == ExperienceMode.MR || CurrentMode == ExperienceMode.MR_EDIT)
            return true;

        if (passthrough != null && passthrough.IsPassthroughActive)
            return true;

        if (layoutRegistry != null && layoutRegistry.SpawnedCount > 0)
            return true;

        if (MRConfigurationCabinetController.Instance != null
            && MRConfigurationCabinetController.Instance.HasVisibleCabinet)
            return true;

        return MRSpaceOrigin != null;
    }

    public void EnterMR()
    {
        if (MRRuntimeSettings.IsMrEntryBlocked("EnterMR"))
            return;

        MRTransitionLog.LogStep("EnterMR", "requested");
        MRTransitionLog.LogManagerState("EnterMR-begin");
        MRTransitionLog.LogScenes("EnterMR-begin");

        if (CurrentMode == ExperienceMode.MR || CurrentMode == ExperienceMode.MR_EDIT)
        {
            MRTransitionLog.LogWarning("EnterMR ignored — already in MR mode");
            ConfigManager.WriteConsoleWarning($"{LogPrefix} EnterMR ignored — already in MR mode");
            return;
        }

        if (transitionInProgress)
        {
            MRTransitionLog.LogWarning("EnterMR ignored — transition already in progress");
            return;
        }

        MRTransitionLog.EnsureSession("EnterMR");
        BeginTransition(EnterMRCoroutine(directBoot: false));
    }

    void EnterMRDirectFromBoot()
    {
        if (MRRuntimeSettings.IsMrEntryBlocked("EnterMRDirectFromBoot"))
            return;

        MRTransitionLog.LogStep("EnterMRDirectFromBoot", "requested");
        MRTransitionLog.LogManagerState("EnterMRDirectFromBoot-begin");
        MRTransitionLog.LogScenes("EnterMRDirectFromBoot-begin");

        if (CurrentMode == ExperienceMode.MR || CurrentMode == ExperienceMode.MR_EDIT)
        {
            MRTransitionLog.LogWarning("EnterMRDirectFromBoot ignored — already in MR mode");
            return;
        }

        if (transitionInProgress)
        {
            MRTransitionLog.LogWarning("EnterMRDirectFromBoot ignored — transition already in progress");
            return;
        }

        MRTransitionLog.EnsureSession("EnterMRDirectFromBoot");
        BeginTransition(EnterMRCoroutine(directBoot: true));
    }

    /// <summary>
    /// TestConfig / editor room: skip VR unload and passthrough; probe MRUK (or SimulateRoom fallback) and spawn YAML layouts.
    /// </summary>
    public void EnterTestSceneMr(Vector3 originPosition, Quaternion originRotation)
    {
        if (MRRuntimeSettings.IsMrEntryBlocked("EnterTestSceneMr"))
            return;

        if (CurrentMode == ExperienceMode.MR || CurrentMode == ExperienceMode.MR_EDIT)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} EnterTestSceneMr ignored — already in MR mode");
            return;
        }

        if (transitionInProgress)
            return;

        MRTransitionLog.EnsureSession("EnterTestSceneMr");
        BeginTransition(EnterTestSceneMrCoroutine(originPosition, originRotation));
    }

    /// <summary>Immersive VR→MR via phone booth — player stays inside the rescued booth.</summary>
    public void EnterMRFromPhoneBooth(MRPhoneBoothPortal portal)
    {
        if (MRRuntimeSettings.IsMrEntryBlocked("EnterMRFromPhoneBooth"))
            return;

        MRTransitionLog.LogStep("EnterMRFromPhoneBooth", "requested");
        MRTransitionLog.LogManagerState("EnterMRFromPhoneBooth-begin");

        if (portal == null)
        {
            MRTransitionLog.LogWarning("EnterMRFromPhoneBooth ignored — portal null");
            return;
        }

        if (CurrentMode == ExperienceMode.MR || CurrentMode == ExperienceMode.MR_EDIT)
        {
            MRTransitionLog.LogWarning("EnterMRFromPhoneBooth ignored — already in MR mode");
            return;
        }

        if (transitionInProgress)
        {
            MRTransitionLog.LogWarning("EnterMRFromPhoneBooth ignored — transition already in progress");
            return;
        }

        MRTransitionLog.EnsureSession("EnterMRFromPhoneBooth");
        BeginTransition(EnterMRFromPhoneBoothCoroutine(portal));
    }

    /// <summary>
    /// VR→MR phone booth: verify Quest room scan before immersive travel (runs on DontDestroyOnLoad).
    /// If Space Setup runs, reloads VR and waits — player grabs the handset again to travel.
    /// </summary>
    public void StartPhoneBoothVrToMrTravel(MRPhoneBoothPortal portal, PhoneBoothTravelState travelState)
    {
        if (MRRuntimeSettings.IsMrEntryBlocked("StartPhoneBoothVrToMrTravel"))
            return;

        if (portal == null)
            return;

        if (phoneBoothVrToMrScanRoutine != null)
        {
            StopCoroutine(phoneBoothVrToMrScanRoutine);
            phoneBoothVrToMrScanRoutine = null;
            ConfigManager.WriteConsoleWarning($"{LogPrefix} phone booth VR→MR scan gate restarted");
        }

        phoneBoothVrToMrScanRoutine = StartCoroutine(PhoneBoothVrToMrTravelWithScanGateRoutine(portal, travelState));
    }

    IEnumerator PhoneBoothVrToMrTravelWithScanGateRoutine(
        MRPhoneBoothPortal portal,
        PhoneBoothTravelState travelState)
    {
        try
        {
            MRTransitionLog.LogStep("PhoneBoothVrToMr", "MRUK deferred to BeginJourneyVisuals");
            portal.StartImmersiveTravelToMrAfterScanGate();
        }
        finally
        {
            phoneBoothVrToMrScanRoutine = null;
        }

        yield break;
    }

    /// <summary>Reload gallery after Space Setup during phone booth VR→MR travel.</summary>
    public IEnumerator ReloadVrScenesForPhoneBoothScanGate()
    {
        MRTransitionLog.LogStep("PhoneBoothVrToMr", "reload VR scenes after Space Setup");
        yield return sceneTransition.ReloadVrScenes();

        if (MRRuntimeSettings.RefreshCameraOffsetAfterPhoneBoothReturn)
            RefreshPlayerControllerCameraOffset();
    }

    /// <summary>Saves gallery player pose before phone booth VR→MR travel (restored on booth return).</summary>
    public void RememberVrPlayerPoseForPhoneBoothTravel()
    {
        if (!MRRuntimeSettings.RememberVrPoseOnPhoneBoothTravelToMr)
            return;

        RememberVrPlayerPose();
    }

    /// <summary>Immersive MR→VR via phone booth — scene booth pose is authoritative.</summary>
    public void EnterVRFromPhoneBooth(MRPhoneBoothPortal portal)
    {
        MRTransitionLog.LogStep("EnterVRFromPhoneBooth", "requested");
        MRTransitionLog.LogManagerState("EnterVRFromPhoneBooth-begin");

        if (portal == null)
        {
            MRTransitionLog.LogWarning("EnterVRFromPhoneBooth ignored — portal null");
            return;
        }

        if (!IsMrEnvironmentActive())
        {
            MRTransitionLog.LogWarning("EnterVRFromPhoneBooth ignored — MR environment not active");
            return;
        }

        if (transitionInProgress)
        {
            MRTransitionLog.LogWarning("EnterVRFromPhoneBooth ignored — transition already in progress");
            return;
        }

        MRTransitionLog.LogStep("EnterVRFromPhoneBooth", "immediate passthrough off");
        passthrough.DisablePassthrough(playFadeOut: false);
        ResetLegacyPassthroughFlags();
        BeginMrExitImmediateSync();
        BeginTransition(EnterVRFromPhoneBoothCoroutine(portal));
    }

    public void EnterVR()
    {
        MRTransitionLog.LogStep("EnterVR", "requested");
        MRTransitionLog.LogManagerState("EnterVR-begin");
        MRTransitionLog.LogScenes("EnterVR-begin");
        MRTransitionLog.LogPassthrough("EnterVR-begin", passthrough);

        if (!IsMrEnvironmentActive())
        {
            MRTransitionLog.LogWarning("EnterVR ignored — MR environment not active");
            ConfigManager.WriteConsoleWarning($"{LogPrefix} EnterVR ignored — MR environment not active");
            return;
        }

        if (transitionInProgress)
        {
            MRTransitionLog.LogWarning("EnterVR ignored — transition already in progress");
            return;
        }

        MRTransitionLog.LogStep("EnterVR", "immediate passthrough off");
        passthrough.DisablePassthrough(playFadeOut: false);
        ResetLegacyPassthroughFlags();
        BeginMrExitImmediateSync();

        MRTransitionLog.LogPassthrough("EnterVR-after-immediate-disable", passthrough);
        BeginTransition(EnterVRCoroutine());
    }

    /// <summary>Fast sync hide only — Destroy deferred to EnterVRCoroutine (Libretro may block).</summary>
    void BeginMrExitImmediateSync()
    {
        MRTransitionLog.LogStep("BeginMrExitImmediateSync");
        CancelActivePlacementRay();
        RememberMrPlayerPose();
        MRConfigurationCabinetController.Instance?.ForceCloseEdit();
        MRVrSystemsGate.SilenceAllCabinetScreensForPhoneBoothTravelToVr();
        PersistMrLayoutPoses("BeginMrExitImmediateSync");
        MRConfigurationCabinetController.Instance?.HideForMrExit();
        mrLighting?.DespawnForMrExit();
        mrEffectMesh?.Despawn();
        int hidden = ActiveRegistry()?.HideAllMrCabinetsImmediateCount() ?? 0;
        int hiddenEnv = ActiveEnvironmentRegistry()?.HideAllImmediateCount() ?? 0;
        MRTransitionLog.Log($"sync hide done mrCabinetsHidden={hidden} envPropsHidden={hiddenEnv}");
    }

    MRLayoutRegistry ActiveRegistry() => MRLayoutRegistry.Instance ?? layoutRegistry;

    MREnvironmentRegistry ActiveEnvironmentRegistry() => MREnvironmentRegistry.Instance ?? environmentRegistry;

    public void EnterMREdit()
    {
        if (CurrentMode != ExperienceMode.MR)
            return;
        SetMode(ExperienceMode.MR_EDIT);
    }

    public void ExitMREdit()
    {
        if (CurrentMode != ExperienceMode.MR_EDIT)
            return;
        SetMode(ExperienceMode.MR);
    }

    void BeginTransition(IEnumerator body)
    {
        MRTransitionLog.LogStep("BeginTransition", $"generation will be {transitionGeneration + 1}");
        CancelRunningTransition();
        int generation = ++transitionGeneration;
        MRTransitionLog.Log($"BeginTransition started generation={generation}");
        runningTransition = StartCoroutine(RunTransition(body, generation));
    }

    void CancelRunningTransition()
    {
        if (runningTransition != null)
        {
            MRTransitionLog.LogWarning("CancelRunningTransition — stopping in-flight coroutine");
            StopCoroutine(runningTransition);
            runningTransition = null;
            ConfigManager.WriteConsoleWarning($"{LogPrefix} cancelled in-flight transition");
        }

        transitionInProgress = false;
        sceneTransition?.ForceResetTransitionState();
    }

    IEnumerator RunTransition(IEnumerator body, int generation)
    {
        transitionInProgress = true;
        MRTransitionLog.LogStep("RunTransition", $"coroutine start generation={generation}");
        yield return body;
        if (generation != transitionGeneration)
        {
            MRTransitionLog.LogWarning($"RunTransition aborted — stale generation={generation} current={transitionGeneration}");
            yield break;
        }

        transitionInProgress = false;
        runningTransition = null;
        MRTransitionLog.LogStep("RunTransition", $"coroutine end generation={generation}");
    }

    bool IsTransitionCurrent(int generation) => generation == transitionGeneration;

    /// <summary>Blackout, unload VR additive scenes, then enable passthrough (avoids VR flash over passthrough).</summary>
    IEnumerator UnloadVrScenesUnderBlackoutThenPassthrough(int generation)
    {
        MRTransitionLog.LogStep("UnloadVrThenPassthrough", "blackout begin");
        passthrough.BeginTransitionBlackout();

        MRTransitionLog.LogStep("UnloadVrThenPassthrough", "before UnloadVrScenes");
        yield return sceneTransition.UnloadVrScenes();
        if (!IsTransitionCurrent(generation))
            yield break;

        yield return null;
        yield return new WaitForEndOfFrame();

        MRTransitionLog.LogScenes("UnloadVrThenPassthrough-after-unload");
        MRTransitionLog.LogStep("UnloadVrThenPassthrough", "before EnablePassthroughWhenReady");
        yield return passthrough.EnablePassthroughWhenReady();
        if (!IsTransitionCurrent(generation))
            yield break;

        MRTransitionLog.LogPassthrough("UnloadVrThenPassthrough-after-passthrough", passthrough);
        if (!passthrough.PassthroughSystemReady)
        {
            MRTransitionLog.LogError("EnterMR aborted — passthrough system not ready");
            ConfigManager.WriteConsoleError($"{LogPrefix} EnterMR aborted — passthrough system not ready");
            yield break;
        }
    }

    /// <summary>
    /// Phone booth VR→MR: passthrough while VR scenes still loaded (Quest-safe), adopt booth, then unload under blackout.
    /// Travel fade already black — do not retrigger FadeIn.
    /// </summary>
    IEnumerator EnablePassthroughAdoptBoothThenUnload(MRPhoneBoothPortal portal, int generation)
    {
        MRTransitionLog.LogStep("BoothEnterMR", "blackout hold (travel fade)");
        passthrough.BeginTransitionBlackout(triggerFadeInAnimator: false, restoreFadeSphere: false);

        MRTransitionLog.LogStep("BoothEnterMR", "before EnablePassthroughWhenReady");
        // keepCameraBlack=true: VR scenes are still loaded here. Without this, ApplyPassthroughRendering
        // would set Color.clear (travel head fade already ended), letting VR geometry show through the
        // passthrough underlay. The black is released by RefreshPassthroughAfterSceneUnload once VR
        // scenes are gone.
        yield return passthrough.EnablePassthroughWhenReady(keepCameraBlack: true);
        MRPhoneBoothTravelHeadFade.ReassertActiveTravelBlackout();
        if (!IsTransitionCurrent(generation))
            yield break;

        if (!passthrough.PassthroughSystemReady)
        {
            MRTransitionLog.LogError("EnterMRFromPhoneBooth aborted — passthrough system not ready");
            ConfigManager.WriteConsoleError($"{LogPrefix} EnterMRFromPhoneBooth aborted — passthrough system not ready");
            yield break;
        }

        MRTransitionLog.LogPassthrough("BoothEnterMR-after-passthrough", passthrough);

        MRPhoneBoothPortal.AdoptAsTraveler(portal, transform);

        MRTransitionLog.LogStep("BoothEnterMR", "before UnloadVrScenes");
        yield return sceneTransition.UnloadVrScenes();
        if (!IsTransitionCurrent(generation))
            yield break;

        yield return null;
        yield return new WaitForEndOfFrame();
        MRTransitionLog.LogScenes("BoothEnterMR-after-unload");

        passthrough.RefreshPassthroughAfterSceneUnload();
        yield return null;
    }

    /// <summary>Enable passthrough and unload VR scenes without fade/blackout (FixedScene auto boot).</summary>
    IEnumerator EnablePassthroughAndUnloadVrDirect(int generation)
    {
        MRTransitionLog.LogStep("DirectBootMR", "begin");
        passthrough.PrepareDirectMrBoot();

        MRTransitionLog.LogStep("DirectBootMR", "before EnablePassthroughWhenReady");
        yield return passthrough.EnablePassthroughWhenReady();
        if (!IsTransitionCurrent(generation))
            yield break;

        if (!passthrough.PassthroughSystemReady)
        {
            MRTransitionLog.LogError("EnterMR direct boot aborted — passthrough system not ready");
            ConfigManager.WriteConsoleError($"{LogPrefix} EnterMR direct boot aborted — passthrough system not ready");
            yield break;
        }

        MRTransitionLog.LogStep("DirectBootMR", "before UnloadVrScenes");
        yield return sceneTransition.UnloadVrScenes();
        if (!IsTransitionCurrent(generation))
            yield break;

        yield return null;
        yield return new WaitForEndOfFrame();

        MRTransitionLog.LogScenes("DirectBootMR-after-unload");
        passthrough.RefreshPassthroughAfterSceneUnload();
        yield return null;
    }

    IEnumerator EnterMRCoroutine(bool directBoot)
    {
        int generation = transitionGeneration;
        MRTransitionLog.LogStep("EnterMRCoroutine", $"start generation={generation} mode={CurrentMode} directBoot={directBoot}");
        MRTransitionLog.LogManagerState("EnterMRCoroutine-start");
        ConfigManager.WriteConsole($"{LogPrefix} EnterMR coroutine (mode={CurrentMode}, directBoot={directBoot})");

        MRScenePermissions.Reset();
        MRTransitionLog.LogStep("EnterMRCoroutine", "before EnsureGranted");
        MRSceneHost.PrepareForMr();
        yield return MRScenePermissions.EnsureGranted();
        if (!IsTransitionCurrent(generation))
            yield break;

        MRRoomInfoUI.Instance?.RefreshContent();

        if (!directBoot && MRRuntimeSettings.RememberVrPoseOnStandardEnterMr)
            RememberVrPlayerPose();

        MRTransitionLog.LogStep("EnterMRCoroutine", "before SuspendForMR");
        MRVrSystemsGate.SuspendForMR();
        yield return MRVrSystemsGate.WaitForShutdownBeforeSceneUnload();
        if (!IsTransitionCurrent(generation))
            yield break;

        if (directBoot)
            yield return EnablePassthroughAndUnloadVrDirect(generation);
        else
            yield return UnloadVrScenesUnderBlackoutThenPassthrough(generation);
        if (!IsTransitionCurrent(generation))
            yield break;

        DestroyMRSpaceOrigin();
        // MR colocado: o rig NÃO é reposicionado. As âncoras MRUK são world-locked
        // ao quarto físico (MRUK.Update sobrescreve trackingSpace), por isso teleportar
        // o rig deslocaria todo o conteúdo virtual de forma uniforme face ao passthrough.
        // RestoreMrPlayerPose() desativado — ver revisão de alinhamento MR.
        MRTransitionLog.LogStep("EnterMRCoroutine", "RestoreMrPlayerPose skipped (colocated MR)");

        EnsureMRSpaceOrigin();
        Transform player = FindPlayerTransform();
        if (environmentSurfaces != null)
            yield return environmentSurfaces.ProbeWhenReady(player);
        if (!IsTransitionCurrent(generation))
            yield break;

        MRTransitionLog.LogStep("EnterMRCoroutine", "before SetMode MR");
        SetMode(ExperienceMode.MR);
        MRTransitionLog.LogStep("EnterMRCoroutine", "after SetMode MR");

        mrLighting?.Spawn(MRSpaceOrigin);
        mrEffectMesh?.Spawn();
        BindLayoutPathsToCurrentRoom();
        MRLayoutRegistry layoutRegistry = ActiveRegistry();
        if (layoutRegistry != null)
            yield return layoutRegistry.SpawnAllAsync(MRSpaceOrigin);
        if (!IsTransitionCurrent(generation))
            yield break;

        MREnvironmentRegistry environmentRegistry = ActiveEnvironmentRegistry();
        if (environmentRegistry != null)
            yield return environmentRegistry.SpawnAllAsync(MRSpaceOrigin);
        if (!IsTransitionCurrent(generation))
            yield break;

        yield return null;
        layoutRegistry?.EnsureAttractPlaybackOnSpawned();

        yield return RefreshMrPosesWhenReady(generation, player);

        yield return FinalizeEnvironmentAfterEnterMr(generation, player);
        if (!IsTransitionCurrent(generation))
            yield break;

        MRConfigurationCabinetController configCabinet = MRConfigurationCabinetController.Instance;
        configCabinet?.PrepareConfigCabinetAfterRoomScan();
        configCabinet?.BeginPlacementRayAfterRoomScan();

        MRPhoneBoothVisibility.EnsureMrInstance();
        MRPhoneBoothVisibility.ApplySavedVisibility();

        MRTransitionLog.LogManagerState("EnterMRCoroutine-final");
        ConfigManager.WriteConsole($"{LogPrefix} EnterMR done");
        MRCameraRigAlignLog.LogEvent("EnterMR-done");
        ConfigManager.WriteConsole($"[MRCameraRigAlignLog] paste log from: {MRCameraRigAlignLog.LogFilePath}");
        MRTransitionLog.LogStep("EnterMRCoroutine", "DONE");
    }

    IEnumerator EnterTestSceneMrCoroutine(Vector3 originPosition, Quaternion originRotation)
    {
        int generation = transitionGeneration;
        MRTransitionLog.LogStep("EnterTestSceneMrCoroutine", $"start generation={generation}");
        ConfigManager.WriteConsole($"{LogPrefix} EnterTestSceneMr coroutine");

        DestroyMRSpaceOrigin();

        var originGo = new GameObject("MRSpaceOrigin");
        originGo.transform.SetPositionAndRotation(originPosition, originRotation);
        MRSpaceOrigin = originGo.transform;

        Transform player = Camera.main != null ? Camera.main.transform : FindPlayerTransform();
        if (environmentSurfaces != null)
        {
            if (MRTestConfigSceneLoader.TryGetSimulateRoomSurfaces(out Transform floor, out Transform ceiling))
                yield return environmentSurfaces.ProbeSimulateRoomWhenReady(floor, ceiling, player);
            else
                yield return environmentSurfaces.ProbeWhenReady(player);
        }
        if (!IsTransitionCurrent(generation))
            yield break;

        MRTestConfigSceneLoader.PositionViewCameraFromSurfaces(environmentSurfaces);

        SetMode(ExperienceMode.MR);

        BindLayoutPathsToCurrentRoom();
        MRLayoutRegistry layout = ActiveRegistry();
        if (layout != null)
            yield return layout.SpawnAllAsync(MRSpaceOrigin);
        if (!IsTransitionCurrent(generation))
            yield break;

        MREnvironmentRegistry environment = ActiveEnvironmentRegistry();
        if (environment != null)
            yield return environment.SpawnAllAsync(MRSpaceOrigin);
        if (!IsTransitionCurrent(generation))
            yield break;

        layout?.EnsureAttractPlaybackOnSpawned();

        MRConfigurationCabinetController configCabinet = MRConfigurationCabinetController.Instance;
        if (configCabinet != null && !configCabinet.HasCabinet)
            configCabinet.SpawnAtMrOrigin();

        ConfigManager.WriteConsole($"{LogPrefix} EnterTestSceneMr done");
        MRTransitionLog.LogStep("EnterTestSceneMrCoroutine", "DONE");
    }

    IEnumerator EnterMRFromPhoneBoothCoroutine(MRPhoneBoothPortal portal)
    {
        int generation = transitionGeneration;
        bool travelBlackoutCleared = false;
        try
        {
            PhoneBoothTravelState travelState = portal.ConsumePendingTravelState();
            MRTransitionLog.LogStep("EnterMRFromPhoneBoothCoroutine", $"start generation={generation}");
            ConfigManager.WriteConsole($"{LogPrefix} EnterMRFromPhoneBooth coroutine");

            CancelActivePlacementRay();
            MRScenePermissions.Reset();
            MRSceneHost.PrepareForMr();
            yield return MRScenePermissions.EnsureGranted();
            if (!IsTransitionCurrent(generation))
                yield break;

            MRRoomInfoUI.Instance?.RefreshContent();
            MRVrSystemsGate.SuspendForMR();
            yield return MRVrSystemsGate.WaitForShutdownBeforeSceneUnload();
            if (!IsTransitionCurrent(generation))
                yield break;

            yield return EnablePassthroughAdoptBoothThenUnload(portal, generation);
            if (!IsTransitionCurrent(generation))
                yield break;

            portal.NotifyHandsetsTravelComplete();

            passthrough.RefreshPassthroughAfterSceneUnload();

            DestroyMRSpaceOrigin();
            EnsureMRSpaceOrigin();

            Transform player = FindPlayerTransform();
            if (environmentSurfaces != null)
                yield return environmentSurfaces.ProbeWhenReady(player);
            if (!IsTransitionCurrent(generation))
                yield break;

            passthrough.RefreshPassthroughAfterSceneUnload();

            MRTransitionLog.LogStep("EnterMRFromPhoneBoothCoroutine", "before SetMode MR");
            SetMode(ExperienceMode.MR);
            MRTransitionLog.LogStep("EnterMRFromPhoneBoothCoroutine", "after SetMode MR");

            MRTransitionLog.LogStep("EnterMRFromPhoneBoothCoroutine", "before mrLighting.Spawn");
            mrLighting?.Spawn(MRSpaceOrigin);
            mrEffectMesh?.Spawn();

            // Match 0.5.0 order: spawn MR layout + config cabinet and refresh poses *before*
            // phone-booth explosion. Spawning config first then exploding caused the initial
            // placement ray to cancel (~1s) and confused MOVE CONFIG on device.
            BindLayoutPathsToCurrentRoom();
            MRLayoutRegistry layoutRegistry = ActiveRegistry();
            MRTransitionLog.LogStep("EnterMRFromPhoneBoothCoroutine", "before layout SpawnAllAsync");
            if (layoutRegistry != null)
                yield return layoutRegistry.SpawnAllAsync(MRSpaceOrigin);
            if (!IsTransitionCurrent(generation))
                yield break;

            MRTransitionLog.LogStep("EnterMRFromPhoneBoothCoroutine", "before env SpawnAllAsync");
            MREnvironmentRegistry environmentRegistry = ActiveEnvironmentRegistry();
            if (environmentRegistry != null)
                yield return environmentRegistry.SpawnAllAsync(MRSpaceOrigin);
            if (!IsTransitionCurrent(generation))
                yield break;

            yield return null;
            layoutRegistry?.EnsureAttractPlaybackOnSpawned();
            yield return RefreshMrPosesWhenReady(generation, player);
            if (!IsTransitionCurrent(generation))
                yield break;

            yield return FinalizeEnvironmentAfterEnterMr(generation, player);
            if (!IsTransitionCurrent(generation))
                yield break;

            MRTransitionLog.LogStep("EnterMRFromPhoneBoothCoroutine", "config cabinet after room scan");
            MRConfigurationCabinetController.Instance?.PrepareConfigCabinetAfterRoomScan();

            // Re-probe immediately before booth placement — MRUK may have registered anchors
            // after the probe at coroutine start (long spawn/refresh gap since merge 47844e94).
            if (environmentSurfaces != null && player != null)
                yield return environmentSurfaces.ProbeWhenReady(player);

            portal.PlaceOnMrFloor(environmentSurfaces, player);
            // MR colocado: não aplicar travel state ao rig (ApplyPhoneBoothTravelState
            // desativado). A cabine já foi colocada no chão real via PlaceOnMrFloor;
            // mover o rig deslocaria todo o conteúdo virtual face ao passthrough.
            MRPhoneBoothSettings.SetVisible(true);
            portal.SetVisible(true, playHideEffect: false);

            MRTransitionLog.LogStep("EnterMRFromPhoneBoothCoroutine", "before arrival explosion");
            yield return portal.PlayTravelArrivalExplosionAndRestoreGlassDoor();
            travelBlackoutCleared = true;
            if (!IsTransitionCurrent(generation))
                yield break;

            // Placement ray only after arrival VFX — avoids freeze when re-entering MR with saved pose.
            MRConfigurationCabinetController.Instance?.BeginPlacementRayAfterRoomScan();

            // Re-snap after smoke/glass/blackout — arrival VFX can skew bounds; MRUK may
            // still be settling when the first snap ran (~20 s earlier in the coroutine).
            if (environmentSurfaces != null)
            {
                if (player != null)
                    yield return environmentSurfaces.ProbeWhenReady(player);
                else
                    yield return null;

                portal.ReconcileVerticalMrFloorSnap(environmentSurfaces);
                MRConfigurationCabinetController.Instance?.RefreshPoseForMrReentry();
            }

            MRPhoneBoothSettings.SetVisible(true);
            portal.SetVisible(true, playHideEffect: false);
            portal.NotifyHandsetsTravelComplete();

            MRTransitionLog.LogManagerState("EnterMRFromPhoneBoothCoroutine-final");
            ConfigManager.WriteConsole($"{LogPrefix} EnterMRFromPhoneBooth done");
            MRCameraRigAlignLog.LogEvent("EnterMRFromPhoneBooth-done");
            ConfigManager.WriteConsole($"[MRCameraRigAlignLog] paste log from: {MRCameraRigAlignLog.LogFilePath}");
            MRTransitionLog.LogStep("EnterMRFromPhoneBoothCoroutine", "DONE");
        }
        finally
        {
            if (!travelBlackoutCleared)
                MRPhoneBoothPortal.EndTravelBlackoutEverywhere();
        }
    }

    sealed class PhoneBoothMrToVrContext
    {
        public MRPhoneBoothPortal travelerPortal;
        public PhoneBoothTravelState travelState;
        public PayphoneHandsetGrab.VrReturnHandsetPlan handsetPlan;
        public MRPhoneBoothPortal scenePortal;
        public AudioClip arrivalExplosionClip;
        public bool restoredGalleryPose;
        /// <summary>World pose that looked correct on VR arrival; reapplied after handoff / late settle.</summary>
        public Vector3? goodPlayerWorldPosition;
        public Quaternion? goodPlayerWorldRotation;
    }

    IEnumerator EnterVRFromPhoneBoothCoroutine(MRPhoneBoothPortal travelerPortal)
    {
        int generation = transitionGeneration;
        bool travelBlackoutCleared = false;
        try
        {
            PhoneBoothTravelState travelState = travelerPortal != null
                ? travelerPortal.ConsumePendingTravelState()
                : null;
            MRTransitionLog.LogStep("EnterVRFromPhoneBoothCoroutine", $"start generation={generation}");
            ConfigManager.WriteConsole($"{LogPrefix} EnterVRFromPhoneBooth coroutine");

            if (CurrentMode == ExperienceMode.MR_EDIT)
                SetMode(ExperienceMode.MR);

            if (travelerPortal != null && travelerPortal.IsTravelerInstance)
                MRPhoneBoothSettings.SaveMrPose(travelerPortal.transform.position, travelerPortal.transform.rotation);

            var ctx = new PhoneBoothMrToVrContext
            {
                travelerPortal = travelerPortal,
                travelState = travelState,
                handsetPlan = PayphoneHandsetGrab.CaptureVrReturnPlan(travelerPortal, travelState),
            };

            foreach (MRPhoneBoothTransitionSequence.MrToVrReturnStep step in MRRuntimeSettings.MrToVrReturnSteps)
            {
                if (!IsTransitionCurrent(generation))
                    yield break;

                yield return RunPhoneBoothMrToVrStep(step, generation, ctx);
                if (step == MRPhoneBoothTransitionSequence.MrToVrReturnStep.ArrivalExplosionAndSmoke)
                    travelBlackoutCleared = true;
            }

            MRTransitionLog.LogManagerState("EnterVRFromPhoneBoothCoroutine-final");
            ConfigManager.WriteConsole($"{LogPrefix} EnterVRFromPhoneBooth done");
            MRTransitionLog.LogStep("EnterVRFromPhoneBoothCoroutine", "DONE");
        }
        finally
        {
            if (!travelBlackoutCleared)
                MRPhoneBoothPortal.EndTravelBlackoutEverywhere();
        }
    }

    IEnumerator RunPhoneBoothMrToVrStep(
        MRPhoneBoothTransitionSequence.MrToVrReturnStep step,
        int generation,
        PhoneBoothMrToVrContext ctx)
    {
        MRTransitionLog.LogStep("EnterVRFromPhoneBoothCoroutine", $"step {step}");

        switch (step)
        {
            case MRPhoneBoothTransitionSequence.MrToVrReturnStep.ReloadVrScenes:
                yield return sceneTransition.ReloadVrScenes();
                MRVrSystemsGate.SilenceCabinetScreensAfterVrSceneReload();
                break;

            case MRPhoneBoothTransitionSequence.MrToVrReturnStep.CacheArrivalExplosionClip:
                ctx.scenePortal = MRPhoneBoothPortal.FindSceneBoothPortal();
                ctx.arrivalExplosionClip =
                    MRPhoneBoothPortal.ResolveExplosionClip(ctx.scenePortal, ctx.travelerPortal);
                break;

            case MRPhoneBoothTransitionSequence.MrToVrReturnStep.RestoreGalleryPlayerPose:
                ctx.restoredGalleryPose = false;
                if (MRRuntimeSettings.RestoreVrPoseOnPhoneBoothReturn)
                    ctx.restoredGalleryPose = TryRestoreVrPlayerPose();

                MRTransitionLog.LogStep("EnterVRFromPhoneBoothCoroutine",
                    ctx.restoredGalleryPose ? "after RestoreVrPlayerPose" : "RestoreVrPlayerPose skipped");
                break;

            case MRPhoneBoothTransitionSequence.MrToVrReturnStep.RefreshCameraOffset:
                if (MRRuntimeSettings.RefreshCameraOffsetAfterPhoneBoothReturn)
                    RefreshPlayerControllerCameraOffset();
                break;

            case MRPhoneBoothTransitionSequence.MrToVrReturnStep.ApplyTravelStateFallback:
                if (!ctx.restoredGalleryPose && MRRuntimeSettings.FallbackTravelStateIfRestoreFails)
                {
                    if (ctx.scenePortal == null)
                        ctx.scenePortal = MRPhoneBoothPortal.FindSceneBoothPortal();
                    ApplyPhoneBoothTravelState(ctx.scenePortal, ctx.travelState);
                }
                break;

            case MRPhoneBoothTransitionSequence.MrToVrReturnStep.FinalizeHandsetOnSceneBooth:
                if (ctx.scenePortal == null)
                    ctx.scenePortal = MRPhoneBoothPortal.FindSceneBoothPortal();
                PayphoneHandsetGrab.FinalizeForVrSceneReturn(
                    ctx.travelerPortal, ctx.scenePortal, ctx.handsetPlan);
                ctx.scenePortal = MRPhoneBoothPortal.FindSceneBoothPortal();
                if (ctx.arrivalExplosionClip == null)
                    ctx.arrivalExplosionClip =
                        MRPhoneBoothPortal.ResolveExplosionClip(ctx.scenePortal, null);
                break;

            case MRPhoneBoothTransitionSequence.MrToVrReturnStep.WaitFramesBeforeArrivalEffects:
                float delay = MRRuntimeSettings.SecondsBeforeArrivalExplosion;
                if (delay > 0f)
                    yield return new WaitForSecondsRealtime(delay);
                break;

            case MRPhoneBoothTransitionSequence.MrToVrReturnStep.ArrivalExplosionAndSmoke:
                if (ctx.scenePortal == null)
                    ctx.scenePortal = MRPhoneBoothPortal.FindSceneBoothPortal();
                if (ctx.arrivalExplosionClip == null)
                    ctx.arrivalExplosionClip =
                        MRPhoneBoothPortal.ResolveExplosionClip(ctx.scenePortal, null);
                yield return MRPhoneBoothPortal.PlayArrivalExplosionForVrReturnAndRestoreGlassDoor(
                    ctx.scenePortal, ctx.arrivalExplosionClip);
                // Pose is usually correct here; remember it before EnableVrMode handoff shoves it.
                SnapshotGoodVrPlayerPose(ctx, "after arrival explosion");
                break;

            case MRPhoneBoothTransitionSequence.MrToVrReturnStep.EnableVrModeAndLocomotion:
                // Snapshot correct pose → handoff (CameraFloorOffset) → put pose back → reapply ~1s later.
                // Do not force OrientPlayerYawToFacePhone — preserve natural facing from travel.
                passthrough.RebindCameraAndDisablePassthrough(playFadeOut: false);
                ResetLegacyPassthroughFlags();

                if (!ctx.goodPlayerWorldPosition.HasValue)
                    SnapshotGoodVrPlayerPose(ctx, "before handoff (fallback)");

                // Capture head world position before offset restore; root XZ alone is unreliable after walk.
                // Keep snapshotted root rotation (natural facing) — do not rewrite yaw from head look.
                Transform head = Camera.main != null ? Camera.main.transform : null;
                Vector3? savedHeadPos = head != null ? head.position : (Vector3?)null;
                if (savedHeadPos.HasValue)
                {
                    MRTransitionLog.LogStep("EnterVRFromPhoneBoothCoroutine",
                        $"snapshot head pos={savedHeadPos.Value}");
                }

                SetMode(ExperienceMode.VR);
                MRSceneHost.SuspendForVr();
                RestoreXrOriginTrackingOffsetFromAppStart();

                Transform player = ResolveLocomotionRootForPose() ?? FindPlayerTransform();
                head = Camera.main != null ? Camera.main.transform : null;
                if (player != null && savedHeadPos.HasValue && head != null)
                {
                    CharacterController cc = player.GetComponent<CharacterController>();
                    if (cc != null)
                        cc.enabled = false;

                    // XZ only: full XYZ would lock MR-era head height and float above the VR floor
                    // after RestoreVrScale / CameraFloorOffset restore.
                    Vector3 delta = savedHeadPos.Value - head.position;
                    delta.y = 0f;
                    player.position += delta;

                    if (ctx.goodPlayerWorldRotation.HasValue)
                        player.rotation = ctx.goodPlayerWorldRotation.Value;

                    if (cc != null)
                        cc.enabled = true;

                    // Refresh snapshot with post-VR-scale height so late reapply does not lift again.
                    ctx.goodPlayerWorldPosition = player.position;
                    ctx.goodPlayerWorldRotation = player.rotation;
                    MRTransitionLog.LogStep(
                        "EnterVRFromPhoneBoothCoroutine",
                        $"reapplied good pose (after offset restore, xz) pos={player.position} rotY={player.rotation.eulerAngles.y:F1}");
                }
                else
                {
                    ApplyGoodVrPlayerPose(ctx, "after offset restore", preserveHeight: true);
                }

                MRVrSystemsGate.ResumeVrSystemsExceptLocomotion();
                if (ctx.scenePortal == null)
                    ctx.scenePortal = MRPhoneBoothPortal.FindSceneBoothPortal();
                PayphoneHandsetGrab.RefreshGrabbedHandVisibility(ctx.scenePortal);
                ctx.scenePortal?.NotifyHandsetsTravelComplete();

                float controlsDelay = MRRuntimeSettings.SecondsBeforeResumeVrControlsOnPhoneBoothReturn;
                if (controlsDelay > 0f)
                {
                    MRTransitionLog.LogStep("EnterVRFromPhoneBoothCoroutine",
                        $"wait {controlsDelay:F2}s before VR locomotion");
                    yield return new WaitForSecondsRealtime(controlsDelay);
                }

                MRVrSystemsGate.ResumePlayerLocomotionForVr();

                for (int i = 0; i < 3; i++)
                {
                    yield return null;
                    ApplyGoodVrPlayerPose(ctx, $"after locomotion resume frame {i}", preserveHeight: true);
                }

                yield return new WaitForSecondsRealtime(1f);
                ApplyGoodVrPlayerPose(ctx, "after 1s settle", preserveHeight: true);
                MRTransitionLog.LogStep("EnterVRFromPhoneBoothCoroutine", "EnableVrModeAndLocomotion done");
                break;

            case MRPhoneBoothTransitionSequence.MrToVrReturnStep.MrEnvironmentCleanup:
                MRLayoutRegistry registry = ActiveRegistry();
                MREnvironmentRegistry envRegistry = ActiveEnvironmentRegistry();
                MRVrSystemsGate.StopActiveLibretroGames();
                if (registry != null)
                    yield return registry.DespawnAllAsync(stopLibretroFirst: true);
                if (envRegistry != null)
                    yield return envRegistry.DespawnAllAsync();
                if (!IsTransitionCurrent(generation))
                    yield break;

                MRVrSystemsGate.StopActiveLibretroGames();
                environmentSurfaces?.ClearMrukScene();
                MRRoomInfoUI.Instance?.Hide();
                DestroyMRSpaceOrigin();
                MRConfigurationCabinetController.Instance?.ReleaseForVrTransition();
                yield return null;
                break;

            case MRPhoneBoothTransitionSequence.MrToVrReturnStep.FinalPassthroughRebind:
                passthrough.RebindCameraAndDisablePassthrough(playFadeOut: false);
                ResetLegacyPassthroughFlags();
                break;
        }
    }

    static void ApplyPhoneBoothTravelState(MRPhoneBoothPortal portal, PhoneBoothTravelState state)
    {
        if (portal == null || state == null)
            return;

        portal.ApplyTravelState(state);
    }

    static void RestorePhoneBoothTravelGlass(MRPhoneBoothPortal portal)
    {
        portal?.RestoreTravelGlassAndDoor();
    }

    static Transform ResolveLocomotionRootForPose()
    {
        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null && pc.PlayerControllerGameObject != null)
            return pc.PlayerControllerGameObject.transform;
        return FindPlayerTransform();
    }

    static void SnapshotGoodVrPlayerPose(PhoneBoothMrToVrContext ctx, string reason)
    {
        if (ctx == null)
            return;

        Transform player = ResolveLocomotionRootForPose() ?? FindPlayerTransform();
        if (player == null)
        {
            MRTransitionLog.LogWarning($"snapshot good pose skipped ({reason}) — no player");
            return;
        }

        ctx.goodPlayerWorldPosition = player.position;
        ctx.goodPlayerWorldRotation = player.rotation;
        MRTransitionLog.LogStep(
            "EnterVRFromPhoneBoothCoroutine",
            $"snapshot good pose ({reason}) pos={player.position} rotY={player.rotation.eulerAngles.y:F1}");
    }

    static void ApplyGoodVrPlayerPose(
        PhoneBoothMrToVrContext ctx,
        string reason,
        bool preserveHeight = false)
    {
        if (ctx == null || !ctx.goodPlayerWorldPosition.HasValue || !ctx.goodPlayerWorldRotation.HasValue)
            return;

        Transform player = ResolveLocomotionRootForPose() ?? FindPlayerTransform();
        if (player == null)
        {
            MRTransitionLog.LogWarning($"reapplied good pose skipped ({reason}) — no player");
            return;
        }

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = false;

        Vector3 target = ctx.goodPlayerWorldPosition.Value;
        if (preserveHeight)
            target.y = player.position.y;
        player.position = target;
        player.rotation = ctx.goodPlayerWorldRotation.Value;

        if (cc != null)
            cc.enabled = true;

        MRTransitionLog.LogStep(
            "EnterVRFromPhoneBoothCoroutine",
            $"reapplied good pose ({reason}) pos={player.position} rotY={player.rotation.eulerAngles.y:F1}");
    }

    IEnumerator RefreshMrPosesWhenReady(int generation, Transform player)
    {
        MRTransitionLog.LogStep("EnterMRCoroutine", "RefreshMrPoses begin");
        const int maxAttempts = 20;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (!IsTransitionCurrent(generation))
                yield break;

            if (environmentSurfaces != null && player != null)
            {
                MRUKRoom room = MRUK.Instance != null ? MRUK.Instance.GetCurrentRoom() : null;
                if (!environmentSurfaces.IsReady || !MRSceneScanState.HasUsableRoom(room))
                    yield return environmentSurfaces.ProbeWhenReady(player);
            }

            ActiveRegistry()?.RefreshAllSpawnedPosesFromLayout();
            if (!MRPlacementRayController.AnyActive)
                MRConfigurationCabinetController.Instance?.RefreshPoseForMrReentry();

            if (attempt == 0 || attempt == maxAttempts - 1)
                MRTransitionLog.LogManagerState($"EnterMRCoroutine-refresh-{attempt}");

            yield return null;
        }

        MRTransitionLog.LogStep("EnterMRCoroutine", "RefreshMrPoses end");
    }

    /// <summary>MRUK RoomCreated/SceneLoaded while already in MR — debounced full environment refresh.</summary>
    public void ScheduleEnvironmentRefreshFromMrukEvent()
    {
        if (transitionInProgress)
            return;

        if (CurrentMode != ExperienceMode.MR && CurrentMode != ExperienceMode.MR_EDIT)
            return;

        if (mrukEnvironmentRefreshRoutine != null)
            StopCoroutine(mrukEnvironmentRefreshRoutine);

        mrukEnvironmentRefreshRoutine = StartCoroutine(DebouncedEnvironmentRefreshFromMrukEvent());
    }

    IEnumerator DebouncedEnvironmentRefreshFromMrukEvent()
    {
        yield return new WaitForSeconds(MrukEnvironmentRefreshDebounceSeconds);
        yield return WaitForUsableRoom(10f);
        yield return RefreshEnvironmentAfterRoomScan(FindPlayerTransform());
        mrukEnvironmentRefreshRoutine = null;
    }

    IEnumerator FinalizeEnvironmentAfterEnterMr(int generation, Transform player)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        yield return WaitForUsableRoom(15f);
#endif
        if (!IsTransitionCurrent(generation))
            yield break;

        yield return RefreshEnvironmentAfterRoomScan(player);
        if (!IsTransitionCurrent(generation))
            yield break;

        MREnvironmentRegistry environmentRegistry = ActiveEnvironmentRegistry();
        if (environmentRegistry != null)
            yield return environmentRegistry.ReapplyRoomSkinsWhenReady();
    }

    static IEnumerator WaitForUsableRoom(float timeoutSeconds)
    {
        float remaining = timeoutSeconds;
        while (remaining > 0f)
        {
            MRUKRoom room = MRUK.Instance != null ? MRUK.Instance.GetCurrentRoom() : null;
            if (MRSceneScanState.HasUsableRoom(room))
                yield break;
            remaining -= Time.unscaledDeltaTime;
            yield return null;
        }
    }

    IEnumerator EnterVRCoroutine()
    {
        int generation = transitionGeneration;
        MRTransitionLog.LogStep(
            "EnterVRCoroutine",
            $"start generation={generation} mode={CurrentMode}");
        MRTransitionLog.LogManagerState("EnterVRCoroutine-start");
        MRTransitionLog.LogScenes("EnterVRCoroutine-start");
        ConfigManager.WriteConsole($"{LogPrefix} EnterVR coroutine (mode={CurrentMode})");

        if (CurrentMode == ExperienceMode.MR_EDIT)
        {
            MRTransitionLog.Log("exiting MR_EDIT before VR transition");
            SetMode(ExperienceMode.MR);
        }

        MRTransitionLog.LogStep("EnterVRCoroutine", "before ReloadVrScenes");
        yield return sceneTransition.ReloadVrScenes();
        if (!IsTransitionCurrent(generation))
        {
            MRTransitionLog.LogWarning($"EnterVRCoroutine aborted after ReloadVrScenes generation={generation}");
            yield break;
        }

        MRTransitionLog.LogStep("EnterVRCoroutine", "after ReloadVrScenes");
        MRTransitionLog.LogScenes("EnterVRCoroutine-after-reload");
        MRTransitionLog.LogManagerState("EnterVRCoroutine-after-reload");

        if (MRRuntimeSettings.RestoreVrPoseOnStandardEnterVr)
            RestoreVrPlayerPose();

        MRTransitionLog.LogStep("EnterVRCoroutine", "after RestoreVrPlayerPose");

        passthrough.RebindCameraAndDisablePassthrough(playFadeOut: false);
        ResetLegacyPassthroughFlags();
        MRSceneHost.SuspendForVr();
        MRTransitionLog.LogPassthrough("EnterVRCoroutine-after-rebind", passthrough);

        MRTransitionLog.LogStep("EnterVRCoroutine", "before SetMode VR");
        SetMode(ExperienceMode.VR);
        MRTransitionLog.LogStep("EnterVRCoroutine", "after SetMode VR");
        MRTransitionLog.LogStep("EnterVRCoroutine", "before ResumeForVR");
        MRVrSystemsGate.ResumeForVR();
        MRTransitionLog.LogManagerState("EnterVRCoroutine-after-resume");

        MRLayoutRegistry registry = ActiveRegistry();
        MREnvironmentRegistry envRegistry = ActiveEnvironmentRegistry();
        MRVrSystemsGate.StopActiveLibretroGames();
        MRTransitionLog.LogStep("EnterVRCoroutine", "after StopActiveLibretroGames");
        MRTransitionLog.Log($"DespawnAllAsync registry={(registry != null ? registry.name : "null")}");
        if (registry != null)
            yield return registry.DespawnAllAsync(stopLibretroFirst: true);
        if (envRegistry != null)
            yield return envRegistry.DespawnAllAsync();
        if (!IsTransitionCurrent(generation))
        {
            MRTransitionLog.LogWarning($"EnterVRCoroutine aborted after DespawnAllAsync generation={generation}");
            yield break;
        }

        MRTransitionLog.LogStep("EnterVRCoroutine", "after DespawnAllAsync");
        MRTransitionLog.LogManagerState("EnterVRCoroutine-after-despawn");
        MRActiveRoom.Clear();

        MRVrSystemsGate.StopActiveLibretroGames();
        MRTransitionLog.LogStep("EnterVRCoroutine", "after StopActiveLibretroGames-final");

        environmentSurfaces?.ClearMrukScene();
        MRRoomInfoUI.Instance?.Hide();
        DestroyMRSpaceOrigin();
        // Phone-booth return destroys the DDOL traveler; standard EnterVR must too
        // or its solid colliders block the reloaded gallery booth.
        CleanupPhoneBoothTravelerForStandardVrExit();
        MRPhoneBoothPortal.EndTravelBlackoutEverywhere();
        MRTransitionLog.LogStep("EnterVRCoroutine", "after MR cleanup");

        // WorldLock may have shoved CameraFloorOffsetObject; reset after MRUK is gone.
        RestoreVrHeightAfterMrExit();
        MRTransitionLog.LogStep("EnterVRCoroutine", "after RestoreVrHeightAfterMrExit");

        MRTransitionLog.LogStep("EnterVRCoroutine", "before config cabinet ReleaseForVr");
        MRConfigurationCabinetController.Instance?.ReleaseForVrTransition();
        yield return null;
        MRTransitionLog.LogStep("EnterVRCoroutine", "after config cabinet ReleaseForVr");

        passthrough.RebindCameraAndDisablePassthrough(playFadeOut: false);
        ResetLegacyPassthroughFlags();
        MRTransitionLog.LogPassthrough("EnterVRCoroutine-final", passthrough);
        MRTransitionLog.LogScenes("EnterVRCoroutine-final");
        MRTransitionLog.LogManagerState("EnterVRCoroutine-final");

        ConfigManager.WriteConsole($"{LogPrefix} EnterVR done");
        MRTransitionLog.LogStep("EnterVRCoroutine", "DONE");
    }

    /// <summary>
    /// Non-booth EnterVR (Quick Travel / hold-A): remove the MR traveler payphone so it does not
    /// leave solid colliders over the gallery after scenes reload.
    /// </summary>
    static void CleanupPhoneBoothTravelerForStandardVrExit()
    {
        MRPhoneBoothPortal traveler = MRPhoneBoothPortal.FindMrTravelerInstance(includeInactive: true);
        if (traveler == null)
            return;

        if (traveler.IsTravelerInstance)
            MRPhoneBoothSettings.SaveMrPose(traveler.transform.position, traveler.transform.rotation);

        MRPhoneBoothPortal.DestroyTravelerInstance();
        MRTransitionLog.LogStep("EnterVRCoroutine", "destroyed MR traveler phone booth");
        ConfigManager.WriteConsole($"{LogPrefix} destroyed MR traveler phone booth for standard VR exit");
    }

    /// <summary>
    /// After MRUK/WorldLock teardown: restore CameraFloorOffset local pose and VR camera height.
    /// </summary>
    void RestoreVrHeightAfterMrExit()
    {
        RestoreXrOriginTrackingOffsetFromAppStart();

        if (appStartPlayerControllerLocalPosition.HasValue)
        {
            PlayerController pc = FindObjectOfType<PlayerController>();
            if (pc != null && pc.PlayerControllerGameObject != null)
            {
                pc.PlayerControllerGameObject.transform.localPosition =
                    appStartPlayerControllerLocalPosition.Value;
            }
        }

        RefreshPlayerControllerCameraOffset();
        RestoreVrScale();
    }

    void RestoreXrOriginTrackingOffsetFromAppStart()
    {
        PlayerController pc = FindObjectOfType<PlayerController>();
        Transform floor = ResolveCameraFloorOffsetTransform(pc);
        if (floor == null)
        {
            MRTransitionLog.LogWarning("RestoreXrOriginTrackingOffset skipped — no CameraFloorOffset");
            return;
        }

        if (appStartCameraFloorLocalPosition.HasValue)
        {
            floor.localPosition = appStartCameraFloorLocalPosition.Value;
            floor.localRotation = appStartCameraFloorLocalRotation ?? Quaternion.identity;
            MRTransitionLog.Log(
                $"restored CameraFloorOffset localPos={floor.localPosition}");
        }
        else
        {
            // Fallback when capture missed: strip WorldLock XZ/rotation; keep configured Y.
            float y = pc != null && pc.xrorigin != null ? pc.xrorigin.CameraYOffset : 0f;
            floor.localPosition = new Vector3(0f, y, 0f);
            floor.localRotation = Quaternion.identity;
            MRTransitionLog.LogWarning(
                $"RestoreXrOriginTrackingOffset fallback localPos={floor.localPosition}");
        }

        ConfigManager.WriteConsole(
            $"{LogPrefix} restored CameraFloorOffset localPos={floor.localPosition}");
    }

    static void ResetLegacyPassthroughFlags()
    {
        if (EventManager.Instance != null)
            EventManager.Instance.IsPassthrough = false;
    }

    void RememberVrPlayerPose()
    {
        Transform player = FindPlayerTransform();
        if (player == null)
            return;

        savedVrPlayerPosition = player.position;
        savedVrPlayerRotation = player.rotation;
        MRTransitionLog.Log($"saved VR player pose pos={savedVrPlayerPosition.Value} rotY={savedVrPlayerRotation.Value.eulerAngles.y:F1}");
        ConfigManager.WriteConsole($"{LogPrefix} saved VR player pose {savedVrPlayerPosition.Value}");
    }

    void RefreshPlayerControllerCameraOffset()
    {
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            MRTransitionLog.LogWarning("RefreshPlayerControllerCameraOffset skipped — PlayerController not found");
            return;
        }

        playerController.AdjustCameraYOffset();
        MRTransitionLog.LogStep("MixedRealityManager", "RefreshPlayerControllerCameraOffset");
    }

    // The VR rig applies a 0.9 camera-offset scale (player scale). That scale compresses the
    // headset's tracked motion, while MRUK anchors are placed at scale 1 — so in MR the virtual
    // room drifts relative to passthrough (the root cause of the "anchors deslocadas" report).
    // Force 1:1 scale while in MR and restore the VR scale on exit.
    static void ApplyMrColocatedScale()
    {
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            MRTransitionLog.LogWarning("ApplyMrColocatedScale skipped — PlayerController not found");
            return;
        }

        playerController.EnterMrColocatedScale();
        MRCameraRigAlignLog.LogCalibration("MR colocated player scale forced to 1:1");
        MRTransitionLog.LogStep("MixedRealityManager", "ApplyMrColocatedScale");
    }

    static void RestoreVrScale()
    {
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController == null || !playerController.IsMrScaleActive)
            return;

        playerController.RestoreScaleFromMr();
        MRCameraRigAlignLog.LogCalibration("VR player scale restored");
        MRTransitionLog.LogStep("MixedRealityManager", "RestoreVrScale");
    }

    void RestoreVrPlayerPose()
    {
        TryRestoreVrPlayerPose();
    }

    bool TryRestoreVrPlayerPose()
    {
        if (!savedVrPlayerPosition.HasValue)
        {
            MRTransitionLog.LogWarning("RestoreVrPlayerPose skipped — no saved pose");
            return false;
        }

        Transform player = FindPlayerTransform();
        if (player == null)
        {
            MRTransitionLog.LogError("RestoreVrPlayerPose failed — player transform null");
            return false;
        }

        Quaternion rotation = savedVrPlayerRotation ?? player.rotation;
        player.SetPositionAndRotation(savedVrPlayerPosition.Value, rotation);
        MRTransitionLog.Log($"restored VR player pose pos={savedVrPlayerPosition.Value} rotY={rotation.eulerAngles.y:F1}");
        ConfigManager.WriteConsole($"{LogPrefix} restored VR player pose {savedVrPlayerPosition.Value}");

        savedVrPlayerPosition = null;
        savedVrPlayerRotation = null;
        return true;
    }

    void RememberMrPlayerPose()
    {
        Transform player = FindPlayerTransform();
        if (player == null)
            return;

        Vector3 position = player.position;
        Quaternion rotation = player.rotation;
        PlayerPrefs.SetString(SavedMrPlayerPositionKey, JsonUtility.ToJson(MRVector3.From(position)));
        PlayerPrefs.SetString(SavedMrPlayerRotationKey, JsonUtility.ToJson(MRQuaternion.From(rotation)));
        PlayerPrefs.Save();
        MRTransitionLog.Log($"saved MR player pose pos={position} rotY={rotation.eulerAngles.y:F1}");
    }

    void RestoreMrPlayerPose()
    {
        string posJson = PlayerPrefs.GetString(SavedMrPlayerPositionKey, string.Empty);
        string rotJson = PlayerPrefs.GetString(SavedMrPlayerRotationKey, string.Empty);
        if (string.IsNullOrEmpty(posJson) || string.IsNullOrEmpty(rotJson))
        {
            MRTransitionLog.Log("RestoreMrPlayerPose skipped — no saved MR pose");
            return;
        }

        MRVector3 pos = JsonUtility.FromJson<MRVector3>(posJson);
        MRQuaternion rot = JsonUtility.FromJson<MRQuaternion>(rotJson);
        if (pos == null || rot == null)
            return;

        Transform player = FindPlayerTransform();
        if (player == null)
        {
            MRTransitionLog.LogError("RestoreMrPlayerPose failed — player transform null");
            return;
        }

        Vector3 position = pos.ToVector3();
        Quaternion rotation = rot.ToQuaternion();
        player.SetPositionAndRotation(position, rotation);
        MRTransitionLog.Log($"restored MR player pose pos={position} rotY={rotation.eulerAngles.y:F1}");
        ConfigManager.WriteConsole($"{LogPrefix} restored MR player pose {position}");
    }

    static void CancelActivePlacementRay()
    {
        MRPlacementRayController ray = FindObjectOfType<MRPlacementRayController>();
        if (ray != null && ray.IsActive)
            ray.CancelActive();
    }

    void EnsureMRSpaceOrigin()
    {
        if (MRSpaceOrigin != null)
            return;

        var player = FindPlayerTransform();
        Vector3 originPosition = player != null ? player.position : Vector3.zero;
        originPosition.y = 0f;

        var originGo = new GameObject("MRSpaceOrigin");
        originGo.transform.position = originPosition;
        originGo.transform.rotation = Quaternion.identity;
        MRSpaceOrigin = originGo.transform;
        ConfigManager.WriteConsole($"{LogPrefix} MRSpaceOrigin at {originPosition}");
    }

    void DestroyMRSpaceOrigin()
    {
        if (MRSpaceOrigin == null)
            return;
        Destroy(MRSpaceOrigin.gameObject);
        MRSpaceOrigin = null;
    }

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

    void SetMode(ExperienceMode mode)
    {
        if (CurrentMode == mode)
            return;
        CurrentMode = mode;
        MRTransitionLog.Log($"SetMode {mode}");
        ConfigManager.WriteConsole($"{LogPrefix} mode={mode}");

        // Colocated MR needs a 1:1 camera-offset scale; VR uses the 0.9 player scale.
        // MR_EDIT stays colocated, so it keeps the MR scale untouched.
        if (mode == ExperienceMode.MR)
            ApplyMrColocatedScale();
        else if (mode == ExperienceMode.VR)
            RestoreVrScale();

        OnModeChanged?.Invoke(mode);
    }

    /// <summary>After Quest Space Setup — re-probe MRUK, refresh meshes and spawned poses.</summary>
    public IEnumerator RefreshEnvironmentAfterRoomScan(Transform player)
    {
        if (player == null)
            player = FindPlayerTransform();

        if (environmentSurfaces != null && player != null)
            yield return environmentSurfaces.ProbeWhenReady(player);

        // New/updated scan may change room UUIDs — rebind and respawn if needed.
        if (MRRoomIdentity.TryGetCurrentRoomId(out string roomId)
            && !string.Equals(roomId, MRActiveRoom.BoundRoomId, StringComparison.OrdinalIgnoreCase)
            && !roomLayoutSwitchInProgress
            && !transitionInProgress)
        {
            yield return SwitchActiveRoomLayoutCoroutine(roomId);
        }
        else
        {
            BindLayoutPathsToCurrentRoom();
        }

        mrEffectMesh?.ApplySettings();
        MREffectMeshVisibility.ApplySavedSettings();

        ActiveRegistry()?.RefreshAllSpawnedPosesFromLayout();
        if (!MRPlacementRayController.AnyActive)
            MRConfigurationCabinetController.Instance?.RefreshPoseForMrReentry();

        MRPhoneBoothPortal phoneBooth = MRPhoneBoothPortal.FindMrTravelerInstance(includeInactive: true);
        if (phoneBooth != null && phoneBooth.gameObject.activeSelf && environmentSurfaces != null)
            phoneBooth.ReconcileVerticalMrFloorSnap(environmentSurfaces);

        MRRoomInfoUI.Instance?.RefreshContent();
        ConfigManager.WriteConsole($"{LogPrefix} environment refreshed after room scan");
    }
}
