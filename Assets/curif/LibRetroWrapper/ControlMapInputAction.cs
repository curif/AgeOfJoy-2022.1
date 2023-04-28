
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
  public static bool hasMameControl(string control)
  {
     foreach (var item in LibretroMameCore.deviceIdsJoypad)
     {
       if (item == control)
         return true;
     }
     foreach (var item in LibretroMameCore.deviceIdsMouse)
     {
       if (item == control)
         return true;
     }
     return false;
  }

  public static InputActionMap inputActionMapFromConfiguration(ControlMapConfiguration mapConfig)
  {
    InputActionMap inputActionMap = new();
    foreach (var map in mapConfig.maps)
    {
      var action = inputActionMap.AddAction(map.controlID);
      foreach (var mapAction in map.actions)
      {
        string path = ControlMapPathDictionary.GetInputPath(mapAction.control, mapAction.action);
        if (string.IsNullOrEmpty(path))
        {
          ConfigManager.WriteConsoleError($"[inputActionMapFromConfiguration] control {mapAction.control} doesn't have a path. ");
        }
        else
        {
          var bind = new InputBinding
          {
            path = path, 
            action = map.controlID
          };
          action.AddBinding(bind);
        }
      }
    }
    return inputActionMap;
  }

}
