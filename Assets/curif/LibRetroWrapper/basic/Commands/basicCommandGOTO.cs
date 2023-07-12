using System;
using System.Collections.Generic;
using System.IO;

class CommandGOTO : ICommandBase
{
    public string CmdToken { get; } = "GOTO";
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    CommandExpression expr = new();

    public CommandGOTO()
    {
    }
    public bool Parse(TokenConsumer tokens)
    {
        expr.Parse(tokens);
        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue lineNumber = expr.Execute(vars);
        return lineNumber;
        /*
        //don't change the BasicValue var.
        vars.GetValue("_linenumber").SetValue(lineNumber.GetValueAsNumber());

        return null;
        */
    }

}