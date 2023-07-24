using System;
using System.Collections.Generic;
using System.IO;


class CommandNEXT : ICommandBase
{
    public string CmdToken { get; } = "NEXT";
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    BasicVar var;

    ConfigurationCommands config;

    public CommandNEXT(ConfigurationCommands config)
    {
        this.config = config;
    }

    public bool Parse(TokenConsumer tokens)
    {
        if (!BasicVar.IsVariable(tokens.Token))
            throw new Exception($"{tokens.Token} isn't a valid variable (FOR)");

        var = new(tokens.Token);

        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC {CmdToken}]");

        if (!config.ForToNext.ContainsKey(var.Name))
            throw new Exception("NEXT without FOR");

        forToStorage ft = config.ForToNext[var.Name];

        BasicValue endValue = ft.endExpr.Execute(vars);
        FunctionHelper.ExpectedNumber(endValue, "- TO must have an expression or number");

        BasicValue actualValue = vars.GetValue(ft.var);
        FunctionHelper.ExpectedNumber(actualValue, "- FOR must have an expression or number");

        double actual = actualValue.GetValueAsNumber() + 1;
        double end = endValue.GetValueAsNumber();
        ConfigManager.WriteConsole($"[AGE BASIC {CmdToken}] var:{var.Name}: {actual} to {end}");

        actualValue.SetValue(actual);
        vars.SetValue(ft.var, actualValue);

        if (actual > end)
        {
            config.ForToNext.Remove(var.Name);
            return null;
        }

        config.JumpNextTo = ft.lineNumber;

        return null;
    }
}