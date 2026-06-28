/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>
/// Global MR tuning for floor game cabinets (scale multiplier and floor Y offset).
/// Neutral baseline is 1.0 for both; floor offset applied as (value - 1.0) meters.
/// </summary>
public static class MRAdjustmentsSettings
{
    const string LogPrefix = "[MRAdjustmentsSettings]";
    const string CabinetScaleKey = "MR.Adjustments.CabinetScale";
    const string FloorCabinetPositionKey = "MR.Adjustments.FloorCabinetPosition";

    public const float DefaultValue = 1f;
    public const float Step = 0.01f;
    public const float MinScale = 0.1f;
    public const float MaxScale = 5f;
    public const float MinFloorPosition = -2f;
    public const float MaxFloorPosition = 3f;

    static bool loaded;
    static float cabinetScale = DefaultValue;
    static float floorCabinetPosition = DefaultValue;

    public static float CabinetScale
    {
        get
        {
            EnsureLoaded();
            return cabinetScale;
        }
    }

    public static float FloorCabinetPosition
    {
        get
        {
            EnsureLoaded();
            return floorCabinetPosition;
        }
    }

    /// <summary>Extra Y meters applied to floor cabinets; 1.0 = no offset.</summary>
    public static float FloorCabinetYOffset => FloorCabinetPosition - DefaultValue;

    public static void EnsureLoaded()
    {
        if (loaded)
            return;

        cabinetScale = PlayerPrefs.GetFloat(CabinetScaleKey, DefaultValue);
        floorCabinetPosition = PlayerPrefs.GetFloat(FloorCabinetPositionKey, DefaultValue);
        loaded = true;
    }

    public static float AdjustCabinetScale(int direction)
    {
        EnsureLoaded();
        if (direction == 0)
            return cabinetScale;

        cabinetScale = SnapStep(
            cabinetScale + direction * Step,
            MinScale,
            MaxScale);

        Save();
        ConfigManager.WriteConsole($"{LogPrefix} cabinet scale={cabinetScale:F2}");
        return cabinetScale;
    }

    public static float AdjustFloorCabinetPosition(int direction)
    {
        EnsureLoaded();
        if (direction == 0)
            return floorCabinetPosition;

        floorCabinetPosition = SnapStep(
            floorCabinetPosition + direction * Step,
            MinFloorPosition,
            MaxFloorPosition);

        Save();
        ConfigManager.WriteConsole($"{LogPrefix} floor position={floorCabinetPosition:F2}");
        return floorCabinetPosition;
    }

    static float SnapStep(float value, float min, float max)
    {
        value = Mathf.Round(value / Step) * Step;
        return Mathf.Clamp(value, min, max);
    }

    static void Save()
    {
        PlayerPrefs.SetFloat(CabinetScaleKey, cabinetScale);
        PlayerPrefs.SetFloat(FloorCabinetPositionKey, floorCabinetPosition);
        PlayerPrefs.Save();
    }
}
