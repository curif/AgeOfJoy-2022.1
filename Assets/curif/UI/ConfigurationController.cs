using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using UnityEngine;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;
using LC = LibretroControlMapDictionnary;
using CM = ControlMapPathDictionary;
using static ConfigInformation;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class ConfigurationHelper
{
    private GlobalConfiguration globalConfiguration = null;
    private RoomConfiguration roomConfiguration = null;

    public ConfigurationHelper(GlobalConfiguration globalConfiguration, RoomConfiguration roomConfiguration)
    {
        this.roomConfiguration = roomConfiguration;
        this.globalConfiguration = globalConfiguration;

        if (globalConfiguration == null)
        {
            GameObject GlobalConfigurationGameObject = GameObject.Find("FixedGlobalConfiguration");
            this.globalConfiguration = GlobalConfigurationGameObject.GetComponent<GlobalConfiguration>();
        }
        if (this.globalConfiguration == null)
            throw new ArgumentException("[ConfigurationHelper] global configuration not found.");

        ConfigManager.WriteConsole($"[ConfigurationHelper] global: {this.globalConfiguration.yamlPath} room: {this.roomConfiguration?.yamlPath}");
    }

    public ConfigInformation getConfigInformation(bool isGlobal)
    {
        ConfigInformation config;

        if (!isGlobal && roomConfiguration == null)
            throw new ArgumentException("Room configuration is missing in cabinet configuration screen gameobject ");

        if (!isGlobal)
        {
            config = roomConfiguration.Configuration;

            if (config == null)
            {
                roomConfiguration.Configuration = ConfigInformation.newDefault();
                config = roomConfiguration.Configuration;
            }
        }
        else
        {
            config = globalConfiguration.Configuration;
            if (config == null)
            {
                globalConfiguration.Configuration = ConfigInformation.newDefault();
                config = globalConfiguration.Configuration;
            }
        }

        return config;
    }
    public bool CanConfigureRoom()
    {
        // ConfigManager.WriteConsole($"[ConfigurationHelper.CanConfigureRoom] room: {roomConfiguration?.yamlPath}");
        return roomConfiguration != null;
    }

    public void Save(bool saveGlobal, ConfigInformation config)
    {
        /*
        string yamlPath;
        if (saveGlobal)
            yamlPath = globalConfiguration.yamlPath;
        else
            yamlPath = roomConfiguration.yamlPath;
        ConfigManager.WriteConsole($"[ConfigurationController] save configuration is global:{saveGlobal} {config} to {yamlPath}");
        config.ToYaml(yamlPath); //saving the file will reload the configuration in GlobalConfiguration and RoomConfiguration by trigger.
*/
        if (saveGlobal)
        {
            globalConfiguration.Configuration = config;
            globalConfiguration.Save();
        }
        else
        {
            roomConfiguration.Configuration = config;
            roomConfiguration.Save();
        }
        return;
    }

    public void Reset(bool isGlobal)
    {
        if (isGlobal)
        {
            globalConfiguration.Reset();
        }
        else
        {
            roomConfiguration.Reset();
        }
    }
}

[RequireComponent(typeof(Teleportation))]
public class ConfigurationController : MonoBehaviour
{
    //public static readonly string UP = ScreenGeneratorFont.STR_GLYPH_UP;
    //public static readonly string DOWN = ScreenGeneratorFont.STR_GLYPH_DOWN;
    //public static readonly string LEFT = ScreenGeneratorFont.STR_GLYPH_LEFT;
    //public static readonly string RIGHT = ScreenGeneratorFont.STR_GLYPH_RIGHT;

    public static readonly string UP = ScreenGeneratorFont.STR_GLYPH_DPAD_UP;
    public static readonly string DOWN = ScreenGeneratorFont.STR_GLYPH_DPAD_DOWN;
    public static readonly string LEFT = ScreenGeneratorFont.STR_GLYPH_DPAD_LEFT;
    public static readonly string RIGHT = ScreenGeneratorFont.STR_GLYPH_DPAD_RIGHT;

    public static readonly string OK = ScreenGeneratorFont.STR_GLYPH_BUTTON_B;

    public readonly string UDLR_TO_CHANGE = UP + "/" + DOWN + "/" + LEFT + "/" + RIGHT + " to change";
    public readonly string UD_TO_CHANGE = UP + "/" + DOWN + " to change";
    public readonly string LR_TO_CHANGE = LEFT + "/" + RIGHT + " to change";
    public readonly string B_TO_SELECT = OK + " to select";

    public ScreenGenerator scr;
    public CoinSlotController CoinSlot;

    [SerializeField]
    public LibretroControlMap libretroControlMap;
    // [Tooltip("The global action manager in the main rig. We will find one if not set.")]
    // public InputActionManager inputActionManager;

    [Tooltip("We will find the correct one")]
    public ChangeControls changeControls;

    [Tooltip("Load with the cabinet's parent gameobject controller")]
    public CabinetsController cabinetsController;

    [Tooltip("Set only to change room configuration, if not setted will use the Global")]
    public RoomConfiguration roomConfiguration;
    [Tooltip("Set to change the Global or the system will find it")]
    public GlobalConfiguration globalConfiguration;

    [Tooltip("AGEBasic engine, will find one if not set.")]
    public basicAGE AGEBasic;

    [Tooltip("Applies only for room configuration")]
    public bool canChangeAudio = true;
    [Tooltip("Applies only for room configuration")]
    public bool canChangeNPC = true;
    [Tooltip("Applies only for room configuration")]
    public bool canChangeControllers = true;
    [Tooltip("Applies only for room configuration")]
    public bool canChangeCabinets = true;

    //Teleport
    public bool canTeleport = true;
    private SceneDatabase sceneDatabase;
    private Teleportation teleportation;
    private GenericOptions teleportDestination;
    private GenericTimedLabel teleportResult;
    private GenericWidgetContainer teleportContainer;

    //locomotion
    private GenericWidgetContainer locomotionContainer;
    private GenericBool locomotionTeleportOn, locomotionSnapTurnOn;
    private GenericOptionsInteger locomotionSpeed;
    private GenericOptionsInteger locomotionTurnSpeed, locomotionSnapTurnAmount;

    //AGEBasic
    private GenericWidgetContainer AGEBasicContainer;
    private GenericOptions AGEBasicPrograms;
    private DateTime AGEBasicRunTimeout;
    private CompilationException AGEBasicCompilationException;
    private bool AGEBasicWaitForPressAKey = false;


    [Header("Tree")]
    [SerializeField]
    public BehaviorTree tree;

    private GenericMenu mainMenu;
    private enum StatusOptions
    {
        init,
        waitingForCoin,
        onMainMenu,
        onBoot,
        onNPCMenu,
        onAudio,
        onChangeMode,
        onChangeController,
        onChangeCabinets,
        onTeleport,
        onReset,
        onChangeLocomotion,
        onChangePlayer,
        onRunAGEBasic,
        onRunAGEBasicRunning,
        onCabinet,
        exit
    }
    private StatusOptions status;
    private BootScreen bootScreen;
    // private LibretroMameCore.Waiter onBootDelayWaiter;

    private string NPCStatus;
    private GenericOptions NPCStatusOptions;

    private GenericBool isGlobalConfigurationWidget;
    private GenericWidgetContainer changeModeContainer, audioContainer, resetContainer, controllerContainer, npcContainer;
    private GenericLabel lblGameSelected;
    private GenericOptions controlMapGameId, controlMapMameControl;
    private GenericOptionsInteger controlMapPort;
    private GenericTimedLabel controlMapSavedLabel;
    private List<GenericOptions> controlMapRealControls;
    private ControlMapConfiguration controlMapConfiguration;

    private GenericWidgetContainer cabinetsReplacementToChangeContainer;
    private GenericOptions cabinetToReplace;
    private GenericOptions cabinetReplaced;
    private GenericTimedLabel cabinetReplacementSavedLabel;

    private GenericWidgetContainer cabinetsConfigurationContainer;
    private GenericOptions cabinetWorldResolution, cabinetWorldFoveatingLevel, cabinetGameplyResolution,
        cabinetGameplayFoveatingLevel, cabinetScreenGlowIntensity;
    private GenericTimedLabel cabinetConfigurationSavedLabel;

    private GenericLabelOnOff lblHaveRoomConfiguration, lblRoomName;

    //player
    private GenericOptions playerHeight;
    private GenericOptions playerScale;
    private GenericOptions playerSkinColor;
    private GenericWidgetContainer playerContainer;

    private ConfigurationHelper configHelper;

    private Dictionary<string, bool> inputDictionary = new Dictionary<string, bool>();
    private List<string> inputKeys;

    private ShaderScreenBase shader;
    private ShaderScreenBase shaderOffLine;

    private Renderer display;


    // Start is called before the first frame update
    void Start()
    {
        ConfigManager.WriteConsole("[ConfigurationController] start");

        gameObject.tag = "screenControlCabinet";

        if (changeControls == null)
        {
            GameObject player = GameObject.Find("OVRPlayerControllerGalery");
            changeControls = player.GetComponent<ChangeControls>();
        }

        if (AGEBasic == null)
            AGEBasic = GetComponent<basicAGE>();
        AGEBasic.ConfigurationController = this;

        if (canTeleport)
        {
            GameObject roomInit = GameObject.Find("FixedObject");
            sceneDatabase = roomInit.GetComponent<SceneDatabase>();
            teleportation = GetComponent<Teleportation>();
        }

        // GameObject inputActionManagerGameobject = GameObject.Find("Input Action Manager");
        // inputActionManager = inputActionManagerGameobject.GetComponent<InputActionManager>();

        if (CoinSlot == null)
        {
            Transform parent = transform.parent;
            if (parent != null)
            {
                // Get the CoinSlotController component from any object within the parent's children
                CoinSlot = parent.GetComponentInChildren<CoinSlotController>();
            }
        }
        if (CoinSlot == null)
            ConfigManager.WriteConsoleError("[ConfigurationController] coin slot wasn't assigned.");

        // first: wait for the room to load.
        StartCoroutine(run());
    }

