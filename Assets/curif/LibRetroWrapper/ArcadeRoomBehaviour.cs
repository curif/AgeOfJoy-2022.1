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
  private string[] animatorTriggers = new String[4] { "Idle", "Buy", "Play", "BoyPlay" }; //@see PlaceInformation Types
  private String[] boyPlayTriggers = new String[3] { "JumpAndPlay", "War", "Fight" };

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

    if (PlayerPositions != null)
        //The cabinets where not loaded when this code runs
        totalDestinationsList.AddRange(
            (from Transform playerPosition in PlayerPositions.transform
            select new PlaceInformation(playerPosition.gameObject, MaxTimeSpentGaming, MinTimeSpentGaming,
                                        MinimalDistanceToReachArcade, PlaceInformation.PlaceType.ArcadeMachine)
            ).ToList());

    Debug.Log($"[ArcadeRoomBehaviour] {gameObject.name}  destinations totalDestinationsList: {totalDestinationsList.Count}");

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
        .Do("Stop", () =>
        {
          stop();
          return TaskStatus.Success;
        })
        .Do("Do something there", () =>
        {
          if (DateTime.Now > timeout)
          {
            idle();
            timeToSpentInPlace = DateTime.Now.AddSeconds(1);
          }
          else
          {
            runDestinationAnimation();
            timeToSpentInPlace = destination.getWaitingDateTime();
          }

          return TaskStatus.Success;
        })

        .RepeatUntilSuccess()
            .Selector()
                .Condition("Wait some time there", () => DateTime.Now > timeToSpentInPlace)
                .Condition("Player is near", () => AvoidPlayer && Vector3.Distance(player.transform.position, transform.position) <= 1f)
            .End()
        .End()

        .Do("Clean", () =>
        {
          destination = null;
          return TaskStatus.Success;
        })
      .End()
      .Build();

  }

  private void stop()
  {
    agent.Stop();
    agent.ResetPath();
  }

  private void idle()
  {
    animator.SetTrigger("Idle");
  }

  private void runDestinationAnimation()
  {
    transform.position = new Vector3(destination.Place.transform.position.x, 
                                     transform.position.y, 
                                     destination.Place.transform.position.z);
    transform.rotation = destination.Place.transform.rotation;

    if (destination.Type == PlaceInformation.PlaceType.BoyPlay) {
        //special behaviour
        int index = UnityEngine.Random.Range(0, boyPlayTriggers.Length - 1);
        animator.SetTrigger(boyPlayTriggers[index]);
    }
    else
        animator.SetTrigger(animatorTriggers[(int)destination.Type]);

  }

  private void walkToDestination()
  {
    animator.SetTrigger("Walk");
    timeout = DateTime.Now.AddSeconds(TimeoutSeconds); //if not reach in time abort
    agent.SetDestination(destination.Place.transform.position);
  }

}
