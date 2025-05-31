using System;
using System.Collections.Generic;

public class BasicVar
{
    string name;
    private BasicValue basicValue = null;

    //using in LET or Expressions to handle the Index of an array.
    private CommandExpressionList indexExpressions = null;
    
    public BasicVar(string varName)
    {
        if (!IsVariable(varName))
            throw new Exception($"var {varName} is not a valid variable name");
        name = varName.ToUpper();
        basicValue = new BasicValue();
    }

    public string Name { get { return name; } }

    public BasicValue BasicValue { get => basicValue; set => basicValue = value; }
    internal CommandExpressionList IndexExpressions { get => indexExpressions; set => indexExpressions = value; }

    // public string Value { get {return value;} set {val = value}; }
    public static bool IsVariable(string name)
    {
        // Return false early for null or empty strings
        if (string.IsNullOrEmpty(name))
            return false;

        // Ensure the first character is a letter
        if (!char.IsLetter(name[0]))
            return false;

        // Check remaining characters are letters or digits
        for (int i = 1; i < name.Length; i++)
        {
            if (!(name[i] == '_' || char.IsLetterOrDigit(name[i])))
                return false;
        }

        // Commands and Functions checks before any other processing
        if (Commands.IsCommand(name) || Commands.IsFunction(name))
            return false;

        return true;
    }
    public bool IsArray()
    {
        return indexExpressions != null && indexExpressions.Count > 0;
    }
    public static BasicVar Parse(TokenConsumer tokens, ConfigurationCommands config)
    {
        BasicVar var;
        var = new BasicVar(tokens.Token);

        if (tokens.TheNextIs("["))
        {
            tokens.Next(); // Consume varname
            tokens.Next(); // Consume '['
            List<CommandExpression> indexExprs = new List<CommandExpression>();
            var.indexExpressions = new(config);
            var.indexExpressions.Parse(tokens);
            if (tokens.Token != "]")
                throw new Exception($"[Var:{var.Name} ERROR #{config.LineNumber}] Malformed index expression.");
        }
        return var;
    }

    public override string ToString()
    {
        return "[" + name +"]";
    }
}