/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>
/// Phone-booth travel: black HMD when the camera leaves the ZoneBlackout box on PF_Payphone.
/// Uses camera culling (render nothing) + solid black clear.
/// </summary>
[DisallowMultipleComponent]
public class MRPhoneBoothTravelHeadFade : MonoBehaviour
{
    const string LogPrefix = "[MRPhoneBoothTravelHeadFade]";

    [SerializeField] float fadeSpeed = 28f;
    [Tooltip("Fade ramps from 0 to 1 across this distance outside ZoneBlackout.")]
    [SerializeField] float exteriorFadeRangeMeters = 0.08f;

    BoxCollider fadeZone;
    bool monitoring;
    bool forcedTravelBlackout;
    bool headBlackoutLogged;
    float currentFade;
    Camera xrCamera;
    int savedCullingMask;
    CameraClearFlags savedClearFlags;
    Color savedBackgroundColor;
    bool cameraStateSaved;

    static MRPhoneBoothTravelHeadFade activeTravelFade;

    public bool IsMonitoring => monitoring;
    public bool IsForcedTravelBlackout => forcedTravelBlackout;
    public float ExteriorFadeRangeMeters => exteriorFadeRangeMeters;

    public static bool IsTravelBlackoutActive =>
        activeTravelFade != null
        && (activeTravelFade.forcedTravelBlackout
            || (activeTravelFade.monitoring && activeTravelFade.currentFade > 0.01f));

    public static void EndActiveTravelFade()
    {
        activeTravelFade?.EndMonitoring();
    }

    public static void ReassertActiveTravelBlackout()
    {
        activeTravelFade?.ReassertBlackoutVisuals();
    }

    public void BeginMonitoring(BoxCollider zoneBlackout)
    {
        fadeZone = zoneBlackout;
        if (fadeZone == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} BeginMonitoring skipped — ZoneBlackout null");
            return;
        }

        if (!EnsureCameraBindings())
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} BeginMonitoring skipped — eye camera not found");
            return;
        }

        monitoring = true;
        activeTravelFade = this;
        currentFade = 0f;
        headBlackoutLogged = false;
        ApplyCameraBlackout(0f);
        ConfigManager.WriteConsole($"{LogPrefix} head fade ON — ZoneBlackout (camera cull)");
        MRTransitionLog.LogStep("MRPhoneBoothTravelHeadFade", "BeginMonitoring");
    }

    /// <summary>Editor / manual test — full blackout regardless of head position.</summary>
    public void EngageForcedTravelBlackout()
    {
        if (forcedTravelBlackout)
        {
            ReassertBlackoutVisuals();
            return;
        }

        if (!EnsureCameraBindings())
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} EngageForcedTravelBlackout skipped — eye camera not found");
            return;
        }

        forcedTravelBlackout = true;
        activeTravelFade = this;
        ReassertBlackoutVisuals();
        MixedRealityManager.Instance?.HoldPhoneBoothTravelBlackout();
#if UNITY_EDITOR
        MREditorMrSimulator.Instance?.ClearPassthroughBackdrop();
