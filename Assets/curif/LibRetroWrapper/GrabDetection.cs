using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

[RequireComponent(typeof(XRGrabInteractable))]
public class GrabDetection : MonoBehaviour
{

    //the LeftHand && RightHand have a Direct Interactor Left/Right Gameobject
    //with a XRDirectInteractor component. Set the InteractionLayerMask to partGrabable.
    //There are a sphere collider also, adjust it to the size of the control.

    // UnityEvents for touch and grab (enter and exit)
    public UnityEvent OnPlayerTouchEnter = new(); //player touch
    public UnityEvent OnPlayerTouchExit = new();
    public UnityEvent OnGrabEnter = new(); //player grab
    public UnityEvent OnGrabExit = new();

    // Interaction Layer for interaction
    static string[] layers = new string[] { "InteractablePart" };

    // Boolean to control whether the object can be grabbed
    public bool isGrabbable = false;
    public bool isTouchable = true;

    public bool Initialized;

    private XRGrabInteractable interactable;
    private void Awake()
    {
        // Initialize the XR Grab Interactable
        interactable = GetComponent<XRGrabInteractable>();
        // right hand / left hand -> XRDirectInteractor -> layerMask = partGrabable (right includes coin too).
        gameObject.layer = LayerMask.NameToLayer("InteractablePart");

        interactable.interactionLayers = InteractionLayerMask.GetMask(layers);
        
        // Register interaction events
        interactable.selectEntered.AddListener(OnGrabEnterEvent);
        interactable.selectExited.AddListener(OnGrabExitEvent);
        interactable.hoverEntered.AddListener(OnTouchEnterEvent);
        interactable.hoverExited.AddListener(OnTouchExitEvent);

        Initialized = true;
    }

    private bool isPlayer(GameObject interactor)
    {
        //note: direct interactors should be in the layer "player" to detect collisions with "interactable part"
        return interactor.name == "Direct Interactor Left" || interactor.name == "Direct Interactor Right"; 
    }

    private void OnTouchEnterEvent(HoverEnterEventArgs args)
    {
        GameObject interactor = args.interactorObject.transform.gameObject;

        if (isPlayer(interactor) && isTouchable)
            OnPlayerTouchEnter?.Invoke();
    }

    private void OnTouchExitEvent(HoverExitEventArgs args)
    {
        GameObject interactor = args.interactorObject.transform.gameObject;

        if (isPlayer(interactor) && isTouchable)
            OnPlayerTouchExit?.Invoke();
    }
    private void OnGrabEnterEvent(SelectEnterEventArgs args)
    {
        GameObject interactor = args.interactorObject.transform.gameObject;

        if (isGrabbable && isPlayer(interactor))
            OnGrabEnter?.Invoke();
    }

    private void OnGrabExitEvent(SelectExitEventArgs args)
    {
        GameObject interactor = args.interactorObject.transform.gameObject;

        if (isGrabbable && isPlayer(interactor))
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
