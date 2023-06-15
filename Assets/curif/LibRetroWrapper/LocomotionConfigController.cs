using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class LocomotionConfigController : MonoBehaviour
{
    public GlobalConfiguration globalConfiguration;

    [Tooltip("Change controls component in the PlayerControllerF")]
    public ChangeControls changeControls;

    // Start is called before the first frame update
    void Start()
    {
        OnEnable();
        change();
    }

    void change()
    {
        ConfigInformation config = globalConfiguration.Configuration;
        if (config.locomotion?.teleportEnabled != null)
        {
            changeControls.teleportationEnabled = (bool)config.locomotion.teleportEnabled;
        }
        if (config.locomotion?.moveSpeed != null)
        {
            changeControls.moveSpeed = (float)config.locomotion.moveSpeed;
        }
        if (config.locomotion?.turnSpeed != null)
        {
            changeControls.turnSpeed = (float)config.locomotion.turnSpeed;
        }
    }
    void OnGlobalConfigChanged()
    {
        change();
    }

    void OnEnable()
    {
        // Listen for the config reload message
        globalConfiguration?.OnGlobalConfigChanged.AddListener(OnGlobalConfigChanged);
    }

    void OnDisable()
    {
        // Stop listening for the config reload message
        globalConfiguration?.OnGlobalConfigChanged.RemoveListener(OnGlobalConfigChanged);
    }
}