    /*
    //runs before Start()
    private void OnEnable()
    {
        ConfigManager.WriteConsoleWarning("[ConfigurationController] enabled");
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        ConfigManager.WriteConsoleWarning($"[ConfigurationController] paused: {pauseStatus}");

    }
    private void OnDisable()
    {
        ConfigManager.WriteConsoleWarning($"[ConfigurationController] disabled");
    }
    */
    void UpdateInputValues()
    {
        inputDictionary["up"] = inputDictionary["up"] || ControlActive(LC.JOYPAD_UP) || ControlActive(LC.KEYB_UP);
        inputDictionary["down"] = inputDictionary["down"] || ControlActive(LC.JOYPAD_DOWN) || ControlActive(LC.KEYB_DOWN);
        inputDictionary["left"] = inputDictionary["left"] || ControlActive(LC.JOYPAD_LEFT) || ControlActive(LC.KEYB_LEFT);
        inputDictionary["right"] = inputDictionary["right"] || ControlActive(LC.JOYPAD_RIGHT) || ControlActive(LC.KEYB_RIGHT);
        inputDictionary["action"] = inputDictionary["action"] || ControlActive(LC.JOYPAD_B);
    }

    void ResetInputValues()
    {
        // Reset all input values to false
        inputKeys.ForEach(key => inputDictionary[key] = false);
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

        ControlMapConfiguration conf = new DefaultControlMap();
#if UNITY_EDITOR
        conf.AddMap(LC.KEYB_UP, CM.KEYBOARD_W);
        conf.AddMap(LC.KEYB_DOWN, CM.KEYBOARD_S);
        conf.AddMap(LC.KEYB_LEFT, CM.KEYBOARD_A);
        conf.AddMap(LC.KEYB_RIGHT, CM.KEYBOARD_D);
#endif
        libretroControlMap.CreateFromConfiguration(conf, "inputMap_ConfigurationController_" + name);
        libretroControlMap.Enable(true);
    }

    private void cleanActionMap()
    {
        libretroControlMap.Clean();
    }

    public void NPCScreenDraw()
    {
        scr.Clear();
        npcContainer.Draw();

        //some help
        scr.Print(2, 16, UDLR_TO_CHANGE);
        scr.Print(2, 17, B_TO_SELECT);

    }

    private void NPCGetStatus()
    {
        string actualNPCStatus = ConfigInformation.NPC.validStatus[0];
        ConfigInformation config = configHelper.getConfigInformation(isGlobalConfigurationWidget.value);
        if (config?.npc != null)
            actualNPCStatus = config.npc.status;

        NPCStatusOptions.SetCurrent(actualNPCStatus);
    }

    private void NPCSave()
    {
        bool isGlobal = isGlobalConfigurationWidget.value;
        ConfigInformation config = configHelper.getConfigInformation(isGlobal);
        config.npc = new();
        config.npc.status = NPCStatusOptions.GetSelectedOption();
        configHelper.Save(isGlobal, config);
    }

    public void resetWindowDraw()
    {
        scr.Clear();
        resetContainer.Draw();
        if (isGlobalConfigurationWidget.value)
        {
            scr.Print(2, 1, "Back global configuration");
            scr.Print(2, 2, "to default.");
            scr.Print(2, 3, "You will lost the global data");
            scr.Print(2, 4, "except controllers information");
            scr.Print(2, 5, "and cabinets positions.");
        }
        else
        {
            scr.Print(2, 1, "Room configuration will");
            scr.Print(2, 2, "be deleted.");
            scr.Print(2, 3, "You will lost all room configured data");
            scr.Print(2, 4, "except controllers information");
            scr.Print(2, 5, "and cabinets positions.");
        }
        scr.Print(2, 16, UD_TO_CHANGE);
        scr.Print(2, 17, B_TO_SELECT + " and exit");
    }

    public void resetSave()
    {
        configHelper.Reset(isGlobalConfigurationWidget.value);
        ConfigManager.WriteConsole($"[ConfigurationController.resetSave] Reset to default - is Global: {isGlobalConfigurationWidget.value}");
    }

    private void audioScreen()
    {
        //set the init value
        ConfigInformation config = configHelper.getConfigInformation(isGlobalConfigurationWidget.value);
        ConfigInformation.Background bkg = ConfigInformation.BackgroundDefault();
        ConfigInformation.Background ingamebkg = ConfigInformation.BackgroundInGameDefault();

        if (config?.audio?.background?.volume != null)
            ((GenericOptionsInteger)audioContainer.GetWidget("BackgroundVolume")).SetCurrent((int)config.audio.background.volume);
        else
            ((GenericOptionsInteger)audioContainer.GetWidget("BackgroundVolume")).SetCurrent((int)bkg.volume);

        if (config?.audio?.inGameBackground?.volume != null)
            ((GenericOptionsInteger)audioContainer.GetWidget("InGameBackgroundVolume")).SetCurrent((int)config.audio.inGameBackground.volume);
        else
            ((GenericOptionsInteger)audioContainer.GetWidget("InGameBackgroundVolume")).SetCurrent((int)ingamebkg.volume);

        if (config?.audio?.background?.muted != null)
            ((GenericBool)audioContainer.GetWidget("BackgroundMuted")).SetValue((bool)config.audio.background.muted);
        else
            ((GenericBool)audioContainer.GetWidget("BackgroundMuted")).SetValue((bool)bkg.muted);

        if (config?.audio?.inGameBackground?.muted != null)
            ((GenericBool)audioContainer.GetWidget("InGameBackgroundMuted")).SetValue((bool)config.audio.inGameBackground.muted);
        else
            ((GenericBool)audioContainer.GetWidget("InGameBackgroundMuted")).SetValue((bool)ingamebkg.muted);

        scr.Clear();
        audioContainer.Draw();
    }
    private void audioSave()
    {
        bool isGlobal = isGlobalConfigurationWidget.value;
        ConfigInformation config = configHelper.getConfigInformation(isGlobal);
        config.audio = new();
        config.audio.background = new(); //ConfigInformation.BackgroundDefault();
        config.audio.inGameBackground = new(); //ConfigInformation.BackgroundInGameDefault();
        config.audio.background.volume = (uint)((GenericOptionsInteger)audioContainer.GetWidget("BackgroundVolume")).GetSelectedOption();
        config.audio.background.muted = ((GenericBool)audioContainer.GetWidget("BackgroundMuted")).value;
        config.audio.inGameBackground.volume = (uint)((GenericOptionsInteger)audioContainer.GetWidget("InGameBackgroundVolume")).GetSelectedOption();
        config.audio.inGameBackground.muted = ((GenericBool)audioContainer.GetWidget("InGameBackgroundMuted")).value;

        configHelper.Save(isGlobal, config);
    }

    public void mainMenuDraw()
    {
        scr.Clear();
        mainMenu.Deselect();
        mainMenu.DrawMenu();
        if (isGlobalConfigurationWidget.value)
        {
            scr.PrintCentered(4, "- global Configuration mode -");
            scr.PrintCentered(5, "(changes affects all rooms)");
        }
        else
        {
            scr.PrintCentered(4, "- room Configuration mode -");
            if (configHelper.CanConfigureRoom())
            {
                lblRoomName.isOn = true;
                lblHaveRoomConfiguration.isOn = roomConfiguration.ExistsRoomConfiguration();
            }
            lblHaveRoomConfiguration.Draw();
            lblRoomName.Draw();
        }
    }

    public void changeModeWindowDraw()
    {
        scr.Clear();
        if (configHelper.CanConfigureRoom())
        {
            isGlobalConfigurationWidget.enabled = true;
            scr.Print(2, 1, "select the configuration mode:");
            scr.Print(2, 2, "- global for all rooms");
            scr.Print(2, 3, "- or for this room only.");
            scr.Print(2, 4, UD_TO_CHANGE);
        }
        else
        {
            isGlobalConfigurationWidget.enabled = false;
            scr.Print(2, 1, "only global configuration allowed");
            scr.Print(2, 3, "for one room configuration go to");
            scr.Print(2, 4, "a gallery room");
        }
        scr.Print(2, 19, B_TO_SELECT);

        changeModeContainer.Draw();
    }

    public void controllerContainerDraw()
    {
        if (isGlobalConfigurationWidget.value)
        {
            lblGameSelected.label = "global configuration";
        }
        else
        {
            controlMapGameId.enabled = true;
            lblGameSelected.label = controlMapGameId.GetSelectedOption();
        }
        scr.Clear();
        scr.PrintCentered(1, "- Controller configuration -");
        controllerContainer.Draw();
        scr.Print(2, 23, UDLR_TO_CHANGE);
        scr.Print(2, 24, B_TO_SELECT);
    }

    private void controlMapUpdateWidgets()
    {
        string mameControl = controlMapMameControl.GetSelectedOption();
        int port = controlMapPort.GetSelectedOption();
        ConfigManager.WriteConsole($"[controlMapUpdateWidgets] updating for mame control id {mameControl} port: {port}");

        //clean
        int idx = 0;
        for (; idx < 5; idx++)
        {
            controlMapRealControls[idx].SetCurrent("--");
        }

        ControlMapConfiguration.Maps maps = controlMapConfiguration.GetMap(mameControl, port);
        if (maps == null)
            return;

        //load
        idx = 0;
        foreach (ControlMapConfiguration.ControlMap m in maps.controlMaps)
        {
            controlMapRealControls[idx].SetCurrent(m.RealControl);
            idx++;
            if (idx > 4)
                break;
        }
    }

    private void controlMapUpdateConfigurationFromWidgets()
    {
        string mameControl = controlMapMameControl.GetSelectedOption();
        int port = controlMapPort.GetSelectedOption();

        ConfigManager.WriteConsole($"[controlMapUpdateConfigurationFromWidgets] updating from widget mame control id {mameControl}");

        controlMapConfiguration.RemoveMaps(mameControl, port);
        for (int idx = 0; idx < 5; idx++)
        {
            string realControl = controlMapRealControls[idx].GetSelectedOption();
            if (realControl != "--")
            {
                controlMapConfiguration.AddMap(mameControl, realControl, null, port);
            }
        }
    }

