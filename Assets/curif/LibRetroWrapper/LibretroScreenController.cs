/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/


#define _debug_

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;


//[AddComponentMenu("curif/LibRetroWrapper/VideoPlayer")]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(GameVideoPlayer))]
// [RequireComponent(typeof(BoxCollider))]
public class LibretroScreenController : MonoBehaviour
{
  [SerializeField]
  public string GameFile = "1942.zip";

  [SerializeField]
  public string GameVideoFile;
  [SerializeField]
  public bool GameVideoInvertX = false;
  [SerializeField]
  public bool GameVideoInvertY = false;

  [SerializeField]
  public bool GameInvertX = false;
  [SerializeField]
  public bool GameInvertY = false;

  [SerializeField]
  public BehaviorTree tree;

  //[SerializeField]
  //public GameObject Player;
  [Tooltip("The minimal distance between the player and the screen to be active.")]
  [SerializeField]
  public float DistanceMinToPlayerToActivate = 2f;
  [Tooltip("The time in secs that the player has to look to another side to exit the game and recover mobility.")]
  [SerializeField]
  public int SecondsToWaitToExitGame = 2;

  [SerializeField]
  public int SecondsToWaitToFinishLoad = 2;

  [Tooltip("Adjust Gamma from 1.0 to 2.0")]
  [SerializeField]
  public LibretroMameCore.GammaOptions Gamma = LibretroMameCore.GammaOptions.GAMA_1;
  [Tooltip("Adjust bright from 0.2 to 2.0")]
  [SerializeField]
  public LibretroMameCore.BrightnessOptions Brightness = LibretroMameCore.BrightnessOptions.BRIGHT_1;

  private GameObject player;
  private OVRPlayerController playerController;
  private CoinSlotController CoinSlot;
  private GameObject centerEyeCamera;
  private Camera cameraComponentCenterEye;
  private Renderer display;
  private GameVideoPlayer videoPlayer;
  private bool isVisible;
  private DateTime timeToExit = DateTime.MinValue;

  private CoinSlotController getCoinSlotController()
  {
    Transform coinslot = gameObject.transform.parent.Find("coin-slot-added");

    if (!coinslot)
      return null;

    return coinslot.gameObject.GetComponent<CoinSlotController>();
  }

  // Start is called before the first frame update
  void Start()
  {
    LibretroMameCore.WriteConsole($"[LibretroScreenController.Start] {gameObject.name}");

    display = GetComponent<Renderer>();

    centerEyeCamera = GameObject.Find("CenterEyeAnchor");
    if (centerEyeCamera == null)
      throw new Exception("Camera not found in GameObject Tree");
    cameraComponentCenterEye = centerEyeCamera.GetComponent<Camera>();

    player = GameObject.Find("OVRPlayerControllerGalery");
    playerController = player.GetComponent<OVRPlayerController>();

    CoinSlot = getCoinSlotController();
    if (CoinSlot == null)
      Debug.LogError("Coin Slot not found in cabinet !!!! no one can play this game.");

    if (! String.IsNullOrEmpty(GameVideoFile)) {
      videoPlayer = gameObject.GetComponent<GameVideoPlayer>();
      videoPlayer.setVideo(GameVideoFile, invertX: GameVideoInvertX, invertY: GameVideoInvertY);
    }
    
    //StartCoroutine(loop());
    StartCoroutine(runBT());

    return;
  }


  IEnumerator runBT()
  {
    tree = buildScreenBT();
    // LibretroMameCore.WriteConsole($"[LibretroScreenController.runBT] coroutine BT cicle Start {gameObject.name}");

    while (true)
    {
      tree.Tick();
      // LibretroMameCore.WriteConsole($"[runBT] {gameObject.name} Is visible: {isVisible} Not running any game: {!LibretroMameCore.GameLoaded} There are coins: {CoinSlot.hasCoins()} Player looking screen: {isPlayerLookingAtScreen()}");
      yield return new WaitForSeconds(1f / 2f);
    }
  }


