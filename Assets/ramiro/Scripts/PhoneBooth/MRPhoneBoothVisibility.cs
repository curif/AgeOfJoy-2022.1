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
        SetVisible(!MRPhoneBoothSettings.Visible);
    }

    public static void SetVisible(bool visible)
    {
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
        if (!IsMrWorldActive())
            return MRPhoneBoothPortal.FindMrTravelerInstance(includeInactive: true);

        return MRPhoneBoothPortal.EnsureMrTravelerInstance();
    }

    public static void ApplySavedVisibility()
    {
        if (!IsMrWorldActive())
            return;

        MRPhoneBoothPortal traveler = EnsureMrInstance();
        if (traveler == null)
            return;

        traveler.SetVisible(MRPhoneBoothSettings.Visible, playHideEffect: false);
    }

    public static string GetStatusLabel()
    {
        MRPhoneBoothSettings.EnsureLoaded();

        if (!IsMrWorldActive())
            return MRPhoneBoothSettings.Visible ? "VISIBLE (pref)" : "HIDDEN (pref)";

        MRPhoneBoothPortal traveler = MRPhoneBoothPortal.FindMrTravelerInstance(includeInactive: true);
        if (traveler != null && traveler.IsVisible)
            return "VISIBLE";

        return "HIDDEN";
    }

    static bool IsMrWorldActive()
    {
        return MixedRealityManager.Instance != null
            && MixedRealityManager.Instance.IsMrEnvironmentActive();
    }
}
