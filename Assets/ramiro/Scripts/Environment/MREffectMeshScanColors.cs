/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using Meta.XR.MRUtilityKit;
using UnityEngine;

/// <summary>
/// Fixed semi-transparent palette per MRUK anchor label — scan debug overlay.
/// </summary>
public static class MREffectMeshScanColors
{
    public const float DefaultAlpha = 0.35f;

    public static bool TryGetColor(MRUKAnchor anchor, out Color color)
    {
        color = default;
        if (anchor == null)
            return false;

        MRUKAnchor.SceneLabels label = anchor.Label;
        if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.TABLE))
        {
            color = new Color(0.95f, 0.85f, 0.15f, 0.40f);
            return true;
        }

        if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.COUCH))
        {
            color = new Color(0.65f, 0.30f, 0.90f, DefaultAlpha);
            return true;
        }

        if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.BED))
        {
            color = new Color(0.95f, 0.35f, 0.65f, DefaultAlpha);
            return true;
        }

        if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.FLOOR))
        {
            color = new Color(0.20f, 0.85f, 0.35f, DefaultAlpha);
            return true;
        }

        if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.WALL_FACE))
        {
            color = new Color(0.95f, 0.45f, 0.20f, DefaultAlpha);
            return true;
        }

        if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.CEILING))
        {
            color = new Color(0.25f, 0.55f, 0.95f, 0.30f);
            return true;
        }

        if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.DOOR_FRAME))
        {
            color = new Color(0.20f, 0.90f, 0.90f, DefaultAlpha);
            return true;
        }

        if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.WINDOW_FRAME))
        {
            color = new Color(0.45f, 0.80f, 1.00f, DefaultAlpha);
            return true;
        }

        if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.GLOBAL_MESH))
        {
            color = new Color(0.85f, 0.85f, 0.85f, 0.18f);
            return true;
        }

        if (label != 0)
        {
            color = new Color(0.90f, 0.20f, 0.90f, 0.28f);
            return true;
        }

        return false;
    }

    public static string DescribeLabel(MRUKAnchor anchor)
    {
        if (anchor == null)
            return "none";

        if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.TABLE))
            return "Table";
        if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.COUCH))
            return "Couch";
        if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.BED))
            return "Bed";
        if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.FLOOR))
            return "Floor";
        if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.WALL_FACE))
            return "Wall";
        if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.CEILING))
            return "Ceiling";
        if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.DOOR_FRAME))
            return "Door";
        if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.WINDOW_FRAME))
            return "Window";
        if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.GLOBAL_MESH))
            return "Global";

        return anchor.Label.ToString();
    }
}
