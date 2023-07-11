using System;
using System.Collections.Generic;
using System.IO;

class CommandPRINT : ICommandBase
{
    public string CmdToken { get; } = "PRINT";
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    public CommandPRINT()
    {
    }

    public bool Parse(TokenConsumer tokens)
    {
        // Implementation specific to CommandImplementation class
        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC {CmdToken}]");
        // Additional implementation specific to CommandImplementation class
        return null;
    }

}