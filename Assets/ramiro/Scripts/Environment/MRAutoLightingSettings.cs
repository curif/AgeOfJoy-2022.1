/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>PlayerPrefs for MR global directional sun (CONFIG → GLOBAL LIGHT).</summary>
public static class MRAutoLightingSettings
{
    const string LogPrefix = "[MRAutoLightingSettings]";
    const string EnabledKey = "MR.AutoLighting.Enabled";
    const string IntensityKey = "MR.AutoLighting.Intensity";

    public const float DefaultIntensity = 1.2f;
    public const float IntensityStep = 0.1f;
    public const float MinIntensity = 0.1f;
    public const float MaxIntensity = 5f;

    static bool loaded;
    static bool enabled = true;
    static float intensity = DefaultIntensity;

    public static bool Enabled
    {
        get
        {
            EnsureLoaded();
            return enabled;
        }
    }

    public static float Intensity
    {
        get
        {
            EnsureLoaded();
            return intensity;
        }
    }

    public static void EnsureLoaded()
    {
        if (loaded)
            return;

        enabled = PlayerPrefs.GetInt(EnabledKey, 1) != 0;
        intensity = Mathf.Clamp(
            PlayerPrefs.GetFloat(IntensityKey, DefaultIntensity),
            MinIntensity,
            MaxIntensity);
        loaded = true;
    }

    public static void SetEnabled(bool value)
    {
        EnsureLoaded();
        enabled = value;
        PlayerPrefs.SetInt(EnabledKey, value ? 1 : 0);
        PlayerPrefs.Save();
        ConfigManager.WriteConsole($"{LogPrefix} enabled={value}");
    }

    public static float AdjustIntensity(int direction)
    {
        EnsureLoaded();
        if (direction == 0)
            return intensity;

        intensity = SnapStep(intensity + direction * IntensityStep, MinIntensity, MaxIntensity);
        PlayerPrefs.SetFloat(IntensityKey, intensity);
        PlayerPrefs.Save();
        ConfigManager.WriteConsole($"{LogPrefix} intensity={intensity:F1}");
        return intensity;
    }

    public static void SetIntensity(float value)
    {
        EnsureLoaded();
        intensity = SnapStep(value, MinIntensity, MaxIntensity);
        PlayerPrefs.SetFloat(IntensityKey, intensity);
        PlayerPrefs.Save();
        ConfigManager.WriteConsole($"{LogPrefix} intensity={intensity:F1}");
    }

    static float SnapStep(float value, float min, float max)
    {
        float snapped = Mathf.Round(value / IntensityStep) * IntensityStep;
        return Mathf.Clamp(snapped, min, max);
    }
}
