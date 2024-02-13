using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class ScenesInBuildSelector : EditorWindow
{
    private Vector2 scrollPosition;
    private List<string> sceneNames;
    private List<string> scenePaths; // To store the full paths of the scenes
    private List<bool> sceneSelections;

    [MenuItem("Tools/Build Lightmaps and Occlusion")]
    private static void ShowWindow()
    {
        var window = GetWindow<ScenesInBuildSelector>("Build Lightmaps and Occlusion");
        window.Init();
    }

    private void Init()
    {
        // Initialize lists
        sceneNames = new List<string>();
        scenePaths = new List<string>();
        sceneSelections = new List<bool>();

        // Populate lists
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                sceneNames.Add(Path.GetFileNameWithoutExtension(scene.path));
                scenePaths.Add(scene.path);
                sceneSelections.Add(false);
            }
        }

        // Sort scenes alphabetically by name
        SortScenesAlphabetically();
    }

    private void SortScenesAlphabetically()
    {
        var sortedScenes = sceneNames.Zip(scenePaths, (name, path) => new { name, path })
                                     .OrderBy(scene => scene.name)
                                     .ToList();

        sceneNames = sortedScenes.Select(scene => scene.name).ToList();
        scenePaths = sortedScenes.Select(scene => scene.path).ToList();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Select Scenes to build Lightmapping and Occlusion", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (int i = 0; i < sceneNames.Count; i++)
        {
            sceneSelections[i] = EditorGUILayout.ToggleLeft(sceneNames[i], sceneSelections[i]);
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Select All"))
        {
            for (int i = 0; i < sceneSelections.Count; i++)
            {
                sceneSelections[i] = true;
            }
        }

        if (GUILayout.Button("Select None"))
        {
            for (int i = 0; i < sceneSelections.Count; i++)
            {
                sceneSelections[i] = false;
            }
        }

        if (GUILayout.Button("Build"))
        {
            BuildSelectedScenes();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void BuildSelectedScenes()
    {
        for (int i = 0; i < sceneSelections.Count; i++)
        {
            if (sceneSelections[i])
            {
                string scenePath = scenePaths[i];
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                Scene scene = SceneManager.GetSceneByPath(scenePath);
                if (scene.IsValid())
                {
                    Debug.Log($"Processing {scene.name}");

                    Lightmapping.Bake(); // Perform lightmapping
                    StaticOcclusionCulling.Compute(); // Perform occlusion culling

                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"{scene.name} processed and saved.");
                }
            }
        }
        Debug.Log("All selected scenes processed.");
    }
}
