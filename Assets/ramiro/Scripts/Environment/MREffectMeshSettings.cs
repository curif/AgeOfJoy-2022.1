/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>
/// PlayerPrefs for MRUK EffectMesh layers (anchor mesh + global mesh) and scan debug colors.
/// </summary>
public static class MREffectMeshSettings
{
    const string LogPrefix = "[MREffectMeshSettings]";
    const string AnchorEnabledKey = "MR.EffectMesh.AnchorEnabled";
    const string GlobalEnabledKey = "MR.EffectMesh.GlobalEnabled";
    const string ScanDebugColorsEnabledKey = "MR.EffectMesh.ScanDebugColorsEnabled";
    const string LegacyColorTintEnabledKey = "MR.EffectMesh.ColorTintEnabled";

    static bool loaded;
    static bool anchorMeshEnabled = true;
    static bool globalMeshEnabled = false;
    static bool scanDebugColorsEnabled;

    public static bool AnchorMeshEnabled
    {
        get
        {
            EnsureLoaded();
            return anchorMeshEnabled;
        }
    }

    public static bool GlobalMeshEnabled
    {
        get
        {
            EnsureLoaded();
            return globalMeshEnabled;
        }
    }

    /// <summary>Per-label scan overlay (floor green, wall orange, table yellow, …).</summary>
    public static bool ScanDebugColorsEnabled
    {
        get
        {
            EnsureLoaded();
            return scanDebugColorsEnabled;
        }
    }

    public static void EnsureLoaded()
    {
        if (loaded)
            return;

        anchorMeshEnabled = PlayerPrefs.GetInt(AnchorEnabledKey, 1) != 0;
        globalMeshEnabled = PlayerPrefs.GetInt(GlobalEnabledKey, 0) != 0;
        if (PlayerPrefs.HasKey(ScanDebugColorsEnabledKey))
            scanDebugColorsEnabled = PlayerPrefs.GetInt(ScanDebugColorsEnabledKey, 0) != 0;
        else
            scanDebugColorsEnabled = PlayerPrefs.GetInt(LegacyColorTintEnabledKey, 0) != 0;
        loaded = true;
    }

    public static void SetAnchorMeshEnabled(bool value)
    {
        EnsureLoaded();
        anchorMeshEnabled = value;
        PlayerPrefs.SetInt(AnchorEnabledKey, value ? 1 : 0);
        PlayerPrefs.Save();
        ConfigManager.WriteConsole($"{LogPrefix} EffectMesh={(value ? "on" : "off")}");
    }

    public static void SetGlobalMeshEnabled(bool value)
    {
        EnsureLoaded();
        globalMeshEnabled = value;
        PlayerPrefs.SetInt(GlobalEnabledKey, value ? 1 : 0);
        PlayerPrefs.Save();
        ConfigManager.WriteConsole($"{LogPrefix} EffectMeshGlobalMesh={(value ? "on" : "off")}");
    }

    public static void SetScanDebugColorsEnabled(bool value)
    {
        EnsureLoaded();
        scanDebugColorsEnabled = value;
        PlayerPrefs.SetInt(ScanDebugColorsEnabledKey, value ? 1 : 0);
        PlayerPrefs.Save();
        ConfigManager.WriteConsole($"{LogPrefix} scan debug colors={(value ? "on" : "off")}");
    }
}
