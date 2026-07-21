using UnityEngine;
using UnityEditor;
// Adds an Inspector button to teleport to the Workshop scene while testing in Play mode.
[CustomEditor(typeof(TeleportationController))]
public class TeleportationControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TeleportationController controller = (TeleportationController)target;

        if (GUILayout.Button("Teleport To Workshop"))
        {
            controller.EditorTeleportToWorkshop();
        }
    }
}
