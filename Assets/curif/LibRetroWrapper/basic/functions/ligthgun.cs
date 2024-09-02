
using System;
using Unity;
using UnityEngine;

class CommandFunctionCABPARTLIGHTGUNHIT : CommandFunctionNoExpressionBase
{

    public CommandFunctionCABPARTLIGHTGUNHIT(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTLIGHTGUNHIT";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config.lightGunTarget == null)
            throw new Exception($"{CmdToken}: AGEBasic can't access lightgun information.");

        GameObject part = config.lightGunTarget.GetLastGameObjectHit();
        if (part != null)
            return new BasicValue(part.name);

        return new BasicValue("");
    }
}
