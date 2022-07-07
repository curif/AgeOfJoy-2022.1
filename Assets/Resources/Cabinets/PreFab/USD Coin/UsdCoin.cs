using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OVRGrabbable))]
public class UsdCoin : MonoBehaviour
{
    // [SerializeField]
    // GameObject hand;

    // private Vector3 offset = new Vector3(0.05f, 0.05f, 0f);
    OVRGrabbable grabbable;
    GameObject CoinSlot;
    //this object must be attached to a hand
    Transform originalParent;
    Vector3 originalLocalPosition;
    Quaternion originalLocalRotation;

    void Start() {

        grabbable = gameObject.GetComponent<OVRGrabbable>();
        // Debug.Log($"obtained grabbable: {grabbable}");
        CoinSlot = null;

        originalParent = gameObject.transform.parent;
        originalLocalPosition = gameObject.transform.localPosition;
        originalLocalRotation = gameObject.transform.localRotation;
    }

    void Update() {
        if (!grabbable.isGrabbed) {
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
        // else {
        //     ConfigManager.WriteConsole("Coin grabbed");
        // }
    }

    void OnTriggerEnter(Collider col) {
        //Debug.Log($" OnTriggerEnter Collision with  {col.gameObject.name}");
        if (col.gameObject.tag == "CoinSlot" && grabbable.isGrabbed) {
            CoinSlot = col.gameObject;
            // Debug.Log($"Coin on {CoinSlot.name}");
        }
    }
}

