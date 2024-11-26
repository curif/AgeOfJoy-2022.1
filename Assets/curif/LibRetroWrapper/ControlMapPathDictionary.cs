using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this class maps the quest controls to unity paths and behaviors
public static class ControlMapPathDictionary
{
    static Dictionary<string, string> map;

    public const string VR_CONTROLLER_X = "quest-x";
    public const string VR_CONTROLLER_Y = "quest-y";
    public const string VR_CONTROLLER_START = "quest-start";
    public const string VR_CONTROLLER_LEFT_GRIP = "quest-left-grip";
    public const string VR_CONTROLLER_LEFT_TRIGGER = "quest-left-trigger";
    public const string VR_CONTROLLER_LEFT_THUMBSTICK = "quest-left-thumbstick";
    public const string VR_CONTROLLER_LEFT_THUMBSTICK_PRESS = "quest-left-thumbstick-press";
    public const string VR_CONTROLLER_LEFT_HAPTIC_DEVICE = "quest-left-haptic-device";
    public const string VR_CONTROLLER_A = "quest-a";
    public const string VR_CONTROLLER_B = "quest-b";
    public const string VR_CONTROLLER_SELECT = "quest-select";
    public const string VR_CONTROLLER_RIGHT_GRIP = "quest-right-grip";
    public const string VR_CONTROLLER_RIGHT_TRIGGER = "quest-right-trigger";
    public const string VR_CONTROLLER_RIGHT_THUMBSTICK = "quest-right-thumbstick";
    public const string VR_CONTROLLER_RIGHT_THUMBSTICK_PRESS = "quest-right-thumbstick-press";
    public const string VR_CONTROLLER_RIGHT_HAPTIC_DEVICE = "quest-right-haptic-device";
    public const string GAMEPAD_A = "gamepad-a";
    public const string GAMEPAD_B = "gamepad-b";
    public const string GAMEPAD_X = "gamepad-x";
    public const string GAMEPAD_Y = "gamepad-y";
    public const string GAMEPAD_SELECT = "gamepad-select";
    public const string GAMEPAD_START = "gamepad-start";
    public const string GAMEPAD_LEFT_BUMPER = "gamepad-left-bumper";
    public const string GAMEPAD_RIGHT_BUMPER = "gamepad-right-bumper";
    public const string GAMEPAD_LEFT_TRIGGER = "gamepad-left-trigger";
    public const string GAMEPAD_RIGHT_TRIGGER = "gamepad-right-trigger";
    public const string GAMEPAD_LEFT_THUMBSTICK = "gamepad-left-thumbstick";
    public const string GAMEPAD_RIGHT_THUMBSTICK = "gamepad-right-thumbstick";
    public const string GAMEPAD_LEFT_THUMBSTICK_PRESS = "gamepad-left-thumbstick-press";
    public const string GAMEPAD_RIGHT_THUMBSTICK_PRESS = "gamepad-right-thumbstick-press";
    public const string GAMEPAD_DPAD_UP = "gamepad-dpad-up";
    public const string GAMEPAD_DPAD_DOWN = "gamepad-dpad-down";
    public const string GAMEPAD_DPAD_LEFT = "gamepad-dpad-left";
    public const string GAMEPAD_DPAD_RIGHT = "gamepad-dpad-right";
    public const string KEYBOARD_A = "keyboard-a";
    public const string KEYBOARD_W = "keyboard-w";
    public const string KEYBOARD_S = "keyboard-s";
    public const string KEYBOARD_D = "keyboard-d";
    public const string KEYBOARD_SPACE = "keyboard-space";
    public const string KEYBOARD_ESC = "keyboard-esc";
    public const string KEYBOARD_ENTER = "keyboard-enter";
    public const string KEYBOARD_X = "keyboard-x";
    public const string KEYBOARD_Y = "keyboard-y";

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
            // Left VR controller mappings
            { VR_CONTROLLER_X, "<XRController>{LeftHand}/primaryButton" },
            { VR_CONTROLLER_Y, "<XRController>{LeftHand}/secondaryButton" },
            { VR_CONTROLLER_START, "<OculusTouchController>/start" },
            { VR_CONTROLLER_LEFT_GRIP, "<XRController>{LeftHand}/gripButton" },
            { VR_CONTROLLER_LEFT_TRIGGER, "<XRController>{LeftHand}/trigger" },
            { VR_CONTROLLER_LEFT_THUMBSTICK, "<XRController>{LeftHand}/Primary2DAxis" },
            { VR_CONTROLLER_LEFT_THUMBSTICK_PRESS, "<XRController>{LeftHand}/thumbstickClicked" },
            { VR_CONTROLLER_LEFT_HAPTIC_DEVICE, "<XRController>{LeftHand}/*" },

