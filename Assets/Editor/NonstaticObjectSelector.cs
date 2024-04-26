using UnityEngine;
using UnityEditor;

public class NonStaticObjectSelector
{
    [MenuItem("Tools/Select Non-Static GameObjects")]
    private static void SelectNonStaticGameObjects()
    {
        // Fetch all GameObjects in the scene
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        // Filter to get only non-static objects
        var nonStaticObjects = new System.Collections.Generic.List<GameObject>();
        foreach (var obj in allObjects)
        {
            if (!obj.isStatic && obj.hideFlags == HideFlags.None)  // Ensure object is not static and part of the scene
            {
                nonStaticObjects.Add(obj);
            }
        }

        // Select the non-static GameObjects in the editor
        Selection.objects = nonStaticObjects.ToArray();

        // Optionally log the selected objects
        foreach (var obj in nonStaticObjects)
        {
            Debug.Log("Selected Non-Static GameObject: " + obj.name);
        }
    }
}
