using CleverCrow.Fluid.BTs.Tasks.Actions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


[RequireComponent(typeof(XRGrabInteractable))]
public class UsdCoin : MonoBehaviour
{

    [SerializeField]
    bool isGrabbed = false;
    public CoinSlotController ctrl;

    GameObject CoinSlot;
    //this object must be attached to a hand
    Transform originalParent;
    Vector3 originalLocalPosition;
    Quaternion originalLocalRotation;
    XRGrabInteractable grabInteractable;

    private bool isListenerAdded = false;

    void Start()
    {
        CoinSlot = null;
        preserveOriginalValues();
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    void preserveOriginalValues()
    {
        originalParent = gameObject.transform.parent;
        originalLocalPosition = gameObject.transform.localPosition;
        originalLocalRotation = gameObject.transform.localRotation;
    }

    void restoreOriginalValues()
    {
        gameObject.transform.parent = originalParent;
        gameObject.transform.localPosition = originalLocalPosition;
        gameObject.transform.localRotation = originalLocalRotation;
    }


    // Method to force release the object
    public void ForceRelease()
    {
        // Check if the object is currently grabbed
        if (grabInteractable.isSelected)
        {
            // Call the SelectExit method to simulate releasing the object
            IXRSelectInteractor currentInteractor = grabInteractable.interactorsSelecting[0];

            // Force the release
            grabInteractable.interactionManager.SelectExit(currentInteractor, grabInteractable);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        //Debug.Log($" OnTriggerEnter Collision with  {col.gameObject.name}");
        if (col.gameObject.tag == "CoinSlot" && isGrabbed)
        {
            CoinSlot = col.gameObject;
        }
    }

    // called from the SelectEventArgs  
    // in the XRGrabInteractable Interactable events.
    public void Selected()
    {
        isGrabbed = true;
        // ConfigManager.WriteConsole($"[UsdCoin.Selected]");
    }

    public void UnSelected()
    {
        isGrabbed = false;
        // ConfigManager.WriteConsole($"[UsdCoin.UnSelected]");

        if (CoinSlot != null)
        {
            ConfigManager.WriteConsole($"[UsdCoin.UnSelected] Dropped coin on {CoinSlot.name}");
            ctrl = CoinSlot.GetComponent<CoinSlotController>();
            ctrl.insertCoin();
            CoinSlot = null;
        }
        restoreOriginalValues();
    }

    private void OnPlayerStopPlaying()
    {
        ForceRelease();
    }

    void addListener()
    {
        if (isListenerAdded) return;
        LibretroMameCore.OnPlayerStopPlaying?.AddListener(OnPlayerStopPlaying);
        isListenerAdded = true;
    }
    void removeListener()
    {
        if (!isListenerAdded) return;
        LibretroMameCore.OnPlayerStopPlaying.RemoveListener(OnPlayerStopPlaying);
        isListenerAdded = false;
    }

    void OnEnable()
    {
        addListener();
    }

    private void OnApplicationPause()
    {
        removeListener();
    }

    private void OnDisable()
    {
        removeListener();
    }

}

