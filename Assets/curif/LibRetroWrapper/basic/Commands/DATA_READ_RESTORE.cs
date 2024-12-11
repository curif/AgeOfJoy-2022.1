
using System;

class CommandDATA : ICommandBase
{
    public string CmdToken { get; } = "DATA";
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    CommandExpressionList exprs;
    ConfigurationCommands config;

    public CommandDATA(ConfigurationCommands config)
    {
        this.config = config;
        exprs = new(config);
    }

    public bool Parse(TokenConsumer tokens)
    {
        exprs.Parse(tokens);
        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");

        BasicValue[] vals;

        vals = exprs.ExecuteList(vars);

        if (vals.Length < 2)
            throw new Exception($"{CmdToken} DATA parameters missing, example DATA 'storage name', 10, 20, 30");
        FunctionHelper.ExpectedString(vals[0], " - storage name");

        string storageName = vals[0].ToString();
        if (!this.config.basicValueLists.ContainsKey(storageName))
        {
            // If it doesn't exist, create a new BasicValueList object and add it to the dictionary.
            this.config.basicValueLists[storageName] = new BasicValueList();
        }
        for (int i = 1; i < exprs.Count; i++)
        {
            this.config.basicValueLists[storageName].Add(vals[i]);
        }

        return null;
    }
}


class CommandREAD : ICommandBase
{
    public string CmdToken { get; } = "READ";
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    BasicVar[] varList = new BasicVar[30];
    int count;
    CommandExpression storageNameExpr;

    CommandExpressionList exprs;
    ConfigurationCommands config;
    public CommandREAD(ConfigurationCommands config)
    {
        this.config = config;
        exprs = new(config);
        storageNameExpr = new(config);
    }

    public bool Parse(TokenConsumer tokens)
    {
        // READ "storage", var , var , var , ...

        storageNameExpr.Parse(tokens);
        if (tokens.Token != ",")
            throw new Exception($"{tokens.Token} READ needs a list of variables");

        int idx = 0;
        do
        {
            tokens.ConsumeIf(",");
            if (tokens.Remains() < 0)
                break;

            if (idx + 1 > 30)
                throw new Exception($"{CmdToken} - More than 30 members in an variable list isn't allowed {tokens.ToString()}");

            if (!BasicVar.IsVariable(tokens.Token))
                throw new Exception($"{tokens.Token} isn't a valid variable name (READ)");

            AGEBasicDebug.WriteConsole($"[READ.Parse] parsing  {tokens.ToString()}");

            varList[idx] = new(tokens.Token);
            idx++;
            tokens++;
        }
        while (tokens.Token == "," || tokens.Remains() < 0);

        count = idx;
        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC {CmdToken}] {count} variables assignment");
        
        BasicValue storage = storageNameExpr.Execute(vars);
        FunctionHelper.ExpectedString(storage, "- storage name should be a string");

        string st = storage.GetString();
        if (!config.basicValueLists.ContainsKey(st))
            throw new Exception($"READ refers to a storage {st} who don't exists");

        BasicValueList l = config.basicValueLists[st];
        //l.Reset();

        // process the list of vars
        foreach (BasicVar var in varList)
        {
            if (var == null || l.EOF())
                break;
            BasicValue v = l.CurrentValue();
            if (v == null)
                break;
            vars.SetValue(var, v);

            l.Next();
        }

        return null;
    }
}


class CommandRESTORE : ICommandBase
{
    public string CmdToken { get; } = "RESTORE";
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    CommandExpressionList exprs;
    ConfigurationCommands config;

    public CommandRESTORE(ConfigurationCommands config)
    {
        this.config = config;
        exprs = new(config);
    }

    public bool Parse(TokenConsumer tokens)
    {
        exprs.Parse(tokens);
        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");

        BasicValue[] vals;

        vals = exprs.ExecuteList(vars);

        if (vals.Length < 2)
            throw new Exception($"{CmdToken} RESTORE parameters missing, example RESTORE 'storage name', 10");
        FunctionHelper.ExpectedString(vals[0], " - storage name");
        FunctionHelper.ExpectedNumber(vals[1], " - offset");

        string storageName = vals[0].ToString();
        if (!this.config.basicValueLists.ContainsKey(storageName))
            throw new Exception($"{CmdToken} RESTORE 'storage name' doesn't exists");

        int offset = (int) vals[1].GetNumber();

        BasicValueList l = config.basicValueLists[storageName];
        l.JumpTo(offset);

        return null;
    }
}
