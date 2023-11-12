#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class SetMaterialUtility : EditorWindow
{
    private Material materialToSet;

    [MenuItem("Custom/Set Material")]
    private static void ShowWindow()
    {
        SetMaterialUtility window = GetWindow<SetMaterialUtility>();
        window.titleContent = new GUIContent("Set Material");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Material to Set", EditorStyles.boldLabel);
        materialToSet = (Material)EditorGUILayout.ObjectField(materialToSet, typeof(Material), false);

        if (GUILayout.Button("Set Material"))
        {
            AssignMaterialToSelectedObjects();
        }
    }

    private void AssignMaterialToSelectedObjects()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        foreach (GameObject obj in selectedObjects)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = materialToSet;
            }
        }
    }
}
#endif
