using System;

class CommandFunctionGETCPU : CommandFunctionNoExpressionBase
{
    public CommandFunctionGETCPU(ConfigurationCommands config) : base(config)
    {
        cmdToken = "GETCPU";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]  ");
        return new BasicValue(config.cpuPercentage);
    }
}


class CommandFunctionSETCPU : CommandFunctionSingleExpressionBase
{
    public CommandFunctionSETCPU(ConfigurationCommands config) : base(config)
    {
        cmdToken = "SETCPU";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]  ");
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNumber(val, "CPU speed multiplier must be a number (e.g., 1 for legacy speed, 500 for fast)");
        config.cpuPercentage = val.GetNumber(); 
        return null;
    }
}
