using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;


public class LibretroControlMap : MonoBehaviour
{
    public InputActionMap actionMap;


    // Start is called before the first frame update
    void Start()
    {
      ControlMapConfiguration conf = DefaultControlMap.map();
      ConfigManager.WriteConsole($"[LibretroControlMap] load default config {conf}");
      actionMap = ControlMapInputAction.inputActionMap(conf);

      /*
        actionMap = new InputActionMap();
        //https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/api/UnityEngine.InputSystem.InputActionSetupExtensions.html#UnityEngine_InputSystem_InputActionSetupExtensions_AddAction_UnityEngine_InputSystem_InputActionMap_System_String_UnityEngine_InputSystem_InputActionType_System_String_System_String_System_String_System_String_System_String_
        string actionId = LibretroMameCore.deviceIdsJoypad[LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_B];
        var action = actionMap.AddAction(actionId);
        var triggerPressed = new InputBinding
        {
          path = "<XRController>{RightHand}/triggerPressed",
          action = actionId
        };
        action.AddBinding(triggerPressed);
        var secondaryButton = new InputBinding
        {
          path = "<XRController>{RightHand}/secondaryButton",
          action = actionId
        };
        action.AddBinding(secondaryButton);
        actionMap.Enable();
*/

      /*
        string deviceLayoutName, controlPath;
        triggerPressed.ToDisplayString(out deviceLayoutName, out controlPath);
        ConfigManager.WriteConsole($"[LibretroControlMap] binding {deviceLayoutName} - {controlPath}");

        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(inputDevices);

        foreach (var device in inputDevices)
        {
            ConfigManager.WriteConsole(string.Format("[LibretroControlMap] Device found with name '{0}' and role '{1}'", device.name, device.role.ToString()));
        }
*/
      /*
        //all devices
        var gendevices = InputSystem.devices;

        // Iterate through the list of devices to find the gamepad
        foreach (var device in gendevices)
          {
            ConfigManager.WriteConsole($"[LibretroControlMap] input generic device: {device.name} {device.description} is gamepad : {device is Gamepad}");
          }
        //only for VR
        var devices = new List<UnityEngine.XR.InputDevice>();
        //UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, devices);
        UnityEngine.XR.InputDevices.GetDevices(devices);

        if(devices.Count == 1)
        {
            UnityEngine.XR.InputDevice device = devices[0];
            Debug.Log(string.Format("[LibretroControlMap] Device name '{0}' with role '{1}'", device.name, device.role.ToString()));
        }
        else if(devices.Count > 1)
        {
            ConfigManager.WriteConsole("[LibretroControlMap] Found more than one left hand!");
        }
        foreach (var device in devices)
        {
            
          var inputFeatures = new List<UnityEngine.XR.InputFeatureUsage>();
          if (device.TryGetFeatureUsages(inputFeatures))
          {
              ConfigManager.WriteConsole(string.Format("[LibretroControlMap] device {0} charact: {1}", device.name, device.characteristics ));
              foreach (var feature in inputFeatures)
              {
                  if (feature.type == typeof(bool))
                  {
                      bool featureValue;
                      if (device.TryGetFeatureValue(feature.As<bool>(), out featureValue))
                      {
                          ConfigManager.WriteConsole(string.Format("[LibretroControlMap]Bool feature {0} = {1}", feature.name, featureValue.ToString()));
                      }
                  }
              }
          }
        }
      */
    }

    // Update is called once per frame
    void Update()
    {
      /*
      // https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/api/UnityEngine.InputSystem.InputAction.html#UnityEngine_InputSystem_InputAction_WasPerformedThisFrame
      string actionId = LibretroMameCore.deviceIdsJoypad[LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_B];
      if (actionMap[actionId].IsPressed())
        ConfigManager.WriteConsole("[LibretroControlMap] Fire pressed");
      if (actionMap[actionId].WasPerformedThisFrame())
        ConfigManager.WriteConsole("[LibretroControlMap] Fire WAS pressed");
      float val = actionMap[actionId].ReadValue<float>();
      if (val > 0f)
        ConfigManager.WriteConsole($"[LibretroControlMap] Fire {val}");
*/
    }
}
