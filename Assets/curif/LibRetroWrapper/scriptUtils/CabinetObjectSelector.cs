#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class CabinetObjectSelector
{
    [MenuItem("Custom/Select elements in Cabinets to move it")]
    private static void SelectObjectsInCabinetController()
    {
        GameObject[] selectedGameObjects = Selection.gameObjects;

        foreach (GameObject selectedObject in selectedGameObjects)
        {
            GameObject[] selectedObjects = new GameObject[3];
            CabinetController cabinetController = selectedObject.GetComponent<CabinetController>();
            
            selectedObjects[0] = cabinetController.gameObject;

            if (cabinetController != null)
                selectedObjects[1] = cabinetController.AgentPlayerTeleportAnchor;

            if (cabinetController.AgentScenePosition != null)
                selectedObjects[2] = cabinetController.AgentScenePosition.gameObject;

            AddToSelection(selectedObjects);
        }
    
    }

    
    static void AddToSelection(GameObject[] objectsToAdd)
    {
        // Get the currently selected objects in the editor
        GameObject[] selectedObjects = Selection.gameObjects;

        // Create a new list to hold the combined selection
        List<GameObject> combinedSelection = new List<GameObject>(selectedObjects);

        // Iterate through objectsToAdd, adding non-null objects to the list
        for (int i = 0; i < objectsToAdd.Length; i++)
        {
            if (objectsToAdd[i] != null)
            {
                combinedSelection.Add(objectsToAdd[i]);
            }
        }

        // Set the list as the new selection
        Selection.objects = combinedSelection.ToArray();

        // Optionally, you can focus the Scene view camera on the new objects
        SceneView.lastActiveSceneView.FrameSelected();
    }
}

#endif
