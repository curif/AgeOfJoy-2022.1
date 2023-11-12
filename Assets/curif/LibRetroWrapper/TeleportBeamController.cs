using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class BeamController : MonoBehaviour
{
    [Tooltip("Input action to activate the beam")]
    public InputActionProperty inputAction;

    [Tooltip("Reference to the Teleport Interactor GameObject")]
    public GameObject teleportInteractor;

    void Start()
    {
        teleportInteractor.SetActive(false);
    }
    private void OnEnable()
    {
        // Subscribe to the input action's started and canceled events
        inputAction.action.started += OnInputActionStarted;
        inputAction.action.canceled += OnInputActionCanceled;
    }

    private void OnDisable()
    {
        // Unsubscribe from the input action's started and canceled events
        inputAction.action.started -= OnInputActionStarted;
        inputAction.action.canceled -= OnInputActionCanceled;

        teleportInteractor.SetActive(false);
    }

    private void OnInputActionStarted(InputAction.CallbackContext context)
    {
        // Show the beam
        // ConfigManager.WriteConsole("[BeamController.OnInputAction] beam activated");
        teleportInteractor.SetActive(true);
    }

    private void OnInputActionCanceled(InputAction.CallbackContext context)
    {
        // Hide the beam
        // ConfigManager.WriteConsole("[BeamController.OnInputAction] beam de-activated");
        teleportInteractor.SetActive(false);
    }
}
