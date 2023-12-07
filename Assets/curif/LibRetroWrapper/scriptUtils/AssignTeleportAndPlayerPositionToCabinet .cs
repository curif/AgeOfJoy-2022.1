#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class AssignTeleportAndPlayerPositionToCabinet : EditorWindow
{
    private GameObject cabinetObject;
    private GameObject teleportAnchorObject;
    private GameObject agentScenePositionObject;

    [MenuItem("Custom/Assign Teleport and Player Position to Cabinet")]
    private static void Init()
    {
        AssignTeleportAndPlayerPositionToCabinet window = GetWindow<AssignTeleportAndPlayerPositionToCabinet>();
        window.titleContent = new GUIContent("Assign Teleport and Player Position");
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Assign Teleport and Player Position to Cabinet", EditorStyles.boldLabel);

        cabinetObject = EditorGUILayout.ObjectField("Cabinet GameObject", cabinetObject, typeof(GameObject), true) as GameObject;
        teleportAnchorObject = EditorGUILayout.ObjectField("Teleportation Anchor GameObject", teleportAnchorObject, typeof(GameObject), true) as GameObject;
        agentScenePositionObject = EditorGUILayout.ObjectField("Agent Scene Position GameObject", agentScenePositionObject, typeof(GameObject), true) as GameObject;

        GUILayout.Space(10);

        if (GUILayout.Button("Assign"))
        {
            AssignComponents();
        }
    }
    private void AssignComponents()
    {
        if (ValidateInputs())
        {
            CabinetController cabinetController = cabinetObject.GetComponent<CabinetController>();
            AgentScenePosition agentScenePosition = agentScenePositionObject.GetComponent<AgentScenePosition>();

            if (cabinetController != null && agentScenePosition != null)
            {
                SerializedObject cabinetSerializedObject = new SerializedObject(cabinetController);

                SerializedProperty teleportAnchorProperty = cabinetSerializedObject.FindProperty("AgentPlayerTeleportAnchor");
                SerializedProperty agentScenePositionProperty = cabinetSerializedObject.FindProperty("AgentScenePosition");

                teleportAnchorProperty.objectReferenceValue = teleportAnchorObject;
                agentScenePositionProperty.objectReferenceValue = agentScenePosition;

                cabinetSerializedObject.ApplyModifiedProperties();

               // Set the position and rotation
                agentScenePositionObject.transform.position = teleportAnchorObject.transform.position;
                agentScenePositionObject.transform.rotation = teleportAnchorObject.transform.rotation;

                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

                Debug.Log("Assignment complete!");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "One or more required components not found on the specified GameObjects.", "OK");
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "One or more GameObject references are null.", "OK");
        }
    }

    private bool ValidateInputs()
    {
        if (cabinetObject == null || teleportAnchorObject == null || agentScenePositionObject == null)
        {
            return false;
        }

        // Add more validation if needed (e.g., check types, scene validation)

        return true;
    }
}
#endif
