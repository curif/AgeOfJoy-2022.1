#if ENABLE_VR || UNITY_GAMECORE
#define XR_MODULE_AVAILABLE
#endif

//PROBLEM: the camera offset scale interfere with the teleportation system. 
// https://github.com/curif/AgeOfJoy-2022.1/issues/237
//#define ADJUST_SCALE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEditor;

public class PlayerController : MonoBehaviour
{
    public GlobalConfiguration globalConfiguration;
    public XROrigin xrorigin;
    public CharacterController characterController;
    public Transform cameraOffset;

    [SerializeField]
    float cameraYOffset;
    public float CameraYOffset
    {
        get => cameraYOffset;
        set
        {
            cameraYOffset = value;
            AdjustCameraYOffset();
        }
    }

    [SerializeField]
    float playerScale;
    public float PlayerScale
    {
        get => playerScale;
        set
        {
            playerScale = value;
            AdjustScale();
        }
    }

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

    public void AdjustCameraYOffset()
    {
        xrorigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Device;
        xrorigin.CameraYOffset = cameraYOffset;

#if XR_MODULE_AVAILABLE
        ConfigManager.WriteConsole($"[AdjustCameraYOffset] XR_MODULE_AVAILABLE CameraYOffset = {cameraYOffset}");

        /*
        this should be done by MoveOffsetHeight and MoveOffsetHeight(float y) in xrOrigin 
        when CameraYOffset is assigned, but don't work even when XR_MODULE_AVAILABLE is defined.
        */
        Vector3 localPosition = cameraOffset.localPosition;
        localPosition.y = cameraYOffset;
        cameraOffset.localPosition = localPosition;
#endif
    }

    public void AdjustScale()
    {
        Vector3 scale = new(playerScale, playerScale, playerScale);
        cameraOffset.localScale = scale;
        ConfigManager.WriteConsole($"[AdjustScale] new player scale {playerScale}");

        Vector3 center = characterController.center;
        center.z = 0;
        center.x = 0;
        characterController.center = center;
    }

    void changeWithPlayerData(ConfigInformation.Player player)
    {
        //player scale
        if (cameraOffset == null)
        {
            ConfigManager.WriteConsoleError("[PlayerController.changeWithPlayerData] Camera Offset gameobject transform not found.");
            return;
        }
        else
        {
        #if ADJUST_SCALE
            PlayerScale = player.scale;
        #else
            PlayerScale = 0.9f;
        #endif
        }

        //player height
        if (player.height == 0f)
        {
            //calculated
            xrorigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
            // CameraYOffset = 0f;
            ConfigManager.WriteConsole($"[changeWithPlayerData] new player eye height calculated from floor");
        }
        else
        {
            // about NotSpecified: when changing to this value after startup, the 
            //   Tracking Origin Mode will not be changed.

            CameraYOffset = player.height;

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

#if UNITY_EDITOR
[CustomEditor(typeof(PlayerController))]
public class MyComponentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PlayerController myComponent = (PlayerController)target;

        // Draw the default inspector for the serialized properties
        DrawDefaultInspector();

        // Add a button to execute the method
        if (GUILayout.Button("Change player height"))
        {
            myComponent.AdjustCameraYOffset();
            myComponent.AdjustScale();
        }
    }
}
#endif