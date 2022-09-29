using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;
using CleverCrow.Fluid.BTs.TaskParents.Composites;
using Eflatun.SceneReference;

// https://github.com/ashblue/fluid-behavior-tree
// https://github.com/starikcetin/Eflatun.SceneReference


public class ExecuteAllSequence : CompositeBase
{
  protected override TaskStatus OnUpdate()
  {
    for (var i = ChildIndex; i < Children.Count; i++)
    {
      var child = Children[ChildIndex];

      child.Update();

      ChildIndex++;
    }

    return TaskStatus.Success;
  }
}

public static class BehaviorTreeBuilderExtensions
{
  public static BehaviorTreeBuilder ExecuteAllSequence(this BehaviorTreeBuilder builder, string name = "My Sequence")
  {
    return builder.ParentTask<ExecuteAllSequence>(name);
  }
}

[Serializable]
public class SceneGate
{
  public SceneReference SceneRef;
  [Tooltip("Objects that block the gate")]
  public GameObject[] Blockers;
}

public class GateController : MonoBehaviour
{

  [Tooltip("The minimal distance between the player and the gate to load/unload scenes.")]
  [SerializeField]
  public float MinimalDistance = 0.9f;

  [Header("Scene Load settings")]
  [Tooltip("Names of the scenes to load.")]
  [SerializeField]
  public SceneReference[] ScenesToLoad;

  [Tooltip("The GameObject parent of the floor child objects in the scene where the gate is")]
  [SerializeField]
  public GameObject Floor;

  [Header("Scene Unload settings")]
  [SerializeField]
  public SceneReference[] ScenesToUnload;


  [Header("Scene Blockers")]
  [SerializeField]
  public SceneGate[] SceneBlockers;

  [SerializeField]
  public BehaviorTree treeLoad, treeUnLoad, treeBlockers;

  private GameObject centerCamera;
  private Bounds floorBounds;

  int loadSceneIdx = 0, unloadSceneIdx = 0, blockerIdx = 0;
  SceneReference controledSceneToLoad, controledSceneToUnLoad, controledSceneBlocker;

  // Start is called before the first frame update
  void Start()
  {
    centerCamera = GameObject.Find("CenterEyeAnchor");
    //ConfigManager.WriteConsole($"[GateController] -Starting- Scenes to control, load: {SceneToLoadNames} unload: {SceneToUnloadNames}");
    if (Floor != null)
    {
      floorBounds = new Bounds(Floor.transform.position, Vector3.zero);
      foreach (Transform floor in Floor.transform)
      {
        floorBounds.Encapsulate(new Bounds(floor.position, new Vector3(floor.localScale.x, 100f, floor.localScale.z)));
      }
    }
    // StartCoroutine(Evaluate());
    StartCoroutine(runBT());
  }

  IEnumerator runBT()
  {
    treeLoad = buildLoadScenesBT();
    treeUnLoad = buildUnLoadScenesBT();
    treeBlockers = buildBlockersBT();

    if (ScenesToLoad.Length > 0)
      controledSceneToLoad = ScenesToLoad[loadSceneIdx];

    if (ScenesToUnload.Length > 0)
      controledSceneToUnLoad = ScenesToUnload[unloadSceneIdx];
    
    if (SceneBlockers.Length > 0)
      controledSceneToUnLoad = SceneBlockers[unloadSceneIdx].SceneRef;

    while (true)
    {
      if (ScenesToLoad.Length > 0)
        treeLoad.Tick();

      if (ScenesToUnload.Length > 0)
        treeUnLoad.Tick();

      if (SceneBlockers.Length > 0)
        treeBlockers.Tick();

      yield return new WaitForSeconds(1f / 3f);
    }
  }

  bool isPlayerCloseToController()
  {
    float d = Vector3.Distance(centerCamera.transform.position, gameObject.transform.position);
    // WriteConsole($"[curif.LibRetroMameCore.isPlayerClose] distance: {d} < {_distanceMinToPlayerToStartGame} {d < _distanceMinToPlayerToStartGame}");
    return d < MinimalDistance;
  }

