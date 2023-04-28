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
    Debug.Log(conf.asMarkdown());
    conf.ToDebug();

    actionMap = ControlMapInputAction.inputActionMapFromConfiguration(conf);
  }

  public bool buttonPressed(string controlName)
  {
    //https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/api/UnityEngine.InputSystem.InputAction.html#UnityEngine_InputSystem_InputAction_WasPerformedThisFrame
    if (actionMap[controlName].IsPressed())
    {
      ConfigManager.WriteConsole($"[buttonPressed] {controlName} pressed");
      return true;
    }
    if (actionMap[controlName].WasPerformedThisFrame())
    {
      ConfigManager.WriteConsole("[LibretroControlMap] Fire WAS pressed [WasPerformedThisFrame]");
      return true;
    }
    float val = actionMap[controlName].ReadValue<float>();
    if (val > 0f)
    {
      ConfigManager.WriteConsole($"[LibretroControlMap] Fire {val}");
      return true;
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
