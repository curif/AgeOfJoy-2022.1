/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>
/// Hold Y 3s in MR to open/close the ConfigurationCabinet layout panel.
/// Double-tap Y (two presses within 0.5s) to summon the cabinet to the player's current position.
/// </summary>
public class MREditMenuInput : MonoBehaviour
{
    const string LogPrefix = "[MREditMenuInput]";

    [SerializeField] float holdDurationSeconds = 3f;
    [SerializeField] float doubleTapWindowSeconds = 0.5f;
    [SerializeField] KeyCode editorToggleKey = KeyCode.M;
    [SerializeField] KeyCode editorSummonKey = KeyCode.N;

    float holdTimer;
    float doubleTapTimer;
    bool wasPressed;
    bool waitingForSecondTap;

    void Update()
    {
        if (MixedRealityManager.Instance == null)
        {
            ResetHold();
            return;
        }

#if UNITY_EDITOR
        if (MREditorInput.WasPressed(editorToggleKey))
        {
            MRConfigurationCabinetController.Instance?.ToggleEdit();
            ConfigManager.WriteConsole($"{LogPrefix} editor toggle ({editorToggleKey})");
            ResetHold();
            return;
        }
        if (MREditorInput.WasPressed(editorSummonKey))
        {
            MRConfigurationCabinetController.Instance?.SummonToPlayer();
            ConfigManager.WriteConsole($"{LogPrefix} editor summon ({editorSummonKey})");
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

        if (waitingForSecondTap)
        {
            doubleTapTimer -= Time.deltaTime;
            if (doubleTapTimer <= 0f)
                waitingForSecondTap = false;
        }

        bool pressed = IsYButtonPressed();
        bool justPressed = pressed && !wasPressed;

        if (justPressed)
        {
            if (waitingForSecondTap)
            {
                waitingForSecondTap = false;
                holdTimer = 0f;
                MRConfigurationCabinetController.Instance?.SummonToPlayer();
                ConfigManager.WriteConsole($"{LogPrefix} double-tap Y — summoning cabinet to player");
                PulseHaptic();
            }
            else
            {
                waitingForSecondTap = true;
                doubleTapTimer = doubleTapWindowSeconds;
            }
        }

        if (pressed)
        {
            holdTimer += Time.deltaTime;
            if (!wasPressed)
                ConfigManager.WriteConsole($"{LogPrefix} holding Y to toggle layout cabinet...");
            if (holdTimer >= holdDurationSeconds)
            {
                waitingForSecondTap = false;
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
        waitingForSecondTap = false;
        doubleTapTimer = 0f;
    }

    static bool IsYButtonPressed()
    {
#if UNITY_EDITOR
        return MREditorInput.YButtonHeld();
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
