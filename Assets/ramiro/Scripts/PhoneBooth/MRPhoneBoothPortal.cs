/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using UnityEngine;

/// <summary>
/// Phone booth portal on PF_Payphone — handset grab triggers immersive VR↔MR travel.
/// </summary>
public class MRPhoneBoothPortal : MonoBehaviour
{
    const string LogPrefix = "[MRPhoneBoothPortal]";
    const string HandsetAudioCueObjectName = "AudioCue";
    const string SpaceshipEngineAudioObjectName = "AudioSpaceshipEngine";
    const string ExplosionAudioObjectName = "AudioExplosion";
    const string ExplosionSmokeObjectName = "ParticleSmoke";
    static MRPhoneBoothPortal activeTraveler;

    [SerializeField] BoxCollider interiorTrigger;
    [Tooltip("Child named AudioCue (PF_Grabbable_Phone) — plays when handset is grabbed, before travel.")]
    [SerializeField] AudioSource handsetAudioCue;
    [Tooltip("Child named AudioSpaceshipEngine — plays during fade; MR/VR load runs after it ends.")]
    [SerializeField] AudioSource spaceshipEngineAudio;
    [Tooltip("Child named AudioExplosion — plays when immersive travel finishes (after MR/VR load).")]
    [SerializeField] AudioSource explosionAudio;
    [Tooltip("Child named ParticleSmoke — burst when arrival explosion plays.")]
    [SerializeField] ParticleSystem explosionSmoke;

    bool isTravelerInstance;
    int playersInside;
    bool travelInProgress;
    bool travelPastHandsetCue;
    bool travelCancelRequested;
    bool handsetGrabbed;
    PhoneBoothTravelState pendingTravelState;
    PhoneBoothJourneyDirection currentJourneyDirection;
    Coroutine travelCoroutine;
    MRPhoneBoothTravelVfx travelVfx;

    public static MRPhoneBoothPortal ActiveTraveler => activeTraveler;

    public bool IsTravelerInstance
    {
        get => isTravelerInstance;
        set => isTravelerInstance = value;
    }

    public bool TravelInProgress => travelInProgress;
    public bool TravelPastHandsetCue => travelPastHandsetCue;
    public bool PlayerInside => playersInside > 0;

    void Awake()
    {
        EnsureInteriorTrigger();
        EnsureHandsetAudioCue();
        EnsureSpaceshipEngineAudio();
        EnsureExplosionAudio();
        PrepareExplosionSmokeIdle();
        EnsureTravelVfx();
    }

    void OnEnable()
    {
        SceneManagerHook.EnsureRegistered();
    }

    void OnDisable()
    {
        if (travelVfx != null && travelVfx.IsJourneyActive)
            travelVfx.EndJourneyVisuals(currentJourneyDirection);
    }

    public PhoneBoothTravelState CaptureTravelState()
    {
        Transform player = FindPlayerTransform();
        if (player == null)
            return null;

        return PhoneBoothTravelState.Capture(transform, player, handsetGrabbed);
    }

    public void ApplyTravelState(PhoneBoothTravelState state)
    {
        if (state == null)
            return;

        Transform player = FindPlayerTransform();
        state.ApplyToPlayer(transform, player);
        MRTransitionLog.Log($"{LogPrefix} applied travel state inside={PlayerInside} traveler={isTravelerInstance}");
    }

    /// <summary>Called by PayphoneHandsetGrab when the handset is grabbed.</summary>
    public void NotifyHandsetGrabbedForTravel()
    {
        if (TryDelegateHandsetTravelToMrTraveler())
            return;

        handsetGrabbed = true;
        ConfigManager.WriteConsole($"{LogPrefix} handset grabbed — trying travel mode={MixedRealityManager.Instance?.CurrentMode}");
        TryStartTravelFromHandset();

        if (!travelInProgress)
            handsetGrabbed = false;
    }

    /// <summary>Handset released — cancel travel only before the first (handset) audio finishes.</summary>
    public void NotifyHandsetReleasedDuringTravel()
    {
        if (!travelInProgress || travelPastHandsetCue)
            return;

        CancelTravelBeforeCommit();
    }

