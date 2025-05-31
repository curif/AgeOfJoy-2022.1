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


class CommandFunctionSCREENSIZE : CommandFunctionNoExpressionBase
{
    public CommandFunctionSCREENSIZE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "SCREENSIZE";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.ScreenGenerator == null)
            throw new Exception("This AGEBasic program doesn't have a screen.");

        BasicValue array = new BasicValue().Dim(2);
        array[0] = new BasicValue(config.ScreenGenerator.CharactersXCount); 
        array[1] = new BasicValue(config.ScreenGenerator.CharactersYCount);
        return array;
    }
}


class CommandFunctionDSCREENWIDTH : CommandFunctionNoExpressionBase
{
    public CommandFunctionDSCREENWIDTH(ConfigurationCommands config) : base(config)
    {
        cmdToken = "DSCREENWIDTH";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.ScreenGenerator == null)
            throw new Exception("This AGEBasic program doesn't have a screen.");

        return new BasicValue(config.ScreenGenerator.TextureWidth);
    }
}


class CommandFunctionDSCREENHEIGHT : CommandFunctionNoExpressionBase
{
    public CommandFunctionDSCREENHEIGHT(ConfigurationCommands config) : base(config)
    {
        cmdToken = "DSCREENHEIGHT";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.ScreenGenerator == null)
            throw new Exception("This AGEBasic program doesn't have a screen.");
        BasicValue ret = new BasicValue(config.ScreenGenerator.TextureHeight);
        return ret;
    }
}


class CommandFunctionDSCREENSIZE : CommandFunctionNoExpressionBase
{
    public CommandFunctionDSCREENSIZE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "DSCREENSIZE";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.ScreenGenerator == null)
            throw new Exception("This AGEBasic program doesn't have a screen.");

        BasicValue array = new BasicValue().Dim(2);
        array[0] = new BasicValue(config.ScreenGenerator.TextureWidth);
        array[1] = new BasicValue(config.ScreenGenerator.TextureHeight);
        return array;
    }
}
