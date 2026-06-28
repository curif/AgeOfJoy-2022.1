/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>Continuous local rotation driven by object.yaml <c>rotator</c> component.</summary>
public class MRCustomObjectRotator : MonoBehaviour
{
    Transform target;
    Vector3 localAxis = Vector3.up;
    float degreesPerSecond;

    public void Configure(Transform targetTransform, string axisName, float speed)
    {
        target = targetTransform != null ? targetTransform : transform;
        localAxis = ParseLocalAxis(axisName);
        degreesPerSecond = speed;
    }

    void Update()
    {
        if (target == null || degreesPerSecond == 0f)
            return;

        target.Rotate(localAxis * degreesPerSecond * Time.deltaTime, Space.Self);
    }

    static Vector3 ParseLocalAxis(string axisName)
    {
        if (string.IsNullOrEmpty(axisName))
            return Vector3.up;

        switch (axisName.Trim().ToLowerInvariant())
        {
            case "x":
            case "-x":
                return axisName.StartsWith("-") ? Vector3.left : Vector3.right;
            case "y":
            case "-y":
                return axisName.StartsWith("-") ? Vector3.down : Vector3.up;
            case "z":
            case "-z":
                return axisName.StartsWith("-") ? Vector3.back : Vector3.forward;
            default:
                ConfigManager.WriteConsoleWarning($"[MRCustomObjectRotator] unknown axis '{axisName}', using y");
                return Vector3.up;
        }
    }
}
