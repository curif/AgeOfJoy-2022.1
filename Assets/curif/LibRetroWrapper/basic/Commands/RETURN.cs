using System;
using System.Collections.Generic;
using System.IO;

class CommandRETURN : ICommandBase
{
    public string CmdToken { get; } = "RETURN";
    public CommandType.Type Type { get; } = CommandType.Type.Command;
    ConfigurationCommands config;
    public CommandRETURN(ConfigurationCommands config)
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

        if (config.Gosub.Count == 0)
            throw new Exception("RETURN without GOSUB");

        config.JumpNextTo = config.Gosub.Pop();
        return null;
    }

}