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
using System;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;
using System.Linq;
using AOJ.Managers; // Geometrrizer: Allows access to EventManager for player FX
using LC = LibretroControlMapDictionnary;
using UnityEngine.Audio;


#if UNITY_EDITOR
using CM = ControlMapPathDictionary;
using UnityEditor;
#endif


//[AddComponentMenu("curif/LibRetroWrapper/VideoPlayer")]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioClip))]
[RequireComponent(typeof(GameVideoPlayer))]
[RequireComponent(typeof(GameAudioPlayer))]
[RequireComponent(typeof(LibretroControlMap))]
[RequireComponent(typeof(basicAGE))]
[RequireComponent(typeof(CabinetAGEBasic))]
public class LibretroScreenController : MonoBehaviour, ISuspendableCabinetScreen
{
    [SerializeField]
    public string GameFile = "1942.zip";
    public string ScreenName = ""; //loaded on start, needed for the multitasking
    public List<string> PlayList = new List<string>();

    [SerializeField]
    public string GameVideoFile;

    [SerializeField]
    public string GameAudioFile;

    [SerializeField]
    public bool GameVideoInvertX = false;
    [SerializeField]
    public bool GameVideoInvertY = false;
    public CabinetInformation.Video GameVideoConfig;

    public GameVideoPlayer videoPlayer;
    public GameAudioPlayer audioPlayer;

    [SerializeField]
    public bool GameInvertX = false;
    [SerializeField]
    public bool GameInvertY = false;

    [SerializeField]
    public BehaviorTree tree;

    [SerializeField]
    private Light screenGlowLight;

    //[SerializeField]
    //public GameObject Player;
    [Tooltip("The maximum distance between the player and the screen to active video.")]
    [SerializeField]
    public float DistanceMaxToPlayerToActivateVideo = 2.5f;

    [Tooltip("The maximum distance between the player and the screen to active audio.")]
    [SerializeField]
    public float DistanceMaxToPlayerToActivateAudio = 3.5f;

    [Tooltip("The time in secs that the player has to look to another side to exit the game and recover mobility.")]
    [SerializeField]
    public int SecondsToWaitToExitGame = 2;

    [SerializeField]
    public int SecondsToWaitToFinishLoad = 2;
    [Tooltip("Save game state after load (Seconds to wait to finish load) for first time if the file (State File) doesn't exist.")]
    public bool EnableSaveState = true;
    [Tooltip("Name of the state file used to save/load the memory game state.")]
    public string StateFile = "state.nv";

    [Header("Colors")]
    [Tooltip("Adjust Gamma from 1.0 to 2.0")]
    [SerializeField]
    public string Gamma = "1.0";

    [Tooltip("Adjust bright from 0.2 to 2.0")]
    [SerializeField]
    public string Brightness = "1.0";

    [SerializeField]
    public string Core = "mame2003+";

    // Flycast only: route the thumbstick to the DC analog stick + triggers (input.analog-stick).
    [SerializeField]
    public bool AnalogStick = false;

    [SerializeField]
    public bool? Persistent;

    [SerializeField]
    [Tooltip("Path that holds cabinet information, save states there.")]
    public string PathBase;
    [Tooltip("Positions where the player can stay to activate atraction videos")]
    public List<AgentScenePosition> AgentPlayerPositions;

    [SerializeField]
    public CabinetInformation.Screen screen;

    [Header("Audio Settings")]
    public AudioMixerGroup audioMixerAttractMode;
    public AudioMixerGroup audioMixerGame;
    private AudioSource audioSource;

    public LightGunInformation lightGunInformation;
    public Cabinet cabinet;
    public CoreEnvironment CabEnvironment;
    public bool? InsertCoinOnStartup;
    public Dictionary<uint, LibretroInputDevice> LibretroInputDevices;

    public bool SimulateExitGame;

    private ShaderScreenBase shader, videoShader;
    private GameObject player;
    private ChangeControls changeControls;
    private CoinSlotController CoinSlot;
    private GameObject centerEyeCamera;
    private Camera cameraComponentCenterEye;
    private Renderer display;
    private DateTime timeToExit = DateTime.MinValue;
    private CabinetReplace cabinetReplace;
    private LightGunTarget lightGunTarget;

    //controls
    private LibretroControlMap libretroControlMap;
    public ControlMapConfiguration CabinetControlMapConfig = null;

