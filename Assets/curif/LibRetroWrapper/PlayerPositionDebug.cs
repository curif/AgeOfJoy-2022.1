using System;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerPositionDebug : MonoBehaviour
{

    public int ShowEverySecs = 2;
    public Transform XRRigTransform;
    public Transform CameraOffset;
    public XROrigin xrorigin;
    public CharacterController characterController;


    private LibretroMameCore.Waiter waiter;

    public void Start()
    {
        waiter = new(ShowEverySecs);
        if (xrorigin == null)
            xrorigin = GetComponent<XROrigin>();
    }

    public void Update()
    {
        if (!waiter.Finished())
            return;

        ConfigManager.WriteConsole("[PlayerPositionDebug] ----------------------");
        ConfigManager.WriteConsole($"[PlayerPositionDebug] XRRig Y: {XRRigTransform.position.y}");
        ConfigManager.WriteConsole($"[PlayerPositionDebug] Camera:       tracking mode: {xrorigin.RequestedTrackingOriginMode}");
        ConfigManager.WriteConsole($"[PlayerPositionDebug]           xrorigin Y offset: {xrorigin.CameraYOffset}");
        ConfigManager.WriteConsole($"[PlayerPositionDebug]                            : {ConfigInformation.Player.FindNearestKey(xrorigin.CameraYOffset)}");
        ConfigManager.WriteConsole($"[PlayerPositionDebug]                    Y offset: {CameraOffset.position.y}");
        ConfigManager.WriteConsole($"[PlayerPositionDebug]                       Scale: {CameraOffset.localScale.y}");
        ConfigManager.WriteConsole($"[PlayerPositionDebug] Character:           Height: {characterController.height}");
        ConfigManager.WriteConsole($"[PlayerPositionDebug]                    Center Y: {characterController.center.y}");
        ConfigManager.WriteConsole("[PlayerPositionDebug] END ------------------");
        waiter.Reset();
    }


}