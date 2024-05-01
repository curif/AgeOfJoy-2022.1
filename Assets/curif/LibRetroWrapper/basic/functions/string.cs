using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class CommandFunctionLEN : CommandFunctionSingleExpressionBase
{
    public CommandFunctionLEN(ConfigurationCommands config) : base(config)
    {
        cmdToken = "LEN";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val);

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
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val);

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
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val);

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
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val);

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
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val);

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
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val);
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
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0]);
        FunctionHelper.ExpectedNumber(vals[1], " - start");
        FunctionHelper.ExpectedNumber(vals[2], " - length");

        string input = vals[0].GetValueAsString();
        int startIndex = (int)vals[1].GetValueAsNumber();
        int length = (int)vals[2].GetValueAsNumber();

        if (length < 0)
            length = 0;

        // Adjust length if it extends beyond the end of the string
        if (startIndex + length > input.Length)
        {
            length = input.Length - startIndex;
        }

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
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
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
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0]);
        FunctionHelper.ExpectedNumber(vals[1], " - index");
        FunctionHelper.ExpectedString(vals[2], " - separator");

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
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0]);
        FunctionHelper.ExpectedString(vals[1], " - separator");

        string input = vals[0].GetValueAsString();
        string separator = vals[1].GetValueAsString();
        int count = input.Split(new[] { separator }, StringSplitOptions.None).Length;

        return new BasicValue(count);
    }
}


class CommandFunctionISMEMBER : CommandFunctionExpressionListBase
{
    public CommandFunctionISMEMBER(ConfigurationCommands config) : base(config)
    {
        cmdToken = "ISMEMBER";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 3);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0]);
        FunctionHelper.ExpectedString(vals[1], " - list");
        FunctionHelper.ExpectedString(vals[2], " - string");

        string separatedList = vals[0].GetValueAsString();
        string member = vals[1].GetValueAsString();
        string separator = vals[2].GetValueAsString();

        string[] parts = separatedList.Split(new[] { separator }, StringSplitOptions.None);
        return new BasicValue(Array.IndexOf(parts, member) != -1);
    }
}

class CommandFunctionINDEXMEMBER : CommandFunctionExpressionListBase
{
    public CommandFunctionINDEXMEMBER(ConfigurationCommands config) : base(config)
    {
        cmdToken = "INDEXMEMBER";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 3);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0]);
        FunctionHelper.ExpectedString(vals[1], " - list");
        FunctionHelper.ExpectedString(vals[2], " - string");

        string separatedList = vals[0].GetValueAsString();
        string member = vals[1].GetValueAsString();
        string separator = vals[2].GetValueAsString();

        string[] parts = separatedList.Split(new[] { separator }, StringSplitOptions.None);
        return new BasicValue(Array.IndexOf(parts, member));
    }
}

class CommandFunctionREMOVEMEMBER : CommandFunctionExpressionListBase
{
    public CommandFunctionREMOVEMEMBER(ConfigurationCommands config) : base(config)
    {
        cmdToken = "REMOVEMEMBER";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 3);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0]);
        FunctionHelper.ExpectedString(vals[1], " - list");
        FunctionHelper.ExpectedString(vals[2], " - string");

        string separatedList = vals[0].GetValueAsString();
        string member = vals[1].GetValueAsString();
        string separator = vals[2].GetValueAsString();

        string[] parts = separatedList.Split(new[] { separator }, StringSplitOptions.None);
        string result = string.Join(separator, parts.Where(p => p != member)); 
        return new BasicValue(result);
    }
}


class CommandFunctionADDMEMBER : CommandFunctionExpressionListBase
{
    public CommandFunctionADDMEMBER(ConfigurationCommands config) : base(config)
    {
        cmdToken = "ADDMEMBER";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 3);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0]);
        FunctionHelper.ExpectedString(vals[1], " - list");
        FunctionHelper.ExpectedString(vals[2], " - string");

        string separatedList = vals[0].GetValueAsString();
        string member = vals[1].GetValueAsString();
        string separator = vals[2].GetValueAsString();

        return new BasicValue(string.IsNullOrEmpty(separatedList) ? 
            member : 
            separatedList + separator + member);
    }
}