    //age basic
    public CabinetAGEBasicInformation ageBasicInformation;
    private CabinetAGEBasic cabinetAGEBasic;
    public BackgroundSoundController backgroundSoundController;
    public GlobalConfiguration globalConfiguration = null;

    private Coroutine mainCoroutine;
    private bool initialized = false;
    private bool videoInitialized = false;
    private bool gameRunning = false;
    private bool playerInTheZone = false;
    private float distanceToPlayer;
    private bool screenLightON = false;
    private DateTime lastAudioStatsReport = DateTime.MinValue;

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
        GameObject GlobalConfigurationGameObject = GameObject.Find("FixedGlobalConfiguration");
        this.globalConfiguration = GlobalConfigurationGameObject.GetComponent<GlobalConfiguration>();
        if (globalConfiguration == null)
            ConfigManager.WriteConsoleError($"[LibretroScreenController.Start] {name} globalConfiguration not found.");

        LibretroMameCore.WriteConsole($"[LibretroScreenController.Start] {gameObject.name}");

        display = GetComponent<Renderer>();
        // Which material slot the active shader writes to. Built-in screen prefabs use slot 1
        // (slot 0 = bezel); crt.type: custom collapses to a single slot 0 via ScreenSurfaceInfo.
        ScreenSurfaceInfo screenSurface = GetComponent<ScreenSurfaceInfo>();
        int screenSlot = screenSurface?.materialSlot ?? 1;
        if (screenSurface != null) // custom screen diagnostic (debug mode only)
            ConfigManager.WriteConsole($"[LibretroScreenController] {name}: custom screen surface slot={screenSlot}, renderer materialCount={display.materials.Length}, mesh submeshes={GetComponent<MeshFilter>()?.sharedMesh?.subMeshCount}");
        // cabinet = gameObject.transform.parent.gameObject;
        videoPlayer = gameObject.GetComponent<GameVideoPlayer>();
        if (videoPlayer == null)
            ConfigManager.WriteConsoleError($"[LibretroScreenController.Start] {name} video player doesn't exists on screen.");
        audioPlayer = gameObject.GetComponent<GameAudioPlayer>();

        libretroControlMap = GetComponent<LibretroControlMap>();
        cabinetAGEBasic = GetComponent<CabinetAGEBasic>();

        audioSource = GetComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = audioMixerAttractMode;

        ScreenName = name;

        //camera
        centerEyeCamera = GameObject.Find("Main Camera");
        if (centerEyeCamera == null)
            throw new Exception("Camera not found in GameObject Tree");
        cameraComponentCenterEye = centerEyeCamera.GetComponent<Camera>();

        player = GameObject.Find("OVRPlayerControllerGalery");
        changeControls = player.GetComponent<ChangeControls>();

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
            ConfigManager.WriteConsoleError($"[LibretroScreenController.Start] {name} Coin Slot not found in cabinet !!!! no one can play this game.");

        //Game screen shader---------------------
        if (!string.IsNullOrEmpty(globalConfiguration.Configuration.cabinet.forcedShader))
            shader = ShaderScreen.Factory(display, screenSlot, globalConfiguration.Configuration.cabinet.forcedShader, screen.config());
        else if (!string.IsNullOrEmpty(screen.shader))
            shader = ShaderScreen.Factory(display, screenSlot, screen.shader, screen.config());
        else
            shader = ShaderScreen.Factory(display, screenSlot, "crt", screen.config());

        //video shader ----------------
        string videoShaderName;
        Dictionary<string, string> videoShaderConfig;
        if (!string.IsNullOrEmpty(GameVideoConfig?.screen?.shader))
        {
            //this code protects from users assigning heavy screen shaders as video shaders.
            videoShaderName = GameVideoConfig.screen.shader;
            videoShaderName = ShaderScreenBase.RecommendedReplacementForAttractionVideos(videoShaderName);
            videoShaderConfig = GameVideoConfig.screen.config();
        }
        else
        {
            //select a video shader depending on the user selected game shader
            videoShaderName = shader.AlternativeShaderForAttractionVideos();
            videoShaderConfig = shader.AlternativeConfigForAttractionVideos();
        }
        videoShader = ShaderScreen.Factory(display, screenSlot, videoShaderName, videoShaderConfig);

        AttractVideoBudget.Configure(globalConfiguration.Configuration.cabinet.maxAttractVideos);

