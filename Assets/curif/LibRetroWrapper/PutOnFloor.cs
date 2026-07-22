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

    public static bool PlaceOnFloor(Transform transform, BoxCollider boxCollider)
    {
        if (transform == null || boxCollider == null)
        {
            ConfigManager.WriteConsoleError($"[PlaceOnFloorFromBoxCollider.PlaceOnFloor] gameObject or boxCollider missing {transform}");
            return false;
        }

        // Align the lower part of the GameObject to the floor
        return AlignLowerPartToFloor(transform, boxCollider);
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

    private static bool AlignLowerPartToFloor(Transform transform, BoxCollider boxCollider)
    {
        float lowerPointY = CalculateLowerPointY(transform, boxCollider);
        Vector3 castOrigin = new Vector3(transform.position.x,
                                            lowerPointY + CastAboveOffset,
                                            transform.position.z);
        Ray ray = new Ray(castOrigin, Vector3.down);

        //CreateSphere(castOrigin, transform.gameObject);

        // Perform a raycast to check for the floor
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, MaxCastDistance, floorLayer))
        {
            float yOffset = lowerPointY - hit.point.y;

            // Adjust the position of the GameObject
            transform.position -= new Vector3(0f, yOffset, 0f);
            return true;
        }
        ConfigManager.WriteConsoleWarning($"[PutOnFloor.AlignLowerPartToFloor] floor not found for {transform.gameObject.name}");
        return false;
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

        if (!PlaceOnFloorFromBoxCollider.PlaceOnFloor(gameObject.transform, boxCollider))
            ConfigManager.WriteConsoleWarning($"[PutOnFloor.Start] can't re-position cabinet on floor {name}");
        yield break;
    }
}