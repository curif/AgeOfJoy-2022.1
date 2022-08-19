using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;


[Serializable]
public class PlaceInformation
{
  public GameObject Place;
  public int MaxTimeSpentThere = 120;
  public int MinTimeSpentThere = 1;
  public float MinimalDistanceToReachObject = 1.5f;

  private System.Random random = new System.Random(DateTime.Now.Millisecond);

  public DateTime getDateTimeUntilWait()
  {
    return DateTime.Now.AddSeconds(random.Next(MinTimeSpentThere, MaxTimeSpentThere));
  }
}

[RequireComponent(typeof(NavMeshAgent))]
// [RequireComponent(typeof(Rigidbody))]
public class IntroGalleryBehaviour : MonoBehaviour
{
  public List<PlaceInformation> Destinations;

  [Tooltip("Where to go if all destinations are in use")]
  public PlaceInformation DefaultDestination;

  [SerializeField]
  public BehaviorTree tree;

  [Tooltip("Max time to spent walking to a place before abort")]
  public int TimeoutSeconds = 5;

  private NavMeshAgent agent;
  private PlaceInformation destination, selectedDestination;
  private GameObject Player;
  private Animator animator;

  private System.Random random = new System.Random(DateTime.Now.Millisecond);
  // private bool onCollisionWithOtherNPC = false;
  private DateTime timeout, timeToSpentInPlace;
  private List<IntroGalleryBehaviour> othersNPC;

  public PlaceInformation Destination { get => destination; }

  void Start()
  {
    agent = gameObject.GetComponent<NavMeshAgent>();
    animator = gameObject.GetComponent<Animator>();

    Player = GameObject.Find("OVRPlayerControllerGalery");

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
    return new BehaviorTreeBuilder(gameObject)
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
                                          npc.Destination != null &&
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
          animator.ResetTrigger("Idle");
          animator.SetTrigger("Walk");
          timeout = DateTime.Now.AddSeconds(TimeoutSeconds); //if not reach in time abort
          agent.SetDestination(destination.Place.transform.position); //trace path
          Debug.Log($"[IntroGalleryBehaviour] {gameObject.name} to {destination.Place.name} timeout {TimeoutSeconds}secs {timeout.ToString()}");
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
          agent.ResetPath();
          animator.ResetTrigger("Walk");
          animator.SetTrigger("Idle");
          timeToSpentInPlace = DateTime.Now > timeout ? DateTime.Now.AddSeconds(1) : destination.getDateTimeUntilWait();
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

  // //https://stackoverflow.com/questions/15039185/collision-detection-between-two-character-controllers
  // void OnControllerColliderHit(ControllerColliderHit col)
  // {
  //   Debug.Log($"[OnControllerColliderHit] {name} hit to {col.gameObject.name}");
  //   if (col.gameObject.tag == "NPC")
  //   {
  //     onCollisionWithOtherNPC = true;
  //     Debug.Log($"[OnControllerColliderHit] {name} Collision with NPC {col.gameObject.name}");
  //   }
  // }
}
