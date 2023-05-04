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
  public List<Maps> MapList { get; set; }
  
  public class ControlMap
  {
    public string RealControl = ""; //quest-x-button
    public string Path = "";

    public ControlMap ()
    {
    }
    public ControlMap(string realControl, string path = "")
    {
      RealControl = realControl;
      Path = path;
    }
  }

  public class Maps {
    public string MAMEControl;
    public string behavior = "button"; // or axis 
    public List<ControlMap> ControlMaps { get; set; }
    public Maps()
    {

    }
    public Maps(string mameControl, string behavior = "button")
    {
      this.MAMEControl = mameControl;
      this.behavior = behavior;
      this.ControlMaps = new();
    }
    public ControlMap AddAction(string realControl, string path = "")
    {
      ControlMap act = new(realControl, path);
      ControlMaps.Add(act);
      return act;
    }

  }
  public void ToDebug()
  {
    ConfigManager.WriteConsole("MAME \t Control \t behavior \t Unity Path ");
    foreach (ControlMapConfiguration.Maps m in MapList)
    {
      foreach (ControlMapConfiguration.ControlMap a in m.ControlMaps)
      {
        ConfigManager.WriteConsole($"{m.MAMEControl} \t {a.RealControl} \t {m.behavior} \t {a.Path} ");
      }
    }
  }

  public string AsMarkdown()
  {
    StringBuilder sb = new StringBuilder();
    sb.Append("| MAME | Control | Behavior | Unity Path |\n");
    sb.Append("| --- | --- | --- | --- |\n");
    foreach (ControlMapConfiguration.Maps m in MapList)
    {
      foreach (ControlMapConfiguration.ControlMap a in m.ControlMaps)
      {
        sb.Append($"| {m.MAMEControl} | {a.RealControl} | {m.behavior} | `{a.Path}` |\n");
      }
    }
    return sb.ToString();
  }
  
  public ControlMapConfiguration.Maps GetMap(string mameControl)
  {
    foreach (var map in MapList)
    {
      if (map.MAMEControl == mameControl)
        return map;
    }
    return null;
  }
  public void AddMap(string mameControl, string realControl, string behavior = "button")
  {  
    ControlMapConfiguration.Maps map;
    map = GetMap(mameControl);
    if (map == null)
    {
      map = new(mameControl, behavior);
      MapList.Add(map);
    }
    string path = ControlMapPathDictionary.GetInputPath(realControl);
    if (string.IsNullOrEmpty(path))
      ConfigManager.WriteConsole($"[ControlMapConfiguration.AddMap] ERROR path unknown action:{mameControl} maped control: {realControl}");
    else
      map.AddAction(realControl, path);
  }

  public void AddMap(string mameControl, string[] realControlsToAssign, string behavior = "button")
  {
    foreach (string realControl in realControlsToAssign)
    {
      AddMap(mameControl, realControl, behavior);      
    }

    return;
  }

  public void AddMap(uint libretroMameCoreID, string libretroMameControlType, string[] realControlsToAssign, string behavior = "button")
  {
    string mameControl = LibretroMameCore.getDeviceNameFromID(libretroMameControlType, libretroMameCoreID);
    if (mameControl == "")
    {
      ConfigManager.WriteConsole($"[ControlMapConfiguration.AddMap] ERROR MAME control type unknown : {libretroMameControlType}-{libretroMameCoreID}");
      return;
    }
    AddMap(mameControl, realControlsToAssign, behavior);

    return;
  }
}

public class DefaultControlMap : ControlMapConfiguration
{
  static DefaultControlMap instance = null;

  private DefaultControlMap() : base()
  {
    MapList = new();

    //fire with b-button and trigger.
    AddMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_B, "joypad", new string[] {"quest-b", "gamepad-b", "quest-right-trigger"});
    
    AddMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_A, "joypad", new string[] {"gamepad-a", "quest-a"});
    AddMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_X, "joypad", new string[] {"gamepad-x", "quest-x"});
    AddMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_Y, "joypad", new string[] {"gamepad-y", "quest-y"});
    // can't be select because the coin is used.
    AddMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_START, "joypad", new string[] {"gamepad-start", "quest-start"});
    AddMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_SELECT, "joypad", new string[] {"gamepad-select", "quest-select"});

    AddMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_UP, "joypad", new string[] {"quest-left-thumbstick", "gamepad-left-thumbstick"}, "axis");
    AddMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_DOWN, "joypad", new string[] {"quest-left-thumbstick","gamepad-left-thumbstick"}, "axis");
    AddMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_LEFT, "joypad", new string[] {"quest-left-thumbstick", "gamepad-left-thumbstick"}, "axis");
    AddMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_RIGHT, "joypad", new string[] {"quest-left-thumbstick", "gamepad-left-thumbstick"}, "axis");

    AddMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_L, "joypad", new string[] {"quest-left-trigger", "gamepad-left-trigger"});
    AddMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_R, "joypad", new string[] {"quest-right-trigger", "gamepad-right-trigger"});

    AddMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_L2, "joypad", new string[] {"quest-left-grip", "gamepad-left-bumper"});
    AddMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_R2, "joypad", new string[] {"quest-right-grip", "gamepad-right-bumper"});

    AddMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_L3, "joypad", new string[] {"quest-left-thumbstick-press", "gamepad-left-thumbstick-press"});
    AddMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_R3, "joypad", new string[] {"quest-right-thumbstick-press", "gamepad-right-thumbstick-press"});

    AddMap("EXIT", new string[] {"quest-left-grip", "gamepad-left-bumper"});
    AddMap("INSERT", "gamepad-select");

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