        ConfigManager.WriteConsole($"[LibretroScreenController.Start] {name} game shader created: {shader} video shader: {videoShader}");

        // age basic ---------------------
        if (ageBasicInformation != null)
        {
            cabinetAGEBasic.Init(ageBasicInformation, PathBase, cabinet, CoinSlot, lightGunTarget);
            cabinetAGEBasic.ExecAfterLoadBas();
        }

        // glow light
        screenGlowLight = GetComponentInChildren<Light>(true);
        screenLightON = screenGlowLight != null && globalConfiguration.Configuration.cabinet.screenGlowIntensity > 0;

        mainCoroutine = StartCoroutine(runBT());
        initialized = true;

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
        //the emulator suspends with the app (headset off / system overlay) — RetroArch pauses too
        if (LibretroFlycastCore.isRunning(ScreenName, GameFile))
            LibretroFlycastCore.SetPaused(pauseStatus);

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

     /// <summary>Restart attract-mode BT after MR hide/destroy or spawn wiring.</summary>
    public void EnsureAttractLoopRunning()
    {
        if (!initialized || !isActiveAndEnabled)
            return;

        if (mainCoroutine == null)
            mainCoroutine = StartCoroutine(runBT());
    }

    /// <summary>Stop attract BT, clip playback, and (if applicable) a running game.</summary>
    public void SuspendAttractAndPlaybackForTransition()
    {
        if (mainCoroutine != null)
        {
            StopCoroutine(mainCoroutine);
            mainCoroutine = null;
        }

        // Fully stop an in-progress game so the emulator run thread and audio
        // streaming don't keep consuming CPU while nobody is playing.
        if (gameRunning || LibretroMameCore.isRunning(ScreenName, GameFile))
            ExitPlayerFromGame();

        // Stop() (not Pause()) releases the video decoder; the shader is left showing
        // the cached attract-video frame (or the standby image if none was cached yet).
        if (videoPlayer != null)
            videoPlayer.Stop();

        if (audioPlayer != null)
            audioPlayer.Stop();
    }



    IEnumerator runBT()
    {
        // LibretroMameCore.WriteConsole($"[LibretroScreenController.runBT] coroutine BT cicle Start {gameObject.name}");

        // Only wire up the video/audio source once: re-running setVideo() (and the
        // TextureCache.Init() it triggers) on every resume races an async cached-thumbnail
        // load against the live video frame binding, sometimes leaving the screen frozen
        // on the thumbnail while the video (and its audio) keeps playing underneath.
        if (!videoInitialized)
        {
            videoPlayer.setVideo(GameVideoFile, videoShader, GameVideoInvertX, GameVideoInvertY);
            audioPlayer.path = GameAudioFile;
            videoInitialized = true;
        }

        tree = buildScreenBT();
        while (true)
        {

            distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            playerInTheZone = playerIsInSomePosition();
            tree.Tick();
            // LibretroMameCore.WriteConsole($"[runBT] {gameObject.name} Is visible: {isVisible} Not running any game: {!LibretroMameCore.GameLoaded} There are coins: {CoinSlot.hasCoins()} Player looking screen: {isPlayerLookingAtScreen()}");
            yield return new WaitForSeconds(1f);
        }
    }

