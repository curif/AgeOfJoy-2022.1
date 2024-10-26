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
using Debug = UnityEngine.Debug;

using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;
using CleverCrow.Fluid.BTs.Tasks.Actions;
using System.Diagnostics;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(EnableDisableRenderers))]
public class ArcadeRoomBehavior : MonoBehaviour
{
    public List<PlaceInformation> Destinations;

    [Tooltip("Where to go if all destinations are in use")]
    public PlaceInformation DefaultDestination;

    [Tooltip("Room configuration object")]
    public GameObject RoomConfiguration;

    [Tooltip("A static NPC stay in the default position all the time.")]
    public bool IsStatic = false;

    [Tooltip("Movement controlled by Animation or by the Nav Mesh Agent")]
    public bool AnimationControlled = false;

    [Header("Arcade Positions")]
    [Tooltip("Parent group arcade cabinets. Each cabinet needs a PlayerPosition Gameobject as destination")]
    public GameObject PlayerPositions;
    public float MinimalDistanceToReachArcade = 1.5f;

    [Tooltip("Time min/max to spent in a game")]
    public int MaxTimeSpentGaming = 10;
    public int MinTimeSpentGaming = 3;

    [Header("Avoid Player")]
    public bool AvoidPlayer = true;
    [Tooltip("Use raycast analysis plus the NavMesh carv obstacle in the player")]
    public bool UseRayCastToAvoidPlayer = false;
    [Tooltip("Shows when the NPC hits the player")]
    public bool collisionWithPlayer = false;

    public float distanceToDetectPlayer = 5f; // distance to detect objects
    public Vector3 centerRaycastPlayerDetection = new Vector3(0, 1, 0);
    //public float pushForce = 10.0f;

    [Header("Tree")]
    [SerializeField]
    public BehaviorTree tree;

    [Tooltip("Max time to spent walking to a place before abort")]
    public int TimeoutSeconds = 5;

    [Tooltip("Next Destination asigned")]
    public PlaceInformation destination;

    [SerializeField]
    public PlaceInformation Destination { get => destination; }

    private NavMeshAgent agent;
    private PlaceInformation selectedDestination;
    private Animator animator;
    private GameObject player;
    private NavMeshObstacle obstacle;
    private RoomConfiguration roomConfiguration;
    private EnableDisableRenderers enableDisableRenderers;

    private System.Random random = new System.Random(DateTime.Now.Millisecond);
    // private bool onCollisionWithOtherNPC = false;
    private DateTime timeout, timeToSpentInPlace;
    private List<ArcadeRoomBehavior> othersNPC;
    private List<PlaceInformation> totalDestinationsList = new List<PlaceInformation>();
    private string[] animatorTriggers = new String[4] { "Idle", "Buy", "Play", "BoyPlay" }; //@see PlaceInformation Types
    private String[] boyPlayTriggers = new String[3] { "JumpAndPlay", "War", "Fight" };
    // private bool inPathToCollisionWithPlayer = false;
    private DateTime avoidCollisionAnalysis = DateTime.Now;
    private Rigidbody npcRigidbody;
    private Coroutine mainCoroutine;

    private bool initialized = false;
    private bool playerIsPlaying = false;
    private bool isListenerAdded = false;


    void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        animator = gameObject.GetComponent<Animator>();
        player = GameObject.Find("OVRPlayerControllerGalery");
        roomConfiguration = RoomConfiguration.GetComponent<RoomConfiguration>();
        enableDisableRenderers = gameObject.GetComponent<EnableDisableRenderers>();

