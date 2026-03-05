using Meta.WitAi.Data;
using System;
using System.Collections.Generic;
using static OVRHaptics;
public class BasicVars
{
    private readonly Dictionary<string, BasicVar> vars = new(); // Case-sensitive dictionary

    public BasicValue this[string varName]
    {
        get
        {
            return GetValue(varName); // Reads the value, throws if not found
        }
        set
        {
            if (!Exists(varName))
            {
                DeclareNewVariable(varName);
            }
            SetValue(varName, value);
        }
    }
    public BasicValue this[BasicValue varName]
    {
        get
        {
            return this[varName.GetString()]; // Reads the value, throws if not found
        }
        set
        {
            this[varName.GetString()] = value;
        }
    }
    public BasicValue this[BasicVar var]
    {
        get
        {
            return this[var.Name]; // Reads the value, throws if not found
        }
        set
        {
            if (!var.IsArray())
            {
                // This is a simple variable assignment (e.g., A = 10)
                this[var.Name] = value; //it will be created if not exists
                return;
            }
            if (!Exists(var.Name))
                throw new Exception($"variable array {var.Name} must be DIMensioned before assignment.");
            BasicValue[] indexes = var.IndexExpressions.ExecuteList(this);
            this[var.Name][indexes] = value;
        }
    }

    public void Register(BasicVar var)
    {
        if (!Exists(var))
            vars[var.Name] = var;
    }
    public BasicValue GetValue(string name)
    {
        name = name.ToUpper();
        if (!vars.TryGetValue(name, out var basicVar))
            throw new Exception($"Variable '{name}' not defined");

        return basicVar.BasicValue;
    }

    public BasicVar DeclareNewVariable(string name)
    {
        BasicVar basicVar = new BasicVar(name);
        vars[basicVar.Name] = basicVar;
        return basicVar;
    }
    public void ThrowIfNotExists(BasicVar basicVar, string msg = "")
    {
        if (!vars.ContainsKey(basicVar.Name))
            throw new Exception($"Variable '{basicVar.Name}' not defined {msg}");
    }

    public void ThrowIfNotExists(string name, string msg = "")
    {
        if (!Exists(name))
            throw new Exception($"Variable '{name}' not defined {msg}");
    }
    public void ThrowIfNotExists(string name)
    {
        if (!Exists(name.ToUpper()))
            throw new Exception($"Variable '{name}' not defined");
    }

    public bool Exists(string name)
    {
        return vars.ContainsKey(name.ToUpper());
    }
    public bool Exists(BasicVar var)
    {
        return vars.ContainsKey(var.Name);
    }
    public BasicVar GetVar(string name)
    {
        ThrowIfNotExists(name);
        return this.vars[name.ToUpper()];
    }
    public BasicValue SetValue(string name, BasicValue val)
    {
        string upperName = name.ToUpper();
        if (!vars.ContainsKey(upperName))
        {
            DeclareNewVariable(upperName);
        }
        vars[upperName].BasicValue = val;
        return val;
    }

    public BasicValue SetValue(BasicVar var, BasicValue val)
    {
        return SetValue(var.Name, val);
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
        foreach (KeyValuePair<string, BasicVar> var in vars)
        {
            str += $"{var.Key}: {vars[var.Key]}\n";
        }
        return str;
    }

}