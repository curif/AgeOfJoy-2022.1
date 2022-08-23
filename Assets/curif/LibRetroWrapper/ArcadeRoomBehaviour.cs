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
public class ArcadeRoomBehaviour : MonoBehaviour
{
  public List<PlaceInformation> Destinations;
  [Tooltip("Parent group arcade cabinets. Each cabinet needs a PlayerPosition Gameobject as destination")]
  public GameObject PlayerPositions;
  public float MinimalDistanceToReachArcade = 1.5f;

  [Tooltip("Where to go if all destinations are in use")]
  public PlaceInformation DefaultDestination;

  public bool AvoidPlayer;

  [Tooltip("Time max to spent in a game")]
  public int MaxTimeSpentGaming = 10;
  public int MinTimeSpentGaming = 3;

  [SerializeField]
  public BehaviorTree tree;

  [Tooltip("Max time to spent walking to a place before abort")]
  public int TimeoutSeconds = 5;

  private NavMeshAgent agent;
  private PlaceInformation destination, selectedDestination;
  private Animator animator;
  private GameObject player;

  private System.Random random = new System.Random(DateTime.Now.Millisecond);
  // private bool onCollisionWithOtherNPC = false;
  private DateTime timeout, timeToSpentInPlace;
  private List<ArcadeRoomBehaviour> othersNPC;
  private List<PlaceInformation> totalDestinationsList = new List<PlaceInformation>();

  public PlaceInformation Destination { get => destination; }

  void Start()
  {
    agent = gameObject.GetComponent<NavMeshAgent>();
    animator = gameObject.GetComponent<Animator>();
    player = GameObject.Find("OVRPlayerControllerGalery");

    totalDestinationsList.AddRange(Destinations);
    Debug.Log($"[ArcadeRoomBehaviour] {gameObject.name} added configured destinations totalDestinationsList: {totalDestinationsList.Count}");

    StartCoroutine(runBT());
  }

  IEnumerator runBT()
  {
    othersNPC = (from npc in GameObject.FindGameObjectsWithTag("NPC")
                 where npc != gameObject
                 select npc.GetComponent<ArcadeRoomBehaviour>()).
                 ToList<ArcadeRoomBehaviour>();
    Debug.Log($"{gameObject.name} found {othersNPC.Count} NPCs active");


    //at that time the cabinets where not loaded.
    totalDestinationsList.AddRange(
        (from Transform playerPosition in PlayerPositions.transform
        select new PlaceInformation(playerPosition.gameObject, MaxTimeSpentGaming, MinTimeSpentGaming, MinimalDistanceToReachArcade)
        ).ToList());
    Debug.Log($"[ArcadeRoomBehaviour] {gameObject.name} added cabinets destinations with PlayerPosition, totalDestinationsList: {totalDestinationsList.Count}");

    tree = buildBT();
    while (true)
    {
      tree.Tick();
      yield return new WaitForSeconds(1f / 3f);
    }
  }

  private BehaviorTree buildBT()
  {

    return new BehaviorTreeBuilder(gameObject)
      .Sequence()
        .Do("Set random destination", () =>
        {
          int index = random.Next(totalDestinationsList.Count);
          selectedDestination = totalDestinationsList[index];
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
            .Condition("Player found", () => AvoidPlayer && Vector3.Distance(player.transform.position, transform.position) <= 1f)
          .End()
        .End()
        .Do("Stop walking", () =>
        {
          Idle();
          timeToSpentInPlace = DateTime.Now > timeout ? DateTime.Now.AddSeconds(1) : destination.getDateTimeUntilWait();
          FaceTarget(destination.Place.transform.position);

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
  //   private Vector3 positionDestination()
  //   {
  //     Transform playerPosition = destination.Place.transform.Find("PlayerPosition");
  //     Debug.Log($"[ArcadeRoomBehaviour] destination PlayerPosition: {playerPosition?.ToString()}");

  //     //trace path
  //     if (playerPosition != null)
  //       return playerPosition.position;

  //     return destination.Place.transform.position;
  //   }
  private void walkToDestination()
  {
    animator.SetTrigger("Walk");

    timeout = DateTime.Now.AddSeconds(TimeoutSeconds); //if not reach in time abort
    agent.SetDestination(destination.Place.transform.position);

  }
  private void FaceTarget(Vector3 positionToLook)
  {
    Vector3 lookPos = positionToLook - transform.position;
    lookPos.y = 0;
    Quaternion rotation = Quaternion.LookRotation(lookPos);
    transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1);
  }

}
