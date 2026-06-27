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

    public string PackageName { get; private set; }
    public string PackageDir { get; private set; }

    public string GetDisplayName() =>
        !string.IsNullOrEmpty(DisplayName) ? DisplayName
        : !string.IsNullOrEmpty(Name) ? Name
        : PackageName;

    public string GetModelFileName() =>
        Model != null && !string.IsNullOrEmpty(Model.File) ? Model.File : null;

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
        if (!File.Exists(yamlPath))
        {
            MRDebugLog.LogError($"Custom object '{packageName}': object.yaml not found");
            return false;
        }

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

            return true;
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[MRCustomObjectDefinition] load {yamlPath}", e);
            MRDebugLog.LogError($"Custom object '{packageName}': object.yaml parse failed ({e.Message})");
            return false;
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
    /// <summary>Optional child name to attach grab; default = package root.</summary>
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
