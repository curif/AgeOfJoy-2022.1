#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

public class DoorAligner : MonoBehaviour
{
    public string roomToAlignName;
    public string doorToAlignName;

    public GameObject RoomToAttachTo;
    public void AlignRoom()
    {
        // Find the room object to align
        GameObject roomToAlignObject = GameObject.Find(roomToAlignName);

        if (roomToAlignObject != null)
        {
            // Find the door object to align
            GameObject doorToAlignObject = GameObject.Find(doorToAlignName);

            if (doorToAlignObject != null)
            {
                // Align the room using the provided Align method
                // Align(parentTransform, childTransform, doorToAlignObject.transform);
                AlignmentOld(roomToAlignObject.transform, doorToAlignObject.transform,
                            RoomToAttachTo.transform, transform);
                // Mark the scene as dirty to apply the changes
                EditorUtility.SetDirty(roomToAlignObject);
            }
            else
            {
                Debug.LogError("Door to align object not found in the scene.");
            }
        }
        else
        {
            Debug.LogError("Room object to align not found in the scene.");
        }
    }
    private static void Align(Transform target, Transform targetChild, Transform source)
    {
        target.rotation = source.rotation * Quaternion.Inverse(Quaternion.Inverse(target.rotation) * targetChild.rotation);
        target.position = source.position + (target.position - targetChild.position);
    }
    private static void AlignBase(Transform target, Transform targetChild, Transform source)
    {
        if (target && targetChild && source)
        {
            target.rotation = source.rotation * Quaternion.Inverse(Quaternion.Inverse(target.rotation) * targetChild.rotation);
            target.position = source.position + (target.position - targetChild.position);
        }
    }

    void Alignment(Transform roomToMove, Transform doorToMove, Transform roomToAttach, Transform doorToAttach)
    {
                Vector3 v1 = roomToMove.position - doorToMove.position;
        Vector3 v2 = doorToAttach.position - roomToAttach.position;
        roomToMove.rotation *= Quaternion.FromToRotation(v1, v2);
        roomToMove.position = doorToAttach.position + v2.normalized * v1.magnitude;
        roomToMove.rotation = roomToAttach.rotation;//.FromToRotation(v1, v2);

    }

     void AlignmentOld(Transform roomToMove, Transform doorToMove, Transform roomToAttach, Transform doorToAttach)
    {
        Vector3 v1 = roomToMove.position - doorToMove.position;
        Vector3 v2 = doorToAttach.position - roomToAttach.position;
        roomToMove.rotation *= Quaternion.FromToRotation(v1, v2);
        roomToMove.position = doorToAttach.position + v2.normalized * v1.magnitude;
    }
}

[CustomEditor(typeof(DoorAligner))]
public class DoorAlignerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DoorAligner doorAligner = (DoorAligner)target;

        if (GUILayout.Button("Align Room"))
        {
            doorAligner.AlignRoom();
        }
    }
}
#endif