    public void BeginTravelToMR()
    {
        if (travelInProgress || MixedRealityManager.Instance == null)
            return;

        if (MixedRealityManager.Instance.CurrentMode != ExperienceMode.VR)
            return;

        if (!CanStartTravel())
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} BeginTravelToMR ignored — player not at booth");
            return;
        }

        if (!MixedRealityManager.Instance.CanToggleMode())
            return;

        pendingTravelState = CaptureTravelState();
        if (pendingTravelState == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} BeginTravelToMR failed — no travel state");
            return;
        }

        MixedRealityManager.Instance.RememberVrPlayerPoseForPhoneBoothTravel();

        MRTransitionLog.LogStep("MRPhoneBoothPortal", "BeginTravelToMR");
        currentJourneyDirection = PhoneBoothJourneyDirection.ToMR;
        travelCoroutine = StartCoroutine(PlayTravelThen(() =>
            MixedRealityManager.Instance.EnterMRFromPhoneBooth(this)));
    }

    public void BeginTravelToVR()
    {
        if (travelInProgress || MixedRealityManager.Instance == null)
            return;

        if (!MixedRealityManager.Instance.IsMrEnvironmentActive())
            return;

        if (!CanStartTravel())
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} BeginTravelToVR ignored — player not at booth");
            return;
        }

        if (!MixedRealityManager.Instance.CanToggleMode())
            return;

        pendingTravelState = CaptureTravelState();
        if (pendingTravelState == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} BeginTravelToVR failed — no travel state");
            return;
        }

        MRTransitionLog.LogStep("MRPhoneBoothPortal", "BeginTravelToVR");
        MRVrSystemsGate.SilenceAllCabinetScreensForPhoneBoothTravelToVr();
        currentJourneyDirection = PhoneBoothJourneyDirection.ToVR;
        travelCoroutine = StartCoroutine(PlayTravelThen(() =>
            MixedRealityManager.Instance.EnterVRFromPhoneBooth(this)));
    }

    public PhoneBoothTravelState ConsumePendingTravelState()
    {
        PhoneBoothTravelState state = pendingTravelState;
        pendingTravelState = null;
        return state;
    }

    Coroutine hideCoroutine;

    public bool IsVisible => gameObject.activeSelf;

    public void ApplyMrVisibility()
    {
        MRPhoneBoothVisibility.ApplySavedVisibility();
    }

    public void SetVisible(bool visible, bool playHideEffect = true)
    {
        if (visible)
        {
            CancelHideEffect();
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
            return;
        }

        if (!gameObject.activeSelf)
            return;

        if (!playHideEffect)
        {
            CancelHideEffect();
            gameObject.SetActive(false);
            return;
        }

        if (hideCoroutine != null)
            return;

        hideCoroutine = StartCoroutine(HideAfterEffectRoutine());
    }

    void CancelHideEffect()
    {
        if (hideCoroutine == null)
            return;

        StopCoroutine(hideCoroutine);
        hideCoroutine = null;
    }

    IEnumerator HideAfterEffectRoutine()
    {
        yield return PlayHideEffectBeforeDisable();
        hideCoroutine = null;
        gameObject.SetActive(false);
    }

    /// <summary>Override or extend later for dissolve/fade before the booth is disabled.</summary>
    protected virtual IEnumerator PlayHideEffectBeforeDisable()
    {
        yield break;
    }

    /// <summary>Restore every handset on this booth after immersive travel.</summary>
    public void NotifyHandsetsTravelComplete()
    {
        PayphoneHandsetGrab onPortal = PayphoneHandsetGrab.FindOnPortal(this);
        if (onPortal != null)
        {
            onPortal.NotifyBoothTravelComplete();
            return;
        }

        PayphoneHandsetGrab[] grabs = FindObjectsOfType<PayphoneHandsetGrab>();
        foreach (PayphoneHandsetGrab grab in grabs)
        {
            if (grab == null || grab.BelongsToTravelerBooth())
                continue;

            grab.NotifyBoothTravelComplete();
        }
    }

    public void PlaceOnMrFloor(MREnvironmentSurfaces surfaces, Transform player)
    {
        if (MRPhoneBoothSettings.TryGetMrPose(out Vector3 savedPos, out Quaternion savedRot))
        {
            transform.SetPositionAndRotation(savedPos, savedRot);
            ConfigManager.WriteConsole($"{LogPrefix} placed booth from saved MR pose");
            return;
        }

        Vector3 near = player != null ? player.position : transform.position;
        Vector3 floorPoint = near;
        if (surfaces != null)
            surfaces.TryGetFloorPointAt(near, out floorPoint);

        float bottomY = ComputeLowestWorldY();
        transform.position += Vector3.up * (floorPoint.y - bottomY);

        if (player != null)
        {
            Vector3 toPlayer = player.position - transform.position;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude > 0.04f)
                transform.rotation = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
        }

        MRPhoneBoothSettings.SaveMrPose(transform.position, transform.rotation);
        ConfigManager.WriteConsole($"{LogPrefix} placed booth on MR floor at {transform.position}");
    }

    public static void AdoptAsTraveler(MRPhoneBoothPortal portal, Transform ddolParent)
    {
        if (portal == null || ddolParent == null)
            return;

        if (activeTraveler != null && activeTraveler != portal)
            DestroyTravelerInstance();

        PayphoneHandsetGrab.AttachLooseHandsetsToBooth(portal);

        activeTraveler = portal;
        portal.isTravelerInstance = true;
        portal.transform.SetParent(ddolParent, worldPositionStays: true);
        ConfigManager.WriteConsole($"{LogPrefix} adopted traveler booth '{portal.name}'");
        MRTransitionLog.LogStep("MRPhoneBoothPortal", "AdoptAsTraveler");
    }

    public static void DestroyTravelerInstance(bool restoreHiddenHands = true)
    {
        if (restoreHiddenHands)
            PayphoneHandsetGrab.ReleaseAllHiddenHandVisuals();

        PayphoneHandsetGrab.DestroyDetachedInstances();

        if (activeTraveler == null)
            return;

        GameObject go = activeTraveler.gameObject;
        activeTraveler = null;
        if (go != null)
            Destroy(go);

        MRTransitionLog.LogStep("MRPhoneBoothPortal", "DestroyTravelerInstance");
    }

    public static MRPhoneBoothPortal FindMrTravelerInstance(bool includeInactive = true)
    {
        if (activeTraveler != null)
            return activeTraveler;

        MixedRealityManager manager = MixedRealityManager.Instance;
        if (manager == null)
            return null;

        return manager.GetComponentInChildren<MRPhoneBoothPortal>(includeInactive);
    }

    public static MRPhoneBoothPortal EnsureMrTravelerInstance()
    {
        MRPhoneBoothPortal traveler = FindMrTravelerInstance(includeInactive: true);
        if (traveler != null)
        {
            if (activeTraveler == null)
                activeTraveler = traveler;
            return traveler;
        }

        MixedRealityManager manager = MixedRealityManager.Instance;
        if (manager == null)
            return null;

        GameObject prefab = Resources.Load<GameObject>("Decoration/PhoneBooth/PF_Payphone");
        if (prefab == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} prefab missing Resources/Decoration/PhoneBooth/PF_Payphone");
            return null;
        }

        GameObject instance = Object.Instantiate(prefab);
        instance.name = MRRuntimeSettings.PhoneBoothObject;
        MRPhoneBoothPortal portal = EnsureOn(instance);
        AdoptAsTraveler(portal, manager.transform);

        Transform player = FindPlayerTransform();
        MREnvironmentSurfaces surfaces = manager.GetComponent<MREnvironmentSurfaces>();
        portal.PlaceOnMrFloor(surfaces, player);
        ConfigManager.WriteConsole($"{LogPrefix} ensured MR phone booth instance");
        return portal;
    }

    public static MRPhoneBoothPortal FindSceneBoothPortal()
    {
        MRPhoneBoothPortal[] portals = FindObjectsOfType<MRPhoneBoothPortal>(includeInactive: true);
        foreach (MRPhoneBoothPortal portal in portals)
        {
            if (portal.isTravelerInstance)
                continue;

            if (portal.gameObject.scene.name == MRRuntimeSettings.ExteriorScene)
                return portal;
        }

        GameObject booth = GameObject.Find(MRRuntimeSettings.PhoneBoothObject);
        return booth != null ? EnsureOn(booth) : null;
    }

    bool CanStartTravel() => PlayerInside || handsetGrabbed;

