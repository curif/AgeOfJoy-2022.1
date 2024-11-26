using System;

public class BasicVar
{
    string name;

    public BasicVar(string varName)
    {
        if (!IsVariable(varName))
            throw new Exception($"var {varName} is not a valid variable name");
        name = varName.ToUpper();
    }

    public string Name { get { return name; } }
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
            if (!char.IsLetterOrDigit(name[i]))
                return false;
        }

        // Commands and Functions checks before any other processing
        if (Commands.IsCommand(name) || Commands.IsFunction(name))
            return false;

        return true;
    }


    public override string ToString()
    {
        return "[" + name +"]";
    }
}