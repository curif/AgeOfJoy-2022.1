using System;
using System.Collections.Generic;
using System.IO;

class CommandSHOW : ICommandBase
{
    public string CmdToken { get; } = "SHOW";
    public CommandType.Type Type { get; } = CommandType.Type.Command;
    ConfigurationCommands config;

    public CommandSHOW(ConfigurationCommands config)
    {
        this.config = config;
    }

    public bool Parse(TokenConsumer tokens)
    {
        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.ScreenGenerator == null)
        {
            return null;
        }

        config.ScreenGenerator.DrawScreen();

        return null;
    }

}