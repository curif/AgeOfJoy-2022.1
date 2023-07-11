using System;
using System.Collections.Generic;
using System.IO;

public class BasicVars
{
    Dictionary<string, BasicValue> vars = new();

    public BasicValue GetValue(BasicVar var)
    {
        if (!vars.ContainsKey(var.Name))
            throw new Exception($"{var} variable not defined");

        return vars[var.Name];
    }

    public BasicValue SetValue(BasicVar var, BasicValue val)
    {
        vars[var.Name] = val;
        return val;
    }

    public void Clean()
    {
        vars = new();
    }

    public override string ToString()
    {
        string str = "";
        foreach(KeyValuePair<string, BasicValue> var in vars)
        {
            str += $"{var.Key}: {vars[var.Key].ToString()}\n";
        }
        return str;
    }

}