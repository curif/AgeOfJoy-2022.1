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
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config == null)
            return new BasicValue(-1);

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNumber(val);

        config.DebugMode = val.GetNumber() != 0;

        return new BasicValue(val);
    }
}


class CommandFunctionTYPE : CommandFunctionSingleExpressionBase
{
    public CommandFunctionTYPE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "TYPE";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);

        if (val.Type() == BasicValue.BasicValueType.String)
            return new BasicValue("STRING");
        else if (val.Type() == BasicValue.BasicValueType.Number)
            return new BasicValue("NUMBER");

        return new BasicValue("EMPTY");
    }
}