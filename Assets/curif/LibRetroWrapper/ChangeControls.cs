using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class ChangeControls : MonoBehaviour
{
    public GameObject leftHandXRControl, rightHandXRControl;

    public GameObject leftHandPrefab;
    public GameObject rightHandPrefab;
    public GameObject leftJoystickPrefab;
    public GameObject rightJoystickPrefab;
    [Tooltip("Teleport interactor gameobject contains it")]
    public XRRayInteractor xrrayInteractor;

    ActionBasedContinuousTurnProvider actionBasedContinuousTurnProvider;
    DynamicMoveProvider dynamicMoveProvider;
    bool reservedTeleportationEnabled;
    ActionBasedController controllerLeftHand;
    ActionBasedController controllerRightHand;
    GameObject leftHandModel;
    GameObject rightHandModel;
    GameObject leftJoystickModel;
    GameObject rightJoystickModel;

    bool isPlaying = false;

    void Start()
    {
        controllerLeftHand = leftHandXRControl.GetComponent<ActionBasedController>();
        controllerRightHand = rightHandXRControl.GetComponent<ActionBasedController>();

        actionBasedContinuousTurnProvider = GetComponent<ActionBasedContinuousTurnProvider>();
        dynamicMoveProvider = GetComponent<DynamicMoveProvider>();

        reservedTeleportationEnabled = xrrayInteractor.enabled;

        leftJoystickModel = GameObject.Instantiate(leftJoystickPrefab, controllerLeftHand.modelParent);
        rightJoystickModel = GameObject.Instantiate(rightJoystickPrefab, controllerRightHand.modelParent);
        leftHandModel = GameObject.Instantiate(leftHandPrefab, controllerLeftHand.modelParent);
        rightHandModel = GameObject.Instantiate(rightHandPrefab, controllerRightHand.modelParent);

        controllerLeftHand.model = leftHandModel.transform;
        controllerRightHand.model = rightHandModel.transform;

        leftJoystickModel.SetActive(false);
        rightJoystickModel.SetActive(false);
    }

    public void PlayerMode(bool modePlaying)
    {
        ConfigManager.WriteConsole($"[ChangeControls.Playermode] modePlaying: {modePlaying} actual status playing: {isPlaying}");
        if (isPlaying == modePlaying)
            return;
        changeMode(modePlaying);
    }

    private void activateDeactivateControls()
    {
        leftJoystickModel.SetActive(isPlaying);
        rightJoystickModel.SetActive(isPlaying);
        leftHandModel.gameObject.SetActive(!isPlaying);
        rightHandModel.gameObject.SetActive(!isPlaying);
    }
    private void setControllers()
    {
        controllerLeftHand.model = isPlaying ? leftJoystickModel.transform : leftHandModel.transform;
        controllerRightHand.model = isPlaying ? rightJoystickModel.transform : rightHandModel.transform;
    }
    private void changeMode(bool modePlaying)
    {
        isPlaying = modePlaying;
        activateDeactivateControls();
        setControllers();
        dynamicMoveProvider.enabled = !isPlaying;
        actionBasedContinuousTurnProvider.enabled = !isPlaying;

        if (isPlaying)
        {
            reservedTeleportationEnabled = xrrayInteractor.enabled;
            xrrayInteractor.enabled = false;
        }
        else
        {
            xrrayInteractor.enabled = reservedTeleportationEnabled;
        }
    }

    //units by second
    public float moveSpeed
    {
        get
        {
            return dynamicMoveProvider.moveSpeed;
        }
        set
        {
            dynamicMoveProvider.moveSpeed = value;
        }
    }

    //number of degress/second to rotate when turn
    public float turnSpeed
    {
        get
        {
            return actionBasedContinuousTurnProvider.turnSpeed;
        }
        set
        {
            actionBasedContinuousTurnProvider.turnSpeed = value;
        }
    }

    public bool teleportationEnabled
    {
        get
        {
            return isPlaying ? xrrayInteractor.enabled : reservedTeleportationEnabled;
        }
        set
        {
            reservedTeleportationEnabled = value;
            if (!isPlaying)
                xrrayInteractor.enabled = reservedTeleportationEnabled;
        }
    }
}
