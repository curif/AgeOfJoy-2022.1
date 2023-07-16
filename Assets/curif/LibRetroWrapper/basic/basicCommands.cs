using System;
using System.Collections.Generic;
using System.IO;


public static class Commands
{
    private static Dictionary<string, Type> commands = new();
    private static Dictionary<string, Type> functions = new();

    static Commands()
    {
        commands["REM"] = typeof(CommandREM);
        commands["PRINT"] = typeof(CommandPRINT);
        commands["LET"] = typeof(CommandLET);
        commands["GOTO"] = typeof(CommandGOTO);
        commands["IF"] = typeof(CommandIFTHEN);
        commands["END"] = typeof(CommandEND);
        commands["GOSUB"] = typeof(CommandGOSUB);
        commands["RETURN"] = typeof(CommandRETURN);
        //screen
        commands["CLS"] = typeof(CommandCLS);
        commands["PRINT"] = typeof(CommandPRINT);

        //funct
        functions["ABS"] = typeof(CommandFunctionABS);
        functions["MAX"] = typeof(CommandFunctionMAX);
        functions["MIN"] = typeof(CommandFunctionMIN);
        functions["RND"] = typeof(CommandFunctionRND);
        functions["COS"] = typeof(CommandFunctionCOS);
        functions["SIN"] = typeof(CommandFunctionSIN);
        functions["TAN"] = typeof(CommandFunctionTAN);
        functions["MOD"] = typeof(CommandFunctionMOD);

        functions["LEN"] = typeof(CommandFunctionLEN);
        functions["UCASE"] = typeof(CommandFunctionUCASE);
        functions["LCASE"] = typeof(CommandFunctionLCASE);
        functions["SUBSTR"] = typeof(CommandFunctionSUBSTR);
        functions["RTRIM"] = typeof(CommandFunctionRTRIM);
        functions["LTRIM"] = typeof(CommandFunctionLTRIM);
        functions["TRIM"] = typeof(CommandFunctionTRIM);
        functions["STR"] = typeof(CommandFunctionSTR);

        // configuration settings
        functions["ROOMNAME"] = typeof(CommandFunctionROOMNAME);

    }

    public static ICommandBase GetNew(string CommandType, ConfigurationCommands config)
    {
        CommandType = CommandType.ToUpper();

        if (commands.ContainsKey(CommandType))
            return (ICommandBase)Activator.CreateInstance(commands[CommandType], config);
        if (functions.ContainsKey(CommandType))
            return (ICommandBase)Activator.CreateInstance(functions[CommandType], config);

        return null;
    }
    public static bool IsCommand(string command)
    {
        return commands.ContainsKey(command);
    }
    public static bool IsFunction(string function)
    {
        return functions.ContainsKey(function);
    }
}