#endif
        ConfigManager.WriteConsole("[blackout] Iniciado");
        MRTransitionLog.LogStep("MRPhoneBoothTravelHeadFade", "EngageForcedTravelBlackout");
    }

    public void ReassertBlackoutVisuals()
    {
        if (forcedTravelBlackout)
        {
            ApplyCameraBlackout(1f);
            return;
        }

        if (!monitoring || currentFade <= 0.01f)
            return;

        ApplyCameraBlackout(currentFade);
    }

    public void EndMonitoring()
    {
        bool wasActive = monitoring || forcedTravelBlackout;
        if (!wasActive)
            return;

        if (headBlackoutLogged || forcedTravelBlackout)
            ConfigManager.WriteConsole("[blackout] Finalizado");

        monitoring = false;
        forcedTravelBlackout = false;
        headBlackoutLogged = false;
        fadeZone = null;
        currentFade = 0f;

        if (activeTravelFade == this)
            activeTravelFade = null;

        ApplyCameraBlackout(0f);
        RestoreCameraState();
        cameraStateSaved = false;
        ConfigManager.WriteConsole($"{LogPrefix} head fade monitoring OFF");
        MRTransitionLog.LogStep("MRPhoneBoothTravelHeadFade", "EndMonitoring");
    }

    public float EvaluateFadeAtEyePosition(Vector3 eyeWorldPosition)
    {
        if (forcedTravelBlackout)
            return 1f;

        if (fadeZone == null)
            return 0f;

        if (IsInsideFadeZone(eyeWorldPosition))
            return 0f;

        float distanceOutside = DistanceOutsideFadeZone(eyeWorldPosition);
        if (exteriorFadeRangeMeters <= 0.001f)
            return 1f;

        float t = Mathf.Clamp01(distanceOutside / exteriorFadeRangeMeters);
        float oneMinus = 1f - t;
        return 1f - oneMinus * oneMinus * oneMinus;
    }

    void Update()
    {
        if (forcedTravelBlackout)
        {
            ApplyCameraBlackout(1f);
            return;
        }

        if (!monitoring)
            return;

        float targetFade = EvaluateFadeAtEyePosition(ResolveEyePosition());
        currentFade = Mathf.MoveTowards(currentFade, targetFade, fadeSpeed * Time.unscaledDeltaTime);
        ApplyCameraBlackout(currentFade);
        UpdateHeadBlackoutLogs();
    }

    void UpdateHeadBlackoutLogs()
    {
        if (currentFade >= 0.99f && !headBlackoutLogged)
        {
            headBlackoutLogged = true;
            ConfigManager.WriteConsole("[blackout] Iniciado");
        }
        else if (currentFade <= 0.01f && headBlackoutLogged)
        {
            headBlackoutLogged = false;
            ConfigManager.WriteConsole("[blackout] Finalizado");
        }
    }

    void OnDisable()
    {
        EndMonitoring();
    }

    bool IsInsideFadeZone(Vector3 worldPoint)
    {
        if (fadeZone == null)
            return false;

        Vector3 local = fadeZone.transform.InverseTransformPoint(worldPoint);
        Vector3 center = fadeZone.center;
        Vector3 halfExtents = fadeZone.size * 0.5f;

        Vector3 delta = local - center;
        return Mathf.Abs(delta.x) <= halfExtents.x
            && Mathf.Abs(delta.y) <= halfExtents.y
            && Mathf.Abs(delta.z) <= halfExtents.z;
    }

    float DistanceOutsideFadeZone(Vector3 worldPoint)
    {
        if (fadeZone == null || IsInsideFadeZone(worldPoint))
            return 0f;

        Vector3 local = fadeZone.transform.InverseTransformPoint(worldPoint);
        Vector3 center = fadeZone.center;
        Vector3 halfExtents = fadeZone.size * 0.5f;

        Vector3 delta = local - center;
        Vector3 outside = new Vector3(
            Mathf.Max(0f, Mathf.Abs(delta.x) - halfExtents.x),
            Mathf.Max(0f, Mathf.Abs(delta.y) - halfExtents.y),
            Mathf.Max(0f, Mathf.Abs(delta.z) - halfExtents.z));
        return outside.magnitude;
    }

    static Vector3 ResolveEyePosition()
    {
        Camera eyeCamera = ResolveEyeCamera();
        return eyeCamera != null ? eyeCamera.transform.position : Vector3.zero;
    }

    bool EnsureCameraBindings()
    {
        Camera resolved = ResolveEyeCamera();
        if (resolved == null)
            return false;

        if (xrCamera != resolved || !cameraStateSaved)
        {
            xrCamera = resolved;
            savedCullingMask = xrCamera.cullingMask;
            savedClearFlags = xrCamera.clearFlags;
            savedBackgroundColor = xrCamera.backgroundColor;
            cameraStateSaved = true;
        }

        return true;
    }

    static Camera ResolveEyeCamera()
    {
        var player = Object.FindObjectOfType<PlayerController>();
        if (player != null && player.xrorigin != null)
            return player.xrorigin.Camera;

        return Camera.main;
    }

    void ApplyCameraBlackout(float fade)
    {
        if (!EnsureCameraBindings())
            return;

        if (fade > 0.01f)
        {
            xrCamera.cullingMask = 0;
            xrCamera.clearFlags = CameraClearFlags.SolidColor;
            xrCamera.backgroundColor = Color.black;
            return;
        }

        RestoreCameraState();
    }

    void RestoreCameraState()
    {
        if (xrCamera == null || !cameraStateSaved)
            return;

        xrCamera.cullingMask = savedCullingMask;
        xrCamera.clearFlags = savedClearFlags;
        xrCamera.backgroundColor = savedBackgroundColor;
    }
}
