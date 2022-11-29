/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
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
