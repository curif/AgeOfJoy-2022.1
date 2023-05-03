
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using System.Text;


public static class ControlMapInputAction
{
  public static bool HasMameControl(string mameControl)
  {
     foreach (var item in LibretroMameCore.deviceIdsJoypad)
     {
       if (item == mameControl)
         return true;
     }
     foreach (var item in LibretroMameCore.deviceIdsMouse)
     {
       if (item == mameControl)
         return true;
     }
     return false;
  }

  public static InputActionMap inputActionMapFromConfiguration(ControlMapConfiguration mapConfig)
  {
    InputActionMap inputActionMap = new();
    foreach (var map in mapConfig.MapList)
    {
      InputActionType t = InputActionType.Button;
      if (map.behavior == "axis")
      {
        t = InputActionType.Value;
      }
      InputAction action = inputActionMap.AddAction(map.MAMEControl, type: t);
      foreach (var controlMap in map.ControlMaps)
      {
        string path = ControlMapPathDictionary.GetInputPath(controlMap.RealControl);
        if (string.IsNullOrEmpty(path))
        {
          ConfigManager.WriteConsoleError($"[inputActionMapFromConfiguration] control {controlMap.RealControl} doesn't have a path. ");
        }
        else
        {
          var bind = new InputBinding
          {
            path = path, 
            action = map.MAMEControl
          };
          action.AddBinding(bind);
        }
      }
    }
    return inputActionMap;
  }

}
