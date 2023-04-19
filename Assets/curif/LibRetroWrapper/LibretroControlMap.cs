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
        actionMap = new InputActionMap();
        //https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/api/UnityEngine.InputSystem.InputActionSetupExtensions.html#UnityEngine_InputSystem_InputActionSetupExtensions_AddAction_UnityEngine_InputSystem_InputActionMap_System_String_UnityEngine_InputSystem_InputActionType_System_String_System_String_System_String_System_String_System_String_
        var action = actionMap.AddAction("fire");
        var triggerPressed = new InputBinding
        {
          path = "<XRController>{RightHand}/triggerPressed",
          action = "fire"
        };
        action.AddBinding(triggerPressed);
        var secondaryButton = new InputBinding
        {
          path = "<XRController>{RightHand}/secondaryButton",
          action = "fire"
        };
        action.AddBinding(secondaryButton);
        actionMap.Enable();

        string deviceLayoutName, controlPath;
        triggerPressed.ToDisplayString(out deviceLayoutName, out controlPath);
        ConfigManager.WriteConsole($"[LibretroControlMap] binding {deviceLayoutName} - {controlPath}");
        
    }

    // Update is called once per frame
    void Update()
    {
      // https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/api/UnityEngine.InputSystem.InputAction.html#UnityEngine_InputSystem_InputAction_WasPerformedThisFrame
      if (actionMap["fire"].IsPressed())
        ConfigManager.WriteConsole("[LibretroControlMap] Fire pressed");
      if (actionMap["fire"].WasPerformedThisFrame())
        ConfigManager.WriteConsole("[LibretroControlMap] Fire WAS pressed");
      float val = actionMap["fire"].ReadValue<float>();
      if (val > 0f)
        ConfigManager.WriteConsole($"[LibretroControlMap] Fire {val}");

    }
}
