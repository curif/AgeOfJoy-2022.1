/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>
/// Toggle / tune MR global directional sun only (not user-placed light prefabs).
/// </summary>
public static class MRAutoLightingVisibility
{
    const string LogPrefix = "[MRAutoLightingVisibility]";

    public const string AutoLightLabel = "Global Light";

    public static void SetEnabled(bool enabled)
    {
        MRAutoLightingSettings.SetEnabled(enabled);
        ApplyInMrWorld();
    }

    public static void AdjustIntensity(int direction)
    {
        float intensity = MRAutoLightingSettings.AdjustIntensity(direction);
        ApplyIntensityInMrWorld(intensity);
    }

    public static void ApplySavedSettings()
    {
        if (!IsMrWorldActive())
            return;

        MRMrEnvironmentLighting lighting = GetLighting();
        if (lighting == null)
            return;

        if (MRAutoLightingSettings.Enabled)
            lighting.Spawn(GetMrSpaceOrigin());
        else
            lighting.Despawn(restoreAmbient: false);
    }

    public static string GetStatusLabel()
    {
        bool prefEnabled = MRAutoLightingSettings.Enabled;
        if (!IsMrWorldActive())
            return prefEnabled ? "ON (pref)" : "OFF (pref)";

        MRMrEnvironmentLighting lighting = GetLighting();
        bool live = lighting != null && lighting.IsSpawned;
        bool keySuspended = lighting != null && lighting.IsKeySuspendedForGameplay;

        if (prefEnabled && live && keySuspended)
            return "ON (game)";
        if (prefEnabled && live)
            return "ON";
        if (!prefEnabled && !live)
            return "OFF";
        if (prefEnabled)
            return "ON (pending)";
        return "OFF (pending)";
    }

    static void ApplyInMrWorld()
    {
        if (!IsMrWorldActive())
            return;

        MRMrEnvironmentLighting lighting = GetLighting();
        if (lighting == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} toggle saved but lighting component missing");
            return;
        }

        if (MRAutoLightingSettings.Enabled)
            lighting.Spawn(GetMrSpaceOrigin());
        else
            lighting.Despawn(restoreAmbient: false);

        ConfigManager.WriteConsole(
            $"{LogPrefix} globalSun={MRAutoLightingSettings.Enabled} intensity={MRAutoLightingSettings.Intensity:F1} (placed lights unchanged)");
    }

    static void ApplyIntensityInMrWorld(float intensity)
    {
        if (!IsMrWorldActive())
            return;

        MRMrEnvironmentLighting lighting = GetLighting();
        if (lighting == null || !lighting.IsSpawned)
            return;

        lighting.ApplyIntensity(intensity);
    }

    static bool IsMrWorldActive()
    {
        MixedRealityManager manager = MixedRealityManager.Instance;
        return manager != null && manager.CurrentMode != ExperienceMode.VR;
    }

    static Transform GetMrSpaceOrigin() => MixedRealityManager.Instance?.MRSpaceOrigin;

    static MRMrEnvironmentLighting GetLighting()
    {
        MixedRealityManager manager = MixedRealityManager.Instance;
        return manager != null ? manager.GetComponent<MRMrEnvironmentLighting>() : null;
    }
}
