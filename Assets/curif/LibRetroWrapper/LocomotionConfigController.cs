using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class LocomotionConfigContoller : MonoBehaviour
{
   public GlobalConfiguration globalConfiguration;

    private GameObject player;
    private GameObject teleportInteractor;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("OVRPlayerControllerGalery");
        teleportInteractor = GameObject.Find("Teleport Interactor");

        OnEnable();
        change();
    }

    void change()
    {
        ConfigInformation config = globalConfiguration.Configuration;
        if (config.locomotion?.teleportEnabled != null)
        {
            XRRayInteractor xrrayInteractor = teleportInteractor.GetComponent<XRRayInteractor>();
            if (xrrayInteractor != null)
            {
                xrrayInteractor.enabled = (bool)config.locomotion.teleportEnabled;
            }
        }
        if (config.locomotion?.moveSpeed != null)
        {
            DynamicMoveProvider dynamicMoveProvider = player.GetComponent<DynamicMoveProvider>();
            if (dynamicMoveProvider != null)
            {
                //https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@2.4/api/UnityEngine.XR.Interaction.Toolkit.ContinuousMoveProviderBase.html#UnityEngine_XR_Interaction_Toolkit_ContinuousMoveProviderBase_moveSpeed
                dynamicMoveProvider.moveSpeed = (int)config.locomotion.moveSpeed;
            }
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
