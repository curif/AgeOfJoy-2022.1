
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

    public static InputActionMap inputActionMapFromConfiguration(ControlMapConfiguration mapConfig, string name = null)
    {
        InputActionMap inputActionMap = new(name);
        foreach (var map in mapConfig.mapList)
        {
            InputActionType t = InputActionType.Button;
            if (map.behavior == "axis")
            {
                t = InputActionType.Value;
            }
            string actionName = map.InputActionName();
            InputAction action = inputActionMap.AddAction(actionName, type: t);
            foreach (var controlMap in map.controlMaps)
            {
                //https://docs.unity3d.com/Packages/com.unity.inputsystem@1.8/api/UnityEngine.InputSystem.InputControlPath.html
                string path;
                if (string.IsNullOrEmpty(controlMap.Path))
                    path = ControlMapPathDictionary.GetInputPath(controlMap.RealControl);
                else
                    path = controlMap.Path;

                if (string.IsNullOrEmpty(path))
                    ConfigManager.WriteConsoleError($"[inputActionMapFromConfiguration] control {controlMap.RealControl} doesn't have a path and wasn't specified.");
                else
                    action.AddBinding(new InputBinding(path, actionName));
            }
        }
        return inputActionMap;
    }
}
