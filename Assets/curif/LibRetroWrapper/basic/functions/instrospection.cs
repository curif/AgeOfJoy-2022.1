using System;
using System.Collections.Generic;


class CommandFunctionEXIST : CommandFunctionSingleExpressionBase
{
    public CommandFunctionEXIST(ConfigurationCommands config) : base(config)
    {
        cmdToken = "EXIST";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val, " - require a variable name to operate.");
        string var = val.GetValueAsString();
        if (!BasicVar.IsVariable(var))
            throw new Exception($"{CmdToken} {var} is not a valid variable name.");

        return (vars.Exists(var) ? new BasicValue(1) : new BasicValue(0));
    }
}