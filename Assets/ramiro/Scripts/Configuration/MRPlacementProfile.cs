/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>
/// Declares how an MR prefab/object is placed (surface, facing, optional stick rotation).
/// Attach to the root of prefabs under Resources/ramiro/PrefabsEnvironment,
/// Resources/ramiro/Lights, or any MR placeable.
/// </summary>
public class MRPlacementProfile : MonoBehaviour
{
    [Tooltip("Where this object can be placed (floor, wall, ceiling, table, …).")]
    public PlacementSurfaceType surfaceType = PlacementSurfaceType.Floor;

    [Tooltip("Local axis that points toward the viewer when auto-facing is used (horizontal spin on floor/ceiling).")]
    public PlacementFacingAxis facingAxis = PlacementFacingAxis.PositiveZ;

    [Tooltip("When enabled, right stick ← → rotates the object during placement ray (no auto-face).")]
    public bool allowStickRotation;

    [Tooltip("World axis for stick rotation. WorldYaw = eixo Y (girar no chão). WorldPitch = X. WorldRoll = Z.")]
    public PlacementStickRotationAxis stickRotationAxis = PlacementStickRotationAxis.WorldYaw;

    [Tooltip("Rotation speed with right stick (degrees per second).")]
    public float stickRotationSpeed = 90f;

    [Tooltip("Optional label for MR menus.")]
    public string displayName;

    [Tooltip("Wall placement ray push off the wall (meters). 0 = sit by the authored pivot " +
        "(object's back face is the contact point). >0 pushes half the depth into the room for " +
        "center-pivot objects (e.g. thin posters ~0.002).")]
    public float wallMountDepthMeters = 0f;

    // No coercion to a default here: an explicit 0 means "respect the pivot" (flush mount).
    public float GetWallMountDepthMeters() => Mathf.Max(0f, wallMountDepthMeters);

    public string GetDisplayName() =>
        string.IsNullOrEmpty(displayName) ? gameObject.name : displayName;

    public static MRPlacementProfile Resolve(GameObject target)
    {
        if (target == null)
            return null;

        MRPlacementProfile onRoot = target.GetComponent<MRPlacementProfile>();
        if (onRoot != null)
            return onRoot;

        return target.GetComponentInChildren<MRPlacementProfile>(true);
    }
}
