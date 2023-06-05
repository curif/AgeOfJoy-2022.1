using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this class maps the quest controls to unity paths and behaviors
public static class ControlMapPathDictionary
{
    static Dictionary<string, string> map;

    static ControlMapPathDictionary()
    {
        /*
        * secondaryButton [LeftHand XR Controller] = Y button
           primaryButton [LeftHand XR Controller] = X button
           secondaryButton [RightHand XR Controller] = B button
           primaryButton [RightHand XR Controller] = A buttonsecondaryButton [LeftHand XR Controller] = Y button
           primaryButton [LeftHand XR Controller] = X button
           secondaryButton [RightHand XR Controller] = B button
           primaryButton [RightHand XR Controller] = A button
       */
        map = new Dictionary<string, string>
    {
        
        //previous maps to 2022.3, ovr oculus controllers
        // Left VR controller mappings
        { "quest-x", "<XRController>{LeftHand}/primaryButton" }, //primaryButton
        { "quest-y", "<XRController>{LeftHand}/secondaryButton" },
        { "quest-start", "<OculusTouchController>/start" },
        { "quest-left-grip", "<XRController>{LeftHand}/gripButton" },
        { "quest-left-trigger", "<OculusTouchController>{LeftHand}/triggerPressed" },
        { "quest-left-thumbstick", "<XRController>{LeftHand}/Primary2DAxis" },
        { "quest-left-thumbstick-press", "<XRController>{LeftHand}/thumbstickClicked" },
        
        // Right VR controller mappings
        { "quest-a", "<XRController>{RightHand}/primaryButton" },
        { "quest-b", "<XRController>{RightHand}/secondaryButton" },
        { "quest-select", "<XRController>{RightHand}/menuButton" },
        { "quest-right-grip", "<XRController>{RightHand}/gripButton" },
        { "quest-right-trigger", "<OculusTouchController>{RightHand}/triggerPressed" },
        { "quest-right-thumbstick", "<XRController>{RightHand}/Primary2DAxis" },
        { "quest-right-thumbstick-press", "<XRController>{RightHand}/thumbstickClicked" },
/*
        // Left VR controller mappings
        { "quest-x", "<OculusTouchController>{LeftHand}/primaryButton" }, //primaryButton
        { "quest-y", "<OculusTouchController>{LeftHand}/secondaryButton" },
        { "quest-start", "<OculusTouchController>/start" },
        { "quest-left-grip", "<OculusTouchController>{LeftHand}/gripPressed" },
        { "quest-left-trigger", "<OculusTouchController>{LeftHand}/triggerPressed" },
        { "quest-left-thumbstick", "<OculusTouchController>{LeftHand}/thumbstick" },
        { "quest-left-thumbstick-press", "<OculusTouchController>{LeftHand}/thumbstickClicked" },
        
        // Right VR controller mappings
        { "quest-a", "<OculusTouchController>{RightHand}/primaryButton" },
        { "quest-b", "<OculusTouchController>{RightHand}/secondaryButton" },
        { "quest-select", "<OculusTouchController>{RightHand}/start" },
        { "quest-right-grip", "<OculusTouchController>{RightHand}/gripPressed" },
        { "quest-right-trigger", "<OculusTouchController>{RightHand}/triggerPressed" },
        { "quest-right-thumbstick", "<OculusTouchController>{RightHand}/thumbstick" },
        { "quest-right-thumbstick-press", "<OculusTouchController>{RightHand}/thumbstickClicked" },
*/
        //Gamepad mappings
        { "gamepad-a", "<Gamepad>/buttonSouth" },
        { "gamepad-b", "<Gamepad>/buttonEast" },
        { "gamepad-x", "<Gamepad>/buttonWest" },
        { "gamepad-y", "<Gamepad>/buttonNorth" },
        { "gamepad-select", "<Gamepad>/select" },
        { "gamepad-start", "<Gamepad>/start" },
        { "gamepad-left-bumper", "<Gamepad>/leftShoulder" },
        { "gamepad-right-bumper", "<Gamepad>/rightShoulder" },
        { "gamepad-left-trigger", "<Gamepad>/leftTrigger" },
        { "gamepad-right-trigger", "<Gamepad>/rightTrigger" },
        { "gamepad-left-thumbstick", "<Gamepad>/leftStick" },
        { "gamepad-right-thumbstick", "<Gamepad>/rightStick" },
        { "gamepad-left-thumbstick-press", "<Gamepad>/leftStickPress" },
        { "gamepad-right-thumbstick-press", "<Gamepad>/rightStickPress" },

        //keyboard
        { "keyboard-a", "<keyboard>/a"},
        { "keyboard-w", "<keyboard>/w"},
        { "keyboard-s", "<keyboard>/s"},
        { "keyboard-d", "<keyboard>/d"},
        { "keyboard-space", "<keyboard>/space"},
        { "keyboard-esc", "<keyboard>/escape"},
        { "keyboard-enter", "<keyboard>/enter"}
      };
    }
    public static string GetBehavior(string realControl)
    {
        if (realControl == "quest-right-thumbstick" || realControl == "quest-left-thumbstick")
            return "axis";

        return "button";
    }
    public static string GetInputPath(string realControl)
    {
        if (map.ContainsKey(realControl))
        {
            return map[realControl];
        }
        else
        {
            ConfigManager.WriteConsoleError("[ControlMapPathDictionary] Control not found in control map: " + realControl);
            return "";
        }
    }

    public static List<string> getList()
    {
        return new List<string>(map.Keys);
    }

}
