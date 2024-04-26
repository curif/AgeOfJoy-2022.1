using UnityEngine;
using UnityEditor;

public class NonStaticObjectSelector
{
    [MenuItem("Tools/Select Non-Static GameObjects")]
    private static void SelectNonStaticGameObjects()
    {
        // Create a list to hold non-static GameObjects
        var nonStaticObjects = new System.Collections.Generic.List<GameObject>();

        // Iterate over all GameObjects in the scene
        foreach (GameObject obj in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        {
            // Check if the GameObject is not static
            if (!obj.isStatic)
            {
                // Make sure the object is part of the scene (not a prefab or unused asset)
                if (obj.hideFlags == HideFlags.None)
                {
                    nonStaticObjects.Add(obj);
                }
            }
        }

        // Select the non-static GameObjects in the editor
        Selection.objects = nonStaticObjects.ToArray();

        // Optional: Log the names of selected objects
        foreach (GameObject obj in nonStaticObjects)
        {
            Debug.Log("Selected Non-Static GameObject: " + obj.name);
        }
    }
}
