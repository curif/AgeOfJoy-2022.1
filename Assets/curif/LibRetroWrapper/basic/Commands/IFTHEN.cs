using System;
using System.Collections.Generic;
using System.IO;

class CommandIFTHEN : ICommandBase
{
    public string CmdToken { get; } = "IFTHEN";
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    CommandExpression expr;

    ICommandBase cmd;
    ICommandBase cmdElse;
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
        
        tokens++;
        cmd.Parse(tokens);

        if (tokens.Token.ToUpper() == "ELSE")
        {
            cmdElse = Commands.GetNew(tokens.Next(), config);
            if (cmd == null)
                throw new Exception($"Syntax error command not found in ELSE clause: {tokens.ToString()}");
            tokens++;
            cmdElse.Parse(tokens);
        }

        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        BasicValue condition = expr.Execute(vars);
        if (condition.IsTrue())
            return cmd.Execute(vars);

        if (cmdElse != null)
            return cmdElse.Execute(vars);

        return null;
    }

}