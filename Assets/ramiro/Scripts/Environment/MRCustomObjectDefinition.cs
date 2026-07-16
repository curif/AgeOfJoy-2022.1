/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

/// <summary>
/// Parsed <c>MR/Custom Objects/&lt;Package&gt;/object.yaml</c> — schema v1.
/// Spec: <see cref="MRPaths"/> + Assets/ramiro/CUSTOM_OBJECT_YAML.md
/// </summary>
[Serializable]
public class MRCustomObjectDefinition
{
    public const int SupportedYamlVersion = 1;

    public int Version = 1;
    public string Name;
    public string DisplayName;
    public string Author;
    public MRCustomObjectModelYaml Model;
    public MRCustomObjectPlacementYaml Placement;
    public MRCustomObjectCollisionYaml Collision;
    public MRCustomObjectAudioYaml Audio;
    public List<string> Components;
    public MRCustomObjectRotatorYaml Rotator;
    public MRCustomObjectGrabYaml Grab;
    public MRCustomObjectVideoYaml Video;
    public MRCustomObjectAnimatorYaml Animator;
    public MRCustomObjectLightYaml Light;

    public string PackageName { get; private set; }
    public string PackageDir { get; private set; }
    /// <summary>When set (dev / test load), GLB is loaded from this absolute path instead of PackageDir + model.file.</summary>
    public string AbsoluteModelPath { get; private set; }

    public string GetDisplayName() =>
        !string.IsNullOrEmpty(DisplayName) ? DisplayName
        : !string.IsNullOrEmpty(Name) ? Name
        : PackageName;

    public string GetModelFileName() =>
        Model != null && !string.IsNullOrEmpty(Model.File) ? Model.File : null;

    public string GetResolvedModelPath()
    {
        if (!string.IsNullOrEmpty(AbsoluteModelPath))
            return AbsoluteModelPath;

        string modelFile = GetModelFileName();
        if (string.IsNullOrEmpty(modelFile) || string.IsNullOrEmpty(PackageDir))
            return null;

        return Path.Combine(PackageDir, modelFile);
    }

    public float GetModelScale() =>
        Model != null && Model.Scale > 0f ? Model.Scale : 1f;

    public Vector3 GetModelLocalOffset() =>
        Model?.Offset != null ? Model.Offset.ToVector3() : Vector3.zero;

    public Vector3 GetModelLocalEuler() =>
        Model?.Rotation != null ? Model.Rotation.ToVector3() : Vector3.zero;

    public PlacementSurfaceType GetSurfaceType() =>
        Placement != null ? Placement.SurfaceType : PlacementSurfaceType.Floor;

    public PlacementFacingAxis GetFacingAxis() =>
        Placement != null ? Placement.FacingAxis : PlacementFacingAxis.PositiveZ;

    public bool GetAllowStickRotation() => Placement != null && Placement.AllowStickRotation;

    public PlacementStickRotationAxis GetStickRotationAxis() =>
        Placement != null ? Placement.StickRotationAxis : PlacementStickRotationAxis.WorldYaw;

    public float GetStickRotationSpeed() =>
        Placement != null && Placement.StickRotationSpeed > 0f ? Placement.StickRotationSpeed : 90f;

    public bool GetProvidesAnchor() => Placement != null && Placement.ProvidesAnchor;

    public string GetAnchorTarget() => Placement?.AnchorTarget;

    public MRCustomObjectCollisionMode GetCollisionMode()
    {
        if (Collision == null || string.IsNullOrEmpty(Collision.Mode))
            return MRCustomObjectCollisionMode.Mesh;

        if (Enum.TryParse(Collision.Mode, true, out MRCustomObjectCollisionMode parsed))
            return parsed;

        ConfigManager.WriteConsoleWarning(
            $"[MRCustomObjectDefinition] {PackageName}: unknown collision.mode '{Collision.Mode}', using mesh");
        return MRCustomObjectCollisionMode.Mesh;
    }

    public bool GetCollisionConvex() => Collision == null || Collision.Convex;

    public bool HasAnyComponent() => Components != null && Components.Count > 0;

