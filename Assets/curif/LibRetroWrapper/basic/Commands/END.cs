using System;
using System.Collections.Generic;
using System.IO;

class CommandEND : ICommandBase
{
    public string CmdToken { get; } = "END";
    public CommandType.Type Type { get; } = CommandType.Type.Command;
    ConfigurationCommands config;
    public CommandEND(ConfigurationCommands config)
    {
        this.config = config;
    }
    public bool Parse(TokenConsumer tokens)
    {
        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}]");
        // return new BasicValue(double.MaxValue);
        this.config.stop = true;
        return null;
    }

}