using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class CreateAgentPosition : Editor
{
    [MenuItem("Custom/Cabinet: Create Agent Position", false, 10)]
    static void CreateAgentPositionObject()
    {
        // Check if there's a selected object
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            Debug.LogError("Please select an object first!");
            return;
        }

        // Create new object named "AgentPosition"
        GameObject agentPosition = new GameObject("AgentPosition");
        agentPosition.name = "Agent position " + selectedObject.name;

        // Parent it to the selected object for cleaner hierarchy
        //agentPosition.transform.SetParent(selectedObject.transform.parent);

        // Add BoxCollider
        BoxCollider boxCollider = agentPosition.AddComponent<BoxCollider>();
        boxCollider.center = new Vector3(0,0,0);
        boxCollider.size = new Vector3(0.5f, 0.5f, 0.5f);
        boxCollider.isTrigger = true;
        // Set Layer Overrides - Include only "player" and "NPC" layers
        int playerLayer = LayerMask.NameToLayer("Player");
        int npcLayer = LayerMask.NameToLayer("NPC");
        if (playerLayer == -1 || npcLayer == -1)
        {
            Debug.LogWarning("Please define 'player' and 'NPC' layers in Project Settings!");
        }
        else
        {
            // Create LayerMask for Include Layers
            LayerMask includeLayers = (1 << playerLayer) | (1 << npcLayer);
            boxCollider.includeLayers = includeLayers;
        }

        // Disable renderer (if any exists)
        MeshRenderer renderer = agentPosition.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }

        // Position it 1 unit from the Z of the original object
        Vector3 originalPosition = selectedObject.transform.position;
        agentPosition.transform.position = originalPosition + new Vector3(0, 0, 0.7f);

        // Make it face the selected object
        agentPosition.transform.LookAt(selectedObject.transform);

        // playerPosition layer
        agentPosition.layer = 7;

        // Add AgentScenePosition script
        AgentScenePosition agScript = agentPosition.AddComponent<AgentScenePosition>();


        // Get the CabinetController from selected object and assign the new object
        CabinetController cabinetController = selectedObject.GetComponent<CabinetController>();
        if (cabinetController != null)
        {
            cabinetController.AgentScenePosition = agScript;
        }
        else
        {
            Debug.LogWarning("Selected object doesn't have a CabinetController component!");
        }

        // Set the new object as the active selection
        Selection.activeGameObject = agentPosition;
    }

    // Validation to enable/disable the menu item
    [MenuItem("GameObject/Create Agent Position", true)]
    static bool ValidateCreateAgentPositionObject()
    {
        return Selection.activeGameObject != null;
    }
}
#endif