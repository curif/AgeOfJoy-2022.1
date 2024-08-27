
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Serialization; //https://github.com/aaubry/YamlDotNet
using UnityEditor;


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
    public bool active = true;
    public bool debug = false;

    [YamlMember(Alias = "system-skin", ApplyNamingConventions = false)]
    public string system_skin = "c64";

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

    public List<EventInformation> events;

    public void Validate(string cabName)
    {
        if (events == null || events.Count == 0)
        {
            return;
        }
        foreach (EventInformation e in events)
        {
            e.Validate(cabName);
        }
    }
    
}

[Serializable]
public class EventInformation
{
    //event identification
    [YamlMember(Alias = "event", ApplyNamingConventions = false)]
    public string eventId;
    static string[] validEvents = { "on-timer", "on-always", "on-control-active", "on-insert-coin", "on-custom", "on-lightgun", 
                                    "on-collision-start", "on-collision-stay", "on-collision-end", "on-touch-start", "on-grab-start",   "on-touch-end", "on-grab-end"};

    static string[] requirePartName = { "on-collision-start", "on-collision-stay", "on-collision-end", 
                                        "on-touch-start", "on-grab-start", 
                                        "on-touch-end", "on-grab-end" };
    public string name = "";
    public string program;
    public double delay = 0;
    public List<ControlInformation> controls;
    public string part;
    // Serialize this field to show it in the editor
    [SerializeField]
    private List<AGEBasicVariable> variables;

    [YamlMember(Alias = "variables", ApplyNamingConventions = false)]
    public List<AGEBasicVariable> Variables { get { return variables; } set { variables = value; } }

