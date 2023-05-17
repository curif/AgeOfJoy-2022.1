using System.Collections;
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

public class ConfigurationController : MonoBehaviour
{
    public ScreenGenerator scr;
    public CoinSlotController CoinSlot;
    public InputActionMap actionMap;
    public ChangeControls changeControls;

    [Tooltip("Set only to change room configuration, if not setted will use the Global")]
    public GameObject RoomConfigurationGameObject;
    [Tooltip("Set to change the Global (if not setted the Room Config)")]
    public GameObject GlobalConfigurationGameObject;

    [Tooltip("Set room or Global configuration")]
    public ConfigInformation Configuration;

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
      exit
    }
    private StatusOptions status;
    private C64BootScreen bootScreen;
    private LibretroMameCore.Waiter onBootDelayWaiter; 

    private string NPCStatus;
    private GenericOptions NPCStatusOptions; 
    private GenericWindow npcWindow;

    private GenericWindow audioWindow;
    private GenericWidgetContainer audioContainer;

    private DefaultControlMap map;
    private GlobalConfiguration globalConfiguration;
    private RoomConfiguration roomConfiguration;

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
      
      if (GlobalConfigurationGameObject != null) 
      {
        globalConfiguration = GlobalConfigurationGameObject.GetComponent<GlobalConfiguration>();
      }
      if (RoomConfigurationGameObject != null)
      {
        roomConfiguration = RoomConfigurationGameObject.GetComponent<RoomConfiguration>();
      }

      StartCoroutine(run());
    }
    private void setMainMenu()
    {
    }

    private bool isRoomConfiguration()
    {
      return roomConfiguration != null;
    }
    private bool isGlobalConfiguration()
    {
      return globalConfiguration != null;
    }

    public void NPCScreen()
    {
      //set the init value
      string actualNPCStatus = "UNDEFINED";
      if (isRoomConfiguration())
      {
        if (roomConfiguration.Configuration?.npc != null)
          actualNPCStatus = roomConfiguration.Configuration.npc.status;
      }
      else if (isGlobalConfiguration())
      {
        if (globalConfiguration.Configuration?.npc != null)
          actualNPCStatus = globalConfiguration.Configuration.npc.status;
      }

      scr.Clear();
      npcWindow.Draw();
      
      //some help
      scr.Print(2,16, "left/right to change");
      scr.Print(2,17, "b to select and exit");

      ConfigManager.WriteConsole($"[ConfigurationController.GoToNPCConfiguration] NPC status: {actualNPCStatus}");
      NPCStatusOptions.SetCurrent(actualNPCStatus);
      NPCStatusOptions.Draw();
    }

    private void audioScreen()
    {
      //set the init value
      ConfigInformation config = null;

      if (isRoomConfiguration())
        config = roomConfiguration.Configuration;
      else if (isGlobalConfiguration())
        config = globalConfiguration.Configuration;

      if (config?.audio?.background?.volume != null)
        ((GenericOptionsInteger)audioContainer.GetWidget("BackgroundVolume")).SetCurrent((int)config.audio.background.volume);
      if (config?.audio?.inGameBackground?.volume != null)
        ((GenericOptionsInteger)audioContainer.GetWidget("InGameBackgroundVolume")).SetCurrent((int)config.audio.inGameBackground.volume);

      if (config?.audio?.background?.muted != null)
        ((GenericBool)audioContainer.GetWidget("BackgroundMuted")).SetValue((bool)config.audio.background.muted);
      if (config?.audio?.inGameBackground?.muted!= null)
        ((GenericBool)audioContainer.GetWidget("InGameBackgroundMuted")).SetValue((bool)config.audio.inGameBackground.muted);
      
      scr.Clear();
      audioWindow.Draw();
      audioContainer.Draw();

    }
    public void InsertCoin()
    {
      ControlEnable(true);
      status = StatusOptions.onBoot;
      scr.Clear();
      bootScreen.Reset();
    }

    IEnumerator run()
    {
      float timeBetweenCycles = 1f/5f;
      string[] options = new string[] {"Audio configuration", "NPC configuration", "Controllers", "exit"};
      string[] helpText = new string[] { "Change sound volume", "To change the NPC behavior", "Map your control to play games", "exit configuration" };

      mainMenu = new(scr, "AGE of Joy - Main configuration", options, helpText);
      bootScreen = new(scr);

      audioWindow = new(scr, 2, 4, 36, 14, " Audio Configuration ");
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

      //NPC configuration options.
      // take the statuses from the static value options in the information NPC class.
      npcWindow = new(scr, 2, 8, 36, 6, " NPC Configuration ", true);
      NPCStatusOptions = new(scr, "npc", "NPC Behavior:", new List<string>(ConfigInformation.NPC.validStatus), 4, 10);

      yield return new WaitForSeconds(2f);

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
                mainMenu.Deselect();
                mainMenu.DrawMenu();
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
                {
                  status = StatusOptions.onNPCMenu;
                }
                else if (mainMenu.GetSelectedOption() == "exit")
                {
                  status = StatusOptions.exit;
                }
                else if (mainMenu.GetSelectedOption() == "Audio configuration")
                {
                  status = StatusOptions.onAudio;
                }
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
                NPCStatusOptions.Draw();
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
                if (isGlobalConfiguration())
                {
                  if (globalConfiguration.Configuration.npc == null)
                    globalConfiguration.Configuration.npc = new();
                  globalConfiguration.Configuration.npc.status = NPCStatusOptions.GetSelectedOption();
                  globalConfiguration.Save();
                }
                else if (isRoomConfiguration())
                {
                  if (roomConfiguration.Configuration.npc == null)
                    roomConfiguration.Configuration.npc = new();
                  roomConfiguration.Configuration.npc.status = NPCStatusOptions.GetSelectedOption();
                  roomConfiguration.Save();
                }
                
                status = StatusOptions.onMainMenu;

                return TaskStatus.Success;
              })
          .End()

          .Sequence("Audio Configuration")
            .Condition("On Config", () => status == StatusOptions.onAudio)
            .Do("Init", () =>
              {
                audioScreen();
                return TaskStatus.Success;
              })
            .Do("Process", () =>
              {
                audioContainer.Draw();
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
                      return TaskStatus.Success;
                    w.Action();
                  }
                }
                return TaskStatus.Continue;
              })
            .Do("Save", () =>
              {
                if (isGlobalConfiguration())
                {
                }
                else if (isRoomConfiguration())
                {
                }
                
                status = StatusOptions.onMainMenu;

                return TaskStatus.Success;
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

