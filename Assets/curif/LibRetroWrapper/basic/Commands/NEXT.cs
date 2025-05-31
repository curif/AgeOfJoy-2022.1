using System;
// Changed from implementing ICommandBase to inheriting from CommandBase
class CommandNEXT : CommandBase
{
    string varName; // The name of the loop variable (e.g., "I" in NEXT I)

    public CommandNEXT(ConfigurationCommands config) : base(config)
    {
        // Set the command token for this specific command via the base class's protected field.
        this.cmdToken = "NEXT";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        if (!BasicVar.IsVariable(tokens.Token))
            throw new Exception($"{tokens.Token} isn't a valid variable (FOR)");

        varName = new(tokens.Token);

        return true;
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC  #{config.LineNumber}  {CmdToken}]");

        vars.ThrowIfNotExists(varName, $" { CmdToken} #{config.LineNumber}]");
        
        if (!config.ForToNext.ContainsKey(varName))
            throw new Exception($"NEXT without FOR: {varName}");

        forToStorage ft = config.ForToNext[varName];

        BasicValue endValue = ft.endExpr.Execute(vars);
        FunctionHelper.ExpectedNumber(endValue, "- TO must compute to expression or number");

        BasicValue actualValue = vars.GetValue(varName);
        FunctionHelper.ExpectedNumber(actualValue, "- FOR must compute to expression or number");

        if (ft.stepExpr != null)
        {
            BasicValue step = ft.stepExpr.Execute(vars);
            FunctionHelper.ExpectedNumber(step, "- STEP must compute to expression or number");
            actualValue.Add(step);
        }
        else
        {
            actualValue.Increment();
        }
        

        AGEBasicDebug.WriteConsole($"[AGE BASIC {CmdToken} #{config.LineNumber}] var:{varName}: {actualValue} to {endValue} step {ft.stepExpr}");

        if (actualValue > endValue)
            config.ForToNext.Remove(varName);
        else
            config.JumpNextTo = ft.lineNumber;

        return null;
    }
}