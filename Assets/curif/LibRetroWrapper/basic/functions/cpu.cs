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
        FunctionHelper.ExpectedNumber(val, "percentage of cpu must be a number betwee 0 and 100");
        config.cpuPercentage = val.GetNumber(); 
        return null;
    }
}
