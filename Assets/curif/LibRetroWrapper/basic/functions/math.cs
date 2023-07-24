using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

class CommandFunctionABS : CommandFunctionSingleExpressionBase
{
    public CommandFunctionABS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "ABS";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        double ret = Math.Abs(val.GetValueAsNumber());
        return new BasicValue(ret);
    }
}

class CommandFunctionMAX : CommandFunctionExpressionListBase
{
    public CommandFunctionMAX(ConfigurationCommands config) : base(config)
    {
        cmdToken = "MAX";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        if (!vals[0].IsNumber() || !vals[1].IsNumber())
            throw new Exception($"{CmdToken} require numbers to operate.");

        double ret = Math.Max(vals[0].GetValueAsNumber(),
                                vals[1].GetValueAsNumber());

        return new BasicValue(ret);
    }

}
class CommandFunctionMIN : CommandFunctionExpressionListBase
{
    public CommandFunctionMIN(ConfigurationCommands config) : base(config)
    {
        cmdToken = "MIN";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 2);
    }


    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0]);
        FunctionHelper.ExpectedNumber(vals[1]);

        double ret = Math.Min(vals[0].GetValueAsNumber(),
                                vals[1].GetValueAsNumber());

        return new BasicValue(ret);
    }

}


class CommandFunctionRND : CommandFunctionExpressionListBase
{
    public CommandFunctionRND(ConfigurationCommands config) : base(config)
    {
        cmdToken = "RND";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0]);
        FunctionHelper.ExpectedNumber(vals[1]);
        double randomValue = UnityEngine.Random.Range((float)vals[0].GetValueAsNumber(),
                                                        (float)vals[1].GetValueAsNumber());
        return new BasicValue(randomValue);
    }

}

class CommandFunctionTAN : CommandFunctionSingleExpressionBase
{
    public CommandFunctionTAN(ConfigurationCommands config) : base(config)
    {
        cmdToken = "TAN";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        if (!val.IsNumber())
            throw new Exception($"{CmdToken} require numbers to operate.");
        double angle = val.GetValueAsNumber();

        double ret = (double)Math.Tan(angle);

        return new BasicValue(ret);
    }
}

class CommandFunctionCOS : CommandFunctionSingleExpressionBase
{
    public CommandFunctionCOS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "COS";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        if (!val.IsNumber())
            throw new Exception($"{CmdToken} require numbers to operate.");

        double angle = val.GetValueAsNumber();

        double ret = (double)Math.Cos(angle);

        return new BasicValue(ret);
    }
}




class CommandFunctionSIN : CommandFunctionSingleExpressionBase
{
    public CommandFunctionSIN(ConfigurationCommands config) : base(config)
    {
        cmdToken = "SIN";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        double angle = val.GetValueAsNumber();

        double ret = (double)Math.Sin(angle);

        return new BasicValue(ret);
    }
}

class CommandFunctionINT : CommandFunctionSingleExpressionBase
{
    public CommandFunctionINT(ConfigurationCommands config) : base(config)
    {
        cmdToken = "INT";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        int retInt = (int)val.GetValueAsNumber();

        return new BasicValue((double)retInt);
    }
}
class CommandFunctionNOT : CommandFunctionSingleExpressionBase
{
    public CommandFunctionNOT(ConfigurationCommands config) : base(config)
    {
        cmdToken = "NOT";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);

        return val.IsTrue() ? new BasicValue(0) : new BasicValue(1);
    }
}

class CommandFunctionMOD : CommandFunctionExpressionListBase
{
    public CommandFunctionMOD(ConfigurationCommands config) : base(config)
    {
        cmdToken = "MOD";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);
        if (!vals[0].IsNumber() || !vals[1].IsNumber())
            throw new Exception($"{CmdToken} require numbers to operate.");

        double dividend = vals[0].GetValueAsNumber();
        double divisor = vals[1].GetValueAsNumber();

        double ret = dividend % divisor;

        return new BasicValue(ret);
    }
}