            // Right VR controller mappings
            { VR_CONTROLLER_A, "<XRController>{RightHand}/primaryButton" },
            { VR_CONTROLLER_B, "<XRController>{RightHand}/secondaryButton" },
            { VR_CONTROLLER_SELECT, "<XRController>{RightHand}/menuButton" },
            { VR_CONTROLLER_RIGHT_GRIP, "<XRController>{RightHand}/gripButton" },
            { VR_CONTROLLER_RIGHT_TRIGGER, "<XRController>{RightHand}/trigger" },
            { VR_CONTROLLER_RIGHT_THUMBSTICK, "<XRController>{RightHand}/Primary2DAxis" },
            { VR_CONTROLLER_RIGHT_THUMBSTICK_PRESS, "<XRController>{RightHand}/thumbstickClicked" },
            { VR_CONTROLLER_RIGHT_HAPTIC_DEVICE, "<XRController>{RightHand}/*" },

            // Gamepad mappings
            { GAMEPAD_A, "<Gamepad>/buttonSouth" },
            { GAMEPAD_B, "<Gamepad>/buttonEast" },
            { GAMEPAD_X, "<Gamepad>/buttonWest" },
            { GAMEPAD_Y, "<Gamepad>/buttonNorth" },
            { GAMEPAD_SELECT, "<Gamepad>/select" },
            { GAMEPAD_START, "<Gamepad>/start" },
            { GAMEPAD_LEFT_BUMPER, "<Gamepad>/leftShoulder" },
            { GAMEPAD_RIGHT_BUMPER, "<Gamepad>/rightShoulder" },
            { GAMEPAD_LEFT_TRIGGER, "<Gamepad>/leftTrigger" },
            { GAMEPAD_RIGHT_TRIGGER, "<Gamepad>/rightTrigger" },
            { GAMEPAD_LEFT_THUMBSTICK, "<Gamepad>/leftStick" },
            { GAMEPAD_RIGHT_THUMBSTICK, "<Gamepad>/rightStick" },
            { GAMEPAD_LEFT_THUMBSTICK_PRESS, "<Gamepad>/leftStickPress" },
            { GAMEPAD_RIGHT_THUMBSTICK_PRESS, "<Gamepad>/rightStickPress" },
            { GAMEPAD_DPAD_UP, "<Gamepad>/dpad/up" },
            { GAMEPAD_DPAD_DOWN, "<Gamepad>/dpad/down" },
            { GAMEPAD_DPAD_LEFT, "<Gamepad>/dpad/left" },
            { GAMEPAD_DPAD_RIGHT, "<Gamepad>/dpad/right" },

            // Keyboard mappings
            { KEYBOARD_A, "<keyboard>/a" },
            { KEYBOARD_W, "<keyboard>/w" },
            { KEYBOARD_S, "<keyboard>/s" },
            { KEYBOARD_D, "<keyboard>/d" },
            { KEYBOARD_SPACE, "<keyboard>/space" },
            { KEYBOARD_ESC, "<keyboard>/escape" },
            { KEYBOARD_ENTER, "<keyboard>/enter" },
            { KEYBOARD_X, "<keyboard>/#(x)" },
            { KEYBOARD_Y, "<Keyboard>/#(y)" }
        };
    }
    public static string GetBehavior(string realControl)
    {
        if (realControl == VR_CONTROLLER_RIGHT_THUMBSTICK || realControl == VR_CONTROLLER_LEFT_THUMBSTICK)
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
            ConfigManager.WriteConsoleWarning("[ControlMapPathDictionary] Control not found in control map: " + realControl);
            return "";
        }
    }

    public static List<string> getList()
    {
        return new List<string>(map.Keys);
    }

}
