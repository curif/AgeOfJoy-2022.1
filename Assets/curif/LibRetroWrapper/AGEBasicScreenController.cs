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
using System.Linq;
using LC = LibretroControlMapDictionnary;
using CM = ControlMapPathDictionary;
using UnityEngine.Audio;



#if UNITY_EDITOR
using UnityEditor;
#endif

//[AddComponentMenu("curif/LibRetroWrapper/VideoPlayer")]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(GameVideoPlayer))]
[RequireComponent(typeof(LibretroControlMap))]
[RequireComponent(typeof(basicAGE))]
[RequireComponent(typeof(CabinetAGEBasic))]
[RequireComponent(typeof(LightGunTarget))]
// [RequireComponent(typeof(BoxCollider))]
public class AGEBasicScreenController : MonoBehaviour, ISuspendableCabinetScreen
{
    public string ScreenName = ""; //loaded on start, needed for the multitasking

    [SerializeField]
    public string VideoFile;
    [SerializeField]
    public bool VideoInvertX = false;
    [SerializeField]
    public bool VideoInvertY = false;
    public GameVideoPlayer videoPlayer;

    [SerializeField]
    public BehaviorTree tree;

    //[SerializeField]
    //public GameObject Player;
    [Tooltip("The minimal distance between the player and the screen to active video.")]
    [SerializeField]
    public float DistanceMinToPlayerToActivate = 4f;
    [Tooltip("The time in secs that the player has to look to another side to exit the game and recover mobility.")]
    [SerializeField]
    public int SecondsToWaitToExit = 2;

    [SerializeField]
    public bool InvertX = false;
    [SerializeField]
    public bool InvertY = false;
    public CabinetInformation.Screen screen = new();

    [SerializeField]
    [Tooltip("Path that holds cabinet information.")]
    public string PathBase;
    [Tooltip("Positions where the player can stay to activate videos")]
    public List<AgentScenePosition> AgentPlayerPositions;

    public GlobalConfiguration globalConfiguration = null;

    public Cabinet cabinet;

    public bool SimulateExitGame;

    // [Tooltip("The global action manager in the main rig. We will find one if not set.")]
    // public InputActionManager inputActionManager;

    private ShaderScreenBase shader, videoShader;
    private GameObject player;
    private ChangeControls changeControls;
    private CoinSlotController CoinSlot;
    private GameObject centerEyeCamera;
    private Camera cameraComponentCenterEye;
    private Renderer display;
    private DateTime timeToExit = DateTime.MinValue;
    // private GameObject cabinet;
    private CabinetReplace cabinetReplace;

    //controls
    private LibretroControlMap libretroControlMap;
    public ControlMapConfiguration CabinetControlMapConfig = null;

    //age basic
    public CabinetAGEBasicInformation ageBasicInformation = new();
    private CabinetAGEBasic cabinetAGEBasic;
    public BackgroundSoundController backgroundSoundController;

    private Coroutine mainCoroutine;
    private bool initialized = false;
    private bool videoInitialized = false;
    private bool CoinWasInserted = false;

    private LightGunTarget lightGunTarget;
    public LightGunInformation lightGunInformation;

    private CoinSlotController getCoinSlotController()
    {
        Transform coinslot = cabinet?.gameObject?.transform.Find("coin-slot-added");
        if (coinslot == null)
            return null;

        return coinslot.gameObject.GetComponent<CoinSlotController>();
    }

    private bool playerIsInSomePosition()
    {
        return AgentPlayerPositions != null && AgentPlayerPositions.Any(asp => asp.IsPlayerPresent);
    }

