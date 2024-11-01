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
    //event identification
    [YamlMember(Alias = "event", ApplyNamingConventions = false)]
    public string eventId;
    static string[] validEvents = { "on-timer", "on-always",  "on-insert-coin", "on-custom", 
                                    "on-collision-start", "on-collision-stay", "on-collision-end", 
                                    "on-touch-start", "on-grab-start", "on-touch-end",
                                    "on-grab-end",  "on-lightgun-start", "on-lightgun-stay", "on-lightgun-exit",
                                    "on-control-active-pressed", "on-control-active-held", "on-control-active-released"};

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
    
    public virtual void PrepareToRun(int lineNumber = 0) 
    {
        AGEBasic.PrepareToRun(eventInformation.program, vars, 
                                maxExecutionLinesAllowed: 0, lineNumber: lineNumber);
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
// ------------------------------- control-active ------------------------------
public class OnControlActiveBase: Event
{
    protected int previousValue, actualValue;
    
    public OnControlActiveBase(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic, 1)
    { }

    protected virtual bool evaluate() { return false; }

    public sealed override void EvaluateTrigger()
    {
        if (AGEBasic.ConfigCommands.ControlMap == null)
            RegisterTrigger(false);

        bool ontime = base.IsTime();
        if (ontime)
        {
            previousValue = actualValue;
            actualValue = AGEBasic.ConfigCommands.ControlMap.Active(eventInformation.control.mameControl,
                                                                    eventInformation.control.port);
            RegisterTrigger(evaluate());
        }
    }
}

public class OnControlActivePressed : OnControlActiveBase
{
    public OnControlActivePressed(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic)
    { }

    protected override bool evaluate() { return actualValue != 0 && previousValue == 0; }

}

public class OnControlActiveReleased : OnControlActiveBase
{
    public OnControlActiveReleased(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic)
    { }

    protected override bool evaluate() { return actualValue == 0 && previousValue != 0; }

}

public class OnControlActiveHeld : OnControlActiveBase
{
    public OnControlActiveHeld(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic)
    { }
    protected override bool evaluate() { return actualValue != 0 && previousValue != 0; }
}

// ------------------------ coin ------------------------------
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
        //custon triggers could be forced before the
        //event coroutine runs and aren't initialized yet.
        Init();
        RegisterTrigger(true);
    }
    public override void EvaluateTrigger() {}
}

// -------------------- lightguns ------------------------
public class OnLightGunBase : Event
{
    protected bool previousState = false;
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

public class OnLightGunStart : OnLightGunBase
{
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
            case "on-control-active-pressed":
                return new OnControlActivePressed(eventInformation, vars, agebasic);
            case "on-control-active-held":
                return new OnControlActiveHeld(eventInformation, vars, agebasic);
            case "on-control-active-released":
                return new OnControlActiveReleased(eventInformation, vars, agebasic);
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

    private Coroutine eventCoroutine;
    private bool eventCoroutineIsRunning;

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
        AGEBasic.SetCabinetEvents(events);
        AGEBasic.SetLightGunTarget(lightGunTarget);

        if (AGEInfo.Variables != null)
        {
            IngestVariables(AGEInfo.Variables);
        }

        
        //events ---
        foreach (EventInformation info in AGEInfo.events)
        {
            Event evt = EventsFactory.Factory(info, vars, AGEBasic);
            if (evt != null)
            {
                events.Add(evt);
                // some events needs more than one initialization.
                evt.Init();
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

        if (CompileWhenNeeded(prgName))
        {
            ConfigManager.WriteConsole($"[CabinetAGEBasic.execute] exec {prgName}");
            AGEBasic.Run(prgName, blocking, vars, maxExecutionLines); //async blocking=false
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
                AGEBasic.ParseFile(pathBase + "/" + prgName);
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
        return events.Count != 0;
    }
    private IEnumerator RunEvents()
    {
        if (events.Count == 0)
            yield break;
        eventCoroutineIsRunning = true;
        while (true) // Continuous loop to keep checking for triggered events
        {
            if (AGEBasic.IsRunning())
            {
                yield return null;
                continue;
            }

            foreach (Event evt in events)
            {
                // Initialize the event if needed
                // some events needs more than one initialization.
                if (!evt.Initialized)
                    evt.Init();
#if EVENT_LOOP_DEBUG
                ConfigManager.WriteConsole($"[CabinetAGEBasic.RunEvents] evaluate {evt.eventInformation.eventId}");
#endif
                //check and cache the trigger action
                try
                {
                    if (evt.Condition())
                        evt.EvaluateTrigger();
                }
                catch (Exception e)
                {
                    ConfigManager.WriteConsoleException($"[CabinetAGEBasic.RunEvents] evaluate exception event:{evt.eventInformation.name} / {evt.eventInformation.eventId}", e);
                }
            }

            foreach (Event evt in events)
            {
                // Check if the event was triggered
                if (evt.WasTriggered())
                {
                    ConfigManager.WriteConsole($"[CabinetAGEBasic.RunEvents] starting event:{evt.eventInformation.name} prg:{evt.eventInformation.program} goto:{evt.eventInformation.line} ");

                    //prepare
                    CompileWhenNeeded(evt.eventInformation.program);

                    if (evt.eventInformation.Variables != null)
                        IngestVariables(evt.eventInformation.Variables);

                    evt.PrepareToRun(lineNumber: evt.eventInformation.line);

                    //run
                    bool moreLines = true;
                    while (moreLines)
                    {
                        YieldInstruction yield;
                        // Run the event's program
                        // excptions are catched internally
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

        startEventCoroutine();
    }

    // stop programs and events.
    public void Stop()
    {
        if (eventCoroutine != null)
            stopEventCoroutine();

        AGEBasic.ForceStop();

        foreach (Event evt in events)
        {
            evt.Dispose();
        }
    }

    private void stopEventCoroutine()
    {
        eventCoroutineIsRunning = false;
        StopCoroutine(eventCoroutine);
    }
    private void startEventCoroutine()
    {
        //  game started, time to start the events corroutine.
        if (events.Count > 0)
            eventCoroutine = StartCoroutine(RunEvents());
    }
    public bool IsCoroutineRunning()
    {
        return eventCoroutineIsRunning;
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