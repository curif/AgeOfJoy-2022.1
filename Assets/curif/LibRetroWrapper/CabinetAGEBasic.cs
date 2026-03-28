//#define EVENT_LOOP_DEBUG
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Serialization; //https://github.com/aaubry/YamlDotNet
using UnityEditor;
using System.Linq;
using YamlDotNet.Core;
using static CabinetInformation;
using static EventInformation;
using UnityEngine.UIElements;
using System.IO;


[Serializable]
public class AGEBasicVariable
{
    public string name; //variable name
    public string type; //STRING or NUMBER
    public string value = ""; //first asigned value.
}

[Serializable]
public class CabinetAGEBasicInformation
{
    public bool? active = null;
    public bool debug = false;

    [YamlMember(Alias = "system-skin", ApplyNamingConventions = false)]
    public string system_skin = "c64";

    [YamlMember(Alias = "max-execution-lines", ApplyNamingConventions = false)]
    public int maxExecutionLines = -1;

    [YamlMember(Alias = "max-lines-per-frame", ApplyNamingConventions = false)]
    public int maxLinesPerFrame = -1;

    [YamlMember(Alias = "max-milliseconds-per-frame", ApplyNamingConventions = false)]
    public double maxMillisecondsPerFrame = -1;

    // Serialize this field to show it in the editor
    [SerializeField]
    private List<AGEBasicVariable> variables;

    [YamlMember(Alias = "variables", ApplyNamingConventions = false)]
    public List<AGEBasicVariable> Variables { get { return variables; } set { variables = value; } }
        

    [YamlMember(Alias = "after-start", ApplyNamingConventions = false)]
    public string afterStart;
    [YamlMember(Alias = "after-load", ApplyNamingConventions = false)]
    public string afterLoad;
    [YamlMember(Alias = "after-insert-coin", ApplyNamingConventions = false)]
    public string afterInsertCoin;
    [YamlMember(Alias = "after-leave", ApplyNamingConventions = false)]
    public string afterLeave;

    public List<EventInformation> events = new();

    public void Validate(string cabName)
    {
        foreach (EventInformation e in events)
        {
            Exception ex = e.Validate(cabName);
            if (ex != null)
                throw ex;
        }
    }

    public CabinetValidationException IsValid(string cabName)
    {
        foreach (EventInformation e in events)
        {
            CabinetValidationException ex = e.Validate(cabName);
            if (ex != null)
                return ex;
        }
        return null;
    }



    public bool HasEvents()
    {
        return events != null && events.Count > 0;
    }
    
}

[Serializable]
public class EventInformation
{
    //event identification
    [YamlMember(Alias = "event", ApplyNamingConventions = false)]
    public string eventId;
    static string[] validEvents = { "on-timer", "on-always",  "on-insert-coin", "on-custom",
                                    "on-collision-start", "on-collision-stay", "on-collision-end",
                                    "on-touch-start", "on-grab-start", "on-touch-end",
                                    "on-grab-end",  "on-lightgun-start", "on-lightgun-stay", "on-lightgun-exit",
                                    "on-control-active-pressed", "on-control-active-held", "on-control-active-released",
                                    "on-sprite-collision-start", "on-sprite-collision-end",
                                    "on-memory-change",
                                    "on-led-change"};

    static string[] requirePartName = { "on-collision-start", "on-collision-stay", "on-collision-end", 
                                        "on-touch-start", "on-grab-start", 
                                        "on-touch-end", "on-grab-end" ,
                                        "on-lightgun-start", "on-lightgun-stay", "on-lightgun-exit"};
    public string name = "";
    public string program;
    [YamlMember(Alias = "goto", ApplyNamingConventions = false)]
    public int line;
    public double delay = 0;
    public ControlInformation control;
    public string part;
    [YamlMember(Alias = "impact-parts", ApplyNamingConventions = false)]
    public List<string> partImpacts; //name of the colliding part.
    //public List<string> parts; //OR parts
    public string spriteA; // first sprite name for sprite collision events
    public string spriteB; // second sprite name for sprite collision events

    // on-memory-change fields
    public uint address;   // memory offset (raw; ignored when cheat is set)
    public uint region;    // memory region (0=SAVE_RAM, 1=RTC, 2=SYSTEM_RAM, 3=VIDEO_RAM)
    [YamlMember(Alias = "var", ApplyNamingConventions = false)]
    public string varName; // AGEBasic variable to inject the new value into
    public string cheat;   // cheat description from XML (alternative to address+region)

    // on-led-change fields
    [YamlMember(Alias = "led", ApplyNamingConventions = false)]
    public int ledIndex;   // LED index 0–7

