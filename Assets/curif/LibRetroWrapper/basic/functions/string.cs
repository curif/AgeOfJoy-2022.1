using System;
using System.Collections.Generic;
using System.IO;

class CommandFunctionLEN : CommandFunctionSingleExpressionBase
{
    public CommandFunctionLEN(ConfigurationCommands config) : base(config)
    {
        cmdToken = "LEN";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        double ret = val.GetValueAsString().Length;
        return new BasicValue(ret);
    }
}
class CommandFunctionUCASE : CommandFunctionSingleExpressionBase
{
    public CommandFunctionUCASE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "UCASE";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        string ret = val.GetValueAsString().ToUpper();
        return new BasicValue(ret);
    }
}


class CommandFunctionLCASE : CommandFunctionSingleExpressionBase
{
    public CommandFunctionLCASE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "LCASE";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        string ret = val.GetValueAsString().ToLower();
        return new BasicValue(ret);
    }
}

class CommandFunctionRTRIM : CommandFunctionSingleExpressionBase
{
    public CommandFunctionRTRIM(ConfigurationCommands config) : base(config)
    {
        cmdToken = "RTRIM";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        string ret = val.GetValueAsString().TrimEnd();

        return new BasicValue(ret);
    }
}

class CommandFunctionLTRIM : CommandFunctionSingleExpressionBase
{
    public CommandFunctionLTRIM(ConfigurationCommands config) : base(config)
    {
        cmdToken = "LTRIM";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        string ret = val.GetValueAsString().TrimStart();

        return new BasicValue(ret);
    }
}

class CommandFunctionTRIM : CommandFunctionSingleExpressionBase
{
    public CommandFunctionTRIM(ConfigurationCommands config) : base(config)
    {
        cmdToken = "TRIM";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        string ret = val.GetValueAsString().Trim();

        return new BasicValue(ret);
    }
}

class CommandFunctionSUBSTR : CommandFunctionExpressionListBase
{
    public CommandFunctionSUBSTR(ConfigurationCommands config) : base(config)
    {
        cmdToken = "SUBSTR";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 3);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);

        string input = vals[0].GetValueAsString();
        int startIndex = (int)vals[1].GetValueAsNumber();
        int length = (int)vals[2].GetValueAsNumber();

        string ret = input.Substring(startIndex, length);

        return new BasicValue(ret);
    }
}

class CommandFunctionSTR : CommandFunctionSingleExpressionBase
{
    public CommandFunctionSTR(ConfigurationCommands config) : base(config)
    {
        cmdToken = "STR";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        BasicValue ret = new BasicValue(val);
        ret.CastTo(BasicValue.BasicValueType.String);
        return ret;
    }
}

class CommandFunctionGETMEMBER : CommandFunctionExpressionListBase
{
    public CommandFunctionGETMEMBER(ConfigurationCommands config) : base(config)
    {
        cmdToken = "GETMEMBER";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 3);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);

        string input = vals[0].GetValueAsString();
        int memberIndex = (int)vals[1].GetValueAsNumber();
        string separator = vals[2].GetValueAsString(); 

        string[] parts = input.Split(new[] { separator }, StringSplitOptions.None);

        if (memberIndex >= 0 && memberIndex < parts.Length)
        {
            string ret = parts[memberIndex];
            return new BasicValue(ret);
        }
        else
        {
            // Handle index out of bounds or invalid member
            return new BasicValue(""); // Return an empty string or handle the error accordingly
        }
    }
}

class CommandFunctionCOUNTMEMBERS : CommandFunctionExpressionListBase
{
    public CommandFunctionCOUNTMEMBERS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "COUNTMEMBERS";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 2);
    }
    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);

        string input = vals[0].GetValueAsString();
        string separator = vals[1].GetValueAsString(); 
        int count = input.Split(new[] { separator }, StringSplitOptions.None).Length;

        return new BasicValue(count);
    }
}
