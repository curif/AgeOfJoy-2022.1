#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

public class SetChildrenStatic : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Set Children Static")]
    private static void SetChildrenAsStatic()
    {
        GameObject[] selection = Selection.gameObjects;
        foreach (GameObject parent in selection)
        {
            // Set this object and all its children as static
            parent.isStatic = true;
            SetChildrenStaticRecursive(parent.transform);
        }
    }

    private static void SetChildrenStaticRecursive(Transform parent)
    {
        foreach (Transform child in parent)
        {
            // Set child as static
            child.gameObject.isStatic = true;

            // Turn off light probes and reflection probes
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            }

            // Set child mesh as static
            MeshFilter meshFilter = child.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(child.gameObject);
                flags |= StaticEditorFlags.BatchingStatic;
                GameObjectUtility.SetStaticEditorFlags(child.gameObject, flags);
            }

            // Recursively set children as static
            SetChildrenStaticRecursive(child);
        }
    }
#endif
}

