/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using Meta.XR.MRUtilityKit;
using UnityEngine;

/// <summary>
/// Converts between world poses and MRUK anchor-local poses using stable OVRAnchor UUIDs.
/// </summary>
public static class MRAnchorPoseResolver
{
    const string LogPrefix = "[MRAnchorPoseResolver]";

    public static bool TryGetUuid(MRUKAnchor anchor, out Guid uuid)
    {
        uuid = Guid.Empty;
        if (anchor == null || !anchor.HasValidHandle)
            return false;

        uuid = anchor.Anchor.Uuid;
        return uuid != Guid.Empty;
    }

    public static MRUKAnchor FindAnchor(MRUKRoom room, Guid uuid)
    {
        if (room == null || uuid == Guid.Empty)
            return null;

        foreach (MRUKAnchor anchor in room.Anchors)
        {
            if (anchor == null || !anchor.HasValidHandle)
                continue;
            if (anchor.Anchor.Uuid == uuid)
                return anchor;
        }

        return null;
    }

    public static MRUKAnchor GetDefaultAnchor(MRUKRoom room, PlacementSurfaceType surface)
    {
        if (room == null)
            return null;

        switch (surface)
        {
            case PlacementSurfaceType.Wall:
                return room.WallAnchors != null && room.WallAnchors.Count > 0
                    ? room.WallAnchors[0]
                    : null;
            case PlacementSurfaceType.Ceiling:
                return room.CeilingAnchor;
            case PlacementSurfaceType.Floor:
            default:
                return room.FloorAnchor;
        }
    }

    public static MRUKAnchor FindClosestWallAnchor(MRUKRoom room, Vector3 worldPosition)
    {
        if (room?.WallAnchors == null || room.WallAnchors.Count == 0)
            return null;

        MRUKAnchor best = null;
        float bestDistance = float.MaxValue;
        foreach (MRUKAnchor wall in room.WallAnchors)
        {
            if (wall == null)
                continue;

            float distance = Vector3.Distance(wall.GetAnchorCenter(), worldPosition);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = wall;
            }
        }

        return best;
    }

    public static Guid ResolveAnchorUuid(
        MRUKRoom room,
        PlacementSurfaceType surface,
        Vector3 worldPosition,
        Guid hitUuid)
    {
        if (hitUuid != Guid.Empty)
            return hitUuid;

        MRUKAnchor fallback = surface == PlacementSurfaceType.Wall
            ? FindClosestWallAnchor(room, worldPosition)
            : GetDefaultAnchor(room, surface);

        return TryGetUuid(fallback, out Guid uuid) ? uuid : Guid.Empty;
    }

    public static void WorldToLocal(
        MRUKAnchor anchor,
        Vector3 worldPosition,
        Quaternion worldRotation,
        out Vector3 localPosition,
        out Quaternion localRotation)
    {
        Transform anchorTransform = anchor.transform;
        localPosition = anchorTransform.InverseTransformPoint(worldPosition);
        localRotation = Quaternion.Inverse(anchorTransform.rotation) * worldRotation;
    }

    public static bool LocalToWorld(
        MRUKAnchor anchor,
        Vector3 localPosition,
        Quaternion localRotation,
        out Vector3 worldPosition,
        out Quaternion worldRotation)
    {
        worldPosition = Vector3.zero;
        worldRotation = Quaternion.identity;
        if (anchor == null)
            return false;

        Transform anchorTransform = anchor.transform;
        worldPosition = anchorTransform.TransformPoint(localPosition);
        worldRotation = anchorTransform.rotation * localRotation;
        return true;
    }

    public static bool TryResolveWorldPose(
        MRUKRoom room,
        string anchorUuidText,
        Vector3 storedPosition,
        Quaternion storedRotation,
        PlacementSurfaceType surface,
        out Vector3 worldPosition,
        out Quaternion worldRotation)
    {
        worldPosition = storedPosition;
        worldRotation = storedRotation;

        if (string.IsNullOrEmpty(anchorUuidText) || !Guid.TryParse(anchorUuidText, out Guid anchorUuid))
            return true;

        MRUKAnchor anchor = FindAnchor(room, anchorUuid);
        if (anchor == null)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} anchor {anchorUuid} missing for {surface} — pose unresolved (will retry)");
            return false;
        }

        return LocalToWorld(anchor, storedPosition, storedRotation, out worldPosition, out worldRotation);
    }

    public static bool TryWriteAnchorRelativePose(
        MRUKRoom room,
        PlacementSurfaceType surface,
        Vector3 worldPosition,
        Quaternion worldRotation,
        Guid hitUuid,
        out string anchorUuidText,
        out MRVector3 storedPosition,
        out MRQuaternion storedRotation)
    {
        anchorUuidText = null;
        storedPosition = MRVector3.From(worldPosition);
        storedRotation = MRQuaternion.From(worldRotation);

        Guid resolvedUuid = ResolveAnchorUuid(room, surface, worldPosition, hitUuid);
        if (room == null || resolvedUuid == Guid.Empty)
            return false;

        MRUKAnchor anchor = FindAnchor(room, resolvedUuid);
        if (anchor == null)
            return false;

        WorldToLocal(anchor, worldPosition, worldRotation, out Vector3 localPosition, out Quaternion localRotation);
        anchorUuidText = resolvedUuid.ToString();
        storedPosition = MRVector3.From(localPosition);
        storedRotation = MRQuaternion.From(localRotation);
        return true;
    }
}
