using System;
using System.Collections.Generic;
using System.IO;

class CommandIFTHEN : ICommandBase
{
    public string CmdToken { get; } = "IFTHEN";
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    CommandExpression expr;
    ICommandBase cmd;
    ConfigurationCommands config;
    public CommandIFTHEN(ConfigurationCommands config)
    {
        this.config = config;
        expr = new(config);
    }

    public bool Parse(TokenConsumer tokens)
    {
        // if (tokens.Token != "(")
        //     throw new Exception($"malformed IF/THEN, expression should be enclosed by ()");

        expr.Parse(tokens);
        
        if (tokens.Token.ToUpper() != "THEN")
            throw new Exception($"malformed IF/THEN, THEN is missing");

        cmd = Commands.GetNew(tokens.Next(), config);
        if (cmd == null)
            throw new Exception($"Syntax error command not found in THEN clause: {tokens.ToString()}");
        cmd.Parse(++tokens);

        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        BasicValue condition = expr.Execute(vars);
        if (condition.IsTrue())
            return cmd.Execute(vars);

        return null;
    }

}