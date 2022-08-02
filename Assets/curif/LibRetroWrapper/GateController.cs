using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

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
  [Tooltip("The minimal distance between the player and the gate to unload the scene.")]
  [SerializeField]
  public float MinimalDistanceToUnload = 3f;


  private GameObject camera;
  private Bounds floorBounds;

  // Start is called before the first frame update
  void Start()
  {
    camera = GameObject.Find("CenterEyeAnchor");
    ConfigManager.WriteConsole($"[GateController] -Starting- Scenes to control, load: {SceneToLoadNames} unload: {SceneToUnloadNames}");
    if (Floor != null)
    {
      var bounds = Floor.GetComponent<Renderer>().bounds;
      floorBounds = new Bounds(Floor.transform.position, new Vector3(Floor.transform.localScale.x, 100f, Floor.transform.localScale.z));
      ConfigManager.WriteConsole($"[GateController]{SceneToLoadNames} floor bounds: [{floorBounds.ToString()}]");
    }
    StartCoroutine(Evaluate());
  }

  bool isPlayerCloseToTheGate()
  {
    float d = Vector3.Distance(camera.transform.position, gameObject.transform.position);
    // WriteConsole($"[curif.LibRetroMameCore.isPlayerClose] distance: {d} < {_distanceMinToPlayerToStartGame} {d < _distanceMinToPlayerToStartGame}");
    return d < MinimalDistanceToLoad;
  }
  bool isPlayerFar()
  {
    float d = Vector3.Distance(camera.transform.position, gameObject.transform.position);
    // WriteConsole($"[curif.LibRetroMameCore.isPlayerClose] distance: {d} < {_distanceMinToPlayerToStartGame} {d < _distanceMinToPlayerToStartGame}");
    return d > MinimalDistanceToUnload;
  }
  void BlockLoadGate(int idx, bool block)
  {
    if (BlockingLoadObjects.Length > idx && BlockingLoadObjects[idx] != null)
    {
      BlockingLoadObjects[idx].SetActive(block);
    }

  }

  bool PlayerIsInMyRoom()
  {
    //https://docs.unity3d.com/ScriptReference/Bounds.html
    if (floorBounds == null)
    {
      return false;
    }
    return floorBounds.Contains(camera.transform.position);
  }

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
    //ConfigManager.WriteConsole($"[GateController] Evaluate Scene to load: [{sceneToLoadName} {controledScene.name}]");
    if (playerIsInMyRoom)
    {
      if (String.IsNullOrEmpty(controledScene.name))
      {
        //the scene is not loaded
        // ConfigManager.WriteConsole($"[GateController] Scene: {sceneToLoadName} is not loaded");
        BlockLoadGate(idx, true);
        if (isPlayerCloseToTheGate())
        {
          SceneManager.LoadSceneAsync(sceneToLoadName, LoadSceneMode.Additive);
          ConfigManager.WriteConsole($"[GateController] ask to LOAD Scene: {sceneToLoadName}");
        }
      }
      else if (controledScene.isLoaded)
      {
        //the engine loaded the scene
        ConfigManager.WriteConsole($"[GateController] Scene: {sceneToLoadName} is loaded, unlock the gate");
        BlockLoadGate(idx, false);
      }
    }
  }

  private void unloadScene(string sceneToUnloadName)
  {
    //https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.GetSceneByName.html
    // If you use SceneManager.GetSceneByName(), after loading the scene in additive mode, then a valid scene is returned.
    Scene controledScene = SceneManager.GetSceneByName(sceneToUnloadName);
    ConfigManager.WriteConsole($"[GateController] Evaluate Scene to unload: [{sceneToUnloadName} {controledScene.name}]");
    if (!String.IsNullOrEmpty(controledScene.name) && controledScene.isLoaded)
    {
      //the scene is loaded
      if (isPlayerFar())
      {
        ConfigManager.WriteConsole($"[GateController] UNLOAD SCENE: {sceneToUnloadName} is loaded, but the player is away ******.");
        SceneManager.UnloadSceneAsync(sceneToUnloadName);
      }
    }
  }
}
