using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerController : MonoBehaviour
{
    public GlobalConfiguration globalConfiguration;
    public XROrigin xrorigin;
    public CharacterController characterController;
    public Transform cameraOffset;

    // Start is called before the first frame update
    void Start()
    {
        if (xrorigin == null)
            xrorigin = GetComponent<XROrigin>();
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
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

        if (cameraOffset == null)
        {
            ConfigManager.WriteConsoleError("[PlayerController.changeWithPlayerData] Camera Offset gameobject transform not found.");
        }
        else
        {
            Vector3 scale = new(player.scale, player.scale, player.scale);
            cameraOffset.localScale = scale;
            ConfigManager.WriteConsole($"[changeWithPlayerData] new player scale {player.scale}");
        }

        if (player.height == 0)
        {
            //calculated
            xrorigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
            ConfigManager.WriteConsole($"[changeWithPlayerData] new player eye height calculated from floor");
        }
        else
        {
            // about NotSpecified: when changing to this value after startup, the Tracking Origin Mode will not be changed.

            xrorigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Device;
            //adjust camera height when assigned.
            xrorigin.CameraYOffset = player.height;
            Vector3 localPosition = cameraOffset.localPosition;
            localPosition.y = player.height;
            cameraOffset.localPosition = localPosition;
            Vector3 center = characterController.center;
            center.z = 0;
            center.x = 0;
            characterController.center = center;
            // characterController.height = player.height + 0.1f;
            ConfigManager.WriteConsole($"[changeWithPlayerData] new player eye height {player.height}");
            ConfigManager.WriteConsole($"[changeWithPlayerData] {player.ShowHeightPlayers()}");
        }

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