        if (totalDestinationsList.Count == 0)
        {
            totalDestinationsList.AddRange(Destinations);
            othersNPC = (from npc in GameObject.FindGameObjectsWithTag("NPC")
                         where npc != gameObject
                         select npc.GetComponent<ArcadeRoomBehavior>()).
                                ToList<ArcadeRoomBehavior>();
            if (PlayerPositions != null)
                //The cabinets where not loaded when this code runs
                totalDestinationsList.AddRange(
                    (from Transform playerPosition in PlayerPositions.transform
                     select new PlaceInformation(playerPosition.gameObject, MaxTimeSpentGaming, MinTimeSpentGaming,
                                             MinimalDistanceToReachArcade, PlaceInformation.PlaceType.ArcadeMachine,
                                             playerPosition.gameObject.GetComponent<AgentScenePosition>())
                    ).ToList());
            //get the agent ScenePosition Component for all the places
            foreach (PlaceInformation place in totalDestinationsList)
            {
                if (place.ScenePosition == null)
                    place.ScenePosition = place.Place.GetComponent<AgentScenePosition>();
                if (place.ScenePosition != null)
                    place.MaxAllowedSpace = place.ScenePosition.MaxAllowedSpace;
            }

            DefaultDestination.ScenePosition = DefaultDestination.Place.GetComponent<AgentScenePosition>();
        }
        tree = buildBT();

