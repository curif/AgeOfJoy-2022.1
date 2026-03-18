
using System;
using UnityEngine;

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
public class OnControlActiveBase : Event
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
    public override void EvaluateTrigger() { }
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
public class OnCollisionBase : Event
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

        return !(interactablePart == null || !interactablePart.Initialized || interactablePart.collisionDetection == null);
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
            if (AGEBasic.ConfigCommands.Cabinet == null)
            {
                status = Status.error;
                return false;
            }
            part = AGEBasic.ConfigCommands.Cabinet.Parts(eventInformation.part);
            if (part == null)
            {
                status = Status.error;
                return false;

                //throw new Exception($"AGEBasic event part {eventInformation.part} not found");
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

public class OnPlayerTouchStartEvent : OnPlayerBaseEvent
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

public class OnPlayerGrabStartEvent : OnPlayerBaseEvent
{
    public OnPlayerGrabStartEvent(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic)
    { }

    public override void Init()
    {
        if (status == Status.initialized)
            return;

        base.Init();
        if (!loadComponents() || interactablePart.grabDetection.OnGrabEnter == null)
        {
            status = Status.error;
            return;
        }

        interactablePart.grabDetection.OnGrabEnter.AddListener(OnGrab);
        status = Status.initialized;
    }

    void OnGrab()
    {
        RegisterTrigger(true);
    }

    public override void Dispose()
    {
        interactablePart.grabDetection.OnGrabEnter?.RemoveListener(OnGrab);
        base.Dispose();
    }
}

public class OnPlayerGrabEndEvent : OnPlayerBaseEvent
{
    public OnPlayerGrabEndEvent(EventInformation eventInformation, BasicVars vars, basicAGE agebasic) :
        base(eventInformation, vars, agebasic)
    { }

    public override void Init()
    {
        if (status == Status.initialized)
            return;

        base.Init();

        if (!loadComponents() || interactablePart.grabDetection.OnGrabExit == null)
        {
            status = Status.error;
            return;
        }
        interactablePart.grabDetection.OnGrabExit.AddListener(OnGrab);
        status = Status.initialized;
    }

    void OnGrab()
    {
        RegisterTrigger(true);
    }

    public override void Dispose()
    {
        interactablePart.grabDetection.OnGrabExit?.RemoveListener(OnGrab);
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
            case "on-grab-start":
                return new OnPlayerGrabStartEvent(eventInformation, vars, agebasic);
            case "on-grab-end":
                return new OnPlayerGrabEndEvent(eventInformation, vars, agebasic);
        }

        throw new Exception($"AGEBasic Unknown event: {eventInformation.eventId}");
    }
}
