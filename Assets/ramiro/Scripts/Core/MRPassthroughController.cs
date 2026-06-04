/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using AOJ.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

public class MRPassthroughController : MonoBehaviour
{
    const string LogPrefix = "[MRPassthroughController]";
    const float SystemInitTimeoutSeconds = 10f;
    const float LayerReadyTimeoutSeconds = 5f;
    const string FadeSphereName = "SM_FadeSphere";
    const string FadeInTrigger = "FadeInTrigger";
    const string FadeOutTrigger = "FadeOutTrigger";

    OVRPassthroughLayer passthroughLayer;
    Camera xrCamera;
    Animator fadeSphereAnimator;
    readonly System.Collections.Generic.List<Renderer> disabledFadeRenderers = new System.Collections.Generic.List<Renderer>();

    CameraClearFlags savedClearFlags;
    Color savedBackgroundColor;
    OVROverlay.OverlayType savedOverlayType;
    bool savedFog;
    bool savedEyeFovPremultipliedAlpha;
    bool initialized;
    bool passthroughSystemReady;
    bool insightPassthroughEnabledBeforeMr;

    public bool IsPassthroughEnabled => passthroughLayer != null && passthroughLayer.enabled;

    /// <summary>True when MR passthrough rendering is active (layer or EventManager flag).</summary>
    public bool IsPassthroughActive =>
        IsPassthroughEnabled
        || (EventManager.Instance != null && EventManager.Instance.IsPassthrough);

    public bool PassthroughSystemReady => passthroughSystemReady;

    /// <summary>True when OVR/XR is active enough to query or enable passthrough (Quest build or Editor + Link).</summary>
    public static bool IsPassthroughRuntimeAvailable()
    {
        if (OVRManager.instance == null)
            return false;

#if UNITY_EDITOR
        return XRSettings.enabled && XRSettings.isDeviceActive;
#else
        return true;
#endif
    }

    public void Initialize()
    {
        if (initialized)
            return;

        xrCamera = ResolveXRCamera();
        if (xrCamera == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} XR camera not found.");
            return;
        }

        passthroughLayer = xrCamera.GetComponent<OVRPassthroughLayer>();
        if (passthroughLayer == null)
            passthroughLayer = xrCamera.gameObject.AddComponent<OVRPassthroughLayer>();

        var fadeSphere = GameObject.Find(FadeSphereName);
        if (fadeSphere != null)
            fadeSphereAnimator = fadeSphere.GetComponent<Animator>();

        savedClearFlags = xrCamera.clearFlags;
        savedBackgroundColor = xrCamera.backgroundColor;
        savedOverlayType = passthroughLayer.overlayType;
        initialized = true;

        if (OVRManager.instance != null)
            insightPassthroughEnabledBeforeMr = OVRManager.instance.isInsightPassthroughEnabled;

