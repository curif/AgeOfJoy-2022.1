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
using System.Linq;

using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;

[RequireComponent(typeof(NavMeshAgent))]
// [RequireComponent(typeof(Rigidbody))]
public class IntroGalleryBehaviour : MonoBehaviour
{
  public List<PlaceInformation> Destinations;

  [Tooltip("Where to go if all destinations are in use")]
  public PlaceInformation DefaultDestination;

  [Tooltip("The Animator needs JumpAndPlay, War, Fight triggers to play")]
  public bool ableToPlay;

  [SerializeField]
  public BehaviorTree tree;

  [Tooltip("Max time to spent walking to a place before abort")]
  public int TimeoutSeconds = 5;

  private NavMeshAgent agent;
  private PlaceInformation destination, selectedDestination;
  private Animator animator;

  private System.Random random = new System.Random(DateTime.Now.Millisecond);
  // private bool onCollisionWithOtherNPC = false;
  private DateTime timeout, timeToSpentInPlace;
  private List<IntroGalleryBehaviour> othersNPC;
  private String[] playTriggers = new String[3] { "JumpAndPlay", "War", "Fight" };

  public PlaceInformation Destination { get => destination; }

  void Start()
  {
    agent = gameObject.GetComponent<NavMeshAgent>();
    animator = gameObject.GetComponent<Animator>();

    StartCoroutine(runBT());
  }

  IEnumerator runBT()
  {
    othersNPC = (from npc in GameObject.FindGameObjectsWithTag("NPC")
                 where npc != gameObject
                 select npc.GetComponent<IntroGalleryBehaviour>()).
                 ToList<IntroGalleryBehaviour>();
    Debug.Log($"{gameObject.name} found {othersNPC.Count} NPCs active");

    tree = buildBT();
    while (true)
    {
      tree.Tick();
      yield return new WaitForSeconds(1f / 3f);
    }
  }

  private BehaviorTree buildBT()
  {

    BehaviorTree walkBT = new BehaviorTreeBuilder(gameObject)
      //.Condition("Has Destination objects", () => Destinations.Count > 0 && animator != null)
      .Sequence()
        .Do("Set random destination", () =>
        {
          int index = random.Next(Destinations.Count);
          selectedDestination = Destinations[index];
          return TaskStatus.Success;
        })
        .ReturnSuccess()
          .Sequence()
            .Condition("Destination taken by other NPC?", () =>
              DefaultDestination != null
              && othersNPC.FirstOrDefault(npc =>
                                          npc?.Destination != null &&
                                          UnityEngine.Object.ReferenceEquals(npc.Destination.Place, selectedDestination.Place)) != null)
            .Do("Use the default destination", () =>
            {
              selectedDestination = DefaultDestination;
              // Debug.Log($"{gameObject.name} falls into the default destination: {DefaultDestination.Place.name}");
              return TaskStatus.Success;
            })
          .End()
        .End()

        .Do("Start walking", () =>
        {
          destination = selectedDestination; //others NPCs can see this
          walkToDestination();// Debug.Log($"[IntroGalleryBehaviour] {gameObject.name} to {destination.Place.name} timeout {TimeoutSeconds}secs {timeout.ToString()}");
          return TaskStatus.Success;
        })
        .RepeatUntilSuccess()
          .Selector()
            .Condition("Timeout", () => DateTime.Now > timeout)
            .Condition("Arrived", () => Vector3.Distance(destination.Place.transform.position, transform.position) <= destination.MinimalDistanceToReachObject)
          .End()
        .End()
        .Do("Stop walking", () =>
        {
          Idle();
          timeToSpentInPlace = DateTime.Now > timeout ? DateTime.Now.AddSeconds(1) : destination.getWaitingDateTime();
          return TaskStatus.Success;
        })

        .RepeatUntilSuccess()
            .Condition("wait some time there", () => DateTime.Now > timeToSpentInPlace)
        .End()
        .Do("Clean", () =>
        {
          destination = null;
          return TaskStatus.Success;
        })
      .End()
      .Build();

    BehaviorTree playBT = new BehaviorTreeBuilder(gameObject)
        .Sequence()
          .Do("Goto default destination", () =>
          {
            selectedDestination = DefaultDestination;
            return TaskStatus.Success;
          })
          .Do("Start walking", () =>
          {
            destination = selectedDestination; //others NPCs can see this
            walkToDestination(); // Debug.Log($"[IntroGalleryBehaviour] {gameObject.name} to {destination.Place.name} timeout {TimeoutSeconds}secs {timeout.ToString()}");
            return TaskStatus.Success;
          })
          .RepeatUntilSuccess()
            .Condition("Arrived", () => Vector3.Distance(destination.Place.transform.position, transform.position) <= destination.MinimalDistanceToReachObject)
          .End()
          .Do("Stop walking", () =>
          {
            Idle();
            return TaskStatus.Success;
          })
          .Do("play", () =>
          {
            int index = random.Next(0, playTriggers.Length - 1);
            animator.SetTrigger(playTriggers[index]);
            timeToSpentInPlace = DateTime.Now.AddSeconds(30);
            return TaskStatus.Success;
          })
          .RepeatUntilSuccess()
              .Condition("wait some time playing", () => DateTime.Now > timeToSpentInPlace)
          .End()
        .End()
        .Build();

    return new BehaviorTreeBuilder(gameObject)
        .Selector()
          .Sequence()
            .Condition("Able to play", () => ableToPlay)
            .RandomChance(1, 2)
            .Splice(playBT)
          .End()
          .Splice(walkBT)
        .End()
        .Build();

  }

  // private void animator.SetTrigger(String trigger)
  // {
  //   if (!String.IsNullOrEmpty(lastTrigger))
  //     GetComponent<Animator>().Reanimator.SetTrigger(lastTrigger);
  //   // Debug.Log($"-----------------");
  //   // foreach (AnimatorControllerParameter p in animator.parameters)
  //   // {
  //   //   Debug.Log($"{p.name} is {p.type.ToString()}");
  //   // }
  //   // Debug.Log($"-----------------");

  //   lastTrigger = trigger;
  //   GetComponent<Animator>().animator.SetTrigger(trigger);
  // }
  private void Idle()
  {
    agent.ResetPath();
    animator.SetTrigger("Idle");
  }

  private void walkToDestination()
  {
    animator.SetTrigger("Walk");

    timeout = DateTime.Now.AddSeconds(TimeoutSeconds); //if not reach in time abort
    agent.SetDestination(destination.Place.transform.position); //trace path
  }
}
