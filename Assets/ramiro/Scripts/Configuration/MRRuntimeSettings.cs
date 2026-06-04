/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MR tuning placed on a GameObject in FixedScene (Inspector). DontDestroyOnLoad singleton.
/// Other MR scripts read static accessors; defaults apply if the scene object is missing.
/// </summary>
[DefaultExecutionOrder(-50)]
public class MRRuntimeSettings : MonoBehaviour
{
    public const string DefaultFixedSceneName = "FixedScene";
    public const string DefaultExteriorSceneName = "IntroGalleryExterior";
    public const string DefaultIntroGallerySceneName = "IntroGallery";
    public const string DefaultPhoneBoothObjectName = "PF_Payphone";

    public static MRRuntimeSettings Instance { get; private set; }

    [Header("Scenes")]
    [Tooltip("Always loaded base scene (VR rig, global config).")]
    public string fixedSceneName = DefaultFixedSceneName;

    [Tooltip("Additive scene with the payphone booth (IntroGallery exterior).")]
    public string exteriorSceneName = DefaultExteriorSceneName;

    [Tooltip("Reloaded with exterior when exiting MR via phone booth or standard Enter VR.")]
    public string introGallerySceneName = DefaultIntroGallerySceneName;

    [Tooltip("Root object name for the payphone prefab instance.")]
    public string phoneBoothObjectName = DefaultPhoneBoothObjectName;

    [Header("MRUK room / anchors")]
    [Tooltip("World-space canvas (MRRoomInfoUI) listing scanned room and anchor counts.")]
    public bool showRoomAnchorInfoCanvas = true;

    [Header("VR → MR (phone booth)")]
    public bool rememberVrPoseOnPhoneBoothTravelToMr = true;

    [Header("VR → MR (standard Enter MR)")]
    public bool rememberVrPoseOnStandardEnterMr = true;

    [Header("MR → VR (phone booth)")]
    public bool restoreVrPoseOnPhoneBoothReturn = true;
    public bool refreshCameraOffsetAfterPhoneBoothReturn = true;
    public bool fallbackTravelStateIfRestoreFails = true;

    [Tooltip("After explosion/glass — wait before player locomotion (walk/teleport) returns.")]
    [Min(0f)]
    public float secondsBeforeResumeVrControlsOnPhoneBoothReturn;

    [Header("MR → VR (standard Enter VR)")]
    public bool restoreVrPoseOnStandardEnterVr = true;

    [Header("Phone booth step durations (seconds)")]
    [Tooltip("HandsetAudioCue — play audio, advance after this time (clip may continue).")]
    [Min(0f)]
    public float immersiveHandsetCueStepDurationSeconds = 2f;

    [Tooltip("SpaceshipEngine — play audio, advance after this time (overlap with handset OK).")]
    [Min(0f)]
    public float immersiveSpaceshipEngineStepDurationSeconds = 3.5f;

    [Tooltip("Optional hold after BeginJourneyVisuals (0 = immediate).")]
    [Min(0f)]
    public float immersiveBeginJourneyStepDurationSeconds;

    [Tooltip("Optional hold after FadeSphereIn (0 = immediate).")]
    [Min(0f)]
    public float immersiveFadeSphereStepDurationSeconds;

    [Tooltip("Optional hold after EndJourneyVisuals (0 = immediate).")]
    [Min(0f)]
    public float immersiveEndJourneyStepDurationSeconds;

    [Tooltip("MR→VR: delay after booth setup before arrival smoke/explosion.")]
    [Min(0f)]
    public float secondsBeforeArrivalExplosion = 0.05f;

    [Tooltip("Arrival explosion step — play audio, advance after this time (clip may continue).")]
    [Min(0f)]
    public float arrivalExplosionStepDurationSeconds = 1.5f;

    [Tooltip("After arrival explosion — wait before restoring normal glass and closing travel door.")]
    [Min(0f)]
    public float secondsAfterArrivalExplosionBeforeGlassAndDoor = 0.8f;

