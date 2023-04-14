using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ChangeControls : MonoBehaviour
{
  public GameObject leftHandXRControl, rightHandXRControl;
  public GameObject leftHandPrefab;
  public GameObject rightHandPrefab;
  public GameObject leftJoystickPrefab;
  public GameObject rightJoystickPrefab;
  
  ActionBasedController controllerLeftHand;
  ActionBasedController controllerRightHand;

  GameObject leftHandModel;
  GameObject rightHandModel;
  GameObject leftJoystickModel;
  GameObject rightJoystickModel;

  void Start()
  {
    controllerLeftHand = leftHandXRControl.GetComponent<ActionBasedController>();
    controllerRightHand = rightHandXRControl.GetComponent<ActionBasedController>();
    
    leftJoystickModel = GameObject.Instantiate(leftJoystickPrefab, controllerLeftHand.modelParent);
    rightJoystickModel = GameObject.Instantiate(rightJoystickPrefab, controllerRightHand.modelParent);
    leftHandModel = GameObject.Instantiate(leftHandPrefab, controllerLeftHand.modelParent);
    rightHandModel = GameObject.Instantiate(rightHandPrefab, controllerRightHand.modelParent);

    controllerLeftHand.model = leftHandModel.transform;
    controllerRightHand.model = rightHandModel.transform;

    PlayerMode(false);
  }
  
  public void PlayerMode(bool modePlaying)
  {
    ConfigManager.WriteConsole($"[ChangeControls.Playermode] modePlaying: {modePlaying}");
    if (modePlaying) 
    {
      leftJoystickModel.SetActive(true);
      rightJoystickModel.SetActive(true);
      leftHandModel.SetActive(false);
      rightHandModel.SetActive(false);
      controllerLeftHand.model = leftJoystickModel.transform;
      controllerRightHand.model = rightJoystickModel.transform;
      controllerLeftHand.enableInputActions = false;
      controllerRightHand.enableInputActions = false;
    }
    else
    {
      controllerLeftHand.model = leftHandModel.transform;
      controllerRightHand.model = rightHandModel.transform;
      leftJoystickModel.SetActive(false);
      rightJoystickModel.SetActive(false);
      leftHandModel.gameObject.SetActive(true);
      rightHandModel.gameObject.SetActive(true);
      controllerLeftHand.enableInputActions = true;
      controllerRightHand.enableInputActions = true;
    }
  }
}