    private void controlMapConfigurationLoad()
    {
        try
        {
            if (isGlobalConfigurationWidget.value)
            {
                controlMapConfiguration = new GlobalControlMap();
            }
            else
            {
                controlMapConfiguration = new GameControlMap(controlMapGameId.GetSelectedOption());
            }
        }
        catch (Exception e)
        {
            controlMapConfiguration = new DefaultControlMap();
            ConfigManager.WriteConsoleException($"[controllerLoadConfigMap] loading configuration, using default. Is Global:{isGlobalConfigurationWidget.value} ", e);
        }
        // ConfigManager.WriteConsole($"[controllerLoadConfigMap] debug in the next line ...");
        // controlMapConfiguration.ToDebug();
    }
    private bool controlMapConfigurationSave()
    {
        try
        {
            if (controlMapConfiguration is GlobalControlMap)
            {
                GlobalControlMap g = (GlobalControlMap)controlMapConfiguration;
                g.Save();
            }
            else if (controlMapConfiguration is GameControlMap)
            {
                GameControlMap g = (GameControlMap)controlMapConfiguration;
                g.Save();
            }
            else
            {
                ConfigManager.WriteConsoleError($"[controlMapConfigurationSave] Only can configure the Global or Game configuration controlls.");
            }
        }
        catch (Exception ex)
        {
            ConfigManager.WriteConsoleException($"[controlMapConfigurationSave] error saving configuration: {controlMapConfiguration}", ex);
            return false;
        }

        return true;
    }

    public void ScreenWaitingDraw()
    {
        scr.Clear();
        scr.PrintCentered(10, " - Wait for room setup - ");
        scr.PrintCentered(12, GetRoomDescription(), true);
    }


    private void SetMainMenuWidgets()
    {
        //main menu (Create any time because the conditionals.)
        mainMenu = new(scr, "AGE of Joy - Main configuration");
        mainMenu.AddOption("AGEBasic", "     Run AGEBasic programs       ");
        if (CanConfigureAudio())
            mainMenu.AddOption("Audio configuration", "     Change sound volume       ");
        if (canChangeNPC)
            mainMenu.AddOption("NPC configuration", "   To change the NPC behavior  ");
        if (CanConfigureControllers())
            mainMenu.AddOption("controllers", "Map your controls to play games");
        if (CanConfigureCabinets())
            mainMenu.AddOption("cabinets", " replace cabinets in the room  ");
        if (isGlobalConfigurationWidget.value)
        {
            mainMenu.AddOption("configure cabinets", " tweak cabinet's behavior  ");
            mainMenu.AddOption("locomotion", " player movement configuration  ");
            mainMenu.AddOption("player", " player configuration  ");
        }
        mainMenu.AddOption("change mode (global/room)", "  global or room configuration ");
        mainMenu.AddOption("reset", "         back to default       ");
        mainMenu.AddOption("teleport", "       teleport to a room       ");
        mainMenu.AddOption("exit", "        exit configuration     ");

    }

    private void SetAudioWidgets()
    {
        if (audioContainer != null)
            return;

        //audio
        audioContainer = new(scr, "audioContainer");
        audioContainer.Add(new GenericWindow(scr, 2, 4, "audiowin", 36, 14, " Audio Configuration "))
                      .Add(new GenericLabel(scr, "BackgroundLabel", "Background Audio", 4, 6))
                      .Add(new GenericBool(scr, "BackgroundMuted", "mute:", false, 6, 8))
                      .Add(new GenericOptionsInteger(scr, "BackgroundVolume", "volume:", 0, 100, 6, 9))
                      .Add(new GenericLabel(scr, "InGameBackgroundLabel", "Background in game audio", 4, 11))
                      .Add(new GenericBool(scr, "InGameBackgroundMuted", "mute:", false, 6, 13))
                      .Add(new GenericOptionsInteger(scr, "InGameBackgroundVolume", "volume:", 0, 100, 6, 14))
                      .Add(new GenericButton(scr, "save", "save & exit", 4, 16, true))
                      .Add(new GenericButton(scr, "exit", "exit", 18, 16, true))
                      .Add(new GenericLabel(scr, "l1", LEFT + "/" + RIGHT + "/" + OK + " to change", 2, 20))
                      .Add(new GenericLabel(scr, "l2", UP + "/" + DOWN + " to move", 2, 21));
    }
    private void SetGlobalWidgets()
    {
        bootScreen = new(scr);

        isGlobalConfigurationWidget = new GenericBool(scr, "isGlobal", "working with global:", !configHelper.CanConfigureRoom(), 4, 10);
        isGlobalConfigurationWidget.enabled = isGlobalConfigurationWidget.value;

        //room screen information
        lblHaveRoomConfiguration = new(scr, "haveRoom", "Room configuration exists", 1, 23, inverted: true);
        string room = "";
        if (configHelper.CanConfigureRoom() && roomConfiguration != null)
            room = roomConfiguration.GetName();
        lblRoomName = new(scr, "roomid", room, 1, 22, inverted: false);
    }

    private void SetChangeModeWidgets()
    {
        if (changeModeContainer != null)
            return;

        //change mode
        changeModeContainer = new(scr, "changeMode");
        changeModeContainer.Add(new GenericWindow(scr, 2, 8, "win", 36, 6, " mode "))
                           .Add(isGlobalConfigurationWidget)
                           .Add(new GenericButton(scr, "exit", "exit", 4, 11, true));
    }

    private void SetResetWidgets()
    {
        if (resetContainer == null)
        {
            //reset options
            resetContainer = new(scr, "reset");
            resetContainer.Add(new GenericWindow(scr, 2, 8, "win", 36, 6, " reset "))
                          .Add(new GenericButton(scr, "reset", "delete and exit", 4, 11, true))
                          .Add(new GenericButton(scr, "exit", "exit", 4, 12, true));
        }
        resetContainer.SetOption(2); //safe in exit.
    }

    private void SetNPCWidgets()
    {
        if (NPCStatusOptions != null)
            return;

        NPCStatusOptions = new(scr, "npc", "NPC Behavior:", new List<string>(ConfigInformation.NPC.validStatus), 4, 8);
        npcContainer = new(scr, "npc");
        npcContainer.Add(new GenericWindow(scr, 2, 6, "npcWindow", 36, 8, " NPC Configuration ", true))
                    .Add(NPCStatusOptions)
                    .Add(new GenericButton(scr, "save", "save and exit", 4, 10, true))
                    .Add(new GenericButton(scr, "exit", "exit", 4, 11, true));
    }

    private void SetControlMapWidgets()
    {
        if (lblGameSelected != null)
        {
            //adjust widgets
            // controlMapGameId.SetOptions(GetCabinetsInRoom());
            controlMapGameId.SetOptions(cabinetsController.gameRegistry.GetCabinetsNamesAssignedToRoom(GetRoomName()));
            controlMapGameId.enabled = !isGlobalConfigurationWidget.value;
            return;
        }

        //controllers
        //Game selection
        // ---- title   : 4
        // | lblGame    : 6
        // |  lblCtlr   : 7
        // | option     : 9


        lblGameSelected = new GenericLabel(scr, "lblGame", "global configuration", 3, 6);
        controlMapGameId = new GenericOptions(scr, "gameId", "game:",
                                                cabinetsController?.gameRegistry?.GetCabinetsNamesAssignedToRoom(GetRoomName()),
                                                3, 9);

        //global configuration by default, changed in the first draw()
        controlMapGameId.enabled = false;
        controlMapMameControl = new GenericOptions(scr, "mameControl", "CTRL:",
                                                    LibretroMameCore.deviceIdsCombined, 3, 10);
        controlMapPort = new GenericOptionsInteger(scr, "controlMapPort",
                                                    "Port:", 0, 10,
                                                    3, 11);
        controlMapRealControls = new();
        List<string> controlMapRealControlList = new List<string>();
        controlMapRealControlList.Add("--");
        controlMapRealControlList = controlMapRealControlList.Concat(ControlMapPathDictionary.getList()).ToList();
        controlMapSavedLabel = new(scr, "saved", "saved", 3, 19, true);
        controllerContainer = new(scr, "controllers");
        controllerContainer.Add(new GenericWindow(scr, 1, 4, "controllerscont", 39, 18, " controllers "))
                            .Add(lblGameSelected)
                            .Add(controlMapGameId)
                            .Add(controlMapMameControl)
                            .Add(controlMapPort);

        for (int x = 0; x < 5; x++)
        {
            controlMapRealControls.Add(new GenericOptions(scr, "controlMapRealControl-" + x.ToString(),
                                        x.ToString() + ":", controlMapRealControlList, 3, 12 + x));
            controllerContainer.Add(controlMapRealControls[x]);
        }
        controllerContainer.Add(new GenericButton(scr, "save", "save", 3, 18, true))
                           .Add(new GenericButton(scr, "exit", "exit", 10, 18, true))
                           .Add(controlMapSavedLabel);
    }

    public string GetRoomName()
    {
        if (roomConfiguration != null)
            return roomConfiguration.GetName();
        else if (cabinetsController != null)
            return cabinetsController.Room;
        return name;
    }
    public string GetRoomDescription()
    {
        return sceneDatabase.FindByName(GetRoomName())?.Description ?? string.Empty;
    }

