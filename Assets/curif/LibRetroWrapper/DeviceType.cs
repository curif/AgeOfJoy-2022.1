using UnityEngine.XR;

public class DeviceType
{
    public static readonly DeviceType OculusQuest2 = new DeviceType(1.0f, 1.5f, OVRPlugin.FoveatedRenderingLevel.Medium, OVRPlugin.FoveatedRenderingLevel.High);
    public static readonly DeviceType OculusQuest3 = new DeviceType(1.3f, 1.8f, OVRPlugin.FoveatedRenderingLevel.Low, OVRPlugin.FoveatedRenderingLevel.High);
    public static readonly DeviceType Computer = new DeviceType(1.0f, 1.0f, OVRPlugin.FoveatedRenderingLevel.Off, OVRPlugin.FoveatedRenderingLevel.Off);
    public static readonly DeviceType Unknown = new DeviceType(1.0f, 1.0f, OVRPlugin.FoveatedRenderingLevel.Off, OVRPlugin.FoveatedRenderingLevel.Off);

    private float WorldScale;
    private float GameScale;
    private OVRPlugin.FoveatedRenderingLevel GameFovLevel;
    private OVRPlugin.FoveatedRenderingLevel WorldFovLevel;

    private DeviceType(float worldScale, float gameScale, OVRPlugin.FoveatedRenderingLevel worldFovLevel, OVRPlugin.FoveatedRenderingLevel gameFovLevel)
    {
        WorldScale = worldScale;
        GameScale = gameScale;
        WorldFovLevel = worldFovLevel;
        GameFovLevel = gameFovLevel;
    }

    public void ApplySettings(bool isGaming)
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
}