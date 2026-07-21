/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Quick Travel VR ↔ MR: grab the left-wrist coin with the right hand, keep holding it,
/// and hold the right index trigger for <see cref="holdDurationSeconds"/>
/// (Editor: hold Space). Blocked while a Libretro game is running.
/// </summary>
public class MRQuickTravelInput : MonoBehaviour
{
    const string LogPrefix = "[MRQuickTravelInput]";

    [SerializeField] float holdDurationSeconds = 2f;
    [SerializeField] float triggerAxisThreshold = 0.55f;
    [SerializeField] float chordCooldownSeconds = 1.5f;
    [SerializeField] float diagnosticIntervalSeconds = 2f;
    [SerializeField] KeyCode editorTravelKey = KeyCode.Space;

    float holdTimer;
    bool holdLogged;
    float cooldownUntil;
    float nextDiagnosticLogTime;

    ChangeControls cachedChangeControls;
    ActionBasedController cachedRightController;
    XRBaseInteractor cachedRightInteractor;

    void Update()
    {
        MixedRealityManager manager = MixedRealityManager.Instance;
        if (manager == null)
        {
            ResetHold();
            return;
        }

        if (Time.unscaledTime < cooldownUntil)
        {
            ResetHold();
            return;
        }

        if (!manager.CanToggleMode())
        {
            ResetHold();
            return;
        }

        bool coinInRightHand = IsCoinHeldInRightHand();
        bool rightTrigger = IsRightIndexTriggerHeld();
        bool chordActive = IsTravelChordHeld(coinInRightHand, rightTrigger);

        if (!chordActive)
        {
            MaybeLogPartialChord(coinInRightHand, rightTrigger);
            ResetHold();
            return;
        }

        holdTimer += Time.unscaledDeltaTime;
        if (!holdLogged)
        {
            holdLogged = true;
            ConfigManager.WriteConsole(
                $"{LogPrefix} holding quick travel ({holdDurationSeconds:0.##}s)...");
        }

        if (holdTimer < holdDurationSeconds)
            return;

        ResetHold();
        TryTravel(manager);
    }

    bool IsTravelChordHeld(bool coinInRightHand, bool rightTrigger)
    {
#if UNITY_EDITOR
        if (MREditorInput.IsHeld(editorTravelKey))
            return true;
#endif
        return coinInRightHand && rightTrigger;
    }

    void MaybeLogPartialChord(bool coinInRightHand, bool rightTrigger)
    {
        if (Time.unscaledTime < nextDiagnosticLogTime)
            return;

        if (!coinInRightHand && !rightTrigger)
            return;

        nextDiagnosticLogTime = Time.unscaledTime + diagnosticIntervalSeconds;
        float right = ReadRightTriggerValue();
        ConfigManager.WriteConsole(
            $"{LogPrefix} chord incomplete coinRight={coinInRightHand} triggerR={rightTrigger} " +
            $"R={right:0.00} threshold={triggerAxisThreshold:0.00}");
    }

    void TryTravel(MixedRealityManager manager)
    {
        if (IsAnyGameRunning())
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} blocked — game running (mame={LibretroMameCore.GameLoaded} flycast={LibretroFlycastCore.GameLoaded})");
            cooldownUntil = Time.unscaledTime + chordCooldownSeconds;
            return;
        }

        ExperienceMode mode = manager.CurrentMode;
        bool inMr = mode == ExperienceMode.MR || mode == ExperienceMode.MR_EDIT;

        if (!inMr && MRRuntimeSettings.IsMrEntryBlocked("MRQuickTravelInput"))
            return;

#if UNITY_EDITOR
        bool editorShortcut = MREditorInput.IsHeld(editorTravelKey);
#else
        bool editorShortcut = false;
