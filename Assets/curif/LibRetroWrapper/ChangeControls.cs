using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
//using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class ChangeControls : MonoBehaviour
{
    public GameObject leftHandXRControl, rightHandXRControl;

    public GameObject leftHandPrefab;
    public GameObject rightHandPrefab;
    public GameObject leftJoystickPrefab;
    public GameObject rightJoystickPrefab;
    [Tooltip("Teleport interactor gameobject contains it")]
    //public XRRayInteractor xrrayInteractor;
    public BeamController beanController;

    public ActionBasedContinuousTurnProvider actionBasedContinuousTurnProvider;
    // public DynamicMoveProvider dynamicMoveProvider;
    public ActionBasedContinuousMoveProvider actionBasedContinuousMoveProvider;

    bool reservedTeleportationEnabled;
    float reservedMoveSpeed;
    float reservedTurnSpeed;
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

        // actionBasedContinuousTurnProvider = GetComponent<ActionBasedContinuousTurnProvider>();
        // dynamicMoveProvider = GetComponent<DynamicMoveProvider>();
        // if (dynamicMoveProvider == null)
        //     throw new System.Exception("[ChangeControls] a DynamicMoveProvider component is required");

        leftJoystickModel = GameObject.Instantiate(leftJoystickPrefab, controllerLeftHand.modelParent);
        rightJoystickModel = GameObject.Instantiate(rightJoystickPrefab, controllerRightHand.modelParent);
        leftHandModel = GameObject.Instantiate(leftHandPrefab, controllerLeftHand.modelParent);
        rightHandModel = GameObject.Instantiate(rightHandPrefab, controllerRightHand.modelParent);

        controllerLeftHand.model = leftHandModel.transform;
        controllerRightHand.model = rightHandModel.transform;

        leftJoystickModel.SetActive(false);
        rightJoystickModel.SetActive(false);
    }

    private void reserveValues()
    {
        reservedTeleportationEnabled = beanController.enabled;
        reservedMoveSpeed = actionBasedContinuousMoveProvider.moveSpeed;
        reservedTurnSpeed = actionBasedContinuousTurnProvider.turnSpeed;
    }

    private void restoreReservedValues()
    {
        beanController.enabled = reservedTeleportationEnabled;
        actionBasedContinuousMoveProvider.moveSpeed = reservedMoveSpeed;
        actionBasedContinuousTurnProvider.turnSpeed = reservedTurnSpeed;
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

        actionBasedContinuousTurnProvider.enabled = !isPlaying;
        actionBasedContinuousMoveProvider.enabled = !isPlaying;

        if (isPlaying)
        {
            reserveValues();
            beanController.enabled = false;
        }
        else
        {
            restoreReservedValues();
        }
    }

    //units by second
    public float moveSpeed
    {
        get
        {
            if (actionBasedContinuousMoveProvider.enabled)
                return actionBasedContinuousMoveProvider.moveSpeed;
            return reservedMoveSpeed;
        }
        set
        {
            ConfigManager.WriteConsole($"[ChangeControls] dynamicMoveProvider : {actionBasedContinuousMoveProvider}");
            if (actionBasedContinuousMoveProvider.enabled)
                actionBasedContinuousMoveProvider.moveSpeed = value;
            reservedMoveSpeed = value;
        }
    }

    //number of degress/second to rotate when turn
    public float turnSpeed
    {
        get
        {
            if (actionBasedContinuousTurnProvider.enabled)
                return actionBasedContinuousTurnProvider.turnSpeed;
            return reservedTurnSpeed;
        }
        set
        {
            if (actionBasedContinuousTurnProvider.enabled)
                actionBasedContinuousTurnProvider.turnSpeed = value;
            reservedTurnSpeed = value;
        }
    }

    public bool teleportationEnabled
    {
        get
        {
            return !isPlaying ? beanController.enabled : reservedTeleportationEnabled;
        }
        set
        {
            reservedTeleportationEnabled = value;
            if (!isPlaying)
                beanController.enabled = reservedTeleportationEnabled;
        }
    }
}
