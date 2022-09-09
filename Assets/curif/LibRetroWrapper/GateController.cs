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
  public SceneGate[] ScenesToLoad;

  [Tooltip("The GameObject parent of the floor child objects in the scene where the gate is")]
  [SerializeField]
  public GameObject Floor;


  [Header("Scene Unload settings")]
  [SerializeField]
  public SceneGate[] ScenesToUnload;

  [SerializeField]
  public BehaviorTree treeLoad, treeUnLoad;

  private GameObject centerCamera;
  private Bounds floorBounds;

  int loadSceneIdx = 0, unloadSceneIdx = 0;
  SceneReference controledSceneToLoad, controledSceneToUnLoad;

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

    if (ScenesToLoad.Length > 0)
      controledSceneToLoad = ScenesToLoad[loadSceneIdx].SceneRef;

    if (ScenesToUnload.Length > 0)
      controledSceneToUnLoad = ScenesToUnload[unloadSceneIdx].SceneRef;

    while (true)
    {
      if (ScenesToLoad.Length > 0)
        treeLoad.Tick();
      if (ScenesToUnload.Length > 0)
        treeUnLoad.Tick();

      yield return new WaitForSeconds(1f);
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
        .Sequence("Control the gate")
          .Condition("Scene registered?", () => controledSceneToUnLoad != null)
          .Do("lock/unlock the gate", () =>
          {
            LockGate(ScenesToUnload[unloadSceneIdx], !controledSceneToUnLoad.IsSafeToUse);
            return TaskStatus.Success;
          })
        .End()
        .Do("Next Scene", () =>
        {
          unloadSceneIdx++;
          if (unloadSceneIdx >= ScenesToUnload.Length)
            unloadSceneIdx = 0;
          controledSceneToUnLoad = ScenesToUnload[unloadSceneIdx].SceneRef;
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

          .Sequence("Control the gate")
            .Condition("Scene registered?", () => controledSceneToLoad != null)
            .Do("lock/unlock the gate", () =>
            {
              LockGate(ScenesToLoad[loadSceneIdx], !controledSceneToLoad.IsSafeToUse);
              return TaskStatus.Success;
            })
          .End()

          .Do("Next Scene to evaluate", () =>
          {
            loadSceneIdx++;
            if (loadSceneIdx >= ScenesToLoad.Length)
              loadSceneIdx = 0;
            controledSceneToLoad = ScenesToLoad[loadSceneIdx].SceneRef;
            return TaskStatus.Success;
          })
        .End()
      .End()
      .Build();
  }

  /// ====================================================================================
  /*IEnumerator Evaluate()
  {
    while (true)
    {
      int idx = 0;
      bool playerInTheRoom = PlayerIsInTheRoom();
      foreach (string scnName in SceneToLoadNames)
      {
        loadScene(idx, scnName, playerInTheRoom);
        idx++;
      }
      foreach (string scnName in SceneToUnloadNames)
      {
        unloadScene(scnName);
      }
      yield return new WaitForSeconds(1f);
    }
  }

  private void loadScene(int idx, string sceneToLoadName, bool playerIsInMyRoom)
  {
    //https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.GetSceneByName.html
    // If you use SceneManager.GetSceneByName(), after loading the scene in additive mode, then a valid scene is returned.
    Scene controledScene = SceneManager.GetSceneByName(sceneToLoadName);
    ////ConfigManager.WriteConsole($"[GateController] Evaluate Scene to load: [{sceneToLoadName} {controledScene.name}]");
    if (playerIsInMyRoom)
    {
      if (String.IsNullOrEmpty(controledScene.name))
      {
        //the scene is not loaded
        // //ConfigManager.WriteConsole($"[GateController] Scene: {sceneToLoadName} is not loaded");
        LockGate(idx, true);
        if (isPlayerCloseToController())
        {
          SceneManager.LoadSceneAsync(sceneToLoadName, LoadSceneMode.Additive);
          //ConfigManager.WriteConsole($"[GateController] ask to LOAD Scene: {sceneToLoadName}");
        }
      }
      else if (controledScene.isLoaded)
      {
        //the engine loaded the scene
        //ConfigManager.WriteConsole($"[GateController] Scene: {sceneToLoadName} is loaded, unlock the gate");
        LockGate(idx, false);
      }
    }
  }

  private void unloadScene(string sceneToUnloadName)
  {
    //https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.GetSceneByName.html
    // If you use SceneManager.GetSceneByName(), after loading the scene in additive mode, then a valid scene is returned.
    Scene controledScene = SceneManager.GetSceneByName(sceneToUnloadName);
    // //ConfigManager.WriteConsole($"[GateController] Evaluate Scene to unload: [{sceneToUnloadName} {controledScene.name}]");
    if (!String.IsNullOrEmpty(controledScene.name) && controledScene.isLoaded)
    {
      //the scene is loaded
      if (isPlayerCloseToController())
      {
        //ConfigManager.WriteConsole($"[GateController] UNLOAD SCENE: {sceneToUnloadName} is loaded, but the player is away ******.");
        SceneManager.UnloadSceneAsync(sceneToUnloadName);
      }
    }
  }
*/
}
