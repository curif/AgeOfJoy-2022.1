using System;
using System.Collections.Generic;
using System.IO;

class CommandCALL : ICommandBase
{
    public string CmdToken { get; } = "CALL";
    public CommandType.Type Type { get; } = CommandType.Type.Command;
    public ConfigurationCommands Config { get; set; }

    CommandExpression expr;

    ConfigurationCommands config;

    public CommandCALL(ConfigurationCommands config)
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

        expr.Execute(vars);
        return null;
    }

}