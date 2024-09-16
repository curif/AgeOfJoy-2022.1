using System;
using System.Collections.Generic;
using System.IO;

class CommandFunctionDEBUGMODE : CommandFunctionSingleExpressionBase
{

    public CommandFunctionDEBUGMODE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "DEBUGMODE";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config == null)
            return new BasicValue(-1);

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNumber(val);

        config.DebugMode = val.IsTrue();

        return new BasicValue(val);
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

        if (val.Type() == BasicValue.BasicValueType.String)
            return new BasicValue("STRING");
        else if (val.Type() == BasicValue.BasicValueType.Number)
            return new BasicValue("NUMBER");

        return new BasicValue("EMPTY");
    }
}


class CommandFunctionLOG : CommandFunctionSingleExpressionBase
{
    public CommandFunctionLOG(ConfigurationCommands config) : base(config)
    {
        cmdToken = "LOG";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        if (!config.DebugMode)
            return null;

        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val, "- LOG*() parameter expect a string");

        ConfigManager.WriteConsoleAGEBasic(val.GetString());

        return null;
    }
}

class CommandFunctionLOGERROR : CommandFunctionSingleExpressionBase
{
    public CommandFunctionLOGERROR(ConfigurationCommands config) : base(config)
    {
        cmdToken = "LOGERROR";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        if (!config.DebugMode)
            return null;

        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val, "- LOG*() parameter expect a string");

        ConfigManager.WriteConsoleErrorAGEBasic(val.GetString());

        return null;
    }
}

class CommandFunctionLOGWARNING : CommandFunctionSingleExpressionBase
{
    public CommandFunctionLOGWARNING(ConfigurationCommands config) : base(config)
    {
        cmdToken = "LOGWARNING";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        if (!config.DebugMode)
            return null;

        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val, "- LOG*() parameter expect a string");

        ConfigManager.WriteConsoleWarningAGEBasic(val.GetString());

        return null;
    }
}


class CommandFunctionASSERT : CommandFunctionExpressionListBase
{

    public CommandFunctionASSERT(ConfigurationCommands config) : base(config)
    {
        cmdToken = "ASSERT";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[1], "- volume should be a number");

        ConfigManager.AssertWriteConsoleAGEBasic(vals[0].IsTrue(), "[AGEBASIC ASSERT] " + vals[1].GetString());

        return null;
    }
}