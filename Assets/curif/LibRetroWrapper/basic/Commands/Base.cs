using System;
using System.Collections.Generic;
using System.IO;

public interface ICommandBase
{
    string CmdToken { get; }
    CommandType.Type Type { get; }

    //returns how many consumes
    bool Parse(TokenConsumer tokens);
    BasicValue Execute(BasicVars vars);
}

public interface ICommandList
{

    int MaxAllowed { get; }

    BasicValue[] ExecuteList(BasicVars vars);
}

public interface ICommandFunctionList
{
    string CmdToken { get; }
    CommandType.Type Type { get; }

    //returns how many consumes
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