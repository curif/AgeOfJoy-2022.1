using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GateController : MonoBehaviour
{
  [SerializeField]
  public string NextScene;

  [Tooltip("The minimal distance between the player and the gate to load the scene.")]
  [SerializeField]
  public float MinimalDistanceToLoadNextScene = 0.9f;

  private LibretroMameCore.Waiter SecsForCheqClose = new(2);
  private GameObject Camera;

  private bool SceneLoaded = false;

  // Start is called before the first frame update
  void Start()
  {
    Camera = GameObject.Find("CenterEyeAnchor");
  }
  bool isPlayerClose()
  {
    float d = Vector3.Distance(Camera.transform.position, gameObject.transform.position);
    // WriteConsole($"[curif.LibRetroMameCore.isPlayerClose] distance: {d} < {_distanceMinToPlayerToStartGame} {d < _distanceMinToPlayerToStartGame}");
    return d < MinimalDistanceToLoadNextScene;
  }

  // Update is called once per frame
  void Update()
  {
    if (SceneLoaded) return;
    if (SecsForCheqClose.Finished())
    {
      SecsForCheqClose.reset();
      ConfigManager.WriteConsole($"[GateController] next Scene: {NextScene} check if player is close");
      if (isPlayerClose())
      {
        ConfigManager.WriteConsole($"[GateController] LOAD next Scene: {NextScene}");
        SceneManager.LoadScene(NextScene, LoadSceneMode.Additive);
        SceneLoaded = true;
      }
    }
  }
}
