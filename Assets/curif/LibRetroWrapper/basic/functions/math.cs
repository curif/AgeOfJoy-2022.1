using System;
using System.Collections.Generic;
using System.IO;

class CommandFunctionMathBase : CommandFunctionBase
{
    public CommandFunctionMathBase(ConfigurationCommands config) : base(config) {}

    protected void validateResults(BasicValue[] vals, int expectedParams)
    {
        if (exprs.Count != expectedParams)
            throw new Exception($"{cmdToken}() parameter/s missing, actual {exprs.Count} expected: {expectedParams}");

        for (int par = 0; par < exprs.Count; par++)
        {
            if (vals[par] == null)
                throw new Exception($"{cmdToken}() parameter #{par} missing");
            if (!vals[par].IsNumber())
                throw new Exception($"{cmdToken}() parameter #{par} must be numbers");
        }
    }
}
class CommandFunctionABS : CommandFunctionMathBase
{
    public CommandFunctionABS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "ABS";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        validateResults(vals, 1);
        double ret = Math.Abs(vals[0].GetValueAsNumber());
        return new BasicValue(ret);
    }
}

class CommandFunctionMAX : CommandFunctionMathBase
{
    public CommandFunctionMAX(ConfigurationCommands config) : base(config)
    {
        cmdToken = "MAX";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        validateResults(vals, 2);
        double ret = Math.Max(vals[0].GetValueAsNumber(),
                                vals[1].GetValueAsNumber());

        return new BasicValue(ret);
    }

}
class CommandFunctionMIN : CommandFunctionMathBase
{
    public CommandFunctionMIN(ConfigurationCommands config) : base(config)
    {
        cmdToken = "MIN";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        validateResults(vals, 2);
        double ret = Math.Min(vals[0].GetValueAsNumber(),
                                vals[1].GetValueAsNumber());

        return new BasicValue(ret);
    }

}


class CommandFunctionRND : CommandFunctionMathBase
{
    Random random = new Random();
    public CommandFunctionRND(ConfigurationCommands config) : base(config)
    {
        cmdToken = "RND";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        validateResults(vals, 2);

        double minValue = vals[0].GetValueAsNumber();
        double maxValue = vals[1].GetValueAsNumber();
        double ret = random.NextDouble() * (maxValue - minValue) + minValue;

        return new BasicValue(ret);
    }

}