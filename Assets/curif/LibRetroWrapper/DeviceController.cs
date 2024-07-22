using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class DeviceController : MonoBehaviour
{
    public static DeviceType Device { get; private set; } = DeviceType.Unknown;
    public static string ControllerMeshL { get; set; } = null;
    public static string ControllerMeshR { get; set; } = null;
    public static string ControllerMeshMaterial { get; set; } = null;

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

        if (SystemInfo.deviceName.Contains("Quest 3"))
        {
            Device = DeviceType.OculusQuest3;
            ControllerMeshL = "SM_Quest3Controller_L";
            ControllerMeshR = "SM_Quest3Controller_R";
            ControllerMeshMaterial = "MI_Quest3_Controller";
        }
        else if (SystemInfo.deviceModel.Contains("Quest"))
        {
            Device = DeviceType.OculusQuest2;
            ControllerMeshL = "SM_QuestController_L";
            ControllerMeshR = "SM_QuestController_R";
            ControllerMeshMaterial = "M_Quest_Controller";
        }
        else if (SystemInfo.deviceType.Equals(UnityEngine.DeviceType.Desktop))
        {
            Device = DeviceType.Computer;
        }
        else
        {
            Device = DeviceType.Unknown;
        }

        ConfigManager.WriteConsole("[DeviceController] Detected device: " + Device);
        Device.ApplySettings(false);
    }
}