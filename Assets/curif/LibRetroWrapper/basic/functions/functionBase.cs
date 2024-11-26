using System;
using System.IO;


class CommandFunctionBase : ICommandBase
{
    protected string cmdToken = "UNKNOWN";
    public string CmdToken { get { return cmdToken; } }
    public CommandType.Type Type { get; } = CommandType.Type.Function;

    protected ConfigurationCommands config;

    public CommandFunctionBase(ConfigurationCommands config)
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

class CommandFunctionExpressionListBase : CommandFunctionBase
{
    protected CommandExpressionList exprs;

    public CommandFunctionExpressionListBase(ConfigurationCommands config) : base(config)
    {
        exprs = new(config);
    }

    public bool Parse(TokenConsumer tokens, int cantParametersRequired)
    {
        // FNCT ( expr ,  ... )
        // tokens points to FNCT

        AGEBasicDebug.WriteConsole($"[functionBase.Parse] START  {tokens.ToString()}");

        if (tokens.Next() != "(")
            throw new Exception($"function without enclosing (): {tokens.ToString()}");
        tokens++; //consumes (

        AGEBasicDebug.WriteConsole($"[FunctionBase.Parse] EXPR LIST {tokens.ToString()}");
        exprs.Parse(tokens);

        if (tokens.Token != ")")
            throw new Exception($"function without enclosing () END is missing: {tokens.ToString()}");
        //tokens++; //consumes )
        //always ends in the final token.

        if (exprs.Count < cantParametersRequired)
            throw new Exception($"{cmdToken}() parameter missing, {cantParametersRequired} expected.");

        AGEBasicDebug.WriteConsole($"[functionBase.Parse] END {tokens.ToString()}");
        return true;
    }

}

class CommandFunctionSingleExpressionBase : CommandFunctionBase
{
    protected CommandExpression expr;

    public CommandFunctionSingleExpressionBase(ConfigurationCommands config) : base(config)
    {
        expr = new(config);
    }

    public override bool Parse(TokenConsumer tokens)
    {
        // FNCT ( expr ,  ... )
        // tokens points to FNCT

        AGEBasicDebug.WriteConsole($"[CommandFunctionSingleExpressionBase.Parse] START  {tokens.ToString()}");

        if (tokens.Next() != "(")
            throw new Exception($"function without enclosing (): {tokens.ToString()}");
        tokens++; //consumes (

        //AGEBasicDebug.WriteConsole($"[CommandFunctionSingleExpressionBase.Parse] EXPR {tokens.ToString()}");
        expr.Parse(tokens);

        if (tokens.Token != ")")
            throw new Exception($"function without enclosing () END is missing: {tokens.ToString()}");
        //tokens++; //consumes )
        //always ends in the final token.

        if (expr.Count < 1)
            throw new Exception($"At least one parameter is required in {CmdToken}");

        //AGEBasicDebug.WriteConsole($"[CommandFunctionSingleExpressionBase.Parse] END {tokens.ToString()}");
        return true;
    }
}

class CommandFunctionNoExpressionBase : CommandFunctionSingleExpressionBase
{

    public CommandFunctionNoExpressionBase(ConfigurationCommands config) : base(config)
    {
    }

    public override bool Parse(TokenConsumer tokens)
    {
        if (tokens.Next() != "(")
            throw new Exception($"function without enclosing (): {tokens.ToString()}");
        tokens++; //consumes (

        if (tokens.Token != ")")
            throw new Exception($"function without enclosing () END is missing: {tokens.ToString()}");

        return true;
    }
}

public static class FunctionHelper
{
    public static bool ExpectedNumber(BasicValue val, string msg = "")
    {
        if (!val.IsNumber())
            throw new Exception("Parameter should be a number " + msg);
        return true;
    }
    public static bool ExpectedNotNull(BasicValue val, string msg = "")
    {
        if (val == null)
            throw new Exception("Missing parameter " + msg);
        return true;
    }
    public static bool ExpectedString(BasicValue val, string msg = "")
    {
        if (!val.IsString())
            throw new Exception("Parameter should be a string " + msg);
        return true;
    }
    public static bool ExpectedNonEmptyString(BasicValue val, string msg = "")
    {
        if (!val.IsString() || string.IsNullOrEmpty(val.GetString()))
            throw new Exception("Parameter should be a non empty string " + msg);
        return true;
    }

    public static string FileTraversalFree(string path, string allowedBasePath)
    {
        string normalizedPath = Path.GetFullPath(path);
        if (!normalizedPath.StartsWith(allowedBasePath))
            throw new Exception($"Invalid file path: {path} Traversal back to parent directories is not allowed.");
        return normalizedPath;
    }
    public static void ExpectedAtLeast(BasicValue[] vals, int count)
    {
        if (vals.Length < count)
            throw new Exception($"At least {count} values are expected.");

    }


    public static int GetValsCount(BasicValue[] vals)
    {
        int loadedCount = 0;
        foreach (BasicValue val in vals)
        {
            if (val != null)
                loadedCount++;
            else
                break; // Stop counting when encountering null
        }
        return loadedCount;
    }

    public static int HexStringToDecimal(string hex)
    {
        // Remove "0x" prefix if it exists
        if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            hex = hex.Substring(2);
        }

        // Convert the hex string to an integer
        return int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
    }

}