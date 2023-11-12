using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

class CommandFunctionCABROOMCOUNT : CommandFunctionNoExpressionBase
{
    public CommandFunctionCABROOMCOUNT(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMCOUNT";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.CabinetsController == null)
            return new BasicValue(0);

        return new BasicValue((double)config.CabinetsController.Count());
    }
}

class CommandFunctionCABROOMGETNAME : CommandFunctionSingleExpressionBase
{
    public CommandFunctionCABROOMGETNAME(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMGETNAME";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        if (config?.CabinetsController == null)
            return new BasicValue("");

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNumber(val);

        CabinetPosition cabPos = config.CabinetsController.GetCabinetByPosition((int)val.GetValueAsNumber());
        if (cabPos == null)
            return new BasicValue("");

        return new BasicValue(cabPos.CabinetDBName);
    }
}

class CommandFunctionCABROOMREPLACE : CommandFunctionExpressionListBase
{
    public CommandFunctionCABROOMREPLACE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMREPLACE";
    }
    
    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        if (config?.CabinetsController == null || config?.ConfigurationController == null)
            return new BasicValue(0);

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - cabinet position");
        FunctionHelper.ExpectedString(vals[1], " - new cabinet name");

        string roomName = config?.ConfigurationController?.GetRoomName();
        if (string.IsNullOrEmpty(roomName))
            return new BasicValue(0); //fail

        string cabinetDBName = vals[1].GetString();
        if (!config.GameRegistry.CabinetExists(cabinetDBName))
            return new BasicValue(0); //fail

        bool result = config.CabinetsController.ReplaceInRoom((int)vals[0].GetNumber(),
                                                                roomName,
                                                                cabinetDBName);

        return new BasicValue(result ? 1 : 0);
    }
}
