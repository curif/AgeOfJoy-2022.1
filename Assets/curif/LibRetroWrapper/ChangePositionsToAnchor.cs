#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ChangePositionsToAnchor : Editor
{
    [MenuItem("Custom/Convert Positions to Anchor", true)]
    private static bool CanConvertPositionsToAnchor()
    {
        // Enable the menu only if there is at least one selected object with the AgentScenePosition component
        return Selection.GetFiltered<AgentScenePosition>(SelectionMode.TopLevel).Length > 0;
    }

    [MenuItem("Custom/Convert Positions to Anchor")]
    private static void ConvertPositionsToAnchor()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        foreach (GameObject selectedObject in selectedObjects)
        {
            AgentScenePosition agentPosition = selectedObject.GetComponent<AgentScenePosition>();
            if (agentPosition != null)
            {
                // Create a new Plane GameObject with the same name and parent
                GameObject anchorObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
                anchorObject.name = agentPosition.name + " Anchor";
                anchorObject.transform.SetParent(agentPosition.transform.parent);
                anchorObject.transform.position = agentPosition.transform.position;
                anchorObject.transform.rotation = agentPosition.transform.rotation;

                // Set the scale of the anchorObject to 0.05 in the X and Z dimensions
                anchorObject.transform.localScale = new Vector3(0.05f, 1f, 0.05f);

                // Destroy the default mesh collider since we'll be adding a new one
                DestroyImmediate(anchorObject.GetComponent<MeshCollider>());

                // Copy the AgentScenePosition component to the new object
                AgentScenePosition newAgentPosition = anchorObject.AddComponent<AgentScenePosition>();
                newAgentPosition.IsPlayerPresent = agentPosition.IsPlayerPresent;
                newAgentPosition.IsNPCPresent = agentPosition.IsNPCPresent;
                newAgentPosition.NPCPresentName = agentPosition.NPCPresentName;
                newAgentPosition.BoxColliderHeight = agentPosition.BoxColliderHeight;

                // Copy the BoxCollider to the new object
                BoxCollider collider = agentPosition.GetComponent<BoxCollider>();
                if (collider != null)
                {
                    BoxCollider newCollider = anchorObject.GetComponent<BoxCollider>();
                    if (newCollider == null)
                        newCollider = anchorObject.AddComponent<BoxCollider>();

                    newCollider.center = Vector3.zero;
                    newCollider.size = new Vector3(10f, 1f, 10f);
                    newCollider.isTrigger = collider.isTrigger;
                }

                // Add MeshCollider to the new object
                MeshCollider meshCollider = anchorObject.AddComponent<MeshCollider>();

                // Add TeleportationAnchor to the new object
                TeleportationAnchor teleportationAnchor = anchorObject.AddComponent<TeleportationAnchor>();

                // Disable the MeshRenderer component on the new object
                MeshRenderer meshRenderer = anchorObject.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    meshRenderer.enabled = false;
                }

                // Disable the original GameObject
                selectedObject.SetActive(false);
            }
        }
    }
}

#endif