    // Start is called before the first frame update
    void Start()
    {
        LibretroMameCore.WriteConsole($"[AGEBasicScreenController.Start] {name}");
        GameObject GlobalConfigurationGameObject = GameObject.Find("FixedGlobalConfiguration");
        this.globalConfiguration = GlobalConfigurationGameObject.GetComponent<GlobalConfiguration>();
        if (globalConfiguration == null)
            ConfigManager.WriteConsoleError($"[LibretroScreenController.Start] {name} globalConfiguration not found.");

        display = GetComponent<Renderer>();

        // cabinet = gameObject.transform.parent.gameObject;
        videoPlayer = gameObject.GetComponent<GameVideoPlayer>();
        if (videoPlayer == null)
            ConfigManager.WriteConsoleError($"[AGEBasicScreenController.Start] {name} video player doesn't exists on screen.");
        else
            videoPlayer.UseDirectAudio = true; // bypass DSP buffer to prevent AudioSampleProvider overflow

        cabinetAGEBasic = GetComponent<CabinetAGEBasic>();

        ScreenName = name;

        //camera
        centerEyeCamera = GameObject.Find("Main Camera");
        if (centerEyeCamera == null)
            throw new Exception("Camera not found in GameObject Tree");
        cameraComponentCenterEye = centerEyeCamera.GetComponent<Camera>();

        player = GameObject.Find("OVRPlayerControllerGalery");
        changeControls = player.GetComponent<ChangeControls>();

        //lightgun activation
        lightGunTarget = GetComponent<LightGunTarget>();
        lightGunTarget.enabled = false;
        for (int i = 0; i < cabinet.gameObject.transform.childCount; i++)
        {
            Transform child = cabinet.gameObject.transform.GetChild(i);

            if (cabinet.IsLightGunTarget(child.name))
                lightGunTarget.addPart(child.gameObject);
        }

        CoinSlot = getCoinSlotController();
        if (CoinSlot == null)
            ConfigManager.WriteConsoleError($"[AGEBasicScreenController.Start] {name} Coin Slot not found in cabinet !!!! no one can play this game.");

        //material and shader
        if (!string.IsNullOrEmpty(globalConfiguration.Configuration.cabinet.forcedShader))
            shader = ShaderScreen.Factory(display, 1, globalConfiguration.Configuration.cabinet.forcedShader, screen.config());
        else if (!string.IsNullOrEmpty(screen.shader))
            shader = ShaderScreen.Factory(display, 1, screen.shader, screen.config());
        else
            shader = ShaderScreen.Factory(display, 1, "crt", screen.config());

        //video shader ----------------
        videoShader = ShaderScreen.Factory(display, 1, shader.AlternativeShaderForAttractionVideos(), screen.config());

        AttractVideoBudget.Configure(globalConfiguration.Configuration.cabinet.maxAttractVideos);

        ConfigManager.WriteConsole($"[AGEBasicScreenController.Start]  {name} shader created: {shader} video shader {videoShader}");

        mainCoroutine = StartCoroutine(runBT());

        initialized = true;
        CoinWasInserted = false;

        return;
    }