    private List<string> GetCabinetsInRoom()
    {
        List<string> cabsWithPosition = new List<string>();
        string room = GetRoomName();

        if (cabinetsController?.gameRegistry == null)
        {
            ConfigManager.WriteConsoleWarning($"[GetCabinetsInRoom] no gameRegistry loaded for {room}");
            return new List<string>();
        }

        List<CabinetPosition> cabinetsInRoomByGameRegistry =
                        cabinetsController.gameRegistry.GetCabinetsAndPositionsAssignedToRoom(room);
        if (cabinetsInRoomByGameRegistry != null)
        {
            ConfigManager.WriteConsole($"[GetCabinetsInRoom] there are {cabinetsInRoomByGameRegistry.Count} cabinets in {room} and {cabinetsController?.gameRegistry?.CabinetsInRegistry} cabinets in the main registry");
            foreach (CabinetPosition cabPos in cabinetsInRoomByGameRegistry)
            {
                cabsWithPosition.Add($"{cabPos.Position:D3}-{cabPos.CabinetDBName}");
            }
        }

        //create empty positions
        List<int> freePos = cabinetsController.gameRegistry.GetFreePositions(
            cabinetsInRoomByGameRegistry,
            cabinetsController.CabinetsCount);
        foreach (int free in freePos)
        {
            cabsWithPosition.Add($"{free:D3}-(free)");
        }

        //sort
        cabsWithPosition.Sort();

        return cabsWithPosition;
    }
    private List<string> GetAllCabinets()
    {
        List<string> cabinetsAll = cabinetsController?.gameRegistry?.GetAllCabinetsName();
        if (cabinetsAll != null)
            cabinetsAll.Sort();
        return cabinetsAll;
    }

    private void SetCabinetsReplacementWidgets()
    {
        if (!CanConfigureCabinets())
            return;

        if (cabinetToReplace != null)
        {
            cabinetToReplace.SetOptions(GetCabinetsInRoom());
            return;
        }

        GenericLabel lblRoomName = new GenericLabel(scr, "lblRoomName", GetRoomName(), 4, 6);
        cabinetToReplace = new GenericOptions(scr, "cabinetToReplace", "replace:", GetCabinetsInRoom(), 4, 8, maxLength: 26);
        cabinetReplaced = new GenericOptions(scr, "cabinetReplaced", "with:", GetAllCabinets(), 4, 9, maxLength: 26);
        cabinetReplacementSavedLabel = new(scr, "saved", "cabinet replaced", 3, 19, true);

        cabinetsReplacementToChangeContainer = new(scr, "cabinetsToChangeContainer");
        cabinetsReplacementToChangeContainer.Add(new GenericWindow(scr, 2, 4, "cabswin", 37, 12, " replace cabinets "))
                                .Add(lblRoomName)
                                .Add(cabinetToReplace)
                                .Add(cabinetReplaced)
                                .Add(new GenericButton(scr, "save", "save", 4, 11, true))
                                .Add(new GenericButton(scr, "exit", "exit", 4, 12, true))
                                .Add(cabinetReplacementSavedLabel);
    }
    public void CabinetsExtractNumberAndName(out int number, out string name)
    {
        string input = cabinetToReplace.GetSelectedOption();
        if (string.IsNullOrEmpty(input))
        {
            number = -1;
            name = "";
            return;
        }
        if (int.TryParse(input.Substring(0, 3), out number))
        {
            name = input.Substring(4);
        }
        else
        {
            number = -1;
            name = "";
        }
    }

    private void SaveCabinetPositions()
    {
        int position;
        string cabinetDBName;
        string room = GetRoomName();
        CabinetsExtractNumberAndName(out position, out cabinetDBName); //the name doesn't care.
        cabinetDBName = cabinetReplaced.GetSelectedOption();
        ConfigManager.WriteConsole($"[SaveCabinetPositions] new replacement in pos:{position} by cabinet: {cabinetDBName} room: {room}");
        // free cabinets dont have a CabinetReplace component but a CabinetController
        cabinetsController.Replace(position, room, cabinetDBName);
    }

    // ---------------------------------------------
    private void CabinetConfigurationWindowDraw()
    {
        if (cabinetsConfigurationContainer == null)
            return;

        scr.Clear();
        cabinetsConfigurationContainer.Draw();
        scr.Print(2, 23, UDLR_TO_CHANGE);
        scr.Print(2, 24, B_TO_SELECT);
    }

    private void SetCabinetsConfigurationWidgets()
    {
        List<string> resolutionMultiplier = CabinetConfigurationResolution.resolutions();
        List<string> levels = CabinetConfigurationResolution.foveatedLevels();
        List<string> glow = CabinetConfiguration.GlowIntensities();
        List<string> shaders = ShaderScreen.list();
        shaders.Insert(0, "");

        cabinetsConfigurationContainer = new(scr, "cabinetsConfigurationContainer");
        cabinetsConfigurationContainer.Add(new GenericWindow(scr, 1, 4, "cabswin", 37, 19, " Cabinet configuration "))
            .Add(new GenericLabel(scr, "lbl0", "World graphics settings", 4, 6))
            .Add(new GenericOptions(scr, "cabinetWorldResolution", "Resolution:", resolutionMultiplier, 4,                                                 cabinetsConfigurationContainer.lastYAdded + 1))
            .Add(new GenericOptions(scr, "cabinetWorldFoveatingLevel", "Foveating Level:", levels, 4, 
                        cabinetsConfigurationContainer.lastYAdded + 1))

            .Add(new GenericLabel(scr, "lbl1", "Gameplay graphics settings", 4, cabinetsConfigurationContainer.lastYAdded + 2))
            .Add(new GenericOptions(scr, "cabinetGameplayResolution", "resolution:", resolutionMultiplier, 4,                                        cabinetsConfigurationContainer.lastYAdded + 1))
            .Add(new GenericOptions(scr, "cabinetGameplayFoveatingLevel", "Foveating Level:", levels, 4, cabinetsConfigurationContainer.lastYAdded + 1))
            
            .Add(new GenericOptions(scr, "glowLevel", "Cabinet screen glow:", glow, 4, cabinetsConfigurationContainer.lastYAdded + 2))
            .Add(new GenericOptions(scr, "forceShader", "Force Shader:", shaders, 4, cabinetsConfigurationContainer.lastYAdded + 2))
            .Add(new GenericBool(scr, "insertCoinStartup", "Insert coin on startup:", false, 4, cabinetsConfigurationContainer.lastYAdded + 2))
            
            .Add(new GenericButton(scr, "save", "save", 4, cabinetsConfigurationContainer.lastYAdded + 2, true))
            .Add(new GenericButton(scr, "exit", "exit", 10, cabinetsConfigurationContainer.lastYAdded, true))
            .Add(new GenericButton(scr, "reset", "reset", 16, cabinetsConfigurationContainer.lastYAdded, true))
            .Add(new GenericTimedLabel(scr, "saved", "Change applies next game start", 4, cabinetsConfigurationContainer.lastYAdded + 1, true));
    }

    private void CabinetConfigurationSetWidgetsValues()
    {
        ConfigInformation config = configHelper.getConfigInformation(true);

        if (config.cabinet.worldResolution == null)
        {
            ((GenericOptions)cabinetsConfigurationContainer.GetWidget("cabinetWorldResolution")).SetCurrent(DeviceController.WorldScale.ToString());
            ((GenericOptions)cabinetsConfigurationContainer.GetWidget("cabinetWorldFoveatingLevel")).SetCurrent(DeviceController.WorldFovLevel.ToString());
        }
        else
        {
            ((GenericOptions)cabinetsConfigurationContainer.GetWidget("cabinetWorldResolution")).SetCurrent(config.cabinet.worldResolution.resolution.ToString());
            ((GenericOptions)cabinetsConfigurationContainer.GetWidget("cabinetWorldFoveatingLevel")).SetCurrent(config.cabinet.worldResolution.foveatedLevelAsString);
        }

        if (config.cabinet.ingameResolution == null)
        {
            ((GenericOptions)cabinetsConfigurationContainer.GetWidget("cabinetGameplayResolution")).SetCurrent(DeviceController.GameScale.ToString());
            ((GenericOptions)cabinetsConfigurationContainer.GetWidget("cabinetGameplayFoveatingLevel")).SetCurrent(DeviceController.GameFovLevel.ToString());
        }
        else
        {
            ((GenericOptions)cabinetsConfigurationContainer.GetWidget("cabinetGameplayResolution")).SetCurrent(config.cabinet.ingameResolution.resolution.ToString());
            ((GenericOptions)cabinetsConfigurationContainer.GetWidget("cabinetGameplayFoveatingLevel")).SetCurrent(config.cabinet.ingameResolution.foveatedLevelAsString);
        }

        ((GenericOptions)cabinetsConfigurationContainer.GetWidget("glowLevel")).SetCurrent(config.cabinet.screenGlowIntensity.ToString());
        
        if (string.IsNullOrEmpty(config.cabinet.forcedShader))
            ((GenericOptions)cabinetsConfigurationContainer.GetWidget("forceShader")).SetCurrent("");
        else
            ((GenericOptions)cabinetsConfigurationContainer.GetWidget("forceShader")).SetCurrent(config.cabinet.forcedShader);
            
        ((GenericBool)cabinetsConfigurationContainer.GetWidget("insertCoinStartup")).SetValue(config.cabinet.insertCoinOnStartup);

    }


    private void CabinetConfigurationReset()
    {
        ConfigInformation config = configHelper.getConfigInformation(true);

        DeviceController.ResetValues();
        config.cabinet.ingameResolution = null;
        config.cabinet.worldResolution = null;
        config.cabinet.insertCoinOnStartup = CabinetConfiguration.insertCoinOnStartupDefault;
        config.cabinet.forcedShader = CabinetConfiguration.forcedShaderDefault;
        config.cabinet.screenGlowIntensity = CabinetConfiguration.screenGlowIntensityDefault;
        configHelper.Save(true, config);
    }


    private void SaveCabinetConfiguration()
    {
        if (!isGlobalConfigurationWidget.value)
            return;

        ConfigInformation config = configHelper.getConfigInformation(true);

        config.cabinet.worldResolution = new();
        config.cabinet.worldResolution.foveatedLevelAsString = ((GenericOptions)cabinetsConfigurationContainer.GetWidget("cabinetWorldFoveatingLevel")).GetSelectedOption();
        config.cabinet.worldResolution.resolution = float.Parse(((GenericOptions)cabinetsConfigurationContainer.GetWidget("cabinetWorldResolution")).GetSelectedOption());

        config.cabinet.ingameResolution = new();
        config.cabinet.ingameResolution.foveatedLevelAsString = ((GenericOptions)cabinetsConfigurationContainer.GetWidget("cabinetGameplayFoveatingLevel")).GetSelectedOption();
        config.cabinet.ingameResolution.resolution = float.Parse(((GenericOptions)cabinetsConfigurationContainer.GetWidget("cabinetGameplayResolution")).GetSelectedOption());

        config.cabinet.screenGlowIntensity = float.Parse(((GenericOptions)cabinetsConfigurationContainer.GetWidget("glowLevel")).GetSelectedOption());
        
        string forceShader = ((GenericOptions) cabinetsConfigurationContainer.GetWidget("forceShader")).GetSelectedOption();
        if (forceShader == "")
            config.cabinet.forcedShader = null;
        else
            config.cabinet.forcedShader = forceShader;

        config.cabinet.insertCoinOnStartup = ((GenericBool)cabinetsConfigurationContainer.GetWidget("insertCoinStartup")).value;

        configHelper.Save(true, config);
    }