    // Serialize this field to show it in the editor
    [SerializeField]
    private List<AGEBasicVariable> variables;

    [YamlMember(Alias = "variables", ApplyNamingConventions = false)]
    public List<AGEBasicVariable> Variables { get { return variables; } set { variables = value; } }

    [YamlMember(Alias = "when", ApplyNamingConventions = false)]
    public Condition condition;
    public class Condition
    {
        public string variable;
        public string value;
        public string comparison = "=";
        [YamlMember(Alias = "compare-to", ApplyNamingConventions = false)]
        public string compareToVariable;  // Another variable to compare against (optional)
        public List<Condition> and;      // Sub-conditions that are combined with AND
        public List<Condition> or;       // Sub-conditions that are combined with OR
    }

    public CabinetValidationException Validate(string cabName)
    {
        if (string.IsNullOrEmpty(eventId))
            return new CabinetValidationException(cabName, $"AGEBasic Event Id unespecified");

        if (Array.IndexOf(validEvents, eventId) < 0)
            return new CabinetValidationException(cabName, $"AGEBasic Event [{eventId}] unknown");

        if (Array.IndexOf(requirePartName, eventId) >= 0 && string.IsNullOrEmpty(part))
            return new CabinetValidationException(cabName, $"AGEBasic Event {eventId} requires a part name");

        if (string.IsNullOrEmpty(program))
            return new CabinetValidationException(cabName, $"AGEBasic Event {eventId} doesn't have a program attached");

        if (control != null)
        {
            CabinetValidationException ex = control.IsValid(cabName);
            if (ex != null)
                return ex;
        }
        return null;
    }
}

[Serializable]
public class ControlInformation
{
    [YamlMember(Alias = "libretro-id", ApplyNamingConventions = false)]
    public string mameControl;
    public int port = 0;
    public CabinetValidationException IsValid(string cabName)
    {
        if (string.IsNullOrEmpty(mameControl))
            return new CabinetValidationException(cabName, $"Event cabinet {cabName} control libretro-id isn't specified");

        return null;

    }
}

/// Event execution
public class Event
{
    public EventInformation eventInformation;
    
    //AGEBasic
    public BasicVars vars;
    public basicAGE AGEBasic;

    protected DateTime startTime;

    private int triggeredCount = 0;
    protected int triggeredCountMAX = int.MaxValue;

    public bool Initialized { get => status == Status.initialized; }

    protected Status status = Status.needsinitilization;

    public enum Status
    {
        needsinitilization = 0,
        initialized,
        error
    }

    public Event(EventInformation eventInformation, BasicVars vars, basicAGE agebasic, int triggerCountMAX = int.MaxValue)
    {
        this.eventInformation = eventInformation;
        this.vars = vars;
        this.triggeredCountMAX = triggerCountMAX;
        AGEBasic = agebasic;
    }

    protected bool RegisterTrigger(bool isTriggered)
    {
        if (isTriggered && triggeredCount < triggeredCountMAX)
            triggeredCount++;
        return triggeredCount > 0;
    }
    public bool WasTriggered()
    {
        return triggeredCount > 0;
    }
    public virtual void Init() {
        if (status == Status.initialized)
            return;

        Reset();
        status = Status.initialized;
    }

    public virtual void Reset()
    {
        triggeredCount = 0;
        startTime = DateTime.Now;
    }

    public virtual void Finish()
    {
        triggeredCount--;
        startTime = DateTime.Now;
    }

    protected virtual bool IsTime()
    {
        return (DateTime.Now - startTime).TotalSeconds >= eventInformation.delay;
    }

    public virtual void EvaluateTrigger() { 
        if (eventInformation.delay > 0)
        {
            if (!WasTriggered())
            {
                RegisterTrigger(IsTime());
            }
        }
        else
        {
            RegisterTrigger(false); 
        }
    }

