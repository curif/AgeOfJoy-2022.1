using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XR;
using System.Diagnostics;

public class LibretroControlMap : MonoBehaviour
{
    [Tooltip("The control action map.")]
    public InputActionMap actionMap;

    // [Tooltip("The global action manager in the main rig")]
    // public InputActionManager inputActionManager;
    private const int wheelDelta = 120;

    /*
    public void LoadConfigurationFromFile(string filename)
    {
      ControlMapConfiguration conf = ControlMapConfiguration.LoadFromYaml(filename);
      if (conf == null)
      {
        conf = DefaultControlMap.Instance;
      }
      ConfigManager.WriteConsole($"[LoadConfigurationFromFile] load config {conf}");
      //ConfigManager.WriteConsole(conf.asMarkdown());
      Debug.Log(conf.AsMarkdown());
      conf.ToDebug();

      actionMap = ControlMapInputAction.inputActionMapFromConfiguration(conf);
    }
    */

    //load the control map from the cabinet configuration, if not found fall to the default one.
    //in fact merge any other control map with the default. 
    public void CreateFromConfiguration(
        ControlMapConfiguration conf,
        string name = null,
        string fileNameToSaveOrEmpty = null
        )
    {
        if (!string.IsNullOrEmpty(fileNameToSaveOrEmpty))
        {
            conf.SaveAsYaml(fileNameToSaveOrEmpty);
        }

        // Debug.Log(conf.AsMarkdown());

        actionMap = ControlMapInputAction.inputActionMapFromConfiguration(conf, name);
    }

    public bool SendHapticImpulse(string mameControl, float amplitude, float duration)
    {
        InputAction action = actionMap.FindAction(mameControl+ "_0");
        ConfigManager.WriteConsole($"[LibretroControlMap.SendHapticImpulse] {mameControl} action: {action}");
        ConfigManager.WriteConsole($"[LibretroControlMap.SendHapticImpulse] {mameControl} active control: {action?.activeControl}");
        ConfigManager.WriteConsole($"[LibretroControlMap.SendHapticImpulse] {mameControl} device: {action?.activeControl?.device}");

        if (action?.activeControl?.device is XRControllerWithRumble rumbleController)
        {
            ConfigManager.WriteConsole($"[LibretroControlMap.SendHapticImpulse] SendImpulse...");
            rumbleController.SendImpulse(amplitude, duration);
            return true;
        }

        return false;
    }

    public int Active(string mameControl, int port = 0)
    {
        int ret = 0;

        string inputActionMapId = mameControl + "_" + port.ToString();

        ConfigManager.AssertWriteConsole(actionMap.enabled, $"[LibretroControlMap.Active] {actionMap.name} is not enabled");

        InputAction action = actionMap.FindAction(inputActionMapId);

        if (action == null)
        {
            //ConfigManager.WriteConsoleError($"[LibretroControlMap.Active] [{inputActionMapId}] not found in controlMap");
            return 0;
        }
        try
        {
            if (!action.enabled)
            {
                ConfigManager.WriteConsoleError($"[LibretroControlMap.Active] {inputActionMapId} is not enabled in the actionMap: {actionMap.name}");
                action.Enable();
            }

        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleException($"[LibretroControlMap.Active] {inputActionMapId} {action.ToString()}", e);
            return 0;
        }

        //https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/api/UnityEngine.InputSystem.InputAction.html#UnityEngine_InputSystem_InputAction_WasPerformedThisFrame
        if (action.type == InputActionType.Button)
        {
            //ConfigManager.WriteConsole($"[LibretroControlMap.Active] {actionMap.name}/{inputActionMapId} enabled: {action.enabled} pressed? {action.IsPressed()} action: {action.ToString()}");

            if (action.IsPressed())
            {
                return 1;
            }
            return 0;
        }
        else if (action.type == InputActionType.Value)
        {
            Vector2 resultVector = new Vector2(0, 0);
            float resultFloat = 0;

            var result = action.ReadValueAsObject();
            if (result is Vector2)
            {
                resultVector = (Vector2)result;
            }
            else if (result is float)
            {
                resultFloat = (float)result;
            }

            switch (mameControl)
            {
                case "JOYPAD_UP":
                    if (resultVector.y > 0.5 || resultFloat == 1.0)
                    {
                        // ConfigManager.WriteConsole($"{inputActionMapId}: val: {val}");
                        return 1;
                    }
                    break;
                case "JOYPAD_DOWN":
                    if (resultVector.y < -0.5 || resultFloat == 1.0)
                    {
                        // ConfigManager.WriteConsole($"{inputActionMapId}: val: {val}");
                        return 1;
                    }
                    break;
                case "JOYPAD_RIGHT":
                    if (resultVector.x > 0.5 || resultFloat == 1.0)
                    {
                        // ConfigManager.WriteConsole($"{inputActionMapId}: val: {val}");
                        return 1;
                    }
                    break;
                case "JOYPAD_LEFT":
                    if (resultVector.x < -0.5 || resultFloat == 1.0)
                    {
                        // ConfigManager.WriteConsole($"{inputActionMapId}: val: {val}");
                        return 1;
                    }
                    break;
                case "MOUSE_X":
                    //left-to-right movement, range of [-0x7fff, 0x7fff], -32768 to 32767
                    if (resultVector.x > 0.5)
                        ret = 10;
                    else if (resultVector.x < -0.5)
                        ret = -10;
                    break;
                case "MOUSE_Y":
                    //left-to-right movement, range of [-0x7fff, 0x7fff], -32768 to 32767
                    if (resultVector.y > 0.5)
                        ret = 10;
                    else if (resultVector.y < -0.5)
                        ret = -10;
                    break;
                case "MOUSE_WHEELUP":
                    //left-to-right movement, range of [-0x7fff, 0x7fff], -32768 to 32767
                    if (resultVector.y > wheelDelta)
                        ret = 10;
                    break;
                case "MOUSE_WHEELDOWN":
                    //left-to-right movement, range of [-0x7fff, 0x7fff], -32768 to 32767
                    if (resultVector.y < -wheelDelta)
                        ret = -10;
                    break;
                case "MOUSE_HORIZ_WHEELUP":
                    //left-to-right movement, range of [-0x7fff, 0x7fff], -32768 to 32767
                    if (resultVector.x > wheelDelta)
                        ret = 10;
                    break;
                case "MOUSE_HORIZ_WHEELDOWN":
                    //left-to-right movement, range of [-0x7fff, 0x7fff], -32768 to 32767
                    if (resultVector.x < -wheelDelta)
                        ret = -10;
                    break;
            }
        }
        return ret;
    }


    public void Enable(bool enable)
    {
        if (enable)
        {
            actionMap.Enable();
            ConfigManager.WriteConsole($"[LibretroControlMap.Enable] actionMap Enabled: {actionMap}");
            // inputActionManager.DisableInput();
            return;
        }
        ConfigManager.WriteConsole($"[LibretroControlMap.Enable] actionMap Disabled: {actionMap}");
        actionMap.Disable();
        // inputActionManager.EnableInput();
        return;
    }

    //when its not longer neccesary
    public void Clean()
    {
        actionMap.Dispose();
        actionMap = null;
    }

}
