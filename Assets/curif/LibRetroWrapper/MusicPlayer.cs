using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

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

    public void ClearQueue()
    {
        musicQueue.Clear();
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
        string filePath = "file://" + musicFilePath;
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
                ConfigManager.WriteConsole($"[MusicPlayer.PlayMusicCoroutine] FFailed to load audio file: " + musicFilePath + ", Error: " + www.error);
            }
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
