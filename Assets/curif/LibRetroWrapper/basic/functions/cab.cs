using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

class CommandFunctionCABPARTSCOUNT : CommandFunctionNoExpressionBase
{

    public CommandFunctionCABPARTSCOUNT(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSCOUNT";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            return new BasicValue(0);

        return new BasicValue((double)config.Cabinet.transform.childCount);
    }
}

class CommandFunctionCABPARTSNAME : CommandFunctionSingleExpressionBase
{

    public CommandFunctionCABPARTSNAME(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSNAME";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            return new BasicValue("");

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNumber(val);

        int partNum = (int)val.GetNumber();
        // Check if the parent has at least 3 children
        if (config.Cabinet.transform.childCount < partNum + 1)
            return new BasicValue("");

        Transform child = config.Cabinet.transform.GetChild(partNum);
        return new BasicValue(child.name);
    }
}


class CommandFunctionCABPARTSPOSITION : CommandFunctionSingleExpressionBase
{

    public CommandFunctionCABPARTSPOSITION(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSPOSITION";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            return new BasicValue(-1);

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val);

        string partName = val.GetString();
        Transform child = config.Cabinet.transform.Find(partName);
        return new BasicValue(child == null ? -1 : child.GetSiblingIndex());
    }
}

class CommandFunctionCABPARTSENABLE : CommandFunctionExpressionListBase
{

    public CommandFunctionCABPARTSENABLE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSENABLE";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2);
    }
    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            return new BasicValue(-1);

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - part number");
        FunctionHelper.ExpectedNumber(vals[1], " - enabled 1/0");

        int partNum = (int)vals[0].GetNumber();
        // Check if the parent has at least N children
        if (config.Cabinet.transform.childCount < partNum + 1)
            return new BasicValue(-1);

        Transform child = config.Cabinet.transform.GetChild(partNum);
        child.gameObject.SetActive(vals[1].GetNumber() != 0);

        return new BasicValue(1);
    }
}

class CommandFunctionCABINSERTCOIN : CommandFunctionNoExpressionBase
{

    public CommandFunctionCABINSERTCOIN(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABINSERTCOIN";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.CoinSlot == null)
            return new BasicValue(-1);

        config.CoinSlot.insertCoin();
        return new BasicValue(1);
    }
}