        ConfigManager.WriteConsole($"{LogPrefix} initialized on {xrCamera.name}, layerType={savedOverlayType}");
    }

    /// <summary>Full-screen black while VR scenes unload — call before UnloadVrScenes, then EnablePassthroughWhenReady.</summary>
    public void BeginTransitionBlackout(bool triggerFadeInAnimator = true, bool restoreFadeSphere = true)
    {
        if (!initialized)
            Initialize();

        RebindXRCamera(createPassthroughLayerIfMissing: false);
        if (restoreFadeSphere)
            RestoreFadeSphereVisuals();
        RefreshFadeSphereAnimator();

        if (triggerFadeInAnimator && fadeSphereAnimator != null)
            fadeSphereAnimator.SetTrigger(FadeInTrigger);

        if (xrCamera != null)
        {
            xrCamera.clearFlags = CameraClearFlags.SolidColor;
            xrCamera.backgroundColor = Color.black;
        }

        EnsureInsightPassthroughEnabled();
        MRTransitionLog.LogStep("MRPassthroughController", "BeginTransitionBlackout");
        ConfigManager.WriteConsole($"{LogPrefix} transition blackout ON");
    }

    public IEnumerator EnablePassthroughWhenReady()
    {
        if (!initialized)
            Initialize();

        // Layer was Destroy()'d on MR→VR exit; recreate before second VR→MR.
        RebindXRCamera(createPassthroughLayerIfMissing: true);
        if (passthroughLayer == null || xrCamera == null)
        {
            MRTransitionLog.LogError("EnablePassthroughWhenReady aborted — no XR camera or passthrough layer");
            yield break;
        }

        if (!IsPassthroughRuntimeAvailable())
        {
#if UNITY_EDITOR
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} Editor: XR/OVR not initialized — skipping passthrough (MR layout test mode)");
            passthroughSystemReady = true;
            MREditorMrSimulator.Instance?.ApplyPassthroughBackdrop(xrCamera);
#else
            ConfigManager.WriteConsoleError($"{LogPrefix} passthrough unavailable — OVR/XR not ready");
            passthroughSystemReady = false;
#endif
            yield break;
        }

        LogPassthroughDiagnostics("before enable");

        yield return WaitForPassthroughSystemReady();
        if (!passthroughSystemReady)
            yield break;

        ApplyPassthroughRendering();
        yield return WaitUntilPassthroughLayerVisible();

        LogPassthroughDiagnostics("after enable");
    }

    public void DisablePassthrough(bool playFadeOut = true)
    {
        MRTransitionLog.LogStep("MRPassthroughController.DisablePassthrough", $"playFadeOut={playFadeOut}");
        RebindXRCamera(createPassthroughLayerIfMissing: false);

        if (passthroughLayer != null)
        {
            passthroughLayer.enabled = false;
            passthroughLayer.hidden = true;
            passthroughLayer.textureOpacity = 0f;
            Destroy(passthroughLayer);
            passthroughLayer = null;
        }

        if (xrCamera != null)
        {
            xrCamera.clearFlags = CameraClearFlags.Skybox;
            xrCamera.backgroundColor = savedBackgroundColor;

            Transform psMotes = xrCamera.transform.Find("PS_Motes");
            if (psMotes != null)
                psMotes.gameObject.SetActive(true);
        }

        RenderSettings.fog = savedFog;

        if (OVRManager.instance != null)
        {
            OVRManager.eyeFovPremultipliedAlphaModeEnabled = savedEyeFovPremultipliedAlpha;
            OVRManager.instance.isInsightPassthroughEnabled = false;
        }

        RestoreFadeSphereForVr();

        if (playFadeOut && fadeSphereAnimator != null)
            fadeSphereAnimator.SetTrigger(FadeOutTrigger);

        MREditorMrSimulator.Instance?.ClearPassthroughBackdrop();

        if (EventManager.Instance != null)
            EventManager.Instance.IsPassthrough = false;

        LogPassthroughDiagnostics("after disable");
        ConfigManager.WriteConsole($"{LogPrefix} passthrough OFF");
        MRTransitionLog.LogPassthrough("DisablePassthrough-done", this);
    }

    /// <summary>Re-resolve camera/layer after additive scene reload, then force passthrough off.</summary>
    public void RebindCameraAndDisablePassthrough(bool playFadeOut = true)
    {
        MRTransitionLog.LogStep("MRPassthroughController.RebindCameraAndDisablePassthrough", $"playFadeOut={playFadeOut}");
        RebindXRCamera(createPassthroughLayerIfMissing: false);
        DisablePassthrough(playFadeOut);
    }

    void RebindXRCamera(bool createPassthroughLayerIfMissing = true)
    {
        Camera resolved = ResolveXRCamera();
        if (resolved == null)
            return;

        if (resolved != xrCamera)
        {
            xrCamera = resolved;
            passthroughLayer = xrCamera.GetComponent<OVRPassthroughLayer>();
            MRTransitionLog.Log($"RebindXRCamera -> {xrCamera.name}");
            ConfigManager.WriteConsole($"{LogPrefix} rebound XR camera to {xrCamera.name}");
        }

        if (passthroughLayer == null && xrCamera != null)
            passthroughLayer = xrCamera.GetComponent<OVRPassthroughLayer>();
        if (createPassthroughLayerIfMissing && passthroughLayer == null && xrCamera != null)
            passthroughLayer = xrCamera.gameObject.AddComponent<OVRPassthroughLayer>();
    }

    void ApplyPassthroughRendering()
    {
        if (!IsPassthroughRuntimeAvailable())
            return;

        if (passthroughLayer == null && xrCamera != null)
            passthroughLayer = xrCamera.gameObject.AddComponent<OVRPassthroughLayer>();

        EnsureInsightPassthroughEnabled();
        ClearFadeSphereForPassthrough();

        if (OVRManager.instance != null)
            insightPassthroughEnabledBeforeMr = OVRManager.instance.isInsightPassthroughEnabled;

        savedOverlayType = passthroughLayer.overlayType;
        savedEyeFovPremultipliedAlpha = OVRManager.eyeFovPremultipliedAlphaModeEnabled;

        // Underlay = passthrough atrás; mãos/controladores VR renderizam por cima.
        // Overlay (opacity=1) tapava as mãos 3D quando o passthrough passou a funcionar.
        OVRManager.eyeFovPremultipliedAlphaModeEnabled = false;
        passthroughLayer.enabled = false;
        passthroughLayer.hidden = false;
        passthroughLayer.overlayType = OVROverlay.OverlayType.Underlay;
        passthroughLayer.textureOpacity = 1f;
        passthroughLayer.enabled = true;

        xrCamera.clearFlags = CameraClearFlags.SolidColor;
        xrCamera.backgroundColor = Color.clear;

        savedFog = RenderSettings.fog;
        RenderSettings.fog = false;

        Transform psMotes = xrCamera.transform.Find("PS_Motes");
        if (psMotes != null)
            psMotes.gameObject.SetActive(false);

        if (EventManager.Instance != null)
            EventManager.Instance.IsPassthrough = true;

        ConfigManager.WriteConsole($"{LogPrefix} passthrough ON (underlay, hands on top)");
    }

    void RefreshFadeSphereAnimator()
    {
        var fadeSphere = GameObject.Find(FadeSphereName);
        if (fadeSphere == null)
            return;

        fadeSphereAnimator = fadeSphere.GetComponent<Animator>();
    }

    IEnumerator WaitForPassthroughSystemReady()
    {
        passthroughSystemReady = false;

        if (!IsPassthroughRuntimeAvailable())
            yield break;

        EnsureInsightPassthroughEnabled();

        float elapsed = 0f;
        while (!OVRManager.IsInsightPassthroughInitialized()
               && !OVRManager.HasInsightPassthroughInitFailed()
               && elapsed < SystemInitTimeoutSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (OVRManager.HasInsightPassthroughInitFailed())
        {
            ConfigManager.WriteConsoleError(
                $"{LogPrefix} Insight Passthrough init FAILED — verifique OVRProjectConfig (Passthrough Support) e Meta > Tools > Update Android Manifest");
            yield break;
        }

        if (!OVRManager.IsInsightPassthroughInitialized())
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} Insight Passthrough init timeout ({SystemInitTimeoutSeconds}s)");
            yield break;
        }

        passthroughSystemReady = true;
        ConfigManager.WriteConsole($"{LogPrefix} Insight Passthrough system ready");
    }

    IEnumerator WaitUntilPassthroughLayerVisible()
    {
        if (passthroughLayer == null || !passthroughLayer.enabled)
            yield break;

        bool resumed = false;
        UnityAction<OVRPassthroughLayer> onResumed = _ => resumed = true;
        passthroughLayer.passthroughLayerResumed.AddListener(onResumed);

        float elapsed = 0f;
        while (!resumed && elapsed < LayerReadyTimeoutSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        passthroughLayer.passthroughLayerResumed.RemoveListener(onResumed);

        if (resumed)
            ConfigManager.WriteConsole($"{LogPrefix} passthrough layer resumed (visible on HMD)");
        else
            ConfigManager.WriteConsoleWarning($"{LogPrefix} passthrough layer resume timeout ({LayerReadyTimeoutSeconds}s)");
    }

    /// <summary>
    /// Hide SM_FadeSphere during MR passthrough. Do not use FadeOutTrigger — that animation
    /// re-enables the renderer at _FadeAmount=1 (full black) for ~2s and covers passthrough.
    /// </summary>
    void ClearFadeSphereForPassthrough()
    {
        disabledFadeRenderers.Clear();
        var fadeSphere = GameObject.Find(FadeSphereName);
        if (fadeSphere == null)
            return;

        fadeSphereAnimator = fadeSphere.GetComponent<Animator>();
        if (fadeSphereAnimator != null)
            fadeSphereAnimator.enabled = false;

        foreach (Renderer renderer in fadeSphere.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null)
                continue;

            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (material != null && material.HasProperty("_FadeAmount"))
                    material.SetFloat("_FadeAmount", 0f);
            }

            disabledFadeRenderers.Add(renderer);
            renderer.enabled = false;
        }

        if (disabledFadeRenderers.Count > 0)
            ConfigManager.WriteConsole($"{LogPrefix} cleared fade sphere for passthrough ({disabledFadeRenderers.Count} renderer(s))");
    }

    void RestoreFadeSphereForVr()
    {
        var fadeSphere = GameObject.Find(FadeSphereName);
        if (fadeSphere != null)
        {
            fadeSphereAnimator = fadeSphere.GetComponent<Animator>();
            if (fadeSphereAnimator != null)
                fadeSphereAnimator.enabled = true;
        }

        RestoreFadeSphereVisuals();
    }

    void SuppressFadeSphereVisuals()
    {
        ClearFadeSphereForPassthrough();
    }

    void RestoreFadeSphereVisuals()
    {
        foreach (Renderer renderer in disabledFadeRenderers)
        {
            if (renderer != null)
                renderer.enabled = true;
        }
        disabledFadeRenderers.Clear();
    }

    void LogPassthroughDiagnostics(string stage)
    {
        if (!IsPassthroughRuntimeAvailable())
        {
            ConfigManager.WriteConsole($"{LogPrefix} diag [{stage}] passthrough runtime unavailable (XR not initialized)");
            return;
        }

        bool supported = OVRManager.IsInsightPassthroughSupported();
        bool initialized = OVRManager.IsInsightPassthroughInitialized();
        bool pending = OVRManager.IsInsightPassthroughInitPending();
        bool failed = OVRManager.HasInsightPassthroughInitFailed();
        bool managerEnabled = OVRManager.instance != null && OVRManager.instance.isInsightPassthroughEnabled;
        bool layerEnabled = passthroughLayer != null && passthroughLayer.enabled;

        ConfigManager.WriteConsole(
            $"{LogPrefix} diag [{stage}] supported={supported} init={initialized} pending={pending} failed={failed} manager={managerEnabled} layer={layerEnabled}");
    }

    static void EnsureInsightPassthroughEnabled()
    {
        if (!IsPassthroughRuntimeAvailable())
            return;

        if (OVRManager.instance == null)
            return;

        if (!OVRManager.instance.isInsightPassthroughEnabled)
            OVRManager.instance.isInsightPassthroughEnabled = true;
    }

    /// <summary>After additive VR scenes unload, rebind the HMD camera and restore passthrough (MainCamera tag can change).</summary>
    public void RefreshPassthroughAfterSceneUnload()
    {
        if (!initialized)
            Initialize();

        RebindXRCamera(createPassthroughLayerIfMissing: true);
        if (xrCamera == null || passthroughLayer == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} RefreshPassthroughAfterSceneUnload skipped — no XR camera/layer");
            return;
        }

        if (!IsPassthroughRuntimeAvailable())
            return;

        if (!passthroughSystemReady && OVRManager.IsInsightPassthroughInitialized())
            passthroughSystemReady = true;

        if (!passthroughSystemReady)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} RefreshPassthroughAfterSceneUnload skipped — passthrough not ready");
            return;
        }

        ApplyPassthroughRendering();
        MRTransitionLog.LogStep("MRPassthroughController", "RefreshPassthroughAfterSceneUnload");
        ConfigManager.WriteConsole($"{LogPrefix} passthrough refreshed after scene unload on {xrCamera.name}");
    }

    static Camera ResolveXRCamera()
    {
        var origin = Object.FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
        if (origin != null && origin.Camera != null)
            return origin.Camera;

        if (Camera.main != null)
            return Camera.main;

        var tagged = GameObject.FindGameObjectWithTag("MainCamera");
        return tagged != null ? tagged.GetComponent<Camera>() : null;
    }
}
