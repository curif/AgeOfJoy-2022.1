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
public class LibretroScreenController : MonoBehaviour
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
    private Light light;

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
    private bool gameRunning = false;
    private bool playerInTheZone = false;
    private float distanceToPlayer;
    private bool screenLightON = false;

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
            shader = ShaderScreen.Factory(display, 1, globalConfiguration.Configuration.cabinet.forcedShader, screen.config());
        else if (!string.IsNullOrEmpty(screen.shader))
            shader = ShaderScreen.Factory(display, 1, screen.shader, screen.config());
        else
            shader = ShaderScreen.Factory(display, 1, "crt", screen.config());

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
        videoShader = ShaderScreen.Factory(display, 1, videoShaderName, videoShaderConfig);

        ConfigManager.WriteConsole($"[LibretroScreenController.Start] {name} game shader created: {shader} video shader: {videoShader}");

        // age basic ---------------------
        if (ageBasicInformation != null)
        {
            cabinetAGEBasic.Init(ageBasicInformation, PathBase, cabinet, CoinSlot, lightGunTarget);
            cabinetAGEBasic.ExecAfterLoadBas();
        }

        // glow light
        light = GetComponentInChildren<Light>(true);
        screenLightON = light != null && globalConfiguration.Configuration.cabinet.screenGlowIntensity > 0;

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

    IEnumerator runBT()
    {
        // LibretroMameCore.WriteConsole($"[LibretroScreenController.runBT] coroutine BT cicle Start {gameObject.name}");

        videoPlayer.setVideo(GameVideoFile, videoShader, GameVideoInvertX, GameVideoInvertY);
        audioPlayer.path = GameAudioFile;

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
              .Condition("Not running any game", () => !LibretroMameCore.GameLoaded)
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
                          light.gameObject.SetActive(true);
                  }

                  //start mame
                  ConfigManager.WriteConsole($"[LibretroScreenController] Start game: {GameFile} in screen {name} +_+_+_+_+_+_+_+__+_+_+_+_+_+_+_+_+_+_+_+_");
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
                  //   ConfigManager.WriteConsole($"[LibretroScreenController] controller configuration as markdown in the next line:");
                  //   ConfigManager.WriteConsole(controlConf.AsMarkdown());
                  libretroControlMap.CreateFromConfiguration(controlConf);
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
                      light.gameObject.SetActive(false);
                  }
                  return TaskStatus.Success;
              })
            .End()

            .Selector("Video/Audio Player control")
                .Sequence()
                    .Condition("Running any game or Player not in the zone?", () => LibretroMameCore.GameLoaded || !playerInTheZone)
                    .Do("Stop video and audio player", () =>
                    {
                        videoPlayer.Stop();
                        audioPlayer.Stop();
                        return TaskStatus.Success;
                    })
                .End()
                .Sequence()
                    .Condition("Player in the zone?", () => playerInTheZone)
                    .Condition("Not running any game", () => !LibretroMameCore.GameLoaded)
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
                light.color = new Color(r,g,b);
                float luminance = (r + g + b) / 3f;
                light.intensity = luminance * globalConfiguration.Configuration.cabinet.screenGlowIntensity;
            }

            LibretroMameCore.UpdateTexture();

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
            LibretroMameCore.MoveAudioStreamTo(data);
    }

    private void OnDestroy()
    {
        if (LibretroMameCore.isRunning(ScreenName, GameFile))
            PreparePlayerToPlayGame(false);

        LibretroMameCore.End(ScreenName, GameFile);
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