#endif

        if (inMr)
        {
            ConfigManager.WriteConsole(
                $"{LogPrefix} MR→VR quick travel{(editorShortcut ? " (editor)" : "")}");
            manager.EnterVRFromQuickTravel();
        }
        else
        {
            ConfigManager.WriteConsole(
                $"{LogPrefix} VR→MR quick travel{(editorShortcut ? " (editor)" : "")}");
            manager.EnterMRFromQuickTravel();
        }

        cooldownUntil = Time.unscaledTime + chordCooldownSeconds;
        PulseHaptic();
    }

    void ResetHold()
    {
        holdTimer = 0f;
        holdLogged = false;
    }

    static bool IsAnyGameRunning() =>
        LibretroMameCore.GameLoaded || LibretroFlycastCore.GameLoaded;

    bool IsCoinHeldInRightHand()
    {
        EnsureControllerCache();

        UsdCoin[] coins = FindObjectsOfType<UsdCoin>(true);
        for (int i = 0; i < coins.Length; i++)
        {
            UsdCoin coin = coins[i];
            if (coin == null || !coin.isActiveAndEnabled)
                continue;

            XRGrabInteractable grab = coin.GetComponent<XRGrabInteractable>();
            if (grab == null || !grab.isSelected)
                continue;

            // Prefer explicit right-hand interactor match (coin lives on left wrist).
            if (cachedRightInteractor != null && grab.interactorsSelecting.Contains(cachedRightInteractor))
                return true;

            // Fallback: any selecting interactor under the right-hand controller hierarchy.
            for (int s = 0; s < grab.interactorsSelecting.Count; s++)
            {
                IXRSelectInteractor interactor = grab.interactorsSelecting[s];
                if (interactor is Component component && IsUnderRightHand(component.transform))
                    return true;
            }

            // Last resort if IsGrabbed but interactor hierarchy unknown — still require right trigger.
            if (coin.IsGrabbed && cachedRightInteractor == null)
                return true;
        }

        return false;
    }

    bool IsUnderRightHand(Transform t)
    {
        if (t == null || cachedChangeControls == null || cachedChangeControls.rightHandXRControl == null)
            return false;

        Transform rightRoot = cachedChangeControls.rightHandXRControl.transform;
        return t == rightRoot || t.IsChildOf(rightRoot);
    }

    bool IsRightIndexTriggerHeld() =>
        ReadRightTriggerValue() >= triggerAxisThreshold;

    float ReadRightTriggerValue()
    {
        float right = 0f;

        EnsureControllerCache();
        right = Mathf.Max(right, ReadActivateValue(cachedRightController));
        right = Mathf.Max(right, ReadXrNodeTrigger(XRNode.RightHand));

#if !UNITY_EDITOR
        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
            right = Mathf.Max(right, 1f);

        right = Mathf.Max(
            right,
            OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch));
#endif
        return right;
    }

    void EnsureControllerCache()
    {
        if (cachedRightController != null && cachedRightInteractor != null)
            return;

        if (cachedChangeControls == null)
            cachedChangeControls = FindObjectOfType<ChangeControls>();

        if (cachedChangeControls == null || cachedChangeControls.rightHandXRControl == null)
            return;

        Transform rightRoot = cachedChangeControls.rightHandXRControl.transform;
        if (cachedRightController == null)
            cachedRightController = rightRoot.GetComponent<ActionBasedController>();

        if (cachedRightInteractor == null)
            cachedRightInteractor = rightRoot.GetComponentInChildren<XRBaseInteractor>(true);
    }

    static float ReadActivateValue(ActionBasedController controller)
    {
        if (controller == null)
            return 0f;

        try
        {
            var valueAction = controller.activateActionValue.action;
            if (valueAction != null)
                return Mathf.Clamp01(valueAction.ReadValue<float>());

            var buttonAction = controller.activateAction.action;
            if (buttonAction != null && buttonAction.IsPressed())
                return 1f;
        }
        catch
        {
            // Input action may be unbound during scene transitions
        }

        return 0f;
    }

    static float ReadXrNodeTrigger(XRNode node)
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(node);
        if (!device.isValid)
            return 0f;

        if (device.TryGetFeatureValue(CommonUsages.trigger, out float axis))
            return Mathf.Clamp01(axis);

        if (device.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed) && pressed)
            return 1f;

        return 0f;
    }

    static void PulseHaptic()
    {
#if !UNITY_EDITOR
        try
        {
            OVRInput.SetControllerVibration(0.35f, 0.55f, OVRInput.Controller.RTouch);
        }
        catch
        {
            // optional feedback
        }
#endif
    }
}
