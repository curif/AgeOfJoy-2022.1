#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class PlaceOnFloor : Editor
{
    [MenuItem("Custom/Place on Floor")]
    private static void PlaceSelectedOnFloor()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        foreach (GameObject selectedObject in selectedObjects)
        {
            Transform floor = DetectFloor(selectedObject);
            if (floor != null)
            {
                float floorHeight = 0.1f;
                float yOffset = Mathf.Max(0, floor.position.y); // Account for floor position above or below Y zero
                Vector3 newPosition = new Vector3(selectedObject.transform.position.x, 
                                                    floor.position.y + floorHeight / 2 + yOffset, 
                                                    selectedObject.transform.position.z);
                selectedObject.transform.position = newPosition;
            }
        }
    }

    private static Transform DetectFloor(GameObject selectedObject)
    {
        RaycastHit hit;
        Ray ray = new Ray(selectedObject.transform.position, Vector3.down);

        if (Physics.Raycast(ray, out hit, 1000))
        {
            return hit.transform;
        }

        return null;
    }
}
#endif
