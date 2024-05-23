/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/


//#define _serialize_
#define _debug_

using System.Collections;
using UnityEngine;
using System;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;
using LC = LibretroControlMapDictionnary;
using CM = ControlMapPathDictionary;

#if UNITY_EDITOR
using UnityEditor;
#endif


[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(LibretroControlMap))]
[RequireComponent(typeof(basicAGE))]
[RequireComponent(typeof(CabinetAGEBasic))]
[RequireComponent(typeof(LightGunTarget))]
public class AGEBasicCabinetController : MonoBehaviour
{
    
    [SerializeField]
    public BehaviorTree tree;

    [SerializeField]
    [Tooltip("Path that holds cabinet information.")]
    public string PathBase;
    public Cabinet cabinet;

    public bool SimulateExitGame;

    // [Tooltip("The global action manager in the main rig. We will find one if not set.")]
    // public InputActionManager inputActionManager;

    private GameObject player;
    private ChangeControls changeControls;
    private CoinSlotController CoinSlot;
    //controls
    private LibretroControlMap libretroControlMap;
    public ControlMapConfiguration CabinetControlMapConfig = null;

    // private GameObject cabinet;
    private CabinetReplace cabinetReplace;

    //age basic
    public CabinetAGEBasicInformation ageBasicInformation;
    private CabinetAGEBasic cabinetAGEBasic;
    public BackgroundSoundController backgroundSoundController;

    private Coroutine mainCoroutine;
    private bool initialized = false;
    private bool CoinWasInserted = false;

    private DateTime timeToExit = DateTime.MinValue;
    public int SecondsToWaitToExit = 2;

    private LightGunTarget lightGunTarget;
    public LightGunInformation lightGunInformation;

    private CoinSlotController getCoinSlotController()
    {
        Transform coinslot = cabinet?.gameObject?.transform.Find("coin-slot-added");
        if (coinslot == null)
            return null;

        return coinslot.gameObject.GetComponent<CoinSlotController>();
    }

    
    // Start is called before the first frame update
    void Start()
    {
        LibretroMameCore.WriteConsole($"[AGEBasicCabinetController.Start] {name}");

        cabinetAGEBasic = GetComponent<CabinetAGEBasic>();
        libretroControlMap = GetComponent<LibretroControlMap>();

        CoinSlot = getCoinSlotController();
        if (CoinSlot == null)
            ConfigManager.WriteConsoleError($"[AGEBasicCabinetController.Start] {name} Coin Slot not found in cabinet !!!! no one can play this game.");

        player = GameObject.Find("OVRPlayerControllerGalery");
        changeControls = player.GetComponent<ChangeControls>();
        lightGunTarget = GetComponent<LightGunTarget>();

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
            ConfigManager.WriteConsole($"[AGEBasicCabinetController.setupActionMap] map loaded with a CustomControlMap (usually cabinet configuration)");
            controlConf = new CustomControlMap(CabinetControlMapConfig);
        }
        else if (!string.IsNullOrEmpty(cabinetReplace?.game?.CabinetDBName) &&
                   GameControlMap.ExistsConfiguration(cabinetReplace.game.CabinetDBName))
        {
            ConfigManager.WriteConsole($"[AGEBasicCabinetController.setupActionMap] loading user controller configuration, GameControlMap: {cabinetReplace.game.CabinetDBName}");
            controlConf = new GameControlMap(cabinetReplace.game.CabinetDBName);
        }
        else
        {
            ConfigManager.WriteConsole($"[AGEBasicCabinetController.setupActionMap] no controller user configuration, no cabinet configuration, using GlobalControlMap");
            controlConf = new GlobalControlMap();
        }

