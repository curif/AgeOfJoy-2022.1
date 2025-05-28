using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Siccity.GLTFUtility.GLTFAccessor.Sparse;



/// <summary>
/// Handles the DIM command in AGEBasic, allowing declaration of single or multi-dimensional arrays.
/// Syntax: DIM var[size], DIM var[size1,size2], etc.
/// </summary>
class CommandDIM : ICommandBase
{
    public string CmdToken { get; } = "DIM";
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    ConfigurationCommands config;
    BasicVar arrayVar; // The BasicVar representing the array (e.g., MYARRAY)

    /// <summary>
    /// Constructor for CommandDIM.
    /// </summary>
    /// <param name="config">Runtime configuration, providing access to services like ScreenGenerator.</param>
    public CommandDIM(ConfigurationCommands config)
    {
        this.config = config;
    }

    /// <summary>
    /// Parses the DIM command syntax from the token stream.
    /// Expected format: DIM variableName[dim1, dim2, ...]
    /// </summary>
    /// <param name="tokens">The TokenConsumer instance holding the BASIC code tokens.</param>
    /// <returns>True if parsing is successful, otherwise throws an exception.</returns>
    public bool Parse(TokenConsumer tokens)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC PARSE {CmdToken} #{config.LineNumber}] Parsing DIM statement.");

        // 1. Expect and parse the variable name (e.g., MYARRAY)
        if (!BasicVar.IsVariable(tokens.Token))
        {
            throw new Exception($"[{CmdToken} ERROR #{config.LineNumber}] '{tokens.Token}' isn't a valid variable name for DIM. Expected a variable name.");
        }
        arrayVar = BasicVar.Parse(tokens, config);
        if (!arrayVar.IsArray())
            throw new Exception($"[{CmdToken} ERROR #{config.LineNumber}] '{tokens.Token}' DIM bad format. Expected array size.");

        AGEBasicDebug.WriteConsole($"[AGE BASIC PARSE {CmdToken} #{config.LineNumber}] Successfully parsed DIM for '{arrayVar.Name}' with {arrayVar.IndexExpressions.Count} dimension/s.");
        return true;
    }

    /// <summary>
    /// Executes the DIM command at runtime, creating and dimensioning the array.
    /// </summary>
    /// <param name="vars">The BasicVars instance holding program variables.</param>
    /// <returns>Null, as DIM is a command and does not return a value.</returns>
    public BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] Executing DIM for array '{arrayVar.Name}'.");
        BasicValue[] indices = arrayVar.IndexExpressions.ExecuteList(vars);
        vars.DeclareNewVariable(arrayVar.Name).BasicValue.Dim(indices);
        return null; // Commands return null
    }

}


/// <summary>
/// Handles the LET command in AGEBasic, allowing assignment to simple variables
/// or indexed array elements (e.g., LET A = 10, LET MYARRAY[3] = 20, LET MYMATRIX[1,2] = "hello").
/// </summary>
class CommandLET : ICommandBase
{
    public string CmdToken { get; } = "LET";
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    private readonly ConfigurationCommands config;
    private BasicVar var;
    CommandExpression assignmentExpression;

    public CommandLET(ConfigurationCommands config)
    {
        this.config = config;
        assignmentExpression = new(config); 
    }

    public bool Parse(TokenConsumer tokens)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC PARSE {CmdToken} #{config.LineNumber}] Parsing LET statement.");

        var = BasicVar.Parse(tokens, config);
        tokens++; //consume varname or ']'

        if (tokens.Token != "=")
        {
            throw new Exception($"[{CmdToken} ERROR #{config.LineNumber}] Malformed LET statement, missing '=' after variable declaration.");
        }
        tokens++; // Consume '='

        assignmentExpression.Parse(tokens);

        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] Executing LET for '{var}'.");

        // Evaluate the value to be assigned on the right-hand side
        BasicValue valueToAssign = assignmentExpression.Execute(vars);

        if (!var.IsArray())
        {
            // This is a simple variable assignment (e.g., A = 10)
            vars[var.Name] = valueToAssign; //it will be created if not exists
            return null;
        }
        if (!vars.Exists(var.Name))
            throw new Exception($"[{CmdToken} ERROR #{config.LineNumber}]: variable {var.Name} must be DIMensioned before assignment.");
        BasicValue[] indexes = var.IndexExpressions.ExecuteList(vars);
        vars[var.Name][indexes] = valueToAssign;

        return null; // LET is a command, and commands typically return null
    }
}

/// <summary>
/// Handles the LETS command, allowing multiple assignments to simple variables
/// or indexed array elements (e.g., LETS A, MYVAR[10,3], B = 10, "string", 0).
/// </summary>
class CommandLETS : ICommandBase
{
    public string CmdToken { get; } = "LETS";
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    private readonly ConfigurationCommands config;
    // This list will store all the left-hand side targets, now as BasicVar objects
    private List<BasicVar> _assignmentTargets;
    private CommandExpressionList _rhsExpressions; // For right-hand side expressions (e.g., LETS a,b=1,2)
    private ICommandFunctionList _rhsFunction;    // For right-hand side function call (e.g., LETS a,b=MYFUNC())

    public CommandLETS(ConfigurationCommands config)
    {
        this.config = config;
        _assignmentTargets = new List<BasicVar>();
        _rhsExpressions = new CommandExpressionList(config);
        _rhsFunction = null; // Will be set if the RHS is a function
    }

