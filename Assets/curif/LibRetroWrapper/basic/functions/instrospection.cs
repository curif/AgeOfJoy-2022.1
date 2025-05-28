using System;

class CommandFunctionEXIST : CommandFunctionSingleExpressionBase
{
    public CommandFunctionEXIST(ConfigurationCommands config) : base(config)
    {
        cmdToken = "EXIST";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val, " - require a variable name to operate.");
        string var = val.GetValueAsString();
        if (!BasicVar.IsVariable(var))
            throw new Exception($"{CmdToken} {var} is not a valid variable name.");

        return (vars.Exists(var) ? new BasicValue(1) : new BasicValue(0));
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

        // Using a switch statement for cleaner type checking
        switch (val.Type())
        {
            case BasicValue.BasicValueType.String:
                return new BasicValue("STRING");
            case BasicValue.BasicValueType.Number:
                return new BasicValue("NUMBER");
            case BasicValue.BasicValueType.Array:
                return new BasicValue("ARRAY");
            default: // Handles any other type that might exist or be added later
                return new BasicValue("EMPTY");
        }
    }
}