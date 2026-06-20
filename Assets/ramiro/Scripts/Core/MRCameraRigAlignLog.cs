/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.IO;
using System.Text;
using Meta.XR.MRUtilityKit;
using Unity.XR.CoreUtils;
using UnityEngine;

/// <summary>
/// MRUK shim vs XROrigin alignment diagnostics.
/// Quest pull: adb pull /sdcard/Android/data/com.curif.AgeOfJoy/debug/mr-camera-rig-align.log
/// </summary>
public static class MRCameraRigAlignLog
{
    const string LogFileName = "mr-camera-rig-align.log";
    const string LatestFileName = "mr-camera-rig-align-latest.log";
    const int MaxLogBytes = 2 * 1024 * 1024;
    const float PeriodicLogIntervalSeconds = 2f;

    const string RoomCalXKey = "MR.RoomCal.X";
    const string RoomCalYKey = "MR.RoomCal.Y";
    const string RoomCalZKey = "MR.RoomCal.Z";
    const string RoomCalYawKey = "MR.RoomCal.Yaw";

    static readonly object FileLock = new object();
    static string logFilePath;
    static string latestFilePath;
    static bool initialized;
    static int sessionId;
    static float lastSnapshotTime = -999f;
    static bool? lastWorldLockActive;

    public static string LogFilePath => logFilePath ?? "(not initialized)";

    public static void EnsureSession(string reason)
    {
        lock (FileLock)
        {
            if (!initialized)
                InitializePaths();

            sessionId++;
            lastSnapshotTime = -999f;
            lastWorldLockActive = null;

            string banner =
                $"{Environment.NewLine}========== MR CAMERA RIG ALIGN #{sessionId} {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} =========={Environment.NewLine}" +
                $"reason={reason}{Environment.NewLine}" +
                $"unity={Application.unityVersion} platform={Application.platform} device={SystemInfo.deviceModel}{Environment.NewLine}" +
                $"logFile={logFilePath}{Environment.NewLine}" +
                $"latestCopy={latestFilePath}{Environment.NewLine}" +
                $"debugDir={ConfigManager.DebugDir}{Environment.NewLine}" +
                $"pull=adb pull \"{logFilePath}\"{Environment.NewLine}" +
                $"TIP: note ROOM ALIGN PlayerPrefs and compare effect mesh walls to passthrough.{Environment.NewLine}";

            WriteRaw(banner);
        }

        ConfigManager.WriteConsole($"[MRCameraRigAlignLog] session #{sessionId} -> {LogFilePath}");
    }

    public static void LogEvent(string reason)
    {
        WriteLine("EVENT", reason);
        LogSnapshot(reason, force: true);
    }

    public static void LogSnapshot(string reason, XROrigin xr = null, MRUKCameraRigStub shim = null, bool force = false)
    {
        if (!force && Time.unscaledTime - lastSnapshotTime < PeriodicLogIntervalSeconds)
            return;

        if (xr == null)
            xr = UnityEngine.Object.FindObjectOfType<XROrigin>();

        if (shim == null)
            shim = UnityEngine.Object.FindObjectOfType<MRUKCameraRigStub>();

        bool worldLockActive = MRUK.Instance != null && MRUK.Instance.IsWorldLockActive;
        if (!force && lastWorldLockActive.HasValue && lastWorldLockActive.Value == worldLockActive
            && Time.unscaledTime - lastSnapshotTime < PeriodicLogIntervalSeconds)
            return;

        if (worldLockActive != lastWorldLockActive)
        {
            WriteLine("WORLDLOCK", $"active={worldLockActive} (was={lastWorldLockActive?.ToString() ?? "null"}) reason={reason}");
            lastWorldLockActive = worldLockActive;
        }

        lastSnapshotTime = Time.unscaledTime;

        var sb = new StringBuilder();
        sb.Append($"SNAP reason={reason} frame={Time.frameCount} t={Time.unscaledTime:F2}s");
        sb.Append($" worldLock={worldLockActive}");
        if (MRUK.Instance != null)
        {
            sb.Append($" enableWorldLock={MRUK.Instance.EnableWorldLock}");
            Matrix4x4 offset = MRUK.Instance.TrackingSpaceOffset;
            sb.Append($" trackingOffset=({offset.GetColumn(3).x:F3},{offset.GetColumn(3).y:F3},{offset.GetColumn(3).z:F3})");
        }

        AppendSavedRoomCal(sb);
        AppendXrOrigin(sb, xr);
        AppendShim(sb, shim);
        AppendDeltas(sb, xr, shim);
        AppendMrukRoom(sb);

        WriteLine("SNAP", sb.ToString());
    }