    public virtual void Dispose()
    {
        status = Status.needsinitilization;
        return;
    }
    private bool EvaluateCondition(Condition condition)
    {
        // Handle 'and' conditions
        if (condition.and != null && condition.and.Count > 0)
        {
            foreach (var subCondition in condition.and)
            {
                if (!EvaluateCondition(subCondition))
                    return false;
            }
            return true;
        }

        // Handle 'or' conditions
        if (condition.or != null && condition.or.Count > 0)
        {
            foreach (var subCondition in condition.or)
            {
                if (EvaluateCondition(subCondition))
                    return true;
            }
            return false;
        }

        // Handle single condition: compare variable to either another variable or a constant value
        if (!string.IsNullOrEmpty(condition.variable))
        {
            if (vars.Exists(condition.variable))
            {
                BasicValue v = vars.GetValue(condition.variable);
                BasicValue compareTo;

                // Check if we're comparing to another variable or a constant value
                if (!string.IsNullOrEmpty(condition.compareToVariable))
                {
                    // Compare two variables
                    if (!vars.Exists(condition.compareToVariable))
                        return false; // Fail if the second variable doesn't exist
                    compareTo = vars.GetValue(condition.compareToVariable);
                }
                else if (!string.IsNullOrEmpty(condition.value))
                {
                    // Compare to a constant value
                    compareTo = v.IsString()
                        ? new BasicValue(condition.value)
                        : new BasicValue(condition.value, forceType: BasicValue.BasicValueType.Number);
                }
                else
                {
                    //throw new InvalidOperationException("Condition must have either 'value' or 'compareToVariable'");
                    return false;
                }

                // Perform comparison based on the 'comparison' field
                switch (condition.comparison)
                {
                    case "equals":
                    case "=":
                    case "==":
                        return v.Equals(compareTo);

                    case "greater than":
                    case ">":
                        return v > compareTo;

                    case "less than":
                    case "<":
                        return v < compareTo;

                    case "greater than or equals":
                    case ">=":
                        return v >= compareTo;

                    case "less than or equals":
                    case "<=":
                        return v <= compareTo;

                    case "not equals":
                    case "<>":
                    case "!=":
                        return !v.Equals(compareTo);

                    default:
                        return false;
                        //throw new InvalidOperationException($"Unsupported comparison: {condition.comparison}");
                }
            }
        }

        // Return false if the condition is invalid or unsupported
        return false;
    }

    public virtual bool Condition()
    {
        if (eventInformation.condition == null)
            return true;

        // Recursively evaluate the condition (single, and, or)
        return EvaluateCondition(eventInformation.condition);
    }

}

// ----------------------------------------------------------------------------------------------
[RequireComponent(typeof(basicAGE))]
public class CabinetAGEBasic : MonoBehaviour
{

    public basicAGE AGEBasic;

    BasicVars vars = new(); //variable's space.

    public string pathBase;

    public CabinetAGEBasicInformation AGEInfo = new();

    public void SetDebugMode(bool debug)
    {
        AGEBasic.DebugMode = debug;
    }
    public void Init(CabinetAGEBasicInformation AGEInfo,
            string pathBase,
            Cabinet cabinet,
            CoinSlotController coinSlot, 
            LightGunTarget lightGunTarget)
    {

        if (AGEBasic == null)
            AGEBasic = GetComponent<basicAGE>();

        this.AGEInfo = AGEInfo;
        this.pathBase = pathBase;

        AGEBasic.InitComponents();
        AGEBasic.SetCoinSlot(coinSlot);
        AGEBasic.SetCabinet(cabinet);
        AGEBasic.SetLightGunTarget(lightGunTarget);

        ResetState(true);
    }

    public void ResetState(bool reinitializeYamlVariables = true)
    {
        Stop(); // Ensure no running programs are active and event loop is safely stopped via Shutdown()

        if (reinitializeYamlVariables)
        {
            // Re-apply initial values for variables defined in the YAML configuration.
            if (AGEInfo.Variables != null)
            {
                IngestVariables(AGEInfo.Variables);
            }
        }
    }

    private void LoadCheatXml()
    {
        AGEBasic.ConfigCommands.CheatAddresses = null;
        try
        {
            string[] xmlFiles = Directory.GetFiles(pathBase, "*.xml");
            if (xmlFiles.Length == 0)
            {
                ConfigManager.WriteConsole($"[CabinetAGEBasic.LoadCheatXml] No XML file found in {pathBase}");
                return;
            }

            AGEBasic.ConfigCommands.CheatAddresses = MameCheatXmlParser.Parse(xmlFiles[0]);
            ConfigManager.WriteConsole($"[CabinetAGEBasic.LoadCheatXml] Loaded {AGEBasic.ConfigCommands.CheatAddresses.Count} cheat addresses from {Path.GetFileName(xmlFiles[0])}");
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsole($"[CabinetAGEBasic.LoadCheatXml] Failed to parse cheat XML: {e.Message}");
        }
    }

    public void RegisterYamlEvents()
    {
        //Load cabinet events from YAML
        int countEvents = 0;
        if (AGEInfo.events != null)
        {
            foreach (EventInformation info in AGEInfo.events)
            {
                Event evt = EventsFactory.Factory(info, vars, AGEBasic);
                if (evt != null)
                {
                    AGEBasic.events.Add(evt);
                    AGEBasic.ConfigCommands.events = AGEBasic.events;
                    // some events needs more than one initialization.
                    evt.Init();
                    countEvents++;
                }
            }
        }
        ConfigManager.WriteConsole($"[CabinetAGeBasic.RegisterYamlEvents] Program {pathBase} registered {countEvents} YAML events correctly.");
    }

