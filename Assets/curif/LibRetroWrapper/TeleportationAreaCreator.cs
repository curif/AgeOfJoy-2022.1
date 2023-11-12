#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleportationAreaCreator : Editor
{
    [MenuItem("Custom/Create Teleportation Areas")]
    private static void CreateTeleportationAreas()
    {
        GateController[] gateControllers = FindObjectsOfType<GateController>();

        foreach (GateController gateController in gateControllers)
        {
            // Skip GameObjects that already have a "Teleport" suffix in their name
            if (gateController.name.EndsWith("Teleport"))
                continue;

            // Create a new plane GameObject with the same name and parent
            GameObject planeObject = new GameObject(gateController.name + " Teleport");
            planeObject.transform.SetParent(gateController.transform.parent);
            planeObject.transform.position = new Vector3(gateController.transform.position.x, 
                                                        gateController.transform.position.y /*- gateController.GetComponent<Collider>().bounds.extents.y*/, 
                                                        gateController.transform.position.z);
            planeObject.transform.rotation = gateController.transform.rotation;

            // Create a custom plane mesh with the size of the BoxCollider
            MeshFilter meshFilter = planeObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = planeObject.AddComponent<MeshRenderer>();
            meshRenderer.enabled = false;

            // Get the collider from the original object
            Collider collider = gateController.GetComponent<Collider>();
            if (collider != null && collider is BoxCollider boxCollider)
            {
                // Create a BoxCollider on the new object
                BoxCollider newCollider = planeObject.AddComponent<BoxCollider>();

                // Copy the properties from the original BoxCollider
                newCollider.center = boxCollider.center;
                newCollider.size = new Vector3(boxCollider.size.x, boxCollider.size.y, boxCollider.size.z);
                newCollider.isTrigger = boxCollider.isTrigger;

                // Create a custom plane mesh with the size of the BoxCollider
                Mesh planeMesh = CreatePlaneMesh(boxCollider.size.x, boxCollider.size.z);
                meshFilter.mesh = planeMesh;
            }

            // Add TeleportationArea component
            TeleportationArea teleportationArea = planeObject.AddComponent<TeleportationArea>();

            // Attach the GateController component to the plane GameObject and copy properties
            GateController newGateController = planeObject.AddComponent<GateController>();
            newGateController.ScenesToLoad = gateController.ScenesToLoad;
            newGateController.ScenesToUnload = gateController.ScenesToUnload;
            newGateController.SceneBlockers = gateController.SceneBlockers;

            // Enable the GateController component on the new object
            newGateController.enabled = true;

            // Disable the original GameObject
            //gateController.gameObject.SetActive(false);
        }
    }

    private static Mesh CreatePlaneMesh(float width, float length)
    {
        Mesh mesh = new Mesh();

        // Define the vertices of the plane
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-width / 2f, 0f, -length / 2f),
            new Vector3(-width / 2f, 0f, length / 2f),
            new Vector3(width / 2f, 0f, length / 2f),
            new Vector3(width / 2f, 0f, -length / 2f)
        };

        // Define the triangles of the plane
        int[] triangles = new int[6] { 0, 1, 2, 0, 2, 3 };

        // Assign the vertices and triangles to the mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        return mesh;
    }
}

#endif