    static void AppendSavedRoomCal(StringBuilder sb)
    {
        float x = PlayerPrefs.GetFloat(RoomCalXKey, 0f);
        float y = PlayerPrefs.GetFloat(RoomCalYKey, 0f);
        float z = PlayerPrefs.GetFloat(RoomCalZKey, 0f);
        float yaw = PlayerPrefs.GetFloat(RoomCalYawKey, 0f);
        if (x != 0f || y != 0f || z != 0f || yaw != 0f)
            sb.Append($" roomCalPrefs=({x:F2},{y:F2},{z:F2}) yaw={yaw:F0}");
    }

    static void AppendXrOrigin(StringBuilder sb, XROrigin xr)
    {
        if (xr == null)
        {
            sb.Append(" | xr=null");
            return;
        }

        Transform root = xr.transform;
        Transform origin = MRCameraRigShim.ResolveXrTrackingOrigin(xr);
        Transform camera = xr.Camera != null ? xr.Camera.transform : null;
        Transform floorOffset = xr.CameraFloorOffsetObject != null ? xr.CameraFloorOffsetObject.transform : null;

        sb.Append($" | xrRoot={Fmt(root)}");
        sb.Append($" xrOrigin={Fmt(origin)}");
        sb.Append($" trackingMode={xr.RequestedTrackingOriginMode}");
        if (floorOffset != null)
            sb.Append($" floorOffsetLocal={FmtLocal(floorOffset)}");
        if (camera != null)
            sb.Append($" xrCamera={Fmt(camera)}");
    }

    static void AppendShim(StringBuilder sb, MRUKCameraRigStub shim)
    {
        if (shim == null)
        {
            sb.Append(" | shim=null");
            return;
        }

        sb.Append($" | shimRoot={Fmt(shim.transform)}");
        if (shim.trackingSpace != null)
            sb.Append($" shimTrackingSpace={Fmt(shim.trackingSpace)}");
        if (shim.centerEyeAnchor != null)
        {
            sb.Append($" shimCenterEyeWorld={Fmt(shim.centerEyeAnchor)}");
            sb.Append($" shimCenterEyeLocal={FmtLocal(shim.centerEyeAnchor)}");
        }
    }

    static void AppendDeltas(StringBuilder sb, XROrigin xr, MRUKCameraRigStub shim)
    {
        if (xr == null || shim == null)
            return;

        Transform xrOrigin = MRCameraRigShim.ResolveXrTrackingOrigin(xr);
        Transform xrCamera = xr.Camera != null ? xr.Camera.transform : null;

        if (xrOrigin != null && shim.trackingSpace != null)
        {
            Vector3 delta = shim.trackingSpace.position - xrOrigin.position;
            float yawDelta = Mathf.DeltaAngle(xrOrigin.eulerAngles.y, shim.trackingSpace.eulerAngles.y);
            sb.Append($" | deltaTrackingSpace=({delta.x:F4},{delta.y:F4},{delta.z:F4}) yaw={yawDelta:F2}");
        }

        if (xrCamera != null && shim.centerEyeAnchor != null)
        {
            Vector3 eyeDelta = shim.centerEyeAnchor.position - xrCamera.position;
            sb.Append($" | deltaCenterEye=({eyeDelta.x:F4},{eyeDelta.y:F4},{eyeDelta.z:F4})");
        }

        if (xrCamera != null && shim.trackingSpace != null)
        {
            Vector3 expectedLocal = shim.trackingSpace.InverseTransformPoint(xrCamera.position);
            Vector3 actualLocal = shim.centerEyeAnchor != null ? shim.centerEyeAnchor.localPosition : Vector3.zero;
            Vector3 localErr = actualLocal - expectedLocal;
            sb.Append($" | centerEyeLocalErr=({localErr.x:F4},{localErr.y:F4},{localErr.z:F4})");
        }
    }

