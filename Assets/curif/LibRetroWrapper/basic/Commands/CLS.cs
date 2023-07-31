using System;
using System.Collections.Generic;
using System.IO;

class CommandCLS : ICommandBase
{
    public string CmdToken { get; } = "CLS";
    public CommandType.Type Type { get; } = CommandType.Type.Command;
    ConfigurationCommands config;

    public CommandCLS(ConfigurationCommands config)
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
        if (config?.ScreenGenerator != null)
        {
            ConfigManager.WriteConsole($"clear and update ");
            config.ScreenGenerator.Clear();
            config.ScreenGenerator.DrawScreen();
        }
        return null;
    }

}