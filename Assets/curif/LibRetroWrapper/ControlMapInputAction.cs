
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

  public static InputActionMap inputActionMapFromConfiguration(ControlMapConfiguration mapConfig)
  {
    InputActionMap inputActionMap = new();
    foreach (var map in mapConfig.mapList)
    {
      InputActionType t = InputActionType.Button;
      if (map.behavior == "axis")
      {
        t = InputActionType.Value;
      }
      InputAction action = inputActionMap.AddAction(map.InputActionMapName(), type: t);
      foreach (var controlMap in map.controlMaps)
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
            action = map.mameControl + "_" + map.port.ToString()
          };
          action.AddBinding(bind);
        }
      }
    }
    return inputActionMap;
  }

}
