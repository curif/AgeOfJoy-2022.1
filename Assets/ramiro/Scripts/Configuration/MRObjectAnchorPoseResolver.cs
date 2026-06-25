/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>Pose local/world relative to a placed MR prop (tag MRPlacementAnchor), not MRUK.</summary>
public static class MRObjectAnchorPoseResolver
{
    public const string AnchorTag = "MRPlacementAnchor";
    const string LogPrefix = "[MRObjectAnchorPoseResolver]";

    public static Transform FindAnchorTransform(GameObject parentRoot, string anchorPoint)
    {
        if (parentRoot == null)
            return null;

        if (!string.IsNullOrEmpty(anchorPoint))
        {
            Transform named = FindChildByName(parentRoot.transform, anchorPoint);
            if (named != null)
                return named;
        }

        foreach (Collider collider in parentRoot.GetComponentsInChildren<Collider>(true))
        {
            if (collider != null && collider.CompareTag(AnchorTag))
                return collider.transform;
        }

        return parentRoot.transform;
    }

    public static bool TryResolveHit(
        RaycastHit hit,
        out string placementId,
        out string anchorPoint,
        out Transform anchorTransform)
    {
        placementId = null;
        anchorPoint = null;
        anchorTransform = null;

        if (hit.collider == null || !hit.collider.CompareTag(AnchorTag))
            return false;

        MRPlacedEnvironment placed = hit.collider.GetComponentInParent<MRPlacedEnvironment>();
        if (placed == null || string.IsNullOrEmpty(placed.PlacementId))
            return false;

        placementId = placed.PlacementId;
        anchorTransform = hit.collider.transform;
        anchorPoint = anchorTransform.name;
        return true;
    }

    public static void WorldToLocal(
        Transform anchor,
        Vector3 worldPosition,
        Quaternion worldRotation,
        out Vector3 localPosition,
        out Quaternion localRotation)
    {
        localPosition = anchor.InverseTransformPoint(worldPosition);
        localRotation = Quaternion.Inverse(anchor.rotation) * worldRotation;
    }

    public static bool LocalToWorld(
        Transform anchor,
        Vector3 localPosition,
        Quaternion localRotation,
        out Vector3 worldPosition,
        out Quaternion worldRotation)
    {
        worldPosition = Vector3.zero;
        worldRotation = Quaternion.identity;
        if (anchor == null)
            return false;

        worldPosition = anchor.TransformPoint(localPosition);
        worldRotation = anchor.rotation * localRotation;
        return true;
    }

    public static bool TryResolveWorldPose(
        GameObject parentRoot,
        string anchorPoint,
        Vector3 storedPosition,
        Quaternion storedRotation,
        out Vector3 worldPosition,
        out Quaternion worldRotation)
    {
        worldPosition = storedPosition;
        worldRotation = storedRotation;

        Transform anchor = FindAnchorTransform(parentRoot, anchorPoint);
        if (anchor == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} anchor transform missing on {parentRoot?.name}");
            return false;
        }

        return LocalToWorld(anchor, storedPosition, storedRotation, out worldPosition, out worldRotation);
    }

    public static bool TryWriteAnchorRelativePose(
        GameObject parentRoot,
        string anchorPoint,
        Vector3 worldPosition,
        Quaternion worldRotation,
        out string storedAnchorPoint,
        out MRVector3 storedPosition,
        out MRQuaternion storedRotation)
    {
        storedAnchorPoint = anchorPoint;
        storedPosition = MRVector3.From(worldPosition);
        storedRotation = MRQuaternion.From(worldRotation);

        Transform anchor = FindAnchorTransform(parentRoot, anchorPoint);
        if (anchor == null)
            return false;

        if (string.IsNullOrEmpty(storedAnchorPoint))
            storedAnchorPoint = anchor.name;

        WorldToLocal(anchor, worldPosition, worldRotation, out Vector3 localPosition, out Quaternion localRotation);
        storedPosition = MRVector3.From(localPosition);
        storedRotation = MRQuaternion.From(localRotation);
        return true;
    }

    public static Transform FindChildByName(Transform root, string childName)
    {
        if (root == null || string.IsNullOrEmpty(childName))
            return null;

        if (string.Equals(root.name, childName, System.StringComparison.OrdinalIgnoreCase))
            return root;

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child != root && string.Equals(child.name, childName, System.StringComparison.OrdinalIgnoreCase))
                return child;
        }

        return null;
    }
}
