/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

/// <summary>Plays an embedded GLB glTF animation (object.yaml <c>animator</c> component).</summary>
public class MRCustomObjectAnimator : MonoBehaviour
{
    const string LogPrefix = "[MRCustomObjectAnimator]";

    Animation legacyAnimation;
    PlayableGraph playableGraph;
    AnimationClipPlayable clipPlayable;
    AnimationClip playingClip;
    GameObject sampleRoot;
    float speed = 1f;
    bool loop = true;
    bool playing;
    float playTime;

    void Update()
    {
        if (!playing || playingClip == null || sampleRoot == null || playingClip.length <= 0f)
            return;

        if (legacyAnimation != null)
            return;

        if (!clipPlayable.IsValid())
            return;

        playTime += Time.deltaTime * speed;
        float sampleTime = loop
            ? Mathf.Repeat(playTime, playingClip.length)
            : Mathf.Min(playTime, playingClip.length);

        clipPlayable.SetTime(sampleTime);
    }

    void OnDestroy()
    {
        StopPlayback();
    }

    public void Configure(
        string packageName,
        MRCustomObjectAnimatorYaml config,
        Transform target,
        AnimationClip[] clips)
    {
        if (config == null || string.IsNullOrEmpty(config.Clip) || target == null)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} {packageName}: animator missing clip or target");
            return;
        }

        Transform packageRoot = transform;
        clips = MRCustomObjectGlbSupport.CollectClips(packageRoot.gameObject, clips);
        if (clips == null || clips.Length == 0)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} {packageName}: no animation clips in GLB");
            MRDebugLog.LogError($"Custom object '{packageName}': GLB has no animation clips");
            return;
        }

        AnimationClip clip = MRCustomObjectGlbSupport.FindClip(clips, config.Clip);
        if (clip == null)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} {packageName}: animator clip not found '{config.Clip}' "
                + $"(available: {MRCustomObjectGlbSupport.DescribeClips(clips)})");
            MRDebugLog.LogError(
                $"Custom object '{packageName}': animator clip not found '{config.Clip}' "
                + $"(available: {MRCustomObjectGlbSupport.DescribeClips(clips)})");
            return;
        }

        if (!string.Equals(clip.name, config.Clip, System.StringComparison.OrdinalIgnoreCase))
        {
            ConfigManager.WriteConsole(
                $"{LogPrefix} {packageName}: matched clip '{clip.name}' for requested '{config.Clip}'");
        }

        GameObject playbackRoot = ResolveSampleRoot(target, packageRoot);
        if (playbackRoot == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} {packageName}: sample root not resolved");
            return;
        }

        StopPlayback();
        sampleRoot = playbackRoot;
        RemoveImportedAnimators(packageRoot.gameObject);

        playingClip = clip;
        speed = config.Speed > 0f ? config.Speed : 1f;
        loop = config.Loop;
        playTime = 0f;
        playing = config.PlayOnAwake;

        if (clip.legacy)
            StartLegacyAnimation(packageName, clip);
        else
            StartPlayableGraph(packageName, clip);

        string mode = legacyAnimation != null ? "LegacyAnimation" : "PlayableGraph";
        ConfigManager.WriteConsole(
            $"{LogPrefix} {packageName}: {mode} clip '{clip.name}' on '{sampleRoot.name}' "
            + $"loop={loop} speed={speed:F2} playing={playing}");
    }

    const string LegacyPlaybackKey = "__MRCustomObjectPlayback";

    void StartLegacyAnimation(string packageName, AnimationClip clip)
    {
        legacyAnimation = sampleRoot.GetComponent<Animation>();
        if (legacyAnimation == null)
            legacyAnimation = sampleRoot.AddComponent<Animation>();

        clip.wrapMode = loop ? WrapMode.Loop : WrapMode.ClampForever;

        if (legacyAnimation.GetClip(LegacyPlaybackKey) == null)
            legacyAnimation.AddClip(clip, LegacyPlaybackKey);

        AnimationClip registeredClip = legacyAnimation.GetClip(LegacyPlaybackKey);
        if (registeredClip == null)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} {packageName}: legacy AddClip failed for '{clip.name}' — trying PlayableGraph");
            MRDebugLog.LogError(
                $"Custom object '{packageName}': legacy animation setup failed for '{clip.name}'");
            legacyAnimation = null;
            StartPlayableGraph(packageName, clip);
            return;
        }

        legacyAnimation.clip = registeredClip;

        if (playing)
            legacyAnimation.Play(LegacyPlaybackKey);

        AnimationState state = legacyAnimation[LegacyPlaybackKey];
        if (state != null)
        {
            state.speed = speed;
            state.wrapMode = loop ? WrapMode.Loop : WrapMode.ClampForever;
            return;
        }

        ConfigManager.WriteConsoleWarning(
            $"{LogPrefix} {packageName}: legacy AnimationState missing after Play — trying PlayableGraph");
        legacyAnimation.Stop();
        legacyAnimation = null;
        StartPlayableGraph(packageName, clip);
    }

    void StartPlayableGraph(string packageName, AnimationClip clip)
    {
#if UNITY_EDITOR
        clip.SampleAnimation(sampleRoot, 0f);
        ConfigManager.WriteConsoleWarning(
            $"{LogPrefix} {packageName}: clip '{clip.name}' is not legacy — using Editor SampleAnimation fallback");
        return;
#endif
        Animator animator = sampleRoot.GetComponent<Animator>();
        if (animator == null)
            animator = sampleRoot.AddComponent<Animator>();

        playableGraph = PlayableGraph.Create($"{sampleRoot.name}_MRCustomObjectAnimator");
        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        clipPlayable = AnimationClipPlayable.Create(playableGraph, clip);
        clipPlayable.SetApplyFootIK(false);
        clipPlayable.SetSpeed(speed);

        AnimationPlayableOutput output = AnimationPlayableOutput.Create(
            playableGraph, "Animation", animator);
        output.SetSourcePlayable(clipPlayable);

        if (playing)
            playableGraph.Play();
    }

    void StopPlayback()
    {
        if (legacyAnimation != null)
        {
            legacyAnimation.Stop();
            legacyAnimation = null;
        }

        if (playableGraph.IsValid())
        {
            playableGraph.Destroy();
            playableGraph = default;
        }

        clipPlayable = default;
        playingClip = null;
        sampleRoot = null;
        playing = false;
        playTime = 0f;
    }

    static GameObject ResolveSampleRoot(Transform target, Transform packageRoot)
    {
        Transform namedRoot = FindChildByName(packageRoot, "Root");
        if (namedRoot != null)
            return namedRoot.gameObject;

        if (packageRoot.childCount > 0)
            return packageRoot.GetChild(0).gameObject;

        return target != null ? target.gameObject : packageRoot.gameObject;
    }

    static void RemoveImportedAnimators(GameObject packageRoot)
    {
        foreach (Animator animator in packageRoot.GetComponentsInChildren<Animator>(true))
        {
            if (animator != null)
                Destroy(animator);
        }
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
