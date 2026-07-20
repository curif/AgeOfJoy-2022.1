using System;
using System.Collections.Generic;
using UnityEngine;

class CommandONEVENT : CommandBase
{
    CommandExpression eventConfigExpr;
    CommandExpression jumpLineExpr;
    CommandExpression nameExpr;
    EventInformation info = new();

    public CommandONEVENT(ConfigurationCommands config) : base(config)
    {
        this.cmdToken = "ONEVENT";
        eventConfigExpr = new(config);
        jumpLineExpr = new(config);
    }

    public override bool Parse(TokenConsumer tokens)
    {
        // ONEVENT ONTIMER(5) GOTO 100 [NAME "id"]
        info.program = config.ProgramName;

        eventConfigExpr.Parse(tokens);

        if (tokens.Token.ToUpper() != "GOTO")
            throw new Exception("Expected GOTO in ONEVENT");
        tokens++;

        jumpLineExpr.Parse(tokens);

        if (tokens.Token.ToUpper() == "NAME")
        {
            tokens++;
            nameExpr = new(config);
            nameExpr.Parse(tokens);
        }

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

        if (nameExpr != null)
        {
            BasicValue nameVal = nameExpr.Execute(vars);
            FunctionHelper.ExpectedString(nameVal, $"- NAME for {CmdToken}");
            info.name = nameVal.GetString();
        }

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
            case "on-sprite-collision-start":
            case "on-sprite-collision-end":
                info.spriteA = configVal[2].GetString();
                info.spriteB = configVal[3].GetString();
                break;
            case "on-memory-change":
                if (configVal[2].IsString())
                {
                    // Cheat name form: ["CONFIG-EVENT", "on-memory-change", cheatName, varName]
                    info.cheat = configVal[2].GetString();
                    info.varName = configVal[3].GetString();
                }
                else
                {
                    // Raw address form: ["CONFIG-EVENT", "on-memory-change", address, region, varName]
                    info.address = (uint)configVal[2].GetValueAsNumber();
                    info.region = (uint)configVal[3].GetValueAsNumber();
                    info.varName = configVal[4].GetString();
                }
                break;
            case "on-led-change":
                // ["CONFIG-EVENT", "on-led-change", ledIndex, varName]
                info.ledIndex = (int)configVal[2].GetValueAsNumber();
                info.varName = configVal[3].GetString();
                break;
            case "on-custom":
                // ["CONFIG-EVENT", "on-custom", name] - explicit NAME clause (if present) wins
                if (nameExpr == null)
                    info.name = configVal[2].GetString();
                break;
        }

        Event newEvt = EventsFactory.Factory(info, vars, config.ageBasic);
        if (newEvt != null)
        {
            newEvt.Init();
            config.events.Add(newEvt);
            AGEBasicDebug.WriteConsole($"[AGE BASIC RUN #{config.LineNumber} {CmdToken}] added evemt: {newEvt}");

        }

        return null;
    }
}
