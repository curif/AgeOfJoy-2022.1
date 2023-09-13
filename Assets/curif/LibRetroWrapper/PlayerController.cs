using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;

public class PlayerController : MonoBehaviour
{
    public GlobalConfiguration globalConfiguration;
    public XROrigin xrorigin;
    private AudioSource audioSource;
    private bool playerIsInGame;

    // Start is called before the first frame update
    void Start()
    {
        xrorigin = GetComponent<XROrigin>();
        OnEnable();
        change();
    }

    void changeWithPlayerData(ConfigInformation.Player player)
    {
        xrorigin.CameraYOffset = player.height;
        ConfigManager.WriteConsole($"[changeWithPlayerData] new player eye height {player.height}");
    }

    void change()
    {
        if (globalConfiguration?.Configuration?.player != null)
            changeWithPlayerData(globalConfiguration.Configuration.player);
        else
            changeWithPlayerData(ConfigInformation.PlayerDefault());
    }
    void OnRoomConfigChanged()
    {
        change();
    }

    void OnEnable()
    {
        // Listen for the config reload message
        globalConfiguration?.OnGlobalConfigChanged.AddListener(OnRoomConfigChanged);
    }

    void OnDisable()
    {
        // Stop listening for the config reload message
        globalConfiguration?.OnGlobalConfigChanged.RemoveListener(OnRoomConfigChanged);
    }
}
