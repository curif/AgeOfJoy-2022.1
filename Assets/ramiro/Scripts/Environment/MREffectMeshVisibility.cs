/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>
/// Toggle MRUK EffectMesh layers from ConfigurationCabinet CRT (MESH menu).
/// </summary>
public static class MREffectMeshVisibility
{
    const string LogPrefix = "[MREffectMeshVisibility]";

    public const string AnchorMeshLabel = "EffectMesh";
    public const string GlobalMeshLabel = "EffectMeshGlobalMesh";
    public const string ScanDebugColorsLabel = "SCAN COLORS";

    public static void ToggleAnchorMesh()
    {
        SetAnchorMeshEnabled(!MREffectMeshSettings.AnchorMeshEnabled);
    }

    public static void ToggleGlobalMesh()
    {
        SetGlobalMeshEnabled(!MREffectMeshSettings.GlobalMeshEnabled);
    }

    public static void SetAnchorMeshEnabled(bool enabled)
    {
        MREffectMeshSettings.SetAnchorMeshEnabled(enabled);
        ApplyInMrWorld();
    }

    public static void SetGlobalMeshEnabled(bool enabled)
    {
        MREffectMeshSettings.SetGlobalMeshEnabled(enabled);
        ApplyInMrWorld();
    }

    public static void SetScanDebugColorsEnabled(bool enabled)
    {
        MREffectMeshSettings.SetScanDebugColorsEnabled(enabled);
        ApplyColorTintInMrWorld();
    }

    public static void ApplySavedSettings()
    {
        if (!IsMrWorldActive())
            return;

        MREffectMeshController controller = GetController();
        controller?.ApplySettings();
    }

    public static string GetAnchorStatusLabel()
    {
        return GetStatusLabel(MREffectMeshSettings.AnchorMeshEnabled, IsAnchorLive());
    }

    public static string GetGlobalStatusLabel()
    {
        return GetStatusLabel(MREffectMeshSettings.GlobalMeshEnabled, IsGlobalLive());
    }

    public static string GetScanDebugColorsStatusLabel()
    {
        MREffectMeshSettings.EnsureLoaded();
        if (!MREffectMeshSettings.ScanDebugColorsEnabled)
            return "OFF";

        if (!IsMrWorldActive())
            return "ON (pref)";

        return IsAnyMeshLive() ? "ON per surface" : "ON (wait mesh)";
    }

    static void ApplyInMrWorld()
    {
        if (!IsMrWorldActive())
            return;

        MREffectMeshController controller = GetController();
        if (controller == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} toggle saved but controller missing");
            return;
        }

        controller.ApplySettings();
        ConfigManager.WriteConsole(
            $"{LogPrefix} anchor={MREffectMeshSettings.AnchorMeshEnabled} global={MREffectMeshSettings.GlobalMeshEnabled}");
    }

    static void ApplyColorTintInMrWorld()
    {
        if (!IsMrWorldActive())
            return;

        MREffectMeshController controller = GetController();
        if (controller == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} scan colors saved but controller missing");
            return;
        }

        controller.ApplyColorTint();
        ConfigManager.WriteConsole(
            $"{LogPrefix} scanColors={MREffectMeshSettings.ScanDebugColorsEnabled}");
    }

    static string GetStatusLabel(bool prefEnabled, bool live)
    {
        MREffectMeshSettings.EnsureLoaded();

        if (!IsMrWorldActive())
            return prefEnabled ? "ON (pref)" : "OFF (pref)";

        if (!prefEnabled)
            return "OFF";

        return live ? "ON" : "WAIT";
    }

    static bool IsAnchorLive()
    {
        MREffectMeshController controller = GetController();
        return controller != null && controller.IsAnchorMeshSpawned;
    }

    static bool IsGlobalLive()
    {
        MREffectMeshController controller = GetController();
        return controller != null && controller.IsGlobalMeshSpawned;
    }

    static bool IsAnyMeshLive()
    {
        MREffectMeshController controller = GetController();
        return controller != null && controller.IsSpawned;
    }

    static MREffectMeshController GetController()
    {
        if (MixedRealityManager.Instance == null)
            return null;

        return MixedRealityManager.Instance.GetComponent<MREffectMeshController>();
    }

    static bool IsMrWorldActive()
    {
        return MixedRealityManager.Instance != null
            && MixedRealityManager.Instance.IsMrEnvironmentActive();
    }
}
