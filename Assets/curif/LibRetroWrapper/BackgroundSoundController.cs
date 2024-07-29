using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


public class BackgroundSoundController : MonoBehaviour
{
    public GameObject RoomConfigurationGameObject;

    private RoomConfiguration roomConfiguration;
    private bool playerIsInGame;

    // Start is called before the first frame update
    void Start()
    {
        roomConfiguration = RoomConfigurationGameObject.GetComponent<RoomConfiguration>();
        OnEnable();
        change();
    }
    void changeWithBackgroundData(ConfigInformation.Background background)
    {
        GameObject[] backgroundSoundObjects = GameObject.FindGameObjectsWithTag("backgroundsound");

        foreach (GameObject obj in backgroundSoundObjects)
        {
            AudioSource audioSource = obj.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.mute = background.muted != null ? (bool)background.muted : audioSource.mute;
                if (background.volume != null)
                {
                    float volume = (float)background.volume * 0.01f;
                    audioSource.volume = volume;
                }
                ConfigManager.WriteConsole($"[BackgroundSoundController] volume {audioSource.volume} muted: {audioSource.mute}");
            }
            else
            {
                ConfigManager.WriteConsoleWarning($"[BackgroundSoundController] {obj.name} doesn't have a AudioSource component");
            }
            
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
