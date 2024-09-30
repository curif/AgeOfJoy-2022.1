using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioClip))]

public class GameAudioPlayer : MonoBehaviour
{
    public string path;
    public AudioClip audioClip;
    public AudioSource audioSource;
    public bool isPlaying;

    // Use this for initialization
    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    IEnumerator LoadAudioFromFile()
    {
        AudioType audioType;
        if (!File.Exists(path))
        {
            ConfigManager.WriteConsoleWarning($"[GameAudioPlayer] file not found: {path}");
            yield break;
        }

        // Check the file extension to determine the audio type
        if (path.EndsWith(".mp3"))
        {
            audioType = AudioType.MPEG;
        }
        else if (path.EndsWith(".wav"))
        {
            audioType = AudioType.WAV;
        }
        else if (path.EndsWith(".ogg"))
        {
            audioType = AudioType.OGGVORBIS;
        }
        else
        {
            ConfigManager.WriteConsoleError($"[GameAudioPlayer] Unsupported audio format: {path}");
            yield break; // Exit the coroutine if unsupported format
        }

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, audioType))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                ConfigManager.WriteConsoleError($"[GameAudioPlayer] loading: {path} - {www.error}");
            }
            else
            {
                audioClip = DownloadHandlerAudioClip.GetContent(www);
                play();
            }
        }
    }

    private void play()
    {
        if (isPlaying)
            return;
        audioSource.clip = audioClip;
        audioSource.loop = true;
        audioSource.Play();
        isPlaying = true; //don't use audioSource.IsPlaying bcz it is shared with the video file.
    }
    public void Play()
    {
        if (string.IsNullOrEmpty(path))
            return;

        if (audioClip == null)
            StartCoroutine(LoadAudioFromFile());
        //else if (!audioSource.isPlaying)
        else
            play();
        
    }

    public void Stop()
    {
        if (audioSource == null)
            return;
        audioSource.Stop();
        audioSource.clip = null;
        isPlaying = false;
    }

}