  void LockGate(SceneGate scn, bool block)
  {
    foreach (GameObject blocker in scn.Blockers)
    {
      if (blocker != null)
        blocker.SetActive(block);
    }
  }

  bool PlayerIsInTheRoom()
  {
    //https://docs.unity3d.com/ScriptReference/Bounds.html
    if (floorBounds == null)
      return false;
    return floorBounds.Contains(centerCamera.transform.position);
  }

  private BehaviorTree buildUnLoadScenesBT()
  {
    return new BehaviorTreeBuilder(gameObject)
    .Sequence()
      .Condition("Is the player close to the controller?", () => isPlayerCloseToController())
      .Condition("Player is in the room", () => PlayerIsInTheRoom())

      .ExecuteAllSequence()
        .Sequence()
          .Condition("Exists the scene?", () => controledSceneToUnLoad != null)
          .Condition("Is scene safe?", () => controledSceneToUnLoad.IsSafeToUse)
          .Condition("Is scene loaded?", () => SceneManager.GetSceneByName(controledSceneToUnLoad.Name).isLoaded)
          .Do("Unload Scene", () =>
          {
            ConfigManager.WriteConsole($"[GateController] UNLOAD SCENE: {controledSceneToUnLoad.Name} ******.");
            SceneManager.UnloadSceneAsync(controledSceneToUnLoad.Name);
            return TaskStatus.Success;
          })
        .End()
        .Do("Next Scene", () =>
        {
          unloadSceneIdx++;
          if (unloadSceneIdx >= ScenesToUnload.Length)
            unloadSceneIdx = 0;
          controledSceneToUnLoad = ScenesToUnload[unloadSceneIdx];
          return TaskStatus.Success;
        })
    .End()
    .Build();
  }

  private BehaviorTree buildLoadScenesBT()
  {
    return new BehaviorTreeBuilder(gameObject)
      .Sequence()
        .Condition("Player is in the room", () => PlayerIsInTheRoom())
        .Condition("Is player close to the controller", () => isPlayerCloseToController())
        .ExecuteAllSequence()
          .Sequence("load the scene if not loaded")
            .Condition("registered?", () => controledSceneToLoad != null)
            .Condition("Safe?", () => controledSceneToLoad.IsSafeToUse)
            .Condition("Is scene not loaded?", () => !SceneManager.GetSceneByName(controledSceneToLoad.Name).isLoaded)
            .Do("Load the scene (async)", () =>
            {
              SceneManager.LoadSceneAsync(controledSceneToLoad.Name, LoadSceneMode.Additive);
              ConfigManager.WriteConsole($"[GateController] LOAD Scene: {controledSceneToLoad.Name}");
              return TaskStatus.Success;
            })
            .RepeatUntilSuccess("Repeat until the scene is loaded")
              .Condition("Scene is safe?", () => controledSceneToLoad.IsSafeToUse)
              .Condition("Is scene loaded?", () => SceneManager.GetSceneByName(controledSceneToLoad.Name).isLoaded)
            .End()
          .End()
          .Do("Next Scene to evaluate", () =>
          {
            loadSceneIdx++;
            if (loadSceneIdx >= ScenesToLoad.Length)
              loadSceneIdx = 0;
            controledSceneToLoad = ScenesToLoad[loadSceneIdx];
            return TaskStatus.Success;
          })
        .End()
      .End()
      .Build();
  }


  private BehaviorTree buildBlockersBT()
  {
    return new BehaviorTreeBuilder(gameObject)
      .ExecuteAllSequence()
        .Sequence("Control the gate")
          .Condition("Scene registered?", () => controledSceneBlocker != null)
          .Do("lock/unlock the gate", () =>
          {
            LockGate(SceneBlockers[blockerIdx], !SceneManager.GetSceneByName(controledSceneBlocker.Name).isLoaded);
            return TaskStatus.Success;
          })
        .End()
        .Do("Next Scene to evaluate", () =>
        {
          blockerIdx++;
          if (blockerIdx >= SceneBlockers.Length)
            blockerIdx = 0;
          controledSceneBlocker = SceneBlockers[blockerIdx].SceneRef;
          return TaskStatus.Success;
        })
        .End()
      .End()
      .Build();
  }
}
