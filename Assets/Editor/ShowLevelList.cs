using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class ScenesInBuildSelector : EditorWindow
{
    private Vector2 scrollPosition;
    private List<string> sceneNames;
    private List<string> scenePaths;
    private List<bool> sceneSelections;

    [MenuItem("Tools/Select Scenes for Build and Lightmapping")]
    private static void ShowWindow()
    {
        var window = GetWindow<ScenesInBuildSelector>("Select Scenes for Build and Lightmapping");
        window.Init();
    }

    private void Init()
    {
        sceneNames = new List<string>();
        scenePaths = new List<string>();
        sceneSelections = new List<bool>();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                string sceneName = Path.GetFileNameWithoutExtension(scene.path);
                sceneNames.Add(sceneName);
                scenePaths.Add(scene.path);
                sceneSelections.Add(false);
            }
        }

        SortScenesAlphabetically();
    }

    private void SortScenesAlphabetically()
    {
        var scenesWithPaths = sceneNames.Zip(scenePaths, (name, path) => new { Name = name, Path = path })
                                        .OrderBy(scene => scene.Name)
                                        .ToList();

        sceneNames = scenesWithPaths.Select(scene => scene.Name).ToList();
        scenePaths = scenesWithPaths.Select(scene => scene.Path).ToList();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        for (int i = 0; i < sceneNames.Count; i++)
        {
            sceneSelections[i] = EditorGUILayout.ToggleLeft(sceneNames[i], sceneSelections[i]);
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Select All"))
        {
            SelectDeselectAll(true);
        }

        if (GUILayout.Button("Select None"))
        {
            SelectDeselectAll(false);
        }

        if (GUILayout.Button("BUILD"))
        {
            BuildSelectedScenes();
        }
    }

    private void SelectDeselectAll(bool selectAll)
    {
        for (int i = 0; i < sceneSelections.Count; i++)
        {
            sceneSelections[i] = selectAll;
        }
    }

    private void BuildSelectedScenes()
    {
        // Your build logic here
    }
}