    private void IngestVariables(List<AGEBasicVariable> variables)
    {
        AGEBasic.IngestVariables(variables, vars);
        ConfigManager.WriteConsole($"[CabinetAGEBasic.Init] injected {variables.Count} variables");
    }

    private bool execute(string prgName, int maxExecutionLines = -1)
    {
        if (string.IsNullOrEmpty(prgName))
            return false;

        if (CompileWhenNeeded(prgName))
        {
            ConfigManager.WriteConsole($"[CabinetAGEBasic.execute] exec {prgName}");
            AGEBasic.Run(prgName, vars, maxExecutionLines); //async blocking=false
            return true;
        }
        return false;
    }

    private bool CompileWhenNeeded(string prgName)
    {
        if (!AGEBasic.Exists(prgName) /*&& afterInsertCoinException == null*/)
        {
            try
            {
                AGEBasic.ParseFile(Path.Combine(pathBase, prgName));
            }
            catch (CompilationException e)
            {
                ConfigManager.WriteConsoleException($"[CabinetAGEBasic.execute] parsing {prgName}", (Exception)e);
                return false;
            }
        }
        return true;
    }

    public bool HasEvents()
    {
        return AGEBasic.events.Count != 0;
    }

    public void ActivateShader(ShaderScreenBase shader)
    {
        AGEBasic.ScreenGenerator.Init(AGEInfo.system_skin).ActivateShader(shader);
    }

    /// <summary>
    /// Called by AGEBasicScreenController after Init() to wire the video player and game
    /// shader into ConfigurationCommands so that VIDEOLOAD/VIDEOPLAY/etc. can use them.
    /// Must not be called on AGEBasicCabinetController cabinets (no video player there).
    /// </summary>
    public void SetVideoConfig(GameVideoPlayer videoPlayer, ShaderScreenBase gameShader)
    {
        AGEBasic.ConfigCommands.VideoPlayer = videoPlayer;
        AGEBasic.ConfigCommands.GameShader = gameShader;
    }

    public void ExecInsertCoinBas()
    {
        ResetState(true);
        LoadCheatXml();
        RegisterYamlEvents();
        AGEBasic.DebugMode = AGEInfo.debug;

        if (!execute(AGEInfo.afterInsertCoin, maxExecutionLines: 0) && AGEBasic.events.Count > 0)
            AGEBasic.StartEventLoop();
    }

    // stop programs and events.
    public void Stop()
    {
        AGEBasic.Shutdown();
    }


    public void StopInsertCoinBas()
    {
        if (AGEBasic.IsRunning(AGEInfo.afterInsertCoin))
            AGEBasic.Shutdown();

        return;
    }

    public void ExecAfterLeaveBas()
    {
        ResetState(false);
        AGEBasic.DebugMode = AGEInfo.debug;
        execute(AGEInfo.afterLeave);

    }
    public void ExecAfterLoadBas()
    {
        AGEBasic.DebugMode = AGEInfo.debug;
        execute(AGEInfo.afterLoad);
    }
    /*    public bool ExecAfterStartBas()
        {
            if (ageBasic.IsRunning())
                return false;

            ageBasic.DebugMode = AGEInfo.debug;
            return execute(AGEInfo.afterStart);
        }
        public void StopAfterStartBas()
        {
            if (ageBasic.IsRunning(AGEInfo.afterStart))
                ageBasic.Stop();
        }
    */
}

#if UNITY_EDITOR


[CustomEditor(typeof(CabinetAGEBasic))]
public class CabinetAGEBasicEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CabinetAGEBasic myScript = (CabinetAGEBasic)target;

        if (GUILayout.Button("Exec Insert Coin Bas"))
        {
            myScript.ExecInsertCoinBas();
        }

        if (GUILayout.Button("Stop Insert Coin Bas"))
        {
            myScript.StopInsertCoinBas();
        }

        if (GUILayout.Button("Exec After Leave Bas"))
        {
            myScript.ExecAfterLeaveBas();
        }

        if (GUILayout.Button("Exec After Load Bas"))
        {
            myScript.ExecAfterLoadBas();
        }

        /*
        if (GUILayout.Button("Exec After Start Bas"))
        {
            myScript.ExecAfterStartBas();
        }

        if (GUILayout.Button("Stop After Start Bas"))
        {
            myScript.StopAfterStartBas();
        }
        */
    }
}
#endif