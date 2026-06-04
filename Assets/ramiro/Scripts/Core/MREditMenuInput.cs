/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>Hold Y 3s in MR to open the ConfigurationCabinet layout panel.</summary>
public class MREditMenuInput : MonoBehaviour
{
    const string LogPrefix = "[MREditMenuInput]";

    [SerializeField] float holdDurationSeconds = 3f;
    [SerializeField] KeyCode editorToggleKey = KeyCode.M;

    float holdTimer;
    bool wasPressed;

    void Update()
    {
        if (MixedRealityManager.Instance == null)
        {
            ResetHold();
            return;
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(editorToggleKey))
        {
            MRConfigurationCabinetController.Instance?.ToggleEdit();
            ConfigManager.WriteConsole($"{LogPrefix} editor toggle ({editorToggleKey})");
            ResetHold();
            return;
        }
#endif

        ExperienceMode mode = MixedRealityManager.Instance.CurrentMode;
        if (mode != ExperienceMode.MR && mode != ExperienceMode.MR_EDIT)
        {
            ResetHold();
            return;
        }

        bool pressed = IsYButtonPressed();
        if (pressed)
        {
            holdTimer += Time.deltaTime;
            if (!wasPressed)
                ConfigManager.WriteConsole($"{LogPrefix} holding Y to toggle layout cabinet...");
            if (holdTimer >= holdDurationSeconds)
            {
                MRConfigurationCabinetController.Instance?.ToggleEdit();
                holdTimer = 0f;
                PulseHaptic();
            }
        }
        else
        {
            holdTimer = 0f;
        }

        wasPressed = pressed;
    }

    void ResetHold()
    {
        holdTimer = 0f;
        wasPressed = false;
    }

    static bool IsYButtonPressed()
    {
#if UNITY_EDITOR
        return Input.GetKey(KeyCode.JoystickButton3) || Input.GetKey(KeyCode.Y);
#else
        return OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.LTouch);
#endif
    }

    static void PulseHaptic()
    {
#if !UNITY_EDITOR
        try
        {
            OVRInput.SetControllerVibration(0.4f, 0.6f, OVRInput.Controller.LTouch);
        }
        catch
        {
            // optional feedback
        }
#endif
    }
}
