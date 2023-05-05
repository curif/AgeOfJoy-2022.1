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
    DefaultControlMap conf = DefaultControlMap.Instance;

    ConfigManager.WriteConsole($"[LibretroControlMap] load default config {conf}");
    //ConfigManager.WriteConsole(conf.asMarkdown());
    Debug.Log(conf.AsMarkdown());
    conf.ToDebug();

    actionMap = ControlMapInputAction.inputActionMapFromConfiguration(conf);
  }

  public bool JoypadActive(string mameControl)
  {
    //https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/api/UnityEngine.InputSystem.InputAction.html#UnityEngine_InputSystem_InputAction_WasPerformedThisFrame
    if (actionMap[mameControl].type == InputActionType.Button)
    {
      if (actionMap[mameControl].IsPressed())
      {
        ConfigManager.WriteConsole($"[JoypadActive] {mameControl} pressed");
        return true;
      }
      return false;
    }
    
    /*
    if (actionMap[controlName].WasPerformedThisFrame())
    {
      ConfigManager.WriteConsole("[LibretroControlMap] Fire WAS pressed [WasPerformedThisFrame]");
      return true;
    }
    */

    if (actionMap[mameControl].type == InputActionType.Value)
    {
      Vector2 val = actionMap[mameControl].ReadValue<Vector2>();
      switch (mameControl)
      {
        case "JOYPAD-UP":
          if (val.y > 0.5)
          {
            ConfigManager.WriteConsole($"{mameControl}: val: {val} true");
            return true;
          }
          break;
        case "JOYPAD-DOWN":
          if (val.y < -0.5)
          {
            ConfigManager.WriteConsole($"{mameControl}: val: {val} true");
            return true;
          }
          break;
        case "JOYPAD-RIGHT":
          if (val.x > 0.5)
          {
            ConfigManager.WriteConsole($"{mameControl}: val: {val} true");
            return true;
          }
          break;
        case "JOYPAD-LEFT":
          if (val.x < -0.5)
          {
            ConfigManager.WriteConsole($"{mameControl}: val: {val} true");
            return true;
          }
          break;
      }
    }
    return false;
  }

  public void Enable(bool enable)
  {
    if (enable)
    {
      actionMap.Enable(); 
      return;
    }
    actionMap.Disable();
    return;
  }

}
