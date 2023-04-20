using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;



public class ControlMapConfiguration 
{
  public List<Map> maps { get; set; }
  
  public class Action
  {
    public string control;
    public string action;
    public string path;
    public Action ()
    {

    }
    public Action(string control, string action, string path = "")
    {
      this.control = control;
      this.action = action;
      this.path = path;
    }
  }


  public class Map {
    public string mame_control;
    public List<Action> actions { get; set; }
    public Map()
    {

    }
    public Map(string mame_control)
    {
      this.mame_control = mame_control;
      this.actions = new();
    }
    public Action addAction(string control, string action)
    {
      Action act = new(control, action);
      actions.Add(act);
      return act;
    }
  }

}

public static class DefaultControlMap
{
  static ControlMapConfiguration instance = null;

  public static ControlMapConfiguration map()
  {
    if (instance != null)
      return instance;

    ControlMapConfiguration config = new();
    config.maps = new();

    ControlMapConfiguration.Map map = new(LibretroMameCore.deviceIdsJoypad[LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_B]);
    map.addAction("gamepad", "b-button");
    map.addAction("vr-right","b-button");
    config.maps.Add(map);


    instance = config;
    return config;
  }
}

public static class ControlMapInputAction
{
  static bool hasMameControl(string control)
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

  public static InputActionMap inputActionMap(ControlMapConfiguration mapConfig)
  {
    InputActionMap inputActionMap = new();
    foreach (var map in mapConfig.maps)
    {
      if (hasMameControl(map.mame_control))
      {
        //the control is one of the MAME required
        var action = inputActionMap.AddAction(map.mame_control);
        foreach (var mapAction in map.actions)
        {
          var bind = new InputBinding
          {
            path = ControlMapPathDictionary.GetInputPath(mapAction.control, mapAction.action),
            action = map.mame_control
          };
          action.AddBinding(bind);
        }
      }
      else {
        ConfigManager.WriteConsole($"[ControlMapConfiguration.ControlMapInputAction.inputActionMap] ERROR MAME control does not exists: {map.mame_control}");
      }
    }
    inputActionMap.Enable();
    return inputActionMap;
  }

}
