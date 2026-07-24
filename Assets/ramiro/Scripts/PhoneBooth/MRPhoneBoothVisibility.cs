/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>
/// Show/hide the MR phone booth from ConfigurationCabinet CRT — toggles SetActive only (instance is never destroyed).
/// </summary>
public static class MRPhoneBoothVisibility
{
    const string LogPrefix = "[MRPhoneBoothVisibility]";

    public static void Toggle()
    {
        if (IsPhoneBoothSuppressedForQuickTravel())
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} toggle ignored — Quick Travel session");
            return;
        }

        SetVisible(!MRPhoneBoothSettings.Visible);
    }

    public static void SetVisible(bool visible)
    {
        if (IsPhoneBoothSuppressedForQuickTravel())
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} SetVisible ignored — Quick Travel session");
            return;
        }

        MRPhoneBoothSettings.SetVisible(visible);

        if (!IsMrWorldActive())
            return;

        MRPhoneBoothPortal traveler = EnsureMrInstance();
        if (traveler == null)
            return;

        if (!visible)
            MRPhoneBoothSettings.SaveMrPose(traveler.transform.position, traveler.transform.rotation);

        traveler.SetVisible(visible);
        ConfigManager.WriteConsole($"{LogPrefix} booth {(visible ? "shown" : "hidden")} (SetActive)");
    }

    /// <summary>Creates the MR booth once if missing; reused for every show/hide.</summary>
    public static MRPhoneBoothPortal EnsureMrInstance()
    {
        if (IsPhoneBoothSuppressedForQuickTravel())
            return null;

        if (!IsMrWorldActive())
            return MRPhoneBoothPortal.FindMrTravelerInstance(includeInactive: true);

        return MRPhoneBoothPortal.EnsureMrTravelerInstance();
    }

    public static void ApplySavedVisibility()
    {
        if (IsPhoneBoothSuppressedForQuickTravel())
            return;

        if (!IsMrWorldActive())
            return;

        MixedRealityManager manager = MixedRealityManager.Instance;
        if (manager != null && manager.TransitionInProgress)
            return;

        MRPhoneBoothPortal traveler = EnsureMrInstance();
        if (traveler == null)
            return;

        traveler.SetVisible(MRPhoneBoothSettings.Visible, playHideEffect: false);
    }

    /// <summary>
    /// After phone-booth VR→MR arrival finishes: hide by default (free floor space),
    /// unless the user disabled auto-hide in the CRT PHONE BOOTH menu.
    /// </summary>
    public static void ApplyAfterPhoneBoothTravelArrival()
    {
        if (IsPhoneBoothSuppressedForQuickTravel())
            return;

        MRPhoneBoothSettings.EnsureLoaded();
        if (MRPhoneBoothSettings.AutoHideAfterTravel)
        {
            SetVisible(false);
            ConfigManager.WriteConsole($"{LogPrefix} auto-hide after phone-booth travel");
            return;
        }

        SetVisible(true);
        ConfigManager.WriteConsole($"{LogPrefix} kept visible after travel (auto-hide off)");
    }

    public static string GetStatusLabel()
    {
        if (IsPhoneBoothSuppressedForQuickTravel())
            return "DISABLED (quick travel)";

        MRPhoneBoothSettings.EnsureLoaded();

        if (!IsMrWorldActive())
            return MRPhoneBoothSettings.Visible ? "VISIBLE (pref)" : "HIDDEN (pref)";

        MRPhoneBoothPortal traveler = MRPhoneBoothPortal.FindMrTravelerInstance(includeInactive: true);
        if (traveler != null && traveler.IsVisible)
            return "VISIBLE";

        return "HIDDEN";
    }

    public static bool IsPhoneBoothSuppressedForQuickTravel()
    {
        MixedRealityManager manager = MixedRealityManager.Instance;
        return manager != null && manager.SuppressPhoneBoothForQuickTravelSession;
    }

    static bool IsMrWorldActive()
    {
        return MixedRealityManager.Instance != null
            && MixedRealityManager.Instance.IsMrEnvironmentActive();
    }
}
