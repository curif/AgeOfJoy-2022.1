using System;
using System.Collections.Generic;
public class BasicVars
{
    private readonly Dictionary<string, BasicValue> vars = new(); // Case-sensitive dictionary

    public BasicValue GetValue(BasicVar var)
    {
        if (!vars.TryGetValue(var.Name, out var value))
            throw new Exception($"Variable '{var.Name}' not defined");

        return value;
    }
    public BasicValue GetValue(string name)
    {
        name = name.ToUpper();
        if (!vars.TryGetValue(name, out var value))
            throw new Exception($"Variable '{name}' not defined");

        return value;
    }

    public bool Exists(string name)
    {
        return vars.ContainsKey(name.ToUpper());
    }

    public BasicValue SetValue(BasicVar var, BasicValue val)
    {
        vars[var.Name] = val; //is uppercase by contructor
        return val;
    }

    public BasicValue SetValue(string name, BasicValue val)
    {
        vars[name.ToUpper()] = val;
        return val;
    }

    public void Clean()
    {
        vars.Clear();
    }

    public void Remove(string name)
    {
        name = name.ToUpper();
        if (vars.ContainsKey(name))
            vars.Remove(name);
    }
    public void Remove(BasicVar var)
    {
        vars.Remove(var.Name);
    }
    
    public override string ToString()
    {
        string str = "";
        foreach (KeyValuePair<string, BasicValue> var in vars)
        {
            str += $"{var.Key}: {vars[var.Key].ToString()} [{vars[var.Key].Type().ToString()}]\n";
        }
        return str;
    }

}