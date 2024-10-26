using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


public class BackgroundSoundController : MonoBehaviour
{
    public GameObject RoomConfigurationGameObject;

    private RoomConfiguration roomConfiguration;
    private bool playerIsInGame;
    private bool isListenerAdded = false;

    // Start is called before the first frame update
    void Start()
    {
        if (RoomConfigurationGameObject == null)
        {
            ConfigManager.WriteConsoleWarning($"[BackgroundSoundController] {name} Devs must set the Room Configuration game object. This background sound controller can't run. EXIT.");
            return;
        }

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
                //ConfigManager.WriteConsole($"[BackgroundSoundController] volume {audioSource.volume} muted: {audioSource.mute}");
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

    void addListener()
    {
        if (isListenerAdded) return;
        roomConfiguration?.OnRoomConfigChanged.AddListener(OnRoomConfigChanged);
        isListenerAdded = true;
    }
    void removeListener()
    {
        if (!isListenerAdded) return;
        roomConfiguration?.OnRoomConfigChanged.RemoveListener(OnRoomConfigChanged);
        isListenerAdded = false;
    }

    void OnEnable()
    {
        addListener();
    }

    void OnDisable()
    {
        removeListener();
    }
}
