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
}

public class DefaultControlMap : ControlMapConfiguration
{
  static DefaultControlMap instance = null;

  private DefaultControlMap() : base()
  {
    MapList = new();

    //fire with b-button and trigger.
    AddMap("JOYPAD_B",  new string[] {"quest-b", "gamepad-b", "quest-right-trigger"});
    
    AddMap("JOYPAD_A",  new string[] {"gamepad-a", "quest-a"});
    AddMap("JOYPAD_X",  new string[] {"gamepad-x", "quest-x"});
    AddMap("JOYPAD_Y",  new string[] {"gamepad-y", "quest-y"});
    // can'sed.
    AddMap("JOYPAD_START",  new string[] {"gamepad-start", "quest-start"});
    AddMap("JOYPAD_SELECT",  new string[] {"gamepad-select", "quest-select"});

    AddMap("JOYPAD_UP",  new string[] {"quest-left-thumbstick", "gamepad-left-thumbstick"}, "axis");
    AddMap("JOYPAD_DOWN",  new string[] {"quest-left-thumbstick","gamepad-left-thumbstick"}, "axis");
    AddMap("JOYPAD_LEFT",  new string[] {"quest-left-thumbstick", "gamepad-left-thumbstick"}, "axis");
    AddMap("JOYPAD_RIGHT",  new string[] {"quest-left-thumbstick", "gamepad-left-thumbstick"}, "axis");

    AddMap("JOYPAD_L",  new string[] {"quest-left-trigger", "gamepad-left-trigger"});
    AddMap("JOYPAD_R",  new string[] {"quest-right-trigger", "gamepad-right-trigger"});

    AddMap("JOYPAD_L2",  new string[] {"quest-left-grip", "gamepad-left-bumper"});
    AddMap("JOYPAD_R2",  new string[] {"quest-right-grip", "gamepad-right-bumper"});

    // mapped por mame menu in LibretroMameCore
    //AddMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_L3,  new string[] {"quest-left-thumbstick-press", "gamepad-left-thumbstick-press"});
    AddMap("JOYPAD_R3", new string[] {"quest-right-thumbstick-press", "gamepad-right-thumbstick-press"});

    AddMap("EXIT", new string[] {"quest-left-grip", "gamepad-left-bumper"});
    AddMap("INSERT", "gamepad-select");

    AddMap("MOUSE_X", new string[] {"quest-right-thumbstick", "gamepad-right-thumbstick"}, "axis");
    AddMap("MOUSE_Y", new string[] {"quest-right-thumbstick", "gamepad-right-thumbstick"}, "axis");
    AddMap("MOUSE_LEFT", new string[] {"quest-b", "gamepad-b"});
    AddMap("MOUSE_RIGHT", new string[] {"quest-a", "gamepad-a"});
    AddMap("MOUSE_MIDDLE", new string[] {"quest-x", "gamepad-x"});
    AddMap("MOUSE_WHEELUP", new string[] {"quest-left-thumbstick", "gamepad-left-thumbstick"}, "axis");
    AddMap("MOUSE_WHEELDOWN", new string[] {"quest-left-thumbstick", "gamepad-left-thumbstick"}, "axis");
    AddMap("MOUSE_HORIZ_WHEELUP", new string[] {"quest-left-thumbstick", "gamepad-left-thumbstick"}, "axis");
    AddMap("MOUSE_HORIZ_WHEELDOWN", new string[] {"quest-left-thumbstick", "gamepad-left-thumbstick"}, "axis");
    AddMap("MOUSE_BUTTON_4", new string[] {"quest-left-thumbstick-press", "gamepad-left-thumbstick-press"});
    AddMap("MOUSE_BUTTON_5", new string[] {"quest-right-thumbstick-press", "gamepad-right-thumbstick-press"});

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

