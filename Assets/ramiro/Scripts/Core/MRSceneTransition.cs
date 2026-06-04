/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MRSceneTransition : MonoBehaviour
{
    const string LogPrefix = "[MRSceneTransition]";

    static string FixedSceneName => MRRuntimeSettings.FixedScene;

    readonly List<string> unloadedSceneNames = new List<string>();
    bool transitionRunning;

    public bool IsTransitionRunning => transitionRunning;

    public void ForceResetTransitionState()
    {
        transitionRunning = false;
    }

    public IEnumerator UnloadVrScenes()
    {
        transitionRunning = true;
        unloadedSceneNames.Clear();

        if (!IsFixedSceneLoaded())
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} FixedScene not loaded — skip VR scene unload (test/dev keeps active scene)");
            transitionRunning = false;
            yield break;
        }

        var scenesToUnload = new List<Scene>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded || scene.name == FixedSceneName)
                continue;
            scenesToUnload.Add(scene);
        }

        foreach (Scene scene in scenesToUnload)
        {
            ConfigManager.WriteConsole($"{LogPrefix} unloading {scene.name}");
            unloadedSceneNames.Add(scene.name);
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(scene);
            if (unloadOp == null)
            {
                ConfigManager.WriteConsoleError($"{LogPrefix} failed to unload {scene.name}");
                continue;
            }

            while (!unloadOp.isDone)
                yield return null;
        }

        transitionRunning = false;
    }

    public IEnumerator ReloadVrScenes()
    {
        transitionRunning = true;
        MRTransitionLog.LogStep("ReloadVrScenes", "begin");
        MRTransitionLog.LogScenes("ReloadVrScenes-begin");

        var scenesToLoad = BuildReloadSceneList();
        MRTransitionLog.Log($"ReloadVrScenes queue=[{string.Join(", ", scenesToLoad)}] unloadedHistory=[{string.Join(", ", unloadedSceneNames)}] fixedScene={IsFixedSceneLoaded()}");

        if (scenesToLoad.Count == 0)
        {
            MRTransitionLog.LogError("ReloadVrScenes — no VR scenes to reload");
            ConfigManager.WriteConsoleWarning($"{LogPrefix} no VR scenes to reload");
            transitionRunning = false;
            yield break;
        }

        foreach (string sceneName in scenesToLoad)
        {
            if (string.IsNullOrEmpty(sceneName) || sceneName == FixedSceneName)
                continue;

            Scene existing = SceneManager.GetSceneByName(sceneName);
            if (existing.isLoaded)
            {
                MRTransitionLog.Log($"ReloadVrScenes skip already loaded: {sceneName}");
                ConfigManager.WriteConsole($"{LogPrefix} already loaded: {sceneName}");
                continue;
            }

            MRTransitionLog.LogStep("ReloadVrScenes", $"LoadSceneAsync start {sceneName}");
            ConfigManager.WriteConsole($"{LogPrefix} loading {sceneName}");
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (loadOp == null)
            {
                MRTransitionLog.LogError($"ReloadVrScenes failed to start load for {sceneName}");
                ConfigManager.WriteConsoleError(
                    $"{LogPrefix} failed to start load for {sceneName} — is it in Build Settings?");
                continue;
            }

            loadOp.allowSceneActivation = true;
            float loadTimeout = 45f;
            float loadElapsed = 0f;
            float lastProgressLog = 0f;
            while (!loadOp.isDone && loadElapsed < loadTimeout)
            {
                loadElapsed += Time.unscaledDeltaTime;
                if (loadElapsed - lastProgressLog >= 1f)
                {
                    lastProgressLog = loadElapsed;
                    MRTransitionLog.Log($"ReloadVrScenes {sceneName} progress={loadOp.progress:F2} elapsed={loadElapsed:F1}s");
                }
                yield return null;
            }

            if (!loadOp.isDone)
            {
                MRTransitionLog.LogError($"ReloadVrScenes timeout {sceneName} after {loadTimeout}s progress={loadOp.progress:F2}");
                ConfigManager.WriteConsoleError($"{LogPrefix} load timeout for {sceneName} after {loadTimeout}s");
                continue;
            }

            MRTransitionLog.LogStep("ReloadVrScenes", $"loaded {sceneName}");
            ConfigManager.WriteConsole($"{LogPrefix} loaded {sceneName} (progress=1)");
            MRTransitionLog.LogScenes($"ReloadVrScenes-after-{sceneName}");
        }

        yield return null;
        yield return null;

        ActivateBestVrScene();
        MRTransitionLog.LogScenes("ReloadVrScenes-after-activate");

        int loadedCount = 0;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded && scene.name != FixedSceneName)
                loadedCount++;
        }

        if (loadedCount == 0)
        {
            MRTransitionLog.LogError("ReloadVrScenes finished but ZERO room scenes loaded");
            ConfigManager.WriteConsoleError($"{LogPrefix} VR reload finished but no room scenes are loaded");
        }

        unloadedSceneNames.Clear();
        transitionRunning = false;
        MRTransitionLog.LogStep("ReloadVrScenes", $"DONE roomScenes={loadedCount}");
        ConfigManager.WriteConsole($"{LogPrefix} VR scenes reloaded ({loadedCount} room scene(s) active)");
    }

    List<string> BuildReloadSceneList()
    {
        var scenesToLoad = new List<string>();

        foreach (string sceneName in unloadedSceneNames)
        {
            if (string.IsNullOrEmpty(sceneName) || sceneName == FixedSceneName)
                continue;
            if (!scenesToLoad.Contains(sceneName))
                scenesToLoad.Add(sceneName);
        }

        if (scenesToLoad.Count == 0)
        {
            foreach (string sceneName in MRRuntimeSettings.VrScenesToReloadOnExitMr())
            {
                if (!scenesToLoad.Contains(sceneName))
                    scenesToLoad.Add(sceneName);
            }
        }

        return scenesToLoad;
    }

    static bool IsFixedSceneLoaded()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded && scene.name == FixedSceneName)
                return true;
        }

        return false;
    }

    static void ActivateBestVrScene()
    {
        Scene target = SceneManager.GetSceneByName(MRRuntimeSettings.IntroGalleryScene);
        if (!target.isLoaded)
            target = SceneManager.GetSceneByName(MRRuntimeSettings.ExteriorScene);

        if (!target.isLoaded)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && scene.name != FixedSceneName)
                {
                    target = scene;
                    break;
                }
            }
        }

        if (target.isLoaded && SceneManager.GetActiveScene() != target)
        {
            SceneManager.SetActiveScene(target);
            MRTransitionLog.Log($"ActivateBestVrScene set active={target.name}");
            ConfigManager.WriteConsole($"{LogPrefix} active scene set to {target.name}");
        }
        else
        {
            MRTransitionLog.LogWarning($"ActivateBestVrScene no change targetLoaded={target.isLoaded} target={target.name} current={SceneManager.GetActiveScene().name}");
        }
    }
}
