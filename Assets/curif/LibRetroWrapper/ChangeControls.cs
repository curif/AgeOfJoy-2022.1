using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
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
    public BeamController beamController;

    public ActionBasedContinuousTurnProvider actionBasedContinuousTurnProvider;
    public ActionBasedContinuousMoveProvider actionBasedContinuousMoveProvider;
    public ActionBasedSnapTurnProvider actionBasedSnapTurnProvider;
    InputActionProperty rightHandSnapTurnAction;
    InputActionProperty rightHandContinuousTurnAction;
    InputActionProperty leftHandMoveAction;
    bool reservedTeleportationEnabled;
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

        leftJoystickModel = GameObject.Instantiate(leftJoystickPrefab, controllerLeftHand.modelParent);
        rightJoystickModel = GameObject.Instantiate(rightJoystickPrefab, controllerRightHand.modelParent);
        reservedRightJoystickModel = rightJoystickModel;
        leftHandModel = GameObject.Instantiate(leftHandPrefab, controllerLeftHand.modelParent);
        rightHandModel = GameObject.Instantiate(rightHandPrefab, controllerRightHand.modelParent);

        controllerLeftHand.model = leftHandModel.transform;
        controllerRightHand.model = rightHandModel.transform;

        rightHandContinuousTurnAction = actionBasedContinuousTurnProvider.rightHandTurnAction;
        rightHandSnapTurnAction = actionBasedSnapTurnProvider.rightHandSnapTurnAction;
        leftHandMoveAction = actionBasedContinuousMoveProvider.leftHandMoveAction;

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
            ConfigManager.WriteConsole($"[ChangeControls.ChangeRightJoystickModel] destroy old model {alternativeModelFilePath} async: {async}");
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
        ConfigManager.WriteConsole($"[ChangeControls.ChangeRightJoystickModel] loading {modelFilePath} async: {async}");

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
        if (isPlaying)
        {
            alternativeRightJoystick = model;
            useAlternativeRightJoystick();
            // changeMode(true);
        }
    }
    void EnableDisableMeshRenderersRecursively(Transform parent, bool enable)
    {
        // Check if the parent has a mesh renderer component
        MeshRenderer renderer = parent.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            // Enable or disable the mesh renderer based on the parameter
            renderer.enabled = enable;
        }

        // Loop through all children of the parent
        foreach (Transform child in parent)
        {
            // Recursively enable/disable mesh renderers of children
            EnableDisableMeshRenderersRecursively(child, enable);
        }
    }

    private void rigthJoystickVisible(bool enabled)
    {
        EnableDisableMeshRenderersRecursively(reservedRightJoystickModel.transform, enabled);
    }

    private void activateAlternativeRigthJoystick()
    {
        if (alternativeRightJoystick == null)
            return;
            
        alternativeRightJoystick.SetActive(true);
        alternativeRightJoystick.transform.parent = controllerRightHand.modelParent;
        alternativeRightJoystick.transform.position = controllerRightHand.transform.position;
        alternativeRightJoystick.transform.rotation = controllerRightHand.transform.rotation;
    }
    private void deactivateAlternativeRigthJoystick()
    {
        if (alternativeRightJoystick != null && alternativeRightJoystick.activeSelf)
        {
            alternativeRightJoystick.SetActive(false);
            rightJoystickModel = reservedRightJoystickModel;
            rigthJoystickVisible(true);
        }
    }

    private void useAlternativeRightJoystick()
    {
        activateAlternativeRigthJoystick();
        rigthJoystickVisible(false);

        rightJoystickModel = alternativeRightJoystick;

        lightGunTarget.spaceGun = alternativeRightJoystick;
    }

    private void reserveValues()
    {
        reservedTeleportationEnabled = beamController.enabled;
    }

    private void restoreReservedValues()
    {
        beamController.enabled = reservedTeleportationEnabled;
        rightJoystickModel = reservedRightJoystickModel;
    }

    public void PlayerMode(bool modePlaying)
    {
        if (isPlaying == modePlaying)
            return;
        ConfigManager.WriteConsole($"[ChangeControls.Playermode] modePlaying: {modePlaying} actual status playing: {isPlaying}");
        changeMode(modePlaying);
    }

    private void activateDeactivateControls(bool playing)
    {
        leftJoystickModel.SetActive(playing);
        rightJoystickModel.SetActive(playing);
        leftHandModel.gameObject.SetActive(!playing);
        rightHandModel.gameObject.SetActive(!playing);
    }
    private void setControllers(bool playerIsPlaying)
    {
        controllerLeftHand.model = playerIsPlaying ? leftJoystickModel.transform : leftHandModel.transform;
        controllerRightHand.model = playerIsPlaying ? rightJoystickModel.transform : rightHandModel.transform;
    }
    private void changeMode(bool playerIsPlaying)
    {
        isPlaying = playerIsPlaying;

        if (playerIsPlaying)
        {
            activateDeactivateControls(true);
            setControllers(true);
            reserveValues();
            rightHandContinuousTurnAction.action.Disable();
            rightHandSnapTurnAction.action.Disable();
            leftHandMoveAction.action.Disable();

            beamController.enabled = false;
        }
        else
        {
            deactivateAlternativeRigthJoystick();
            restoreReservedValues();
            activateDeactivateControls(false);
            setControllers(false);

            rightHandContinuousTurnAction.action.Enable();
            rightHandSnapTurnAction.action.Enable();
            leftHandMoveAction.action.Enable();
        }
    }

    public bool SnapTurnActive
    {
        get
        {
            return actionBasedSnapTurnProvider.enableTurnLeftRight;
        }
        set
        {
            actionBasedContinuousTurnProvider.enabled = !value;
            actionBasedSnapTurnProvider.enableTurnLeftRight = value;
        }
    }
    public float SnapTurnAmount
    {
        get
        {
            return actionBasedSnapTurnProvider.turnAmount;
        }
        set
        {
            actionBasedSnapTurnProvider.turnAmount = value;
        }
    }
    //units by second
    public float moveSpeed
    {
        get
        {
            return actionBasedContinuousMoveProvider.moveSpeed;
        }
        set
        {
            actionBasedContinuousMoveProvider.moveSpeed = value;
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
