
using System;
class CommandDATA : CommandExpressionListBase // Changed base class
{
    public CommandDATA(ConfigurationCommands config) : base(config) // Call base constructor
    {
        this.cmdToken = "DATA"; // Set CmdToken
    }

    public override bool Parse(TokenConsumer tokens) // Override Parse method
    {
        return base.Parse(tokens, 2); // Call base to parse the list of expressions
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");

        // exprs is now a protected member from CommandExpressionListBase
        BasicValue[] vals = exprs.ExecuteList(vars);

        // The count check is now handled in Parse, but a runtime check for type/content is still good.
        FunctionHelper.ExpectedString(vals[0], " - storage name");

        string storageName = vals[0].GetString();
        if (!this.config.basicValueLists.ContainsKey(storageName))
        {
            this.config.basicValueLists[storageName] = new BasicValueList();
        }
        for (int i = 1; i < exprs.Count; i++) // Use exprs.Count, consistent with vals.Length
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



class CommandRESTORE : CommandExpressionListBase // Changed base class
{
    // config and exprs are now inherited from CommandExpressionListBase

    public CommandRESTORE(ConfigurationCommands config) : base(config) // Call base constructor
    {
        this.cmdToken = "RESTORE"; // Set CmdToken
    }

    public override bool Parse(TokenConsumer tokens) // Override Parse method
    {
        return base.Parse(tokens, 1);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);
        string storageName;
        int offset = 0;

        // The count check is done in Parse, so vals[0] should be safe to access.
        FunctionHelper.ExpectedString(vals[0], " - storage name");
        storageName = vals[0].GetString();

        if (!this.config.basicValueLists.ContainsKey(storageName))
            throw new Exception($"{CmdToken} RESTORE '{storageName}' storage name doesn't exists"); 
        if (vals.Length > 1) // Use vals.Length as it reflects the number of evaluated expressions
        {
            FunctionHelper.ExpectedNumber(vals[1], " - offset");
            offset = vals[1].GetInt();
        }

        config.basicValueLists[storageName].JumpTo(offset);

        return null;
    }
}