#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class ClearListEditor : Editor
{
    [MenuItem("Custom/Free CabinetController ToUnload zone List")]
    private static void ClearList()
    {
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject != null)
        {
            ClearAgentPlayerPositionsToUnload(selectedObject);
            EditorUtility.SetDirty(selectedObject);
        }
        else
        {
            Debug.LogWarning("Please select a GameObject first.");
        }
    }

    private static void ClearAgentPlayerPositionsToUnload(GameObject parentObject)
    {
        CabinetController[] cabinetControllers = parentObject.GetComponentsInChildren<CabinetController>(true);

        foreach (var controller in cabinetControllers)
        {
            if (controller != null)
            {
                controller.AgentPlayerPositionsToUnload.Clear();
                Debug.Log("Cleared AgentPlayerPositionsToUnload List in CabinetController attached to: " + controller.gameObject.name);
            }
        }
    }
}
#endif


