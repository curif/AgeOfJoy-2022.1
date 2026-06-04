/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;
using UnityEngine.SceneManagement;

public static class MixedRealityBootstrap
{
    const string RootName = "MixedRealitySystem";

    static bool ShouldInstallForScene(string sceneName) =>
        sceneName == MRRuntimeSettings.FixedScene
        || sceneName == "TestMRmanager"
        || sceneName == MRRuntimeSettings.ExteriorScene;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        TryInstallForLoadedScenes();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void RegisterSceneHook()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryInstallForLoadedScenes();
    }

    static void TryInstallForLoadedScenes()
    {
        if (MixedRealityManager.Instance != null)
            return;

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (!ShouldInstallForScene(SceneManager.GetSceneAt(i).name))
                continue;

            InstallSystem();
            return;
        }
    }

    static void InstallSystem()
    {
        if (MixedRealityManager.Instance != null)
            return;

        var root = new GameObject(RootName);
        root.AddComponent<MixedRealityManager>();

        Scene active = SceneManager.GetActiveScene();
        if (active.name == "TestMRmanager")
            root.AddComponent<MRTestGameCabinetSpawn>();

        Object.DontDestroyOnLoad(root);
        ConfigManager.WriteConsole("[MixedRealityBootstrap] MixedRealitySystem installed");
    }
}
