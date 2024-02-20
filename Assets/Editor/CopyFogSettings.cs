using UnityEngine;
using UnityEditor;

// Adds functionality to auto-copy current level's fog settings to "Fog Settings" gameobject
[CustomEditor(typeof(SceneFogSettings))]
public class FogSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draws the default inspector

        SceneFogSettings script = (SceneFogSettings)target;

        if (GUILayout.Button("Copy Global Fog Settings"))
        {
            // Copy the current global fog settings
            script.settings.fogEnabled = RenderSettings.fog;
            script.settings.fogColor = RenderSettings.fogColor;
            script.settings.fogMode = RenderSettings.fogMode;
            script.settings.fogDensity = RenderSettings.fogDensity;
            script.settings.startDistance = RenderSettings.fogStartDistance;
            script.settings.endDistance = RenderSettings.fogEndDistance;

            EditorUtility.SetDirty(script); // Mark the script as dirty so the changes are saved
            Debug.Log("Global Fog Settings copied to FogSettings object.");
        }
    }
}
