using System;
using System.Collections.Generic;
using System.IO;

class CommandGOSUB : ICommandBase
{
    public string CmdToken { get; } = "GOSUB";
    public CommandType.Type Type { get; } = CommandType.Type.Command;
    public ConfigurationCommands Config { get; set;}

    CommandExpression expr;

    ConfigurationCommands config;

    public CommandGOSUB(ConfigurationCommands config)
    {
        this.config = config;
        expr = new(config);
    }

    public bool Parse(TokenConsumer tokens)
    {
        expr.Parse(tokens);
        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue lineNumberToJump = expr.Execute(vars);
        BasicValue actualLineNumber = vars.GetValue("_linenumber");

        BasicValue gosubstack = vars.GetValue("_gosubstack");
        string stack = gosubstack.GetValueAsString();
        if (stack.Length > 1024)
            throw new Exception("GOSUB stack overflow");
        
        string sep = string.IsNullOrEmpty(stack)? "" : ",";
        stack += sep + actualLineNumber.GetValueAsNumber().ToString();
        gosubstack.SetValue(stack);
        
        ConfigManager.WriteConsole($"[CommandGosub] new stack: {stack}");

        return lineNumberToJump;
        /*
        vars.GetValue("_linenumber").SetValue(lineNumberToJump.GetValueAsNumber());
        return null;
        */
    }

}