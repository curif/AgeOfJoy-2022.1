using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class DeviceController : MonoBehaviour
{
    /* DeviceController is attached to the Configuration gameobject
     * in the FixedScene.
     * GlobalConfiguration is the component attached to Configuration
     * in fixedScene.
     */ 
    public static DeviceType Device { get; private set; } = DeviceType.Unknown;
    public GlobalConfiguration globalConfiguration;

    static string deviceName;
    static string deviceModel;
    static UnityEngine.DeviceType deviceType;

    public static float WorldScale;
    public static float GameScale;
    public static OVRPlugin.FoveatedRenderingLevel GameFovLevel;
    public static OVRPlugin.FoveatedRenderingLevel WorldFovLevel;

    public static bool IsQ3 = false;
    private bool isListenerAdded = false;

    void Start()
    {
        ConfigManager.WriteConsole("[DeviceController] XRSettings.loadedDeviceName: " + XRSettings.loadedDeviceName);
        ConfigManager.WriteConsole("[DeviceController] SystemInfo.deviceModel: " + SystemInfo.deviceModel);
        ConfigManager.WriteConsole("[DeviceController] SystemInfo.deviceName: " + SystemInfo.deviceName);
        ConfigManager.WriteConsole("[DeviceController] SystemInfo.deviceType: " + SystemInfo.deviceType);

        List<XRDisplaySubsystem> displaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(displaySubsystems);
        foreach (var subsystem in displaySubsystems)
        {
            if (subsystem.running)
            {
                ConfigManager.WriteConsole("[DeviceController] subsystem.SubsystemDescriptor.id: " + subsystem.SubsystemDescriptor.id);
                ConfigManager.WriteConsole("[DeviceController] subsystem.running: " + subsystem.running);
            }
        }

        deviceName = SystemInfo.deviceName; //can be used in Start() only and in the main thread.
        deviceModel = SystemInfo.deviceModel;
        deviceType = SystemInfo.deviceType;

        ResetValues();
        OnGlobalConfigChanged();
        ApplySettings(false);

        addListener();

    }

    public static void ResetValues()
    {
        if (deviceName.Contains("Quest 3"))
            setAsQuest3();
        else if (deviceModel.Contains("Quest"))
            setAsQuest2();
        else if (deviceType.Equals(UnityEngine.DeviceType.Desktop))
            setAsComputer();
        else
            setAsComputer();
    }

    static void setAsQuest2()
    {
        WorldScale = 1f;
        GameScale = 1.5f;
        WorldFovLevel = OVRPlugin.FoveatedRenderingLevel.Medium;
        GameFovLevel = OVRPlugin.FoveatedRenderingLevel.High;
        IsQ3 = false;
    }

    static void setAsQuest3()
    {
        WorldScale = 1.3f;
        GameScale = 1.8f;
        WorldFovLevel = OVRPlugin.FoveatedRenderingLevel.Low;
        GameFovLevel = OVRPlugin.FoveatedRenderingLevel.High;
        IsQ3 = true;
    }

    static void setAsComputer()
    {
        WorldScale = 1f;
        GameScale = 1f;
        WorldFovLevel = OVRPlugin.FoveatedRenderingLevel.Off;
        GameFovLevel = OVRPlugin.FoveatedRenderingLevel.Off;
        IsQ3 = false;
    }

    /*
  UnityEngine.UnityException: get_eyeTextureResolutionScale can only be called from the main thread.
  Constructors and field initializers will be executed from the loading thread when loading a scene.
    Don't use this function in the constructor or field initializers, instead move initialization code to the Awake or Start function.
  */
    public static void ApplySettings(bool isGaming)
    {
        if (isGaming)
        {
            XRSettings.eyeTextureResolutionScale = GameScale;
            OVRPlugin.foveatedRenderingLevel = GameFovLevel;
        }
        else
        {
            XRSettings.eyeTextureResolutionScale = WorldScale;
            OVRPlugin.foveatedRenderingLevel = WorldFovLevel;
        }
    }

    void OnGlobalConfigChanged()
    {
        ConfigManager.WriteConsole("[DeviceController.OnGlobalConfigChanged] invoked");
        ConfigInformation info = globalConfiguration.Configuration;

        if (!string.IsNullOrEmpty(info?.cabinet?.worldResolution?.foveatedLevelAsString))
        {
            WorldFovLevel = info.cabinet.worldResolution.foveatedLevel();
            WorldScale = info.cabinet.worldResolution.resolution;
            //ApplySettings(false); UnityEngine.UnityException: get_eyeTextureResolutionScale can only be called from the main thread.
        }

        if (!string.IsNullOrEmpty(info?.cabinet?.ingameResolution?.foveatedLevelAsString))
        { 
            GameScale = info.cabinet.ingameResolution.resolution;
            GameFovLevel = info.cabinet.ingameResolution.foveatedLevel();
        }
    }


    private void addListener()
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
        addListener();
    }

    void OnDisable()
    {
        removeListener();
    }
}