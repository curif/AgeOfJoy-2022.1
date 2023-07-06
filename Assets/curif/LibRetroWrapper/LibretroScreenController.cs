/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/


//#define _serialize_
#define _debug_

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
#if UNITY_EDITOR
using UnityEditor;
#endif


//[AddComponentMenu("curif/LibRetroWrapper/VideoPlayer")]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(GameVideoPlayer))]
[RequireComponent(typeof(LibretroControlMap))]
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
    [Tooltip("Save game state after load (Seconds to wait to finish load) for first time if the file (State File) doesn't exist.")]
    public bool EnableSaveState = true;
    [Tooltip("Name of the state file used to save/load the memory game state.")]
    public string StateFile = "state.nv";

    [Tooltip("Adjust Gamma from 1.0 to 2.0")]
    [SerializeField]
    public string Gamma = "1.0";

    [Tooltip("Adjust bright from 0.2 to 2.0")]
    [SerializeField]
    public string Brightness = "1.0";

    [SerializeField]
    public string ShaderName = "damage";

    [SerializeField]
    [Tooltip("Path that holds cabinet information, save states there.")]
    public string PathBase;
    [Tooltip("Positions where the player can stay to activate atraction videos")]
    public List<GameObject> AgentPlayerPositions;

    [SerializeField]
    public Dictionary<string, string> ShaderConfig = new Dictionary<string, string>();

    // [Tooltip("The global action manager in the main rig. We will find one if not set.")]
    // public InputActionManager inputActionManager;

    private ShaderScreenBase shader;
    private List<AgentScenePosition> agentPlayerPositionComponents;
    private GameObject player;
    private ChangeControls changeControls;
    private CoinSlotController CoinSlot;
    private GameObject centerEyeCamera;
    private Camera cameraComponentCenterEye;
    private Renderer display;
    private GameVideoPlayer videoPlayer;
    private DateTime timeToExit = DateTime.MinValue;
    private GameObject cabinet;
    private CabinetReplace cabinetReplace;

    //hands
    private GameObject leftHand, rightHand, rightControl, leftControl;

    //controls
    private LibretroControlMap libretroControlMap;
    public ControlMapConfiguration CabinetControlMapConfig = null;
    //public ControlMapConfiguration GlobalControlMapConfig = null;
    //public ControlMapConfiguration UserControlMapConfig = null;

    private CoinSlotController getCoinSlotController()
    {
        Transform coinslot = cabinet.transform.Find("coin-slot-added");

        if (!coinslot)
            return null;

        return coinslot.gameObject.GetComponent<CoinSlotController>();
    }

    private bool playerIsInSomePosition()
    {
        if (agentPlayerPositionComponents == null)
            return false;

        foreach (AgentScenePosition asp in agentPlayerPositionComponents)
        {
            if (asp.IsPlayerPresent)
                return true;
        }
        return false;
    }

    // Start is called before the first frame update
    void Start()
    {
        LibretroMameCore.WriteConsole($"[LibretroScreenController.Start] {gameObject.name}");

        display = GetComponent<Renderer>();
        cabinet = gameObject.transform.parent.gameObject;
        videoPlayer = gameObject.GetComponent<GameVideoPlayer>();
        libretroControlMap = GetComponent<LibretroControlMap>();

        //camera
        centerEyeCamera = GameObject.Find("Main Camera");
        if (centerEyeCamera == null)
            throw new Exception("Camera not found in GameObject Tree");
        cameraComponentCenterEye = centerEyeCamera.GetComponent<Camera>();

        player = GameObject.Find("OVRPlayerControllerGalery");
        changeControls = player.GetComponent<ChangeControls>();

        leftHand = GameObject.Find("LeftHand");
        rightHand = GameObject.Find("RightHand");
        rightControl = GameObject.Find("RightControl");
        leftControl = GameObject.Find("LeftControl");

        // GameObject inputActionManagerGameobject = GameObject.Find("Input Action Manager");
        // if (inputActionManagerGameobject == null)
        //     ConfigManager.WriteConsoleError("[LibretroScreenController.Start] input action manager in the main rig doesn't found.");
        // else
        //     inputActionManager = inputActionManagerGameobject.GetComponent<InputActionManager>();

        CoinSlot = getCoinSlotController();
        if (CoinSlot == null)
            ConfigManager.WriteConsoleError("[LibretroScreenController.Start] Coin Slot not found in cabinet !!!! no one can play this game.");

        agentPlayerPositionComponents = new List<AgentScenePosition>();
        if (AgentPlayerPositions != null)
        {
            foreach (GameObject playerPos in AgentPlayerPositions)
            {
                AgentScenePosition asp = playerPos.GetComponent<AgentScenePosition>();
                if (asp != null)
                    agentPlayerPositionComponents.Add(asp);
            }
        }

        StartCoroutine(runBT());

        return;
    }

    IEnumerator runBT()
    {
        tree = buildScreenBT();

        //material and shader
        shader = ShaderScreen.Factory(display, 1, ShaderName, ShaderConfig);
        ConfigManager.WriteConsole($"[LibretroScreenController.Start] shader created: {shader}");

        videoPlayer.setVideo(GameVideoFile, shader, GameVideoInvertX, GameVideoInvertY);
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
              //.Condition("Is visible", () => display.isVisible)
              .Condition("Not running any game", () => !LibretroMameCore.GameLoaded)
              .Condition("There are coins", () => CoinSlot.hasCoins())
              // .Condition("Player near", () => Vector3.Distance(Player.transform.position, Display.transform.position) < DistanceMinToPlayerToActivate)
              //.Condition("Player looking screen", () => isPlayerLookingAtScreen3()) if coinslot is present with coins is sufficient
              .Do("Start game", () =>
              {
                  videoPlayer.Stop();

                  // core do it: shader.texture = LibretroMameCore.GameTexture;
                  shader.Invert(GameInvertX, GameInvertY);

                  //start mame
                  LibretroMameCore.WriteConsole($"MAME Start game: {GameFile} in screen {name} +_+_+_+_+_+_+_+__+_+_+_+_+_+_+_+_+_+_+_+_");
                  LibretroMameCore.Speaker = GetComponent<AudioSource>();
                  LibretroMameCore.Display = display;
                  LibretroMameCore.SecondsToWaitToFinishLoad = SecondsToWaitToFinishLoad;
#if _serialize_
            LibretroMameCore.EnableSaveState = EnableSaveState;
            LibretroMameCore.StateFile = StateFile;
#endif
                  LibretroMameCore.Brightness = Brightness;
                  LibretroMameCore.Gamma = Gamma;
                  LibretroMameCore.CoinSlot = CoinSlot;
                  LibretroMameCore.PathBase = PathBase;
                  LibretroMameCore.shader = shader;

                  //controllers
                  cabinetReplace = cabinet.GetComponent<CabinetReplace>();
                  ControlMapConfiguration controlConf;
                  if (CabinetControlMapConfig != null)
                  {
                      ConfigManager.WriteConsole($"[LibretroScreenController] map loaded with a CustomControlMap (usually cabinet configuration)");
                      controlConf = new CustomControlMap(CabinetControlMapConfig);
                  }
                  else if (GameControlMap.ExistsConfiguration(cabinetReplace.game.CabinetDBName))
                  {
                      ConfigManager.WriteConsole($"[LibretroScreenController] loading user controller configuration, GameControlMap: {cabinetReplace.game.CabinetDBName}");
                      controlConf = new GameControlMap(cabinetReplace.game.CabinetDBName);
                  }
                  else
                  {
                      ConfigManager.WriteConsole($"[LibretroScreenController] no controller user configuration, no cabinet configuration, using GlobalControlMap");
                      controlConf = new GlobalControlMap();
                  }
                  //   ConfigManager.WriteConsole($"[LibretroScreenController] controller configuration as markdown in the next line:");
                  // ConfigManager.WriteConsole(controlConf.AsMarkdown());
                  libretroControlMap.CreateFromConfiguration(controlConf);
                  LibretroMameCore.ControlMap = libretroControlMap;
                  // start libretro
                  if (!LibretroMameCore.Start(name, GameFile))
                  {
                      CoinSlot.clean();
                      return TaskStatus.Failure;
                  }

                  PreparePlayerToPlayGame(true);

                  return TaskStatus.Success;
              })
            .End()

            .Sequence("Game Started")
              .Condition("Game is running?", () => LibretroMameCore.isRunning(name, GameFile))
              .RepeatUntilSuccess("Run until player exit")
                .Sequence()
                  //.Condition("Player looking screen", () => isPlayerLookingAtScreen3())
                  //.Condition("Is visible", () => display.isVisible)
                  .Condition("user EXIT pressed?", () =>
                  {
                      if (libretroControlMap.Active("EXIT") == 1)
                          return true;

                      timeToExit = DateTime.MinValue;
                      return false;
                  })
                  .Condition("N secs pass with user EXIT pressed", () =>
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
                  //videoPlayer.Play();
                  LibretroMameCore.End(name, GameFile);
                  PreparePlayerToPlayGame(false);
                  libretroControlMap.Clean();
                  timeToExit = DateTime.MinValue;

                  return TaskStatus.Success;
              })
            .End()

            .Sequence("Video Player control")
              //.Condition("Have video player", () => videoPlayer != null)
              .Selector()
                .Sequence()
                  //.Condition("Is visible", () => videoPlayer.isVisible())
                  .Condition("Not running any game", () => !LibretroMameCore.GameLoaded)
                  //.Condition("Is visible", () => display.isVisible)
                  // .Condition("Player near", () => Vector3.Distance(Player.transform.position, Display.transform.position) < DistanceMinToPlayerToActivate)
                  //.Condition("Player looking screen", () => isPlayerLookingAtScreen4())
                  .Condition("Player in the zone?", () => playerIsInSomePosition())
                  //.Condition("Game is not running?", () => !LibretroMameCore.isRunning(name, GameFile))
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

          .End()
        .Build();
    }

    void PreparePlayerToPlayGame(bool isPlaying)
    {

        ConfigManager.WriteConsole($"[LibRetroMameCore.PreparePlayerToPlayGame] disable hands: {isPlaying}");
        changeControls.PlayerMode(isPlaying);

        //change sound configuration
        GameObject[] allSpeakers = GameObject.FindGameObjectsWithTag("speaker");
        foreach (GameObject speaker in allSpeakers)
        {
            BackgroundSoundController bsc = speaker.GetComponent<BackgroundSoundController>();
            if (bsc)
                bsc.InGame(isPlaying);
        }

        //enable-disable inputMap
        ConfigManager.WriteConsole($"[LibRetroMameCore.PreparePlayerToPlayGame] enable game inputs: {isPlaying}");
        libretroControlMap.Enable(isPlaying);
    }

    public void Update()
    {
        // LibretroMameCore.WriteConsole($"MAME {GameFile} Libretro {LibretroMameCore.GameFileName} loaded: {LibretroMameCore.GameLoaded}");
        LibretroMameCore.Run(name, GameFile); //only runs if this game is running
        shader.Update();
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

    private bool isPlayerLookingAtScreen4()
    {
        Vector3 screenPos = cameraComponentCenterEye.WorldToViewportPoint(transform.position);
        if (screenPos.x > 0 && screenPos.x < 1 && screenPos.y > 0 && screenPos.y < 1 && screenPos.z > 0)
        {
            // The target object is within the viewport bounds
            RaycastHit hitInfo;
            if (Physics.Linecast(cameraComponentCenterEye.transform.position, transform.position, out hitInfo))
            {
                // The linecast hit something, check if it was the target object
                //special case when the screen is blocked with the cabine's box collider (it's own parent)
                return hitInfo.transform == transform || hitInfo.transform == display.transform.parent;
            }
        }
        return false;
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        LibretroMameCore.MoveAudioStreamTo(GameFile, data);
    }

    private void OnDestroy()
    {
        LibretroMameCore.End(name, GameFile);

        if (LibretroMameCore.isRunning(name, GameFile))
            PreparePlayerToPlayGame(false);
    }

    public void InsertCoin()
    {
        CoinSlot.insertCoin();
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(LibretroScreenController))]
public class LibretroScreenControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LibretroScreenController myScript = (LibretroScreenController)target;
        if(GUILayout.Button("InsertCoin"))
        {
          myScript.InsertCoin();
        }
    }
}
#endif