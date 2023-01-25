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
public class ArcadeRoomBehavior : MonoBehaviour
{
  public List<PlaceInformation> Destinations;

  [Tooltip("Where to go if all destinations are in use")]
  public PlaceInformation DefaultDestination;

  [Header("Arcade Positions")]
  [Tooltip("Parent group arcade cabinets. Each cabinet needs a PlayerPosition Gameobject as destination")]
  public GameObject PlayerPositions;
  public float MinimalDistanceToReachArcade = 1.5f;
  [Tooltip("Time max to spent in a game")]
  public int MaxTimeSpentGaming = 10;
  public int MinTimeSpentGaming = 3;

  [Header("Avoid Player")]
  public bool AvoidPlayer;
  public float distanceToDetectPlayer = 5f; // distance to detect objects
  public Vector3 centerRaycastPlayerDetection = new Vector3(0,1,0);

  [SerializeField]
  public BehaviorTree tree;

  [Tooltip("Max time to spent walking to a place before abort")]
  public int TimeoutSeconds = 5;

  public PlaceInformation Destination { get => destination; }

  private NavMeshAgent agent;
  private PlaceInformation destination, selectedDestination;
  private Animator animator;
  private GameObject player;

  private System.Random random = new System.Random(DateTime.Now.Millisecond);
  // private bool onCollisionWithOtherNPC = false;
  private DateTime timeout, timeToSpentInPlace;
  private List<ArcadeRoomBehavior> othersNPC;
  private List<PlaceInformation> totalDestinationsList = new List<PlaceInformation>();
  private string[] animatorTriggers = new String[4] { "Idle", "Buy", "Play", "BoyPlay" }; //@see PlaceInformation Types
  private String[] boyPlayTriggers = new String[3] { "JumpAndPlay", "War", "Fight" };
  private bool collisionWithPlayer = false;


  void Start()
  {
    agent = gameObject.GetComponent<NavMeshAgent>();
    animator = gameObject.GetComponent<Animator>();
    player = GameObject.Find("OVRPlayerControllerGalery");

    totalDestinationsList.AddRange(Destinations);
    // ConfigManager.WriteConsole($"[ArcadeRoomBehavior] {gameObject.name} added configured destinations totalDestinationsList: {totalDestinationsList.Count}");

    StartCoroutine(runBT());
  }
  /*
   * Detects collision against the Character Controller that acts as collider
   * The character controller is like a capsule collider, the detection needs to 
   * be at center or higher than the floor level, the property cetnerRaycastPlayerDetection
   * is used to move the center of the Raycast.
   * */
  bool detectPlayer() {
    float angleIncrement = 20f; // angle increment between rays
    float startAngle = -45f; // starting angle of the rays
    float endAngle = 45f; // ending angle of the rays
    
    collisionWithPlayer = false; 
    
    for (float angle = startAngle; angle <= endAngle; angle += angleIncrement)
    {
	// calculate direction of the ray based on angle
        Vector3 direction = Quaternion.AngleAxis(angle, transform.up) * transform.forward;
        Vector3 center =  transform.position + centerRaycastPlayerDetection;
	RaycastHit hit;
        if (Physics.Raycast(center, direction, out hit, distanceToDetectPlayer))
        {
	    if (hit.collider.gameObject == player)
            {
                //ConfigManager.WriteConsole($"[ArcadeRoomBehavior.DetectPlayerInPath] {gameObject} Player in collision path!");
		Debug.DrawRay(center, direction * distanceToDetectPlayer, Color.red, 0);
		collisionWithPlayer = true; 
                return true;
                // The ray hit the player, so do something (e.g. change direction)
            }
	    else {
		Debug.DrawRay(center, direction * distanceToDetectPlayer, Color.yellow, 0);
	    }
        }
    
    }
    return false;
  
  }

