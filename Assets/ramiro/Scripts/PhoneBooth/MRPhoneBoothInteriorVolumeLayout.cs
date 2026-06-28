/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>
/// Derives phone-booth interior volume pose from PF_Payphone wall colliders.
/// </summary>
public static class MRPhoneBoothInteriorVolumeLayout
{
    const string InteriorVolumeName = "InteriorVolume";

    /// <summary>Fallback when wall colliders are missing (legacy).</summary>
    public static readonly Vector3 LegacyLocalCenter = new Vector3(-0.21f, 1.02f, -0.18f);
    public static readonly Vector3 LegacyLocalSize = new Vector3(0.72f, 1.75f, 0.78f);

    public static bool TryComputeLocalBounds(Transform boothRoot, out Vector3 center, out Vector3 size)
    {
        center = LegacyLocalCenter;
        size = LegacyLocalSize;
        if (boothRoot == null)
            return false;

        Bounds? outer = null;
        foreach (BoxCollider collider in boothRoot.GetComponentsInChildren<BoxCollider>(true))
        {
            if (collider == null || collider.transform == boothRoot.Find(InteriorVolumeName))
                continue;
            if (!IsWallCollider(collider))
                continue;

            Bounds localBounds = ColliderBoundsInBoothLocalSpace(boothRoot, collider);
            if (outer == null)
                outer = localBounds;
            else
            {
                Bounds merged = outer.Value;
                merged.Encapsulate(localBounds);
                outer = merged;
            }
        }

        if (outer == null)
            return false;

        Bounds interior = ShrinkOuterShellToInterior(outer.Value);
        center = interior.center;
        size = interior.size;
        return true;
    }

    static bool IsWallCollider(BoxCollider collider)
    {
        string name = collider.name;
        return name == "Collider" || name.StartsWith("Collider (");
    }

    static Bounds ColliderBoundsInBoothLocalSpace(Transform boothRoot, BoxCollider collider)
    {
        Bounds world = collider.bounds;
        Vector3 worldCenter = world.center;
        Vector3 extents = world.extents;

        Bounds local = new Bounds(boothRoot.InverseTransformPoint(worldCenter), Vector3.zero);
        for (int xi = -1; xi <= 1; xi += 2)
        {
            for (int yi = -1; yi <= 1; yi += 2)
            {
                for (int zi = -1; zi <= 1; zi += 2)
                {
                    Vector3 corner = worldCenter + Vector3.Scale(extents, new Vector3(xi, yi, zi));
                    local.Encapsulate(boothRoot.InverseTransformPoint(corner));
                }
            }
        }

        return local;
    }

    static Bounds ShrinkOuterShellToInterior(Bounds outer)
    {
        const float horizontalWallInset = 0.09f;
        const float verticalTrim = 0.04f;

        Vector3 size = outer.size;
        size.x = Mathf.Max(0.25f, size.x - horizontalWallInset * 2f);
        size.z = Mathf.Max(0.25f, size.z - horizontalWallInset * 2f);
        size.y = Mathf.Max(1.35f, size.y - verticalTrim);

        return new Bounds(outer.center, size);
    }
}
