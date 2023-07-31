using System;
using System.Collections.Generic;
using System.IO;

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
        name = name.ToUpper();

        if (Commands.IsCommand(name) || Commands.IsFunction(name))
            return false;

        if (string.IsNullOrEmpty(name))
            return false;

        if (!char.IsLetter(name[0]))
            return false;

        for (int i = 1; i < name.Length; i++)
        {
            if (!char.IsLetterOrDigit(name[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override string ToString()
    {
        return " var: " + name;
    }
}