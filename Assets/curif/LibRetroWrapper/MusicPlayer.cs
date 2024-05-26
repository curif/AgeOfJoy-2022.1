using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using Oculus.Interaction;
using UnityEngine.Networking;

public class MusicPlayer : MonoBehaviour
{
    private List<string> musicQueue = new List<string>();
    public bool isPlaying = false;
    public bool Loop = false;
    private int currentIndex = 0;
    private AudioSource audioSource;

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
/*
        AddMusic(ConfigManager.MusicDir + "/Talkshow_Boy_-_01_-_Three_Cheers_For_That_Fucking_Dickhead_Adrian.mp3");
        AddMusic(ConfigManager.MusicDir + "/better-day-186374.mp3");
        AddMusic(ConfigManager.MusicDir + "/Death_Grips_-_04_-_Lord_of_the_Game_ft_Mexican_Girl.mp3");
        Play();
*/
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
        musicQueue.Clear();
        Stop();
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

    public void Play()
    {
        if (musicQueue.Count > 0 && !isPlaying)
        {
            isPlaying = true;
            PlayMusic();
        }
    }

    public bool IsInQueue(string musicFilePath)
    {
        return musicQueue.Contains(musicFilePath);
    }

    private void PlayMusic()
    {
        if (currentIndex < musicQueue.Count)
        {
            string musicFilePath = musicQueue[currentIndex];
            ConfigManager.WriteConsole($"[MusicPlayer.PlayMusic] {musicFilePath}");

            StartCoroutine(PlayMusicCoroutine(musicFilePath));
        }
    }

    public void SetVolume(float amount)
    {
        audioSource.volume = Mathf.Clamp01(audioSource.volume + amount);
    }

    private IEnumerator PlayMusicCoroutine(string musicFilePath)
    {
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
            else
            {
                // https://github.com/mikepierce/internet-radio-streams
                // is a stream
                ConfigManager.WriteConsole($"[MusicPlayer.PlayMusicCoroutine] Stream: {filePath}");

                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.UNKNOWN))
                {
                    DownloadHandlerAudioClip dHA = new DownloadHandlerAudioClip(string.Empty, AudioType.UNKNOWN);
                    dHA.streamAudio = true;
                    www.downloadHandler = dHA;
                    www.SendWebRequest();
                    while (www.downloadProgress < 1)
                    {
                        ConfigManager.WriteConsole($"[MusicPlayer.PlayMusicCoroutine] Failed to load a stream file: {filePath} progress: {www.downloadProgress}");
                        yield return new WaitForSeconds(.5f);
                    }

                    if (www.result == UnityWebRequest.Result.ConnectionError || 
                        www.result == UnityWebRequest.Result.ProtocolError)
                    {
                        ConfigManager.WriteConsole($"[MusicPlayer.PlayMusicCoroutine] Failed to load a stream url: " + musicFilePath);
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
            }
        }
        else
        {
            ConfigManager.WriteConsole($"[MusicPlayer.PlayMusicCoroutine] Failed to load audio empty audio file: " + musicFilePath);
        }

        currentIndex++;

        if (currentIndex >= musicQueue.Count)
        {
            if (Loop)
            {
                currentIndex = 0;
            }
            else
            {
                isPlaying = false;
                yield break;
            }
        }

        PlayMusic();
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


    public void Next()
    {
        if (musicQueue.Count > 0)
        {
            currentIndex++;
            if (currentIndex >= musicQueue.Count)
            {
                if (Loop)
                {
                    currentIndex = 0;
                }
                else
                {
                    currentIndex = musicQueue.Count - 1;
                    Stop();
                    return;
                }
            }
            Stop();
            Play();
        }
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
        isPlaying = false;
        audioSource.Stop();
        audioSource.clip = null;
        currentIndex = Mathf.Clamp(currentIndex, 0, musicQueue.Count - 1);
    }
}
