#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class PlaceOnFloor : Editor
{
    [MenuItem("Custom/Adjust Cabinets childrens")]
    private static void AdjustCabinetChildrens()
    {

        GameObject cabinets = Selection.activeGameObject;
        if (cabinets == null)
        {
            Debug.LogError("No game object selected!");
            return;
        }

        cabinets.isStatic = true;
            
        foreach(Transform transform in cabinets.transform)
        {
            // Get the BoxCollider component attached to the GameObject
            BoxCollider boxCollider = transform.gameObject.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                ConfigManager.WriteConsoleError($"[PutOnFloor.AdjustCabinetChildrens] there is not a boxCollider for {transform.gameObject}");
                continue;
            }

            // in PutOnFloor.cs
            if (! PlaceOnFloorFromBoxCollider.PlaceOnFloor(transform, boxCollider))
                ConfigManager.WriteConsoleError($"[PutOnFloor.Start] can't re-position cabinet on floor after two intents {transform.gameObject}");

            // Set the child GameObject and its children recursively as static
            transform.gameObject.isStatic = true;
            SetObjectsStatic(transform);
        
        }

        var scene = SceneManager.GetActiveScene (); 
        EditorSceneManager.MarkSceneDirty (scene);
    }

    private static void SetObjectsStatic(Transform parent)
    {
        foreach (Transform child in parent)
        {
            // Set the child GameObject as static
            child.gameObject.isStatic = true;

            // Set the child GameObject and its children recursively as static
            SetObjectsStatic(child);
        }
    }
}
#endif
