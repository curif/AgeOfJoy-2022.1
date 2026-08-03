/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

/// <summary>
/// MR-side phone booth visibility, pose, and travel preferences.
/// Persisted in <c>MR/phone-booth.yaml</c> (not PlayerPrefs).
/// </summary>
public static class MRPhoneBoothSettings
{
    const string LogPrefix = "[MRPhoneBoothSettings]";
    const int YamlVersion = 1;

    /// <summary>Legacy PlayerPrefs — migrated once into phone-booth.yaml when present.</summary>
    const string LegacyVisibleKey = "MR.PhoneBooth.Visible";
    const string LegacyAutoHideAfterTravelKey = "MR.PhoneBooth.AutoHideAfterTravel";
    const string LegacyMrPositionKey = "MR.PhoneBooth.MrPosition";
    const string LegacyMrRotationKey = "MR.PhoneBooth.MrRotation";

    static bool loaded;
    static bool visible = true;
    /// <summary>Default on: hide the MR booth after phone-booth VR→MR arrival.</summary>
    static bool autoHideAfterTravel = true;
    static bool hasMrPose;
    static Vector3 mrPosition;
    static Quaternion mrRotation = Quaternion.identity;
    static MRPhoneBoothYamlFile cachedFile;

    public static bool Visible
    {
        get
        {
            EnsureLoaded();
            return visible;
        }
    }