    // ---------------------------------------------



    private bool CanConfigureCabinets()
    {
        if (isGlobalConfigurationWidget.value)
            return false;

        return canChangeCabinets && configHelper.CanConfigureRoom() && cabinetsController?.gameRegistry != null;
    }
    private bool CanConfigureControllers()
    {
        if (isGlobalConfigurationWidget.value)
            return true;

        return canChangeControllers && configHelper.CanConfigureRoom() && cabinetsController?.gameRegistry != null;
    }
    private bool CanConfigureAudio()
    {
        if (isGlobalConfigurationWidget.value)
            return true;

        return canChangeAudio && configHelper.CanConfigureRoom() && cabinetsController?.gameRegistry != null;
    }

    private void CabinetsReplacementWindowDraw()
    {
        if (cabinetsReplacementToChangeContainer == null)
            return;

        scr.Clear();
        cabinetsReplacementToChangeContainer.Draw();
        scr.Print(2, 23, UDLR_TO_CHANGE);
        scr.Print(2, 24, B_TO_SELECT);
    }


    private void SetTeleportWidgets()
    {
        if (!canTeleport)
            return;

        if (teleportDestination != null)
            return;

        teleportDestination = new GenericOptions(scr, "teleportdest",
                                    "des:", sceneDatabase.GetTeleportationDestinationRoomDescritions(),
                                    4, 7, maxLength: 26);
        teleportResult = new GenericTimedLabel(scr, "teleportResult", "Teleport failed", 4, 12, true);
        teleportContainer = new(scr, "teleportContainer");
        teleportContainer.Add(new GenericWindow(scr, 2, 4, "teleportwin", 37, 12,
                                        " teleportation "))
                            .Add(new GenericLabel(scr, "lbl", "select destination", 4, 6))
                            .Add(teleportDestination)
                            .Add(new GenericButton(scr, "teleport", "teleport", 4, 11, true))
                            .Add(teleportResult)
                            .Add(new GenericButton(scr, "exit", "exit", 4, 14, true));
    }

    private void TeleportWindowDraw()
    {
        if (!canTeleport)
            return;

        scr.Clear();
        teleportContainer.Draw();
        scr.Print(2, 23, UDLR_TO_CHANGE);
        scr.Print(2, 24, B_TO_SELECT);
    }

    private void SetAGEBasicWidgets()
    {
        if (AGEBasic == null)
            AGEBasic = GetComponent<basicAGE>();

        if (AGEBasicContainer == null)
        {
            AGEBasicContainer = new(scr, "AGEBasicContainer");
            AGEBasicPrograms = new GenericOptions(scr, "AGEBasicPrograms",
                                    "program: ", null, 4, 6, maxLength: 26);
            AGEBasicContainer.Add(new GenericWindow(scr, 2, 4, "AGEBasic", 37, 15, " AGEBasic "))
                                .Add(AGEBasicPrograms)

                                .Add(new GenericButton(scr, "run", "run", 4,
                                    AGEBasicContainer.lastYAdded + 1, true))
                                .Add(new GenericButton(scr, "Compile",
                                                        "Compile all files again", 4,
                                                        AGEBasicContainer.lastYAdded + 2, true))

                                .Add(new GenericButton(scr, "RunTimeError",
                                                        "show last runtime error", 4,
                                                        AGEBasicContainer.lastYAdded + 1, true))
                                .Add(new GenericButton(scr, "CompError",
                                                        "show last compilation error", 4,
                                                        AGEBasicContainer.lastYAdded + 1, true))

                                .Add(new GenericButton(scr, "exit", "exit", 4,
                                                        AGEBasicContainer.lastYAdded + 2, true))

                                .Add(new GenericTimedLabel(scr, "RuntimeStatus",
                                            "runtime error", 4,
                                            AGEBasicContainer.lastYAdded + 2, true, false))
                                .Add(new GenericTimedLabel(scr, "CompStatus",
                                            "compilation error", 4,
                                            AGEBasicContainer.lastYAdded + 1, true, false))
                                .Add(new GenericLabel(scr, "lblpath", ConfigManager.AGEBasicDir, 0, 21, false));
        }

        AGEBasicCompile();
        return;
    }

    private void AGEBasicCompile()
    {
        try
        {
            AGEBasic.ParseFiles(ConfigManager.AGEBasicDir);
            AGEBasicCompilationException = null;
        }
        catch (CompilationException ce)
        {
            ConfigManager.WriteConsoleException("[ParseFiles]", ce);
            ((GenericTimedLabel)AGEBasicContainer.GetWidget("CompStatus")).Start(5);
            AGEBasicCompilationException = ce;
        }
        AGEBasicPrograms.SetOptions(AGEBasic.GetParsedPrograms());
    }
    private void AGEBasicRun()
    {
        string program = AGEBasicPrograms.GetSelectedOption();
        AGEBasic.Run(program, blocking: false);
        return;
    }


    private void AGEBasicShowLastCompilationError()
    {
        scr.Clear();
        scr.Print(2, 24, "press b to continue");

        if (AGEBasicCompilationException == null)
        {
            scr.Print(0, 0, "NO error", true);
            return;
        }
        scr.Print(0, 0, "compilation error", true);
        scr.Print(0, 1, AGEBasicCompilationException.Program);
        scr.Print(0, 2, "line: " + AGEBasicCompilationException.LineNumber.ToString());
        scr.Print(0, 3, AGEBasicCompilationException.Message);
    }

    private void AGEBasicShowLastRuntimeError()
    {
        scr.Clear();
        if (AGEBasic.LastRuntimeException != null)
        {
            scr.Print(0, 0, "runtime error", true);
            scr.Print(0, 1, AGEBasic.LastRuntimeException.Program);
            scr.Print(0, 2, "line: " + AGEBasic.LastRuntimeException.LineNumber.ToString());
            scr.Print(0, 3, AGEBasic.LastRuntimeException.Message);
        }
        else
        {
            scr.Print(0, 0, "NO error", true);
        }
        scr.Print(0, 24, "press b to continue");
    }

    private void AGEBasicWindowDraw()
    {
        scr.Clear();
        AGEBasicContainer.SetOption(0);
        AGEBasicContainer.Draw();

        scr.Print(2, 23, UDLR_TO_CHANGE);
        scr.Print(2, 24, B_TO_SELECT);
    }

    private void SetLocomotionWidgets()
    {
        if (locomotionContainer != null)
            return;

        locomotionSpeed = new GenericOptionsInteger(scr, "locomotionSpeed",
                                          "Speed:", 1, 12, 4, 6);

        locomotionTurnSpeed = new GenericOptionsInteger(scr, "locomotionTurnSpeed",
                                          "Turn Speed:", 10, 100, 4, 7);
        locomotionSnapTurnAmount = new GenericOptionsInteger(scr, "locomotionSnapTurnAmount",
                                          "Snap Turn Amount:", 10, 180, 4, 7);
        locomotionTeleportOn = new GenericBool(scr, "teleport", "teleport on/off: ", false, 4, 8);
        locomotionSnapTurnOn = new GenericBool(scr, "snap", "snap turn on/off: ", false, 4, 8);
        locomotionContainer = new(scr, "locmotionContainer");
        locomotionContainer.Add(new GenericWindow(scr, 2, 4, "locomotionwin", 37, 12, " locomotion "))
                            .Add(new GenericLabel(scr, "uxs", "in units per second:", 4, 6))
                            .Add(locomotionSpeed, 6, locomotionContainer.lastYAdded + 1)
                            .Add(new GenericLabel(scr, "degrotate", "degrees/second to rotate:", 4, locomotionContainer.lastYAdded + 1))
                            .Add(locomotionTurnSpeed, 6, locomotionContainer.lastYAdded + 1)
                            .Add(new GenericLabel(scr, "sanp", "snap turn:", 4, locomotionContainer.lastYAdded + 1))
                            .Add(locomotionSnapTurnOn, 6, locomotionContainer.lastYAdded + 1)
                            .Add(locomotionSnapTurnAmount, 6, locomotionContainer.lastYAdded + 1)
                            .Add(new GenericLabel(scr, "teleport", "activate/deactivate teleportation:", 4, locomotionContainer.lastYAdded + 1))
                            .Add(locomotionTeleportOn, 6, locomotionContainer.lastYAdded + 1)
                            .Add(new GenericButton(scr, "save", "save", 4, locomotionContainer.lastYAdded + 2, true))
                            .Add(new GenericButton(scr, "exit", "exit", 4, locomotionContainer.lastYAdded + 1, true));
    }
    private void LocomotionWindowDraw()
    {
        scr.Clear();
        locomotionContainer.Draw();

        scr.Print(2, 23, UDLR_TO_CHANGE);
        scr.Print(2, 24, B_TO_SELECT);
    }

    private void LocomotionSetWidgetsValues()
    {
        locomotionSpeed.SetCurrent((int)changeControls.moveSpeed);
        locomotionTurnSpeed.SetCurrent((int)changeControls.turnSpeed);
        locomotionTeleportOn.value = changeControls.teleportationEnabled;
        locomotionSnapTurnOn.value = changeControls.SnapTurnActive;
        locomotionSnapTurnAmount.SetCurrent((int)changeControls.SnapTurnAmount);
    }


