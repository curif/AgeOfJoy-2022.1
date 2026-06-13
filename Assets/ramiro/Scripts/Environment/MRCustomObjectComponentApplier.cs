/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>Attaches runtime behaviours listed in object.yaml <c>components:</c>.</summary>
public static class MRCustomObjectComponentApplier
{
    const string LogPrefix = "[MRCustomObjectComponentApplier]";

    public static void Apply(GameObject root, MRCustomObjectDefinition definition)
    {
        if (root == null || definition == null || !definition.HasAnyComponent())
            return;

        foreach (string componentId in definition.Components)
        {
            if (string.IsNullOrEmpty(componentId))
                continue;

            if (string.Equals(componentId, "rotator", System.StringComparison.OrdinalIgnoreCase))
                continue;

            if (string.Equals(componentId, "grab", System.StringComparison.OrdinalIgnoreCase))
                continue;

            if (string.Equals(componentId, "video", System.StringComparison.OrdinalIgnoreCase))
                continue;

            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} {definition.PackageName}: unknown component '{componentId}'");
        }

        if (definition.HasComponent("rotator"))
            ApplyRotator(root, definition);

        if (definition.HasGrabComponent())
            ApplyGrab(root, definition);

        if (definition.HasVideoComponent())
            ApplyVideo(root, definition);
    }

    static void ApplyGrab(GameObject root, MRCustomObjectDefinition definition)
    {
        MRCustomObjectGrabYaml config = definition.Grab ?? new MRCustomObjectGrabYaml();

        MRCustomObjectGrab grab = root.GetComponent<MRCustomObjectGrab>();
        if (grab == null)
            grab = root.AddComponent<MRCustomObjectGrab>();

        grab.Configure(config, root.transform);
        ConfigManager.WriteConsole(
            $"{LogPrefix} {definition.PackageName}: grab twoHands={config.TwoHands} returnOnRelease={config.ReturnOnRelease}");
    }

    static void ApplyVideo(GameObject root, MRCustomObjectDefinition definition)
    {
        MRCustomObjectVideoYaml config = definition.Video;
        if (config == null || string.IsNullOrEmpty(config.File) || string.IsNullOrEmpty(config.Target))
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} {definition.PackageName}: components includes video but video.file or video.target is missing");
            return;
        }

        Transform target = FindChildByName(root.transform, config.Target);
        if (target == null)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} {definition.PackageName}: video target not found '{config.Target}'");
            MRDebugLog.LogError($"Custom object '{definition.PackageName}': video target not found '{config.Target}'");
            return;
        }

        MRCustomObjectVideo video = root.GetComponent<MRCustomObjectVideo>();
        if (video == null)
            video = root.AddComponent<MRCustomObjectVideo>();

        video.Configure(definition.PackageName, config, target, definition.PackageDir);
        ConfigManager.WriteConsole(
            $"{LogPrefix} {definition.PackageName}: video on '{config.Target}' file={config.File} loop={config.Loop}");
    }

    static void ApplyRotator(GameObject root, MRCustomObjectDefinition definition)
    {
        MRCustomObjectRotatorYaml config = definition.Rotator;
        if (config == null || string.IsNullOrEmpty(config.Target))
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} {definition.PackageName}: components includes rotator but rotator.target is missing");
            return;
        }

        Transform target = FindChildByName(root.transform, config.Target);
        if (target == null)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} {definition.PackageName}: rotator target not found '{config.Target}'");
            return;
        }

        MRCustomObjectRotator rotator = root.GetComponent<MRCustomObjectRotator>();
        if (rotator == null)
            rotator = root.AddComponent<MRCustomObjectRotator>();

        float speed = config.Speed != 0f ? config.Speed : 180f;
        rotator.Configure(target, config.Axis, speed);
        ConfigManager.WriteConsole(
            $"{LogPrefix} {definition.PackageName}: rotator on '{config.Target}' axis={config.Axis ?? "y"} speed={speed}");
    }

    static Transform FindChildByName(Transform root, string childName)
    {
        if (root == null || string.IsNullOrEmpty(childName))
            return null;

        if (string.Equals(root.name, childName, System.StringComparison.OrdinalIgnoreCase))
            return root;

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child != root && string.Equals(child.name, childName, System.StringComparison.OrdinalIgnoreCase))
                return child;
        }

        return null;
    }
}
