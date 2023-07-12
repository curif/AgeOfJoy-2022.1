using System;
using System.Collections.Generic;
using System.IO;

class CommandIFTHEN : ICommandBase
{
    public string CmdToken { get; } = "IFTHEN";
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    CommandExpression expr = new();
    ICommandBase cmd;

    public CommandIFTHEN()
    {
    }

    public bool Parse(TokenConsumer tokens)
    {
        if (tokens.Token != "(")
            throw new Exception($"malformed IF/THEN, expression should be enclosed by ()");

        expr.Parse(++tokens);
        
        if (tokens.Next("THEN") == null)
            throw new Exception($"malformed IF/THEN, THEN is missing");

        cmd = Commands.GetNew(tokens.Next());
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