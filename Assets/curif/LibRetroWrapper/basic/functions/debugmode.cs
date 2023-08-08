using System;
using System.Collections.Generic;
using System.IO;

class CommandFunctionDEBUGMODE : CommandFunctionSingleExpressionBase
{

    public CommandFunctionDEBUGMODE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "DEBUGMODE";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config == null)
            return new BasicValue(-1);

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNumber(val);

        config.DebugMode = val.GetNumber() != 0;
        
        return new BasicValue(val);
    }
}
