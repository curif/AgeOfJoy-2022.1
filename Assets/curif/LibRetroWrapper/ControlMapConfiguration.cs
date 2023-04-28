using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using System.Text;



public class ControlMapConfiguration 
{
  public List<Map> maps { get; set; }
  
  public class Action
  {
    public string control = "";
    public string action = "";
    public string path = "";
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
    public string controlID;
    public List<Action> actions { get; set; }
    public Map()
    {

    }
    public Map(string controlID)
    {
      this.controlID = controlID;
      this.actions = new();
    }
    public Action addAction(string control, string action, string path = "")
    {
      Action act = new(control, action, path);
      actions.Add(act);
      return act;
    }

  }
  public void ToDebug()
  {
    ConfigManager.WriteConsole("ID \t Control map \t Action \t Unity Path ");
    foreach (ControlMapConfiguration.Map m in maps)
    {
      foreach (ControlMapConfiguration.Action a in m.actions)
      {
        ConfigManager.WriteConsole($"{m.controlID} \t {a.control} \t {a.action} \t {a.path} ");
      }
    }
  }
  public string asMarkdown()
  {
    StringBuilder sb = new StringBuilder();
    sb.Append("| MAME | Control map | Action | Unity Path |\n");
    sb.Append("| --- | --- | --- | --- |\n");
    foreach (ControlMapConfiguration.Map m in maps)
    {
      foreach (ControlMapConfiguration.Action a in m.actions)
      {
        sb.Append($"| {m.controlID} | {a.control} | {a.action} | `{a.path}` |\n");
      }
    }
    return sb.ToString();
  }
  
  public ControlMapConfiguration.Map GetMap(string controlID)
  {
    foreach (var map in maps)
    {
      if (map.controlID == controlID)
        return map;
    }
    return null;
  }

  public void addMap(string controlID, string configAction, string[] mapsToAssign)
  {
    ControlMapConfiguration.Map map;
    map = GetMap(controlID);
    if (map == null)
    {
      map = new(controlID);
      maps.Add(map);
    }

    foreach (string item in mapsToAssign)
    {
      string path = ControlMapPathDictionary.GetInputPath(item, configAction);
      if (string.IsNullOrEmpty(path))
        ConfigManager.WriteConsole($"[ControlMapConfiguration.AddMap] ERROR path unknown action:{controlID} mapped control action:{configAction} maped control: {item}");
      else
        map.addAction(item, configAction, path);
    }

    return;
  }

  public void addMap(uint libretroMameCoreID, string controlType, string configAction, string[] mapsToAssign)
  {
    ControlMapConfiguration.Map map;
    string controlID = LibretroMameCore.getDeviceNameFromID(controlType, libretroMameCoreID);
    if (controlID == "")
    {
      ConfigManager.WriteConsole($"[ControlMapConfiguration.AddMap] ERROR controlType unknown : {controlType}");
      return;
    }
    addMap(controlID, configAction, mapsToAssign);

    return;
  }
}

public class DefaultControlMap : ControlMapConfiguration
{
  static DefaultControlMap instance = null;

  private DefaultControlMap() : base()
  {
    maps = new();

    //fire with b-button and trigger.
    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_B, "joypad", "b-button", new string[] {"gamepad", "vr-right"});
    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_B, "joypad", "trigger", new string[] {"vr-right"});
    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_B, "joypad", "right-trigger", new string[] {"gamepad"});

    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_A, "joypad", "a-button", new string[] {"gamepad", "vr-right"});
    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_X, "joypad", "x-button", new string[] {"gamepad", "vr-left"});
    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_Y, "joypad", "y-button", new string[] {"gamepad", "vr-left"});
    // can't be select because the coin is used.
    //addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_Y, "joypad", "select-button", new string[] {"gamepad", "vr-right"});
    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_START, "joypad", "start-button", new string[] {"gamepad", "vr-left"});
    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_SELECT, "joypad", "select-button", new string[] {"gamepad"});

    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_UP, "joypad", "thumbstick-up", new string[] {"vr-left"});
    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_UP, "joypad", "left-thumbstick-up", new string[] {"gamepad"});
    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_DOWN, "joypad", "thumbstick-down", new string[] {"vr-left"});
    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_DOWN, "joypad", "left-thumbstick-down", new string[] {"gamepad"});
    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_LEFT, "joypad", "thumbstick-left", new string[] {"vr-left"});
    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_LEFT, "joypad", "left-thumbstick-left", new string[] {"gamepad"});
    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_RIGHT, "joypad", "thumbstick-right", new string[] {"vr-left"});
    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_RIGHT, "joypad", "left-thumbstick-right", new string[] {"gamepad"});

    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_L, "joypad", "trigger", new string[] {"vr-left"});
    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_L, "joypad", "left-trigger", new string[] {"gamepad"});
    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_R, "joypad", "trigger", new string[] {"vr-right"});
    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_R, "joypad", "right-trigger", new string[] {"gamepad"});
    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_L2, "joypad", "grip", new string[] {"vr-left"});
    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_L2, "joypad", "left-bumper", new string[] {"gamepad"});
    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_R2, "joypad", "grip", new string[] {"vr-right"});
    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_R2, "joypad", "right-bumper", new string[] {"gamepad"});

    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_L3, "joypad", "thumbstick-press", new string[] {"vr-left"});
    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_L3, "joypad", "left-thumbstick-press", new string[] {"gamepad"});
    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_R3, "joypad", "thumbstick-press", new string[] {"vr-right"});
    addMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_R3, "joypad", "right-thumbstick-press", new string[] {"gamepad"});

    addMap("EXIT", "grip", new string[] {"vr-left"});
    addMap("EXIT", "left-bumper", new string[] {"gamepad"}); //L2
    addMap("INSERT", "select-button", new string[] {"gamepad"});

  }

  public static DefaultControlMap Instance { 
    get
    {
      if (instance == null)
        instance = new();
      return instance;
    }
  }
}

