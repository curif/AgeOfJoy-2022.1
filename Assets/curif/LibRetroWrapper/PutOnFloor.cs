/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlaceOnFloorFromBoxCollider
{
    static LayerMask floorLayer = LayerMask.GetMask("floor");

    public static bool PlaceOnFloor(Transform transform, BoxCollider boxCollider, Collider floorOverride = null)
    {
        if (transform == null || boxCollider == null)
        {
            ConfigManager.WriteConsoleError($"[PlaceOnFloorFromBoxCollider.PlaceOnFloor] gameObject or boxCollider missing {transform}");
            return false;
        }

        // Align the lower part of the GameObject to the floor
        return AlignLowerPartToFloor(transform, boxCollider, floorOverride);
    }
    public static void CreateSphere(Vector3 position, GameObject go)
    {
        // Create a new sphere GameObject
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = go.name + "_boxlowpart";
        // Set the position of the sphere to the specified position
        sphere.transform.position = position;
        sphere.transform.parent = go.transform;

        // Set the scale of the sphere to 0.1 in all dimensions
        sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }
    // Models placed in the same slot (e.g. workshop test cabinets reloaded via
    // CabinetAutoReload) can have wildly different heights/pivots than whatever
    // occupied that slot before, so the lower point of the box collider can end up
    // far above or below the floor. Cast from well above it through a generous
    // distance instead of a narrow guess-and-retry window, so the floor is found
    // regardless of how far off the spawn height is.
    private const float CastAboveOffset = 50f;
    private const float MaxCastDistance = 200f;

    private static bool AlignLowerPartToFloor(Transform transform, BoxCollider boxCollider, Collider floorOverride = null)
    {
        float lowerPointY = CalculateLowerPointY(transform, boxCollider);

        // Manual escape hatch for rooms where automatic floor detection picks the
        // wrong level (multi-story rooms with stacked "floor" colliders). When set,
        // skip detection entirely and align straight to the assigned floor's top.
        if (floorOverride != null)
        {
            float overrideY = floorOverride.bounds.max.y;
            float overrideOffset = lowerPointY - overrideY;
            transform.position -= new Vector3(0f, overrideOffset, 0f);
            return true;
        }

        RaycastHit closestBelow;
        if (FindClosestFloorBelow(new Vector3(transform.position.x, lowerPointY, transform.position.z), out closestBelow))
        {
            float yOffset = lowerPointY - closestBelow.point.y;

            // Adjust the position of the GameObject
            transform.position -= new Vector3(0f, yOffset, 0f);
            return true;
        }
        ConfigManager.WriteConsoleWarning($"[PutOnFloor.AlignLowerPartToFloor] floor not found for {transform.gameObject.name}");
        return false;
    }

    // In multi-floor buildings several floor colliders share the "floor" layer and
    // can be stacked above/below a given point (e.g. every story's slab shares X/Z).
    // A plain Raycast only returns the nearest hit to the cast origin, which can land
    // on a floor above the point instead of the one directly underneath it. RaycastAll
    // every "floor" hit and keep only those at or below referenceY, then pick the
    // highest of those (i.e. the floor immediately below the point).
    private static bool FindClosestFloorBelow(Vector3 referencePoint, out RaycastHit closestBelow)
    {
        Vector3 castOrigin = new Vector3(referencePoint.x, referencePoint.y + CastAboveOffset, referencePoint.z);
        Ray ray = new Ray(castOrigin, Vector3.down);

        //CreateSphere(castOrigin, transform.gameObject);

        const float BelowTolerance = 0.01f;
        RaycastHit[] hits = Physics.RaycastAll(ray, MaxCastDistance, floorLayer);
        bool found = false;
        closestBelow = default;
        float closestBelowY = float.NegativeInfinity;
        foreach (RaycastHit candidate in hits)
        {
            if (candidate.point.y > referencePoint.y + BelowTolerance)
                continue; // floor is above the reference point, not a valid resting surface

            if (!found || candidate.point.y > closestBelowY)
            {
                closestBelow = candidate;
                closestBelowY = candidate.point.y;
                found = true;
            }
        }
        return found;
    }

    // Editor-time detection helper: finds the floor collider directly below a world
    // position (e.g. a CabinetController placeholder's transform) so it can be baked
    // into floorOverride ahead of time, without ambiguity at runtime.
    public static Collider DetectFloorBelow(Vector3 worldPosition)
    {
        RaycastHit hit;
        return FindClosestFloorBelow(worldPosition, out hit) ? hit.collider : null;
    }

    public static float CalculateLowerPointY(Transform transform, BoxCollider boxCollider)
    {
        // Get the center of the BoxCollider in world space
        Vector3 colliderCenter = transform.TransformPoint(boxCollider.center);

        // Calculate the half size of the BoxCollider in world space
        Vector3 colliderHalfSize = Vector3.Scale(boxCollider.size * 0.5f, transform.lossyScale);

        // Calculate the lower point in the Y-axis
        float lowerPointY = colliderCenter.y - colliderHalfSize.y;

        return lowerPointY;
    }
}

public class PutOnFloor : MonoBehaviour
{
    public BoxCollider boxCollider;

    [Tooltip("Optional manual override: the floor collider to align to. Leave empty " +
        "for automatic detection; set this for cabinets in multi-story rooms where " +
        "automatic detection picks the wrong floor.")]
    public Collider floorOverride;

    private void Start()
    {
        // Get the BoxCollider component attached to the GameObject
        if (boxCollider == null)
            boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            ConfigManager.WriteConsoleError($"[PutOnFloor.Start] there is not a boxCollider for {name}");
            return;
        }
        StartCoroutine(placeOnFloorCoroutine());
    }


    private IEnumerator placeOnFloorCoroutine()
    {

        if (!PlaceOnFloorFromBoxCollider.PlaceOnFloor(gameObject.transform, boxCollider, floorOverride))
            ConfigManager.WriteConsoleWarning($"[PutOnFloor.Start] can't re-position cabinet on floor {name}");
        yield break;
    }
}