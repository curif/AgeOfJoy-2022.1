using System;
using System.IO;
using UnityEngine;


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
    public static bool ExpectedNonEmptyArray(BasicValue val, string msg = "")
    {
        if (!val.IsArray() || val.GetArrayLength() == 0)
            throw new Exception("Parameter should be a non empty array " + msg);
        return true;
    }

    public static string FileTraversalFree(string path, string allowedBasePath)
    {
        string normalizedPath = Path.GetFullPath(path);
        if (!normalizedPath.StartsWith(allowedBasePath))
            throw new Exception($"Invalid file path: {path} Traversal back to parent directories is not allowed. Mininimal parent: {allowedBasePath}");
        return normalizedPath;
    }
    public static void ExpectedAtLeast(BasicValue[] vals, int count)
    {
        if (vals.Length < count)
            throw new Exception($"At least {count} values are expected.");

    }


    /// <summary>
    /// Tries to parse a color from the provided BasicValue arguments, starting at a given index.
    /// The color can be specified as a string name (1 argument) or as R, G, B numeric values (3 arguments).
    /// </summary>
    /// <param name="args">The array of BasicValue arguments. Assumed that elements are non-null if the array index is valid.</param>
    /// <param name="startIndex">The index in 'args' from which to start parsing the color.</param>
    /// <param name="config">The command configuration, used to access ScreenGenerator for color space.</param>
    /// <param name="parsedColor">Output: The parsed Color32 if successful.</param>
    /// <param name="consumedArgs">Output: The number of arguments consumed (1 for name, 3 for R,G,B, or 0 if parsing failed).</param>
    /// <returns>True if a color was successfully parsed, false otherwise.</returns>
    public static bool TryParseColor(BasicValue[] args, int startIndex, ConfigurationCommands config,
                                     out Color32 parsedColor, out int consumedArgs)
    {
        parsedColor = default; // Default to transparent black or similar
        consumedArgs = 0;

        if (args == null || startIndex >= args.Length)
        {
            return false; // Not enough arguments to even start
        }

        // Argument at startIndex is guaranteed non-null if args.Length > startIndex based on our last discussion.

        // Check if the argument at startIndex is a string (potential color name)
        if (args[startIndex].IsString())
        {
            if (config?.ScreenGenerator?.GetColorSpace() != null)
            {
                try
                {
                    // It's crucial that GetColorByName throws an exception or returns a clear error
                    // if the name is not found, rather than a default color, so we know it was an invalid name.
                    parsedColor = config.ScreenGenerator.GetColorSpace().GetColorByName(args[startIndex].GetString());
                    consumedArgs = 1;
                    return true;
                }
                catch (Exception) // Catches if GetColorByName throws for an unknown name
                {
                    // Color name not found in the current color space, or other error from GetColorByName.
                    // This string was not a valid color name.
                    return false;
                }
            }
            else
            {
                // ScreenGenerator or ColorSpace not available, cannot parse by name.
                // This is an environmental issue rather than a parsing failure of the value itself.
                // Depending on desired strictness, could log an error or throw.
                // For TryParse pattern, returning false is typical.
                AGEBasicDebug.WriteConsole($"[TryParseColor WARNING] ScreenGenerator or ColorSpace not available for parsing color name '{args[startIndex].GetString()}'.");
                return false;
            }
        }
        // Check if arguments from startIndex are R, G, B (3 numeric values)
        else if (args[startIndex].IsNumber() && // First part is a number
                 args.Length >= startIndex + 3 && // Enough args for R,G,B
                 args[startIndex + 1].IsNumber() &&
                 args[startIndex + 2].IsNumber())
        {
            try
            {
                byte r = (byte)Mathf.Clamp(args[startIndex].GetInt(), 0, 255);
                byte g = (byte)Mathf.Clamp(args[startIndex + 1].GetInt(), 0, 255);
                byte b = (byte)Mathf.Clamp(args[startIndex + 2].GetInt(), 0, 255);
                parsedColor = new Color32(r, g, b, 255); // Assuming full alpha for R,G,B spec
                consumedArgs = 3;
                return true;
            }
            catch (Exception ex)
            {
                // Error during GetInt() or conversion
                AGEBasicDebug.WriteConsole($"[TryParseColor ERROR] Error converting R,G,B values to color: {ex.Message}");
                return false;
            }
        }

        // If none of the above matched, it's not a recognized color specification starting at startIndex.
        return false;
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