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
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN #{config.LineNumber}  {CmdToken}] [{expr}] ");

        config.Gosub.Push(config.LineNumber);

        BasicValue lineNumber = expr.Execute(vars);
        config.JumpTo = lineNumber.GetValueAsNumber();

        return null;
    }

}