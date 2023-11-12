using System;
using System.Collections.Generic;
using System.IO;

public class BasicVars
{
    Dictionary<string, BasicValue> vars = new();

    public BasicValue GetValue(BasicVar var)
    {
        return GetValue(var.Name);
    }

    public bool Exists(string name)
    {
        return vars.ContainsKey(name.ToUpper());
    }
    public BasicValue GetValue(string name)
    {
        name = name.ToUpper();
        if (!vars.ContainsKey(name))
            throw new Exception($"variable '{name}' not defined");

        return vars[name];
    }

    public BasicValue SetValue(BasicVar var, BasicValue val)
    {
        return SetValue(var.Name, val);
    }

    public BasicValue SetValue(string name, BasicValue val)
    {
        vars[name] = val;
        return val;
    }

    public void Clean()
    {
        vars = new();
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