
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Serialization; //https://github.com/aaubry/YamlDotNet
using UnityEditor;
using System.Linq;
using YamlDotNet.Core;
using static CabinetInformation;


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
    /*
    [Serializable]
    public class PartImpact
    {
        [Serializable]
        public class Or
        {
            [YamlMember(Alias = "impact-part", ApplyNamingConventions = false)]
            public PartImpact partImpact;
        }
        [Serializable]
        public class And
        {
            [YamlMember(Alias = "impact-part", ApplyNamingConventions = false)]
            public PartImpact partImpact;
        }
        //public And and;
        public Or or;
        public string part;
    }
    */
    //event identification
    [YamlMember(Alias = "event", ApplyNamingConventions = false)]
    public string eventId;
    static string[] validEvents = { "on-timer", "on-always", "on-control-active", "on-insert-coin", "on-custom", 
                                    "on-collision-start", "on-collision-stay", "on-collision-end", "on-touch-start", "on-grab-start", "on-touch-end", "on-grab-end",  "on-lightgun-start", "on-lightgun-stay", "on-lightgun-exit"};

    static string[] requirePartName = { "on-collision-start", "on-collision-stay", "on-collision-end", 
                                        "on-touch-start", "on-grab-start", 
                                        "on-touch-end", "on-grab-end" ,
                                        "on-lightgun-start", "on-lightgun-stay", "on-lightgun-exit"};
    public string name = "";
    public string program;
    public double delay = 0;
    public ControlInformation control;
    public string part;
    [YamlMember(Alias = "impact-parts", ApplyNamingConventions = false)]
    public List<string> partImpacts; //name of the colliding part.
    //public List<string> parts; //OR parts

    // Serialize this field to show it in the editor
    [SerializeField]
    private List<AGEBasicVariable> variables;

    [YamlMember(Alias = "variables", ApplyNamingConventions = false)]
    public List<AGEBasicVariable> Variables { get { return variables; } set { variables = value; } }

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
    public string state = "pressed";
    public static string[] validStates = { "pressed", "held", "released" };
    public CabinetValidationException IsValid(string cabName)
    {
        if (string.IsNullOrEmpty(mameControl))
            return new CabinetValidationException(cabName, $"Event cabinet {cabName} control libretro-id isn't specified");


        if (!validStates.Contains(state))
            return new CabinetValidationException(cabName, $"Event cabinet {cabName} control {mameControl} state {state} is invalid, only {string.Join(',', validStates)} are allowed");

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
    //across all the object of the same class.
    protected static Dictionary<string, int> previousValues = new Dictionary<string, int>();
    string key;
    public OnControlActive(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic, 1)
    { }

    private void RegisterTriggerStatus(int status)
    {
        RegisterTrigger(true);
        previousValues[key] = status;
    }

    private int getPreviousValue()
    {
        if (!previousValues.ContainsKey(key))
            return 0;
        return previousValues[key];
    }

    public override void Init()
    {
        if (status == Status.initialized)
            return;
        
        key = eventInformation.control.mameControl + "_" + eventInformation.control.port;

        Reset();
        status = Status.initialized;
    }
    public override void EvaluateTrigger()
    {
        if (AGEBasic.ConfigCommands.ControlMap == null)
            RegisterTrigger(false);

        bool ontime = base.IsTime();
        if (ontime)
        {
            int status = AGEBasic.ConfigCommands.ControlMap.Active(eventInformation.control.mameControl, eventInformation.control.port);
            switch (eventInformation.control.state)
            {
                case "pressed":
                    if (status == 1 && getPreviousValue() == 0)
                        RegisterTriggerStatus(status);
                    else
                        RegisterTrigger(false);
                    break;
                case "released":
                    if (status == 0 && getPreviousValue() == 1)
                        RegisterTriggerStatus(status);
                    else
                        RegisterTrigger(false);
                    break;
                case "held":
                    if (status == 1 && getPreviousValue() == 1)
                        RegisterTriggerStatus(status);
                    else
                        RegisterTrigger(false);
                    break;
            }
        }
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


public class OnLightGunBase : Event
{
    bool previousState = false;
    public OnLightGunBase(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic)
    { }

    protected bool actualState()
    {
        if (AGEBasic.ConfigCommands.lightGunTarget != null)
        {
            GameObject go = AGEBasic.ConfigCommands.lightGunTarget.GetLastGameObjectHit();
            if (go != null)
            {
                return go.name == eventInformation.part;
            }
        }
        return false;
    }

}

// -------------------- lightguns ------------------------

public class OnLightGunStart : OnLightGunBase
{
    bool previousState = false;
    public OnLightGunStart(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic)
    { }

    public override void EvaluateTrigger()
    {
        bool state = actualState();
        if (state && !previousState)
            RegisterTrigger(true);
        previousState = state;
    }
}

public class OnLightGunStay : OnLightGunBase
{
    bool previousState = false;
    public OnLightGunStay(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic)
    { }

    public override void EvaluateTrigger()
    {
        bool state = actualState();
        if (state && previousState)
            RegisterTrigger(true);
        previousState = state;

    }
}

public class OnLightGunExit : OnLightGunBase
{
    bool previousState = false;
    public OnLightGunExit(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic)
    { }

    public override void EvaluateTrigger()
    {
        bool state = actualState();
        if (!state && previousState)
            RegisterTrigger(true);
        previousState = state;
    }
}

// -------------------- collisions ------------------------
public class OnCollisionBase: Event
{
    protected GameObject partColliding;
    protected InteractablePart interactablePart;

    public OnCollisionBase(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic)
    { }

    protected bool loadComponents()
    {
        partColliding = AGEBasic.ConfigCommands.Cabinet.Parts(eventInformation.part);
        if (partColliding == null)
            throw new Exception($"AGEBasic event on-collision-start part collider {eventInformation.part} not found");

        interactablePart = partColliding.GetComponent<InteractablePart>();
        
        //detected: some times onTriggerEnterEvent is null. 

        return !(interactablePart == null || !interactablePart.Initialized || interactablePart.collisionDetection == null );
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

        if (!loadComponents() || interactablePart.collisionDetection.OnCollisionStart == null)
        {
            status = Status.error;
            return;
        }
        
        interactablePart.collisionDetection.OnCollisionStart.AddListener(OnCollisionTriggerStart);
        status = Status.initialized;
    }

    void OnCollisionTriggerStart(string collidingPartName)
    {
        if (eventInformation.partImpacts == null ||
            eventInformation.partImpacts.Count == 0 ||
            eventInformation.partImpacts.Contains(collidingPartName))
            RegisterTrigger(true);
    }

    public override void Dispose()
    {
        interactablePart.collisionDetection.OnCollisionStart?.RemoveListener(OnCollisionTriggerStart);
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

        if (!loadComponents() || interactablePart.collisionDetection.OnCollisionContinue == null)
        {
            status = Status.error;
            return;
        }
        interactablePart.collisionDetection.OnCollisionContinue.AddListener(OnCollisionTrigger);
        status = Status.initialized;
    }

    void OnCollisionTrigger(string collidingPartName)
    {
        if (eventInformation.partImpacts == null ||
            eventInformation.partImpacts.Count == 0 ||
            eventInformation.partImpacts.Contains(collidingPartName))
            RegisterTrigger(true);
    }

    public override void Dispose()
    {
        interactablePart.collisionDetection.OnCollisionStart?.RemoveListener(OnCollisionTrigger);
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

        if (!loadComponents() || interactablePart.collisionDetection.OnCollisionEnd == null)
        {
            status = Status.error;
            return;
        }
        
        interactablePart.collisionDetection.OnCollisionEnd.AddListener(OnCollisionTriggerExit);
        status = Status.initialized;
    }

    void OnCollisionTriggerExit(string collidingPartName)
    {
        if (eventInformation.partImpacts == null || 
            eventInformation.partImpacts.Count == 0 || 
            eventInformation.partImpacts.Contains(collidingPartName))
            RegisterTrigger(true);
    }

    public override void Dispose()
    {
        interactablePart.collisionDetection.OnCollisionEnd?.RemoveListener(OnCollisionTriggerExit);
        base.Dispose();
    }
}

public class OnPlayerBaseEvent : Event
{
    protected InteractablePart interactablePart;
    protected GameObject part;

    public OnPlayerBaseEvent(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic)
    { }

    protected bool loadComponents()
    {
        if (part == null)
        {
            part = AGEBasic.ConfigCommands.Cabinet.Parts(eventInformation.part);
            if (part == null)
            {
                status = Status.error;
                throw new Exception($"AGEBasic event on-touch part {eventInformation.part} not found");
            }
        }

        interactablePart = part.GetComponent<InteractablePart>();

        //detected: some times onTriggerEnterEvent is null. 

        return !(interactablePart == null || !interactablePart.Initialized || interactablePart.grabDetection == null);
    }
    public override void Init()
    {
        if (status == Status.initialized)
            return;

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
        if (!loadComponents() || interactablePart.grabDetection.OnPlayerTouchEnter == null)
        {
            status = Status.error;
            return;
        }

        interactablePart.grabDetection.OnPlayerTouchEnter.AddListener(OnTouch);
        status = Status.initialized;
    }

    void OnTouch()
    {
        RegisterTrigger(true);
    }

    public override void Dispose()
    {
        interactablePart.grabDetection.OnPlayerTouchEnter?.RemoveListener(OnTouch);
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

        if (!loadComponents() || interactablePart.grabDetection.OnPlayerTouchExit == null)
        {
            status = Status.error;
            return;
        }
        interactablePart.grabDetection.OnPlayerTouchExit.AddListener(OnTouch);
        status = Status.initialized;
    }

    void OnTouch()
    {
        RegisterTrigger(true);
    }

    public override void Dispose()
    {
        interactablePart.grabDetection.OnPlayerTouchExit?.RemoveListener(OnTouch);
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
            case "on-lightgun-start":
                return new OnLightGunStart(eventInformation, vars, agebasic);
            case "on-lightgun-stay":
                return new OnLightGunStay(eventInformation, vars, agebasic);
            case "on-lightgun-exit":
                return new OnLightGunExit(eventInformation, vars, agebasic);
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
        foreach (EventInformation info in AGEInfo.events)
        {
            Event ev = EventsFactory.Factory(info, vars, AGEBasic);
            if (ev != null)
                events.Add(ev);
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