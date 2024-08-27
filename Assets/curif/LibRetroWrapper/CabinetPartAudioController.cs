using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using static CabinetInformation;

public class CabinetPartAudioController : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioMixerGroup attractModeGroup;

    private void Awake()
    {
        // Check if the GameObject already has an AudioSource component
        audioSource = GetComponent<AudioSource>();

        // If no AudioSource is found, add one
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Set the output audio mixer group
        audioSource.outputAudioMixerGroup = attractModeGroup;
    }

    /// <summary>
    /// Configures the AudioSource based on the provided Part object's audio settings.
    /// </summary>
    /// <param name="part">The Part object containing audio settings from YAML.</param>
    public static CabinetPartAudioController GetOrAdd(GameObject go, CabinetAudio audio)
    {
        if (audio != null)
        {
            CabinetPartAudioController audioCtrl = go.GetComponent<CabinetPartAudioController>();
            if (audioCtrl ==  null)
                audioCtrl = go.AddComponent<CabinetPartAudioController>();
            audioCtrl.AssignAudioClip(audio.file);
            audioCtrl.SetVolume(audio.volume);
            audioCtrl.SetLoop(audio.loop);
            audioCtrl.SetMinMaxDistance(audio.distance.min, audio.distance.max);
            return audioCtrl;
        }
        return null;
    }

    private IEnumerator LoadAudioClip(string filePath)
    {
        if (System.IO.File.Exists(filePath))
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, GetAudioType(filePath)))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Error loading audio clip: {www.error}");
                }
                else
                {
                    audioSource.clip = DownloadHandlerAudioClip.GetContent(www);
                }
            }
        }
        else
        {
            Debug.LogError($"Audio file not found at path: {filePath}");
        }
    }

    private AudioType GetAudioType(string filePath)
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

    public void AssignAudioClip(string filePath)
    {
        StartCoroutine(LoadAudioClip(filePath));
    }
}
