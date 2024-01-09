#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class RenameSelectedObjects : EditorWindow
{
    private string baseName = "ObjectName";

    [MenuItem("Custom/Rename Selected Objects")]
    static void Init()
    {
        RenameSelectedObjects window = (RenameSelectedObjects)EditorWindow.GetWindow(typeof(RenameSelectedObjects));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Base Name:", EditorStyles.boldLabel);
        baseName = EditorGUILayout.TextField(baseName);

        if (GUILayout.Button("Rename Selected"))
        {
            RenameObjects();
        }
    }

    void RenameObjects()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length > 0)
        {
            for (int i = 0; i < selectedObjects.Length; i++)
            {
                Undo.RecordObject(selectedObjects[i], "Rename Object");

                // Append serial number to the base name
                string newName = baseName + (i + 1);
                selectedObjects[i].name = newName;
            }
        }
        else
        {
            Debug.Log("No objects selected. Please select at least one object to rename.");
        }
    }
}
#endif