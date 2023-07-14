using System;
using System.Collections.Generic;
using System.IO;

class CommandExpressionList : ICommandBase, ICommandList
{
    public string CmdToken { get; } = "EXPRLIST";

    public int MaxAllowed { get { return maxAllowed; } }

    public CommandType.Type Type { get; } = CommandType.Type.ExpressionList;

    const int maxAllowed = 15;
    CommandExpression[] exprs = new CommandExpression[15];
    public int Count = 0;

    public CommandExpressionList()
    {
    }

    public bool Parse(TokenConsumer tokens)
    {
        ConfigManager.WriteConsole($"[ExpressionList.Parse] START  {tokens.ToString()}");

        int idx = -1;
        do
        {
            tokens.ConsumeIf(",");

            if (idx + 1 > maxAllowed - 1)
                throw new Exception($"More than {maxAllowed} members in an expressionList {tokens.ToString()}");

            ConfigManager.WriteConsole($"[ExpressionList.Parse] parsing  {tokens.ToString()}");
            CommandExpression expr = new();
            expr.Parse(tokens);

            idx++;
            exprs[idx] = expr;
        }
        while (tokens.Token == ",");
        
        this.Count = idx + 1;

        ConfigManager.WriteConsole($"[ExpressionList.Parse] END members: {this.Count} {tokens.ToString()}");

        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        throw new Exception($"can't exec a expr list");
    }

    public BasicValue[] ExecuteList(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC {CmdToken}] ");
        BasicValue[] vals = new BasicValue[exprs.Length];
        int idx = 0;
        while (exprs[idx] != null)
        {
            ConfigManager.WriteConsole($"[AGE BASIC {CmdToken}] execute expression idx: {idx}");
            vals[idx] = exprs[idx].Execute(vars);
            idx++;
        }
        return vals;
    }

}