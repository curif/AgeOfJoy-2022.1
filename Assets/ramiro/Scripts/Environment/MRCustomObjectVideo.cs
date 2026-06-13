/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

/// <summary>Plays a video file on a child mesh (object.yaml <c>video</c> component).</summary>
[DisallowMultipleComponent]
public class MRCustomObjectVideo : MonoBehaviour
{
    const string LogPrefix = "[MRCustomObjectVideo]";
    const float PrepareTimeoutSeconds = 35f;
    const float PlaybackTimeoutSeconds = 25f;

    enum AudioMode
    {
        None,
        Direct,
        Spatial
    }

    string packageName;
    string videoFilePath;
    bool loop = true;
    bool playOnAwake = true;
    float volume = 1f;
    bool invertX;
    bool invertY;

    Transform screenTarget;
    VideoPlayer videoPlayer;
    Renderer targetRenderer;
    AudioSource audioSource;
    Material runtimeMaterial;
    bool started;
    bool playbackActive;
    bool sawDurationWarning;
    string lastFatalError;

    public void Configure(
        string packageLabel,
        MRCustomObjectVideoYaml config,
        Transform screenTransform,
        string packageDir)
    {
        packageName = packageLabel;
        screenTarget = screenTransform;
        loop = config == null || config.Loop;
        playOnAwake = config == null || config.PlayOnAwake;
        volume = config != null && config.Volume > 0f ? config.Volume : 1f;
        invertX = config != null && config.InvertX;
        invertY = config != null && config.InvertY;

        if (config == null || string.IsNullOrEmpty(config.File) || string.IsNullOrEmpty(packageDir))
            return;

        videoFilePath = Path.GetFullPath(Path.Combine(packageDir, config.File));
    }

    void Start()
    {
        if (started || screenTarget == null || string.IsNullOrEmpty(videoFilePath))
            return;

        started = true;
        StartCoroutine(StartVideoCoroutine());
    }

    void LateUpdate()
    {
        if (!playbackActive || videoPlayer == null || targetRenderer == null)
            return;

        Texture frame = videoPlayer.texture;
        if (frame == null)
            return;

        if (runtimeMaterial == null || runtimeMaterial.mainTexture != frame)
            ApplyVideoTexture(frame);
    }

    IEnumerator StartVideoCoroutine()
    {
        if (!File.Exists(videoFilePath))
        {
            LogVideoError($"video file missing ({videoFilePath})");
            yield break;
        }

        targetRenderer = screenTarget.GetComponent<Renderer>();
        if (targetRenderer == null)
        {
            LogVideoError($"video target has no Renderer ({screenTarget.name})");
            yield break;
        }

        EnsureVideoPlayer();

        string videoUrl = ToVideoPlayerUrl(videoFilePath);
        if (string.IsNullOrEmpty(videoUrl))
        {
            LogVideoError($"could not build video URL for {videoFilePath}");
            yield break;
        }

        playbackActive = false;
        sawDurationWarning = false;
        AudioMode usedAudio = AudioMode.None;
        yield return TryStartPlayback(videoUrl, mode => usedAudio = mode);

        if (!playbackActive && (sawDurationWarning || IsNonFatalDurationError(lastFatalError)))
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} {packageName}: source has broken audio metadata — trying video-only cache");

            string cachePath = null;
            string cacheError = null;
            yield return MRVideoRemuxUtility.EnsureNoAudioCacheAsync(
                videoFilePath,
                (path, error) =>
                {
                    cachePath = path;
                    cacheError = error;
                });