    [Header("Phone booth arrival VFX / audio")]
    [Range(0f, 1f)]
    public float arrivalExplosionVolume = 1f;

    [Header("Immersive travel sequence (handset journey)")]
    [Tooltip("Order of steps between grab and scene load callback. HandsetAudioCue must stay first.")]
    public List<MRPhoneBoothTransitionSequence.ImmersiveTravelStep> immersiveTravelSteps =
        MRPhoneBoothTransitionSequence.CopyDefaultImmersive();

    [Header("MR → VR return sequence (phone booth)")]
    [Tooltip("Order after travel callback. ReloadVrScenes must stay first; cache clip before finalize handset.")]
    public List<MRPhoneBoothTransitionSequence.MrToVrReturnStep> mrToVrReturnSteps =
        MRPhoneBoothTransitionSequence.CopyDefaultMrToVr();

    [Header("Debug")]
    public bool logWarningWhenInstanceMissing = true;

    static bool loggedMissingInstance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            ConfigManager.WriteConsoleWarning(
                "[MRRuntimeSettings] duplicate destroyed — keep one MRRuntimeSettings in FixedScene");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ConfigManager.WriteConsole("[MRRuntimeSettings] ready (DontDestroyOnLoad)");
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!MRPhoneBoothTransitionSequence.ValidateImmersive(immersiveTravelSteps, out string immersiveWarn))
            Debug.LogWarning($"[MRRuntimeSettings] immersive travel: {immersiveWarn}", this);

        if (!MRPhoneBoothTransitionSequence.ValidateMrToVr(mrToVrReturnSteps, out string mrToVrWarn))
            Debug.LogWarning($"[MRRuntimeSettings] MR→VR return: {mrToVrWarn}", this);

        if (Application.isPlaying && !showRoomAnchorInfoCanvas)
            MRRoomInfoUI.Instance?.Hide();
    }

    [ContextMenu("Reset phone booth transition sequences to defaults")]
    void ResetTransitionSequencesToDefaults()
    {
        immersiveTravelSteps = MRPhoneBoothTransitionSequence.CopyDefaultImmersive();
        mrToVrReturnSteps = MRPhoneBoothTransitionSequence.CopyDefaultMrToVr();
    }
