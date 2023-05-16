using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ConfigurationController : MonoBehaviour
{
    public ScreenGenerator scr;

    private DefaultControlMap map;
    public CoinSlotController CoinSlot;
    public InputActionMap actionMap;
    public ChangeControls changeControls;

  [Tooltip("Set only to change room configuration, if not setted will use the Global")]
  public GameObject RoomConfigurationGameObject;
  private RoomConfiguration roomConfiguration;
  [Tooltip("Set to change the Global (if not setted the Room Config)")]
  public GameObject GlobalConfigurationGameObject;
  private GlobalConfiguration globalConfiguration;

  [Tooltip("Set room or Global configuration")]
  public ConfigInformation Configuration;

    private GenericMenu mainMenu;
    private enum StatusOptions
    {
      init,
      waitingForCoin,
      onMainMenu,
      onBoot,
      onNPCMenu
    }
    private StatusOptions status;
    private C64BootScreen bootScreen;
    private LibretroMameCore.Waiter onBootDelayWaiter; 

    private string NPCStatus;
    private GenericOptions NPCStatusOptions; 

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
      string[] options = new string[] {"Sound configuration", "NPC configuration", "Controllers"};
      string[] helpText = new string[] { "Change sound volume", "To change the NPC behavior", "Map your control to play games" };
      mainMenu = new(scr, "AGE of Joy - Main configuration", options, helpText);
    }

    private bool isRoomConfiguration()
    {
      return roomConfiguration != null;
    }
    private bool isGlobalConfiguration()
    {
      return globalConfiguration != null;
    }

    private void reset()
    {
      setMainMenu();
      ControlEnable(false);
      showInsertCoinMessage();
      status = StatusOptions.waitingForCoin;
    }

    public void InsertCoin()
    {
      ControlEnable(true);
      status = StatusOptions.onBoot;
      scr.Clear();
      onBootDelayWaiter = new(3); 
    }
    public void GoToMainMenu()
    {
      status = StatusOptions.onMainMenu;
      scr.Clear();
      mainMenu.DrawMenu();
    }
    public void GoToNPCConfiguration()
    {
      status = StatusOptions.onNPCMenu;
      scr.Clear();
      scr.PrintCentered(9, "-- NPC configuration --");

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
      ConfigManager.WriteConsole($"[ConfigurationController.GoToNPCConfiguration] NPC status: {actualNPCStatus}");
      NPCStatusOptions.SetCurrent(actualNPCStatus);
      NPCStatusOptions.Print();
    }

    IEnumerator run()
    {
      float timeBetweenCycles = 1f/3f;

      setMainMenu();

      status = StatusOptions.init;
      
      //C64 Boot screen
      bootScreen = new(scr);
      onBootDelayWaiter = new(3); 

      //NPC configuration options.
      // take the statuses from the static value options in the information NPC class.
      NPCStatusOptions = new(scr, "NPC Behavior:", new List<string>(ConfigInformation.NPC.validStatus), 1, 20);

      yield return new WaitForSeconds(2f);

      while (true)
      {
        ConfigManager.WriteConsole($"[ConfigurationController.run] status: {status}");
        if (status == StatusOptions.init)
        {
          reset();
        }

        bool takeCoin = (CoinSlot != null && CoinSlot.takeCoin()) || ControlActive("INSERT");
        if (takeCoin && status == StatusOptions.waitingForCoin)
        {
          ConfigManager.WriteConsole($"[ConfigurationController.run] INSERT COIN status: {status}");
          InsertCoin();    
        }

        if (status == StatusOptions.waitingForCoin)
          yield return new WaitForSeconds(timeBetweenCycles);

        if (status == StatusOptions.onBoot)
        {
          if (onBootDelayWaiter.Finished())
            GoToMainMenu();
          else
            bootScreen.PrintNextLine();
          yield return new WaitForSeconds(timeBetweenCycles);
        }

        if (ControlActive("EXIT"))
        {
          ConfigManager.WriteConsole($"[ConfigurationController.run] EXIT status: {status}");
          reset();
          yield return new WaitForSeconds(timeBetweenCycles);
        }

        if (status == StatusOptions.onMainMenu)
        {
          if (ControlActive("JOYPAD_UP") || ControlActive("KEYB-UP"))
            mainMenu.PreviousOption();
          else if (ControlActive("JOYPAD_DOWN")|| ControlActive("KEYB-DOWN"))
            mainMenu.NextOption();
          else if (ControlActive("JOYPAD_B"))
            mainMenu.Select();

          if (mainMenu.IsSelected())
          {
            ConfigManager.WriteConsole($"[ConfigurationController.Update] option selected: {mainMenu.GetSelectedOption()}");
            if (mainMenu.GetSelectedOption() == "NPC configuration")
            {
              status = StatusOptions.onNPCMenu;
              GoToNPCConfiguration();
            }
            mainMenu.Deselect();
          }
        }
        else if (status == StatusOptions.onNPCMenu)
        {
          NPCStatusOptions.Print();
          if (ControlActive("JOYPAD_LEFT")|| ControlActive("KEYB-LEFT"))
            NPCStatusOptions.PreviousOption();
          else if (ControlActive("JOYPAD_RIGHT")|| ControlActive("KEYB-RIGHT"))
            NPCStatusOptions.NextOption();
          else if (ControlActive("JOYPAD_B"))
          {
            if (isGlobalConfiguration())
            {
              if (globalConfiguration.Configuration.npc == null)
                globalConfiguration.Configuration.npc = new();
              globalConfiguration.Configuration.npc.status = NPCStatusOptions.GetSelectedOption();
            }
            else if (isRoomConfiguration())
            {
              if (roomConfiguration.Configuration.npc == null)
                roomConfiguration.Configuration.npc = new();
              roomConfiguration.Configuration.npc.status = NPCStatusOptions.GetSelectedOption();
            }
            // TODO SAVE
            GoToMainMenu();
          }
        }
        
        yield return new WaitForSeconds(timeBetweenCycles);
      }
    }

    private void showInsertCoinMessage()
    {
      scr.Clear();
      scr.PrintCentered(10, "Insert coin to start", true);
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

