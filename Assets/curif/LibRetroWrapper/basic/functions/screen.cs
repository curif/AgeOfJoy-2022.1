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
            return new BasicValue(0);

        return new BasicValue(config.ScreenGenerator.CharactersWidth);
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
            return new BasicValue(0);

        return new BasicValue(config.ScreenGenerator.CharactersHeight);
    }
}