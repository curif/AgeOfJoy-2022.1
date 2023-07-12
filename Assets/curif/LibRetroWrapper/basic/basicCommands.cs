using System;
using System.Collections.Generic;
using System.IO;


public static class Commands
{
    private static Dictionary<string, Type> commands = new();

    static Commands()
    {
        commands["REM"] = typeof(CommandREM);
        commands["PRINT"] = typeof(CommandPRINT);
        commands["LET"] = typeof(CommandLET);
        commands["GOTO"] = typeof(CommandGOTO);
        commands["IF"] = typeof(CommandIFTHEN);
        commands["END"] = typeof(CommandEND);
    }

    public static ICommandBase GetNew(string CommandType)
    {
        CommandType = CommandType.ToUpper();
        if (!commands.ContainsKey(CommandType))
            return null;
        // return new commands[CommandType](args);
        return (ICommandBase)Activator.CreateInstance(commands[CommandType]);
    }
    public static bool IsCommand(string command)
    {
        return commands.ContainsKey(command);
    }
}
