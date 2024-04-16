#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public class RunUtility : EditorWindow
{
    [MenuItem("Custom/Run")]
    static void Run()
    {

        // Prompt to save any modified scenes
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return; // If user cancels, do nothing

        // Unload all scenes
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Load the FixedScene
        EditorSceneManager.OpenScene("Assets/Scenes/FixedScene.unity");

        // Run the game
        EditorApplication.isPlaying = true;
    }
}
#endif