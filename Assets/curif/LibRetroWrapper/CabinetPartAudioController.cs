using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;

public class CabinetPartAudioController : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioMixerGroup gameModeGroup;
    public AudioMixer audioMixer;
    public string filePath;
    public string cabPathBase;

    private void Awake()
    {
        // Check if the GameObject already has an AudioSource component
        audioSource = GetComponent<AudioSource>();

        // If no AudioSource is found, add one
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();


    }

    private void SetAudioMixer()
    {
        if (gameModeGroup != null)
            return;

        if (audioMixer == null)
            audioMixer = Resources.Load<AudioMixer>("Mixers/SpatializerMixer");

        if (audioMixer != null && gameModeGroup == null)
        {
            AudioMixerGroup[] matchingGroups = audioMixer.FindMatchingGroups("Game");
            gameModeGroup = matchingGroups[0];
            audioSource.outputAudioMixerGroup = gameModeGroup;
        }
    }

    /// <summary>
    /// Configures the AudioSource based on the provided Part object's audio settings.
    /// </summary>
    /// <param name="part">The Part object containing audio settings from YAML.</param>
    public static CabinetPartAudioController GetOrAdd(GameObject go, string cabPathBase, CabinetInformation.CabinetAudioPart audio)
    {
        if (audio != null)
        {
            CabinetPartAudioController audioCtrl = go.GetComponent<CabinetPartAudioController>();
            if (audioCtrl == null)
                audioCtrl = go.AddComponent<CabinetPartAudioController>();
            audioCtrl.SetVolume(audio.volume);
            audioCtrl.SetLoop(audio.loop);
            audioCtrl.SetMinMaxDistance(audio.distance.min, audio.distance.max);
            audioCtrl.SetAudioMixer();
            audioCtrl.cabPathBase = cabPathBase.Replace("\\", "/");
            audioCtrl.SetFilePath(audio.file);
            return audioCtrl;
        }
        return null;
    }

    private void SetFilePath(string audioFile)
    {
        filePath = cabPathBase + "/" + audioFile;
    }


    private IEnumerator LoadAudioClipAndPlay(bool play)
    {
        if (System.IO.File.Exists(filePath))
        {
            SetAudioMixer();

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file:///" + filePath, GetAudioType()))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    ConfigManager.WriteConsoleWarning($"Loading audio clip {filePath}: {www.error}");
                }
                else
                {
                    audioSource.clip = DownloadHandlerAudioClip.GetContent(www);
                    if (play)
                        audioSource.Play();

                    yield break;
                }
            }
        }
        else
        {
            ConfigManager.WriteConsoleWarning($"Audio file not found at path: {filePath}");
        }
    }

    public void AssignAudioClip(string audioFileName)
    {
        SetFilePath(audioFileName);
        AssignAudioClip(false);
    }

    public void AssignAudioClip(bool play = true)
    {
        StartCoroutine(LoadAudioClipAndPlay(play));
    }

    private AudioType GetAudioType()
    {
        string extension = System.IO.Path.GetExtension(filePath).ToLower();
        switch (extension)
        {
            case ".wav":
                return AudioType.WAV;
            case ".mp3":
                return AudioType.MPEG;
            case ".ogg":
                return AudioType.OGGVORBIS;
            default:
                Debug.LogError($"Unsupported audio format: {extension}");
                return AudioType.UNKNOWN;
        }
    }

    public void PlayAudio()
    {
        if (audioSource.clip != null)
        {
            audioSource.Play();
        }
        else
        {
            AssignAudioClip();
        }
    }

    public void StopAudio()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void PauseAudio()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    public void SetLoop(bool shouldLoop)
    {
        audioSource.loop = shouldLoop;
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume);
    }

    public void SetMinMaxDistance(float minDistance, float maxDistance)
    {
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
    }

}
