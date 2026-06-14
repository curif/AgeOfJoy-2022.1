/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>PlayerPrefs for MR auto ceiling + fill lights spawned on EnterMR.</summary>
public static class MRAutoLightingSettings
{
    const string LogPrefix = "[MRAutoLightingSettings]";
    const string EnabledKey = "MR.AutoLighting.Enabled";

    static bool loaded;
    static bool enabled = true;

    public static bool Enabled
    {
        get
        {
            EnsureLoaded();
            return enabled;
        }
    }

    public static void EnsureLoaded()
    {
        if (loaded)
            return;

        enabled = PlayerPrefs.GetInt(EnabledKey, 1) != 0;
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
}
