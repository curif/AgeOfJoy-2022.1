/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections.Generic;
using UnityEngine;

/// <summary>Harvests GLB animation clips and applies readable world scale for tiny skinned/animated exports.</summary>
public static class MRCustomObjectGlbSupport
{
    const string LogPrefix = "[MRCustomObjectGlbSupport]";
    const float MinReadableHeightMeters = 0.12f;
    const float TargetHumanoidHeightMeters = 1.6f;

    public static AnimationClip[] CollectClips(GameObject loadedRoot, AnimationClip[] importerClips)
    {
        var collected = new List<AnimationClip>();

        if (importerClips != null)
        {
            foreach (AnimationClip clip in importerClips)
                AddClip(collected, clip);
        }

        if (loadedRoot != null)
        {
            foreach (Animation animation in loadedRoot.GetComponentsInChildren<Animation>(true))
            {
                foreach (AnimationState state in animation)
                    AddClip(collected, state.clip);
            }

            foreach (Animator animator in loadedRoot.GetComponentsInChildren<Animator>(true))
            {
                if (animator.runtimeAnimatorController == null)
                    continue;

                foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
                    AddClip(collected, clip);
            }
        }

        return collected.ToArray();
    }

    public static void EnsureReadableWorldScale(GameObject packageRoot)
    {
        if (packageRoot == null)
            return;

        Renderer[] renderers = packageRoot.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        float height = bounds.size.y;
        if (height >= MinReadableHeightMeters)
            return;

        float safeHeight = Mathf.Max(height, 0.001f);
        float multiplier = TargetHumanoidHeightMeters / safeHeight;
        packageRoot.transform.localScale *= multiplier;

        ConfigManager.WriteConsole(
            $"{LogPrefix} {packageRoot.name}: auto-scaled x{multiplier:F1} (mesh height was {height:F3}m)");
    }

    public static AnimationClip FindClip(AnimationClip[] clips, string clipName)
    {
        if (clips == null || clips.Length == 0 || string.IsNullOrEmpty(clipName))
            return null;

        foreach (AnimationClip clip in clips)
        {
            if (clip != null && string.Equals(clip.name, clipName, System.StringComparison.OrdinalIgnoreCase))
                return clip;
        }

        string normalizedRequest = NormalizeClipName(clipName);
        foreach (AnimationClip clip in clips)
        {
            if (clip == null)
                continue;
            if (NormalizeClipName(clip.name) == normalizedRequest)
                return clip;
        }

        foreach (AnimationClip clip in clips)
        {
            if (clip == null)
                continue;
            if (clip.name.IndexOf(clipName, System.StringComparison.OrdinalIgnoreCase) >= 0
                || clipName.IndexOf(clip.name, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return clip;
        }

        return clips[0];
    }

    public static string DescribeClips(AnimationClip[] clips)
    {
        if (clips == null || clips.Length == 0)
            return "(none)";

        var names = new List<string>();
        foreach (AnimationClip clip in clips)
        {
            if (clip != null)
                names.Add(clip.name);
        }

        return names.Count > 0 ? string.Join(", ", names) : "(none)";
    }

    static void AddClip(List<AnimationClip> clips, AnimationClip clip)
    {
        if (clip == null)
            return;

        foreach (AnimationClip existing in clips)
        {
            if (existing != null && existing.name == clip.name)
                return;
        }

        clips.Add(clip);
    }

    static string NormalizeClipName(string name) =>
        name.Replace("\\", "|").Replace("/", "|").Trim().ToLowerInvariant();
}
