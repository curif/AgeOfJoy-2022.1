using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

class CommandFunctionBase : ICommandBase
{
    protected string cmdToken = "UNKNOWN";
    public string CmdToken { get { return cmdToken; } }
    public CommandType.Type Type { get; } = CommandType.Type.Function;

    protected CommandExpressionList exprs;

    protected ConfigurationCommands config;

    public CommandFunctionBase(ConfigurationCommands config)
    {
        this.config = config;
        exprs = new(config);
    }
    
    public virtual bool Parse(TokenConsumer tokens)
    {
        // FNCT ( expr ,  ... )
        // tokens points to FNCT

        ConfigManager.WriteConsole($"[functionBase.Parse] START  {tokens.ToString()}");

        if (tokens.Next() != "(")
            throw new Exception($"function without enclosing (): {tokens.ToString()}");
        tokens++; //consumes (

        ConfigManager.WriteConsole($"[FunctionBase.Parse] EXPR LIST {tokens.ToString()}");
        exprs.Parse(tokens);

        if (tokens.Token != ")")
            throw new Exception($"function without enclosing () END is missing: {tokens.ToString()}");
        //tokens++; //consumes )
        //always ends in the final token.

        ConfigManager.WriteConsole($"[functionBase.Parse] END {tokens.ToString()}");
        return true;
    }

    public virtual BasicValue Execute(BasicVars vars)
    {
        throw new Exception("function without execution method");
    }

    public override string ToString()
    {
        return "Func: " + CmdToken;
    }

}