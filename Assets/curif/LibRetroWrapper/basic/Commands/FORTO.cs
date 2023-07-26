using System;
using System.Collections.Generic;
using System.IO;

public class forToStorage
{
    public int lineNumber;
    public CommandExpression endExpr;
    public CommandExpression stepExpr;

}

class CommandFORTO : ICommandBase
{
    public string CmdToken { get; } = "FOR";
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    CommandExpression expr;
    CommandExpression exprTo;
    CommandExpression exprStep;
    BasicVar var;

    ConfigurationCommands config;
    public CommandFORTO(ConfigurationCommands config)
    {
        this.config = config;
        expr = new(config);
        exprTo = new(config);
        exprStep = null;
    }

    public bool Parse(TokenConsumer tokens)
    {
        if (!BasicVar.IsVariable(tokens.Token))
            throw new Exception($"{tokens.Token} isn't a valid variable (FOR)");

        var = new(tokens.Token);

        if (tokens.Next("=") == null)
            throw new Exception($"malformed FOR/TO missing [=] var: {var.ToString()}");

        tokens++;
        expr.Parse(tokens);

        if (tokens.Token.ToUpper() != "TO")
            throw new Exception($"malformed FOR/TO, TO is missing");

        tokens++;
        exprTo.Parse(tokens);

        if (tokens.Token.ToUpper() == "STEP")
        {
            tokens++;
            exprStep = new(config);
            exprStep.Parse(tokens);
        }

        return true;
    }


    public BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC {CmdToken}]");

        BasicValue startVal = new(expr.Execute(vars));
        FunctionHelper.ExpectedNumber(startVal, "- FOR must have an expression or number");

        vars.SetValue(var, startVal);

        forToStorage ft = new();
        ft.lineNumber = config.LineNumber;
        ft.endExpr = exprTo;
        ft.stepExpr = exprStep;

        ConfigManager.WriteConsole($"[AGE BASIC {CmdToken}] var:{var.Name} from {startVal.ToString()} expr:({expr.ToString()}) to expr: ({exprTo.ToString()})  lineNumber:{ft.lineNumber}");
        config.ForToNext[var.Name] = ft;

        return null;
    }
}