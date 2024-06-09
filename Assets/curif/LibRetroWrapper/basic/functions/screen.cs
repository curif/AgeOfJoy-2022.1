using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using UnityEngine;

class CommandFunctionSCREENWIDTH : CommandFunctionNoExpressionBase
{
    public CommandFunctionSCREENWIDTH(ConfigurationCommands config) : base(config)
    {
        cmdToken = "SCREENWIDTH";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.ScreenGenerator == null)
            throw new Exception("This AGEBasic program doesn't have a screen.");

        return new BasicValue(config.ScreenGenerator.CharactersXCount);
    }
}


class CommandFunctionSCREENHEIGHT : CommandFunctionNoExpressionBase
{
    public CommandFunctionSCREENHEIGHT(ConfigurationCommands config) : base(config)
    {
        cmdToken = "SCREENHEIGHT";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.ScreenGenerator == null)
            throw new Exception("This AGEBasic program doesn't have a screen.");
        BasicValue ret = new BasicValue(config.ScreenGenerator.CharactersYCount);
        return ret;
    }
}