    private BehaviorTree buildScreenBT()
    {
        return new BehaviorTreeBuilder(gameObject).
          Selector()
            .Sequence("Start the game")
              .Condition("CoinSlot is present", () => CoinSlot != null)
              //.Condition("Is visible", () => display.isVisible)
              .Condition("Not running any game", () => !LibretroMameCore.GameLoaded && !LibretroFlycastCore.GameLoaded)
              .Condition("There are coins", () => CoinSlot.hasCoins())
              // .Condition("Player near", () => Vector3.Distance(Player.transform.position, Display.transform.position) < DistanceMinToPlayerToActivate)
              //.Condition("Player looking screen", () => isPlayerLookingAtScreen3()) if coinslot is present with coins is sufficient
              .Do("Start game", () =>
              {
                  if (isGameFilePresent())
                  {
                      videoPlayer.Pause();
                      audioPlayer.Stop();
                   
                      if (screenLightON)
                          screenGlowLight.gameObject.SetActive(true);
                  }

                  ConfigManager.WriteConsole($"[LibretroScreenController] Start game: {GameFile} in screen {name} +_+_+_+_+_+_+_+__+_+_+_+_+_+_+_+_+_+_+_+_");

                  //hardware-rendered core (own Vulkan device, libpdlr) has its own start path
                  if (isFlycast)
                      return StartFlycastGame();

                  //start mame
                  LibretroMameCore.Speaker = audioSource;
                  LibretroMameCore.SecondsToWaitToFinishLoad = SecondsToWaitToFinishLoad;
                  LibretroMameCore.Brightness = Brightness;
                  LibretroMameCore.Gamma = Gamma;
                  LibretroMameCore.CoinSlot = CoinSlot;
                  LibretroMameCore.Persistent = Persistent;
                  LibretroMameCore.Core = Core;
                  LibretroMameCore.CabEnvironment = CabEnvironment;
                  LibretroMameCore.Shader = shader;
#if _serialize_
                  LibretroMameCore.EnableSaveState = EnableSaveState;
                  LibretroMameCore.StateFile = StateFile;
#endif

                  //controllers
                  libretroControlMap.CreateFromConfiguration(BuildControlMapConfiguration());
                  LibretroMameCore.ControlMap = libretroControlMap;
                  // Light guns configuration
                  if (lightGunTarget != null && lightGunInformation != null)
                  {
                      lightGunTarget.enabled = true;

                      lightGunTarget.Init(lightGunInformation, PathBase, player);
                      LibretroMameCore.lightGunTarget = lightGunTarget;
                  }

                  LibretroMameCore.libretroInputDevices = LibretroInputDevices;

                  // start libretro
                  bool insertCoinOnStartup = InsertCoinOnStartup.HasValue ? 
                    InsertCoinOnStartup.Value : globalConfiguration.Configuration.cabinet.insertCoinOnStartup;
                  if (!insertCoinOnStartup)
                  {
                      CoinSlot.clean();
                  }
#if !UNITY_EDITOR

                  if (isGameFilePresent()) 
                  {
                      // start libretro
                      if (!LibretroMameCore.Start(ScreenName, GameFile, PlayList))
                      {
                          CoinSlot.clean();
                          return TaskStatus.Failure;
                      }
                  } 
                  else
                  {
                      LibretroMameCore.AssignControls();
                  }
#else
                  LibretroMameCore.simulateInEditor(ScreenName, GameFile);
#endif

                  PreparePlayerToPlayGame(true);
                  if (lightGunTarget != null)
                      changeControls.ChangeRightJoystickModelLightGun(lightGunTarget.GetModelPath(), true);

                  //admit user interactions (like insert coins)
                  LibretroMameCore.StartInteractions();

                  // start retro_run cycle
#if !UNITY_EDITOR
                  if (isGameFilePresent()) 
                  {
                      LibretroMameCore.StartRunThread();
                  }
#endif

                  if (isGameFilePresent())
                  {
                      shader.Activate(LibretroMameCore.GameTexture);
                      shader.Invert(GameInvertX, GameInvertY);

                      //audio mixer group
                      audioSource.outputAudioMixerGroup = audioMixerGame;
                      audioSource.spatialize = false;
                  }

                  cabinet.PhyActivate();

                  // age basic Insert coin
                  if (ageBasicInformation != null && ageBasicInformation.active != false)
                      cabinetAGEBasic.ExecInsertCoinBas();

                  gameRunning = true;

                  return TaskStatus.Success;
              })
            .End()

            .Sequence("Game Started")
              .Condition("Game is running?", () => gameRunning)
              .RepeatUntilSuccess("Run until player exit")
                .Sequence()
                  .Condition("user EXIT pressed?", () =>
                  {
                      if (PlayerWantsToExit())
                      {
                          if (!EventManager.Instance.IsPlayingExitSound)
                              EventManager.Instance.PlayExitGameSound();
                          return true;
                      }
                      else
                      {
                          if (EventManager.Instance.IsPlayingExitSound)
                              EventManager.Instance.StopExitGameSound();
                          timeToExit = DateTime.MinValue;
                          return false;
                      }
                  })
                  .Condition("N secs pass with user EXIT pressed", () =>
                  {
                      if (timeToExit == DateTime.MinValue)
                      {
                          timeToExit = DateTime.Now.AddSeconds(SecondsToWaitToExitGame);
                      }
                      else if (DateTime.Now > timeToExit)
                      {
                          return true;
                      }
                      return false;
                  })
                .End()
              .End()
              .Do("Exit game", () =>
              {
                  EventManager.Instance.StopExitGameSound();
                  ExitPlayerFromGame();
                  if (screenLightON)
                  {
                      screenGlowLight.gameObject.SetActive(false);
                  }
                  return TaskStatus.Success;
              })
            .End()

            .Selector("Video/Audio Player control")
                .Sequence()
                    .Condition("Running any game or Player not in the zone?", () => LibretroMameCore.GameLoaded || LibretroFlycastCore.GameLoaded || !playerInTheZone)
                    .Do("Stop video and audio player", () =>
                    {
                        videoPlayer.Stop();
                        audioPlayer.Stop();
                        return TaskStatus.Success;
                    })
                .End()
                .Sequence()
                    .Condition("Player in the zone?", () => playerInTheZone)
                    .Condition("Not running any game", () => !LibretroMameCore.GameLoaded && !LibretroFlycastCore.GameLoaded)
                    .Selector()
                        .Sequence()
                            .Condition("Is Player near enough to see video", () =>
                                            distanceToPlayer <= DistanceMaxToPlayerToActivateVideo)
                            .Condition("Is Player looking the screen zone", () => isPlayerLookingAtScreenZone())
                            .Do("Play video", () =>
                            {
                                audioSource.spatialize = true;
                                audioSource.maxDistance = DistanceMaxToPlayerToActivateVideo;

                                audioPlayer.Stop();
                                videoPlayer.Play();

                                return TaskStatus.Success;
                            })
                        .End()
                        .Sequence()
                            .Condition("Is Player near to ear audio", () =>
                                    distanceToPlayer <= DistanceMaxToPlayerToActivateAudio)
                            .Do("Play audio clip", () =>
                            {
                                audioSource.spatialize = true;
                                audioSource.maxDistance = DistanceMaxToPlayerToActivateAudio;

                                videoPlayer.Pause();
                                audioPlayer.Play();

                                return TaskStatus.Success;
                            })
                        .End()
                    .End()
                .End()
            .End()
          .End()
        .Build();
    }

