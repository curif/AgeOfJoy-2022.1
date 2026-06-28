/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>
/// MR-side phone booth visibility and pose (VR booth pose comes from the scene).
/// </summary>
public static class MRPhoneBoothSettings
{
    const string LogPrefix = "[MRPhoneBoothSettings]";
    const string VisibleKey = "MR.PhoneBooth.Visible";
    const string MrPositionKey = "MR.PhoneBooth.MrPosition";
    const string MrRotationKey = "MR.PhoneBooth.MrRotation";

    static bool loaded;
    static bool visible = true;
    static bool hasMrPose;
    static Vector3 mrPosition;
    static Quaternion mrRotation = Quaternion.identity;

    public static bool Visible
    {
        get
        {
            EnsureLoaded();
            return visible;
        }
    }

    public static bool HasMrPose
    {
        get
        {
            EnsureLoaded();
            return hasMrPose;
        }
    }

    public static void EnsureLoaded()
    {
        if (loaded)
            return;

        visible = PlayerPrefs.GetInt(VisibleKey, 1) != 0;

        string posJson = PlayerPrefs.GetString(MrPositionKey, string.Empty);
        string rotJson = PlayerPrefs.GetString(MrRotationKey, string.Empty);
        hasMrPose = !string.IsNullOrEmpty(posJson) && !string.IsNullOrEmpty(rotJson);
        if (hasMrPose)
        {
            MRVector3 pos = JsonUtility.FromJson<MRVector3>(posJson);
            MRQuaternion rot = JsonUtility.FromJson<MRQuaternion>(rotJson);
            if (pos != null && rot != null)
            {
                mrPosition = pos.ToVector3();
                mrRotation = rot.ToQuaternion();
            }
            else
            {
                hasMrPose = false;
            }
        }

        loaded = true;
    }

    public static bool TryGetMrPose(out Vector3 position, out Quaternion rotation)
    {
        EnsureLoaded();
        position = mrPosition;
        rotation = mrRotation;
        return hasMrPose;
    }

    public static void SetVisible(bool value)
    {
        EnsureLoaded();
        visible = value;
        PlayerPrefs.SetInt(VisibleKey, value ? 1 : 0);
        PlayerPrefs.Save();
        ConfigManager.WriteConsole($"{LogPrefix} visible={value}");
    }

    public static void SaveMrPose(Vector3 position, Quaternion rotation)
    {
        EnsureLoaded();
        mrPosition = position;
        mrRotation = rotation;
        hasMrPose = true;
        PlayerPrefs.SetString(MrPositionKey, JsonUtility.ToJson(MRVector3.From(position)));
        PlayerPrefs.SetString(MrRotationKey, JsonUtility.ToJson(MRQuaternion.From(rotation)));
        PlayerPrefs.Save();
        ConfigManager.WriteConsole($"{LogPrefix} saved MR pose pos={position} rotY={rotation.eulerAngles.y:F1}");
    }

    public static void ClearMrPose()
    {
        EnsureLoaded();
        hasMrPose = false;
        PlayerPrefs.DeleteKey(MrPositionKey);
        PlayerPrefs.DeleteKey(MrRotationKey);
        PlayerPrefs.Save();
    }
}
