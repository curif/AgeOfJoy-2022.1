using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Windows;

class CommandFunctionABS : CommandFunctionSingleExpressionBase
{
    public CommandFunctionABS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "ABS";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
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
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");
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
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");
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
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");
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
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
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
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
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
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
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
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        int retInt = (int)val.GetValueAsNumber();

        return new BasicValue((double)retInt);
    }
}

class CommandFunctionVAL : CommandFunctionSingleExpressionBase
{
    public CommandFunctionVAL(ConfigurationCommands config) : base(config)
    {
        cmdToken = "VAL";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);

        if (val.IsNumber())
            return new BasicValue(val);

        if (double.TryParse(val.GetValueAsString(), out double result))
            return new BasicValue(result);

        throw new Exception("string value can't be coarsed to a number");
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
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
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
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);
        if (!vals[0].IsNumber() || !vals[1].IsNumber())
            throw new Exception($"{CmdToken} require numbers to operate.");

        double dividend = vals[0].GetValueAsNumber();
        double divisor = vals[1].GetValueAsNumber();

        double ret = dividend % divisor;

        return new BasicValue(ret);
    }
}

class CommandFunctionAND : CommandFunctionExpressionListBase
{
    public CommandFunctionAND(ConfigurationCommands config) : base(config)
    {
        cmdToken = "AND";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);

        // Iterate through all expressions to evaluate logical AND
        bool result = true;  // Start with true, and set to false if any expression is false
        foreach (var val in vals)
        {
            if (val == null) break;
            if (!val.IsTrue())  // If any expression is false, result will be false
            {
                result = false;
                break;  // Short-circuit if we find one false expression
            }
        }

        return new BasicValue(result ? 1 : 0);
    }
}

class CommandFunctionOR : CommandFunctionExpressionListBase
{
    public CommandFunctionOR(ConfigurationCommands config) : base(config)
    {
        cmdToken = "OR";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        
        bool result = false;
        foreach (var val in vals)
        {
            if (val == null) break;
            if (val.IsTrue())  // If any expression is true, result will be true
            {
                result = true;
                break;  // Short-circuit if we find one true expression
            }
        }

        return new BasicValue(result ? 1 : 0);
    }
}

class CommandFunctionIIF : CommandFunctionExpressionListBase
{
    public CommandFunctionIIF(ConfigurationCommands config) : base(config)
    {
        cmdToken = "IIF";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 3);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        BasicValue condition = exprs.ExecuteByPosition(0, vars);
        if (condition.IsTrue())
        {
            return exprs.ExecuteByPosition(1, vars);
        }
        return exprs.ExecuteByPosition(2, vars);
    }
}


class CommandFunctionHEXTODEC : CommandFunctionSingleExpressionBase
{
    public CommandFunctionHEXTODEC(ConfigurationCommands config) : base(config)
    {
        cmdToken = "HEXTODEC";
    }
    
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        double ret = FunctionHelper.HexStringToDecimal(val.GetValueAsString());
        return new BasicValue(ret);
    }
}
