using UnityEngine;
using UnityEditor;
// Adds functionality to auto-copy current level's fog settings to "FogTriggerZone" gameobject
[CustomEditor(typeof(FogTrigger))]
public class FogTriggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draws the default inspector

        FogTrigger script = (FogTrigger)target;

        if (GUILayout.Button("Copy Global Fog Settings"))
        {
            // Copy the current global fog settings to the FogTrigger's fogSettings
            script.fogSettings.fogEnabled = RenderSettings.fog;
            script.fogSettings.fogColor = RenderSettings.fogColor;
            script.fogSettings.fogMode = RenderSettings.fogMode;
            script.fogSettings.fogDensity = RenderSettings.fogDensity;
            script.fogSettings.startDistance = RenderSettings.fogStartDistance;
            script.fogSettings.endDistance = RenderSettings.fogEndDistance;

            EditorUtility.SetDirty(script); // Mark the script as dirty so the changes are saved
            Debug.Log("Global Fog Settings copied to FogTrigger's FogSettings object.");
        }
    }
}
