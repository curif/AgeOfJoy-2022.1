#define AGEBASIC_DEBUG_ACTIVE

using System;

public interface ICommandBase
{
    string CmdToken { get; }
    CommandType.Type Type { get; }

    bool Parse(TokenConsumer tokens);
    void CheckConfigRequirements(ConfigurationCommands config) { }
    BasicValue Execute(BasicVars vars);
}

class CommandBase : ICommandBase
{
    protected string cmdToken = "UNKNOWN";
    public string CmdToken { get { return cmdToken; } }
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    protected ConfigurationCommands config;

    public CommandBase(ConfigurationCommands config)
    {
        this.config = config;
    }

    public virtual bool Parse(TokenConsumer tokens)
    {
        throw new Exception("function without parse method");
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

class CommandExpressionListBase : CommandBase
{
    protected CommandExpressionList exprs;
    protected int MinCantParamsRequired = 1;

    public CommandExpressionListBase(ConfigurationCommands config) : base(config)
    {
        exprs = new(config);
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, MinCantParamsRequired);
    }
    public bool Parse(TokenConsumer tokens, int cantParametersRequired)
    {
        // FNCT ( expr ,  ... )
        // tokens points to FNCT

        AGEBasicDebug.WriteConsole($" EXPR LIST {tokens}");
        exprs.Parse(tokens);

        if (exprs.Count < cantParametersRequired)
            throw new Exception($"{cmdToken}() parameter missing, {cantParametersRequired} expected.");

        AGEBasicDebug.WriteConsole($"[functionBase.Parse] END {tokens}");
        return true;
    }

}


class CommandSingleExpressionBase : CommandBase
{
    protected CommandExpression expr;

    public CommandSingleExpressionBase(ConfigurationCommands config) : base(config)
    {
        expr = new(config);
    }

    public override bool Parse(TokenConsumer tokens)
    {
        // FNCT ( expr ,  ... )
        // tokens points to FNCT

        AGEBasicDebug.WriteConsole($"[CommandFunctionSingleExpressionBase.Parse] START  {tokens.ToString()}");

        //AGEBasicDebug.WriteConsole($"[CommandFunctionSingleExpressionBase.Parse] EXPR {tokens.ToString()}");
        expr.Parse(tokens);

        if (expr.Count < 1)
            throw new Exception($"At least one parameter is required in {CmdToken}");

        //AGEBasicDebug.WriteConsole($"[CommandFunctionSingleExpressionBase.Parse] END {tokens.ToString()}");
        return true;
    }
}

class CommandNoExpressionBase : CommandBase
{
    public CommandNoExpressionBase(ConfigurationCommands config) : base(config)
    {
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return true;
    }
}

public interface ICommandList
{
    BasicValue[] ExecuteList(BasicVars vars);
}

public interface ICommandFunctionList
{
    string CmdToken { get; }
    CommandType.Type Type { get; }

    bool Parse(TokenConsumer tokens);
    int MaxAllowed { get; }

    BasicValue[] ExecuteList(BasicVars vars);
}


public class CommandType
{
    public enum Type
    {
        Command,
        Variable,
        Constant,
        Function,
        Expression,
        ExpressionList,
        Operation,
        Unknown
    }

    public static Type TokenType(TokenConsumer tokens)
    {
        // the order is important
        if (Commands.IsCommand(tokens.Token))
            return Type.Command;
        if (Commands.IsFunction(tokens.Token))
            return Type.Function;
        else if (BasicValue.IsValidOperation(tokens.Token))
            return Type.Operation;
        else if (tokens.Token == "(")
            return Type.Expression;
        else if (BasicValue.IsValidNumber(tokens.Token))
            return Type.Constant;
        else if (BasicValue.IsValidString(tokens.Token))
            return Type.Constant;
        else if (BasicVar.IsVariable(tokens.Token))
            return Type.Variable;

        return Type.Constant;
        // return Type.Unknown;
    }
}

public static class AGEBasicDebug
{
    public static void WriteConsole(string st)
    {
#if AGEBASIC_DEBUG_ACTIVE
        ConfigManager.WriteConsole(st);
#endif
    }
}