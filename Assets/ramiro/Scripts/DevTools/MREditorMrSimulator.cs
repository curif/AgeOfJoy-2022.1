/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Editor-only passthrough-like camera backdrop.
/// </summary>
public class MREditorMrSimulator : MonoBehaviour
{
    const string LogPrefix = "[MREditorMrSimulator]";

#if UNITY_EDITOR
    [SerializeField] Color passthroughBackdropColor = new Color(0.38f, 0.4f, 0.43f, 1f);
    [SerializeField] bool logControlsOnStart = true;

    Camera simulatedCamera;
    CameraClearFlags savedClearFlags;
    Color savedBackgroundColor;
    bool savedFog;
    bool backdropActive;
#endif

    public static MREditorMrSimulator Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    void Start()
    {
#if UNITY_EDITOR
        if (logControlsOnStart && Application.isEditor)
        {
            ConfigManager.WriteConsole($"{LogPrefix} FixedScene editor — P = viagem cabine (VR→MR se em VR, MR→VR se em MR)");
            ConfigManager.WriteConsole($"{LogPrefix} Shift+P = forçar VR→MR (precisa modo VR + PF_Payphone na exterior)");
            ConfigManager.WriteConsole($"{LogPrefix} B = toggle blackout imediato (camera cull teste)");
            ConfigManager.WriteConsole($"{LogPrefix} VR genérico — Enter 3s | MR cabine: ver acima");
        }
#endif
    }

    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isEditor)
            return;

        if (EditorBlackoutToggleKeyPressed())
        {
            TryToggleEditorTravelBlackout();
            return;
        }

        if (EditorVrToMrTravelKeyPressed())
        {
            TrySimulateVrToMrTravel();
            return;
        }

        if (EditorPhoneBoothTravelKeyPressed())
            TrySimulatePhoneBoothTravel();
#endif
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ApplyPassthroughBackdrop(Camera camera)
    {
#if UNITY_EDITOR
        if (!Application.isEditor || camera == null || backdropActive)
            return;

        if (MRPhoneBoothTravelHeadFade.IsTravelBlackoutActive)
            return;

        simulatedCamera = camera;
        savedClearFlags = camera.clearFlags;
        savedBackgroundColor = camera.backgroundColor;
        savedFog = RenderSettings.fog;

        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = passthroughBackdropColor;
        RenderSettings.fog = false;
        backdropActive = true;

        ConfigManager.WriteConsole($"{LogPrefix} passthrough backdrop (editor simulation)");
#endif
    }

    public void ClearPassthroughBackdrop()
    {
#if UNITY_EDITOR
        if (!backdropActive || simulatedCamera == null)
            return;

        simulatedCamera.clearFlags = savedClearFlags;
        simulatedCamera.backgroundColor = savedBackgroundColor;
        RenderSettings.fog = savedFog;
        simulatedCamera = null;
        backdropActive = false;
#endif
    }