    static void AppendMrukRoom(StringBuilder sb)
    {
        if (MRUK.Instance == null)
        {
            sb.Append(" | mruk=null");
            return;
        }

        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room == null)
        {
            sb.Append(" | room=null");
            return;
        }

        int wallCount = room.WallAnchors != null ? room.WallAnchors.Count : 0;
        sb.Append($" walls={wallCount}");

        MRUKAnchor floor = room.FloorAnchor;
        if (floor != null)
            sb.Append($" floorCenterY={floor.GetAnchorCenter().y:F3}");

        if (wallCount > 0 && room.WallAnchors[0] != null)
        {
            Vector3 wallCenter = room.WallAnchors[0].GetAnchorCenter();
            sb.Append($" wall0=({wallCenter.x:F3},{wallCenter.y:F3},{wallCenter.z:F3})");
        }
    }

    static string Fmt(Transform t)
    {
        Vector3 p = t.position;
        Vector3 r = t.rotation.eulerAngles;
        return $"({p.x:F3},{p.y:F3},{p.z:F3}) rot({r.x:F1},{r.y:F1},{r.z:F1})";
    }

    static string FmtLocal(Transform t)
    {
        Vector3 p = t.localPosition;
        Vector3 r = t.localRotation.eulerAngles;
        return $"({p.x:F3},{p.y:F3},{p.z:F3}) rot({r.x:F1},{r.y:F1},{r.z:F1})";
    }

    static void InitializePaths()
    {
        try
        {
            ConfigManager.CreateFolder(ConfigManager.DebugDir);
            logFilePath = Path.Combine(ConfigManager.DebugDir, LogFileName);
            latestFilePath = Path.Combine(ConfigManager.DebugDir, LatestFileName);
        }
        catch (Exception ex)
        {
            logFilePath = Path.Combine(Application.persistentDataPath, LogFileName);
            latestFilePath = Path.Combine(Application.persistentDataPath, LatestFileName);
            Debug.LogWarning($"[MRCameraRigAlignLog] DebugDir fallback: {ex.Message} -> {logFilePath}");
        }

        initialized = true;
        TrimIfNeeded();
    }

    static void WriteLine(string level, string message)
    {
        string line = $"{DateTime.Now:HH:mm:ss.fff} [{level}] {message}";
        Debug.Log($"[MRCameraRigAlignLog] {line}");

        lock (FileLock)
        {
            if (!initialized)
                InitializePaths();
            WriteRaw(line + Environment.NewLine);
        }
    }

    static void WriteRaw(string text)
    {
        try
        {
            File.AppendAllText(logFilePath, text, Encoding.UTF8);
            File.AppendAllText(latestFilePath, text, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[MRCameraRigAlignLog] file write failed: {ex.Message}");
        }
    }

    static void TrimIfNeeded()
    {
        try
        {
            if (!File.Exists(logFilePath))
                return;

            var info = new FileInfo(logFilePath);
            if (info.Length <= MaxLogBytes)
                return;

            string tail = File.ReadAllText(logFilePath, Encoding.UTF8);
            if (tail.Length > MaxLogBytes / 2)
                tail = tail.Substring(tail.Length - MaxLogBytes / 2);

            File.WriteAllText(logFilePath, $"--- trimmed {DateTime.Now:O} ---{Environment.NewLine}{tail}", Encoding.UTF8);
        }
        catch
        {
            // best effort
        }
    }
}
