/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections;
using AOJ.Managers;
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
    MREnvironmentSurfaces environmentSurfaces;
    bool transitionInProgress;
    Coroutine runningTransition;
    int transitionGeneration;

    Vector3? savedVrPlayerPosition;
    Quaternion? savedVrPlayerRotation;

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

        environmentSurfaces = GetComponent<MREnvironmentSurfaces>();
        if (environmentSurfaces == null)
            environmentSurfaces = gameObject.AddComponent<MREnvironmentSurfaces>();

        if (GetComponent<MRRoomInfoUI>() == null)
            gameObject.AddComponent<MRRoomInfoUI>();

        if (GetComponent<MRModeInput>() == null)
            gameObject.AddComponent<MRModeInput>();

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

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
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
        BeginTransition(EnterMRCoroutine());
    }

    /// <summary>Immersive VR→MR via phone booth — player stays inside the rescued booth.</summary>
    public void EnterMRFromPhoneBooth(MRPhoneBoothPortal portal)
    {
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
        ActiveRegistry()?.SnapshotSpawnedWorldPosesToLayout();
        ActiveEnvironmentRegistry()?.SnapshotSpawnedWorldPosesToLayout();
        MRConfigurationCabinetController.Instance?.HideForMrExit();
        mrLighting?.Despawn();
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
        yield return passthrough.EnablePassthroughWhenReady();
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

    IEnumerator EnterMRCoroutine()
    {
        int generation = transitionGeneration;
        MRTransitionLog.LogStep("EnterMRCoroutine", $"start generation={generation} mode={CurrentMode}");
        MRTransitionLog.LogManagerState("EnterMRCoroutine-start");
        ConfigManager.WriteConsole($"{LogPrefix} EnterMR coroutine (mode={CurrentMode})");

        MRScenePermissions.Reset();
        MRTransitionLog.LogStep("EnterMRCoroutine", "before EnsureGranted");
        yield return MRScenePermissions.EnsureGranted();
        if (!IsTransitionCurrent(generation))
            yield break;

        MRRoomInfoUI.Instance?.RefreshContent();

        if (MRRuntimeSettings.RememberVrPoseOnStandardEnterMr)
            RememberVrPlayerPose();

        MRTransitionLog.LogStep("EnterMRCoroutine", "before SuspendForMR");
        MRVrSystemsGate.SuspendForMR();

        yield return UnloadVrScenesUnderBlackoutThenPassthrough(generation);
        if (!IsTransitionCurrent(generation))
            yield break;

        DestroyMRSpaceOrigin();
        RestoreMrPlayerPose();
        MRTransitionLog.LogStep("EnterMRCoroutine", "after RestoreMrPlayerPose");

        EnsureMRSpaceOrigin();
        Transform player = FindPlayerTransform();
        if (environmentSurfaces != null)
            yield return environmentSurfaces.ProbeWhenReady(player);
        if (!IsTransitionCurrent(generation))
            yield break;

        if (environmentSurfaces != null && MRSpaceOrigin != null)
            environmentSurfaces.AlignOriginToFloor(MRSpaceOrigin, player);

        MRTransitionLog.LogStep("EnterMRCoroutine", "before SetMode MR");
        SetMode(ExperienceMode.MR);
        MRTransitionLog.LogStep("EnterMRCoroutine", "after SetMode MR");

        mrLighting?.Spawn(MRSpaceOrigin);
        ActiveRegistry()?.SpawnAll(MRSpaceOrigin);
        ActiveEnvironmentRegistry()?.SpawnAll(MRSpaceOrigin);
        MRConfigurationCabinetController.Instance?.SpawnAtMrOrigin();

        yield return null;
        ActiveRegistry()?.EnsureAttractPlaybackOnSpawned();

        yield return RefreshMrPosesWhenReady(generation, player);

        MRPhoneBoothVisibility.EnsureMrInstance();
        MRPhoneBoothVisibility.ApplySavedVisibility();

        MRTransitionLog.LogManagerState("EnterMRCoroutine-final");
        ConfigManager.WriteConsole($"{LogPrefix} EnterMR done");
        MRTransitionLog.LogStep("EnterMRCoroutine", "DONE");
    }

    IEnumerator EnterMRFromPhoneBoothCoroutine(MRPhoneBoothPortal portal)
    {
        int generation = transitionGeneration;
        PhoneBoothTravelState travelState = portal.ConsumePendingTravelState();
        MRTransitionLog.LogStep("EnterMRFromPhoneBoothCoroutine", $"start generation={generation}");
        ConfigManager.WriteConsole($"{LogPrefix} EnterMRFromPhoneBooth coroutine");

        MRScenePermissions.Reset();
        yield return MRScenePermissions.EnsureGranted();
        if (!IsTransitionCurrent(generation))
            yield break;

        MRRoomInfoUI.Instance?.RefreshContent();
        MRVrSystemsGate.SuspendForMR();

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

        if (environmentSurfaces != null && MRSpaceOrigin != null)
            environmentSurfaces.AlignOriginToFloor(MRSpaceOrigin, player);

        passthrough.RefreshPassthroughAfterSceneUnload();

        SetMode(ExperienceMode.MR);
        mrLighting?.Spawn(MRSpaceOrigin);
        ActiveRegistry()?.SpawnAll(MRSpaceOrigin);
        ActiveEnvironmentRegistry()?.SpawnAll(MRSpaceOrigin);
        MRConfigurationCabinetController.Instance?.SpawnAtMrOrigin();

        yield return null;
        ActiveRegistry()?.EnsureAttractPlaybackOnSpawned();
        yield return RefreshMrPosesWhenReady(generation, player);

        portal.PlaceOnMrFloor(environmentSurfaces, player);
        ApplyPhoneBoothTravelState(portal, travelState);
        MRPhoneBoothSettings.SetVisible(true);
        portal.SetVisible(true);

        yield return portal.PlayTravelArrivalExplosionAndRestoreGlassDoor();
        if (!IsTransitionCurrent(generation))
            yield break;

        portal.NotifyHandsetsTravelComplete();

        MRTransitionLog.LogManagerState("EnterMRFromPhoneBoothCoroutine-final");
        ConfigManager.WriteConsole($"{LogPrefix} EnterMRFromPhoneBooth done");
        MRTransitionLog.LogStep("EnterMRFromPhoneBoothCoroutine", "DONE");
    }

    sealed class PhoneBoothMrToVrContext
    {
        public MRPhoneBoothPortal travelerPortal;
        public PhoneBoothTravelState travelState;
        public PayphoneHandsetGrab.VrReturnHandsetPlan handsetPlan;
        public MRPhoneBoothPortal scenePortal;
        public AudioClip arrivalExplosionClip;
        public bool restoredGalleryPose;
    }

    IEnumerator EnterVRFromPhoneBoothCoroutine(MRPhoneBoothPortal travelerPortal)
    {
        int generation = transitionGeneration;
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
        }

        MRTransitionLog.LogManagerState("EnterVRFromPhoneBoothCoroutine-final");
        ConfigManager.WriteConsole($"{LogPrefix} EnterVRFromPhoneBooth done");
        MRTransitionLog.LogStep("EnterVRFromPhoneBoothCoroutine", "DONE");
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
                break;

            case MRPhoneBoothTransitionSequence.MrToVrReturnStep.EnableVrModeAndLocomotion:
                passthrough.RebindCameraAndDisablePassthrough(playFadeOut: false);
                ResetLegacyPassthroughFlags();
                SetMode(ExperienceMode.VR);
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

    IEnumerator RefreshMrPosesWhenReady(int generation, Transform player)
    {
        MRTransitionLog.LogStep("EnterMRCoroutine", "RefreshMrPoses begin");
        const int maxAttempts = 20;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (!IsTransitionCurrent(generation))
                yield break;

            if (environmentSurfaces != null && player != null && !environmentSurfaces.IsReady)
                yield return environmentSurfaces.ProbeWhenReady(player);

            ActiveRegistry()?.RefreshAllSpawnedPosesFromLayout();
            MRConfigurationCabinetController.Instance?.RefreshPoseForMrReentry();

            if (attempt == 0 || attempt == maxAttempts - 1)
                MRTransitionLog.LogManagerState($"EnterMRCoroutine-refresh-{attempt}");

            yield return null;
        }

        MRTransitionLog.LogStep("EnterMRCoroutine", "RefreshMrPoses end");
    }

    IEnumerator EnterVRCoroutine()
    {
        int generation = transitionGeneration;
        MRTransitionLog.LogStep("EnterVRCoroutine", $"start generation={generation} mode={CurrentMode}");
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

        MRVrSystemsGate.StopActiveLibretroGames();
        MRTransitionLog.LogStep("EnterVRCoroutine", "after StopActiveLibretroGames-final");

        environmentSurfaces?.ClearMrukScene();
        MRRoomInfoUI.Instance?.Hide();
        DestroyMRSpaceOrigin();
        MRTransitionLog.LogStep("EnterVRCoroutine", "after MR cleanup");

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
        OnModeChanged?.Invoke(mode);
    }
}
