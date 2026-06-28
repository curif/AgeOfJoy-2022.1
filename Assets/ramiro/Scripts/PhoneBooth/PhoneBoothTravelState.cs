/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using UnityEngine;

/// <summary>
/// Player pose relative to the phone booth root at the moment travel begins.
/// </summary>
[Serializable]
public class PhoneBoothTravelState
{
    public MRVector3 LocalPosition = new MRVector3();
    public MRQuaternion LocalRotation = new MRQuaternion { W = 1f };
    public bool HandsetGrabbed;

    public static PhoneBoothTravelState Capture(Transform boothRoot, Transform player, bool handsetGrabbed)
    {
        if (boothRoot == null || player == null)
            return null;

        Vector3 localPos = boothRoot.InverseTransformPoint(player.position);
        Quaternion localRot = Quaternion.Inverse(boothRoot.rotation) * player.rotation;

        return new PhoneBoothTravelState
        {
            LocalPosition = MRVector3.From(localPos),
            LocalRotation = MRQuaternion.From(localRot),
            HandsetGrabbed = handsetGrabbed
        };
    }

    public void ApplyToPlayer(Transform boothRoot, Transform player)
    {
        if (boothRoot == null || player == null || LocalPosition == null || LocalRotation == null)
            return;

        Vector3 worldPos = boothRoot.TransformPoint(LocalPosition.ToVector3());
        Quaternion worldRot = boothRoot.rotation * LocalRotation.ToQuaternion();
        player.SetPositionAndRotation(worldPos, worldRot);
    }
}
