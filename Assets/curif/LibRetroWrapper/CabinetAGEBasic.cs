
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Serialization; //https://github.com/aaubry/YamlDotNet
using YamlDotNet.Serialization.NamingConventions;
using UnityEditor;
using Unity.VisualScripting;
using Palmmedia.ReportGenerator.Core;
using YamlDotNet.Core;
using static OVRHaptics;
using UnityEditor.Experimental.GraphView;
using Newtonsoft.Json.Converters;

[Serializable]
public class CabinetAGEBasicInformation
{
    public bool active = true;
    public bool debug = false;

    [YamlMember(Alias = "system-skin", ApplyNamingConventions = false)]
    public string system_skin = "c64";

    // Serialize this field to show it in the editor
    [SerializeField]
    private List<Variable> variables;
    public List<Variable> Variables { get { return variables; } set { variables = value; } }

    [Serializable]
    public class Variable
    {
        public string name; //variable name
        public string type; //STRING or NUMBER
        public string value = ""; //first asigned value.
    }

    [YamlMember(Alias = "after-start", ApplyNamingConventions = false)]
    public string afterStart;
    [YamlMember(Alias = "after-load", ApplyNamingConventions = false)]
    public string afterLoad;
    [YamlMember(Alias = "after-insert-coin", ApplyNamingConventions = false)]
    public string afterInsertCoin;
    [YamlMember(Alias = "after-leave", ApplyNamingConventions = false)]
    public string afterLeave;

    public List<EventInformation> events;

    public void Validate()
    {
        foreach (EventInformation e in events)
        {
            e.Validate();
        }
    }
    
}

[Serializable]
public class EventInformation
{
    //event identification
    [YamlMember(Alias = "event", ApplyNamingConventions = false)]
    public string eventId;
    static string[] validEvents = { "on-timer", "on-always", "on-control-active" };

    public string program;
    public double delay = 0;
    public List<ControlInformation> controls;

    public void Validate()
    {
        if (string.IsNullOrEmpty(eventId))
            throw new Exception($"AGEBasic Event Id unespecified");

        if (Array.IndexOf(validEvents, eventId) < 0)
            throw new Exception($"AGEBasic Event [{eventId}] unknown");

        if (string.IsNullOrEmpty(program))
            throw new Exception($"AGEBasic Event {eventId} doesn't have a program attached");

        if (controls != null)
        {
            foreach (var control in controls)
            {
                if (string.IsNullOrEmpty(control.mameControl))
                    throw new Exception($"Event {eventId} one of the control isn't specified");
            }
        }
    }
}
[Serializable]
public class ControlInformation
{
    [YamlMember(Alias = "libretro-id", ApplyNamingConventions = false)]
    public string mameControl;
    public int port = 0;
}

/// Event execution
public class Event
{
    public enum Type
    {
        Timer,
        Always,
        ControlActive
    }
    public EventInformation eventInformation;
    
    //AGEBasic
    public BasicVars vars;
    public basicAGE AGEBasic;

    protected DateTime startTime;

    private bool triggered;

    public Event(EventInformation eventInformation, BasicVars vars, basicAGE agebasic)
    {
        this.eventInformation = eventInformation;
        this.vars = vars;
        AGEBasic = agebasic;
    }

    protected bool RegisterTrigger(bool isTriggered)
    {
        triggered = isTriggered;
        return triggered;
    }
    protected bool WasTriggered()
    {
        return triggered;
    }
    public virtual void Init() {
        triggered = false;
        startTime = DateTime.Now;
    }
    public virtual void PrepareToRun() 
    {
        AGEBasic.PrepareToRun(eventInformation.program, vars, 0);
    }

    //run next line
    public virtual YieldInstruction Run(ref bool moreLines)
    {
        YieldInstruction yield;
        yield = AGEBasic.runNextLineCurrentProgram(ref moreLines);
        if (!moreLines)
        {
            triggered = false;
            startTime = DateTime.Now;
        }
        return yield;
    }

    protected virtual bool IsTime()
    {
        return (DateTime.Now - startTime).TotalSeconds >= eventInformation.delay;
    }
    public virtual bool Triggered() { 
        if (triggered)
        {
            //was but not executed.
            return true;
        }
        if (eventInformation.delay > 0)
        {
            return RegisterTrigger(IsTime());
        }
        return RegisterTrigger(false); 
    }
}

public class OnTimer : Event
{
    public OnTimer(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic)
    { }
}

public class OnAlways : Event
{
    public OnAlways(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic)
    { }

    public override bool Triggered()
    {
        return RegisterTrigger(true);
    }
}

public class OnControlActive: Event
{
    public OnControlActive(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic)
    { }

    public override bool Triggered()
    {
        if (AGEBasic.ConfigCommands.ControlMap == null)
            return RegisterTrigger(false);

        if (WasTriggered())
            return true;

        bool ontime = base.IsTime();
        if (ontime)
        {
            foreach (var control in eventInformation.controls)
                if (AGEBasic.ConfigCommands.ControlMap.Active(control.mameControl, control.port) == 0)
                    return RegisterTrigger(false);
            return RegisterTrigger(true);
        }
        return RegisterTrigger(false);
    }
}

