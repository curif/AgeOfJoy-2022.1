using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
//using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using Siccity.GLTFUtility;

public class ChangeControls : MonoBehaviour
{
    public GameObject leftHandXRControl, rightHandXRControl;

    public GameObject leftHandPrefab;
    public GameObject rightHandPrefab;
    public GameObject leftJoystickPrefab;
    public GameObject rightJoystickPrefab;
    [Tooltip("Teleport interactor gameobject contains it")]
    //public XRRayInteractor xrrayInteractor;
    public BeamController beamController;

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
    GameObject reservedRightJoystickModel;

    public GameObject LeftHand { get { return leftHandModel; } }
    public GameObject RightHand { get { return rightHandModel; } }
    public GameObject LeftJoystick { get { return leftJoystickModel; } }
    public GameObject RightJoystick { get { return rightJoystickModel; } }

    bool isPlaying = false;
    GameObject alternativeRightJoystick = null;
    string alternativeModelFilePath;

    LightGunTarget lightGunTarget;

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
        reservedRightJoystickModel = rightJoystickModel;
        leftHandModel = GameObject.Instantiate(leftHandPrefab, controllerLeftHand.modelParent);
        rightHandModel = GameObject.Instantiate(rightHandPrefab, controllerRightHand.modelParent);

        controllerLeftHand.model = leftHandModel.transform;
        controllerRightHand.model = rightHandModel.transform;

        leftJoystickModel.SetActive(false);
        rightJoystickModel.SetActive(false);
    }

    public void ChangeRightJoystickModelLightGun(LightGunTarget lightGunTarget, bool async)
    {
        GameObject model = null;

        string modelFilePath = lightGunTarget.GetModelPath();
        this.lightGunTarget = lightGunTarget;

        if (string.IsNullOrEmpty(modelFilePath))
        {
            // ConfigManager.WriteConsoleError($"[ChangeControls.ChangeRightJoystickModel] ERROR model path missing");
            return;
        }

        if (modelFilePath != alternativeModelFilePath && alternativeRightJoystick != null)
        {
            ConfigManager.WriteConsole($"[ChangeControls.ChangeRightJoystickModel] destroy old model {alternativeModelFilePath} async: {true}");
            Destroy(alternativeRightJoystick);
            alternativeModelFilePath = "";
            alternativeRightJoystick = null;
        }
        if (alternativeRightJoystick != null)
        {
            useAlternativeRightJoystick();
            return;
        }

        alternativeModelFilePath = modelFilePath;
        ConfigManager.WriteConsole($"[ChangeControls.ChangeRightJoystickModel] loading {modelFilePath} async: {true}");

        try
        {
            if (async)
            {
                Importer.ImportGLBAsync(alternativeModelFilePath, new ImportSettings(), OnFinishAsync);
                return;
            }
            else
                model = Importer.LoadFromFile(alternativeModelFilePath);
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[ChangeControls.ChangeRightJoystickModel] ERROR loading model {alternativeModelFilePath}", e);
        }

        if (model == null)
        {
            ConfigManager.WriteConsoleError($"[ChangeControls.ChangeRightJoystickModel] ERROR loading model {alternativeModelFilePath}");
            return;
        }

        alternativeRightJoystick = model;
        model.transform.parent = controllerRightHand.modelParent;
        useAlternativeRightJoystick();

        return;
    }

    void OnFinishAsync(GameObject model, AnimationClip[] animations)
    {
        ConfigManager.WriteConsole($"[ChangeControls.OnFinishAsync] finished async load {alternativeModelFilePath}");

        if (model == null)
        {
            ConfigManager.WriteConsoleError($"[ChangeControls.OnFinishAsync] ERROR loading model {alternativeModelFilePath}");
            return;
        }

        alternativeRightJoystick = model;
        model.transform.parent = controllerRightHand.modelParent;
        useAlternativeRightJoystick();
        changeMode(true);
    }
    private void useAlternativeRightJoystick()
    {
        alternativeRightJoystick.SetActive(false);
        alternativeRightJoystick.transform.position = controllerRightHand.transform.position;
        alternativeRightJoystick.transform.rotation = controllerRightHand.transform.rotation;
        
        rightJoystickModel.SetActive(false);
        rightJoystickModel = alternativeRightJoystick;
        
        lightGunTarget.spaceGun = alternativeRightJoystick;
    }

    private void reserveValues()
    {
        reservedTeleportationEnabled = beamController.enabled;
        reservedMoveSpeed = actionBasedContinuousMoveProvider.moveSpeed;
        reservedTurnSpeed = actionBasedContinuousTurnProvider.turnSpeed;
    }

    private void restoreReservedValues()
    {
        beamController.enabled = reservedTeleportationEnabled;
        actionBasedContinuousMoveProvider.moveSpeed = reservedMoveSpeed;
        actionBasedContinuousTurnProvider.turnSpeed = reservedTurnSpeed;
        rightJoystickModel = reservedRightJoystickModel;
    }

    public void PlayerMode(bool modePlaying)
    {
        ConfigManager.WriteConsole($"[ChangeControls.Playermode] modePlaying: {modePlaying} actual status playing: {isPlaying}");
        if (isPlaying == modePlaying)
            return;
        changeMode(modePlaying);
    }

    private void activateDeactivateControls(bool playing)
    {
        leftJoystickModel.SetActive(playing);
        rightJoystickModel.SetActive(playing);
        leftHandModel.gameObject.SetActive(!playing);
        rightHandModel.gameObject.SetActive(!playing);
    }
    private void setControllers(bool playing)
    {
        controllerLeftHand.model = playing ? leftJoystickModel.transform : leftHandModel.transform;
        controllerRightHand.model = playing ? rightJoystickModel.transform : rightHandModel.transform;
    }
    private void changeMode(bool modePlaying)
    {
        activateDeactivateControls(modePlaying);
        setControllers(modePlaying);

        if (isPlaying == modePlaying)
            return;

        isPlaying = modePlaying;

        actionBasedContinuousTurnProvider.enabled = !modePlaying;
        actionBasedContinuousMoveProvider.enabled = !modePlaying;

        if (modePlaying)
        {
            reserveValues();
            beamController.enabled = false;
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
            return !isPlaying ? beamController.enabled : reservedTeleportationEnabled;
        }
        set
        {
            reservedTeleportationEnabled = value;
            if (!isPlaying)
                beamController.enabled = reservedTeleportationEnabled;
        }
    }
}