    bool isGameFilePresent()
    {
        return GameFile != null && GameFile.Length > 0;
    }

    //hardware-rendered core driven by LibretroFlycastCore/libpdlr instead of the software wrapper
    private bool isFlycast
    {
        get { return Core == LibretroFlycastCore.CoreName; }
    }

    //select the control map by priority: cabinet yaml > per-game user config > control scheme > global
    private ControlMapConfiguration BuildControlMapConfiguration()
    {
        cabinetReplace = cabinet.gameObject.GetComponent<CabinetReplace>();
        ControlMapConfiguration controlConf;
        if (CabinetControlMapConfig != null)
        {
            ConfigManager.WriteConsole($"[LibretroScreenController] map loaded with a CustomControlMap (usually cabinet configuration)");
            controlConf = new CustomControlMap(CabinetControlMapConfig);
        }
        else if (!string.IsNullOrEmpty(cabinetReplace?.game?.CabinetDBName) &&
                   GameControlMap.ExistsConfiguration(cabinetReplace.game.CabinetDBName))
        {
            ConfigManager.WriteConsole($"[LibretroScreenController] loading user controller configuration, GameControlMap: {cabinetReplace.game.CabinetDBName}");
            controlConf = new GameControlMap(cabinetReplace.game.CabinetDBName);
        }
        else if (!string.IsNullOrEmpty(cabinetReplace.cabinet?.ControlScheme) &&
                   ControlSchemeControlMap.ExistsConfiguration(cabinetReplace.cabinet.ControlScheme))
        {
            ConfigManager.WriteConsole($"[LibretroScreenController] loading control scheme configuration, ControlSchemeControlMap: {cabinetReplace.cabinet.ControlScheme}");
            controlConf = new ControlSchemeControlMap(cabinetReplace.cabinet.ControlScheme);
        }
        else
        {
            ConfigManager.WriteConsole($"[LibretroScreenController] no controller user configuration, no cabinet configuration, using GlobalControlMap");
            controlConf = new GlobalControlMap();
        }
#if UNITY_EDITOR
        controlConf.AddMap(LC.KEYB_UP, CM.KEYBOARD_W);
        controlConf.AddMap(LC.KEYB_DOWN, CM.KEYBOARD_S);
        controlConf.AddMap(LC.KEYB_LEFT, CM.KEYBOARD_A);
        controlConf.AddMap(LC.KEYB_RIGHT, CM.KEYBOARD_D);
#endif
        return controlConf;
    }

