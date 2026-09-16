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
using System.Collections.Specialized;
using UnityEditor.XR.LegacyInputHelpers;
using static OVRHaptics;

/*
Player rig height contract (Floor tracking mode):

  rig root (CharacterController, XROrigin)          <- rests on the virtual floor
    PlayerControllerGameObject ("PlayerController")  <- eye-height OVERRIDE node, local Y = 0 by default
      CameraOffset (XROrigin.CameraFloorOffsetObject, scale 0.9)  <- local Y = 0 in Floor mode
        Main Camera                                   <- tracked pose, Y = eye height above the real floor

In Floor mode the headset already reports the eye height above the Guardian floor, so the
app must NOT add a static lift of its own: any local Y on the override node raises the
player's real floor above the virtual floor by exactly that amount. The configured player
height (config cabinet) therefore no longer moves the rig on device. The override node is
reserved for scripted, dynamic eye-height snaps (AGEBasic PLAYERSETHEIGHT on sit-down
cabinets), computed against the measured tracked height so they land the same for every
player. In the editor (no HMD) the camera sits at local 0, and the configured height is used
as a simulated eye height instead.
*/
public class PlayerController : MonoBehaviour
{
    public GlobalConfiguration globalConfiguration;
    public XROrigin xrorigin;
    public CharacterController characterController;
    public ChangeControls changeControls;
    public GameObject PlayerControllerGameObject;
    public Transform cameraOffset; // Assign the CameraOffset GameObject
    public GameObject OVRPlayerGameObject; // Assign the root XR Origin (OVRPlayer)

    [SerializeField]
    float cameraYOffset;

    private bool isListenerAdded = false;

    /// <summary>
    /// Configured player height (config cabinet / global configuration). On device this is
    /// informational only: the rig is not lifted by it (see the height contract above).
    /// </summary>
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

        if (changeControls == null)
            changeControls = GetComponent<ChangeControls>();

