using System;
using System.Collections.Generic;
using System.IO;

class CommandGOSUB : ICommandBase
{
    public string CmdToken { get; } = "GOSUB";
    public CommandType.Type Type { get; } = CommandType.Type.Command;
    public ConfigurationCommands Config { get; set; }

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

        config.Gosub.Push(config.LineNumber);

        BasicValue lineNumber = expr.Execute(vars);
        if (!lineNumber.IsNumber())
            throw new Exception($"GOSUB accepts number expression only");

        config.JumpTo = (int)lineNumber.GetValueAsNumber();

        return null;
    }

}