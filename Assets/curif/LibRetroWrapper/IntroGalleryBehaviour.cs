using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;

[RequireComponent(typeof(NavMeshAgent))]
public class IntroGalleryBehaviour : MonoBehaviour
{
  public List<GameObject> Destinations;

  [SerializeField]
  private BehaviorTree tree;

  private NavMeshAgent agent;
  private GameObject destination;
  private GameObject Player;

  private System.Random random = new System.Random(DateTime.Now.Second);
  private int waitTime = 2;

  void Start()
  {
    agent = gameObject.GetComponent<NavMeshAgent>();
    Player = GameObject.Find("OVRPlayerControllerGalery");
  }

  private void Awake()
  {
    tree = new BehaviorTreeBuilder(gameObject)
        .Sequence()  //Runs each child node in order and expects a Success status to tick the next node..
            .Condition("Has Destination objects", () => Destinations.Count > 0)
            .Do("Set random destination", () =>
            {
              int index = random.Next(Destinations.Count);
              destination = Destinations[index];
              return TaskStatus.Success;
            })
            .Do("To Random Destination", () =>
            {
              //go far way the player
              if (Vector3.Distance(Player.transform.position, transform.position) <= 1f)
              {
                tree.Reset();
                agent.ResetPath();
                waitTime = 0;
                return TaskStatus.Success;
              }

              if (Vector3.Distance(destination.transform.position, transform.position) <= 1.5f)
              {
                waitTime = random.Next(3*60);
                return TaskStatus.Success;
              }

              LibretroMameCore.WriteConsole($"[IntroGalleryBehaviour] {gameObject.name} to {destination.name}]");
              agent.SetDestination(destination.transform.position);
              return TaskStatus.Continue;
            })
            .WaitTime((float)waitTime)
        .End()
        .Build();
  }

  // Update is called once per frame
  void Update()
  {
    tree.Tick();
  }
}
