using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;

public class GateController : MonoBehaviour
{

  [Header("Scene Load settings")]
  [Tooltip("Names of the scenes to load, comma separated.")]
  [SerializeField]
  public string[] SceneToLoadNames;
  [Tooltip("The minimal distance between the player and the gate to load the scene.")]
  [SerializeField]
  public float MinimalDistanceToLoad = 0.9f;
  [Tooltip("Object that block the gate")]
  [SerializeField]
  public GameObject[] BlockingLoadObjects;
  [Tooltip("The floor object in the scene where the gate is")]
  [SerializeField]
  public GameObject Floor;


  [Header("Scene Unload settings")]
  [Tooltip("Names of the scenes to unload, comma separated.")]
  [SerializeField]
  public string[] SceneToUnloadNames;
  [SerializeField]
  public GameObject[] BlockingUnLoadObjects;  
  [Tooltip("The minimal distance between the player and the gate to unload the scene.")]
  [SerializeField]
  public float MinimalDistanceToUnload = 3f;

  [SerializeField]
  public BehaviorTree treeLoad, treeUnLoad;

  private GameObject centerCamera;
  private Bounds floorBounds;

  int loadSceneIdx = 0, unloadSceneIdx = 0;
  Scene controledSceneToLoad, controledSceneToUnLoad;

  // Start is called before the first frame update
  void Start()
  {
    centerCamera = GameObject.Find("CenterEyeAnchor");
    //ConfigManager.WriteConsole($"[GateController] -Starting- Scenes to control, load: {SceneToLoadNames} unload: {SceneToUnloadNames}");
    if (Floor != null)
    {
      var bounds = Floor.GetComponent<Renderer>().bounds;
      floorBounds = new Bounds(Floor.transform.position, new Vector3(Floor.transform.localScale.x, 100f, Floor.transform.localScale.z));
      ////ConfigManager.WriteConsole($"[GateController]{SceneToLoadNames} floor bounds: [{floorBounds.ToString()}]");
    }
    // StartCoroutine(Evaluate());
    StartCoroutine(runBT());
  }


  IEnumerator runBT()
  {
    treeLoad = buildLoadScenesBT();
    treeUnLoad = buildUnLoadScenesBT();

    if (SceneToLoadNames.Length > 0)
      controledSceneToLoad = SceneManager.GetSceneByName(SceneToLoadNames[loadSceneIdx]);

    if (SceneToUnloadNames.Length > 0)
      controledSceneToUnLoad = SceneManager.GetSceneByName(SceneToUnloadNames[unloadSceneIdx]);

    while (true)
    {
      if (SceneToLoadNames.Length > 0)
        treeLoad.Tick();
      if (SceneToUnloadNames.Length > 0)
        treeUnLoad.Tick();
      yield return new WaitForSeconds(1f / 4f);
    }
  }

  bool isPlayerCloseToController()
  {
    float d = Vector3.Distance(centerCamera.transform.position, gameObject.transform.position);
    // WriteConsole($"[curif.LibRetroMameCore.isPlayerClose] distance: {d} < {_distanceMinToPlayerToStartGame} {d < _distanceMinToPlayerToStartGame}");
    return d < MinimalDistanceToLoad;
  }
  bool isPlayerFar()
  {
    float d = Vector3.Distance(centerCamera.transform.position, gameObject.transform.position);
    // WriteConsole($"[curif.LibRetroMameCore.isPlayerClose] distance: {d} < {_distanceMinToPlayerToStartGame} {d < _distanceMinToPlayerToStartGame}");
    return d > MinimalDistanceToUnload;
  }
  void BlockLoadGate(int idx, bool block)
  {
    if (BlockingLoadObjects.Length > idx && BlockingLoadObjects[idx] != null)
      BlockingLoadObjects[idx].SetActive(block);

  }

  bool PlayerIsInMyRoom()
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
      .Condition("Is evaluated scene loaded?", () => !String.IsNullOrEmpty(controledSceneToUnLoad.name) && controledSceneToUnLoad.isLoaded)
      .Condition("Is the player far from the controller?", () => isPlayerFar())
      .Sequence()
        .Condition("Exists the scene to unload?", () => controledSceneToUnLoad != null)
        .Do("Unload Scene", () =>
        {
          ConfigManager.WriteConsole($"[GateController] UNLOAD SCENE: {controledSceneToUnLoad.name} is loaded, but the player is away ******.");
          BlockLoadGate(unloadSceneIdx, true);
          SceneManager.UnloadSceneAsync(controledSceneToUnLoad.name);
          return TaskStatus.Success;
        })
      .End()
      .Do("Next Scene to unload", () =>
      {
        unloadSceneIdx++;
        if (unloadSceneIdx >= SceneToUnloadNames.Length)
          unloadSceneIdx = 0;
        controledSceneToUnLoad = SceneManager.GetSceneByName(SceneToUnloadNames[unloadSceneIdx]);
        return TaskStatus.Success;
      })
    .End()
    .Build();
  }

  private BehaviorTree buildLoadScenesBT()
  {
    return new BehaviorTreeBuilder(gameObject)
    .Sequence()
      .Condition("Player is in the room", () => PlayerIsInMyRoom())
      .Condition("Is player close to the gate", () => isPlayerCloseToController())
      .Sequence()
        .Condition("Exist the scene to load", () => controledSceneToLoad != null)
        .Condition("Scene is not loaded", () => String.IsNullOrEmpty(controledSceneToLoad.name))
        // .Do("Block the door", () =>
        // {
        //   BlockLoadGate(loadSceneIdx, true);
        //   return TaskStatus.Success;
        // })
        .Do("Load the scene (async)", () =>
        {
          SceneManager.LoadSceneAsync(SceneToLoadNames[loadSceneIdx], LoadSceneMode.Additive);
          ConfigManager.WriteConsole($"[GateController] LOAD Scene: {SceneToLoadNames[loadSceneIdx]}");
          return TaskStatus.Success;
        })
        .RepeatUntilSuccess("Repeat until the scene is loaded")
          .Sequence()
            .Condition("Scene is loaded?", () =>
            {
              controledSceneToLoad = SceneManager.GetSceneByName(SceneToLoadNames[loadSceneIdx]);
              return controledSceneToLoad.isLoaded;
            })
            .Do("Unlock gate", () =>
            {
              //the engine loaded the scene
              ConfigManager.WriteConsole($"[GateController] Scene: {controledSceneToLoad.name} is loaded, unlock the gate");
              BlockLoadGate(loadSceneIdx, false);
              return TaskStatus.Success;
            })
          .End()
        .End()
      .End()
      .Do("Next Scene to evaluate", () =>
      {
        loadSceneIdx++;
        if (loadSceneIdx >= SceneToLoadNames.Length)
          loadSceneIdx = 0;
        controledSceneToLoad = SceneManager.GetSceneByName(SceneToLoadNames[loadSceneIdx]);
        return TaskStatus.Success;
      })
    .End()
    .Build();
  }

  /// ====================================================================================
  IEnumerator Evaluate()
  {
    while (true)
    {
      int idx = 0;
      bool playerInTheRoom = PlayerIsInMyRoom();
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
        BlockLoadGate(idx, true);
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
        BlockLoadGate(idx, false);
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
      if (isPlayerFar())
      {
        //ConfigManager.WriteConsole($"[GateController] UNLOAD SCENE: {sceneToUnloadName} is loaded, but the player is away ******.");
        SceneManager.UnloadSceneAsync(sceneToUnloadName);
      }
    }
  }
}
