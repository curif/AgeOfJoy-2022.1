using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ControlMapPathDictionary
{
  static Dictionary<string, Dictionary<string, string>> map; 

  static ControlMapPathDictionary() {
    map = new Dictionary<string, Dictionary<string, string>>();

    map.Add("vr-left", new());
    map.Add("vr-right", new());
    map.Add("gamepad", new());
    
    /*
     * secondaryButton [LeftHand XR Controller] = Y button
        primaryButton [LeftHand XR Controller] = X button
        secondaryButton [RightHand XR Controller] = B button
        primaryButton [RightHand XR Controller] = A buttonsecondaryButton [LeftHand XR Controller] = Y button
        primaryButton [LeftHand XR Controller] = X button
        secondaryButton [RightHand XR Controller] = B button
        primaryButton [RightHand XR Controller] = A button
    */

    // Left VR controller mappings
    //map["vr-left"].Add("x-button", "<XRController>{LeftHand}/gripButton"); //primaryButton
    map["vr-left"].Add("x-button", "<XRController>{LeftHand}/primaryButton"); //primaryButton
    map["vr-left"].Add("y-button", "<XRController>{LeftHand}/secondaryButton");
    map["vr-left"].Add("start-button", "<XRController>{LeftHand}/startButton");
    map["vr-left"].Add("grip", "<XRController>{LeftHand}/gripButton");
    map["vr-left"].Add("trigger", "<XRController>{LeftHand}/triggerButton");
    map["vr-left"].Add("thumbstick-down", "<XRController>{LeftHand}/thumbstick/down");
    map["vr-left"].Add("thumbstick-up", "<XRController>{LeftHand}/thumbstick/up");
    map["vr-left"].Add("thumbstick-left", "<XRController>{LeftHand}/thumbstick/left");
    map["vr-left"].Add("thumbstick-right", "<XRController>{LeftHand}/thumbstick/right");
    map["vr-left"].Add("thumbstick-press", "<XRController>{LeftHand}/thumbstickClicked");

    // Right VR controller mappings
    map["vr-right"].Add("a-button", "<XRController>{RightHand}/primaryButton");
    map["vr-right"].Add("b-button", "<XRController>{RightHand}/secondaryButton");
    map["vr-right"].Add("select-button", "<XRController>{RightHand}/selectButton");
    map["vr-right"].Add("grip", "<XRController>{RightHand}/gripButton");
    map["vr-right"].Add("trigger", "<XRController>{RightHand}/triggerButton");
    map["vr-right"].Add("thumbstick-down", "<XRController>{RightHand}/thumbstick/down");
    map["vr-right"].Add("thumbstick-up", "<XRController>{RightHand}/thumbstick/up");
    map["vr-right"].Add("thumbstick-left", "<XRController>{RightHand}/thumbstick/left");
    map["vr-right"].Add("thumbstick-right", "<XRController>{RightHand}/thumbstick/right");
    map["vr-right"].Add("thumbstick-press", "<XRController>{RightHand}/thumbstickClicked");

    map["gamepad"].Add("a-button", "<Gamepad>/buttonSouth");
    map["gamepad"].Add("b-button", "<Gamepad>/buttonEast");
    map["gamepad"].Add("x-button", "<Gamepad>/buttonWest");
    map["gamepad"].Add("y-button", "<Gamepad>/buttonNorth");
    map["gamepad"].Add("select-button", "<Gamepad>/select");
    map["gamepad"].Add("start-button", "<Gamepad>/start");
    map["gamepad"].Add("left-bumper", "<Gamepad>/leftShoulder");
    map["gamepad"].Add("right-bumper", "<Gamepad>/rightShoulder");
    map["gamepad"].Add("left-trigger", "<Gamepad>/leftTrigger");
    map["gamepad"].Add("right-trigger", "<Gamepad>/rightTrigger");
    map["gamepad"].Add("left-thumbstick-down", "<Gamepad>/leftStick/down");
    map["gamepad"].Add("left-thumbstick-up", "<Gamepad>/leftStick/up");
    map["gamepad"].Add("left-thumbstick-left", "<Gamepad>/leftStick/left");
    map["gamepad"].Add("left-thumbstick-right", "<Gamepad>/leftStick/right");
    map["gamepad"].Add("left-thumbstick-press", "<Gamepad>/leftStickPress");
    map["gamepad"].Add("right-thumbstick-down", "<Gamepad>/rightStick/down");
    map["gamepad"].Add("right-thumbstick-up", "<Gamepad>/rightStick/up");
    map["gamepad"].Add("right-thumbstick-left", "<Gamepad>/rightStick/left");
    map["gamepad"].Add("right-thumbstick-right", "<Gamepad>/rightStick/right");
    map["gamepad"].Add("right-thumbstick-press", "<Gamepad>/rightStickPress");

  }

  public static string GetInputPath(string mapName, string controlName) {
    if (map.ContainsKey(mapName) && map[mapName].ContainsKey(controlName)) {
        return map[mapName][controlName];
    }
    else {
        ConfigManager.WriteConsole("[ControlMapPathDictionary] Control or map name not found in control map: " + controlName + ", " + mapName);
        return "";
    }
  }

}
