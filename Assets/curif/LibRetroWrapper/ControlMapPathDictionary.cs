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
    //Adding X-Arcade Arcade2TV-XR keyboard mode mappings (works for Gen 1 dongle and 2 dongle in keyboard/"red" mode)
    public const string KEYBOARD_F = "keyboard-f";
    public const string KEYBOARD_Q = "keyboard-q";
    public const string KEYBOARD_E = "keyboard-e";
    public const string KEYBOARD_R = "keyboard-r";
    public const string KEYBOARD_T = "keyboard-t";
    public const string KEYBOARD_U = "keyboard-u";
    public const string KEYBOARD_I = "keyboard-i";
    public const string KEYBOARD_O = "keyboard-o";
    public const string KEYBOARD_P = "keyboard-p";
    public const string KEYBOARD_Z = "keyboard-z";
    public const string KEYBOARD_C = "keyboard-c";
    public const string KEYBOARD_V = "keyboard-v";
    public const string KEYBOARD_0 = "keyboard-0";
    public const string KEYBOARD_1 = "keyboard-1";
    public const string KEYBOARD_2 = "keyboard-2";
    public const string KEYBOARD_3 = "keyboard-3";
    public const string KEYBOARD_4 = "keyboard-4";
    public const string KEYBOARD_5 = "keyboard-5";
    public const string KEYBOARD_6 = "keyboard-6";
    public const string KEYBOARD_7 = "keyboard-7";
    public const string KEYBOARD_8 = "keyboard-8";
    public const string KEYBOARD_9 = "keyboard-9";
    public const string MOUSE = "mouse";
    public const string MOUSE_LEFT = "mouse-left";
    public const string MOUSE_RIGHT = "mouse-right";
    public const string MOUSE_MIDDLE = "mouse-middle";
    public const string MOUSE_WHEELUP = "mouse-scroll-up";
    public const string MOUSE_WHEELDOWN = "mouse-scroll-down";
    public const string MOUSE_HORIZ_WHEELUP = "mouse-scroll-left";
    public const string MOUSE_HORIZ_WHEELDOWN = "mouse-scroll-right";
    public const string MOUSE_BUTTON_4 = "mouse-button-4";
    public const string MOUSE_BUTTON_5 = "mouse-button-5";

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

            // Mouse mappings
            { MOUSE, "<Mouse>/delta" },
            { MOUSE_LEFT, "<Mouse>/leftButton" },
            { MOUSE_RIGHT, "<Mouse>/rightButton" },
            { MOUSE_MIDDLE, "<Mouse>/middleButton" },
            { MOUSE_WHEELUP, "<Mouse>/scroll/y" },
            { MOUSE_WHEELDOWN, "<Mouse>/scroll/y" },
            { MOUSE_HORIZ_WHEELUP, "<Mouse>/scroll/x" },
            { MOUSE_HORIZ_WHEELDOWN, "<Mouse>/scroll/x" },
            { MOUSE_BUTTON_4, "<Mouse>/button4" },
            { MOUSE_BUTTON_5, "<Mouse>/button5" },

            // Keyboard mappings
            { KEYBOARD_A, "<keyboard>/a" },
            { KEYBOARD_W, "<keyboard>/w" },
            { KEYBOARD_S, "<keyboard>/s" },
            { KEYBOARD_D, "<keyboard>/d" },
            { KEYBOARD_SPACE, "<keyboard>/space" },
            { KEYBOARD_ESC, "<keyboard>/escape" },
            { KEYBOARD_ENTER, "<keyboard>/enter" },
            { KEYBOARD_X, "<keyboard>/#(x)" },
            { KEYBOARD_Y, "<Keyboard>/#(y)" },
            { KEYBOARD_F, "<Keyboard>/#(f)" },
            { KEYBOARD_Q, "<Keyboard>/#(q)" },
            { KEYBOARD_E, "<Keyboard>/#(e)" },
            { KEYBOARD_R, "<Keyboard>/#(r)" },
            { KEYBOARD_T, "<Keyboard>/#(t)" },
            { KEYBOARD_U, "<Keyboard>/#(u)" },
            { KEYBOARD_I, "<Keyboard>/#(i)" },
            { KEYBOARD_O, "<Keyboard>/#(o)" },
            { KEYBOARD_P, "<Keyboard>/#(p)" },
            { KEYBOARD_Z, "<Keyboard>/#(z)" },
            { KEYBOARD_C, "<Keyboard>/#(c)" },
            { KEYBOARD_V, "<Keyboard>/#(v)" },
            { KEYBOARD_0, "<Keyboard>/#(0)" },
            { KEYBOARD_1, "<Keyboard>/#(1)" },
            { KEYBOARD_2, "<Keyboard>/#(2)" },
            { KEYBOARD_3, "<Keyboard>/#(3)" },
            { KEYBOARD_4, "<Keyboard>/#(4)" },
            { KEYBOARD_5, "<Keyboard>/#(5)" },
            { KEYBOARD_6, "<Keyboard>/#(6)" },
            { KEYBOARD_7, "<Keyboard>/#(7)" },
            { KEYBOARD_8, "<Keyboard>/#(8)" },
            { KEYBOARD_9, "<Keyboard>/#(9)" }
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
