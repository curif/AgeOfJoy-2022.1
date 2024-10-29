using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

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
        if (!musicQueue.Contains(musicFilePath))
        {
            if (!File.Exists(musicFilePath))
                throw new Exception($"Audio file doesn't exists: {musicFilePath}");
            
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
        if (currentIndex == -1 && musicQueue.Count > 0)
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
            if (currentIndex == -1 || musicQueue.Count == 0)
            { 
                yield return new WaitForSeconds(1f);
                continue;
            }

            musicFilePath = musicQueue[currentIndex];
            ConfigManager.WriteConsole($"[MusicPlayer.PlayMusic] {musicFilePath}");
            
            string filePath;
            if (musicFilePath.EndsWith(".strm"))
                filePath = ReadUrlFromStrmFile(musicFilePath);
            else
                filePath = "file://" + musicFilePath;

            if (!string.IsNullOrEmpty(filePath))
            {
                if (filePath.StartsWith("file:"))
                {
                    using (WWW www = new WWW(filePath))
                    {
                        // Wait until the audio file is loaded
                        yield return www;

                        // Check if there was an error loading the audio file
                        if (string.IsNullOrEmpty(www.error))
                        {
                            AudioClip clip = www.GetAudioClip(false, false);

                            if (clip != null)
                            {
                                audioSource.clip = clip;
                                audioSource.Play();

                                yield return new WaitForSeconds(clip.length);
                            }
                            else
                            {
                                ConfigManager.WriteConsole($"[MusicPlayer.PlayMusicCoroutine] Failed to load audio clip: " + musicFilePath);
                            }
                        }
                        else
                        {
                            ConfigManager.WriteConsoleWarning($"[MusicPlayer.PlayMusicCoroutine] Failed to load audio file: " + musicFilePath + ", Error: " + www.error);
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
