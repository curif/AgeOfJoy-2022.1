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
            throw new Exception($"NEXT without FOR: {var.Name}");

        forToStorage ft = config.ForToNext[var.Name];

        BasicValue endValue = ft.endExpr.Execute(vars);
        FunctionHelper.ExpectedNumber(endValue, "- TO must compute to expression or number");

        BasicValue actualValue = vars.GetValue(var);
        FunctionHelper.ExpectedNumber(actualValue, "- FOR must compute to expression or number");

        double step = 1;
        if (ft.stepExpr != null)
        {
            BasicValue stepValue = ft.stepExpr.Execute(vars);
            FunctionHelper.ExpectedNumber(stepValue, "- STEP must compute to expression or number");
            step = stepValue.GetNumber();
        }

        //it's faster than create a new instance and assign to the vars dic.
        actualValue.SetValue(actualValue.GetNumber() + step);
        // vars.SetValue(var, actualValue);
        ConfigManager.WriteConsole($"[AGE BASIC {CmdToken}] var:{var.Name}: {actualValue.ToString()} to {endValue.ToString()} step {step}");

        if (actualValue > endValue)
            config.ForToNext.Remove(var.Name);
        else
            config.JumpNextTo = ft.lineNumber;

        return null;
    }
}