public static class EventsFactory
{
    public static Event Factory(EventInformation eventInformation, BasicVars vars, basicAGE agebasic)
    {
        switch (eventInformation.eventId)
        {
            case "on-always":
                return new OnAlways(eventInformation, vars, agebasic);
            case "on-timer":
                return new OnTimer(eventInformation, vars, agebasic);
            case "on-control-active":
                return new OnControlActive(eventInformation, vars, agebasic);
        }

        throw new Exception($"AGEBasic Unknown event: {eventInformation.eventId}");
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

    private List<Event> events = new List<Event>();

    private Coroutine coroutine;

    public void SetDebugMode(bool debug)
    {
        AGEBasic.DebugMode = debug;
    }
    public void Init(CabinetAGEBasicInformation AGEInfo,
            string pathBase,
            Cabinet cabinet,
            CoinSlotController coinSlot)
    {
        if (AGEBasic == null)
            AGEBasic = GetComponent<basicAGE>();

        this.AGEInfo = AGEInfo;
        this.pathBase = pathBase;

        AGEBasic.SetCoinSlot(coinSlot);
        AGEBasic.SetCabinet(cabinet);

        if (AGEInfo.Variables != null)
        {       
            //variable injection
            foreach (CabinetAGEBasicInformation.Variable var in AGEInfo.Variables)
            {
                BasicValue bv;
                if (var.type.ToUpper() == "STRING")
                    bv = new BasicValue(var.value, forceType: BasicValue.BasicValueType.String);
                else if (var.type.ToUpper() == "NUMBER")
                    bv = new BasicValue(var.value, forceType: BasicValue.BasicValueType.Number);
                else
                    throw new Exception($"[CabinetAGEBasic.init] AGEBasic variable injection error var: {var.name} value type unknown: {var.type}");

                vars.SetValue(var.name, bv);
                ConfigManager.WriteConsole($"[CabinetAGEBasic.Init] inject variable: {var.name}: {bv}");
            }
        }

        //events ---
        if (AGEInfo.events.Count > 0)
        {

            foreach (EventInformation info in AGEInfo.events)
            {
                Event ev = EventsFactory.Factory(info, vars, AGEBasic);
                if (ev != null)
                    events.Add(ev);
            }

        }
    }

    private bool execute(string prgName, bool blocking = false, int maxExecutionLines = 10000)
    {
        if (string.IsNullOrEmpty(prgName))
            return false;
        
        CompileWhenNeeded(prgName);

        ConfigManager.WriteConsole($"[CabinetAGEBasic.execute] exec {prgName}");
        AGEBasic.Run(prgName, blocking, vars, maxExecutionLines); //async blocking=false
        return true;
    }

    private void CompileWhenNeeded(string prgName)
    {
        if (!AGEBasic.Exists(prgName) /*&& afterInsertCoinException == null*/)
        {
            try
            {
                AGEBasic.ParseFile(pathBase + "/" + prgName);
            }
            catch (CompilationException e)
            {
                ConfigManager.WriteConsoleException($"[CabinetAGEBasic.execute] parsing {prgName}", (Exception)e);
                throw e;
            }
        }
    }
    public IEnumerator RunEvents()
    {
        if (events.Count == 0)
            yield break;

        foreach (Event evt in events)
        {
            // Initialize the event if needed
            evt.Init();
        }

        while (true) // Continuous loop to keep checking for triggered events
        {
            
            foreach (Event evt in events)
            {

                // Check if the event is/was triggered
                if (evt.Triggered())
                {
                    ConfigManager.WriteConsole($"[CabinetAGEBasic.RunEvents] starting {evt.eventInformation.program}");
                    if (!AGEBasic.IsRunning())
                    {
                        //no other program is running

                        //prepare
                        CompileWhenNeeded(evt.eventInformation.program);
                        evt.PrepareToRun();

                        //run
                        bool moreLines = true;
                        while (moreLines)
                        {
                            YieldInstruction yield;
                            // Run the event's program
                            yield = evt.Run(ref moreLines);
                            yield return yield;
                        }
                    }
                }
            }
            yield return null;
        }
    }

    public void ActivateShader(ShaderScreenBase shader)
    {
        AGEBasic.ScreenGenerator.Init(AGEInfo.system_skin).
                                    ActivateShader(shader);
    }

    public void ExecInsertCoinBas()
    {
        AGEBasic.DebugMode = AGEInfo.debug;
        execute(AGEInfo.afterInsertCoin, maxExecutionLines: 0);

        //  game started, time to start the events corroutine.
        if (events.Count > 0)
            coroutine = StartCoroutine(RunEvents());
    }

    public void StopInsertCoinBas()
    {
        if (AGEBasic.IsRunning(AGEInfo.afterInsertCoin))
            AGEBasic.ForceStop();

        return;
    }

    public void ExecAfterLeaveBas()
    {
        AGEBasic.DebugMode = AGEInfo.debug;
        execute(AGEInfo.afterLeave);
        if (coroutine != null)
            StopCoroutine(coroutine);
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