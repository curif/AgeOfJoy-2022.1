using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


[RequireComponent(typeof(XRGrabInteractable))]
public class UsdCoin : MonoBehaviour
{

    [SerializeField]
    bool isGrabbed = false;

    XRGrabInteractable grabbable;
    GameObject CoinSlot;
    //this object must be attached to a hand
    Transform originalParent;
    Vector3 originalLocalPosition;
    Quaternion originalLocalRotation;
    
    // called from the SelectEventArgs in the XRGrabInteractable Interactable events.
    public void Selected()
    {
      isGrabbed = true;
    }
    public void UnSelected()
    {
      isGrabbed = false;
    }

    void Start() {

        grabbable = gameObject.GetComponent<XRGrabInteractable>();
        // Debug.Log($"obtained grabbable: {grabbable}");
        CoinSlot = null;

        originalParent = gameObject.transform.parent;
        originalLocalPosition = gameObject.transform.localPosition;
        originalLocalRotation = gameObject.transform.localRotation;
    }

    void Update() {

      if (!isGrabbed) {
        if (CoinSlot != null) {
            ConfigManager.WriteConsole($"Dropped coin on {CoinSlot.name}");
            var ctrl = CoinSlot.GetComponent<CoinSlotController>();
            ctrl.insertCoin();
            CoinSlot = null;
        }
        gameObject.transform.parent = originalParent;
        gameObject.transform.localPosition = originalLocalPosition;
        gameObject.transform.localRotation = originalLocalRotation;
      }
    }

    void OnTriggerEnter(Collider col) {
      //Debug.Log($" OnTriggerEnter Collision with  {col.gameObject.name}");
      if (col.gameObject.tag == "CoinSlot" && isGrabbed)
      {
        CoinSlot = col.gameObject;
      }
    }

}

