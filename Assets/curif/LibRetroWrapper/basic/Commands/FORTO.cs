using System;
using System.Collections.Generic;
using System.IO;

public class forToStorage
{
    public double lineNumber;
    public CommandExpression endExpr;
    public CommandExpression stepExpr;

}

class CommandFORTO : CommandBase
{

    CommandExpression expr;      // The initial value expression (e.g., A = 10)
    CommandExpression exprTo;    // The 'TO' expression (e.g., TO 100)
    CommandExpression exprStep;  // The 'STEP' expression (e.g., STEP 5), can be null
    string varName;
    // ConfigurationCommands config is now inherited from CommandBase

    // Constructor now calls the base constructor
    public CommandFORTO(ConfigurationCommands config) : base(config)
    {
        // Set the command token for this specific command via the base class's protected field.
        this.cmdToken = "FOR";

        expr = new(config);
        exprTo = new(config);
        exprStep = null; // Initialize as null, will be set if STEP is present
    }

    public override bool Parse(TokenConsumer tokens)
    {
        if (!BasicVar.IsVariable(tokens.Token))
            throw new Exception($"{tokens.Token} isn't a valid variable (FOR)");

        varName = tokens.Token;
        if (tokens.TheNextIs("["))
            throw new Exception($"malformed FOR/TO: arrays aren't allowed as FOR variables var: {varName.ToString()}");

        if (tokens.Next("=") == null)
            throw new Exception($"malformed FOR/TO missing [=] var: {varName.ToString()}");

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


    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC  #{config.LineNumber} {CmdToken}]");

        BasicValue startVal = expr.Execute(vars);
        FunctionHelper.ExpectedNumber(startVal, "- FOR must be an expression or number");
        vars.DeclareNewVariable(varName);
        vars.SetValue(varName, startVal);

        forToStorage ft = new();
        ft.lineNumber = config.LineNumber;
        ft.endExpr = exprTo;
        ft.stepExpr = exprStep;

        AGEBasicDebug.WriteConsole($"[AGE BASIC {CmdToken}] var:{varName} from {startVal.ToString()} expr:({expr.ToString()}) to expr: ({exprTo.ToString()})  lineNumber:{ft.lineNumber}");
        config.ForToNext[varName] = ft;

        return null;
    }
}