using System;
using System.Collections.Generic;
using System.IO;

class CommandLETS : ICommandBase
{
    public string CmdToken { get; } = "LETS";
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    BasicVar[] var = new BasicVar[30];
    int count;

    CommandExpressionList exprs;
    ICommandFunctionList fnct;
    ConfigurationCommands config;
    public CommandLETS(ConfigurationCommands config)
    {
        this.config = config;
        exprs = new(config);
    }

    public bool Parse(TokenConsumer tokens)
    {
        // LET var , var , var , ... = exprList , ...

        int idx = 0;
        while (idx < 30)
        {
            if (!BasicVar.IsVariable(tokens.Token))
                throw new Exception($"{tokens.Token} isn't a valid variable name (LETS)");

            var[idx] = new(tokens.Token);
            idx++;
            tokens++;

            if (tokens.Token == "=")
            {
                tokens++;
                break;
            }

            if (tokens.Token != ",")
                throw new Exception($"malformed, a ',' or '=' is expected, get {tokens.Token}");

            tokens++;
        }

        if (idx == 30)
            throw new Exception($"LETS admits 30 variables or less");

        count = idx;

        //could be an expression list or a function that returns 
        // a list of values
        fnct = Commands.GetNew(tokens.Token, config) as ICommandFunctionList;
        ConfigManager.WriteConsole($"Parse LETS: is function? {fnct != null} {tokens.ToString()}");
        if (fnct != null)
            fnct.Parse(tokens);
        else
            exprs.Parse(tokens);

        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC {CmdToken}] {count} variables assignment");
        BasicValue[] vals;
        if (fnct == null)
            vals = exprs.ExecuteList(vars);
        else
            vals = fnct.ExecuteList(vars);
        
        if (vals.Length < count)
            throw new Exception($"malformed LETS, all variables must get a value ({vals.Length} <> {count})");
        
        for (int idx = 0; idx < count; idx++)
        {
            vars.SetValue(var[idx], vals[idx]);
        }
        return null;
    }
}