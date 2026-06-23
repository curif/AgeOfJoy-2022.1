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

public class PlayerController : MonoBehaviour
{
    public GlobalConfiguration globalConfiguration;
    public XROrigin xrorigin;
    public CharacterController characterController;
    public ChangeControls changeControls;
    public GameObject PlayerControllerGameObject;
    public Transform cameraOffset; // Assign the CameraOffset GameObject
    public GameObject OVRPlayerGameObject; // Assign the root XR Origin (OVRPlayer)

    private Coroutine coroutine;

    private const float IntroGalleryPFMegaFloorY = 0.4826951f; //compensate. Same value that IntroGallery's PF MegaFloor.
    
    [SerializeField]
    float cameraYOffset;

    private bool isListenerAdded = false;
    /*
    public float CameraYOffset
    {
        get => cameraYOffset;
        set
        {
            cameraYOffset = value;
            if (cameraYOffset > 0)
                AdjustCameraYOffset();
            else
                changeToCalculatedFromFloor();
        }
    }
    */

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

    // public void JumpTo(Vector3 pos)
    // {
    //     Vector3 pos = new Vector3()
    //     MoveCameraToWorldLocation
    // }

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
    public void ForceHeightBad3(float targetHeight)
    {
        // This is your real-world head height (headset Y relative to rig origin)
        float cameraLocalY = Camera.main.transform.localPosition.y;

        // Calculate how much to shift the rig to get the desired world height
        float diff = targetHeight - cameraLocalY;

        // Apply offset to XR Origin (root)
        Vector3 pos = OVRPlayerGameObject.transform.position;
        pos.y = diff; // Set absolute rig position so camera ends up at target height
        OVRPlayerGameObject.transform.position = pos;

        Debug.Log($"[ForceHeight] Target: {targetHeight:F2}, Local Camera Y: {cameraLocalY:F2}, Setting Rig Y to: {diff:F2}");
    }



    public void ForceHeightBad2(float targetHeight)
    {
        // Get current world camera height
        float currentCameraHeight = Camera.main.transform.position.y;

        // Get the difference between current and target height
        float diff = currentCameraHeight - targetHeight;

        // Move the entire XR rig (PlayerController or root) downward to match the desired height
        Vector3 localPos = PlayerControllerGameObject.transform.localPosition;
        localPos.y -= diff;
        PlayerControllerGameObject.transform.localPosition = localPos;

        Debug.Log($"[ForceHeight] Target: {targetHeight:F2}, Current: {currentCameraHeight:F2}, Diff: {diff:F2}");
    }

    public float GetHeightBAd()
    {
        // Assumes the MainCamera is the HMD camera
        return Camera.main.transform.localPosition.y;
    }

    //activate playerPositionDebug to debug the player behavior. Deactivate on production.
    public void ForceHeightBad(float height)
    {
        //[AGE][PlayerController.ForceHeight] height: 1.261735 PlayerControllerGameObject: (0.00, -0.34, 0.00).Actual CameraYOffset(xrorigin):1.6
        Vector3 playerControllerLocalPosition = PlayerControllerGameObject.transform.localPosition;
        Vector3 cameraOffsetLocalPosition = xrorigin.CameraFloorOffsetObject.transform.localPosition;
        float realHeight = cameraOffsetLocalPosition.y + playerControllerLocalPosition.y;
        float diff = realHeight - height;

        playerControllerLocalPosition.y -= diff;
        PlayerControllerGameObject.transform.localPosition = playerControllerLocalPosition;
        ConfigManager.WriteConsole($"[PlayerController.ForceHeight] height: {height} Player RealHeight: {realHeight} PlayerControllerGameObject: {PlayerControllerGameObject.transform.localPosition}. Actual CameraYOffset (xrorigin):{xrorigin.CameraYOffset}");
     }

    //activate playerPositionDebug to debug the player behavior. Deactivate on production.
    public void AdjustCameraYOffset()
    {
        Vector3 localPosition = PlayerControllerGameObject.transform.localPosition;

        if (cameraYOffset == 0)
            cameraYOffset = ConfigInformation.Player.avgHeigh;
        else if (cameraYOffset < ConfigInformation.Player.minimalHeight)
            cameraYOffset = ConfigInformation.Player.minimalHeight;

#if UNITY_EDITOR
        ConfigInformation.Player.ShowHeightPlayers();
        localPosition.y = cameraYOffset;
#else
        ConfigManager.WriteConsole($"[PlayerController.AdjustCameraYOffset] actual cameraOfset transform: {cameraOffset.localPosition}");

        /*
        adjust the gameobject that controls the player position Y to a position that is the main floor Y (introgallery)
        plus the difference between the average height and the height set by the user.
        */
        //localPosition.y = /*IntroGalleryPFMegaFloorY +*/ cameraYOffset - ConfigInformation.Player.avgHeigh ;
        localPosition.y = ConfigInformation.Player.HeightCalculatorPlayerController(cameraYOffset);
#endif

        PlayerControllerGameObject.transform.localPosition = localPosition;
        ConfigManager.WriteConsole($"[PlayerController.AdjustCameraYOffset] height: {cameraYOffset} PlayerControllerGameObject: {PlayerControllerGameObject.transform.localPosition}. Actual CameraYOffset (xrorigin):{xrorigin.CameraYOffset}");
        

        //if (coroutine == null)
        //    coroutine = StartCoroutine(SetFakeHeightRoutine(cameraYOffset));

#if XR_MODULE_AVAILABLE

        //ConfigManager.WriteConsole($"[AdjustCameraYOffset] XR_MODULE_AVAILABLE CameraYOffset = {cameraYOffset}");

        /*
        this should be done by MoveOffsetHeight and MoveOffsetHeight(float y) in xrOrigin 
        when CameraYOffset is assigned, but don't work even when XR_MODULE_AVAILABLE is defined.
        */
        //Vector3 localPosition = cameraOffset.localPosition;
        //localPosition.y = cameraYOffset;
        //cameraOffset.localPosition = localPosition;

#endif
    }
    /*
    private IEnumerator SetFakeHeightRoutine(float height)
    {
        Debug.Log($"[SetFakeHeightRoutine] Setting Player Height to FAKE ({height}m) (Device Tracking)");

        // 1. Set the desired offset VALUE first
        xrorigin.CameraYOffset = height;

        // 2. Request the mode switch
        xrorigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Device;

        // 3. Wait a frame for the system to potentially process the mode switch and recenter
        yield return null;

        // 4. (Optional but sometimes helpful) Re-assert the offset value
        // This ensures the internal MoveOffsetHeight uses the correct value AFTER the mode switch settles.
        xrorigin.CameraYOffset = height;

        ConfigManager.WriteConsole($"[SetFakeHeightRoutine] Fake Height routine finished. CameraFloorOffsetObject localPos: {xrorigin.CameraFloorOffsetObject.transform.localPosition}");

        coroutine = null;
    }
    void changeToCalculatedFromFloor()
    {
        xrorigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
        cameraYOffset = 0f;
        xrorigin.CameraYOffset = 0f;

        ConfigManager.WriteConsole($"[changeToCalculatedFromFloor] new player eye height calculated from floor");
    }
    */

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
        //ForceHeight(player.height);

        // characterController.height = player.height + 0.1f;
        ConfigManager.WriteConsole($"[changeWithPlayerData] new player eye height {player.height}");
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