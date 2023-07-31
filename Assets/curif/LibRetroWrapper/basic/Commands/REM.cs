using System;
using System.Collections.Generic;
using System.IO;

class CommandREM : ICommandBase
{
    public virtual string CmdToken { get; } = "REM";
    public CommandType.Type Type { get; } = CommandType.Type.Command;
    public ConfigurationCommands Config { get; set; }

    public CommandREM(ConfigurationCommands config)
    {
    }
    public bool Parse(TokenConsumer tokens)
    {
        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC {CmdToken}] [] ");
        // Additional implementation specific to CommandImplementation class
        return null;
    }

}


class CommandREM2 : ICommandBase
{
    public virtual string CmdToken { get; } = "'";
    public CommandType.Type Type { get; } = CommandType.Type.Command;
    public ConfigurationCommands Config { get; set; }

    public CommandREM2(ConfigurationCommands config)
    {
    }
    public bool Parse(TokenConsumer tokens)
    {
        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        return null;
    }

}