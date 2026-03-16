using System;
using System.Collections.Generic;
using UnityEngine;

class CommandONEVENT : CommandBase
{
    CommandExpression eventConfigExpr;
    CommandExpression jumpLineExpr;
    EventInformation info = new();

    public CommandONEVENT(ConfigurationCommands config) : base(config)
    {
        this.cmdToken = "ONEVENT";
        eventConfigExpr = new(config);
        jumpLineExpr = new(config);
    }

    public override bool Parse(TokenConsumer tokens)
    {
        // ONEVENT ONTIMER(5) GOTO 100
        info.program = config.ProgramName;

        eventConfigExpr.Parse(tokens);

        if (tokens.Token.ToUpper() != "GOTO")
            throw new Exception("Expected GOTO in ONEVENT");
        tokens++;

        jumpLineExpr.Parse(tokens);
        
        return true;
    }

    public override BasicValue Execute(BasicVars vars)
    {
        if (config.events == null)
            throw new Exception($"{CmdToken}: this command requires an initialized events list in ConfigurationCommands.");

        BasicValue configVal = eventConfigExpr.Execute(vars);
        if (configVal.Type() != BasicValue.BasicValueType.Array)
            throw new Exception($"{CmdToken}: first parameter must be an event configuration function (e.g., ONTIMER, ONCONTROL, ONTOUCH, ONGRAB, ONCOLLISION).");

        if (configVal.GetArrayLength() < 2 || configVal[0].GetString() != "CONFIG-EVENT")
            throw new Exception($"{CmdToken}: invalid event configuration array.");

        info.eventId = configVal[1].GetString();
        BasicValue lineVal = jumpLineExpr.Execute(vars);
        info.line = (int)lineVal.GetValueAsNumber();

        // Specific field mapping from array
        switch (info.eventId)
        {
            case "on-timer":
                info.delay = configVal[2].GetValueAsNumber();
                break;
            case "on-control-active-pressed":
            case "on-control-active-held":
            case "on-control-active-released":
                info.control = new ControlInformation();
                info.control.mameControl = configVal[2].GetString();
                info.control.port = (int)configVal[3].GetValueAsNumber();
                break;
            case "on-collision-start":
                info.part = configVal[2].GetString();
                if (configVal.GetArrayLength() > 3)
                {
                    info.partImpacts = new List<string>();
                    for (int i = 3; i < configVal.GetArrayLength(); i++)
                        info.partImpacts.Add(configVal[i].GetString());
                }
                break;
            case "on-touch-start":
            case "on-touch-end":
            case "on-grab-start":
            case "on-grab-end":
                info.part = configVal[2].GetString();
                break;
        }

        // Context search logic
        CabinetAGEBasic cabAgeBasic = null;
        basicAGE ageBasicInstance = null;
        
        if (config.Cabinet != null && config.Cabinet.gameObject != null)
        {
             cabAgeBasic = config.Cabinet.gameObject.GetComponent<CabinetAGEBasic>();
             if (cabAgeBasic != null)
                ageBasicInstance = cabAgeBasic.AGEBasic;
        }
        
        if (ageBasicInstance == null)
        {
            basicAGE[] instances = GameObject.FindObjectsOfType<basicAGE>();
            foreach (var instance in instances)
            {
                if (instance.ConfigCommands == config)
                {
                    ageBasicInstance = instance;
                    break;
                }
            }
            if (ageBasicInstance == null)
                ageBasicInstance = GameObject.FindObjectOfType<basicAGE>();
        }

        if (ageBasicInstance == null)
            throw new Exception($"{CmdToken}: could not find an active basicAGE engine to register the event.");

        Event newEvt = EventsFactory.Factory(info, vars, ageBasicInstance);
        if (newEvt != null)
        {
            config.events.Add(newEvt);
            newEvt.Init();
            if (cabAgeBasic == null)
                ageBasicInstance.startEventCoroutine();
        }

        return null;
    }
}
