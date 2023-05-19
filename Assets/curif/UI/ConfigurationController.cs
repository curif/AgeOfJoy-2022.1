using System.Collections;
using System;
using System.Collections.Generic;
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
    if (! isGlobal && roomConfiguration == null)
      throw new ArgumentException("Room configuration is missing in cabinet configuration screen gameobject ");

    if (! isGlobal)
      return roomConfiguration.Configuration;
    
    return globalConfiguration.Configuration;
  }

  public bool CanConfigureRoom()
  {
    ConfigManager.WriteConsole($"[ConfigurationHelper.CanConfigureRoom] room: {roomConfiguration?.yamlPath}");
    return roomConfiguration != null;
  }

  public void Save(bool saveGlobal)
  {
    string yamlPath;
    ConfigInformation config;

    if (saveGlobal)
    {
      yamlPath = globalConfiguration.yamlPath;
      config = globalConfiguration.Configuration;
    }
    else
    {
      yamlPath = roomConfiguration.yamlPath;
      config = roomConfiguration.Configuration;
    }
    ConfigManager.WriteConsole($"[ConfigurationController] save configuration {config} to {yamlPath}");
    config.ToYaml(yamlPath);

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

public class ConfigurationController : MonoBehaviour
{
    public ScreenGenerator scr;
    public CoinSlotController CoinSlot;
    public InputActionMap actionMap;
    public ChangeControls changeControls;

    [Tooltip("Set only to change room configuration, if not setted will use the Global")]
    public RoomConfiguration roomConfiguration;
    [Tooltip("Set to change the Global or the system will find it")]
    public GlobalConfiguration globalConfiguration;

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
      onReset,
      exit
    }
    private StatusOptions status;
    private C64BootScreen bootScreen;
    private LibretroMameCore.Waiter onBootDelayWaiter; 

    private string NPCStatus;
    private GenericOptions NPCStatusOptions; 
    private GenericWindow npcWindow;

    private GenericBool isGlobalConfigurationWidget;
    private GenericWidgetContainer changeModeContainer;

    private GenericWindow audioWindow;
    private GenericWidgetContainer audioContainer;

    private GenericWidgetContainer resetContainer;

    private DefaultControlMap map;

    private ConfigurationHelper configHelper;

    // Start is called before the first frame update
    void Start()
    {
      map = new();
      ControlMapConfiguration conf = new DefaultControlMap();
      conf.AddMap("KEYB-UP", "keyboard-w");
      conf.AddMap("KEYB-DOWN", "keyboard-s");
      conf.AddMap("KEYB-LEFT", "keyboard-a");
      conf.AddMap("KEYB-RIGHT", "keyboard-d");

      actionMap = ControlMapInputAction.inputActionMapFromConfiguration(conf);

      StartCoroutine(run());
    }


    public void NPCScreen()
    {
      //set the init value
      string actualNPCStatus = ConfigInformation.NPC.validStatus[0];
      ConfigInformation config = configHelper.getConfigInformation(isGlobalConfigurationWidget.value);
      if (config?.npc != null)
        actualNPCStatus = config.npc.status;

      scr.Clear();
      npcWindow.Draw();
      
      //some help
      scr.Print(2,16, "left/right to change");
      scr.Print(2,17, "b to select and exit");

      ConfigManager.WriteConsole($"[ConfigurationController.GoToNPCConfiguration] NPC status: {actualNPCStatus}");
      NPCStatusOptions.SetCurrent(actualNPCStatus);
      NPCStatusOptions.Draw();
    }
    
    private void NPCSave()
    {
      bool isGlobal = isGlobalConfigurationWidget.value;
      ConfigInformation config = configHelper.getConfigInformation(isGlobal);
      if (config == null)
        config = new();
      if (config.npc == null)
        config.npc = new();
      config.npc.status = NPCStatusOptions.GetSelectedOption();
      configHelper.Save(isGlobal);
    }

    public void resetWindowDraw()
    {
      scr.Clear();
      resetContainer.Draw();
      if (isGlobalConfigurationWidget.value)
      {
        scr.Print(2,1, "Back global configuration");
        scr.Print(2,2, "to default.");
        scr.Print(2,3, "Lost all configured data.");
      }
      else
      {
        scr.Print(2,1, "Room configuration will");
        scr.Print(2,2, "be deleted.");
        scr.Print(2,3, "You will lost all room configured data");
        scr.Print(2,4, "except controllers information.");
      }
      scr.Print(2,16, "up/down to change");
      scr.Print(2,17, "b to select and exit");
    }
    
    public void resetSave()
    {
      configHelper.Reset(isGlobalConfigurationWidget.value);
      ConfigManager.WriteConsole($"[ConfigurationController.resetSave] Reset to default Global: {isGlobalConfigurationWidget.value}");
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

      if (config?.audio?.inGameBackground?.muted!= null)
        ((GenericBool)audioContainer.GetWidget("InGameBackgroundMuted")).SetValue((bool)config.audio.inGameBackground.muted);
      else
        ((GenericBool)audioContainer.GetWidget("InGameBackgroundMuted")).SetValue((bool)ingamebkg.muted);

      scr.Clear();
      audioWindow.Draw();
      audioContainer.Draw();
    }
    private void audioSave()
    {
      bool isGlobal = isGlobalConfigurationWidget.value;
      ConfigInformation config = configHelper.getConfigInformation(isGlobal);
      config ??= new();
      config.audio ??= new();
      config.audio.background ??= ConfigInformation.BackgroundDefault();
      config.audio.inGameBackground ??= ConfigInformation.BackgroundInGameDefault();
      config.audio.background.volume = (uint)((GenericOptionsInteger)audioContainer.GetWidget("BackgroundVolume")).GetSelectedOption();
      config.audio.background.muted = ((GenericBool)audioContainer.GetWidget("BackgroundMuted")).value;
      config.audio.inGameBackground.volume = (uint)((GenericOptionsInteger)audioContainer.GetWidget("InGameBackgroundVolume")).GetSelectedOption();
      config.audio.inGameBackground.muted = ((GenericBool)audioContainer.GetWidget("InGameBackgroundMuted")).value;

      configHelper.Save(isGlobal);
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
      }
    }

    public void changeModeWindowDraw()
    {
      scr.Clear();
      if (configHelper.CanConfigureRoom())
      {
        isGlobalConfigurationWidget.enabled = true;
        scr.Print(2,1, "select the configuration mode:");
        scr.Print(2,2, "- global for all rooms");
        scr.Print(2,3, "- or for this room only.");
        scr.Print(2,4, "up/down to change");
      }
      else
      {
        isGlobalConfigurationWidget.enabled = false;
        scr.Print(2,1, "only global configuration.");
        scr.Print(2,2, "configure all rooms/games.");
        scr.Print(2,3, "for one room configuration go to");
        scr.Print(2,4, "a gallery room");
      }
      scr.Print(2,19, "b to select");

      changeModeContainer.Draw();
    }

    IEnumerator run()
    {
      float timeBetweenCycles = 1f/5f;
      
      //wait for the room to load.
      yield return new WaitForSeconds(2f);
      
      configHelper = new(globalConfiguration, roomConfiguration);

      string[] options = new string[] {
        "Audio configuration", 
        "NPC configuration", 
        "Controllers", 
        "reset", 
        "change mode", 
        "exit"
      };
      string[] helpText = new string[] { 
        "Change sound volume", 
        "To change the NPC behavior", 
        "Map your control to play games", 
        "global or room configuration", 
        "back to default",
        "exit configuration" 
      };

      mainMenu = new(scr, "AGE of Joy - Main configuration", options, helpText);
      bootScreen = new(scr);

      audioWindow = new(scr, 2, 4, "audiowin", 36, 14, " Audio Configuration ");
      audioContainer = new(scr, "audioContainer", 0, 0);
      audioContainer.Add(new GenericLabel(scr, "BackgroundLabel", "Background Audio", 4, 6))
                    .Add(new GenericBool(scr, "BackgroundMuted", "mute:", false, 6, 8))
                    .Add(new GenericOptionsInteger(scr, "BackgroundVolume", "volume:", 0, 100, 6, 9))
                    .Add(new GenericLabel(scr, "InGameBackgroundLabel", "Background in game audio", 4, 11))
                    .Add(new GenericBool(scr, "InGameBackgroundMuted", "mute:", false, 6, 13))
                    .Add(new GenericOptionsInteger(scr, "InGameBackgroundVolume", "volume:", 0, 100, 6, 14))
                    .Add(new GenericButton(scr, "save", "save & exit", 4, 16, true))
                    .Add(new GenericButton(scr, "exit", "exit", 18, 16, true))
                    .Add(new GenericLabel(scr, "l1", "left/right/b to change", 2, 20))
                    .Add(new GenericLabel(scr, "l2", "up/down to move", 2, 21));

      changeModeContainer = new(scr, "changeMode", 0, 0);
      isGlobalConfigurationWidget = new GenericBool(scr, "isGlobal", "working with global:", !configHelper.CanConfigureRoom(), 4, 10);
      isGlobalConfigurationWidget.enabled = configHelper.CanConfigureRoom();
      changeModeContainer.Add(new GenericWindow(scr, 2, 8, "win", 36, 6, " mode "))
                         .Add(isGlobalConfigurationWidget)
                         .Add(new GenericButton(scr, "exit", "exit", 4, 11, true));

      //NPC configuration options.
      // take the statuses from the static value options in the information NPC class.
      npcWindow = new(scr, 2, 8, "npcWindow", 36, 6, " NPC Configuration ", true);
      NPCStatusOptions = new(scr, "npc", "NPC Behavior:", new List<string>(ConfigInformation.NPC.validStatus), 4, 10);

      resetContainer = new(scr, "reset", 0, 0);
      resetContainer.Add(new GenericWindow(scr, 2, 8, "win", 36, 6, " reset "))
                    .Add(new GenericButton(scr, "reset", "reset and save", 4, 11, true))
                    .Add(new GenericButton(scr, "exit", "exit", 4, 12, true));

      status = StatusOptions.init;
      tree = buildBT();
      while (true)
      {
        tree.Tick();
        yield return new WaitForSeconds(timeBetweenCycles);
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
                scr.Clear();
                scr.PrintCentered(10, "Insert coin to start", true);
                return TaskStatus.Success;
              })
          .End()

          .Sequence("Insert coin")
            .Condition("Waiting for coin", () => status == StatusOptions.waitingForCoin)
            .Condition("Is a coin in the bucket",() => (CoinSlot != null && CoinSlot.takeCoin()) || ControlActive("INSERT"))
            .Do("coin inserted", () => 
              {
                InsertCoin();
                return TaskStatus.Success;
              })
          .End()

          .Sequence("Boot")
            .Condition("Booting", () => status == StatusOptions.onBoot)
            .WaitTime(0.1f)
            .Condition("Finished lines", () => bootScreen.PrintNextLine())
            .Do("Start main menu", () => 
              {
                status = StatusOptions.onMainMenu;
                return TaskStatus.Success;
              })
          .End()

          .Sequence("Main menu")
            .Condition("On main menu",  () => status == StatusOptions.onMainMenu)
            .Do("Init", () =>
              {
                scr.Clear();
                mainMenuDraw();
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
                  return TaskStatus.Continue;

                ConfigManager.WriteConsole($"[ConfigurationController] option selected: {mainMenu.GetSelectedOption()}");
                if (mainMenu.GetSelectedOption() == "NPC configuration")
                  status = StatusOptions.onNPCMenu;
                else if (mainMenu.GetSelectedOption() == "exit")
                  status = StatusOptions.exit;
                else if (mainMenu.GetSelectedOption() == "Audio configuration")
                  status = StatusOptions.onAudio;
                else if (mainMenu.GetSelectedOption() == "change mode")
                  status = StatusOptions.onChangeMode;
                else if (mainMenu.GetSelectedOption() == "reset")
                  status = StatusOptions.onReset;
                mainMenu.Deselect();
                return TaskStatus.Success;
              })
          .End()

          .Sequence("NPC Configuration")
            .Condition("On NPC Config", () => status == StatusOptions.onNPCMenu)
            .Do("Init", () =>
              {
                NPCScreen();
                return TaskStatus.Success;
              })
            .Do("Process", () =>
              {
                if (ControlActive("JOYPAD_LEFT")|| ControlActive("KEYB-LEFT"))
                  NPCStatusOptions.PreviousOption();
                else if (ControlActive("JOYPAD_RIGHT")|| ControlActive("KEYB-RIGHT"))
                  NPCStatusOptions.NextOption();
                else if (ControlActive("JOYPAD_B"))
                {
                  return TaskStatus.Success;
                }
                return TaskStatus.Continue;
              })
            .Do("Save", () =>
              {
                NPCSave();
                status = StatusOptions.onMainMenu;
                return TaskStatus.Success;
              })
          .End()

          .Sequence("Audio Configuration")
            .Condition("On Config", () => status == StatusOptions.onAudio)
            .Do("Init", () =>
              {
                audioScreen();
                audioContainer.Draw();
                return TaskStatus.Success;
              })
            .Do("Process", () =>
              {
                if (ControlActive("JOYPAD_UP")|| ControlActive("KEYB-UP"))
                  audioContainer.PreviousOption();
                else if (ControlActive("JOYPAD_DOWN")|| ControlActive("KEYB-DOWN"))
                  audioContainer.NextOption();
                else if (ControlActive("JOYPAD_LEFT")|| ControlActive("KEYB-LEFT"))
                  audioContainer.GetSelectedWidget()?.PreviousOption();
                else if (ControlActive("JOYPAD_RIGHT")|| ControlActive("KEYB-RIGHT"))
                  audioContainer.GetSelectedWidget()?.NextOption();
                else if (ControlActive("JOYPAD_B"))
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
                return TaskStatus.Continue;
              })
          .End() 

          .Sequence("Change Mode")
            .Condition("On change mode", () => status == StatusOptions.onChangeMode)
            .Do("Init", () =>
              {
                changeModeWindowDraw();
                return TaskStatus.Success;
              })
            .Do("Process", () =>
              {
                if (ControlActive("JOYPAD_UP")|| ControlActive("KEYB-UP"))
                  changeModeContainer.PreviousOption();
                else if (ControlActive("JOYPAD_DOWN")|| ControlActive("KEYB-DOWN"))
                  changeModeContainer.NextOption();
                if (ControlActive("JOYPAD_B"))
                {
                  GenericWidget w = changeModeContainer.GetSelectedWidget();
                  if (w != null) 
                  {
                    if (w.name == "exit")
                    {
                      status = StatusOptions.onMainMenu;
                      return TaskStatus.Success;
                    }
                    w.Action();
                  }
                  return TaskStatus.Success;
                }
                return TaskStatus.Continue;
              })
          .End()
          
          .Sequence("back to default")
            .Condition("On back to default", () => status == StatusOptions.onReset)
            .Do("Init", () =>
              {
                resetWindowDraw();
                return TaskStatus.Success;
              })
            .Do("Process", () =>
              {
                if (ControlActive("JOYPAD_UP")|| ControlActive("KEYB-UP"))
                  resetContainer.PreviousOption();
                else if (ControlActive("JOYPAD_DOWN")|| ControlActive("KEYB-DOWN"))
                  resetContainer.NextOption();
                if (ControlActive("JOYPAD_B"))
                {
                  GenericWidget w = resetContainer.GetSelectedWidget();
                  if (w != null)
                  {
                    if (w.name == "exit")
                    {
                      status = StatusOptions.onMainMenu;
                    }
                    else if (w.name == "reset")
                    {
                      resetSave();
                      status = StatusOptions.onMainMenu;
                    }
                  }
                  return TaskStatus.Success;
                }
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
        actionMap.Enable(); 
        return;
      }
      actionMap.Disable();
      return;
    }

    public bool ControlActive(string mameControl)
    {
      bool ret = false;

      InputAction action = actionMap.FindAction(mameControl);
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