    /// <summary>
    /// When true (default), the booth hides after immersive phone-booth entry into MR.
    /// Show it again from the CRT to return to VR. When false, the booth stays in the room
    /// until the user hides it manually in PHONE BOOTH.
    /// </summary>
    public static bool AutoHideAfterTravel
    {
        get
        {
            EnsureLoaded();
            return autoHideAfterTravel;
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

        MRPaths.EnsureFolders();
        TryMigrateLegacyPlayerPrefsIfNeeded();
        MRPhoneBoothYamlFile file = LoadYamlFile();
        ApplyFromFile(file);
        loaded = true;
        ConfigManager.WriteConsole(
            $"{LogPrefix} loaded visible={visible} autoHideAfterTravel={autoHideAfterTravel} " +
            $"hasPose={hasMrPose} ({MRPaths.PhoneBoothYamlPath})");
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
        if (visible == value)
            return;

        visible = value;
        Persist();
        ConfigManager.WriteConsole($"{LogPrefix} visible={value}");
    }

    public static void SetAutoHideAfterTravel(bool value)
    {
        EnsureLoaded();
        if (autoHideAfterTravel == value)
            return;

        autoHideAfterTravel = value;
        Persist();
        ConfigManager.WriteConsole($"{LogPrefix} autoHideAfterTravel={value}");
    }

    public static void SaveMrPose(Vector3 position, Quaternion rotation)
    {
        EnsureLoaded();
        mrPosition = position;
        mrRotation = rotation;
        hasMrPose = true;
        Persist();
        ConfigManager.WriteConsole($"{LogPrefix} saved MR pose pos={position} rotY={rotation.eulerAngles.y:F1}");
    }

    public static void ClearMrPose()
    {
        EnsureLoaded();
        if (!hasMrPose)
            return;

        hasMrPose = false;
        mrPosition = Vector3.zero;
        mrRotation = Quaternion.identity;
        Persist();
    }

    /// <summary>Call after CRT Delete Configs wipes <c>MR/*.yaml</c>.</summary>
    public static void InvalidateAfterYamlWipe()
    {
        loaded = false;
        cachedFile = null;
        visible = true;
        autoHideAfterTravel = true;
        hasMrPose = false;
        mrPosition = Vector3.zero;
        mrRotation = Quaternion.identity;
        MRPaths.SeedPhoneBoothYamlIfNeeded();
        EnsureLoaded();
    }

    static void ApplyFromFile(MRPhoneBoothYamlFile file)
    {
        if (file == null)
        {
            visible = true;
            autoHideAfterTravel = true;
            hasMrPose = false;
            return;
        }

        visible = file.Visible;
        autoHideAfterTravel = file.AutoHideAfterTravel;
        hasMrPose = false;
        mrPosition = Vector3.zero;
        mrRotation = Quaternion.identity;

        if (file.MrPosition != null && file.MrRotation != null)
        {
            mrPosition = file.MrPosition.ToVector3();
            mrRotation = file.MrRotation.ToQuaternion();
            hasMrPose = true;
        }
    }

    /// <summary>
    /// If legacy PlayerPrefs exist and yaml is still the seeded default (or missing pose),
    /// copy prefs into yaml once and delete the keys.
    /// </summary>
    static void TryMigrateLegacyPlayerPrefsIfNeeded()
    {
        bool hadLegacy = PlayerPrefs.HasKey(LegacyVisibleKey)
            || PlayerPrefs.HasKey(LegacyAutoHideAfterTravelKey)
            || PlayerPrefs.HasKey(LegacyMrPositionKey)
            || PlayerPrefs.HasKey(LegacyMrRotationKey);

        if (!hadLegacy)
            return;

        visible = PlayerPrefs.GetInt(LegacyVisibleKey, 1) != 0;
        autoHideAfterTravel = PlayerPrefs.GetInt(LegacyAutoHideAfterTravelKey, 1) != 0;

        string posJson = PlayerPrefs.GetString(LegacyMrPositionKey, string.Empty);
        string rotJson = PlayerPrefs.GetString(LegacyMrRotationKey, string.Empty);
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

        Persist();

        PlayerPrefs.DeleteKey(LegacyVisibleKey);
        PlayerPrefs.DeleteKey(LegacyAutoHideAfterTravelKey);
        PlayerPrefs.DeleteKey(LegacyMrPositionKey);
        PlayerPrefs.DeleteKey(LegacyMrRotationKey);
        PlayerPrefs.Save();

        ConfigManager.WriteConsole($"{LogPrefix} migrated PlayerPrefs → {MRPaths.PhoneBoothYamlPath}");
    }

    static MRPhoneBoothYamlFile LoadYamlFile()
    {
        if (cachedFile != null)
            return cachedFile;

        string path = MRPaths.PhoneBoothYamlPath;
        MRPaths.SeedPhoneBoothYamlIfNeeded();

        if (!File.Exists(path))
        {
            cachedFile = CreateDefaultFile();
            return cachedFile;
        }

        try
        {
            string text = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(text))
            {
                cachedFile = CreateDefaultFile();
                return cachedFile;
            }

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            cachedFile = deserializer.Deserialize<MRPhoneBoothYamlFile>(text) ?? CreateDefaultFile();
            if (cachedFile.Version <= 0)
                cachedFile.Version = YamlVersion;
            return cachedFile;
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"{LogPrefix} failed to read {path}", e);
            cachedFile = CreateDefaultFile();
            return cachedFile;
        }
    }

    static MRPhoneBoothYamlFile CreateDefaultFile() =>
        new MRPhoneBoothYamlFile
        {
            Version = YamlVersion,
            Visible = true,
            AutoHideAfterTravel = true
        };

    static void Persist()
    {
        MRPhoneBoothYamlFile file = cachedFile ?? CreateDefaultFile();
        file.Version = YamlVersion;
        file.Visible = visible;
        file.AutoHideAfterTravel = autoHideAfterTravel;
        if (hasMrPose)
        {
            file.MrPosition = MRVector3.From(mrPosition);
            file.MrRotation = MRQuaternion.From(mrRotation);
        }
        else
        {
            file.MrPosition = null;
            file.MrRotation = null;
        }

        cachedFile = file;
        SaveYamlFile(file);
    }

    static void SaveYamlFile(MRPhoneBoothYamlFile file)
    {
        if (file == null)
            return;

        string path = MRPaths.PhoneBoothYamlPath;
        try
        {
            ConfigManager.CreateFolder(MRPaths.MrDir);
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .Build();
            string body = serializer.Serialize(file);
            string text =
                "# Age of Joy — MR phone booth preferences\n" +
                "# Path: MR/phone-booth.yaml\n" +
                "# visible: show/hide booth in MR\n" +
                "# autoHideAfterTravel: hide after phone-booth VR→MR (default true)\n" +
                body;
            File.WriteAllText(path, text);
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"{LogPrefix} failed to write {path}", e);
        }
    }
}

/// <summary>On-disk schema for <c>MR/phone-booth.yaml</c>.</summary>
public class MRPhoneBoothYamlFile
{
    public int Version = 1;
    public bool Visible = true;
    public bool AutoHideAfterTravel = true;
    public MRVector3 MrPosition;
    public MRQuaternion MrRotation;
}
