/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Place on an existing scene object (mesh already in the hierarchy). Assign only
/// <c>object.yaml</c> — applies components (e.g. spot <c>light</c>, grab) to this GameObject.
/// Does not load or instantiate a GLB.
/// </summary>
public class MRTestCustomObjectComponent : MonoBehaviour
{
    const string LogPrefix = "[MRTestCustomObjectComponent]";

    [Tooltip("object.yaml under Assets/ — components are applied to this GameObject.")]
    [SerializeField] Object yamlAsset;

    [SerializeField] bool applyOnStart = true;

    void Start()
    {
        if (applyOnStart)
            ApplyComponents();
    }

    [ContextMenu("Apply YAML Components")]
    public void ApplyComponents()
    {
        if (!TryResolveYamlPath(out string yamlPath))
            return;

        if (!MRCustomObjectDefinition.TryLoadYamlOnly(yamlPath, out MRCustomObjectDefinition definition))
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} failed to parse {yamlPath}");
            return;
        }

        StripPreviousRuntimeComponents();
        MRCustomObjectComponentApplier.Apply(gameObject, definition);

        ConfigManager.WriteConsole(
            $"{LogPrefix} applied components on '{gameObject.name}' from {yamlPath} "
            + $"(light={definition.HasLightComponent()} grab={definition.HasGrabComponent()})");
    }

    [ContextMenu("Strip Applied Components")]
    public void StripPreviousRuntimeComponents()
    {
        foreach (MRCustomObjectLight light in GetComponentsInChildren<MRCustomObjectLight>(true))
        {
            if (light.AttachedLight != null)
            {
                Transform lightTransform = light.AttachedLight.transform;
                if (lightTransform != null && lightTransform != transform && lightTransform.name == "MRCustomObjectLight")
                    Destroy(lightTransform.gameObject);
                else if (light.AttachedLight != null)
                    Destroy(light.AttachedLight);
            }

            Destroy(light);
        }

        foreach (MRCustomObjectGrab grab in GetComponentsInChildren<MRCustomObjectGrab>(true))
            Destroy(grab);

        foreach (MRCustomObjectRotator rotator in GetComponentsInChildren<MRCustomObjectRotator>(true))
            Destroy(rotator);

        foreach (MRCustomObjectVideo video in GetComponentsInChildren<MRCustomObjectVideo>(true))
            Destroy(video);

        foreach (MRCustomObjectAnimator animator in GetComponentsInChildren<MRCustomObjectAnimator>(true))
            Destroy(animator);
    }

    bool TryResolveYamlPath(out string yamlPath)
    {
        yamlPath = null;

#if UNITY_EDITOR
        if (yamlAsset == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} assign yamlAsset (object.yaml) in the Inspector");
            return false;
        }

        string yamlAssetPath = AssetDatabase.GetAssetPath(yamlAsset);
        if (string.IsNullOrEmpty(yamlAssetPath))
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} could not resolve AssetDatabase path for yamlAsset");
            return false;
        }

        yamlPath = ToAbsoluteProjectPath(yamlAssetPath);
        if (!File.Exists(yamlPath))
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} yaml file missing on disk: {yamlPath}");
            return false;
        }

        return true;
#else
        ConfigManager.WriteConsoleWarning($"{LogPrefix} YAML asset apply is Editor-only");
        return false;
#endif
    }

#if UNITY_EDITOR
    static string ToAbsoluteProjectPath(string assetPath)
    {
        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
        if (string.IsNullOrEmpty(projectRoot))
            return Path.GetFullPath(assetPath);

        return Path.GetFullPath(Path.Combine(projectRoot, assetPath));
    }
#endif
}