    public bool HasComponent(string componentId)
    {
        if (string.IsNullOrEmpty(componentId) || Components == null)
            return false;

        foreach (string entry in Components)
        {
            if (string.Equals(entry, componentId, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public bool HasGrabComponent() => HasComponent("grab");

    public bool HasVideoComponent() => HasComponent("video");

    public bool HasAnimatorComponent() => HasComponent("animator");

    public bool HasLightComponent() => HasComponent("light");

    public static bool TryLoad(string packageName, out MRCustomObjectDefinition definition)
    {
        definition = null;
        string packageDir = MRPaths.GetCustomObjectPackageDir(packageName);
        if (string.IsNullOrEmpty(packageDir))
        {
            MRDebugLog.LogError($"Custom object '{packageName}': package folder not found");
            return false;
        }

        string yamlPath = Path.Combine(packageDir, MRPaths.CustomObjectYamlFileName);
        return TryLoadFromPaths(
            yamlPath,
            glbAbsolutePath: null,
            packageNameOverride: packageName,
            requireModelFile: true,
            out definition);
    }

    /// <summary>
    /// Load from explicit YAML + optional absolute GLB (dev / Inspector drag-drop test).
    /// <paramref name="glbAbsolutePath"/> overrides <c>model.file</c> when set.
    /// </summary>
    public static bool TryLoadFromPaths(
        string yamlPath,
        string glbAbsolutePath,
        out MRCustomObjectDefinition definition)
    {
        return TryLoadFromPaths(
            yamlPath,
            glbAbsolutePath,
            packageNameOverride: null,
            requireModelFile: true,
            out definition);
    }

    /// <summary>
    /// Parse <c>object.yaml</c> without requiring <c>model.file</c> / GLB on disk
    /// (apply components onto an existing scene object).
    /// </summary>
    public static bool TryLoadYamlOnly(string yamlPath, out MRCustomObjectDefinition definition)
    {
        return TryLoadFromPaths(
            yamlPath,
            glbAbsolutePath: null,
            packageNameOverride: null,
            requireModelFile: false,
            out definition);
    }

    static bool TryLoadFromPaths(
        string yamlPath,
        string glbAbsolutePath,
        string packageNameOverride,
        bool requireModelFile,
        out MRCustomObjectDefinition definition)
    {
        definition = null;

        if (string.IsNullOrEmpty(yamlPath) || !File.Exists(yamlPath))
        {
            MRDebugLog.LogError($"Custom object: object.yaml not found ({yamlPath})");
            return false;
        }

        string packageDir = Path.GetDirectoryName(yamlPath);
        if (string.IsNullOrEmpty(packageDir))
        {
            MRDebugLog.LogError($"Custom object: invalid yaml path '{yamlPath}'");
            return false;
        }

        string packageName = !string.IsNullOrEmpty(packageNameOverride)
            ? packageNameOverride
            : new DirectoryInfo(packageDir).Name;

        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            definition = deserializer.Deserialize<MRCustomObjectDefinition>(File.ReadAllText(yamlPath));
            if (definition == null)
                return false;

            definition.PackageName = packageName;
            definition.PackageDir = packageDir;

            if (definition.Version <= 0)
                definition.Version = 1;

            if (definition.Version > SupportedYamlVersion)
            {
                ConfigManager.WriteConsoleWarning(
                    $"[MRCustomObjectDefinition] {packageName}: object.yaml version {definition.Version} > supported {SupportedYamlVersion}");
            }

            if (string.IsNullOrEmpty(definition.Name))
                definition.Name = packageName;

            if (!string.IsNullOrEmpty(glbAbsolutePath))
            {
                if (!File.Exists(glbAbsolutePath))
                {
                    ConfigManager.WriteConsoleWarning(
                        $"[MRCustomObjectDefinition] {packageName}: GLB not found ({glbAbsolutePath})");
                    MRDebugLog.LogError($"Custom object '{packageName}': GLB not found ({glbAbsolutePath})");
                    return false;
                }

                definition.AbsoluteModelPath = Path.GetFullPath(glbAbsolutePath);
                if (definition.Model == null)
                    definition.Model = new MRCustomObjectModelYaml();
                if (string.IsNullOrEmpty(definition.Model.File))
                    definition.Model.File = Path.GetFileName(glbAbsolutePath);
            }
            else if (requireModelFile)
            {
                string modelFile = definition.GetModelFileName();
                if (string.IsNullOrEmpty(modelFile))
                {
                    ConfigManager.WriteConsoleWarning($"[MRCustomObjectDefinition] {packageName}: model.file missing");
                    MRDebugLog.LogError($"Custom object '{packageName}': model.file missing in object.yaml");
                    return false;
                }

                if (!File.Exists(Path.Combine(packageDir, modelFile)))
                {
                    ConfigManager.WriteConsoleWarning($"[MRCustomObjectDefinition] {packageName}: model not found ({modelFile})");
                    MRDebugLog.LogError($"Custom object '{packageName}': model not found ({modelFile})");
                    return false;
                }
            }

            ValidateOptionalAssets(definition);
            return true;
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[MRCustomObjectDefinition] load {yamlPath}", e);
            MRDebugLog.LogError($"Custom object '{packageName}': object.yaml parse failed ({e.Message})");
            return false;
        }
    }

    static void ValidateOptionalAssets(MRCustomObjectDefinition definition)
    {
        string packageName = definition.PackageName;
        string packageDir = definition.PackageDir;

        if (definition.Audio != null && !string.IsNullOrEmpty(definition.Audio.File))
        {
            string audioPath = Path.Combine(packageDir, definition.Audio.File);
            if (!File.Exists(audioPath))
            {
                ConfigManager.WriteConsoleWarning(
                    $"[MRCustomObjectDefinition] {packageName}: audio file missing ({definition.Audio.File})");
            }
        }

        if (definition.HasComponent("rotator")
            && (definition.Rotator == null || string.IsNullOrEmpty(definition.Rotator.Target)))
        {
            ConfigManager.WriteConsoleWarning(
                $"[MRCustomObjectDefinition] {packageName}: components lists rotator but rotator.target is missing");
        }

        if (definition.HasGrabComponent() && definition.Grab == null)
        {
            ConfigManager.WriteConsoleWarning(
                $"[MRCustomObjectDefinition] {packageName}: components lists grab but grab: block is missing — using defaults");
        }

        if (definition.HasVideoComponent())
        {
            if (definition.Video == null || string.IsNullOrEmpty(definition.Video.File))
            {
                ConfigManager.WriteConsoleWarning(
                    $"[MRCustomObjectDefinition] {packageName}: components lists video but video.file is missing");
                MRDebugLog.LogError($"Custom object '{packageName}': video.file missing in object.yaml");
            }
            else if (string.IsNullOrEmpty(definition.Video.Target))
            {
                ConfigManager.WriteConsoleWarning(
                    $"[MRCustomObjectDefinition] {packageName}: components lists video but video.target is missing");
                MRDebugLog.LogError($"Custom object '{packageName}': video.target missing (screen mesh name)");
            }
            else
            {
                string videoPath = Path.Combine(packageDir, definition.Video.File);
                if (!File.Exists(videoPath))
                {
                    ConfigManager.WriteConsoleWarning(
                        $"[MRCustomObjectDefinition] {packageName}: video file missing ({definition.Video.File})");
                    MRDebugLog.LogError($"Custom object '{packageName}': video file missing ({definition.Video.File})");
                }
            }
        }

        if (definition.HasAnimatorComponent()
            && (definition.Animator == null || string.IsNullOrEmpty(definition.Animator.Clip)))
        {
            ConfigManager.WriteConsoleWarning(
                $"[MRCustomObjectDefinition] {packageName}: components lists animator but animator.clip is missing");
        }

        if (definition.HasLightComponent())
        {
            if (definition.Light == null)
            {
                ConfigManager.WriteConsoleWarning(
                    $"[MRCustomObjectDefinition] {packageName}: components lists light but light: block is missing — using defaults");
            }
            else if (!MRCustomObjectLightYaml.IsSupportedType(definition.Light.Type))
            {
                ConfigManager.WriteConsoleWarning(
                    $"[MRCustomObjectDefinition] {packageName}: unknown light.type '{definition.Light.Type}' (use point or spot)");
            }
        }
    }
}

public enum MRCustomObjectCollisionMode
{
    Mesh,
    Box,
    None
}

[Serializable]
public class MRCustomObjectModelYaml
{
    public string File;
    public float Scale = 1f;
    public MRVector3 Offset;
    public MRVector3 Rotation;
}

[Serializable]
public class MRCustomObjectPlacementYaml
{
    public PlacementSurfaceType SurfaceType = PlacementSurfaceType.Floor;
    public PlacementFacingAxis FacingAxis = PlacementFacingAxis.PositiveZ;
    public bool AllowStickRotation;
    public PlacementStickRotationAxis StickRotationAxis = PlacementStickRotationAxis.WorldYaw;
    public float StickRotationSpeed = 90f;
    /// <summary>When true, a child collider is tagged MRPlacementAnchor at spawn for other props to snap to.</summary>
    public bool ProvidesAnchor;
    /// <summary>Optional GLB child name for the anchor surface; omit for package root.</summary>
    public string AnchorTarget;
}

[Serializable]
public class MRCustomObjectCollisionYaml
{
    public string Mode = "mesh";
    public bool Convex = true;
}

[Serializable]
public class MRCustomObjectAudioYaml
{
    public string File;
    public bool Loop = true;
    public float Volume = 1f;
    public bool PlayOnAwake = true;
    public MRCustomObjectAudioDistanceYaml Distance;
}

[Serializable]
public class MRCustomObjectAudioDistanceYaml
{
    public float Min = 0.5f;
    public float Max = 4f;
}

[Serializable]
public class MRCustomObjectGrabYaml
{
    /// <summary>When true, both hands must grab handle colliders (like PortableGames).</summary>
    public bool TwoHands;
    public bool ReturnOnRelease = true;
    public bool HideHands = true;
    /// <summary>One-hand: optional GLB child used as grip pivot (pose snaps to hand). Two-hands: grab root.</summary>
    public string Target;
    public float ReturnDurationSeconds = 0.15f;
}

[Serializable]
public class MRCustomObjectVideoYaml
{
    /// <summary>Video file relative to package folder (.mp4, .webm, .mov).</summary>
    public string File;
    /// <summary>Child mesh name in the GLB hierarchy (e.g. Screen).</summary>
    public string Target;
    public bool Loop = true;
    public bool PlayOnAwake = true;
    public float Volume = 1f;
    public bool InvertX;
    public bool InvertY;
}

[Serializable]
public class MRCustomObjectRotatorYaml
{
    /// <summary>Child transform name in the GLB hierarchy (e.g. Blades).</summary>
    public string Target;
    /// <summary>Local axis: x, y, z (optional - prefix).</summary>
    public string Axis = "y";
    /// <summary>Degrees per second.</summary>
    public float Speed = 180f;
}

[Serializable]
public class MRCustomObjectAnimatorYaml
{
    /// <summary>Animation name from the GLB (glTF animation). Required.</summary>
    public string Clip;
    /// <summary>Optional GLB child; omit = auto-detect Animation/Animator on model.</summary>
    public string Target;
    public bool Loop = true;
    public bool PlayOnAwake = true;
    /// <summary>Playback speed multiplier (default 1).</summary>
    public float Speed = 1f;
}

[Serializable]
public class MRCustomObjectLightYaml
{
    public const string TypePoint = "point";
    public const string TypeSpot = "spot";

    /// <summary>Optional GLB child for the light transform; omit = package root. Spot uses local +Z as beam direction.</summary>
    public string Target;
    /// <summary><c>point</c> or <c>spot</c>.</summary>
    public string Type = TypePoint;
    public float Intensity = 2f;
    public float Range = 4f;
    public MRColor Color;
    /// <summary>Outer cone angle in degrees (spot only).</summary>
    public float SpotAngle = 60f;
    /// <summary>Inner cone angle in degrees (spot only).</summary>
    public float InnerSpotAngle = 30f;
    public bool Shadows;

    public static bool IsSupportedType(string type)
    {
        if (string.IsNullOrEmpty(type))
            return true;

        return string.Equals(type, TypePoint, StringComparison.OrdinalIgnoreCase)
            || string.Equals(type, TypeSpot, StringComparison.OrdinalIgnoreCase);
    }
}