    private void LocomotionUpdateConfigurationFromWidgets()
    {
        if (!isGlobalConfigurationWidget.value)
            return;

        ConfigInformation config = configHelper.getConfigInformation(true);
        config.locomotion = new();
        config.locomotion.moveSpeed = locomotionSpeed.GetSelectedOption();
        config.locomotion.turnSpeed = locomotionTurnSpeed.GetSelectedOption();
        config.locomotion.SnapTurnActive = locomotionSnapTurnOn.value;
        config.locomotion.SnapTurnAmount = locomotionSnapTurnAmount.GetSelectedOption();
        config.locomotion.teleportEnabled = locomotionTeleportOn.value;
        configHelper.Save(true, config);
        //after save the LocomotionConfigController (in introGallery configuration)
        //should detect the file change and configure the controls via ChangeControls component.
    }

    private void PlayerWindowDraw()
    {
        if (playerContainer == null)
            return;

        scr.Clear();
        playerContainer.Draw();
        scr.Print(2, 23, UDLR_TO_CHANGE);
        scr.Print(2, 24, B_TO_SELECT);
    }

    private void SetPlayerWidgets()
    {
        if (playerContainer != null)
            return;

        playerHeight = new GenericOptions(scr, "playerHeight", "height: ",
                                            new List<string>(ConfigInformation.Player.HeightPlayers.Keys),
                                            4, 7, maxLength: 26);

        /*playerScale = new GenericOptions(scr, "playerScale", "Age: ",
                                            new List<string>(ConfigInformation.Player.Scales.Keys),
                                            4, 8, maxLength: 26);
        */
        playerSkinColor = new GenericOptions(scr, "playerSkinColor", "Skin Color: ",
                                         ConfigInformation.Player.SkinColors,
                                         4, 9, maxLength: 26);
        playerContainer = new(scr, "playerContainer");
        playerContainer.Add(new GenericWindow(scr, 2, 4, "playerContainerWin", 37, 12, " Player "))
                        .Add(playerHeight, 4, 6)
                        //              .Add(playerScale, 4, playerContainer.lastYAdded + 1)
                        .Add(playerSkinColor, 4, playerContainer.lastYAdded + 1)  // add the skin color option
                        .Add(new GenericButton(scr, "save", "save", 4, playerContainer.lastYAdded + 2, true))
                        .Add(new GenericButton(scr, "exit", "exit", 4, playerContainer.lastYAdded + 1, true));

    }
    private void PlayerSetWidgetValues()
    {
        // Only for global configuration
        ConfigInformation.Player p;
        string height, skinColor; //, scale  // Declare the skinColor variable
        ConfigInformation config = configHelper.getConfigInformation(true);
        p = config.player ?? ConfigInformation.PlayerDefault();
        height = ConfigInformation.Player.FindNearestKey(p.height);
        //scale = ConfigInformation.Player.FindNearestKeyScale(p.scale);
        skinColor = p.skinColor ?? "light";  // default to light if not set

        playerHeight.SetCurrent(height);
        //playerScale.SetCurrent(scale);
        playerSkinColor.SetCurrent(skinColor);  // set current skin color value

        //ConfigManager.WriteConsole($"[ConfigurationController.PlayerSetWidgetValues] height:{height} scale:{scale} skinColor:{skinColor}.");
        ConfigManager.WriteConsole($"[ConfigurationController.PlayerSetWidgetValues] height:{height}  skinColor:{skinColor}");
    }

    private void PlayerUpdateConfigurationFromWidgets()
    {
        if (!isGlobalConfigurationWidget.value)
            return;

        ConfigInformation config = configHelper.getConfigInformation(true);
        config.player = ConfigInformation.PlayerDefault();
        //float scale = ConfigInformation.Player.GetScale(playerScale.GetSelectedOption());
        float height = ConfigInformation.Player.GetHeight(playerHeight.GetSelectedOption());
        string skinColor = playerSkinColor.GetSelectedOption();

        //if (scale != -1)
        //    config.player.scale = scale;
        if (height != -1)
            config.player.height = height;
        config.player.skinColor = skinColor;

        configHelper.Save(true, config);

    }

    void initScreen()
    {
        display = GetComponent<Renderer>();

        Dictionary<string, string> shaderConfig = new Dictionary<string, string>();
        shaderConfig["damage"] = "none";
        shader = ShaderScreen.Factory(display, 1, "crt", shaderConfig);
        shaderOffLine = ShaderScreen.Factory(display, 1, "crtlod", shaderConfig);

        ConfigInformation config = configHelper.getConfigInformation(true);

        scr.Init(config.system_skin);
        ActivateShader();
        scr.ClearBackground();
        scr.Clear();
        scr.PrintCentered(1, "BIOS ROM firmware loaded", true);
        scr.PrintCentered(2, GetRoomDescription());
        scr.DrawScreen();
    }
    void ActivateShader(bool isRunning = false)
    {
        if (isRunning)
            scr.ActivateShader(shader);
        else
            scr.ActivateShader(shaderOffLine);
    }