        //   ConfigManager.WriteConsole($"[AGEBasicCabinetController] controller configuration as markdown in the next line:");
        //   ConfigManager.WriteConsole(controlConf.AsMarkdown());
#if UNITY_EDITOR
        controlConf.AddMap(LC.KEYB_UP, CM.KEYBOARD_W);
        controlConf.AddMap(LC.KEYB_DOWN, CM.KEYBOARD_S);
        controlConf.AddMap(LC.KEYB_LEFT, CM.KEYBOARD_A);
        controlConf.AddMap(LC.KEYB_RIGHT, CM.KEYBOARD_D);
#endif
        libretroControlMap.CreateFromConfiguration(controlConf);
    }

    IEnumerator runBT()
    {
        yield return new WaitForEndOfFrame();

        cabinetAGEBasic.Init(ageBasicInformation, PathBase, cabinet, CoinSlot);
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

    private BehaviorTree buildScreenBT()
    {
        return new BehaviorTreeBuilder(gameObject).
          Selector()
            .Sequence("Start the agebasic screen")
              .Condition("AGEBasic is active?", () => ageBasicInformation.active)
              .Condition("CoinSlot is present", () => CoinSlot != null)
              .Condition("Not initialized?", () => !CoinWasInserted)
              .Condition("There are coins", () => CoinSlot.hasCoins())
              .Do("Start AGEBasic", () =>
              {

                  //start mame
                  ConfigManager.WriteConsole($"[AGEBasicCabinetController] in screen {name} +_+_+_+_+_+_+_+__+_+_+_+_+_+_+_+_+_+_+_+_");

                  //controllers (lazy load)
                  setupActionMap();

                  //change hands
                  PreparePlayerToRunPrograms(true);

                  // Ligth guns configuration (lazy load)
                  if (lightGunTarget != null && lightGunInformation != null && !lightGunTarget.Initialized())
                  {
                      lightGunTarget.Init(lightGunInformation, PathBase);
                      changeControls.ChangeRightJoystickModelLightGun(lightGunTarget, true);
                  }

                  CoinWasInserted = true;
                  CoinSlot.clean();

                  return TaskStatus.Success;
              })
            .End()

            .Sequence("AGEBasic running control")
              .Condition("AGEBasic is active?", () => ageBasicInformation.active)
              .Condition("Coin inserted?", () => CoinWasInserted)
              .Condition("A program is not running?", () => !cabinetAGEBasic.AGEBasic.IsRunning())
              .Do("Run main program", () =>
              {

                  cabinetAGEBasic.ExecInsertCoinBas();

                  CoinWasInserted = false;

                  return TaskStatus.Success;
              })
              .Sequence("AGEBasic Running")
                .RepeatUntilSuccess("Until player exit")
                    .Sequence()
                        .Condition("user EXIT pressed?", () =>
                        {
                            if (libretroControlMap.Active(LC.EXIT) == 1)
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
              .Do("END Program", () =>
              {
                  // age basic leave
                  cabinetAGEBasic.StopInsertCoinBas(); //force
                  cabinetAGEBasic.ExecAfterLeaveBas();

                  EndPlayerActivities();
#if UNITY_EDITOR
                  SimulateExitGame = false;
#endif
                  return TaskStatus.Success;
              })
            .End()

          .End()
        .Build();
    }

    void EndPlayerActivities()
    {
        //to replace the shader texture ASAP:
        timeToExit = DateTime.MinValue;

        PreparePlayerToRunPrograms(false);
        libretroControlMap.Clean();
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


    public void CalculateLightGunPosition()
    {
        if (lightGunTarget == null ||
            lightGunInformation == null ||
            !lightGunInformation.active ||
            !lightGunTarget.Initialized())
            return;

        lightGunTarget.Shoot();
    }

    public void Update()
    {
        if (cabinetAGEBasic.AGEBasic != null && cabinetAGEBasic.AGEBasic.IsRunning())
            CalculateLightGunPosition();

        return;
    }

#if UNITY_EDITOR
    public void InsertCoin()
    {
        CoinSlot.insertCoin();
    }
    public void ExitProgram()
    {
        ConfigManager.WriteConsole("[AGEBasicCabinetController] EXIT PROGRAM by button ------ ");
        SimulateExitGame = true;
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(AGEBasicCabinetController))]
public class AGEBasicControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AGEBasicCabinetController myScript = (AGEBasicCabinetController)target;
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