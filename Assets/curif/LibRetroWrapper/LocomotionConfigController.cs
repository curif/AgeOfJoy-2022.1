using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LocomotionConfigController : MonoBehaviour
{
    public GlobalConfiguration globalConfiguration;

    [Tooltip("Change controls component in the PlayerControllerF")]
    public ChangeControls changeControls;
    private bool isListenerAdded = false;

    void Start()
    {
        OnEnable();
        change();
    }

    void change()
    {
        ConfigInformation config = globalConfiguration.Configuration;
        if (config == null)
        {
            ConfigManager.WriteConsoleError("[LocomotionConfigController.change] can't get global configuration");
            return;
        }

        if (config.locomotion?.teleportEnabled != null)
        {
            changeControls.teleportationEnabled = (bool)config.locomotion.teleportEnabled;
        }
        if (config.locomotion?.turnSpeed != null)
        {
            changeControls.turnSpeed = (float)config.locomotion.turnSpeed;
        }

        if (config.locomotion?.moveSpeed != null)
        {
            changeControls.moveSpeed = (float)config.locomotion.moveSpeed;
        }

        if (config.locomotion?.SnapTurnActive != null)
            changeControls.SnapTurnActive = (bool)config.locomotion.SnapTurnActive;

        if (config.locomotion?.SnapTurnAmount != null)
            changeControls.SnapTurnAmount = (float)config.locomotion.SnapTurnAmount;

        ConfigManager.WriteConsole("[LocomotionConfigController.change] applied locomotion configuration");
    }

    void OnGlobalConfigChanged()
    {
        ConfigManager.WriteConsole("[LocomotionConfigController.OnGlobalConfigChanged] invoked");
        change();
    }

    void addListener()
    {
        if (isListenerAdded) return;
        globalConfiguration?.OnGlobalConfigChanged.AddListener(OnGlobalConfigChanged);
        isListenerAdded = true;
    }
    void removeListener()
    {
        if (!isListenerAdded) return;
        globalConfiguration?.OnGlobalConfigChanged.RemoveListener(OnGlobalConfigChanged);
        isListenerAdded = false;
    }

    void OnEnable()
    {
        // Listen for the config reload message
        addListener();
    }

    void OnDisable()
    {
        removeListener();
    }
}