    public bool Parse(TokenConsumer tokens)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC PARSE {CmdToken} #{config.LineNumber}] Parsing LETS statement.");

        _assignmentTargets.Clear(); // Clear any previous parse state for reuse

        // Parse the left-hand side (LHS) targets: var1, var2[idx], ...
        int maxTargets = 30; // Limit for number of variables/elements in LETS (arbitrary limit)
        for (int i = 0; i < maxTargets; i++)
        {
            // Use the BasicVar.Parse method to get each individual assignment target on the LHS.
            // This method is assumed to handle variable name and optional array indexing.
            BasicVar currentTarget = BasicVar.Parse(tokens, config);
            tokens++; //consume varname or ']'
            _assignmentTargets.Add(currentTarget);


            // Check for '=' (end of LHS) or ',' (more LHS targets)
            if (tokens.Token == "=")
            {
                tokens++; // Consume '='
                break; // End of LHS, proceed to RHS parsing
            }

            if (tokens.Token != ",")
            {
                throw new Exception($"[{CmdToken} ERROR #{config.LineNumber}] Malformed LETS. Expected ',' or '=' after variable '{currentTarget.Name}'.");
            }
            tokens++; // Consume ',' and continue to parse the next LHS target
        }

        // If the loop finished due to maxTargets and we didn't find '=', it's an error
        if (_assignmentTargets.Count == maxTargets && tokens.Token != "=")
        {
            throw new Exception($"[{CmdToken} ERROR #{config.LineNumber}] LETS command admits a maximum of {maxTargets} assignments on the left-hand side.");
        }
        if (_assignmentTargets.Count == 0) // Ensure at least one target was parsed
        {
            throw new Exception($"[{CmdToken} ERROR #{config.LineNumber}] LETS command requires at least one assignment target on the left-hand side.");
        }


        // Parse the right-hand side (RHS) values
        // It could be a list of expressions (e.g., 10, "string", 0) or a function call that returns a list
        _rhsFunction = Commands.GetNew(tokens.Token, config) as ICommandFunctionList;
        AGEBasicDebug.WriteConsole($"[AGE BASIC PARSE {CmdToken} #{config.LineNumber}] RHS is function? {_rhsFunction != null} ({tokens.ToString()})");
        if (_rhsFunction != null)
        {
            _rhsFunction.Parse(tokens);
        }
        else
        {
            _rhsExpressions.Parse(tokens);
        }

        AGEBasicDebug.WriteConsole($"[AGE BASIC PARSE {CmdToken} #{config.LineNumber}] Successfully parsed LETS with {_assignmentTargets.Count} targets.");
        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] Executing LETS with {_assignmentTargets.Count} assignment(s).");

        // 1. Get the values from the right-hand side (RHS)
        BasicValue[] rhsValues;
        if (_rhsFunction == null)
        {
            rhsValues = _rhsExpressions.ExecuteList(vars);
        }
        else
        {
            rhsValues = _rhsFunction.ExecuteList(vars);
        }

        // 2. Validate that there are enough values for all targets
        if (rhsValues.Length < _assignmentTargets.Count)
        {
            throw new Exception($"[{CmdToken} ERROR #{config.LineNumber}] Not enough values provided on the right-hand side ({rhsValues.Length}) for {_assignmentTargets.Count} targets. Missing values for: {string.Join(", ", _assignmentTargets.Skip(rhsValues.Length).Select(t => t.Name + (t.IsArray() ? "[...]" : "")))}.");
        }
        // Optionally, check for too many values, though some BASICs allow trailing unused RHS values.
        // if (rhsValues.Length > _assignmentTargets.Count)
        // {
        //     AGEBasicDebug.WriteConsole($"[{CmdToken} WARNING #{config.LineNumber}] Too many values provided on the right-hand side ({rhsValues.Length}) for {_assignmentTargets.Count} targets. Excess values will be ignored.");
        // }


        // 3. Assign each RHS value to its corresponding LHS target
        for (int i = 0; i < _assignmentTargets.Count; i++)
        {
            BasicVar currentVar = _assignmentTargets[i]; // This BasicVar object now holds the name and potential index expressions
            BasicValue valueToAssign = rhsValues[i];

            if (!currentVar.IsArray())
            {
                // This is a simple variable assignment (e.g., A = 10)
                vars[currentVar.Name] = valueToAssign;
                AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] Assigned '{valueToAssign}' to '{currentVar.Name}'.");
            }
            else
            {
                // This is an array element assignment (e.g., MYVAR[10,3] = "string")
                // First, check if the base array variable exists.
                if (!vars.Exists(currentVar.Name))
                {
                    throw new Exception($"[{CmdToken} ERROR #{config.LineNumber}]: Array variable '{currentVar.Name}' must be DIMensioned before assignment. Did you forget a DIM statement?");
                }

                // Evaluate the index expressions stored within the BasicVar object
                // Assumes BasicVar.IndexExpressions is a CommandExpressionList or similar that can execute to BasicValue[].
                BasicValue[] indexes = currentVar.IndexExpressions.ExecuteList(vars);

                // Use the BasicVars indexer to assign to the specific array element.
                // This implies BasicVars[string name][BasicValue[] indexes] is correctly implemented
                // to handle traversal and assignment into nested arrays.
                vars[currentVar.Name][indexes] = valueToAssign;
                AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] Assigned '{valueToAssign}' to '{currentVar.Name}[{string.Join(",", indexes.Select(idx => idx.ToString()))}]'.");
            }
        }

        return null; // LETS is a command, and commands typically return null
    }
}