#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using Unity.AI.Navigation;

public class NavMeshModifierRemover : EditorWindow
{
    [MenuItem("Custom/Remove NavMeshModifiers Recursively")]
    private static void RemoveNavMeshModifiersRecursively()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        foreach (GameObject selectedObject in selectedObjects)
        {
            RemoveNavMeshModifierRecursively(selectedObject);
        }
    }

    private static void RemoveNavMeshModifierRecursively(GameObject gameObject)
    {
        NavMeshModifier navMeshModifier = gameObject.GetComponent<NavMeshModifier>();
        if (navMeshModifier != null)
        {
            Component.DestroyImmediate(navMeshModifier);
        }

        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            GameObject child = gameObject.transform.GetChild(i).gameObject;
            RemoveNavMeshModifierRecursively(child);
        }
    }
}
#endif