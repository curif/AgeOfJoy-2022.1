/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>Read/apply uniform Y+Z scale on MR wall poster instances.</summary>
public static class MRPosterPlacement
{
    public const float TuneStep = 0.1f;
    public const float DefaultScale = 1f;

    public static float SnapScale(float value)
    {
        float snapped = Mathf.Round(value / TuneStep) * TuneStep;
        return snapped > 0f ? snapped : TuneStep;
    }

    public static bool TryReadUserScale(GameObject root, out float scale)
    {
        scale = DefaultScale;
        if (root == null)
            return false;

        MRWallPoster poster = root.GetComponent<MRWallPoster>();
        if (poster == null)
            return false;

        scale = poster.UserScale;
        return true;
    }

    public static void ApplyUserScale(GameObject root, float scale)
    {
        if (root == null)
            return;

        MRWallPoster poster = root.GetComponent<MRWallPoster>();
        if (poster == null)
            return;

        poster.SetUserScale(scale);
    }
}