        OnEnable();
        change();
    }

    /// <summary>
    /// Current eye height above the rig origin (== above the virtual floor while the rig rests on
    /// it). Includes the tracked headset height, the CameraOffset scale and any active override.
    /// </summary>
    public float MeasuredEyeHeight
    {
        get
        {
            if (xrorigin != null && xrorigin.Camera != null)
                return xrorigin.CameraInOriginSpaceHeight;
            return PlayerControllerGameObject != null ? PlayerControllerGameObject.transform.localPosition.y : 0f;
        }
    }

    /// <summary>
    /// Eye height the player would have with no override applied (override node at local Y = 0).
    /// </summary>
    public float NaturalEyeHeight
    {
        get
        {
            float overrideY = PlayerControllerGameObject != null ? PlayerControllerGameObject.transform.localPosition.y : 0f;
            return MeasuredEyeHeight - overrideY;
        }
    }

    /// <summary>
    /// Dynamic eye-height snap used by AGEBasic PLAYERSETHEIGHT: places the player's eyes exactly
    /// <paramref name="eyeHeight"/> meters above the virtual floor, whatever their real height.
    /// Cleared by <see cref="ClearEyeHeightOverride"/> (room change, config reload).
    /// </summary>
    public void SetEyeHeightOverride(float eyeHeight)
    {
        if (PlayerControllerGameObject == null)
            return;

        float target = Mathf.Max(eyeHeight, ConfigInformation.Player.minimalHeight);
        Vector3 localPosition = PlayerControllerGameObject.transform.localPosition;
        localPosition.y = target - NaturalEyeHeight;
        PlayerControllerGameObject.transform.localPosition = localPosition;

        ConfigManager.WriteConsole($"[PlayerController.SetEyeHeightOverride] eye height {target} natural: {NaturalEyeHeight} override Y: {localPosition.y}");
    }

    /// <summary>
    /// Returns the override node to its baseline (0 on device; simulated eye height in the editor).
    /// </summary>
    public void ClearEyeHeightOverride()
    {
        AdjustCameraYOffset();
    }

    /// <summary>
    /// Applies the configured player height. On device (Floor tracking) this resets the override
    /// node to local Y = 0: the headset supplies the eye height and any static lift here would
    /// float the player above the floor. In the editor the configured value simulates eye height.
    /// </summary>
    public void AdjustCameraYOffset()
    {
        if (PlayerControllerGameObject == null)
            return;

        Vector3 localPosition = PlayerControllerGameObject.transform.localPosition;

        if (cameraYOffset == 0)
            cameraYOffset = ConfigInformation.Player.avgHeigh;
        else if (cameraYOffset < ConfigInformation.Player.minimalHeight)
            cameraYOffset = ConfigInformation.Player.minimalHeight;

#if UNITY_EDITOR
        ConfigInformation.Player.ShowHeightPlayers();
        // No HMD: the camera sits at local 0, so the configured height stands in for the tracked eye height.
        localPosition.y = cameraYOffset;
#else
        // Floor tracking: never lift the rig statically (see the height contract at the top of this file).
        localPosition.y = 0f;
#endif

        PlayerControllerGameObject.transform.localPosition = localPosition;
        ConfigManager.WriteConsole($"[PlayerController.AdjustCameraYOffset] configured height: {cameraYOffset} override node: {PlayerControllerGameObject.transform.localPosition} cameraOffset: {(cameraOffset != null ? cameraOffset.localPosition.ToString() : "?")} tracking: {(xrorigin != null ? xrorigin.CurrentTrackingOriginMode.ToString() : "?")}");
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

    // In colocated Mixed Reality the camera offset scale (0.9 in VR) must be 1:1 with the
    // physical room: it scales the headset's tracked motion, while MRUK anchors live at
    // scale 1, so any scale != 1 makes virtual surfaces drift relative to passthrough as
    // the player moves. We force scale 1 on MR entry and restore the VR scale on exit.
    private float? mrSavedScale;

    public bool IsMrScaleActive => mrSavedScale.HasValue;

    public void EnterMrColocatedScale()
    {
        if (cameraOffset == null)
            return;
        if (mrSavedScale == null)
            mrSavedScale = playerScale;
        PlayerScale = 1f;
        ConfigManager.WriteConsole($"[PlayerController] MR colocated scale 1:1 (VR scale saved={mrSavedScale})");
    }

    public void RestoreScaleFromMr()
    {
        if (mrSavedScale == null)
            return;
        float restore = mrSavedScale.Value;
        mrSavedScale = null;
        PlayerScale = restore;
        ConfigManager.WriteConsole($"[PlayerController] restored VR player scale {restore}");
    }


    void changeWithPlayerData(ConfigInformation.Player player)
    {
        //player scale
        if (cameraOffset == null)
        {
            ConfigManager.WriteConsoleError("[PlayerController.changeWithPlayerData] Camera Offset gameobject transform not found.");
            return;
        }

#if ADJUST_SCALE
            PlayerScale = player.scale;
#else
            PlayerScale = 0.9f;
#endif

        CameraYOffset = player.height;

        ConfigManager.WriteConsole($"[changeWithPlayerData] configured player height {player.height} (rig not lifted in Floor tracking mode)");
        ConfigManager.WriteConsole($"[changeWithPlayerData] {ConfigInformation.Player.ShowHeightPlayers()}");
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


    void addListener()
    {
        if (isListenerAdded) return;
        globalConfiguration?.OnGlobalConfigChanged.AddListener(OnGlobalConfigChanged);
        isListenerAdded = true;
    }
    void removeListener()
    {
        if (!isListenerAdded) return;
        globalConfiguration?.OnGlobalConfigChanged.RemoveListener(OnGlobalConfigChanged);
        isListenerAdded = false;
    }

    void OnEnable()
    {
        // Listen for the config reload message
        addListener();
    }

    void OnDisable()
    {
        // Stop listening for the config reload message
        removeListener();
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
