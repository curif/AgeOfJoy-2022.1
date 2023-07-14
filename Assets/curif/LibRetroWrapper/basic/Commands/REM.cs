using System;
using System.Collections.Generic;
using System.IO;

class CommandREM : ICommandBase
{
    public string CmdToken { get; } = "REM";
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    public CommandREM()
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