    //flycast branch of the "Start game" BT node: same cabinet lifecycle, hardware core underneath.
    private TaskStatus StartFlycastGame()
    {
        // Flycast-only map surgery (gamepads polled directly with the standard flycast layout;
        // gun-cabinet default rebinds) — see LibretroFlycastCore.AdjustControlMap.
        ControlMapConfiguration flycastConf = BuildControlMapConfiguration();
        LibretroFlycastCore.AdjustControlMap(flycastConf, lightGunTarget != null && lightGunInformation != null);
        libretroControlMap.CreateFromConfiguration(flycastConf);

        LibretroFlycastCore.Shader = shader;
        LibretroFlycastCore.ControlMap = libretroControlMap;
        LibretroFlycastCore.CoinSlot = CoinSlot;
        LibretroFlycastCore.AnalogStick = AnalogStick;
        LibretroFlycastCore.CabEnvironment = CabEnvironment;

        // Light guns configuration (same wiring as the MAME path; must precede LibretroFlycastCore.Start,
        // which declares the gun's maple port before the core loads the game)
        if (lightGunTarget != null && lightGunInformation != null)
        {
            lightGunTarget.enabled = true;
            lightGunTarget.Init(lightGunInformation, PathBase, player);
            LibretroFlycastCore.lightGunTarget = lightGunTarget;
        }

        bool insertCoinOnStartup = InsertCoinOnStartup.HasValue ?
          InsertCoinOnStartup.Value : globalConfiguration.Configuration.cabinet.insertCoinOnStartup;
        if (!insertCoinOnStartup)
        {
            CoinSlot.clean();
        }

#if !UNITY_EDITOR
        if (isGameFilePresent())
        {
            if (!LibretroFlycastCore.Start(ScreenName, GameFile))
            {
                CoinSlot.clean();
                return TaskStatus.Failure;
            }
        }
#endif

        PreparePlayerToPlayGame(true);
        if (lightGunTarget != null)
            changeControls.ChangeRightJoystickModelLightGun(lightGunTarget.GetModelPath(), true);

        if (isGameFilePresent())
        {
            //stand-by texture until the first emulated frame is bound (the AHB import takes a moment)
            shader.Activate(ShaderScreenBase.StandByTexture);
            shader.Invert(GameInvertX, GameInvertY);

            //audio mixer group
            audioSource.outputAudioMixerGroup = audioMixerGame;
            audioSource.spatialize = false;
            audioSource.Play();
        }

        cabinet.PhyActivate();

        // age basic Insert coin
        if (ageBasicInformation != null && ageBasicInformation.active != false)
            cabinetAGEBasic.ExecInsertCoinBas();

        gameRunning = true;

        return TaskStatus.Success;
    }

    bool PlayerWantsToExit()
    {
        if (libretroControlMap.isActive(LC.MODIFIER) && libretroControlMap.isActive(LC.EXIT))
        {
            return true;
        }
#if UNITY_EDITOR
        if (SimulateExitGame)
        {
            ConfigManager.WriteConsole($"SimulateExitGame: {SimulateExitGame}");
            return true;
        }
#endif
        return false;
    }
    void ExitPlayerFromGame()
    {
        cabinet.PhyDeactivate();

        if (isGameFilePresent())
        {
            //audio mixer group
            audioSource.outputAudioMixerGroup = audioMixerAttractMode;

            //to replace the shader texture ASAP:
            videoPlayer.Play();

            LibretroMameCore.End(ScreenName, GameFile);
            LibretroFlycastCore.End(ScreenName, GameFile);
        }
        timeToExit = DateTime.MinValue;

        PreparePlayerToPlayGame(false);
        libretroControlMap.Clean();

        // age basic
        if (ageBasicInformation != null && ageBasicInformation.active != false)
        {
            cabinetAGEBasic.Stop(); //force
            cabinetAGEBasic.ExecAfterLeaveBas();
        }

        if (lightGunTarget != null && lightGunInformation != null)
            lightGunTarget.enabled = false;


#if UNITY_EDITOR
        SimulateExitGame = false;
        ConfigManager.WriteConsole($"simulated player exit finished.");
#endif

        gameRunning = false;
    }

