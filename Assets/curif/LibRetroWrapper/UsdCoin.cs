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

  void Start()
  {
    CoinSlot = null;
    preserveOriginalValues();
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

}