#if UNITY_EDITOR
    void TrySimulatePhoneBoothTravel()
    {
        MixedRealityManager manager = MixedRealityManager.Instance;
        if (manager == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} P — MixedRealityManager missing (play from FixedScene?)");
            return;
        }

        if (manager.TransitionInProgress)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} P — transition already running");
            return;
        }

        if (!manager.CanToggleMode())
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} P — scene transition busy");
            return;
        }

        MRPhoneBoothPortal portal = ResolvePhoneBoothPortalForEditor(manager);
        if (portal == null)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} P — no PF_Payphone (need IntroGalleryExterior or MR traveler booth)");
            return;
        }

        portal.EditorMarkReadyForSimulatedTravel();
        SnapPlayerInsideBoothForEditor(portal);

        PayphoneHandsetGrab grab = PayphoneHandsetGrab.FindOnPortal(portal);
        if (grab != null)
            grab.SimulateEditorGrabForTravel();
        else
            portal.NotifyHandsetGrabbedForTravel();

        if (!portal.TravelInProgress)
        {
            portal.EditorRetryStartTravelFromHandset();
            if (!portal.TravelInProgress)
            {
                ConfigManager.WriteConsoleWarning(
                    $"{LogPrefix} P — travel did not start (check console for [MRPhoneBoothPortal] / player rig)");
                return;
            }
        }

        ConfigManager.WriteConsole(
            $"{LogPrefix} P — immersive phone booth travel (mode={manager.CurrentMode}, portal={portal.name})");
    }

    void TrySimulateVrToMrTravel()
    {
        MixedRealityManager manager = MixedRealityManager.Instance;
        if (manager == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} Shift+P — MixedRealityManager missing (play FixedScene?)");
            return;
        }

        if (manager.CurrentMode != ExperienceMode.VR)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} Shift+P — precisa modo VR (Enter VR ou desliga auto-boot MR no MRRuntimeSettings)");
            return;
        }

        if (manager.TransitionInProgress || !manager.CanToggleMode())
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} Shift+P — transição ocupada");
            return;
        }

        MRPhoneBoothPortal portal = MRPhoneBoothPortal.FindSceneBoothPortal();
        if (portal == null)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} Shift+P — PF_Payphone não encontrado (espera IntroGalleryExterior carregar?)");
            return;
        }

        portal.EditorMarkReadyForSimulatedTravel();
        SnapPlayerInsideBoothForEditor(portal);
        portal.BeginTravelToMR();

        if (!portal.TravelInProgress)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} Shift+P — viagem VR→MR não arrancou (ver consola [MRPhoneBoothPortal])");
            return;
        }

        ConfigManager.WriteConsole($"{LogPrefix} Shift+P — viagem imersiva VR→MR (blackout no BeginJourneyVisuals)");
    }

    void TryToggleEditorTravelBlackout()
    {
        MRPhoneBoothTravelHeadFade active = Object.FindObjectOfType<MRPhoneBoothTravelHeadFade>();
        if (active != null && MRPhoneBoothTravelHeadFade.IsTravelBlackoutActive)
        {
            MRPhoneBoothPortal.EndTravelBlackoutEverywhere();
            ConfigManager.WriteConsole($"{LogPrefix} B — blackout OFF");
            return;
        }

        MRPhoneBoothPortal portal = ResolvePhoneBoothPortalForEditor(MixedRealityManager.Instance)
            ?? MRPhoneBoothPortal.FindSceneBoothPortal();
        if (portal == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} B — nenhuma cabine encontrada");
            return;
        }

        var headFade = portal.GetComponent<MRPhoneBoothTravelHeadFade>();
        if (headFade == null)
            headFade = portal.gameObject.AddComponent<MRPhoneBoothTravelHeadFade>();

        headFade.EngageForcedTravelBlackout();
        ConfigManager.WriteConsole($"{LogPrefix} B — blackout ON (camera cull teste)");
    }

    static bool EditorVrToMrTravelKeyPressed() =>
        MREditorInput.IsAnyHeld(KeyCode.LeftShift, KeyCode.RightShift)
        && MREditorInput.WasPressed(KeyCode.P);

    static bool EditorBlackoutToggleKeyPressed() =>
        MREditorInput.WasPressed(KeyCode.B);

    static bool EditorPhoneBoothTravelKeyPressed() =>
        !MREditorInput.IsAnyHeld(KeyCode.LeftShift, KeyCode.RightShift)
        && MREditorInput.WasPressed(KeyCode.P);

    static MRPhoneBoothPortal ResolvePhoneBoothPortalForEditor(MixedRealityManager manager)
    {
        if (manager.CurrentMode == ExperienceMode.VR)
            return MRPhoneBoothPortal.FindSceneBoothPortal();

        if (!manager.IsMrEnvironmentActive())
            return null;

        MRPhoneBoothPortal traveler = MRPhoneBoothPortal.FindMrTravelerInstance(includeInactive: true);
        if (traveler != null)
            return traveler;

        return MRPhoneBoothPortal.EnsureMrTravelerInstance();
    }

    static void SnapPlayerInsideBoothForEditor(MRPhoneBoothPortal portal)
    {
        Transform player = FindPlayerTransformForEditor();
        if (player == null || portal == null)
            return;

        Vector3 localPos = new Vector3(0f, 0.05f, -0.15f);
        player.SetPositionAndRotation(
            portal.transform.TransformPoint(localPos),
            portal.transform.rotation);
    }

    static Transform FindPlayerTransformForEditor()
    {
        var pc = Object.FindObjectOfType<PlayerController>();
        if (pc != null && pc.PlayerControllerGameObject != null)
            return pc.PlayerControllerGameObject.transform;

        var xrOrigin = Object.FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
        return xrOrigin != null ? xrOrigin.transform : null;
    }
#endif

}