  IEnumerator runBT()
  {
    othersNPC = (from npc in GameObject.FindGameObjectsWithTag("NPC")
                 where npc != gameObject
                 select npc.GetComponent<ArcadeRoomBehavior>()).
                 ToList<ArcadeRoomBehavior>();

    if (PlayerPositions != null)
        //The cabinets where not loaded when this code runs
        totalDestinationsList.AddRange(
            (from Transform playerPosition in PlayerPositions.transform
            select new PlaceInformation(playerPosition.gameObject, MaxTimeSpentGaming, MinTimeSpentGaming,
                                        MinimalDistanceToReachArcade, PlaceInformation.PlaceType.ArcadeMachine)
            ).ToList());

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
          if (destination != null && UnityEngine.Object.ReferenceEquals(selectedDestination.Place, destination.Place))
          {
            ConfigManager.WriteConsole($"[ArcadeRoomBehavior.BehaviorTreeBuilder] {gameObject.name} selected destination is the actual destination, repeat");
            return TaskStatus.Failure;
          }
          destination = null;
          return TaskStatus.Success;
        })
        .ReturnSuccess()
          .Sequence()
            .Condition("NPC has a default destination configured?", () => DefaultDestination != null)
            .Condition("Destination taken by other NPC?", () =>
                othersNPC.FirstOrDefault(npc =>
                                          npc?.Destination != null &&
                                          UnityEngine.Object.ReferenceEquals(npc.Destination.Place, selectedDestination.Place)) != null)
            .Do("Use the default destination", () =>
            {
              selectedDestination = DefaultDestination;
              ConfigManager.WriteConsole($"[ArcadeRoomBehavior.BehaviorTreeBuilder]{gameObject.name} falls into the default destination: {DefaultDestination.Place.name}");
              return TaskStatus.Success;
            })
          .End()
        .End()

        .Do("Start walking", () =>
        {
          destination = selectedDestination; //others NPCs can see this
          if (! walkToDestination())
          {
            stop();
            destination = null;
            return TaskStatus.Failure;
          }
          return TaskStatus.Success;
        })

        .RepeatUntilSuccess()
          .Condition("Agent path is ready?", () => !agent.pathPending) 
        .End()

        .RepeatUntilSuccess()
          .Selector()
            .Condition("Timeout", () => DateTime.Now > timeout)
            .Condition("Arrived", () => Vector3.Distance(destination.Place.transform.position, transform.position) <= destination.MinimalDistanceToReachObject)
            .Condition("Player found or blocked", () => AvoidPlayer && detectPlayer())
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
            timeToSpentInPlace = DateTime.Now.AddSeconds(1);
          }
          else if (collisionWithPlayer)
	  {
	    rotateAndWalk();
	    timeToSpentInPlace = DateTime.Now.AddSeconds(1.5);
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
            .End()
        .End()

        .Do("Clean", () =>
        {
          destination = null;
          stop();
          return TaskStatus.Success;
        })
      .End()
      .Build();

  }

  private void stop()
  {
    //ConfigManager.WriteConsole($"[ArcadeRoomBehavior.stop]  {gameObject.name} ");
    agent.isStopped = true;
    agent.ResetPath();
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

  private bool walkToDestination()
  {
    timeout = DateTime.Now.AddSeconds(TimeoutSeconds); //if not reach in time abort
    if (!agent.SetDestination(destination.Place.transform.position))
    {
      //ConfigManager.WriteConsole($"[ArcadeRoomBehavior.walkToDestination] ERROR {gameObject.name} to {destination.Place.name} not possible");
      return false;
    }

    animator.SetTrigger("Walk");
    agent.isStopped = false;

    ConfigManager.WriteConsole($"[ArcadeRoomBehavior.walkToDestination] {gameObject.name} to {destination.Place.name} timeout {TimeoutSeconds} secs {timeout.ToString()}");
    
    return true;
  }

  private void rotateAndWalk()
  {
    //ConfigManager.WriteConsole($"[ArcadeRoomBehavior.rotateAndWalk] {gameObject.name} ");
    animator.SetTrigger("Turn");
  }
}
