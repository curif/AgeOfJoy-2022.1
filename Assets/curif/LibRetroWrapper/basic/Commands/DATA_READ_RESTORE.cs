
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

        string storageName = vals[0].GetString();
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

    ConfigurationCommands config;
    public CommandREAD(ConfigurationCommands config)
    {
        this.config = config;
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

            varList[idx] = BasicVar.Parse(tokens, config);
            tokens++; //consume varname or ']'

            idx++;
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

        // process the list of vars
        foreach (BasicVar var in varList)
        {
            if (var == null || l.EOF())
                break;

            BasicValue v = l.CurrentValue();
            if (v == null)
                break;

            vars[var] = new(v);

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
        if (exprs.Count < 1)
            throw new Exception($"{CmdToken} RESTORE parameters missing, example RESTORE 'storage name'[, 10]");
        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");

        BasicValue[] vals;

        vals = exprs.ExecuteList(vars);
        string storageName;
        int offset = 0;
        FunctionHelper.ExpectedString(vals[0], " - storage name");
        storageName = vals[0].GetString();
        if (!this.config.basicValueLists.ContainsKey(storageName))
            throw new Exception($"{CmdToken} RESTORE 'storage name' doesn't exists");

        if (vals.Length > 1)
        {
            FunctionHelper.ExpectedNumber(vals[1], " - offset");
            offset = vals[1].GetInt();
        }

        config.basicValueLists[storageName].JumpTo(offset);
        
        return null;
    }
}