  private BehaviorTree buildScreenBT()
  {
    return new BehaviorTreeBuilder(gameObject).
      Selector()
        .Sequence("Start the game")
          .Condition("CoinSlot is present", () => CoinSlot != null)
          .Condition("Is visible", () => isVisible)
          .Condition("Not running any game", () => !LibretroMameCore.GameLoaded)
          .Condition("There are coins", () => CoinSlot.hasCoins())
          // .Condition("Player near", () => Vector3.Distance(Player.transform.position, Display.transform.position) < DistanceMinToPlayerToActivate)
          .Condition("Player looking screen", () => isPlayerLookingAtScreen())
          .Do("Start game", () =>
          {
            videoPlayer?.Stop();

            display.materials[1].SetFloat("MirrorX", GameInvertX ? 1f : 0f);
            display.materials[1].SetFloat("MirrorY", GameInvertY ? 1f : 0f);

            //start mame
            LibretroMameCore.WriteConsole($"MAME Start game: {GameFile} in screen {name} +_+_+_+_+_+_+_+__+_+_+_+_+_+_+_+_+_+_+_+_");
            LibretroMameCore.Speaker = GetComponent<AudioSource>();
            LibretroMameCore.Display = display;
            LibretroMameCore.SecondsToWaitToFinishLoad = SecondsToWaitToFinishLoad;
            LibretroMameCore.Brightness = Brightness;
            LibretroMameCore.Gamma = Gamma;
            LibretroMameCore.CoinSlot = CoinSlot;
            if (!LibretroMameCore.Start(name, GameFile)) {
              CoinSlot.clean();
              return TaskStatus.Failure;  
            }

            LockControls(true);

            return TaskStatus.Success;
          })
        .End()
        .Sequence("Game Started")
          .Condition("Game is running?", () => LibretroMameCore.isRunning(name, GameFile))
          .RepeatUntilSuccess("Run until player exit")
            .Sequence()
              // .Condition("Game running", () => LibretroMameCore.isRunning(name, GameFile))
              .Condition("Is visible", () => isVisible)
              .Condition("Left trigger", () =>
              {
                if (OVRInput.Get(OVRInput.RawButton.LHandTrigger))
                  return true;

                timeToExit = DateTime.MinValue;
                return false;
              })
              .Condition("N secs pass with trigger pressed", () =>
              {
                if (timeToExit == DateTime.MinValue)
                  timeToExit = DateTime.Now.AddSeconds(SecondsToWaitToExitGame);
                else if (DateTime.Now > timeToExit)
                  return true;

                return false;
              })
            .End()
          .End()
          .Do("Exit game", () =>
          {
            videoPlayer?.Play();
            LibretroMameCore.End(name, GameFile);
            LockControls(false);
            timeToExit = DateTime.MinValue;

            return TaskStatus.Success;
          })
        .End()

        .Sequence("Video Player control")
          .Condition("Have video player", () => videoPlayer != null)
          .Selector()
            .Sequence()
              .Condition("Is visible", () => isVisible)
              // .Condition("Player near", () => Vector3.Distance(Player.transform.position, Display.transform.position) < DistanceMinToPlayerToActivate)
              .Condition("Player looking screen", () => isPlayerLookingAtScreen())
              .Condition("Game is not running?", () => !LibretroMameCore.isRunning(name, GameFile))
              .Do("Play video player", () =>
              {
                videoPlayer.Play();
                return TaskStatus.Success;
              })
            .End()
            .Do("Pause video player", () =>
            {
              videoPlayer.Pause();
              return TaskStatus.Success;
            })
        .End()
      .End()
    .Build();
  }

  void LockControls(bool takeControls)
  {
    //lock controls, if takeControls is true the Player can't move.
    LibretroMameCore.WriteConsole($"[LibRetroMameCore.LockControls] {takeControls}");
    playerController.EnableLinearMovement = !takeControls;
    playerController.EnableRotation = !takeControls;
  }

  public void Update()
  {
    // LibretroMameCore.WriteConsole($"MAME {GameFile} Libretro {LibretroMameCore.GameFileName} loaded: {LibretroMameCore.GameLoaded}");
    LibretroMameCore.Run(name, GameFile); //only runs if this game is running
    return;
  }

  /*
  bool isPlayerCloser(GameObject _camera, Renderer _display, float _distanceMinToPlayerToStartGame)
  {
    float d = Vector3.Distance(_camera.transform.position, _display.transform.position);
    // WriteConsole($"[curif.LibRetroMameCore.isPlayerClose] distance: {d} < {_distanceMinToPlayerToStartGame} {d < _distanceMinToPlayerToStartGame}");
    return d < _distanceMinToPlayerToStartGame;
  }
  */

  // https://answers.unity.com/questions/8003/how-can-i-know-if-a-gameobject-is-seen-by-a-partic.html?page=1&pageSize=5&sort=votes
  private bool isPlayerLookingAtScreen()
  {
    RaycastHit hit = new RaycastHit();
    //var planes = GeometryUtility.CalculateFrustumPlanes(cameraComponentCenterEye);
    
    //Debug.DrawLine(cam.transform.position, Display.bounds.center, Color.red);
    
    /*//screen inside fustrum
    if (!GeometryUtility.TestPlanesAABB(planes, display.bounds)) {
      LibretroMameCore.WriteConsole(display.name + " TestPlanesAABB fail");
      return false;
    }
    */
    //https://docs.unity3d.com/ScriptReference/Physics.Linecast.html
    if (! Physics.Linecast(cameraComponentCenterEye.transform.position, display.bounds.center, out hit))
      return true; 
     
    // LibretroMameCore.WriteConsole(display.name + " occluded by " + hit.transform.name);
    //special case when the screen is blocked with the cabine's box collider (it's own parent)
    return hit.transform == display.transform.parent
  }

  private void OnAudioFilterRead(float[] data, int channels)
  {
    LibretroMameCore.MoveAudioStreamTo(GameFile, data);
  }

  private void OnDestroy()
  {
    LibretroMameCore.End(name, GameFile);

    if (LibretroMameCore.isRunning(name, GameFile))
      LockControls(false);
  }

  void OnBecameVisible()
  {
    isVisible = true;
  }
  void OnBecameInvisible()
  {
    isVisible = false;
  }

}