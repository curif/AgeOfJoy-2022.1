using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;

using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;

public class BehaviourWaiting : MonoBehaviour
{

  public enum WaitingTypeController
  {
    Waiting,
    Talking
  }
  public WaitingTypeController type = WaitingTypeController.Waiting;

  void Start()
  {
    Animator animator = gameObject.GetComponent<Animator>();
    Debug.Log($"Animator for {gameObject.name} {type.ToString()}");
    if (type == WaitingTypeController.Waiting)
    {
      Debug.Log($"Animator for {gameObject.name} trigger Waiting");
      animator.ResetTrigger("Talking");
      animator.SetTrigger("Waiting");
    }
    else if (type == WaitingTypeController.Talking)
    {
      Debug.Log($"Animator for {gameObject.name} trigger Talking");
      animator.SetTrigger("Talking");
      animator.ResetTrigger("Waiting");
    }
  }

}
