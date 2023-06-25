#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

public static class CabinetObjectSelector
{
    [MenuItem("Custom/Select Objects in Cabinet Controller")]
    private static void SelectObjectsInCabinetController()
    {
        CabinetController cabinetController = Selection.activeGameObject.GetComponent<CabinetController>();
        Object[] selectedObjects = new Object[3];
        selectedObjects[0] = cabinetController.gameObject;

        if (cabinetController != null)
        {
            selectedObjects[1] = cabinetController.AgentPlayerTeleportAnchor;
        }

        if (cabinetController.AgentScenePosition != null)
            selectedObjects[2] = cabinetController.AgentScenePosition.gameObject;
        
        Selection.objects = selectedObjects;
    }
}

#endif
