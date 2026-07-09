#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Custom/MR menu (last item under Custom) for MRRuntimeSettings.mrFeatureEnabled on FixedScene.
/// </summary>
static class MRFeatureToggleEditor
{
    const string FixedScenePath = "Assets/Scenes/FixedScene.unity";
    const string DisableMenuPath = "Custom/MR/Disable Mixed Reality";
    const string EnableMenuPath = "Custom/MR/Enable Mixed Reality";
    const int MenuPriority = 10000;

    [MenuItem(DisableMenuPath, false, MenuPriority)]
    static void DisableMrForRelease() => SetMrFeatureEnabled(false);

    [MenuItem(DisableMenuPath, true)]
    static bool DisableMrValidate() => ReadMrFeatureEnabled();

    [MenuItem(EnableMenuPath, false, MenuPriority)]
    static void EnableMrForDevelopment() => SetMrFeatureEnabled(true);

    [MenuItem(EnableMenuPath, true)]
    static bool EnableMrValidate() => !ReadMrFeatureEnabled();

    static void SetMrFeatureEnabled(bool enabled)
    {
        MRRuntimeSettings settings = FindSettingsInFixedScene(loadSceneIfNeeded: true);
        if (settings == null)
        {
            EditorUtility.DisplayDialog(
                "Age of Joy MR",
                $"MRRuntimeSettings not found in {FixedScenePath}.",
                "OK");
            return;
        }

        string undoLabel = enabled ? "Enable MR" : "Disable MR";
        Undo.RecordObject(settings, undoLabel);
        settings.mrFeatureEnabled = enabled;
        EditorUtility.SetDirty(settings);
        EditorSceneManager.MarkSceneDirty(settings.gameObject.scene);

        string state = enabled ? "enabled" : "disabled";
        if (EditorSceneManager.SaveScene(settings.gameObject.scene))
        {
            Debug.Log($"[MRFeatureToggle] Mixed Reality {state} — FixedScene saved.");
        }
        else
        {
            Debug.LogWarning($"[MRFeatureToggle] Mixed Reality {state} but FixedScene was not saved.");
        }
    }

    static bool ReadMrFeatureEnabled()
    {
        MRRuntimeSettings settings = FindSettingsInFixedScene(loadSceneIfNeeded: false);
        if (settings != null)
            return settings.mrFeatureEnabled;

        return ReadMrFeatureEnabledFromDisk();
    }

    static bool ReadMrFeatureEnabledFromDisk()
    {
        if (!File.Exists(FixedScenePath))
            return true;

        string yaml = File.ReadAllText(FixedScenePath);
        return !yaml.Contains("mrFeatureEnabled: 0");
    }

    static MRRuntimeSettings FindSettingsInFixedScene(bool loadSceneIfNeeded)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded || scene.path != FixedScenePath)
                continue;

            MRRuntimeSettings settings = FindInScene(scene);
            if (settings != null)
                return settings;
        }

        if (!loadSceneIfNeeded)
            return null;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return null;

        bool sceneWasLoaded = false;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).path == FixedScenePath)
            {
                sceneWasLoaded = true;
                break;
            }
        }

        Scene sceneToUse;
        if (!sceneWasLoaded)
        {
            sceneToUse = EditorSceneManager.OpenScene(FixedScenePath, OpenSceneMode.Single);
        }
        else
        {
            sceneToUse = SceneManager.GetSceneByPath(FixedScenePath);
            if (sceneToUse.IsValid() && sceneToUse.isLoaded)
                EditorSceneManager.SetActiveScene(sceneToUse);
        }

        return FindInScene(sceneToUse);
    }

    static MRRuntimeSettings FindInScene(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
            return null;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            MRRuntimeSettings settings = root.GetComponentInChildren<MRRuntimeSettings>(true);
            if (settings != null)
                return settings;
        }

        return null;
    }
}
#endif
