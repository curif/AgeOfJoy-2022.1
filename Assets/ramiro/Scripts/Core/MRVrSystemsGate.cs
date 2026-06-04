/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using AOJ.Managers;
using UnityEngine;

/// <summary>
/// Stops VR arcade systems (LibRetro, deployed cabinets) before MR.
/// MR cabinets are placed later via UI + MRLayoutRegistry (see MIXED_REALITY_DESIGN.md).
/// </summary>
public static class MRVrSystemsGate
{
    const string LogPrefix = "[MRVrSystemsGate]";

    public static void SuspendForMR()
    {
        ConfigManager.WriteConsole($"{LogPrefix} SuspendForMR");
        StopActiveLibretroGames();
        TeardownDeployedVrCabinets();
        DisableVrCabinetControllers();
        EnsureHandModelsVisible();
        SuspendPlayerLocomotion();
        ResetLegacyPassthroughState();
    }

    public static void ResumeForVR()
    {
        MRTransitionLog.LogStep("MRVrSystemsGate.ResumeForVR");
        ConfigManager.WriteConsole($"{LogPrefix} ResumeForVR (VR scenes reload separately)");
        ResumeVrSystemsExceptLocomotion();
        ResumePlayerLocomotionForVr();
        MRTransitionLog.Log("MRVrSystemsGate.ResumeForVR done");
    }

    /// <summary>VR cabinets / hands — locomotion stays suspended until <see cref="ResumePlayerLocomotionForVr"/>.</summary>
    public static void ResumeVrSystemsExceptLocomotion()
    {
        StopActiveLibretroGames();
        EnableVrCabinetControllers();
        EnsureHandModelsVisible();
        ResetLegacyPassthroughState();
    }

    public static void ResumePlayerLocomotionForVr()
    {
        MRTransitionLog.LogStep("MRVrSystemsGate.ResumePlayerLocomotionForVr");
        ResumePlayerLocomotion();
    }

    /// <summary>MR→VR phone booth: silence running cores and attract loops before/during immersive travel.</summary>
    public static void SilenceAllCabinetScreensForPhoneBoothTravelToVr()
    {
        MRTransitionLog.LogStep("MRVrSystemsGate", "SilenceAllCabinetScreensForPhoneBoothTravelToVr");
        ConfigManager.WriteConsole($"{LogPrefix} silence cabinet screens for phone booth MR→VR travel");
        StopActiveLibretroGames();
        SuspendAttractOnAllCabinetScreens();
        MRLayoutRegistry registry = MRLayoutRegistry.Instance;
        registry?.StopAllMrLibretroGames();
    }

    public static void StopActiveLibretroGames()
    {
        var screens = Object.FindObjectsOfType<LibretroScreenController>(true);
        MRTransitionLog.Log($"StopActiveLibretroGames screens={screens.Length}");
        bool ended = false;
        foreach (LibretroScreenController screen in screens)
        {
            if (screen == null)
                continue;
            if (LibretroMameCore.isRunning(screen.ScreenName, screen.GameFile))
            {
                MRTransitionLog.Log($"StopActiveLibretroGames End on {screen.name}");
                ConfigManager.WriteConsole($"{LogPrefix} ending LibRetro on {screen.name}");
                LibretroMameCore.End(screen.ScreenName, screen.GameFile);
                ended = true;
            }
        }

        if (LibretroMameCore.GameLoaded && !ended)
        {
            MRTransitionLog.LogWarning("StopActiveLibretroGames force end — screen destroyed before End()");
            LibretroMameCore.ForceEndActiveGame();
        }
    }

    /// <summary>After VR scenes reload — attract loops on gallery cabinets may have started.</summary>
    public static void SilenceCabinetScreensAfterVrSceneReload()
    {
        MRTransitionLog.LogStep("MRVrSystemsGate", "SilenceCabinetScreensAfterVrSceneReload");
        StopActiveLibretroGames();
        SuspendAttractOnAllCabinetScreens();
    }

    static void SuspendAttractOnAllCabinetScreens()
    {
        var screens = Object.FindObjectsOfType<LibretroScreenController>(true);
        foreach (LibretroScreenController screen in screens)
        {
            if (screen == null)
                continue;
            screen.SuspendAttractAndPlaybackForTransition();
        }
    }

    static void TeardownDeployedVrCabinets()
    {
        var replacements = Object.FindObjectsOfType<CabinetReplace>(true);
        foreach (CabinetReplace replace in replacements)
        {
            if (replace == null)
                continue;

            if (replace.game != null && replace.game.Room == MixedRealityManager.MrRoomName)
                continue;

            ConfigManager.WriteConsole($"{LogPrefix} removing deployed cabinet {replace.name}");
            if (replace.outOfOrderCabinet != null)
                replace.outOfOrderCabinet.SetActive(true);

            Object.Destroy(replace.gameObject);
        }
    }

    static void DisableVrCabinetControllers()
    {
        var controllers = Object.FindObjectsOfType<CabinetsController>(true);
        foreach (CabinetsController controller in controllers)
        {
            if (controller == null)
                continue;
            controller.StopAllCoroutines();
            controller.enabled = false;
            ConfigManager.WriteConsole($"{LogPrefix} disabled CabinetsController on {controller.gameObject.name} (room={controller.Room})");
        }
    }

    static void EnableVrCabinetControllers()
    {
        var controllers = Object.FindObjectsOfType<CabinetsController>(true);
        MRTransitionLog.Log($"EnableVrCabinetControllers count={controllers.Length}");
        foreach (CabinetsController controller in controllers)
        {
            if (controller == null || controller.enabled)
                continue;
            controller.enabled = true;
            ConfigManager.WriteConsole($"{LogPrefix} re-enabled CabinetsController on {controller.gameObject.name} (room={controller.Room})");
        }
    }

    static void ResetLegacyPassthroughState()
    {
        if (EventManager.Instance != null)
            EventManager.Instance.IsPassthrough = false;
    }

    static ChangeControls FindChangeControls()
    {
        return Object.FindObjectOfType<ChangeControls>(true);
    }

    static void EnsureHandModelsVisible()
    {
        ChangeControls changeControls = FindChangeControls();
        if (changeControls == null)
            return;

        changeControls.PlayerMode(false);
        ConfigManager.WriteConsole($"{LogPrefix} hand models restored for MR");
    }

    static void SuspendPlayerLocomotion()
    {
        ChangeControls changeControls = FindChangeControls();
        if (changeControls == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} ChangeControls not found — MR locomotion not suspended");
            return;
        }

        changeControls.SetMrLocomotionSuspended(true);
        ConfigManager.WriteConsole($"{LogPrefix} player locomotion suspended for MR");
    }

    static void ResumePlayerLocomotion()
    {
        ChangeControls changeControls = FindChangeControls();
        if (changeControls == null)
            return;

        changeControls.SetMrLocomotionSuspended(false);
        ConfigManager.WriteConsole($"{LogPrefix} player locomotion restored for VR");
    }
}
