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
        if (!AlignLowerPartToFloor(transform, boxCollider))
        {
            Vector3 reservedPosition = transform.position;
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y + 0.5f,
                transform.position.z
            );
            if (!AlignLowerPartToFloor(transform, boxCollider))
            {
                transform.position = reservedPosition;
                return false;
            }
        }
        return true;
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
    private static bool AlignLowerPartToFloor(Transform transform, BoxCollider boxCollider)
    {

        // Cast a ray downwards from the center of the BoxCollider
        Vector3 lowBoxCollider = new Vector3(transform.position.x,
                                                CalculateLowerPointY(transform, boxCollider) + 0.5f,
                                                transform.position.z);
        Ray ray = new Ray(lowBoxCollider, Vector3.down);

        //CreateSphere(lowBoxCollider, transform.gameObject);

        // Perform a raycast to check for the floor
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1f, floorLayer))
        {
            float yOffset = lowBoxCollider.y - hit.point.y - 0.5f;
            
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

        if (!PlaceOnFloorFromBoxCollider.PlaceOnFloor(gameObject.transform, boxCollider))
            ConfigManager.WriteConsoleError($"[PutOnFloor.Start] can't re-position cabinet on floor after two intents {name}");
    }

}