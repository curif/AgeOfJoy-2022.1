using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


public class BackgroundSoundController : MonoBehaviour
{
    public GameObject RoomConfigurationGameObject;

    private RoomConfiguration roomConfiguration;
    private bool playerIsInGame;
    private List<AudioSource> audioSources = new List<AudioSource>();

    // Start is called before the first frame update
    void Start()
    {
        roomConfiguration = RoomConfigurationGameObject.GetComponent<RoomConfiguration>();
        CacheAudioSources();
        OnEnable();
        change();
    }
    private void CacheAudioSources()
    {
        // Find all GameObjects with the tag "backgroundsound"
        GameObject[] backgroundSoundObjects = GameObject.FindGameObjectsWithTag("backgroundsound");

        // Clear the list to ensure it's empty before adding new AudioSources
        audioSources.Clear();

        // Iterate through each GameObject and get the AudioSource component
        foreach (GameObject obj in backgroundSoundObjects)
        {
            AudioSource audioSource = obj.GetComponent<AudioSource>();
            if (audioSource != null)
                audioSources.Add(audioSource);
        }
    }

    void changeWithBackgroundData(ConfigInformation.Background background)
    {
        foreach (AudioSource audioSource in audioSources)
        {
            audioSource.mute = background.muted != null ? (bool)background.muted : audioSource.mute;
            if (background.volume != null)
            {
                float volume = (float)background.volume * 0.01f;
                audioSource.volume = volume;
            }
            ConfigManager.WriteConsole($"[BackgroundSoundController] volume {audioSource.volume} muted: {audioSource.mute}");
        }
    }

    void change()
    {
        if (!playerIsInGame)
        {
            if (roomConfiguration?.Configuration?.audio?.background != null)
                changeWithBackgroundData(roomConfiguration.Configuration.audio.background);
            else
                changeWithBackgroundData(ConfigInformation.BackgroundDefault());
        }
        else
        {
            if (roomConfiguration?.Configuration?.audio?.inGameBackground != null)
                changeWithBackgroundData(roomConfiguration.Configuration.audio.inGameBackground);
            else
                changeWithBackgroundData(ConfigInformation.BackgroundInGameDefault());
        }
    }
    void OnRoomConfigChanged()
    {
        change();
    }

    public void InGame(bool inGame)
    {
        playerIsInGame = inGame;
        change();
    }

    void OnEnable()
    {
        // Listen for the config reload message
        roomConfiguration?.OnRoomConfigChanged.AddListener(OnRoomConfigChanged);

    }

    void OnDisable()
    {
        // Stop listening for the config reload message
        roomConfiguration?.OnRoomConfigChanged.RemoveListener(OnRoomConfigChanged);
    }
}