            if (!string.IsNullOrEmpty(cachePath) && File.Exists(cachePath))
            {
                playbackActive = false;
                sawDurationWarning = false;
                lastFatalError = null;
                string cacheUrl = ToVideoPlayerUrl(cachePath);
                yield return TryStartPlayback(cacheUrl, mode => usedAudio = mode, forceSilent: true);

                if (playbackActive)
                {
                    ConfigManager.WriteConsole(
                        $"{LogPrefix} {packageName}: playing cached video-only {Path.GetFileName(cachePath)}");
                }
            }
            else if (!string.IsNullOrEmpty(cacheError))
            {
                MRDebugLog.LogWarning($"Custom object '{packageName}': video remux cache failed — {cacheError}");
            }
        }

        if (!playbackActive)
        {
            string detail = !string.IsNullOrEmpty(lastFatalError)
                ? lastFatalError
                : "playback timeout";
            LogVideoError($"video playback failed ({Path.GetFileName(videoFilePath)}): {detail}");
            yield break;
        }

        ConfigManager.WriteConsole(
            $"{LogPrefix} {packageName}: playing on '{screenTarget.name}' audio={usedAudio}");
    }

    void LogVideoError(string message)
    {
        ConfigManager.WriteConsoleWarning($"{LogPrefix} {packageName}: {message}");
        MRDebugLog.LogError($"Custom object '{packageName}': {message}");
    }

    static bool IsNonFatalDurationError(string message)
    {
        if (string.IsNullOrEmpty(message))
            return false;

        return message.IndexOf("0xc00d36e6", StringComparison.OrdinalIgnoreCase) >= 0
            || message.IndexOf("Getting duration", StringComparison.OrdinalIgnoreCase) >= 0
            || message.IndexOf("ATTRIBUTENOTFOUND", StringComparison.OrdinalIgnoreCase) >= 0
            || message.IndexOf("atributo solicitado", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    void EnsureVideoPlayer()
    {
        if (videoPlayer != null)
            return;

        GameObject host = new GameObject($"{packageName}_VideoPlayer");
        host.transform.SetParent(transform, false);
        host.hideFlags = HideFlags.DontSave;

        videoPlayer = host.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.skipOnDrop = true;
        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        videoPlayer.source = VideoSource.Url;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
    }

    void ApplyAudioMode(AudioMode mode)
    {
        if (videoPlayer == null)
            return;

        switch (mode)
        {
            case AudioMode.Direct:
                videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
                videoPlayer.SetDirectAudioVolume(0, volume);
                break;
            case AudioMode.Spatial:
                audioSource = videoPlayer.GetComponent<AudioSource>();
                if (audioSource == null)
                    audioSource = videoPlayer.gameObject.AddComponent<AudioSource>();

                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f;
                audioSource.volume = volume;
                audioSource.minDistance = 0.5f;
                audioSource.maxDistance = 8f;
                videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                videoPlayer.SetTargetAudioSource(0, audioSource);
                break;
            default:
                videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                break;
        }
    }

    void ApplyVideoTexture(Texture frame)
    {
        if (targetRenderer == null || frame == null)
            return;

        if (runtimeMaterial == null)
            runtimeMaterial = targetRenderer.material;

        runtimeMaterial.mainTexture = frame;

        if (runtimeMaterial.HasProperty("_BaseMap"))
            runtimeMaterial.SetTexture("_BaseMap", frame);

        if (runtimeMaterial.HasProperty("_EmissionMap"))
            runtimeMaterial.SetTexture("_EmissionMap", frame);

        if (runtimeMaterial.HasProperty("_EmissionColor"))
            runtimeMaterial.EnableKeyword("_EMISSION");

        float scaleX = invertX ? -1f : 1f;
        float scaleY = invertY ? -1f : 1f;
        runtimeMaterial.mainTextureScale = new Vector2(scaleX, scaleY);
        runtimeMaterial.mainTextureOffset = new Vector2(invertX ? 1f : 0f, invertY ? 1f : 0f);

        if (runtimeMaterial.HasProperty("_BaseMap"))
        {
            runtimeMaterial.SetTextureScale("_BaseMap", runtimeMaterial.mainTextureScale);
            runtimeMaterial.SetTextureOffset("_BaseMap", runtimeMaterial.mainTextureOffset);
        }

        frame.filterMode = FilterMode.Bilinear;
    }

    IEnumerator TryStartPlayback(string videoUrl, Action<AudioMode> onAudioUsed, bool forceSilent = false)
    {
        foreach (AudioMode audioMode in BuildAudioAttempts(forceSilent))
        {
            playbackActive = false;
            lastFatalError = null;
            yield return RunPlaybackAttempt(videoUrl, audioMode);

            if (playbackActive)
            {
                onAudioUsed?.Invoke(audioMode);
                yield break;
            }
        }
    }

    List<AudioMode> BuildAudioAttempts(bool forceSilent)
    {
        var attempts = new List<AudioMode>();
        if (forceSilent || volume <= 0f)
        {
            attempts.Add(AudioMode.None);
            return attempts;
        }

        attempts.Add(AudioMode.Direct);
        attempts.Add(AudioMode.None);
        attempts.Add(AudioMode.Spatial);
        return attempts;
    }

    IEnumerator RunPlaybackAttempt(string videoUrl, AudioMode audioMode)
    {
        bool prepared = false;
        bool durationWarning = false;

        void OnPrepared(VideoPlayer vp) => prepared = true;

        void OnError(VideoPlayer vp, string message)
        {
            if (IsNonFatalDurationError(message))
            {
                durationWarning = true;
                sawDurationWarning = true;
                ConfigManager.WriteConsoleWarning(
                    $"{LogPrefix} {packageName}: duration warning ({audioMode}) — {message}");
                return;
            }

            lastFatalError = message;
            ConfigManager.WriteConsoleWarning($"{LogPrefix} {packageName}: player error — {message}");
        }

        ApplyAudioMode(audioMode);
        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.errorReceived += OnError;
        videoPlayer.Stop();
        videoPlayer.url = null;
        yield return null;
        yield return null;

        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        videoPlayer.isLooping = loop;
        videoPlayer.url = videoUrl;
        videoPlayer.Prepare();

        ConfigManager.WriteConsole($"{LogPrefix} {packageName}: prepare ({audioMode}) {videoUrl}");

        float deadline = Time.realtimeSinceStartup + PrepareTimeoutSeconds;
        while (!prepared && !durationWarning && string.IsNullOrEmpty(lastFatalError)
            && Time.realtimeSinceStartup < deadline)
        {
            yield return null;
        }

        if (!prepared && (durationWarning || string.IsNullOrEmpty(lastFatalError)))
        {
            ConfigManager.WriteConsole(
                $"{LogPrefix} {packageName}: prepare incomplete — trying Play() ({audioMode})");
        }

        if (!string.IsNullOrEmpty(lastFatalError))
        {
            videoPlayer.prepareCompleted -= OnPrepared;
            videoPlayer.errorReceived -= OnError;
            yield break;
        }

        if (playOnAwake)
            videoPlayer.Play();

        deadline = Time.realtimeSinceStartup + PlaybackTimeoutSeconds;
        while (Time.realtimeSinceStartup < deadline)
        {
            if (HasLiveVideoFrame())
            {
                playbackActive = true;
                ApplyVideoTexture(videoPlayer.texture);
                break;
            }

            if (!string.IsNullOrEmpty(lastFatalError))
                break;

            if (!videoPlayer.isPlaying && playOnAwake)
                videoPlayer.Play();

            yield return null;
        }

        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.errorReceived -= OnError;
    }

    bool HasLiveVideoFrame() =>
        videoPlayer != null
        && videoPlayer.texture != null
        && (videoPlayer.isPlaying || videoPlayer.frame > 0);

    /// <summary>Same URL format as MRConfigurationUI / cabinet preview — never raw C:\ paths.</summary>
    static string ToVideoPlayerUrl(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return null;

        string full = Path.GetFullPath(filePath);
        if (full.IndexOf("://", StringComparison.Ordinal) >= 0)
            return full;

        return "file:///" + full.Replace("\\", "/");
    }

    void OnDestroy()
    {
        playbackActive = false;
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            if (videoPlayer.gameObject != null)
                Destroy(videoPlayer.gameObject);
        }

        if (runtimeMaterial != null)
            Destroy(runtimeMaterial);
    }
}
