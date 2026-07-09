/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

/// <summary>Editor keyboard/gamepad input via Input System (Player Settings disallow legacy Input).</summary>
public static class MREditorInput
{
    public static bool IsHeld(KeyCode keyCode)
    {
#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
        return IsHeldInternal(keyCode);
#else
        return false;
#endif
    }

    public static bool WasPressed(KeyCode keyCode)
    {
#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
        return WasPressedInternal(keyCode);
#else
        return false;
#endif
    }

    public static bool IsAnyHeld(params KeyCode[] keyCodes)
    {
#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
        if (keyCodes == null)
            return false;
        for (int i = 0; i < keyCodes.Length; i++)
        {
            if (IsHeldInternal(keyCodes[i]))
                return true;
        }
#endif
        return false;
    }

    public static bool WasAnyPressed(params KeyCode[] keyCodes)
    {
#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
        if (keyCodes == null)
            return false;
        for (int i = 0; i < keyCodes.Length; i++)
        {
            if (WasPressedInternal(keyCodes[i]))
                return true;
        }
#endif
        return false;
    }

    public static bool WasMouseLeftPressed()
    {
#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
        return false;
#endif
    }

    public static bool WasMouseRightPressed()
    {
#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
#else
        return false;
#endif
    }

    public static bool IsMouseRightHeld()
    {
#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.rightButton.isPressed;
#else
        return false;
#endif
    }

    public static Vector2 ReadMouseDelta()
    {
#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
#else
        return Vector2.zero;
#endif
    }

    public static bool TryGetMouseScreenRay(Camera camera, out Ray ray)
    {
        ray = default;
#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
        if (camera == null || Mouse.current == null)
            return false;

        Vector2 screen = Mouse.current.position.ReadValue();
        ray = camera.ScreenPointToRay(screen);
        return true;
#else
        return false;
#endif
    }

    public static float GamepadStickX()
    {
#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
        Gamepad gamepad = Gamepad.current;
        return gamepad != null ? gamepad.leftStick.ReadValue().x : 0f;
#else
        return 0f;
#endif
    }

    public static float GamepadStickY()
    {
#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
        Gamepad gamepad = Gamepad.current;
        return gamepad != null ? gamepad.leftStick.ReadValue().y : 0f;
#else
        return 0f;
#endif
    }

    public static bool YButtonHeld()
    {
#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
        return IsHeld(KeyCode.Y) || IsGamepadButtonHeld(3);
#else
        return false;
#endif
    }

#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
    static bool IsHeldInternal(KeyCode keyCode)
    {
        ButtonControl button = ResolveButton(keyCode);
        return button != null && button.isPressed;
    }

    static bool WasPressedInternal(KeyCode keyCode)
    {
        ButtonControl button = ResolveButton(keyCode);
        return button != null && button.wasPressedThisFrame;
    }

    static ButtonControl ResolveButton(KeyCode keyCode)
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            ButtonControl keyButton = ResolveKeyboardButton(keyCode, keyboard);
            if (keyButton != null)
                return keyButton;
        }

        if (IsJoystickButton(keyCode))
            return ResolveGamepadButton(keyCode, Gamepad.current);

        return null;
    }

    static ButtonControl ResolveKeyboardButton(KeyCode keyCode, Keyboard keyboard)
    {
        switch (keyCode)
        {
            case KeyCode.Return: return keyboard.enterKey;
            case KeyCode.KeypadEnter: return keyboard.numpadEnterKey;
            case KeyCode.Escape: return keyboard.escapeKey;
            case KeyCode.Backspace: return keyboard.backspaceKey;
            case KeyCode.Space: return keyboard.spaceKey;
            case KeyCode.UpArrow: return keyboard.upArrowKey;
            case KeyCode.DownArrow: return keyboard.downArrowKey;
            case KeyCode.LeftArrow: return keyboard.leftArrowKey;
            case KeyCode.RightArrow: return keyboard.rightArrowKey;
            case KeyCode.LeftShift: return keyboard.leftShiftKey;
            case KeyCode.RightShift: return keyboard.rightShiftKey;
        }

        if (keyCode >= KeyCode.A && keyCode <= KeyCode.Z)
            return keyboard[(Key)((int)Key.A + (keyCode - KeyCode.A))];

        return null;
    }

    static bool IsJoystickButton(KeyCode keyCode) =>
        keyCode >= KeyCode.JoystickButton0 && keyCode <= KeyCode.JoystickButton19;

    static ButtonControl ResolveGamepadButton(KeyCode keyCode, Gamepad gamepad)
    {
        if (gamepad == null || !IsJoystickButton(keyCode))
            return null;

        int index = keyCode - KeyCode.JoystickButton0;
        switch (index)
        {
            case 0: return gamepad.buttonSouth;
            case 1: return gamepad.buttonEast;
            case 2: return gamepad.buttonWest;
            case 3: return gamepad.buttonNorth;
            default: return null;
        }
    }

    static bool IsGamepadButtonHeld(int index)
    {
        Gamepad gamepad = Gamepad.current;
        if (gamepad == null)
            return false;

        switch (index)
        {
            case 0: return gamepad.buttonSouth.isPressed;
            case 1: return gamepad.buttonEast.isPressed;
            case 2: return gamepad.buttonWest.isPressed;
            case 3: return gamepad.buttonNorth.isPressed;
            default: return false;
        }
    }
#endif
}
