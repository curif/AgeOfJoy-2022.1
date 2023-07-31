using System;
using System.Collections.Generic;
using System.IO;

class CommandLET : ICommandBase
{
    public string CmdToken { get; } = "LET";
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    BasicVar var;
    CommandExpression expr;
    ConfigurationCommands config;
    public CommandLET(ConfigurationCommands config)
    {
        this.config = config;
        expr = new(config);
    }

    public bool Parse(TokenConsumer tokens)
    {
        // LET var = expr
        if (!BasicVar.IsVariable(tokens.Token))
            throw new Exception($"{tokens.Token} isn't a valid variable name (LET)");

        var = new(tokens.Token);

        if (tokens.Next("=") == null)
            throw new Exception($"malformed LET missing [=] var: {var.ToString()}");

        tokens++;
        expr.Parse(tokens);

        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        vars.SetValue(var, expr.Execute(vars));
        ConfigManager.WriteConsole($"[AGE BASIC {CmdToken}] []");
        return null;
    }

}