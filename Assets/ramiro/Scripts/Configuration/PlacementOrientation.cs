/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>
/// Which local axis is the object's visible "front" for MR placement.
/// Default Unity meshes use +Z; ConfigurationCabinetMiniMR wall frame uses -X (screen faces -X on root).
/// </summary>
public enum PlacementFacingAxis
{
    PositiveZ = 0,
    NegativeZ = 1,
    PositiveX = 2,
    NegativeX = 3
}

/// <summary>
/// World axis used when the player rotates an object with the left stick during placement ray.
/// </summary>
public enum PlacementStickRotationAxis
{
    /// <summary>World Y (vertical). Spin on the floor like a turntable. Quest: left stick ← →.</summary>
    WorldYaw = 0,
    /// <summary>World X. Tilt forward/back around horizontal X.</summary>
    WorldPitch = 1,
    /// <summary>World Z. Roll around forward Z.</summary>
    WorldRoll = 2
}

public static class PlacementOrientation
{
    public static Vector3 WorldAxis(PlacementStickRotationAxis axis) =>
        axis switch
        {
            PlacementStickRotationAxis.WorldPitch => Vector3.right,
            PlacementStickRotationAxis.WorldRoll => Vector3.forward,
            _ => Vector3.up
        };

    /// <summary>Apply stick rotation offset (degrees) around the chosen world axis.</summary>
    public static Quaternion ApplyStickRotationOffset(
        Quaternion baseRotation,
        PlacementStickRotationAxis axis,
        float degrees)
    {
        if (Mathf.Approximately(degrees, 0f))
            return baseRotation;
        return Quaternion.AngleAxis(degrees, WorldAxis(axis)) * baseRotation;
    }

    public static Vector3 LocalForward(PlacementFacingAxis axis) =>
        axis switch
        {
            PlacementFacingAxis.NegativeZ => Vector3.back,
            PlacementFacingAxis.PositiveX => Vector3.right,
            PlacementFacingAxis.NegativeX => Vector3.left,
            _ => Vector3.forward
        };

    /// <summary>
    /// World rotation so the chosen local axis points along worldDirection (horizontal facing).
    /// </summary>
    public static Quaternion LookRotationWithFacing(Vector3 worldDirection, PlacementFacingAxis axis, Vector3 up)
    {
        if (worldDirection.sqrMagnitude < 0.001f)
            worldDirection = Vector3.forward;
        worldDirection.Normalize();

        if (up.sqrMagnitude < 0.001f)
            up = Vector3.up;
        up.Normalize();

        Quaternion baseRot = Quaternion.LookRotation(worldDirection, up);
        return axis switch
        {
            PlacementFacingAxis.NegativeZ => baseRot * Quaternion.Euler(0f, 180f, 0f),
            PlacementFacingAxis.PositiveX => baseRot * Quaternion.Euler(0f, -90f, 0f),
            PlacementFacingAxis.NegativeX => baseRot * Quaternion.Euler(0f, 90f, 0f),
            _ => baseRot
        };
    }

    public static Vector3 WorldForward(Quaternion rotation, PlacementFacingAxis axis)
    {
        Vector3 local = LocalForward(axis);
        Vector3 world = rotation * local;
        world.y = 0f;
        if (world.sqrMagnitude < 0.001f)
            return Vector3.forward;
        return world.normalized;
    }

    /// <summary>
    /// If the facing axis points away from the viewer, apply 180° yaw.
    /// </summary>
    public static Quaternion EnsureFacingViewer(
        Quaternion rotation,
        PlacementFacingAxis axis,
        Vector3 objectPosition,
        Vector3 viewerPosition)
    {
        Vector3 toViewer = viewerPosition - objectPosition;
        toViewer.y = 0f;
        if (toViewer.sqrMagnitude < 0.001f)
            return rotation;

        Vector3 forward = WorldForward(rotation, axis);
        if (Vector3.Dot(forward, toViewer.normalized) < 0f)
            rotation *= Quaternion.Euler(0f, 180f, 0f);

        return rotation;
    }
}
