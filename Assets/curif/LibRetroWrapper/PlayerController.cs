using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;

public class PlayerController : MonoBehaviour
{
    public GlobalConfiguration globalConfiguration;
    public XROrigin xrorigin;
    public Transform cameraOffset;

    // Start is called before the first frame update
    void Start()
    {
        if (xrorigin == null)
            xrorigin = GetComponent<XROrigin>();
        if (cameraOffset == null)
        {
            GameObject co = GameObject.Find("CameraOffset");
            if (co != null)
                cameraOffset = co.transform;
            else
                ConfigManager.WriteConsoleError("[PlayerController] Camera Offset gameobject transform not found.");
        }
        OnEnable();
        change();
    }

    void changeWithPlayerData(ConfigInformation.Player player)
    {
        if (player.height == 0)
        {
            //calculated
            xrorigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
            ConfigManager.WriteConsole($"[changeWithPlayerData] new player eye height calculated from floor");
        }
        else
        {
            xrorigin.CameraYOffset = player.height;
            xrorigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Device;
            ConfigManager.WriteConsole($"[changeWithPlayerData] new player eye height {player.height}");
        }

        if (cameraOffset == null)
        {
            ConfigManager.WriteConsoleError("[PlayerController.changeWithPlayerData] Camera Offset gameobject transform not found.");
            return;
        }

        Vector3 scale = new(player.scale, player.scale, player.scale);
        cameraOffset.localScale = scale;
        ConfigManager.WriteConsole($"[changeWithPlayerData] new scale {player.scale}");

        return;
    }

    void change()
    {
        if (globalConfiguration?.Configuration?.player != null)
            changeWithPlayerData(globalConfiguration.Configuration.player);
        else
            changeWithPlayerData(ConfigInformation.PlayerDefault());
    }
    void OnGlobalConfigChanged()
    {
        change();
    }

    void OnEnable()
    {
        // Listen for the config reload message
        globalConfiguration?.OnGlobalConfigChanged.AddListener(OnGlobalConfigChanged);
    }

    void OnDisable()
    {
        // Stop listening for the config reload message
        globalConfiguration?.OnGlobalConfigChanged.RemoveListener(OnGlobalConfigChanged);
    }
}
