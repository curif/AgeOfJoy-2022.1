using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit;

#if UNITY_EDITOR

[CustomEditor(typeof(CabinetController))]
public class CabinetEditor : Editor
{
    [MenuItem("Custom/Cabinets: Create Teleport Anchor", false, 10)]
    static void CreateTeleportAnchor()
    {
        // Check if an object is selected
        if (Selection.activeGameObject == null)
        {
            Debug.LogError("Please select a cabinet object first!");
            return;
        }

        GameObject selectedObject = Selection.activeGameObject;

        // Verify the selected object has CabinetController
        CabinetController cabinetController = selectedObject.GetComponent<CabinetController>();
        if (cabinetController == null)
        {
            Debug.LogError("Selected object must have a CabinetController component!");
            return;
        }

        // Create new teleport anchor object
        GameObject teleportAnchor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        teleportAnchor.name = "PlayerTeleportAnchor " + selectedObject.name;
        teleportAnchor.layer = 9; //teleport area

        // Add MeshFilter with plane mesh
        //MeshFilter meshFilter = teleportAnchor.AddComponent<MeshFilter>();
        //meshFilter.sharedMesh = CreatePlaneMesh();

        // Parent it to the selected object
        //teleportAnchor.transform.SetParent(selectedObject.transform);

        // Set scale
        teleportAnchor.transform.localScale = new Vector3(0.05f, 1f, 0.05f);

        // Position it 1 unit from the Z of the original object
        Vector3 originalPosition = selectedObject.transform.position;
        teleportAnchor.transform.position = originalPosition - new Vector3(0, 0, 0.7f);

        // Make forward point towards selected object
        teleportAnchor.transform.LookAt(selectedObject.transform);

        // Add and configure MeshCollider
        //MeshCollider meshCollider = teleportAnchor.GetComponent<MeshCollider>();

        // Add and configure TeleportationAnchor
        TeleportationAnchor teleportComponent = teleportAnchor.AddComponent<TeleportationAnchor>();
        teleportComponent.matchOrientation = MatchOrientation.TargetUpAndForward;
        //teleportComponent.teleportTrigger = TeleportTrigger.OnSelectExited;

        MeshRenderer renderer = teleportAnchor.GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.enabled = false;

        // Assign to CabinetController's AgentPlayerTeleportAnchor property
        cabinetController.AgentPlayerTeleportAnchor = teleportAnchor;

        // Mark the scene as dirty to ensure changes are saved
        EditorUtility.SetDirty(cabinetController);
        EditorUtility.SetDirty(teleportAnchor);

        // Select the new object in the hierarchy
        Selection.activeGameObject = teleportAnchor;

        Debug.Log("Teleport Anchor created successfully!");
    }

    // Ensure the menu item is only enabled when appropriate
    [MenuItem("Custom/Cabinets/Create Teleport Anchor", true)]
    static bool ValidateCreateTeleportAnchor()
    {
        return Selection.activeGameObject != null &&
               Selection.activeGameObject.GetComponent<CabinetController>() != null;
    }
}

#endif