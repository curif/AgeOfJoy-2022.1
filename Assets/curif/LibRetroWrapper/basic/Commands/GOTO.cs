using System;
using System.Collections.Generic;
using System.IO;

class CommandGOTO : ICommandBase
{
    public string CmdToken { get; } = "GOTO";
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    CommandExpression expr;
    ConfigurationCommands config;
    public CommandGOTO(ConfigurationCommands config)
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

        BasicValue lineNumber = expr.Execute(vars);
        if (! lineNumber.IsNumber())
            throw new Exception($"GOTO accepts number expression only");

        config.JumpTo = (int) lineNumber.GetValueAsNumber(); 

        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] Jump to {config.JumpTo} ");

        return null;
    }

}