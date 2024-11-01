using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.Networking;

public class MusicPlayer : MonoBehaviour
{
    private List<string> musicQueue = new List<string>();
    public bool Loop = false;
    private int currentIndex = 0;
    private AudioSource audioSource;
    private Coroutine coroutine;

    public bool IsCyclic
    {
        get { return Loop; }
        set { Loop = value; }
    }

    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.playOnAwake = false;

        currentIndex = -1;
        Play();
    }

    public void AddMusic(string musicFilePath)
    {
        if (musicFilePath.EndsWith(".m3u"))
        {
            StartCoroutine(LoadPlaylist(musicFilePath));
        }
        else if (!musicQueue.Contains(musicFilePath))
        {
            if (!File.Exists(musicFilePath))
                throw new Exception($"Audio file or list doesn't exists: {musicFilePath}");
            
            ConfigManager.WriteConsole($"[MusicPlayer.AddMusic] {musicFilePath}");
            musicQueue.Add(musicFilePath);
            Play();
        }
    }

    public void RemoveMusic(string musicFilePath)
    {
        if (musicQueue.Contains(musicFilePath))
        {
            ConfigManager.WriteConsole($"[MusicPlayer.RemoveMusic] {musicFilePath}");
            musicQueue.Remove(musicFilePath);
        }
    }

    public int CountQueue()
    {
        return musicQueue.Count;
    }

    public void ClearQueue()
    {
        Stop();
        musicQueue.Clear();
    }

    public void ResetQueue()
    {
        if (musicQueue.Count > 0)
        {
            Stop();
            currentIndex = 0;
            Play();
        }
    }

    public string Separated(string separator)
    {
        return string.Join(separator, musicQueue);
    }

    public void Play()
    {
        if (currentIndex <= -1 && musicQueue.Count > 0)
            currentIndex = 0;

        if (coroutine == null)
            coroutine = StartCoroutine(PlayMusicCoroutine());
    }

    public bool IsInQueue(string musicFilePath)
    {
        return musicQueue.Contains(musicFilePath);
    }
    public void SetVolume(float amount)
    {
        audioSource.volume = Mathf.Clamp01(audioSource.volume + amount);
    }

    private IEnumerator PlayMusicCoroutine()
    {
        string musicFilePath;

        while (true)
        {
            if (currentIndex <= -1 || musicQueue.Count == 0)
            { 
                yield return new WaitForSeconds(2f);
                continue;
            }

            musicFilePath = musicQueue[currentIndex];
            ConfigManager.WriteConsole($"[MusicPlayer.PlayMusic] {musicFilePath}");
            
            string filePath;
            if (musicFilePath.EndsWith(".strm"))
                filePath = ReadUrlFromStrmFile(musicFilePath);
            else if (musicFilePath.EndsWith(".m3u"))
            {
                yield return StartCoroutine(LoadPlaylist(musicFilePath));
                RemoveMusic(musicFilePath);
                next();
                continue;
            }
            else
                filePath = "file://" + musicFilePath;

            if (!string.IsNullOrEmpty(filePath))
            {
                if (filePath.StartsWith("file:"))
                {
                    ConfigManager.WriteConsole($"[MusicPlayer.PlayMusicCoroutine] File: {filePath}");

                    AudioType audioType = DetectAudioTypeFromExtension(filePath);
                    if (audioType == AudioType.UNKNOWN)
                    {
                        ConfigManager.WriteConsoleError($"[MusicPlayer.PlayMusicCoroutine] Unsupported or unknown file extension: " + musicFilePath);
                        RemoveMusic(musicFilePath);
                        next(); 
                        continue;
                    }

                    using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, audioType))
                    {
                        yield return www.SendWebRequest();

                        // Track download progress
                        while (!www.isDone)
                        {
                            ConfigManager.WriteConsole($"Download progress: {www.downloadProgress * 100}%");
                            yield return null; // Wait for the next frame before checking progress again
                        }

                        if (www.result == UnityWebRequest.Result.ConnectionError ||
                            www.result == UnityWebRequest.Result.ProtocolError)
                        {
                            ConfigManager.WriteConsoleError($"[MusicPlayer.PlayMusicCoroutine] Failed to load a file url: " + musicFilePath);
                            RemoveMusic(musicFilePath);
                        }
                        else
                        {
                            AudioClip audioClip = null;
                            try
                            {
                                audioClip = DownloadHandlerAudioClip.GetContent(www);
                                audioSource.clip = audioClip;
                                audioSource.loop = true; // Ensure continuous playback
                                audioSource.Play();
                            }
                            catch (Exception e)
                            {
                                ConfigManager.WriteConsoleException($"[MusicPlayer.PlayMusicCoroutine] Failed to load a file: " + musicFilePath, e);
                                RemoveMusic(musicFilePath);
                            }

                            if (audioClip != null)
                                yield return new WaitForSeconds(audioClip.length);

                        }
                    }
                }
                /*
                else
                {
                    // curated list https://github.com/mikepierce/internet-radio-streams
                    // is a stream
                    ConfigManager.WriteConsole($"[MusicPlayer.PlayMusicCoroutine] Stream: {filePath}");

                    using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.UNKNOWN))
                    {
                        DownloadHandlerAudioClip dHA = new DownloadHandlerAudioClip(string.Empty, AudioType.UNKNOWN);
                        dHA.streamAudio = true;
                        www.downloadHandler = dHA;
                        try
                        {
                            www.SendWebRequest();
                        }
                        catch (Exception e)
                        {
                            ConfigManager.WriteConsoleException($"[MusicPlayer.PlayMusicCoroutine] Downloading stream file: {filePath} progress: {www.downloadProgress}", e);
                            next();
                            continue;
                        }
                  
                        if (www.result == UnityWebRequest.Result.ConnectionError ||
                            www.result == UnityWebRequest.Result.ProtocolError)
                        {
                            ConfigManager.WriteConsoleError($"[MusicPlayer.PlayMusicCoroutine] Failed to load a stream url: " + musicFilePath);
                            RemoveMusic(musicFilePath);
                        }
                        else
                        {
                            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                            audioSource.clip = audioClip;
                            audioSource.Play();
                            audioSource.loop = true; // Ensure continuous playback
                        }
                    }
                
                }*/
            }
            else
            {
                ConfigManager.WriteConsole($"[MusicPlayer.PlayMusicCoroutine] Failed to load audio empty audio file: " + musicFilePath);
            }

            next();
        }
    }

    IEnumerator LoadPlaylist(string m3uPath)
    {
        UnityWebRequest request = UnityWebRequest.Get(m3uPath);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            ConfigManager.WriteConsoleError("[MusicPlayer.LoadPlaylist] Error loading playlist: " + request.error);
            yield break;
        }

        // Parse M3U content
        string[] lines = request.downloadHandler.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            // Skip comments (lines starting with '#')
            if (!line.StartsWith("#"))
            {
                string file = FunctionHelper.FileTraversalFree(Path.Combine(ConfigManager.MusicDir, line.Trim()),
                                                                ConfigManager.MusicDir);

                if (File.Exists(file)) AddMusic(file);
            }
        }
    }

    private AudioType DetectAudioTypeFromExtension(string path)
    {
        string extension = Path.GetExtension(path).ToLower();

        switch (extension)
        {
            case ".mp3": return AudioType.MPEG;
            case ".wav": return AudioType.WAV;
            case ".ogg": return AudioType.OGGVORBIS;
            //case ".aac": return AudioType.AAC;
            default: return AudioType.UNKNOWN;
        }
    }


    string ReadUrlFromStrmFile(string filePath)
    {
        try
        {
            return System.IO.File.ReadAllText(filePath).Trim();
        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleException($"Error reading .strm file: {filePath}", e);
            return null;
        }
    }

    private void next()
    {
        if (musicQueue.Count == 0)
        {
            currentIndex = -1;
            return;
        }

        currentIndex++;
        if (currentIndex >= musicQueue.Count)
            currentIndex = Loop ? 0 : -1;
    }

    //force to play the next one
    public void Next()
    {
        if (musicQueue.Count == 0)
            return;

        Stop();
        next();
        Play();
    }

    public void Previous()
    {
        if (musicQueue.Count > 0)
        {
            currentIndex--;
            if (currentIndex < 0)
            {
                if (Loop)
                {
                    currentIndex = musicQueue.Count - 1;
                }
                else
                {
                    currentIndex = 0;
                    Stop();
                    return;
                }
            }
            Stop();
            Play();
        }
    }

    private void Stop()
    {
        audioSource.Stop();
        audioSource.clip = null;
        currentIndex = Mathf.Clamp(currentIndex, 0, musicQueue.Count - 1);
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        
    }
}
