using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;

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
            GameObject GlobalConfigurationGameObject = GameObject.Find("GlobalConfiguration");
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
    public ScreenGenerator scr;
    public CoinSlotController CoinSlot;
    public InputActionMap actionMap;
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
    private GenericWidgetContainer teleportContainer;

    //locomotion
    private GenericWidgetContainer locomotionContainer;
    private GenericBool locomotionTeleportOn;
    private GenericOptionsInteger locomotionSpeed;
    private GenericOptionsInteger locomotionTurnSpeed;

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
        exit
    }
    private StatusOptions status;
    private C64BootScreen bootScreen;
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

    private GenericWidgetContainer cabinetsToChangeContainer;
    private GenericOptions cabinetToReplace;
    private GenericOptions cabinetReplaced;
    private GenericTimedLabel cabinetSavedLabel;

    private GenericLabelOnOff lblHaveRoomConfiguration, lblRoomName;

    //player
    private GenericOptions playerHeight;
    private GenericOptions playerScale;
    private GenericWidgetContainer playerContainer;

    private DefaultControlMap map;

    private ConfigurationHelper configHelper;


    // Start is called before the first frame update
    void Start()
    {
        ConfigManager.WriteConsole("[ConfigurationController] start");
        setupActionMap();

        if (changeControls == null)
        {
            GameObject player = GameObject.Find("OVRPlayerControllerGalery");
            changeControls = player.GetComponent<ChangeControls>();
        }

        if (canTeleport)
        {
            GameObject roomInit = GameObject.Find("RoomInit");
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

        StartCoroutine(run());
    }

    private void setupActionMap()
    {
        map = new();
        ControlMapConfiguration conf = new DefaultControlMap();
        conf.AddMap("KEYB-UP", "keyboard-w");
        conf.AddMap("KEYB-DOWN", "keyboard-s");
        conf.AddMap("KEYB-LEFT", "keyboard-a");
        conf.AddMap("KEYB-RIGHT", "keyboard-d");
        actionMap = ControlMapInputAction.inputActionMapFromConfiguration(conf);
    }

    public void NPCScreenDraw()
    {
        scr.Clear();
        npcContainer.Draw();

        //some help
        scr.Print(2, 16, "left/right/up/down to change");
        scr.Print(2, 17, "b to select");

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
        scr.Print(2, 16, "up/down to change");
        scr.Print(2, 17, "b to select and exit");
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

    public void InsertCoin()
    {
        ControlEnable(true);
        status = StatusOptions.onBoot;
        scr.Clear();
        bootScreen.Reset();
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
            scr.Print(2, 4, "up/down to change");
        }
        else
        {
            isGlobalConfigurationWidget.enabled = false;
            scr.Print(2, 1, "only global configuration allowed");
            scr.Print(2, 3, "for one room configuration go to");
            scr.Print(2, 4, "a gallery room");
        }
        scr.Print(2, 19, "b to select");

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
        scr.Print(2, 23, "up/down/left/right to change");
        scr.Print(2, 24, "b to select");
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
                      .Add(new GenericLabel(scr, "l1", "left/right/b to change", 2, 20))
                      .Add(new GenericLabel(scr, "l2", "up/down to move", 2, 21));
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

    private void SetCabinetsWidgets()
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
        cabinetSavedLabel = new(scr, "saved", "cabinet replaced", 3, 19, true);

        cabinetsToChangeContainer = new(scr, "cabinetsToChangeContainer");
        cabinetsToChangeContainer.Add(new GenericWindow(scr, 2, 4, "cabswin", 37, 12, " replace cabinets "))
                                .Add(lblRoomName)
                                .Add(cabinetToReplace)
                                .Add(cabinetReplaced)
                                .Add(new GenericButton(scr, "save", "save", 4, 11, true))
                                .Add(new GenericButton(scr, "exit", "exit", 4, 12, true))
                                .Add(cabinetSavedLabel);
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

    private void CabinetsWindowDraw()
    {
        if (cabinetsToChangeContainer == null)
            return;

        scr.Clear();
        cabinetsToChangeContainer.Draw();
        scr.Print(2, 23, "up/down/left/right to change");
        scr.Print(2, 24, "b to select");
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

        teleportContainer = new(scr, "teleportContainer");
        teleportContainer.Add(new GenericWindow(scr, 2, 4, "teleportwin", 37, 12,
                                        " teleportation "))
                            .Add(new GenericLabel(scr, "lbl", "select destination", 4, 6))
                            .Add(teleportDestination)
                            .Add(new GenericButton(scr, "teleport", "teleport", 4, 11, true))
                            .Add(new GenericButton(scr, "exit", "exit", 4, 12, true));
    }

    private void TeleportWindowDraw()
    {
        if (!canTeleport)
            return;

        scr.Clear();
        teleportContainer.Draw();
        scr.Print(2, 23, "up/down/left/right to change");
        scr.Print(2, 24, "b to select");
    }

    private void LocomotionUpdateConfigurationFromWidgets()
    {
        if (!isGlobalConfigurationWidget.value)
            return;

        ConfigInformation config = configHelper.getConfigInformation(true);
        config.locomotion = new();
        config.locomotion.moveSpeed = locomotionSpeed.GetSelectedOption();
        config.locomotion.turnSpeed = locomotionTurnSpeed.GetSelectedOption();
        config.locomotion.teleportEnabled = locomotionTeleportOn.value;
        configHelper.Save(true, config);
        //after save the LocomotionConfigController (in introGallery configuration)
        //should detect the file change and configure the controls via ChangeControls component.
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

        scr.Print(2, 23, "up/down/left/right to change");
        scr.Print(2, 24, "b to select");
    }

    private void SetLocomotionWidgets()
    {
        if (locomotionContainer != null)
            return;

        locomotionSpeed = new GenericOptionsInteger(scr, "locomotionSpeed",
                                          "Speed:", 1, 12, 4, 6);

        locomotionTurnSpeed = new GenericOptionsInteger(scr, "locomotionTurnSpeed",
                                          "Turn Speed:", 10, 100, 4, 7);
        locomotionTeleportOn = new GenericBool(scr, "teleport", "teleport on/off: ", false, 4, 8);

        locomotionContainer = new(scr, "locmotionContainer");
        locomotionContainer.Add(new GenericWindow(scr, 2, 4, "locomotionwin", 37, 12, " locomotion "))
                            .Add(new GenericLabel(scr, "descrip1", "in units per second:", 4, 6))
                            .Add(locomotionSpeed, 6, locomotionContainer.lastYAdded + 1)
                            .Add(new GenericLabel(scr, "descrip2", "degrees/second to rotate:", 4, locomotionContainer.lastYAdded + 1))
                            .Add(locomotionTurnSpeed, 6, locomotionContainer.lastYAdded + 1)
                            .Add(new GenericLabel(scr, "descrip3", "activate/deactivate teleportation:", 4, locomotionContainer.lastYAdded + 1))
                            .Add(locomotionTeleportOn, 6, locomotionContainer.lastYAdded + 1)
                            .Add(new GenericButton(scr, "save", "save", 4, locomotionContainer.lastYAdded + 2, true))
                            .Add(new GenericButton(scr, "exit", "exit", 4, locomotionContainer.lastYAdded + 1, true));
    }
    private void LocomotionWindowDraw()
    {
        scr.Clear();
        locomotionContainer.Draw();

        scr.Print(2, 23, "up/down/left/right to change");
        scr.Print(2, 24, "b to select");
    }

    private void LocomotionSetWidgetsValues()
    {
        locomotionSpeed.SetCurrent((int)changeControls.moveSpeed);
        locomotionTurnSpeed.SetCurrent((int)changeControls.turnSpeed);
        locomotionTeleportOn.value = changeControls.teleportationEnabled;
    }

    private void PlayerWindowDraw()
    {
        if (playerContainer == null)
            return;

        scr.Clear();
        playerContainer.Draw();
        scr.Print(2, 23, "up/down/left/right to change");
        scr.Print(2, 24, "b to select");
    }

    private void SetPlayerWidgets()
    {
        if (playerContainer != null)
            return;

        playerHeight = new GenericOptions(scr, "playerHeight", "height: ",
                                            new List<string>(ConfigInformation.Player.HeightPlayers.Keys),
                                            4, 7, maxLength: 26);

        playerScale = new GenericOptions(scr, "playerScale", "Age: ",
                                            new List<string>(ConfigInformation.Player.Scales.Keys),
                                            4, 8, maxLength: 26);
        playerContainer = new(scr, "playerContainer");
        playerContainer.Add(new GenericWindow(scr, 2, 4, "playerContainerWin", 37, 12, " Player "))
                            .Add(playerHeight, 4, 6)
                            //.Add(playerScale, 4, playerContainer.lastYAdded + 1)
                            .Add(new GenericButton(scr, "save", "save", 4, playerContainer.lastYAdded + 2, true))
                            .Add(new GenericButton(scr, "exit", "exit", 4, playerContainer.lastYAdded + 1, true));
    }
    private void PlayerSetWidgetValues()
    {
        //only for global configuration
        ConfigInformation.Player p;
        string height, scale;
        ConfigInformation config = configHelper.getConfigInformation(true);
        p = config.player ?? ConfigInformation.PlayerDefault();
        height = ConfigInformation.Player.FindNearestKey(p.height);
        scale = ConfigInformation.Player.FindNearestKeyScale(p.scale);
        playerHeight.SetCurrent(height);
        playerScale.SetCurrent(scale);
        ConfigManager.WriteConsole($"[ConfigurationController.PlayerSetWidgetValues] height:{height} scale:{scale}.");
    }

    private void PlayerUpdateConfigurationFromWidgets()
    {
        if (!isGlobalConfigurationWidget.value)
            return;

        ConfigInformation config = configHelper.getConfigInformation(true);
        config.player = ConfigInformation.PlayerDefault();
        float scale = ConfigInformation.Player.GetScale(playerScale.GetSelectedOption());
        float height = ConfigInformation.Player.GetHeight(playerHeight.GetSelectedOption());
        if (scale != -1)
            config.player.scale = scale;
        if (height != -1)
            config.player.height = height;

        configHelper.Save(true, config);
    }

    IEnumerator run()
    {
        ConfigManager.WriteConsole("[ConfigurationController.run] coroutine started.");

        scr.Clear();
        scr.PrintCentered(1, "BIOS ROM firmware loaded", true);
        scr.PrintCentered(2, GetRoomDescription());
        scr.DrawScreen();
        yield return null;

        // first: wait for the room to load.
        configHelper = new(globalConfiguration, roomConfiguration);
        if (configHelper.CanConfigureRoom() && cabinetsController != null)
        {
            ScreenWaitingDraw();
            scr.DrawScreen();
            while (!cabinetsController.Loaded)
            {
                yield return new WaitForSeconds(1f / 2f);
                // ConfigManager.WriteConsole("[ConfigurationController] waiting for cabinets load.");
                ScreenWaitingDraw();
                scr.DrawScreen();
            }
        }
        else
        {
            //wait a bit to setup all elements in the room.
            yield return new WaitForSeconds(1f / 2f);
        }

        if (cabinetsController?.gameRegistry == null)
            ConfigManager.WriteConsole("[ConfigurationController] gameregistry component not found, cant configure games controllers");

        //create widgets
        SetGlobalWidgets();

        //main cycle
        status = StatusOptions.init;
        tree = buildBT();
        while (true)
        {
            tree.Tick();

            if (status == StatusOptions.init || status == StatusOptions.waitingForCoin)
                yield return new WaitForSeconds(1f);
            else if (status == StatusOptions.onBoot)
                yield return new WaitForSeconds(1f / 4f);
            else
                yield return new WaitForSeconds(1f / 6f);
        }
    }

    private BehaviorTree buildBT()
    {
        return new BehaviorTreeBuilder(gameObject)
          .Selector()

            .Sequence("Init")
              .Condition("On init", () => status == StatusOptions.init)
              .Do("Process", () =>
                {
                    ControlEnable(false);
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
              .Condition("Is a coin in the bucket", () => (CoinSlot != null && CoinSlot.takeCoin()) || ControlActive("INSERT"))
              .Do("coin inserted", () =>
                {
                    InsertCoin();
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
                    if (ControlActive("JOYPAD_UP") || ControlActive("KEYB-UP"))
                        mainMenu.PreviousOption();
                    else if (ControlActive("JOYPAD_DOWN") || ControlActive("KEYB-DOWN"))
                        mainMenu.NextOption();
                    else if (ControlActive("JOYPAD_B"))
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

                    if (ControlActive("JOYPAD_B"))
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
                    if (ControlActive("JOYPAD_B"))
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
                    if (ControlActive("JOYPAD_B"))
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
                    if (w != null && ControlActive("JOYPAD_B"))
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
                    if (w != null && ControlActive("JOYPAD_B"))
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
                    if (ControlActive("JOYPAD_UP") || ControlActive("KEYB-UP"))
                        controllerContainer.PreviousOption();
                    else if (ControlActive("JOYPAD_DOWN") || ControlActive("KEYB-DOWN"))
                        controllerContainer.NextOption();

                    GenericWidget w = controllerContainer.GetSelectedWidget();

                    if (w != null)
                    {
                        bool right = ControlActive("JOYPAD_LEFT") || ControlActive("KEYB-LEFT");
                        bool left = ControlActive("JOYPAD_RIGHT") || ControlActive("KEYB-RIGHT");

                        if (ControlActive("JOYPAD_B"))
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

            .Sequence("Cabinets")
              .Condition("On cabinets replacement", () => status == StatusOptions.onChangeCabinets)
              .Condition("Can configure cabinets", () => CanConfigureCabinets())
              .Do("Init", () =>
                {
                    SetCabinetsWidgets();
                    CabinetsWindowDraw();
                    scr.DrawScreen();
                    return TaskStatus.Success;
                })
              .Do("Process", () =>
                {
                    changeContainerSelection(cabinetsToChangeContainer);
                    GenericWidget w = cabinetsToChangeContainer.GetSelectedWidget();
                    if (w != null && ControlActive("JOYPAD_B"))
                    {
                        if (w.name == "exit")
                        {
                            status = StatusOptions.onMainMenu;
                            return TaskStatus.Success;
                        }
                        else if (w.name == "save")
                        {
                            SaveCabinetPositions();
                            SetCabinetsWidgets();
                            cabinetSavedLabel.SetSecondsAndDraw(2);
                        }
                    }
                    cabinetSavedLabel.Draw();
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
                    if (w != null && ControlActive("JOYPAD_B"))
                    {
                        if (w.name == "exit")
                        {
                            status = StatusOptions.onMainMenu;
                            return TaskStatus.Success;
                        }
                        else if (w.name == "teleport")
                        {
                            string sceneDescription = teleportDestination.GetSelectedOption();
                            SceneDocument toScene = sceneDatabase.FindByDescription(sceneDescription);
                            if (toScene == null)
                            {
                                ConfigManager.WriteConsoleError($"[ConfigurationController.tree] scene [{sceneDescription}] not found in scenes database, jump to room001");
                                toScene = sceneDatabase.FindByName("Room001");
                            }
                            ConfigManager.WriteConsole($"[ConfigurationController.tree] teleport to scene [{sceneDescription}]");
                            ControlEnable(false); //free the player
                            teleportation.Teleport(toScene);
                            status = StatusOptions.init;
                            return TaskStatus.Success;
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
                    if (w != null && ControlActive("JOYPAD_B"))
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

                  if (!AGEBasic.Running())
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
                        if (ControlActive("JOYPAD_B"))
                        {
                            AGEBasicWaitForPressAKey = false;
                            AGEBasicWindowDraw();
                            scr.DrawScreen();
                        }
                        return TaskStatus.Continue;
                    }

                    changeContainerSelection(AGEBasicContainer);
                    GenericWidget w = AGEBasicContainer.GetSelectedWidget();
                    if (w != null && ControlActive("JOYPAD_B"))
                    {
                        if (w.name == "exit")
                        {
                            status = StatusOptions.onMainMenu;
                            return TaskStatus.Success;
                        }
                        else if (w.name == "run")
                        {
                            AGEBasicRun();
                            AGEBasicRunTimeout = DateTime.Now.AddSeconds(60); //if not reach in time abort
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
                  status = StatusOptions.init;
                  return TaskStatus.Success;
              })
            .End()

          .End()
          .Build();
    }

    public void ControlEnable(bool enable)
    {
        changeControls.PlayerMode(enable);
        if (enable)
        {
            // inputActionManager?.EnableInput();
            actionMap.Enable();
            return;
        }
        // cant disable, headset goes crazy.
        // inputActionManager?.DisableInput();
        actionMap.Disable();

        return;
    }

    public bool ControlEnabled()
    {
        return actionMap != null && actionMap.enabled;
    }

    public bool ControlActive(string mameControl)
    {
        bool ret = false;

        InputAction action = actionMap.FindAction(mameControl + "_0");
        if (action == null)
        {
            //ConfigManager.WriteConsoleError($"[ConfigurationControl.Active] [{mameControl}] not found in controlMap");
            return false;
        }

        //https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/api/UnityEngine.InputSystem.InputAction.html#UnityEngine_InputSystem_InputAction_WasPerformedThisFrame
        if (action.type == InputActionType.Button)
        {
            if (action.IsPressed())
            {
                return true;
            }
            return false;
        }

        else if (action.type == InputActionType.Value)
        {
            Vector2 val = action.ReadValue<Vector2>();
            switch (mameControl)
            {
                case "JOYPAD_UP":
                    if (val.y > 0.5)
                    {
                        return true;
                    }
                    break;
                case "JOYPAD_DOWN":
                    if (val.y < -0.5)
                    {
                        return true;
                    }
                    break;
                case "JOYPAD_RIGHT":
                    if (val.x > 0.5)
                    {
                        return true;
                    }
                    break;
                case "JOYPAD_LEFT":
                    if (val.x < -0.5)
                    {
                        return true;
                    }
                    break;
            }
        }
        return ret;
    }

    private void changeContainerSelection(GenericWidgetContainer gwc)
    {
        if (ControlActive("JOYPAD_UP") || ControlActive("KEYB-UP"))
            gwc.PreviousOption();
        else if (ControlActive("JOYPAD_DOWN") || ControlActive("KEYB-DOWN"))
            gwc.NextOption();
        else if (ControlActive("JOYPAD_LEFT") || ControlActive("KEYB-LEFT"))
            gwc.GetSelectedWidget()?.PreviousOption();
        else if (ControlActive("JOYPAD_RIGHT") || ControlActive("KEYB-RIGHT"))
            gwc.GetSelectedWidget()?.NextOption();
        return;
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(ConfigurationController))]
public class ConfigurationControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ConfigurationController myScript = (ConfigurationController)target;
        if(GUILayout.Button("InsertCoin"))
        {
          myScript.InsertCoin();
        }
    }
}
#endif

