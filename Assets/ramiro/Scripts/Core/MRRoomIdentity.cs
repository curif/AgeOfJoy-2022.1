/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections.Generic;
using System.Text;
using Meta.XR.MRUtilityKit;

/// <summary>
/// Stable id for a scanned MRUK room (FloorAnchor UUID, with anchor-fingerprint fallback).
/// Used as the folder name under MR/Rooms/{roomId}/.
/// </summary>
public static class MRRoomIdentity
{
    const string LogPrefix = "[MRRoomIdentity]";

    public static bool TryGetCurrentRoomId(out string roomId)
    {
        roomId = null;
        MRUKRoom room = MREnvironmentSurfaces.Instance != null
            ? MREnvironmentSurfaces.Instance.CurrentRoom
            : null;
        if (room == null && MRUK.Instance != null)
            room = MRUK.Instance.GetCurrentRoom();

        return TryGetRoomId(room, out roomId);
    }

    public static bool TryGetRoomId(MRUKRoom room, out string roomId)
    {
        roomId = null;
        if (room == null)
            return false;

        if (TryGetFloorAnchorUuid(room, out Guid floorUuid))
        {
            roomId = floorUuid.ToString("D");
            return true;
        }

        if (TryBuildAnchorFingerprint(room, out string fingerprint))
        {
            roomId = fingerprint;
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} floor UUID missing — using anchor fingerprint {roomId}");
            return true;
        }

        return false;
    }

    static bool TryGetFloorAnchorUuid(MRUKRoom room, out Guid uuid)
    {
        uuid = Guid.Empty;
        MRUKAnchor floor = room.FloorAnchor;
        return floor != null && MRAnchorPoseResolver.TryGetUuid(floor, out uuid) && uuid != Guid.Empty;
    }

    /// <summary>
    /// Stable short id from sorted anchor UUIDs when FloorAnchor UUID is unavailable.
    /// </summary>
    static bool TryBuildAnchorFingerprint(MRUKRoom room, out string fingerprint)
    {
        fingerprint = null;
        if (room.Anchors == null || room.Anchors.Count == 0)
            return false;

        var uuids = new List<string>(room.Anchors.Count);
        foreach (MRUKAnchor anchor in room.Anchors)
        {
            if (anchor == null || !MRAnchorPoseResolver.TryGetUuid(anchor, out Guid uuid) || uuid == Guid.Empty)
                continue;
            uuids.Add(uuid.ToString("D"));
        }

        if (uuids.Count == 0)
            return false;

        uuids.Sort(StringComparer.Ordinal);
        var sb = new StringBuilder(uuids.Count * 36);
        for (int i = 0; i < uuids.Count; i++)
        {
            if (i > 0)
                sb.Append('|');
            sb.Append(uuids[i]);
        }

        // FNV-1a 64-bit → hex (filesystem-safe, stable across sessions).
        ulong hash = 14695981039346656037UL;
        string payload = sb.ToString();
        for (int i = 0; i < payload.Length; i++)
        {
            hash ^= payload[i];
            hash *= 1099511628211UL;
        }

        fingerprint = $"fp-{hash:x16}";
        return true;
    }
}
