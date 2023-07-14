using System;
using System.Collections.Generic;
using System.IO;

class CommandFunctionStringBase : CommandFunctionBase
{
    protected void validateParams(BasicValue[] vals, int expectedParams)
    {
        if (exprs.Count != expectedParams)
            throw new Exception($"{cmdToken}() parameter/s missing, actual {exprs.Count} expected: {expectedParams}");

        for (int par = 0; par < exprs.Count; par++)
        {
            if (vals[par] == null)
                throw new Exception($"{cmdToken}() parameter #{par} missing");
            if (!vals[par].IsString())
                throw new Exception($"{cmdToken}() parameter #{par} must be a string");
        }
    }
}
class CommandFunctionLEN : CommandFunctionStringBase
{
    public CommandFunctionLEN()
    {
        cmdToken = "LEN";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        validateParams(vals, 1);
        double ret = vals[0].GetValueAsString().Length;
        return new BasicValue(ret);
    }
}
class CommandFunctionUCASE : CommandFunctionStringBase
{
    public CommandFunctionUCASE()
    {
        cmdToken = "UCASE";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        validateParams(vals, 1);
        string ret = vals[0].GetValueAsString().ToUpper();
        return new BasicValue(ret);
    }
}


class CommandFunctionLCASE : CommandFunctionStringBase
{
    public CommandFunctionLCASE()
    {
        cmdToken = "LCASE";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        validateParams(vals, 1);
        string ret = vals[0].GetValueAsString().ToLower();
        return new BasicValue(ret);
    }
}