#if UNITY_EDITOR
    /// <summary>Editor P-key simulator — satisfy CanStartTravel / CaptureTravelState without trigger volume.</summary>
    public void EditorMarkReadyForSimulatedTravel()
    {
        playersInside = Mathf.Max(1, playersInside);
        handsetGrabbed = true;
    }

    /// <summary>Fallback when grab simulation did not start the coroutine.</summary>
    public void EditorRetryStartTravelFromHandset()
    {
        handsetGrabbed = true;
        TryStartTravelFromHandset();
    }
#endif

    void EnsureInteriorTrigger()
    {
        if (interiorTrigger != null)
            return;

        var volumeGo = new GameObject("InteriorVolume");
        volumeGo.transform.SetParent(transform, false);
        volumeGo.transform.localPosition = new Vector3(0f, 1f, -0.15f);
        volumeGo.transform.localRotation = Quaternion.identity;

        interiorTrigger = volumeGo.AddComponent<BoxCollider>();
        interiorTrigger.isTrigger = true;
        interiorTrigger.size = new Vector3(1.1f, 2.2f, 1.1f);
        interiorTrigger.center = Vector3.zero;

        var relay = volumeGo.AddComponent<MRPhoneBoothInteriorRelay>();
        relay.Portal = this;
    }

    bool TryDelegateHandsetTravelToMrTraveler()
    {
        if (isTravelerInstance)
            return false;

        MixedRealityManager manager = MixedRealityManager.Instance;
        if (manager == null || !manager.IsMrEnvironmentActive())
            return false;

        MRPhoneBoothPortal traveler = FindMrTravelerInstance(includeInactive: true);
        if (traveler == null || traveler == this)
            return false;

        ConfigManager.WriteConsole(
            $"{LogPrefix} delegating handset travel from scene booth '{name}' to traveler '{traveler.name}'");
        traveler.NotifyHandsetGrabbedForTravel();
        return true;
    }

    void TryStartTravelFromHandset()
    {
        if (TryDelegateHandsetTravelToMrTraveler())
            return;

        if (travelInProgress)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} travel ignored — already in progress");
            return;
        }

        if (!CanStartTravel())
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} travel ignored — player not at booth");
            return;
        }

        if (MixedRealityManager.Instance == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} travel ignored — MixedRealityManager missing");
            return;
        }

        if (MixedRealityManager.Instance.CurrentMode == ExperienceMode.VR)
            BeginTravelToMR();
        else if (MixedRealityManager.Instance.IsMrEnvironmentActive())
            BeginTravelToVR();
        else
            ConfigManager.WriteConsoleWarning($"{LogPrefix} travel ignored — mode={MixedRealityManager.Instance.CurrentMode}");
    }

    internal void NotifyPlayerEntered()
    {
        playersInside++;
    }

    internal void NotifyPlayerExited()
    {
        playersInside = Mathf.Max(0, playersInside - 1);
    }

    IEnumerator PlayTravelThen(System.Action onComplete)
    {
        travelInProgress = true;
        travelPastHandsetCue = false;
        travelCancelRequested = false;
        EnsureTravelVfx();
        MRTransitionLog.LogStep("MRPhoneBoothPortal", "PlayTravelEffect start");

        foreach (MRPhoneBoothTransitionSequence.ImmersiveTravelStep step in
                 MRRuntimeSettings.ImmersiveTravelSteps)
        {
            if (travelCancelRequested)
                yield break;

            yield return RunImmersiveTravelStep(step);
            if (travelCancelRequested)
                yield break;
        }

        CompleteTravelSequence();
        MRTransitionLog.LogStep("MRPhoneBoothPortal", "PlayTravelEffect end");
        onComplete?.Invoke();
    }

    IEnumerator RunImmersiveTravelStep(MRPhoneBoothTransitionSequence.ImmersiveTravelStep step)
    {
        MRTransitionLog.LogStep("MRPhoneBoothPortal", $"immersive step {step}");

        switch (step)
        {
            case MRPhoneBoothTransitionSequence.ImmersiveTravelStep.HandsetAudioCue:
                yield return PlayHandsetAudioCueAndWait();
                travelPastHandsetCue = true;
                break;

            case MRPhoneBoothTransitionSequence.ImmersiveTravelStep.BeginJourneyVisuals:
                MRTransitionLog.LogStep("MRPhoneBoothPortal", "Travel journey start (spaceship engine)");
                travelVfx?.BeginJourneyVisuals(currentJourneyDirection);
                yield return WaitOptionalImmersiveStep(
                    MRRuntimeSettings.ImmersiveBeginJourneyStepDurationSeconds);
                break;

            case MRPhoneBoothTransitionSequence.ImmersiveTravelStep.FadeSphereIn:
                var fadeSphere = GameObject.Find("SM_FadeSphere");
                Animator fadeAnimator = fadeSphere != null ? fadeSphere.GetComponent<Animator>() : null;
                if (fadeAnimator != null)
                    fadeAnimator.SetTrigger("FadeInTrigger");
                yield return WaitOptionalImmersiveStep(
                    MRRuntimeSettings.ImmersiveFadeSphereStepDurationSeconds);
                break;

            case MRPhoneBoothTransitionSequence.ImmersiveTravelStep.SpaceshipEngine:
                yield return PlaySpaceshipEngineAndWait();
                break;

            case MRPhoneBoothTransitionSequence.ImmersiveTravelStep.EndJourneyVisuals:
                travelVfx?.EndJourneyVisuals(currentJourneyDirection);
                yield return WaitOptionalImmersiveStep(
                    MRRuntimeSettings.ImmersiveEndJourneyStepDurationSeconds);
                break;
        }
    }

    void CancelTravelBeforeCommit()
    {
        if (!travelInProgress || travelPastHandsetCue)
            return;

        travelCancelRequested = true;

        if (travelCoroutine != null)
        {
            StopCoroutine(travelCoroutine);
            travelCoroutine = null;
        }

        EnsureHandsetAudioCue();
        if (handsetAudioCue != null)
            handsetAudioCue.Stop();

        if (travelVfx != null && travelVfx.IsJourneyActive)
            travelVfx.EndJourneyVisuals(currentJourneyDirection);

        travelInProgress = false;
        travelPastHandsetCue = false;
        handsetGrabbed = false;
        pendingTravelState = null;
        ConfigManager.WriteConsole($"{LogPrefix} travel cancelled — handset released before AudioCue finished");
        MRTransitionLog.LogStep("MRPhoneBoothPortal", "CancelTravelBeforeCommit");
    }

    void CompleteTravelSequence()
    {
        travelInProgress = false;
        travelPastHandsetCue = false;
        travelCancelRequested = false;
        travelCoroutine = null;
        handsetGrabbed = false;
    }

    void EnsureTravelVfx()
    {
        if (travelVfx != null)
            return;

        travelVfx = GetComponent<MRPhoneBoothTravelVfx>();
        if (travelVfx == null)
            travelVfx = gameObject.AddComponent<MRPhoneBoothTravelVfx>();
    }

    void EnsureHandsetAudioCue()
    {
        if (handsetAudioCue != null)
            return;

        handsetAudioCue = FindChildAudioSource(HandsetAudioCueObjectName);
    }

    void EnsureSpaceshipEngineAudio()
    {
        if (spaceshipEngineAudio != null)
            return;

        spaceshipEngineAudio = FindChildAudioSource(SpaceshipEngineAudioObjectName);
    }

    void EnsureExplosionAudio()
    {
        if (explosionAudio != null)
            return;

        explosionAudio = FindChildAudioSource(ExplosionAudioObjectName);
    }

    void EnsureExplosionSmoke()
    {
        if (explosionSmoke != null)
            return;

        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (!string.Equals(child.name, ExplosionSmokeObjectName, System.StringComparison.OrdinalIgnoreCase))
                continue;

            ParticleSystem smoke = child.GetComponent<ParticleSystem>();
            if (smoke == null)
                continue;

            explosionSmoke = smoke;
            return;
        }
    }

    void PrepareExplosionSmokeIdle()
    {
        EnsureExplosionSmoke();
        if (explosionSmoke == null)
            return;

        explosionSmoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        explosionSmoke.gameObject.SetActive(false);
    }

    void PlayArrivalExplosionSmoke()
    {
        EnsureExplosionSmoke();
        if (explosionSmoke == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} {ExplosionSmokeObjectName} missing — skipping smoke burst");
            return;
        }

        EnsureBoothActiveForArrivalEffects();

        GameObject smokeObject = explosionSmoke.gameObject;
        if (!smokeObject.activeSelf)
            smokeObject.SetActive(true);

        explosionSmoke.Clear(true);
        explosionSmoke.Play(true);
        MRTransitionLog.LogStep("MRPhoneBoothPortal", "ParticleSmoke play");
        ConfigManager.WriteConsole($"{LogPrefix} {ExplosionSmokeObjectName} play");
    }

    void EnsureBoothActiveForArrivalEffects()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    AudioSource FindChildAudioSource(string objectName)
    {
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (!string.Equals(child.name, objectName, System.StringComparison.OrdinalIgnoreCase))
                continue;

            AudioSource source = child.GetComponent<AudioSource>();
            if (source != null)
                return source;
        }

        return null;
    }

    IEnumerator PlayHandsetAudioCueAndWait()
    {
        EnsureHandsetAudioCue();
        yield return PlayAudioSourceForFixedDuration(
            handsetAudioCue,
            HandsetAudioCueObjectName,
            MRRuntimeSettings.ImmersiveHandsetCueStepDurationSeconds);
    }

    IEnumerator PlaySpaceshipEngineAndWait()
    {
        float stepDuration = MRRuntimeSettings.ImmersiveSpaceshipEngineStepDurationSeconds;
        if (stepDuration <= 0f)
            stepDuration = MRRuntimeSettings.DefaultImmersiveSpaceshipEngineStepDurationSeconds;

        EnsureSpaceshipEngineAudio();
        if (spaceshipEngineAudio == null || spaceshipEngineAudio.clip == null)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} {SpaceshipEngineAudioObjectName} missing or has no clip — fallback travel wait");
            yield return WaitImmersiveStepDuration(Mathf.Max(1f, stepDuration));
            yield break;
        }

        yield return PlayAudioSourceForFixedDuration(
            spaceshipEngineAudio,
            SpaceshipEngineAudioObjectName,
            stepDuration);
    }

    /// <summary>Starts clip and waits a fixed time; does not stop the source when the step ends (overlap OK).</summary>
    IEnumerator PlayAudioSourceForFixedDuration(AudioSource source, string objectName, float durationSeconds)
    {
        if (source == null || source.clip == null)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} {objectName} missing or has no clip — skipping");
            yield break;
        }

        if (!source.gameObject.activeInHierarchy)
            source.gameObject.SetActive(true);

        MRTransitionLog.LogStep("MRPhoneBoothPortal",
            $"{objectName} play clip={source.clip.name} step={durationSeconds:F2}s (fixed, no stop on advance)");

        source.Stop();
        source.Play();

        yield return WaitImmersiveStepDuration(durationSeconds);
        MRTransitionLog.LogStep("MRPhoneBoothPortal", $"{objectName} step duration elapsed (audio may still play)");
    }

    IEnumerator WaitOptionalImmersiveStep(float durationSeconds)
    {
        if (durationSeconds <= 0f)
            yield break;

        yield return WaitImmersiveStepDuration(durationSeconds);
    }

    IEnumerator WaitImmersiveStepDuration(float durationSeconds)
    {
        float elapsed = 0f;
        while (elapsed < durationSeconds && !travelCancelRequested)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    static IEnumerator WaitStepDurationSeconds(float durationSeconds)
    {
        if (durationSeconds <= 0f)
            yield break;

        float elapsed = 0f;
        while (elapsed < durationSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    /// <summary>After MR/VR load completes — journey has ended.</summary>
    public IEnumerator PlayTravelArrivalExplosionAndWait()
    {
        MRTransitionLog.LogStep("MRPhoneBoothPortal", "Travel journey end (AudioExplosion)");
        yield return PlayArrivalExplosionAndWait(ResolveExplosionClip(), preferSpatialExplosionAudio: true);
    }

    /// <summary>Arrival explosion, then delay, then normal glass + travel door off (ToMR).</summary>
    public IEnumerator PlayTravelArrivalExplosionAndRestoreGlassDoor()
    {
        yield return PlayTravelArrivalExplosionAndWait();
        yield return WaitAfterArrivalExplosionBeforeGlassDoor();
        RestoreTravelGlassAndDoor();
    }

    public void RestoreTravelGlassAndDoor()
    {
        EnsureTravelVfx();
        travelVfx?.RestoreGlassAfterMrTransition();
    }

    IEnumerator WaitAfterArrivalExplosionBeforeGlassDoor()
    {
        float delay = MRRuntimeSettings.SecondsAfterArrivalExplosionBeforeGlassAndDoor;
        if (delay <= 0f)
            yield break;

        MRTransitionLog.LogStep("MRPhoneBoothPortal",
            $"wait {delay:F2}s after explosion before glass/door restore");
        yield return WaitStepDurationSeconds(delay);
    }

    /// <summary>Smoke + explosion audio on this booth (or 2D fallback clip).</summary>
    public IEnumerator PlayArrivalExplosionAndWait(AudioClip fallbackClip, bool preferSpatialExplosionAudio = true)
    {
        PlayArrivalExplosionSmoke();
        EnsureExplosionAudio();

        AudioClip clip = fallbackClip ?? ResolveExplosionClip();

        if (preferSpatialExplosionAudio
            && explosionAudio != null
            && explosionAudio.clip != null
            && gameObject.activeInHierarchy)
        {
            yield return PlayAudioSourceForFixedDuration(
                explosionAudio,
                ExplosionAudioObjectName,
                MRRuntimeSettings.ArrivalExplosionStepDurationSeconds);
            yield break;
        }

        yield return PlayArrivalExplosionClipForFixedDuration(clip);
    }

    /// <summary>MR→VR: explosion on scene booth, delay, then glass/door restore.</summary>
    public static IEnumerator PlayArrivalExplosionForVrReturnAndRestoreGlassDoor(
        MRPhoneBoothPortal scenePortal,
        AudioClip clip)
    {
        if (scenePortal == null)
            scenePortal = FindSceneBoothPortal();

        if (scenePortal != null)
        {
            yield return scenePortal.PlayArrivalExplosionAndWait(clip, preferSpatialExplosionAudio: true);
            yield return scenePortal.WaitAfterArrivalExplosionBeforeGlassDoor();
            scenePortal.RestoreTravelGlassAndDoor();
            yield break;
        }

        if (clip == null)
            yield break;

        yield return PlayArrivalExplosionClipForFixedDuration(clip);
        yield return WaitStepDurationSeconds(
            MRRuntimeSettings.SecondsAfterArrivalExplosionBeforeGlassAndDoor);
    }

    /// <summary>Arrival burst on a scene/traveler booth; audio falls back to 2D one-shot if needed.</summary>
    public static IEnumerator PlayArrivalExplosionAndWait(
        MRPhoneBoothPortal portal,
        AudioClip fallbackClip,
        bool preferSpatialExplosionAudio = false)
    {
        if (portal != null)
        {
            yield return portal.PlayArrivalExplosionAndWait(fallbackClip, preferSpatialExplosionAudio);
            yield break;
        }

        yield return PlayArrivalExplosionClipForFixedDuration(fallbackClip);
    }

    public AudioClip ResolveExplosionClip()
    {
        EnsureExplosionAudio();
        return explosionAudio != null ? explosionAudio.clip : null;
    }

    public static AudioClip ResolveExplosionClip(MRPhoneBoothPortal primary, MRPhoneBoothPortal fallback)
    {
        AudioClip clip = primary != null ? primary.ResolveExplosionClip() : null;
        if (clip != null)
            return clip;

        return fallback != null ? fallback.ResolveExplosionClip() : null;
    }

    /// <summary>2D one-shot — reliable after MR traveler booth is destroyed.</summary>
    public static IEnumerator PlayArrivalExplosionClipForFixedDuration(AudioClip clip)
    {
        if (clip == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} AudioExplosion clip missing — skipping");
            yield break;
        }

        float duration = MRRuntimeSettings.ArrivalExplosionStepDurationSeconds;
        MRTransitionLog.LogStep("MRPhoneBoothPortal",
            $"AudioExplosion play clip={clip.name} step={duration:F2}s (fixed)");

        var oneShotObject = new GameObject("PhoneBoothExplosionOneShot");
        Object.DontDestroyOnLoad(oneShotObject);
        AudioSource source = oneShotObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.spatialBlend = 0f;
        source.playOnAwake = false;
        source.volume = MRRuntimeSettings.ArrivalExplosionVolume;
        source.Play();

        yield return WaitStepDurationSeconds(duration);
        Destroy(oneShotObject);
        MRTransitionLog.LogStep("MRPhoneBoothPortal", "AudioExplosion step duration elapsed");
    }

    float ComputeLowestWorldY()
    {
        float minY = transform.position.y;
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>(includeInactive: true))
            minY = Mathf.Min(minY, renderer.bounds.min.y);

        foreach (Collider collider in GetComponentsInChildren<Collider>(includeInactive: true))
            minY = Mathf.Min(minY, collider.bounds.min.y);

        return minY;
    }

    static Transform FindPlayerTransform()
    {
        var pc = FindObjectOfType<PlayerController>();
        if (pc != null && pc.PlayerControllerGameObject != null)
            return pc.PlayerControllerGameObject.transform;

        var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
        return xrOrigin != null ? xrOrigin.transform : null;
    }

    /// <summary>Attach portal to scene booths that lack one.</summary>
    public static MRPhoneBoothPortal EnsureOn(GameObject boothRoot)
    {
        if (boothRoot == null)
            return null;

        var portal = boothRoot.GetComponent<MRPhoneBoothPortal>();
        if (portal == null)
            portal = boothRoot.AddComponent<MRPhoneBoothPortal>();

        return portal;
    }

    static class SceneManagerHook
    {
        static bool registered;

        public static void EnsureRegistered()
        {
            if (registered)
                return;

            registered = true;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (scene.name != MRRuntimeSettings.ExteriorScene)
                return;

            string boothName = MRRuntimeSettings.PhoneBoothObject;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name != boothName && !root.name.StartsWith(boothName))
                    continue;

                EnsureOn(root);
            }
        }
    }
}

/// <summary>Forwards trigger events from the interior volume child to the portal.</summary>
public class MRPhoneBoothInteriorRelay : MonoBehaviour
{
    public MRPhoneBoothPortal Portal;

    void OnTriggerEnter(Collider other)
    {
        if (Portal == null || !IsPlayerCollider(other))
            return;

        Portal.NotifyPlayerEntered();
    }

    void OnTriggerExit(Collider other)
    {
        if (Portal == null || !IsPlayerCollider(other))
            return;

        Portal.NotifyPlayerExited();
    }

    static bool IsPlayerCollider(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() != null)
            return true;

        Transform t = other.transform;
        while (t != null)
        {
            string n = t.name;
            if (n.Contains("OVRPlayerController") || n == "GrabVolumeSmall" || n == "GrabVolumeBig")
                return true;
            t = t.parent;
        }

        return false;
    }
}
