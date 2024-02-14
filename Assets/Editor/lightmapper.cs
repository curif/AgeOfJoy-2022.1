using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEditor.SceneManagement; // This is required for EditorSceneManager

public class BuildLightingAndOcclusionForBuildScenes : EditorWindow
{
    [MenuItem("Tools/Build Lighting and Occlusion for Build Scenes")]
    public static void BuildForBuildScenes()
    {
        // Get scenes from the build settings
        List<string> buildScenes = new List<string>();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                buildScenes.Add(scene.path);
            }
        }

        // Check if there are enabled scenes in build settings
        if (buildScenes.Count == 0)
        {
            Debug.LogError("No scenes are marked for inclusion in the build settings.");
            return;
        }

        for (int i = 0; i < buildScenes.Count; i++)
        {
            string scenePath = buildScenes[i];
            // Open the scene
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            float progress = (float)i / buildScenes.Count;
            string progressMessage = $"Processing {SceneManager.GetActiveScene().name} ({i + 1}/{buildScenes.Count})";

            // Display progress for lighting bake
            EditorUtility.DisplayProgressBar("Building Lighting and Occlusion",
                $"{progressMessage}\nBaking lighting...",
                progress);

            // Bake lighting
            Lightmapping.Bake();

            // Display progress for occlusion compute
            EditorUtility.DisplayProgressBar("Building Lighting and Occlusion",
                $"{progressMessage}\nComputing occlusion...",
                progress + 0.5f / buildScenes.Count); // Slightly increase progress for occlusion

            StaticOcclusionCulling.Compute();

            // Save changes made during the bake
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            EditorUtility.ClearProgressBar();
        }

        Debug.Log("Finished building lighting and occlusion for build scenes.");
    }
}
