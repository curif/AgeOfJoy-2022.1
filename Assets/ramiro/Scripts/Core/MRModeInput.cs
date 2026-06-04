/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

public class MRModeInput : MonoBehaviour
{
    const string LogPrefix = "[MRModeInput]";

    [SerializeField] float holdDurationSeconds = 3f;
    [SerializeField] float toggleCooldownSeconds = 5f;

    float holdTimer;
    float cooldownTimer;
    bool wasPressed;

    void Update()
    {
        if (MixedRealityManager.Instance == null)
        {
            ResetHold();
            return;
        }

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.unscaledDeltaTime;

        bool pressed = IsToggleButtonPressed();
        if (pressed)
        {
            holdTimer += Time.unscaledDeltaTime;
            if (!wasPressed)
                ConfigManager.WriteConsole($"{LogPrefix} holding toggle (A / Menu / Enter)...");
            if (holdTimer >= holdDurationSeconds && cooldownTimer <= 0f)
            {
                holdTimer = 0f;
                cooldownTimer = toggleCooldownSeconds;
                ToggleMode();
            }
        }
        else
        {
            ResetHold();
        }

        wasPressed = pressed;
    }

    void ToggleMode()
    {
        var manager = MixedRealityManager.Instance;

        ExperienceMode mode = manager.CurrentMode;
        if (mode == ExperienceMode.MR || mode == ExperienceMode.MR_EDIT)
        {
            MRTransitionLog.Log("MRModeInput exit MR requested");
            ConfigManager.WriteConsole($"{LogPrefix} exit MR requested (mode={mode})");
            manager.EnterVR();
        }
        else
        {
            MRTransitionLog.Log("MRModeInput enter MR requested");
            ConfigManager.WriteConsole($"{LogPrefix} enter MR requested (mode={mode})");
            manager.EnterMR();
        }

        PulseHaptic();
    }

    void ResetHold()
    {
        holdTimer = 0f;
        wasPressed = false;
    }

    static bool IsToggleButtonPressed()
    {
#if UNITY_EDITOR
        return Input.GetKey(KeyCode.JoystickButton0)
            || Input.GetKey(KeyCode.Return)
            || Input.GetKey(KeyCode.KeypadEnter);
#else
        return OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch)
            || OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.LTouch)
            || OVRInput.Get(OVRInput.Button.Start, OVRInput.Controller.RTouch)
            || OVRInput.Get(OVRInput.Button.Start, OVRInput.Controller.LTouch);
#endif
    }

    static void PulseHaptic()
    {
#if !UNITY_EDITOR
        try
        {
            OVRInput.SetControllerVibration(1f, 0.8f, OVRInput.Controller.RTouch);
            OVRInput.SetControllerVibration(1f, 0.8f, OVRInput.Controller.LTouch);
        }
        catch
        {
            // optional feedback
        }
#endif
    }
}
