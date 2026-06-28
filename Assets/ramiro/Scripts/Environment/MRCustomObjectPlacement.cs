/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>Read/apply uniform XYZ scale on MR custom object instances.</summary>
public static class MRCustomObjectPlacement
{
    public const float TuneStep = 0.1f;
    public const float DefaultScale = 1f;
    public const float MinScale = 0.1f;
    public const float MaxScale = 10f;

    public static float SnapScale(float value)
    {
        float snapped = Mathf.Round(value / TuneStep) * TuneStep;
        return Mathf.Clamp(snapped, MinScale, MaxScale);
    }

    public static bool TryReadUserScale(GameObject root, out float scale)
    {
        scale = DefaultScale;
        if (root == null)
            return false;

        scale = root.transform.localScale.x;
        return scale > 0f;
    }

    public static void ApplyUserScale(GameObject root, float scale)
    {
        if (root == null)
            return;

        scale = SnapScale(scale);
        root.transform.localScale = Vector3.one * scale;
    }
}