    //runs before Start()
    private void OnEnable()
    {
        if (!initialized)
            return;
        if (mainCoroutine == null)
            mainCoroutine = StartCoroutine(runBT());
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            //is pausing
            if (mainCoroutine != null)
            {
                StopCoroutine(mainCoroutine);
                mainCoroutine = null;
            }
        }
        else
        {
            if (initialized)
                mainCoroutine = StartCoroutine(runBT());
        }
    }

    private void OnDisable()
    {
        if (!initialized)
            return;
        if (mainCoroutine != null)
        {
            StopCoroutine(mainCoroutine);
            mainCoroutine = null;
        }
    }

    /// <summary>Restart the attract-mode BT after a suspend.</summary>
    public void EnsureAttractLoopRunning()
    {
        if (!initialized || !isActiveAndEnabled)
            return;

        if (mainCoroutine == null)
            mainCoroutine = StartCoroutine(runBT());
    }

    /// <summary>Stop BT ticking, any running AGEBasic program, and the attract video.</summary>
    public void SuspendAttractAndPlaybackForTransition()
    {
        if (mainCoroutine != null)
        {
            StopCoroutine(mainCoroutine);
            mainCoroutine = null;
        }

        // Fully stop a running program (same cleanup as the "END Program" BT node)
        // so the AGEBasic interpreter doesn't keep executing invisibly.
        if (CoinWasInserted || cabinetAGEBasic.AGEBasic.IsRunning() || cabinetAGEBasic.AGEBasic.IsRunningInBackground())
        {
            cabinet.PhyDeactivate();
            cabinetAGEBasic.Stop(); //force
            cabinetAGEBasic.ExecAfterLeaveBas();

            videoPlayer.BudgetPin(false);
            if (!string.IsNullOrEmpty(VideoFile))
                videoPlayer.setVideo(VideoFile, videoShader, VideoInvertX, VideoInvertY);
            else
                videoPlayer.StopAndReset();

            EndPlayerActivities();
            if (lightGunTarget != null && lightGunInformation != null)
                lightGunTarget.enabled = false;

            CoinWasInserted = false;
        }

        // Stop() (not Pause()) releases the video decoder; the shader is left showing
        // the cached attract-video frame (or the standby image if none was cached yet).
        if (videoPlayer != null)
            videoPlayer.Stop();
    }

    IEnumerator runBT()
    {
        yield return new WaitForEndOfFrame();

        // LibretroMameCore.WriteConsole($"[AGEBasicScreenController.runBT] coroutine BT cicle Start {gameObject.name}");
        // Only wire up the video source once: re-running setVideo() (and the
        // TextureCache.Init() it triggers) on every resume races an async cached-thumbnail
        // load against the live video frame binding, sometimes leaving the screen frozen
        // on the thumbnail while the video (and its audio) keeps playing underneath.
        if (!videoInitialized)
        {
            videoPlayer.setVideo(VideoFile, videoShader, VideoInvertX, VideoInvertY);
            videoInitialized = true;
        }
        cabinetAGEBasic.Init(ageBasicInformation, PathBase, cabinet, CoinSlot, lightGunTarget);
        cabinetAGEBasic.ActivateShader(shader);
        cabinetAGEBasic.SetVideoConfig(videoPlayer, shader);
        shader.Invert(InvertX, InvertY);
        // age basic after load
        cabinetAGEBasic.ExecAfterLoadBas();

        tree = buildScreenBT();
        while (true)
        {
            tree.Tick();
            // LibretroMameCore.WriteConsole($"[runBT] {gameObject.name} Is visible: {isVisible} Not running any game: {!LibretroMameCore.GameLoaded} There are coins: {CoinSlot.hasCoins()} Player looking screen: {isPlayerLookingAtScreen()}");
            yield return new WaitForSeconds(1f);
        }
    }

    private void setupActionMap()
    {
        if (libretroControlMap == null)
            libretroControlMap = GetComponent<LibretroControlMap>();
        if (libretroControlMap == null)
        {
            ConfigManager.WriteConsoleError($"[ConfigurationController.setupActionMap] ControlMap component not found.");
            return;
        }

        if (cabinetReplace == null)
            cabinetReplace = cabinet.gameObject.GetComponent<CabinetReplace>();
    
        ControlMapConfiguration controlConf;
        if (CabinetControlMapConfig != null)
        {
            ConfigManager.WriteConsole($"[AGEBasicScreenController.setupActionMap] map loaded with a CustomControlMap (usually cabinet configuration)");
            controlConf = new CustomControlMap(CabinetControlMapConfig);
        }
        else if (!string.IsNullOrEmpty(cabinetReplace?.game?.CabinetDBName) &&
                   GameControlMap.ExistsConfiguration(cabinetReplace.game.CabinetDBName))
        {
            ConfigManager.WriteConsole($"[AGEBasicScreenController.setupActionMap] loading user controller configuration, GameControlMap: {cabinetReplace.game.CabinetDBName}");
            controlConf = new GameControlMap(cabinetReplace.game.CabinetDBName);
        }
        else if (!string.IsNullOrEmpty(cabinetReplace.cabinet?.ControlScheme) &&
                             ControlSchemeControlMap.ExistsConfiguration(cabinetReplace.cabinet.ControlScheme))
        {
            ConfigManager.WriteConsole($"[AGEBasicScreenController] loading control scheme configuration, ControlSchemeControlMap: {cabinetReplace.cabinet.ControlScheme}");
            controlConf = new ControlSchemeControlMap(cabinetReplace.cabinet.ControlScheme);
        }
        else
        {
            ConfigManager.WriteConsole($"[AGEBasicScreenController.setupActionMap] no controller user configuration, no cabinet configuration, using GlobalControlMap");
            controlConf = new GlobalControlMap();
        }

        //   ConfigManager.WriteConsole($"[AGEBasicScreenController] controller configuration as markdown in the next line:");
        //   ConfigManager.WriteConsole(controlConf.AsMarkdown());
#if UNITY_EDITOR
        controlConf.AddMap(LC.KEYB_UP, CM.KEYBOARD_W);
        controlConf.AddMap(LC.KEYB_DOWN, CM.KEYBOARD_S);
        controlConf.AddMap(LC.KEYB_LEFT, CM.KEYBOARD_A);
        controlConf.AddMap(LC.KEYB_RIGHT, CM.KEYBOARD_D);
#endif
        libretroControlMap.CreateFromConfiguration(controlConf, name);
        libretroControlMap.Enable(true);
    }

    private BehaviorTree buildScreenBT()
    {
        return new BehaviorTreeBuilder(gameObject).
          Selector()
            .Sequence("Start the agebasic screen")
              .Condition("AGEBasic is active?", () => ageBasicInformation.active != false)
              .Condition("CoinSlot is present", () => CoinSlot != null)
              .Condition("Not initialized?", () => !CoinWasInserted)
              .Condition("There are coins", () => CoinSlot.hasCoins())
              .Do("Start AGEBasic", () =>
              {

                  //start mame
                  ConfigManager.WriteConsole($"[AGEBasicScreenController] in screen {name} +_+_+_+_+_+_+_+__+_+_+_+_+_+_+_+_+_+_+_+_");
                  
                  cabinet.PhyActivate();

                  StartPlayerActivities();

                  // Ligth guns configuration (lazy load)
                  if (lightGunTarget != null && lightGunInformation != null && !lightGunTarget.Initialized())
                  {
                      lightGunTarget.enabled = true;
                      lightGunTarget.Init(lightGunInformation, PathBase, player);
                      changeControls.ChangeRightJoystickModelLightGun(lightGunTarget.GetModelPath(), true);
                  }

                  CoinWasInserted = true;
                  CoinSlot.clean();

                  return TaskStatus.Success;
              })
            .End()

            .Sequence("AGEBasic running control")
              .Condition("AGEBasic is active?", () => ageBasicInformation.active != false)
              .Condition("Coin inserted?", () => CoinWasInserted)
              //.Condition("A program is not running?", () => !cabinetAGEBasic.AGEBasic.IsRunning())
              //.Condition("A program is not running (background)?", () => !cabinetAGEBasic.AGEBasic.IsRunningInBackground())
              .Do("Run main program", () =>
              {
                  //   videoPlayer.Stop();
                  // the player paid a coin: this screen's video always wins a decoder slot
                  videoPlayer.BudgetPin(true);
                  videoPlayer.Pause();

                  cabinetAGEBasic.ActivateShader(shader);
                  cabinetAGEBasic.ExecInsertCoinBas();

                  CoinWasInserted = false;

                  return TaskStatus.Success;
              })

              .Sequence("AGEBasic Running")
                .RepeatUntilSuccess("Until player or program exit")
                    .Selector()
                        .Condition("not running anymore?", () => 
                        {
                            return !cabinetAGEBasic.AGEBasic.IsRunning() && 
                                   !cabinetAGEBasic.AGEBasic.IsRunningInBackground();
                        })
                        .Sequence("Manual Exit")
                            .Condition("user EXIT or not running anymore?", () =>
                            {
                                if (libretroControlMap.isActive(LC.EXIT))
                                    return true;
#if UNITY_EDITOR
                                if (SimulateExitGame)
                                    return true;
#endif
                                timeToExit = DateTime.MinValue;
                                return false;
                            })
                            .Condition("N secs pass with user EXIT pressed", () =>
                            {
                                if (timeToExit == DateTime.MinValue)
                                    timeToExit = DateTime.Now.AddSeconds(SecondsToWaitToExit);
                                else if (DateTime.Now > timeToExit)
                                    return true;
                                return false;
                            })
                        .End()
                    .End()
                .End()
              .End()
              .Do("END Program", () =>
                  {
                      cabinet.PhyDeactivate();

                      // age basic leave
                      cabinetAGEBasic.Stop(); //force
                      cabinetAGEBasic.ExecAfterLeaveBas();

                      // Restore attraction video so EndPlayerActivities() replays it correctly.
                      // VIDEOLOAD may have changed videoPath during the session; reset it here.
                      // If no attraction video is configured, reset completely so Play() is a
                      // no-op and the BT video loop cannot restart the last AGEBasic-loaded clip.
                      videoPlayer.BudgetPin(false);
                      if (!string.IsNullOrEmpty(VideoFile))
                          videoPlayer.setVideo(VideoFile, videoShader, VideoInvertX, VideoInvertY);
                      else
                          videoPlayer.StopAndReset();

                      EndPlayerActivities();
                      if (lightGunTarget != null && lightGunInformation != null)
                           lightGunTarget.enabled = false;

#if UNITY_EDITOR
                      SimulateExitGame = false;
#endif
                      return TaskStatus.Success;
                  })
                .End()
            .End()

            .Sequence("Video Player control")
              //.Condition("Have video player", () => videoPlayer != null)
              .Selector()
                .Sequence()
                  .Condition("Player not in the zone?", () => !playerIsInSomePosition())
                  .Do("Stop video player", () =>
                  {
                      videoPlayer.Stop();
                      return TaskStatus.Success;
                  })
                .End()
                .Sequence()
                  .Condition("A program is not running?", () => !cabinetAGEBasic.AGEBasic.IsRunning())
                  .Condition("A program is not running (background)?", () => !cabinetAGEBasic.AGEBasic.IsRunningInBackground())
                  .Condition("Is Player looking the screen", () => isPlayerLookingAtScreen4())
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

    void EndPlayerActivities()
    {
        //to replace the shader texture ASAP:
        videoPlayer.Play();

        timeToExit = DateTime.MinValue;

        PreparePlayerToRunPrograms(false);
        libretroControlMap.Clean();
    }

    void StartPlayerActivities()
    {
        setupActionMap();
        PreparePlayerToRunPrograms(true);
    }

    void PreparePlayerToRunPrograms(bool isPlaying)
    {
        ConfigManager.WriteConsole($"[LibRetroMameCore.PreparePlayerToPlayGame] disable hands: {isPlaying}");
        changeControls.PlayerMode(isPlaying);

        //change sound configuration
        if (backgroundSoundController != null)
            backgroundSoundController.InGame(isPlaying);

        //enable-disable inputMap
        ConfigManager.WriteConsole($"[LibRetroMameCore.PreparePlayerToPlayGame] enable game inputs: {isPlaying}");
        libretroControlMap.Enable(isPlaying);
    }

    public void Update()
    {
        if (shader != null)
            shader.Update();

        return;
    }

    public bool IsNearPlayer()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        return distance <= DistanceMinToPlayerToActivate;
    }

    private bool isPlayerLookingAtScreen4()
    {
        LayerMask layerMask = 1 << gameObject.layer; // 10:CRT
        Vector3 screenPos = cameraComponentCenterEye.WorldToViewportPoint(transform.position);

        if (screenPos.z > 0 && screenPos.x > 0 && screenPos.x < 1 && screenPos.y > 0 && screenPos.y < 1)
        {
            // The target object is within the viewport bounds
            RaycastHit hitInfo;

            if (Physics.Linecast(cameraComponentCenterEye.transform.position,
                                    transform.position, out hitInfo, layerMask))
            {
                // The linecast hit something, check if it was the target object
                //special case when the screen is blocked with the cabine's box collider (it's own parent)
                // return hitInfo.transform == transform || hitInfo.transform == display.transform.parent;
                return hitInfo.transform == transform;
            }
        }
        return false;
    }

#if UNITY_EDITOR
    public void InsertCoin()
    {
        CoinSlot.insertCoin();
    }
    public void ExitProgram()
    {
        ConfigManager.WriteConsole("[AGEBasicScreenController] EXIT PROGRAM by button ------ ");
        SimulateExitGame = true;
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(AGEBasicScreenController))]
public class AGEBasicScreenControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AGEBasicScreenController myScript = (AGEBasicScreenController)target;
        if(GUILayout.Button("InsertCoin"))
        {
          myScript.InsertCoin();
        }
        if(GUILayout.Button("Simulate Exit Game"))
        {
          myScript.ExitProgram();
        }
    }
}
#endif