        configureCollider();
        configureNavMesh();
        forceToDefaultDestination();
        mainCoroutine = StartCoroutine(runBT());
        initialized = true;
    }
    void configureNavMesh()
    {
        /*
         * where root motion is used to drive the NavMeshAgent's movement, the NavMeshAgent's velocity is automatically calculated based on the animation's root motion, and it is not directly controlled by the script.
         * So, while the animator is controlling the movement, the NavMeshAgent's velocity is still being calculated and updated by the NavMesh system based on the object's position, destination, and other factors. The NavMeshAgent will try to move the object to the desired location at a speed that is consistent with the animation's root motion.
         * Therefore, in this case, you don't need to manually control the NavMeshAgent's velocity. Instead, you can use the animator to control the movement and the NavMeshAgent will automatically calculate the appropriate velocity to reach the destination.
        */
        //GetComponent<NavMeshAgent>().speed = 0.1f; //bcz is controlled by the RootMotion
        GetComponent<NavMeshAgent>().radius = 0.3f;
    }

    void forceToDefaultDestination()
    {
        if (DefaultDestination != null)
        {
            GetComponent<NavMeshAgent>().Warp(DefaultDestination.Place.transform.position);
            //transform.position = DefaultDestination.Place.transform.position;

            // Copy rotation
            transform.rotation = DefaultDestination.Place.transform.rotation;

            // Copy forward direction
            transform.forward = DefaultDestination.Place.transform.forward;
        }

    }
    void configureCollider()
    {
        GetComponent<CapsuleCollider>().isTrigger = true;
        GetComponent<CapsuleCollider>().center = new Vector3(0f, 0.7f, 0f);
        GetComponent<CapsuleCollider>().radius = 0.3f;
        GetComponent<CapsuleCollider>().height = 1.7f;

        npcRigidbody = GetComponent<Rigidbody>();
        npcRigidbody.useGravity = false;
        npcRigidbody.isKinematic = true;
        npcRigidbody.mass = 0.5f; //player is 1.0, NPC can't push the player

    }
    /*
     * Detects collision against the Character Controller that acts as collider
     * The character controller is like a capsule collider, the detection needs to 
     * be at center or higher than the floor level, the property cetnerRaycastPlayerDetection
     * is used to move the center of the Raycast.
     * 
    bool detectPlayer()
    {
        float angleIncrement = 20f; // angle increment between rays
        float startAngle = -45f; // starting angle of the rays
        float endAngle = 45f; // ending angle of the rays

        if (!UseRayCastToAvoidPlayer || !AvoidPlayer)
            return false;

        inPathToCollisionWithPlayer = false;

        for (float angle = startAngle; angle <= endAngle; angle += angleIncrement)
        {
            // calculate direction of the ray based on angle
            Vector3 direction = Quaternion.AngleAxis(angle, transform.up) * transform.forward;
            Vector3 center = transform.position + centerRaycastPlayerDetection;
            RaycastHit hit;
            if (Physics.Raycast(center, direction, out hit, distanceToDetectPlayer))
            {
                if (hit.collider.gameObject == player)
                {
                    //ConfigManager.WriteConsole($"[ArcadeRoomBehavior.DetectPlayerInPath] {gameObject} Player in collision path!");
                    // The ray hit the player, so do something (e.g. change direction)
                    Debug.DrawRay(center, direction * distanceToDetectPlayer, Color.red, 0);
                    inPathToCollisionWithPlayer = true;
                    return true;
                }
                else
                {
                    Debug.DrawRay(center, direction * distanceToDetectPlayer, Color.yellow, 0);
                }
            }
        }
        return false;

    }
    */

    void addListener()
    {
        if (isListenerAdded) return;
        LibretroMameCore.OnPlayerStartPlaying?.AddListener(OnPlayerStartPlaying);
        LibretroMameCore.OnPlayerStopPlaying?.AddListener(OnPlayerStopPlaying);
        isListenerAdded = true;
    }
    void removeListener()
    {
        if (!isListenerAdded) return;
        LibretroMameCore.OnPlayerStartPlaying?.RemoveListener(OnPlayerStartPlaying);
        LibretroMameCore.OnPlayerStopPlaying?.RemoveListener(OnPlayerStopPlaying);
        isListenerAdded = false;
    }
    void OnEnable()
    {
        addListener();
        if (!initialized)
            return;

        if (mainCoroutine == null)
            mainCoroutine = StartCoroutine(runBT());

        ConfigManager.WriteConsole($"[ArcadeRoomBehavior.OnEnabled] {gameObject.name} is enabled");
    }

    private void OnApplicationPause()
    {
        if (mainCoroutine != null)
        {
            StopCoroutine(mainCoroutine);
            mainCoroutine = null;
        }
        removeListener();
    }

    private void OnDisable()
    {
        removeListener();

        if (!initialized)
            return;
        
        if (mainCoroutine != null)
        {
            StopCoroutine(mainCoroutine);
            mainCoroutine = null;
        }
    }

    IEnumerator runBT()
    {
        while (true)
        {
            if (!AnimationControlled)
                animator.SetFloat("Speed", agent.velocity.magnitude);

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
                if (destination != null && ReferenceEquals(selectedDestination.Place, destination.Place))
                {
                    // ConfigManager.WriteConsole($"[ArcadeRoomBehavior.BehaviorTreeBuilder] {gameObject.name} selected destination is the actual destination, repeat");
                    return TaskStatus.Failure;
                }

                destination = null;
                return TaskStatus.Success;
            })
            .ReturnSuccess()
              .Sequence("Back to the default?")
                .Condition("NPC should go to default destination?", () =>
                    DefaultDestination != null
                    && (IsStatic || playerIsPlaying ||
                        selectedDestination.IsTaken ||
                        selectedDestination.MaxAllowedSpace != "1x1x2" ||  //only cabinets size 1x1x2 (no animation for others yet)
                        othersNPC.FirstOrDefault(npc =>
                                                  npc?.Destination != null &&
                                                  ReferenceEquals(npc.Destination.Place,                                                                     selectedDestination.Place)) != null))
                .Do("Use the default destination", () =>
                {
                    selectedDestination = DefaultDestination;
                    // ConfigManager.WriteConsole($"[ArcadeRoomBehavior.BehaviorTreeBuilder]{gameObject.name} falls into the default destination: {DefaultDestination.Place.name}");
                    return TaskStatus.Success;
                })
              .End()
            .End()

            .Do("Start walking", () =>
            {
                destination = selectedDestination; //others NPCs can see this
                if (!walkToDestination())
                {
                    stop();
                    destination = null;
                    return TaskStatus.Failure;
                }
                return TaskStatus.Success;
            })

            .RepeatUntilSuccess("Wait for agent path")
              .Condition("Agent path is ready?", () => !agent.pathPending)
            .End()

            .RepeatUntilSuccess()
              .Selector("Walking...")
                .Condition("Timeout", () => DateTime.Now > timeout)
                .Condition("Arrived", () => destination.ScenePosition.NPCIsPresent(name))
                .Condition("Player found or blocked", () => collisionWithPlayer /*|| detectPlayer()*/)
                .Condition("Destination taken by other NPC or Player?", () => destination.ScenePosition.IsTaken)
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
                else if (AvoidPlayer && (collisionWithPlayer /*|| detectPlayer()*/))
                {
                    rotateAndWalk();
                    avoidCollisionAnalysis = DateTime.Now.AddSeconds(2); //give some time to the NPC to reach the destination before check for collisions again 
                    collisionWithPlayer = false;
                    timeToSpentInPlace = DateTime.Now.AddSeconds(2);
                }
                else if (collisionWithPlayer)
                {
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
                .Selector("Spending time...")
                  .Condition("Wait some time there (or hit player)", () => DateTime.Now > timeToSpentInPlace)
                  .Condition("hit player", () => collisionWithPlayer)
                //.Condition("Other NPC got the position", () => !destination.ScenePosition.ItsMe(name))
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

    private void SetMovementController()
    {
        animator.applyRootMotion = AnimationControlled;
        GetComponent<NavMeshAgent>().speed = 1f; //bcz is controlled by the RootMotion
    }

    private void stop()
    {
        //ConfigManager.WriteConsole($"[ArcadeRoomBehavior.stop]  {gameObject.name} ");
        //Debug.Log($"[ArcadeRoomBehavior.stop]  {gameObject.name} ");
        animator.SetBool("Idle", true);
        animator.SetBool("IsWalking", false); // Ensure everything else is disabled
        animator.SetBool("Buy", false);
        animator.SetBool("Play", false);
        //animator.SetTrigger("Idle");
        agent.updateRotation = false;
        SetMovementController();
    }
    private void walk()
    {
        //ConfigManager.WriteConsole($"[ArcadeRoomBehavior.stop]  {gameObject.name} ");
        //Debug.Log($"[ArcadeRoomBehavior.stop]  {gameObject.name} ");
        animator.SetBool("IsWalking", true);
        animator.SetBool("Idle", false); // Ensure everything else is disabled
        animator.SetBool("Buy", false);
        animator.SetBool("Play", false);
        agent.updateRotation = true;
        // animator.SetTrigger("Walk");
        SetMovementController();
    }

    private void faceToDestination()
    {
        agent.updateRotation = false;
        transform.position = new Vector3(destination.Place.transform.position.x,
                                        transform.position.y,
                                        destination.Place.transform.position.z);
        transform.rotation = destination.Place.transform.rotation;
    }
    private void runDestinationAnimation()
    {

        faceToDestination();
        try
        {
            if (destination.Type == PlaceInformation.PlaceType.BoyPlay)
            {
                //special behaviour -- Geometrizer: I don't think this does anything
                int index = UnityEngine.Random.Range(0, boyPlayTriggers.Length - 1);
                animator.SetTrigger(boyPlayTriggers[index]);
                animator.SetBool(boyPlayTriggers[index], true);
                Debug.Log($"[runDestinationAnimation] on {name}");
            }
            else
                //animator.SetTrigger(animatorTriggers[(int)destination.Type]);
                animator.SetBool("IsWalking", false);
                animator.SetBool("Idle", false);
                animator.SetBool(animatorTriggers[(int)destination.Type], true);

            // geometrizer: Set the "Random" parameter to a random float value between 0 and 1
            animator.SetFloat("Random", UnityEngine.Random.Range(0f, 1f));

            // geometrizer: Output the current value of the "Random" parameter to the console
            //UnityEngine.Debug.Log("Random value: " + animator.GetFloat("Random"));

            //Debug.Log($"[runDestinationAnimation] on {name}");
            //    Debug.Log($"Trigger set: {animatorTriggers[(int)destination.Type]}, Destination Type: {destination.Type}");
                //geometrizer: This is where we start Buy/Play anims, need to shut down walk here
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[runDestinationAnimation] on {name}", e);
        }
    }

    private bool walkToDestination()
    {
        timeout = DateTime.Now.AddSeconds(TimeoutSeconds); //if not reach in time abort
        if (!agent.SetDestination(destination.Place.transform.position))
        {
            //ConfigManager.WriteConsole($"[ArcadeRoomBehavior.walkToDestination] ERROR {gameObject.name} to {destination.Place.name} not possible");
            return false;
        }
        walk();

        //ConfigManager.WriteConsole($"[ArcadeRoomBehavior.walkToDestination] {gameObject.name} to {destination.Place.name} timeout {TimeoutSeconds} secs {timeout.ToString()}");

        return true;
    }

    private void rotateAndWalk()
    {
        //ConfigManager.WriteConsole($"[ArcadeRoomBehavior.rotateAndWalk] {gameObject.name} ");
        // TODO change for a new path to avoid the player.
        animator.applyRootMotion = true;
        animator.SetTrigger("Turn");
        animator.SetBool("IsTurning", true);
    }

    private void OnTriggerEnter(Collider collision)
    {
        //ConfigManager.WriteConsole($"[OnTriggerEnter] agentBehavior {name}: collision with {collision.gameObject.name}");
        if ((collision.gameObject.name == "OVRPlayerControllerGalery" || collision.gameObject.name == "GrabVolumeSmall" || collision.gameObject.name == "GrabVolumeBig")
            && DateTime.Now > avoidCollisionAnalysis)
        {
            //ConfigManager.WriteConsole($"[OnTriggerEnter] agentBehavior collision with player{name}: {collision.gameObject.name}");
            collisionWithPlayer = true;
        }
    }
    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.name == "OVRPlayerControllerGalery" || collision.gameObject.name == "GrabVolumeSmall" || collision.gameObject.name == "GrabVolumeBig")
        {
            //ConfigManager.WriteConsole($"[OnTriggerExit] agentBehavior collision with player no more {name}: {collision.gameObject.name}");
            collisionWithPlayer = false;
        }
    }


    private void OnPlayerStartPlaying()
    {
        playerIsPlaying = true;
    }
    private void OnPlayerStopPlaying()
    {
        playerIsPlaying = false;
    }



    /*
      private void walkLeftOrRightOfPlayer()
      {
        // Setting linear velocity of a kinematic body is not supported.
        //npcRigidbody.velocity = Vector3.zero;
        //npcRigidbody.angularVelocity = Vector3.zero;
        //Vector3 pushDirection = (transform.position - player.transform.position).normalized;
        //npcRigidbody.AddForce(pushDirection * pushForce, ForceMode.Impulse);
        //move left or right
        //
        Transform playerTransform = player.GetComponent<Transform>();

        Vector3 dest = playerTransform.position;
        dest.x += UnityEngine.Random.Range(-3f, 3f);
        dest.z += -3f;
        ConfigManager.WriteConsole($"[walkLeftOrRightOfPlayer] {name} range: {dest.x}");
        agent.SetDestination(dest);

        walk();
        avoidCollisionAnalysis = DateTime.Now.AddSeconds(2); //give some time to the NPC to reach the destination before check for collisions again 
        collisionWithPlayer = false;
      }
      private void OnTriggerStay(Collider collision)
      {
        //ConfigManager.WriteConsole($"[OnTriggerEnter] {collision.gameObject.name} is {player.name}?");
        if ((collision.gameObject.name == "OVRPlayerControllerGalery" || collision.gameObject.name == "GrabVolumeSmall")
            && DateTime.Now > avoidCollisionAnalysis)
        {
          ConfigManager.WriteConsole($"[OnTriggerStay] {name}: {collision.gameObject.name}");
          collisionWithPlayer = true;
        }
      }
      */
}
