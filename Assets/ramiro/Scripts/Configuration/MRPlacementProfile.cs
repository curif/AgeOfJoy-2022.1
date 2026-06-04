/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>
/// Declares how an MR prefab/object is placed (surface, facing, optional stick rotation).
/// Attach to the root of prefabs under Resources/ramiro/PrefabsEnvironment (or any MR placeable).
/// </summary>
public class MRPlacementProfile : MonoBehaviour
{
    [Tooltip("Where this object can be placed (floor, wall, ceiling, …).")]
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
