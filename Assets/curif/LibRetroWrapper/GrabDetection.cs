using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEditor;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(BoxCollider))]
public class GrabDetection : MonoBehaviour
{

    //the LeftHand && RightHand have a Direct Interactor Left/Right Gameobject
    //with a XRDirectInteractor component. Set the InteractionLayerMask to partGrabable.
    //There are a sphere collider also, adjust it to the size of the control.

    // UnityEvents for touch and grab (enter and exit)
    public UnityEvent OnPlayerTouchEnter; //player touch
    public UnityEvent OnPlayerTouchExit;
    public UnityEvent OnGrabEnter; //player grab
    public UnityEvent OnGrabExit;

    // Interaction Layer for interaction
    string[] layers = new string[] { "partGrabable" };

    // Boolean to control whether the object can be grabbed
    public bool canBeGrabbed = true;

    private XRGrabInteractable interactable;
    private void Start()
    {
        // Initialize the XR Grab Interactable
        interactable = GetComponent<XRGrabInteractable>();
        // right hand / left hand -> XRDirectInteractor -> layerMask = partGrabable (right includes coin too).
        interactable.interactionLayers = InteractionLayerMask.GetMask(layers);
        
        // Register interaction events
        interactable.selectEntered.AddListener(OnGrabEnterEvent);
        interactable.selectExited.AddListener(OnGrabExitEvent);
        interactable.hoverEntered.AddListener(OnTouchEnterEvent);
        interactable.hoverExited.AddListener(OnTouchExitEvent);
    }

    private bool isPlayer(GameObject interactor)
    {
        return interactor.name == "Direct Interactor Left" || interactor.name == "Direct Interactor Right"; 
    }

    private void OnTouchEnterEvent(HoverEnterEventArgs args)
    {
        GameObject interactor = args.interactorObject.transform.gameObject;

        if (isPlayer(interactor))
            OnPlayerTouchEnter?.Invoke();
    }

    private void OnTouchExitEvent(HoverExitEventArgs args)
    {
        GameObject interactor = args.interactorObject.transform.gameObject;

        if (isPlayer(interactor))
            OnPlayerTouchExit?.Invoke();
    }
    private void OnGrabEnterEvent(SelectEnterEventArgs args)
    {
        GameObject interactor = args.interactorObject.transform.gameObject;

        if (canBeGrabbed && isPlayer(interactor))
            OnGrabEnter?.Invoke();
    }

    private void OnGrabExitEvent(SelectExitEventArgs args)
    {
        GameObject interactor = args.interactorObject.transform.gameObject;

        if (canBeGrabbed && isPlayer(interactor))
            OnGrabExit?.Invoke();
    }

    private void OnDestroy()
    {
        // Unregister interaction events
        interactable.selectEntered.RemoveListener(OnGrabEnterEvent);
        interactable.selectExited.RemoveListener(OnGrabExitEvent);
        interactable.hoverEntered.RemoveListener(OnTouchEnterEvent);
        interactable.hoverExited.RemoveListener(OnTouchExitEvent);
    }
}
