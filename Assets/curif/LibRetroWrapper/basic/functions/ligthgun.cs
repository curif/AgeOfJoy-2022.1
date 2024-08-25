
using System;
using Unity;
using UnityEngine;

class CommandFunctionLIGHTGUNGETPOINTEDPART : CommandFunctionSingleExpressionBase
{

    public CommandFunctionLIGHTGUNGETPOINTEDPART(ConfigurationCommands config) : base(config)
    {
        cmdToken = "LIGHTGUNGETPOINTEDPART";
    }

    public Event GetEventByName(string name)
    {
        foreach (Event evt in config.events)
            if (evt.eventInformation.name == name)
                return evt;
        return null;
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config.lightGunTarget == null)
            throw new Exception($"{CmdToken}: AGEBasic can't access lightgun information.");

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val, " - event name should be a string");

        GameObject part = config.lightGunTarget.GetLastGameObjectHit();
        if (part != null)
            return new BasicValue(part.name);

        return new BasicValue("");
    }
}
