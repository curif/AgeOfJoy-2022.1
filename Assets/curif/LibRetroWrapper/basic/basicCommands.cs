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
        commands["'"] = typeof(CommandREM2);
        commands["PRINT"] = typeof(CommandPRINT);
        commands["LET"] = typeof(CommandLET);
        commands["LETS"] = typeof(CommandLETS);
        commands["GOTO"] = typeof(CommandGOTO);
        commands["IF"] = typeof(CommandIFTHEN);
        commands["END"] = typeof(CommandEND);
        commands["GOSUB"] = typeof(CommandGOSUB);
        commands["RETURN"] = typeof(CommandRETURN);
        commands["CALL"] = typeof(CommandCALL);
        commands["FOR"] = typeof(CommandFORTO);
        commands["NEXT"] = typeof(CommandNEXT);
        
        //screen
        commands["CLS"] = typeof(CommandCLS);
        commands["PRINT"] = typeof(CommandPRINT);
        commands["SHOW"] = typeof(CommandSHOW);

        //functions ------

        //math
        functions["ABS"] = typeof(CommandFunctionABS);
        functions["MAX"] = typeof(CommandFunctionMAX);
        functions["MIN"] = typeof(CommandFunctionMIN);
        functions["RND"] = typeof(CommandFunctionRND);
        functions["COS"] = typeof(CommandFunctionCOS);
        functions["SIN"] = typeof(CommandFunctionSIN);
        functions["TAN"] = typeof(CommandFunctionTAN);
        functions["MOD"] = typeof(CommandFunctionMOD);
        functions["INT"] = typeof(CommandFunctionINT);
        functions["NOT"] = typeof(CommandFunctionNOT);
        // strings
        functions["LEN"] = typeof(CommandFunctionLEN);
        functions["UCASE"] = typeof(CommandFunctionUCASE);
        functions["LCASE"] = typeof(CommandFunctionLCASE);
        functions["SUBSTR"] = typeof(CommandFunctionSUBSTR);
        functions["RTRIM"] = typeof(CommandFunctionRTRIM);
        functions["LTRIM"] = typeof(CommandFunctionLTRIM);
        functions["TRIM"] = typeof(CommandFunctionTRIM);
        functions["STR"] = typeof(CommandFunctionSTR);

        //introspection
        functions["EXISTS"] = typeof(CommandFunctionEXIST);

        // configuration settings -------

        //ROOMs
        functions["ROOMNAME"] = typeof(CommandFunctionROOMNAME);
        functions["ROOMCOUNT"] = typeof(CommandFunctionROOMCOUNT);
        functions["ROOMGET"] = typeof(CommandFunctionROOMGET);
        functions["ROOMGETNAME"] = typeof(CommandFunctionROOMGETNAME);
        functions["ROOMGETDESC"] = typeof(CommandFunctionROOMGETDESC);
        functions["ROOMTELEPORT"] = typeof(CommandFunctionROOMTELEPORT);
        
        //Controllers
        functions["CONTROLACTIVE"] = typeof(CommandFunctionCONTROLACTIVE);

        //cabinets in the room
        functions["CABROOMCOUNT"] = typeof(CommandFunctionCABROOMCOUNT);
        functions["CABROOMGETNAME"] = typeof(CommandFunctionCABROOMGETNAME);
        functions["CABROOMREPLACE"] = typeof(CommandFunctionCABROOMREPLACE);

        //cabinets in registry
        functions["CABDBCOUNT"] = typeof(CommandFunctionCABDBCOUNT);
        functions["CABDBCOUNTINROOM"] = typeof(CommandFunctionCABDBCOUNTINROOM);
        //functions["CABDBREPLACE"] = typeof(CommandFunctionCABDBREPLACE);
        functions["CABDBGETNAME"] = typeof(CommandFunctionCABDBGETNAME);
        //functions["CABDBGET"] = typeof(CommandFunctionCABDBGET);
        functions["CABDBDELETE"] = typeof(CommandFunctionCABDBDELETE);
        functions["CABDBADD"] = typeof(CommandFunctionCABDBADD);
        functions["CABDBSAVE"] = typeof(CommandFunctionCABDBSAVE);
        functions["CABDBGETASSIGNED"] = typeof(CommandFunctionCABDBGETASSIGNED);
        functions["CABDBASSIGN"] = typeof(CommandFunctionCABDBASSIGN);

        //cabinet in AGEBasic for the actual cabinet
        functions["CABPARTSCOUNT"] = typeof(CommandFunctionCABPARTSCOUNT);
        functions["CABPARTSNAME"] = typeof(CommandFunctionCABPARTSNAME);
        functions["CABPARTSPOSITION"] = typeof(CommandFunctionCABPARTSPOSITION);
        functions["CABPARTSENABLE"] = typeof(CommandFunctionCABPARTSENABLE);
        functions["CABINSERTCOIN"] = typeof(CommandFunctionCABINSERTCOIN);

        //debug
        functions["DEBUGMODE"] = typeof(CommandFunctionDEBUGMODE);

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
        return commands.ContainsKey(command.ToUpper());
    }
    public static bool IsFunction(string function)
    {
        return functions.ContainsKey(function.ToUpper());
    }
}