#endif

    public static string FixedScene => Instance != null ? Instance.fixedSceneName : DefaultFixedSceneName;

    public static string ExteriorScene =>
        Instance != null ? Instance.exteriorSceneName : DefaultExteriorSceneName;

    public static string IntroGalleryScene =>
        Instance != null ? Instance.introGallerySceneName : DefaultIntroGallerySceneName;

    public static string PhoneBoothObject =>
        Instance != null ? Instance.phoneBoothObjectName : DefaultPhoneBoothObjectName;

    public static bool ShowRoomAnchorInfoCanvas =>
        Instance == null || Instance.showRoomAnchorInfoCanvas;

    public static bool RememberVrPoseOnPhoneBoothTravelToMr =>
        Instance == null || Instance.rememberVrPoseOnPhoneBoothTravelToMr;

    public static bool RememberVrPoseOnStandardEnterMr =>
        Instance == null || Instance.rememberVrPoseOnStandardEnterMr;

    public static bool RestoreVrPoseOnPhoneBoothReturn =>
        Instance == null || Instance.restoreVrPoseOnPhoneBoothReturn;

    public static bool RefreshCameraOffsetAfterPhoneBoothReturn =>
        Instance == null || Instance.refreshCameraOffsetAfterPhoneBoothReturn;

    public static bool FallbackTravelStateIfRestoreFails =>
        Instance == null || Instance.fallbackTravelStateIfRestoreFails;

    public static bool RestoreVrPoseOnStandardEnterVr =>
        Instance == null || Instance.restoreVrPoseOnStandardEnterVr;

    public static float ArrivalExplosionVolume =>
        Instance != null ? Mathf.Clamp01(Instance.arrivalExplosionVolume) : 1f;

    public const float DefaultImmersiveHandsetCueStepDurationSeconds = 2f;
    public const float DefaultImmersiveSpaceshipEngineStepDurationSeconds = 3.5f;
    public const float DefaultSecondsBeforeArrivalExplosion = 0.05f;
    public const float DefaultArrivalExplosionStepDurationSeconds = 1.5f;
    public const float DefaultSecondsAfterArrivalExplosionBeforeGlassAndDoor = 0.8f;
    public const float DefaultSecondsBeforeResumeVrControlsOnPhoneBoothReturn = 0f;

    public static float ImmersiveHandsetCueStepDurationSeconds =>
        Instance != null
            ? Mathf.Max(0f, Instance.immersiveHandsetCueStepDurationSeconds)
            : DefaultImmersiveHandsetCueStepDurationSeconds;

    public static float ImmersiveSpaceshipEngineStepDurationSeconds =>
        Instance != null
            ? Mathf.Max(0f, Instance.immersiveSpaceshipEngineStepDurationSeconds)
            : DefaultImmersiveSpaceshipEngineStepDurationSeconds;

    public static float ImmersiveBeginJourneyStepDurationSeconds =>
        Instance != null
            ? Mathf.Max(0f, Instance.immersiveBeginJourneyStepDurationSeconds)
            : 0f;

    public static float ImmersiveFadeSphereStepDurationSeconds =>
        Instance != null
            ? Mathf.Max(0f, Instance.immersiveFadeSphereStepDurationSeconds)
            : 0f;

    public static float ImmersiveEndJourneyStepDurationSeconds =>
        Instance != null
            ? Mathf.Max(0f, Instance.immersiveEndJourneyStepDurationSeconds)
            : 0f;

    public static float SecondsBeforeArrivalExplosion =>
        Instance != null
            ? Mathf.Max(0f, Instance.secondsBeforeArrivalExplosion)
            : DefaultSecondsBeforeArrivalExplosion;

    public static float ArrivalExplosionStepDurationSeconds =>
        Instance != null
            ? Mathf.Max(0f, Instance.arrivalExplosionStepDurationSeconds)
            : DefaultArrivalExplosionStepDurationSeconds;

    public static float SecondsAfterArrivalExplosionBeforeGlassAndDoor =>
        Instance != null
            ? Mathf.Max(0f, Instance.secondsAfterArrivalExplosionBeforeGlassAndDoor)
            : DefaultSecondsAfterArrivalExplosionBeforeGlassAndDoor;

    public static float SecondsBeforeResumeVrControlsOnPhoneBoothReturn =>
        Instance != null
            ? Mathf.Max(0f, Instance.secondsBeforeResumeVrControlsOnPhoneBoothReturn)
            : DefaultSecondsBeforeResumeVrControlsOnPhoneBoothReturn;

    public static IReadOnlyList<MRPhoneBoothTransitionSequence.ImmersiveTravelStep> ImmersiveTravelSteps =>
        MRPhoneBoothTransitionSequence.ResolveImmersive(
            Instance != null ? Instance.immersiveTravelSteps : null);

    public static IReadOnlyList<MRPhoneBoothTransitionSequence.MrToVrReturnStep> MrToVrReturnSteps =>
        MRPhoneBoothTransitionSequence.ResolveMrToVr(
            Instance != null ? Instance.mrToVrReturnSteps : null);

    public static string[] VrScenesToReloadOnExitMr()
    {
        if (Instance == null)
            return new[] { DefaultExteriorSceneName, DefaultIntroGallerySceneName };

        return new[]
        {
            Instance.exteriorSceneName,
            Instance.introGallerySceneName
        };
    }

    public static void LogMissingInstanceOnce(string caller)
    {
        if (Instance != null || loggedMissingInstance)
            return;

        loggedMissingInstance = true;
        ConfigManager.WriteConsoleWarning(
            $"[MRRuntimeSettings] no scene instance — {caller} using built-in defaults (add MRRuntimeSettings to FixedScene)");
    }
}