    public void Validate(string cabName)
    {
        if (string.IsNullOrEmpty(eventId))
            throw new CabinetValidationException(cabName, $"AGEBasic Event Id unespecified");

        if (Array.IndexOf(validEvents, eventId) < 0)
            throw new CabinetValidationException(cabName, $"AGEBasic Event [{eventId}] unknown");

        if (Array.IndexOf(requirePartName, eventId) >= 0 && string.IsNullOrEmpty(part))
            throw new CabinetValidationException(cabName, $"AGEBasic Event {eventId} requires a part name");

        if (string.IsNullOrEmpty(program))
            throw new CabinetValidationException(cabName, $"AGEBasic Event {eventId} doesn't have a program attached");

        if (controls != null)
        {
            foreach (var control in controls)
            {
                if (string.IsNullOrEmpty(control.mameControl))
                    throw new CabinetValidationException(cabName, $"Event {eventId} one of the control isn't specified");
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
            triggeredCount --;
            startTime = DateTime.Now;
        }
        return yield;
    }

    protected virtual bool IsTime()
    {
        return (DateTime.Now - startTime).TotalSeconds >= eventInformation.delay;
    }

    public virtual void EvaluateTrigger() { 
        if (eventInformation.delay > 0)
        {
            RegisterTrigger(IsTime());
        }
        RegisterTrigger(false); 
    }

    public virtual void Dispose()
    {
        status = Status.needsinitilization;
        return;
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
        base(eventInformation, vars, agebasic, 1)
    { }

    public override void EvaluateTrigger()
    {
        RegisterTrigger(true);
    }
}

public class OnControlActive: Event
{
    public OnControlActive(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic, 1)
    { }

    public override void EvaluateTrigger()
    {
        if (AGEBasic.ConfigCommands.ControlMap == null)
            RegisterTrigger(false);

        bool ontime = base.IsTime();
        if (ontime)
        {
            foreach (var control in eventInformation.controls)
                if (AGEBasic.ConfigCommands.ControlMap.Active(control.mameControl, control.port) == 0)
                { 
                    RegisterTrigger(false);
                    return;
                }
            RegisterTrigger(true);
        }
         RegisterTrigger(false);
    }
}

public class OnInsertCoin : Event
{
    public OnInsertCoin(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic)
    { }

    void OnInsertCoinTrigger()
    {
        RegisterTrigger(true);
    }
    public override void EvaluateTrigger()
    {
    }
    public override void Init()
    {
        if (status == Status.needsinitilization)
        {
            base.Reset();
            
            if (AGEBasic.ConfigCommands.CoinSlot?.OnInsertCoin == null)
                return;

            AGEBasic.ConfigCommands.CoinSlot.OnInsertCoin.AddListener(OnInsertCoinTrigger);
            status = Status.initialized;
        }
    }
}

public class OnCustom : Event
{
    public OnCustom(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic)
    { }

    public void ForceTrigger()
    {
        RegisterTrigger(true);
    }
    public override void EvaluateTrigger() {}

}


public class OnLightGun : Event
{
    public OnLightGun(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic)
    { }

    public override void EvaluateTrigger()
    {
        if (AGEBasic.ConfigCommands.lightGunTarget != null)
        {
            RegisterTrigger(AGEBasic.ConfigCommands.lightGunTarget.GetLastGameObjectHit() != null);
        }
    }
}


public class OnCollisionBase: Event
{
    protected GameObject partColliding;
    protected CollisionDetection touchDetection;

    public OnCollisionBase(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic)
    { }

    protected void loadComponents()
    {
        partColliding = AGEBasic.ConfigCommands.Cabinet.Parts(eventInformation.part);
        if (partColliding == null)
        {
            status = Status.initialized;
            throw new Exception($"AGEBasic event on-collision-start part collider {eventInformation.part} not found");
        }

        touchDetection = partColliding.GetComponent<CollisionDetection>();
        if (touchDetection == null)
        {
            status = Status.initialized;

            throw new Exception($"AGEBasic event on-collision-start part collider {eventInformation.part} wasn't declared collision parts in cabinet configuration (yaml)");
        }

        if (touchDetection.impact.parts.Count == 0)
        {
            status = Status.initialized;
            throw new Exception($"AGEBasic event on-collision-start part collider {eventInformation.part} needs declared collision parts list in cabinet configuration (yaml)");
        }
    }

    public override void EvaluateTrigger()
    {
    }

}

public class OnCollisionStart : OnCollisionBase
{
    public OnCollisionStart(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic)
    { }

    public override void Init()
    {
        if (status == Status.initialized)
            return;

        base.Reset();

        loadComponents();
        
        //detected: some times onTriggerEnterEvent is null. needs reinitialization
        if (touchDetection.OnCollisionStart == null)
        {
            status = Status.error;
            return;
        }
        touchDetection.OnCollisionStart.AddListener(OnCollisionTriggerStart);
        status = Status.initialized;
    }

    void OnCollisionTriggerStart(string part)
    {
        RegisterTrigger(true);
    }

    public override void Dispose()
    {
        touchDetection.OnCollisionStart?.RemoveListener(OnCollisionTriggerStart);
        base.Dispose();
    }
}

public class OnCollisionStay : OnCollisionBase
{
    public OnCollisionStay(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic)
    { }

    public override void Init()
    {
        if (status == Status.initialized)
            return;

        base.Reset();

        loadComponents();

        //detected: some times onTriggerEnterEvent is null. needs reinitialization
        if (touchDetection.OnCollisionStart == null)
        {
            status = Status.error;
            return;
        }
        touchDetection.onCollisionContinue.AddListener(OnCollisionTrigger);
        status = Status.initialized;
    }

    void OnCollisionTrigger(string part)
    {
        RegisterTrigger(true);
    }

    public override void Dispose()
    {
        touchDetection.OnCollisionStart?.RemoveListener(OnCollisionTrigger);
        base.Dispose();
    }
}
public class OnCollisionEnd : OnCollisionBase
{
    public OnCollisionEnd(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic)
    { }

    public override void Init()
    {
        if (status == Status.initialized)
            return;

        base.Reset();

        loadComponents();
        
        //detected: some time onTriggerEnterEvent is null. needs reinitialization
        if (touchDetection.OnCollisionEnd == null)
        {
            status = Status.error;
            return;
        }
        touchDetection.OnCollisionStart.AddListener(OnCollisionTriggerExit);
        status = Status.initialized;
    }

    void OnCollisionTriggerExit(string part)
    {
        RegisterTrigger(true);
    }

    public override void Dispose()
    {
        touchDetection.OnCollisionEnd?.RemoveListener(OnCollisionTriggerExit);
        base.Dispose();
    }
}

public class OnPlayerBaseEvent : Event
{
    protected GrabDetection grabDetection;
    protected GameObject part;

    public OnPlayerBaseEvent(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic)
    { }

    public override void Init()
    {
        if (status == Status.initialized)
            return;

        if (part == null)
        {
            part = AGEBasic.ConfigCommands.Cabinet.Parts(eventInformation.part);
            if (part == null)
            {
                status = Status.initialized;
                throw new Exception($"AGEBasic event on-touch part {eventInformation.part} not found");
            }
        }

        grabDetection = part.GetComponent<GrabDetection>();
        if (grabDetection == null)
        {
            status = Status.initialized;
            throw new Exception($"AGEBasic event on-touch  part {eventInformation.part} bad configuration");
        }

        base.Reset();
    }
}

public class OnPlayerTouchStartEvent: OnPlayerBaseEvent
{
    public OnPlayerTouchStartEvent(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic)
    { }

    public override void Init()
    {
        if (status == Status.initialized)
            return;

        base.Init();

        //detected: some time onTriggerEnterEvent is null. needs reinitialization
        if (grabDetection.OnPlayerTouchEnter == null)
        {
            status = Status.error;
            return;
        }

        grabDetection.OnPlayerTouchEnter.AddListener(OnTouch);
        status = Status.initialized;
    }

    void OnTouch()
    {
        RegisterTrigger(true);
    }

    public override void Dispose()
    {
        grabDetection.OnPlayerTouchEnter?.RemoveListener(OnTouch);
        base.Dispose();
    }
}

public class OnPlayerTouchEndEvent : OnPlayerBaseEvent
{
    public OnPlayerTouchEndEvent(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic)
    { }

    public override void Init()
    {
        if (status == Status.initialized)
            return;

        base.Init();

        //detected: some time onTriggerEnterEvent is null. needs reinitialization
        if (grabDetection.OnPlayerTouchExit == null)
        {
            status = Status.error;
            return;
        }

        grabDetection.OnPlayerTouchExit.AddListener(OnTouch);
        status = Status.initialized;
    }

    void OnTouch()
    {
        RegisterTrigger(true);
    }

    public override void Dispose()
    {
        grabDetection.OnPlayerTouchExit?.RemoveListener(OnTouch);
        base.Dispose();
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
            case "on-insert-coin":
                return new OnInsertCoin(eventInformation, vars, agebasic);
            case "on-custom":
                return new OnCustom(eventInformation, vars, agebasic);
            case "on-lightgun":
                return new OnLightGun(eventInformation, vars, agebasic);
            case "on-collision-start":
                return new OnCollisionStart(eventInformation, vars, agebasic);
            case "on-collision-stay":
                return new OnCollisionStay(eventInformation, vars, agebasic);
            case "on-collision-end":
                return new OnCollisionEnd(eventInformation, vars, agebasic);
            case "on-touch-start":
                return new OnPlayerTouchStartEvent(eventInformation, vars, agebasic);
            case "on-touch-end":
                return new OnPlayerTouchEndEvent(eventInformation, vars, agebasic);
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
            CoinSlotController coinSlot, 
            LightGunTarget lightGunTarget)
    {
        if (AGEBasic == null)
            AGEBasic = GetComponent<basicAGE>();

        this.AGEInfo = AGEInfo;
        this.pathBase = pathBase;

        AGEBasic.SetCoinSlot(coinSlot);
        AGEBasic.SetCabinet(cabinet);
        AGEBasic.SetCabinetEvents(events);
        AGEBasic.SetLightGunTarget(lightGunTarget);

        if (AGEInfo.Variables != null)
        {
            IngestVariables(AGEInfo.Variables);
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

    private void IngestVariables(List<AGEBasicVariable> variables)
    {
        //variable injection
        foreach (AGEBasicVariable var in variables)
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
    private IEnumerator RunEvents()
    {
        if (events.Count == 0)
            yield break;

        while (true) // Continuous loop to keep checking for triggered events
        {

            foreach (Event evt in events)
            {
                // Initialize the event if needed
                // some events needs more than one initialization.
                if (!evt.Initialized)
                    evt.Init();
                //check and cache the trigger action
                evt.EvaluateTrigger();
            }
            
            foreach (Event evt in events)
            {
                // Check if the event was triggered
                if (evt.WasTriggered() && !AGEBasic.IsRunning())
                {
                    ConfigManager.WriteConsole($"[CabinetAGEBasic.RunEvents] starting {evt.eventInformation.program}");

                    //prepare
                    CompileWhenNeeded(evt.eventInformation.program);

                    if (evt.eventInformation.Variables != null)
                        IngestVariables(evt.eventInformation.Variables);
                    
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
            yield return null;
        }
    }

    public void ActivateShader(ShaderScreenBase shader)
    {
        AGEBasic.ScreenGenerator.Init(AGEInfo.system_skin).ActivateShader(shader);
    }

    public void ExecInsertCoinBas()
    {
        AGEBasic.DebugMode = AGEInfo.debug;
        execute(AGEInfo.afterInsertCoin, maxExecutionLines: 0);

        //  game started, time to start the events corroutine.
        if (events.Count > 0)
            coroutine = StartCoroutine(RunEvents());
    }

    // stop programs and events.
    public void Stop()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
        AGEBasic.ForceStop();

        foreach (Event evt in events)
        {
            evt.Dispose();
        }
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