    IEnumerator run()
    {
        ConfigManager.WriteConsole("[ConfigurationController.run] coroutine started.");

        // Initialize the dictionary with default values (false for all keys)
        inputDictionary.Add("up", false);
        inputDictionary.Add("down", false);
        inputDictionary.Add("left", false);
        inputDictionary.Add("right", false);
        inputDictionary.Add("action", false);
        inputKeys = inputDictionary.Keys.ToList();

        configHelper = new(globalConfiguration, roomConfiguration);
        yield return new WaitForEndOfFrame(); //needed to wait the config load

        initScreen();
        yield return new WaitForEndOfFrame();

        if (configHelper.CanConfigureRoom() && cabinetsController != null)
        {
            ScreenWaitingDraw();
            scr.DrawScreen();
            yield return new WaitForEndOfFrame();

            while (!cabinetsController.Loaded)
            {
                yield return new WaitForSeconds(1f / 2f);
                // ConfigManager.WriteConsole("[ConfigurationController] waiting for cabinets load.");
                ScreenWaitingDraw();
                scr.DrawScreen();
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            //wait a bit to setup all elements in the room.
            yield return new WaitForSeconds(1f / 2f);
        }

        if (cabinetsController?.gameRegistry == null)
            ConfigManager.WriteConsole("[ConfigurationController] gameregistry component not found, cant configure game controllers");

        //create widgets
        SetGlobalWidgets();

        //init status
        status = StatusOptions.init;

        //AGEBASIC
        ConfigInformation config = configHelper.getConfigInformation(false);
        ConfigInformation.AGEBasicInformation ageBasicInformation = config?.agebasic;
        if (ageBasicInformation != null && ageBasicInformation.active)
        {
            ConfigManager.WriteConsole($"[ConfigurationController] run after load agebasic program {ageBasicInformation.afterLoad}");

            scr.Clear();
            scr.PrintCentered(1, "Running After-load");
            scr.PrintCentered(2, "program");
            scr.PrintCentered(3, ageBasicInformation.afterLoad, true);
            scr.DrawScreen();

            bool compilationError = false;
            string program = Path.Combine(ConfigManager.AGEBasicDir, ageBasicInformation.afterLoad);
            ConfigManager.WriteConsole($"[ConfigurationController] [{ageBasicInformation.afterLoad}] {program}");
            try
            {
                AGEBasic.ParseFile(program);
            }
            catch (CompilationException ex)
            {
                ConfigManager.WriteConsoleException($"[ConfigurationController] [{ageBasicInformation.afterLoad}] compilation error", ex);

                compilationError = true;
                scr.PrintCentered(5, "compilation error:", true);
                scr.Print(0, 6, $"Line: {ex.LineNumber}");
                scr.Print(0, 7, ex.Message);
            }
            catch (Exception ex)
            {
                compilationError = true;
                ConfigManager.WriteConsoleException($"[ConfigurationController] [{ageBasicInformation.afterLoad}]", ex);
                scr.PrintCentered(5, "compilation error:", true);
                scr.Print(0, 6, ex.Message);
            }
            yield return new WaitForEndOfFrame();


            if (!compilationError)
            {
                ConfigManager.WriteConsole($"[ConfigurationController] [{ageBasicInformation.afterLoad}] start run");
                AGEBasic.DebugMode = ageBasicInformation.debug;
                AGEBasic.Run(ageBasicInformation.afterLoad, blocking: false);

                while (AGEBasic.IsRunning())
                    yield return new WaitForSeconds(1f / 2f);

                ConfigManager.WriteConsole($"[ConfigurationController] [{ageBasicInformation.afterLoad}] ended. Error: [{AGEBasic.LastRuntimeException}]");

                if (AGEBasic.ExceptionOccurred())
                {
                    ConfigManager.WriteConsoleException($"[ConfigurationController] [{ageBasicInformation.afterLoad}] runtime", AGEBasic.LastRuntimeException);
                    scr.Print(0, 5, "runtime exception:", true);
                    scr.Print(0, 6, $"Line: {AGEBasic.LastRuntimeException.LineNumber}");
                    scr.Print(0, 7, AGEBasic.LastRuntimeException.Message);
                }
                else
                {
                    scr.PrintCentered(5, "FINISHED - RUN OK", true);
                }
            }

            ConfigManager.WriteConsole($"[ConfigurationController] [{ageBasicInformation.afterLoad}] OK");

            scr.DrawScreen();
            yield return new WaitForEndOfFrame();

            status = StatusOptions.waitingForCoin;
        }

        //main cycle
        tree = buildBT();
        while (true)
        {
            tree.Tick();
            ResetInputValues();

            if (status == StatusOptions.init || status == StatusOptions.waitingForCoin)
                yield return new WaitForSeconds(2f);
            else if (status == StatusOptions.onBoot)
                yield return new WaitForSeconds(1f / 4f);
            else
            {
                yield return new WaitForSeconds(1f / 6f);
                UpdateInputValues();
            }
        }
    }

    public bool Teleport(string roomNameOrDescription)
    {
        SceneDocument toScene = sceneDatabase.FindByDescription(roomNameOrDescription);
        if (toScene == null)
            toScene = sceneDatabase.FindByName(roomNameOrDescription);

        if (toScene == null)
            return false;
        //throw new Exception($"Teleport to room '{roomNameOrDescription}' fail: room is unknown, please chech the room name");

        ConfigManager.WriteConsole($"[ConfigurationController.Teleport] teleport to scene [{roomNameOrDescription}]");
        ControllersEnable(false); //free the player
        teleportation.Teleport(toScene);
        return true;
    }

    private BehaviorTree buildBT()
    {
        return new BehaviorTreeBuilder(gameObject)
          .Selector()

            .Sequence("Init")
              .Condition("On init", () => status == StatusOptions.init)
              .Do("Process", () =>
              {
                  status = StatusOptions.waitingForCoin;
                  scr.Clear()
                     .PrintCentered(10, "Insert coin to start", true)
                     .PrintCentered(12, GetRoomDescription(), false)
                     .DrawScreen();
                  return TaskStatus.Success;
              })
            .End()

            .Sequence("Insert coin")
              .Condition("Waiting for coin", () => status == StatusOptions.waitingForCoin)
              .Condition("Is a coin in the bucket", () => (CoinSlot != null && CoinSlot.takeCoin()))
              .Do("coin inserted", () =>
              {
                  ActivateShader(true);

                  scr.Clear();
                  bootScreen.Reset();
                  ControllersEnable(true);

                  status = StatusOptions.onBoot;

                  return TaskStatus.Success;
              })
            .End()

            .Sequence("Boot")
              .Condition("Booting", () => status == StatusOptions.onBoot)
              .Condition("Finished lines", () =>
              {
                  bool finished = bootScreen.PrintNextLine();
                  scr.DrawScreen();
                  return finished;
              })
              .Do("Start main menu", () =>
              {
                  setupActionMap();
                  status = StatusOptions.onMainMenu;

                  return TaskStatus.Success;
              })
            .End()

            .Sequence("Main menu")
              .Condition("On main menu", () => status == StatusOptions.onMainMenu)
              .Do("Init", () =>
              {
                  SetMainMenuWidgets();
                  scr.Clear();
                  mainMenuDraw();
                  scr.DrawScreen();
                  return TaskStatus.Success;
              })
              .Do("Process", () =>
              {
                  if (inputDictionary["up"])
                      mainMenu.PreviousOption();
                  else if (inputDictionary["down"])
                      mainMenu.NextOption();
                  else if (inputDictionary["action"])
                      mainMenu.Select();

                  if (!mainMenu.IsSelected())
                  {
                      scr.DrawScreen();
                      return TaskStatus.Continue;
                  }

                  ConfigManager.WriteConsole($"[ConfigurationController] option selected: {mainMenu.GetSelectedOption()}");
                  string selectedOption = mainMenu.GetSelectedOption();
                  switch (selectedOption)
                  {
                      case "NPC configuration":
                          status = StatusOptions.onNPCMenu;
                          break;
                      case "exit":
                          status = StatusOptions.exit;
                          break;
                      case "Audio configuration":
                          status = StatusOptions.onAudio;
                          break;
                      case "change mode (global/room)":
                          status = StatusOptions.onChangeMode;
                          break;
                      case "reset":
                          status = StatusOptions.onReset;
                          break;
                      case "cabinets":
                          status = StatusOptions.onChangeCabinets;
                          break;
                      case "configure cabinets":
                          status = StatusOptions.onCabinet;
                          break;
                      case "controllers":
                          status = StatusOptions.onChangeController;
                          break;
                      case "teleport":
                          status = StatusOptions.onTeleport;
                          break;
                      case "locomotion":
                          status = StatusOptions.onChangeLocomotion;
                          break;
                      case "AGEBasic":
                          status = StatusOptions.onRunAGEBasic;
                          break;
                      case "player":
                          status = StatusOptions.onChangePlayer;
                          break;
                  }

                  mainMenu.Deselect();
                  scr.DrawScreen();
                  return TaskStatus.Success;
              })
            .End()

            .Sequence("NPC Configuration")
              .Condition("On NPC Config", () => status == StatusOptions.onNPCMenu)
              .Do("Init", () =>
              {
                  SetNPCWidgets();
                  NPCGetStatus();
                  NPCScreenDraw();
                  scr.DrawScreen();
                  return TaskStatus.Success;
              })
              .Do("Process", () =>
              {
                  changeContainerSelection(npcContainer);

                  if (inputDictionary["action"])
                  {
                      GenericWidget w = npcContainer.GetSelectedWidget();
                      if (w.name == "exit")
                      {
                          status = StatusOptions.onMainMenu;
                          return TaskStatus.Success;
                      }
                      else if (w.name == "save")
                      {
                          NPCSave();
                          status = StatusOptions.onMainMenu;
                          return TaskStatus.Success;
                      }
                      scr.DrawScreen();
                      return TaskStatus.Success;
                  }
                  scr.DrawScreen();
                  return TaskStatus.Continue;
              })
            .End()

            .Sequence("Audio Configuration")
              .Condition("On Config", () => status == StatusOptions.onAudio)
              .Do("Init", () =>
              {
                  SetAudioWidgets();
                  audioScreen();
                  audioContainer.Draw();
                  scr.DrawScreen();
                  return TaskStatus.Success;
              })
              .Do("Process", () =>
              {
                  changeContainerSelection(audioContainer);
                  if (inputDictionary["action"])
                  {
                      GenericWidget w = audioContainer.GetSelectedWidget();
                      if (w != null)
                      {
                          if (w.name == "exit")
                          {
                              status = StatusOptions.onMainMenu;
                              return TaskStatus.Success;
                          }
                          else if (w.name == "save")
                          {
                              audioSave();
                              status = StatusOptions.onMainMenu;
                              return TaskStatus.Success;
                          }
                          w.Action();
                      }
                  }
                  scr.DrawScreen();
                  return TaskStatus.Continue;
              })
            .End()

            .Sequence("Player Configuration")
              .Condition("On Config", () => status == StatusOptions.onChangePlayer)
              .Do("Init", () =>
              {
                  SetPlayerWidgets();
                  PlayerSetWidgetValues();
                  PlayerWindowDraw();
                  scr.DrawScreen();
                  return TaskStatus.Success;
              })
              .Do("Process", () =>
              {
                  changeContainerSelection(playerContainer);
                  if (inputDictionary["action"])
                  {
                      GenericWidget w = playerContainer.GetSelectedWidget();
                      if (w != null)
                      {
                          if (w.name == "exit")
                          {
                              status = StatusOptions.onMainMenu;
                              return TaskStatus.Success;
                          }
                          else if (w.name == "save")
                          {
                              PlayerUpdateConfigurationFromWidgets();
                              status = StatusOptions.onMainMenu;
                              return TaskStatus.Success;
                          }
                          w.Action();
                      }
                  }
                  scr.DrawScreen();
                  return TaskStatus.Continue;
              })
            .End()

            .Sequence("Change Mode")
              .Condition("On change mode", () => status == StatusOptions.onChangeMode)
              .Do("Init", () =>
              {
                  SetChangeModeWidgets();
                  changeModeWindowDraw();
                  scr.DrawScreen();
                  return TaskStatus.Success;
              })
              .Do("Process", () =>
              {
                  changeContainerSelection(changeModeContainer);
                  GenericWidget w = changeModeContainer.GetSelectedWidget();
                  if (w != null && inputDictionary["action"])
                  {
                      if (w.name == "exit")
                      {
                          status = StatusOptions.onMainMenu;
                          return TaskStatus.Success;
                      }
                      w.Action();
                  }
                  scr.DrawScreen();
                  return TaskStatus.Continue;
              })
            .End()

            .Sequence("back to default reset")
              .Condition("On back to default", () => status == StatusOptions.onReset)
              .Do("Init", () =>
              {
                  SetResetWidgets();
                  resetWindowDraw();
                  scr.DrawScreen();
                  return TaskStatus.Success;
              })
              .Do("Process", () =>
              {
                  changeContainerSelection(resetContainer);
                  GenericWidget w = resetContainer.GetSelectedWidget();
                  if (w != null && inputDictionary["action"])
                  {
                      if (w.name == "exit")
                      {
                          status = StatusOptions.onMainMenu;
                          return TaskStatus.Success;
                      }
                      else if (w.name == "reset")
                      {
                          resetSave();
                          status = StatusOptions.onMainMenu;
                          return TaskStatus.Success;
                      }
                  }
                  scr.DrawScreen();
                  return TaskStatus.Continue;
              })
            .End()

            .Sequence("controller config")
              .Condition("On selecting game", () => status == StatusOptions.onChangeController)
              .Do("Init", () =>
              {
                  SetControlMapWidgets();
                  controlMapConfigurationLoad();
                  controlMapUpdateWidgets();
                  controllerContainerDraw();
                  scr.DrawScreen();

                  return TaskStatus.Success;
              })
              .Do("Process", () =>
              {
                  if (inputDictionary["up"] || ControlActive(LC.KEYB_UP))
                      controllerContainer.PreviousOption();
                  else if (inputDictionary["down"] || ControlActive(LC.KEYB_DOWN))
                      controllerContainer.NextOption();

                  GenericWidget w = controllerContainer.GetSelectedWidget();

                  if (w != null)
                  {
                      bool right = inputDictionary["left"] || ControlActive(LC.KEYB_LEFT);
                      bool left = inputDictionary["right"] || ControlActive(LC.KEYB_RIGHT);

                      if (inputDictionary["action"])
                      {
                          if (w.name == "exit")
                          {
                              status = StatusOptions.onMainMenu;
                              return TaskStatus.Success;
                          }
                          else if (w.name == "save")
                          {
                              if (controlMapConfigurationSave())
                                  controlMapSavedLabel.label = "saved ok    ";
                              else
                                  controlMapSavedLabel.label = "error saving";
                              controlMapSavedLabel.SetSecondsAndDraw(2);

                              controlMapUpdateWidgets();
                              controllerContainerDraw();
                          }
                      }
                      else if (right || left)
                      {
                          if (left)
                              w.NextOption();
                          else if (right)
                              w.PreviousOption();

                          if (w.name == "gameId")
                          {
                              lblGameSelected.label = controlMapGameId.GetSelectedOption();
                              controlMapPort.SetCurrent(0);
                              controlMapConfigurationLoad();
                          }
                          else if (w.name == "mameControl")
                          {
                              controlMapPort.SetCurrent(0);
                          }
                          else if (w.name.StartsWith("controlMapRealControl")) // controlMapRealControl or mameControl
                          {
                              controlMapUpdateConfigurationFromWidgets();
                          }
                          controlMapUpdateWidgets();
                          controllerContainerDraw();
                      }
                  }

                  controlMapSavedLabel.Draw();
                  scr.DrawScreen();

                  return TaskStatus.Continue;
              })

            .End()

            .Sequence("Cabinets Replacement")
              .Condition("On cabinets replacement", () => status == StatusOptions.onChangeCabinets)
              .Condition("Can configure cabinets", () => CanConfigureCabinets())
              .Do("Init", () =>
              {
                  SetCabinetsReplacementWidgets();
                  CabinetsReplacementWindowDraw();
                  scr.DrawScreen();
                  return TaskStatus.Success;
              })
              .Do("Process", () =>
              {
                  changeContainerSelection(cabinetsReplacementToChangeContainer);
                  GenericWidget w = cabinetsReplacementToChangeContainer.GetSelectedWidget();
                  if (w != null && inputDictionary["action"])
                  {
                      if (w.name == "exit")
                      {
                          status = StatusOptions.onMainMenu;
                          return TaskStatus.Success;
                      }
                      else if (w.name == "save")
                      {
                          SaveCabinetPositions();
                          SetCabinetsReplacementWidgets();
                          cabinetReplacementSavedLabel.SetSecondsAndDraw(2);
                      }
                  }
                  cabinetReplacementSavedLabel.Draw();
                  scr.DrawScreen();
                  return TaskStatus.Continue;
              })
            .End()


            .Sequence("Cabinets")
              .Condition("On cabinets", () => status == StatusOptions.onCabinet)
              .Do("Init", () =>
              {
                  SetCabinetsConfigurationWidgets();
                  CabinetConfigurationSetWidgetsValues();
                  CabinetConfigurationWindowDraw();
                  scr.DrawScreen();
                  return TaskStatus.Success;
              })
              .Do("Process", () =>
              {
                  GenericTimedLabel tlbl = (GenericTimedLabel)cabinetsConfigurationContainer.GetWidget("saved");
                  changeContainerSelection(cabinetsConfigurationContainer);
                  GenericWidget w = cabinetsConfigurationContainer.GetSelectedWidget();
                  if (w != null && inputDictionary["action"])
                  {
                      if (w.name == "exit")
                      {
                          status = StatusOptions.onMainMenu;
                          return TaskStatus.Success;
                      }
                      else if (w.name == "reset")
                      {
                          CabinetConfigurationReset();
                          CabinetConfigurationSetWidgetsValues();
                          CabinetConfigurationWindowDraw();
                      }
                      else if (w.name == "save")
                      {
                          SaveCabinetConfiguration();
                          //SetCabinetsConfigurationWidgets();
                          tlbl.SetSecondsAndDraw(3);
                      }
                      else
                      {
                          w.Action();
                      }
                  }
                  tlbl.Draw();
                  scr.DrawScreen();
                  return TaskStatus.Continue;
              })
            .End()

            .Sequence("Teleport")
              .Condition("On teleport", () => status == StatusOptions.onTeleport)
              .Condition("Can teleport", () => canTeleport)
              .Do("Init", () =>
              {
                  SetTeleportWidgets();
                  TeleportWindowDraw();
                  scr.DrawScreen();

                  return TaskStatus.Success;
              })
                .Do("Process", () =>
                {
                    changeContainerSelection(teleportContainer);
                    GenericWidget w = teleportContainer.GetSelectedWidget();
                    if (w != null && inputDictionary["action"])
                    {
                        if (w.name == "exit")
                        {
                            status = StatusOptions.onMainMenu;
                            return TaskStatus.Success;
                        }
                        else if (w.name == "teleport")
                        {
                            string sceneDescription = teleportDestination.GetSelectedOption();
                            bool teleported = Teleport(sceneDescription);
                            if (teleported)
                            {
                                ControllersEnable(false); //free the player
                                status = StatusOptions.init;
                                return TaskStatus.Success;
                            }
                            teleportResult.Start(5);
                        }
                    }
                    scr.DrawScreen();
                    return TaskStatus.Continue;
                })
            .End()

            .Sequence("Locomotion")
              .Condition("On locomotion", () => status == StatusOptions.onChangeLocomotion)
              .Condition("is global config", () => isGlobalConfigurationWidget.value)
              .Do("Init", () =>
              {
                  SetLocomotionWidgets();
                  LocomotionSetWidgetsValues();
                  LocomotionWindowDraw();
                  scr.DrawScreen();
                  return TaskStatus.Success;
              })
                .Do("Process", () =>
                {
                    changeContainerSelection(locomotionContainer);
                    GenericWidget w = locomotionContainer.GetSelectedWidget();
                    if (w != null && inputDictionary["action"])
                    {
                        if (w.name == "exit")
                        {
                            status = StatusOptions.onMainMenu;
                            return TaskStatus.Success;
                        }
                        else if (w.name == "teleport")
                        {
                            locomotionTeleportOn.Action();
                        }
                        else if (w.name == "snap")
                        {
                            locomotionSnapTurnOn.Action();
                        }
                        else if (w.name == "save")
                        {
                            LocomotionUpdateConfigurationFromWidgets();
                            status = StatusOptions.onMainMenu;
                            return TaskStatus.Success;
                        }
                    }
                    scr.DrawScreen();
                    return TaskStatus.Continue;
                })
            .End()

            .Sequence("AGEBasicRunning")
              .Condition("On AGEBasic", () => status == StatusOptions.onRunAGEBasicRunning)
              .Do("Process", () =>
              {
                  if (DateTime.Now > AGEBasicRunTimeout)
                      AGEBasic.Stop();

                  if (AGEBasic.LastRuntimeException != null)
                      ((GenericTimedLabel)AGEBasicContainer.GetWidget("RuntimeStatus")).Start(4);

                  if (AGEBasic.IsRunning())
                      return TaskStatus.Continue;

                  status = StatusOptions.onRunAGEBasic;
                  return TaskStatus.Success;
              })
            .End()

            .Sequence("AGEBasic")
              .Condition("On AGEBasic", () => status == StatusOptions.onRunAGEBasic)
              .Do("Init", () =>
              {
                  SetAGEBasicWidgets();
                  AGEBasicWindowDraw();
                  scr.DrawScreen();
                  return TaskStatus.Success;
              })
                .Do("Process", () =>
                {
                    if (AGEBasicWaitForPressAKey)
                    {
                        if (inputDictionary["action"])
                        {
                            AGEBasicWaitForPressAKey = false;
                            AGEBasicWindowDraw();
                            scr.DrawScreen();
                        }
                        return TaskStatus.Continue;
                    }

                    changeContainerSelection(AGEBasicContainer);
                    GenericWidget w = AGEBasicContainer.GetSelectedWidget();
                    if (w != null && inputDictionary["action"])
                    {
                        if (w.name == "exit")
                        {
                            status = StatusOptions.onMainMenu;
                            return TaskStatus.Success;
                        }
                        else if (w.name == "run")
                        {
                            AGEBasicRun();
                            AGEBasicRunTimeout = DateTime.Now.AddSeconds(60 * 30); //if not reach in time abort
                            status = StatusOptions.onRunAGEBasicRunning;
                            return TaskStatus.Success;
                        }
                        else if (w.name == "Compile")
                        {
                            AGEBasicCompile();
                        }
                        else if (w.name == "CompError" || w.name == "RunTimeError")
                        {
                            AGEBasicWaitForPressAKey = true;
                            if (w.name == "CompError")
                                AGEBasicShowLastCompilationError();
                            else
                                AGEBasicShowLastRuntimeError();
                            scr.DrawScreen();
                            return TaskStatus.Continue;
                        }
                    }

                    AGEBasicContainer.GetWidget("CompStatus").Draw();
                    AGEBasicContainer.GetWidget("RuntimeStatus").Draw();
                    scr.DrawScreen();
                    return TaskStatus.Continue;
                })
            .End()

            .Sequence("EXIT")
              //.Condition("Exit button", () => ControlActive("EXIT"))
              .Condition("Exit", () => status == StatusOptions.exit)
              .Do("exit", () =>
              {
                  ConfigManager.WriteConsole($"[ConfigurationController] EXIT ");

                  ActivateShader(false);

                  cleanActionMap();
                  ControllersEnable(false);

                  status = StatusOptions.init;
                  return TaskStatus.Success;
              })
            .End()

          .End()
          .Build();
    }

    public void ControllersEnable(bool enable)
    {
        changeControls.PlayerMode(enable);
        //libretroControlMap.Enable(enable);
        return;
    }

    public bool ControlActive(string mameControl)
    {
        try
        {
            return libretroControlMap.Active(mameControl) != 0;
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[ConfigurationController.ControlActive] {mameControl}", e);
            libretroControlMap.Enable(true);
            return false;
        }

    }

    private void changeContainerSelection(GenericWidgetContainer gwc)
    {
        if (inputDictionary["up"])
            gwc.PreviousOption();
        else if (inputDictionary["down"])
            gwc.NextOption();
        else if (inputDictionary["left"])
            gwc.GetSelectedWidget()?.PreviousOption();
        else if (inputDictionary["right"])
            gwc.GetSelectedWidget()?.NextOption();
        return;
    }


#if UNITY_EDITOR
    public void EditorInsertCoin()
    {
        CoinSlot.insertCoin();
    }
#endif

}

#if UNITY_EDITOR
[CustomEditor(typeof(ConfigurationController))]
public class ConfigurationControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ConfigurationController myScript = (ConfigurationController)target;
        if (GUILayout.Button("InsertCoin"))
        {
            myScript.EditorInsertCoin();
        }
    }
}
#endif