    void PreparePlayerToPlayGame(bool isPlaying)
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
        // LibretroMameCore.WriteConsole($"MAME {GameFile} Libretro {LibretroMameCore.GameFileName} loaded: {LibretroMameCore.GameLoaded}");
        //LibretroMameCore.Run(name, GameFile); //only runs if this game is running
        if (shader == null)
            return;

        if (LibretroMameCore.isRunning(ScreenName, GameFile))
        {
            if (screenLightON)
            {
                float r, g, b;

#if !UNITY_EDITOR
                r = LibretroMameCore.getLightRed();
                g = LibretroMameCore.getLightGreen();
                b = LibretroMameCore.getLightBlue();
#else
                r = 255; g = 0; b = 0; //red color
#endif
                screenGlowLight.color = new Color(r,g,b);
                float luminance = (r + g + b) / 3f;
                screenGlowLight.intensity = luminance * globalConfiguration.Configuration.cabinet.screenGlowIntensity;
            }

            LibretroMameCore.UpdateTexture();

            if (DateTime.Now >= lastAudioStatsReport)
            {
                string stats = LibretroMameCore.GetAndResetAudioStats();
                if (stats != null)
                {
                    // how many voices the audio thread is servicing besides this game
                    int playingSources = 0;
                    foreach (AudioSource src in FindObjectsOfType<AudioSource>())
                        if (src.isPlaying)
                            playingSources++;
                    ConfigManager.WriteConsole($"[LibretroScreenController] {name} {stats} playingAudioSources: {playingSources}");
                }
                lastAudioStatsReport = DateTime.Now.AddSeconds(5);
            }

            LibretroMameCore.FlushAudioCaptureIfReady();
        }
        else if (LibretroFlycastCore.isRunning(ScreenName, GameFile))
        {
            LibretroFlycastCore.Update();
        }

        shader.Update();

        return;
    }

    private bool isPlayerLookingAtScreen4()
    {
        if (!isPlayerLookingAtScreenZone())
            return false;

        // The target object is within the viewport bounds
        LayerMask layerMask = 1 << gameObject.layer; // 10:CRT
        RaycastHit hitInfo;
        if (Physics.Linecast(cameraComponentCenterEye.transform.position,
                                transform.position, out hitInfo, layerMask))
        {
            // The linecast hit something, check if it was the target object
            //special case when the screen is blocked with the cabine's box collider (it's own parent)
            // return hitInfo.transform == transform || hitInfo.transform == display.transform.parent;
            return hitInfo.transform == transform;
        }
        return false;
    }

    private bool isPlayerLookingAtScreenZone()
    {
        Vector3 screenPos = cameraComponentCenterEye.WorldToViewportPoint(transform.position);
        return (screenPos.z > 0 && screenPos.x > 0 && screenPos.x < 1 && screenPos.y > 0 && screenPos.y < 1);
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (LibretroMameCore.isRunning(ScreenName, GameFile))
            LibretroMameCore.MoveAudioStreamTo(data, channels);
        else if (LibretroFlycastCore.isRunning(ScreenName, GameFile))
            LibretroFlycastCore.MoveAudioStreamTo(data);
    }

    private void OnDestroy()
    {
        if (LibretroMameCore.isRunning(ScreenName, GameFile) || LibretroFlycastCore.isRunning(ScreenName, GameFile))
            PreparePlayerToPlayGame(false);

        LibretroMameCore.End(ScreenName, GameFile);
        LibretroFlycastCore.End(ScreenName, GameFile);

        shader?.ReleaseMaterialInstance();
        videoShader?.ReleaseMaterialInstance();
    }

#if UNITY_EDITOR
    public void InsertCoin()
    {
        CoinSlot.insertCoin();
    }
    public void ExitGame()
    {
        ConfigManager.WriteConsole("[LibretroScreenController] EXIT GAME ------ ");
        SimulateExitGame = true;
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(LibretroScreenController))]
public class LibretroScreenControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LibretroScreenController myScript = (LibretroScreenController)target;
        if (GUILayout.Button("InsertCoin"))
        {
            myScript.InsertCoin();
        }
        if (GUILayout.Button("Simulate Exit Game"))
        {
            myScript.ExitGame();
        }
    }
}
#endif