using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

class CommandFunctionCONTROLACTIVE : CommandFunctionSingleExpressionBase
{
    public CommandFunctionCONTROLACTIVE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CONTROLACTIVE";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        if (config?.ControlMap == null /* ||
                !config.ConfigurationController.ControlEnabled()*/)
            throw new Exception("no control map assigned.");
        
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNonEmptyString(val, "- Control ID");

        string mameControl = val.GetValueAsString();

        if (config.ControlMap.Active(mameControl) == 0)
            return BasicValue.False;
        return BasicValue.True;
    }

}


class CommandFunctionCONTROLRUMBLE : CommandFunctionExpressionListBase
{
    public CommandFunctionCONTROLRUMBLE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CONTROLRUMBLE";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 3);
    }
    public override BasicValue Execute(BasicVars vars)
    {
        if (config?.ControlMap == null)
            throw new Exception("no controller assigned.");
        
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[0], "- Control");
        FunctionHelper.ExpectedNumber(vals[1], "- amplitude");
        FunctionHelper.ExpectedNumber(vals[2], "- duration");

        string mameControl = vals[0].GetString();
        float amplitude = (float)vals[1].GetNumber();
        float duration = (float)vals[2].GetNumber();

        ConfigManager.WriteConsole($"[CommandFunctionCONTROLRUMBLE] {mameControl} amp: {amplitude} dur: {duration}");
        if (config.ControlMap.SendHapticImpulse(mameControl, amplitude, duration))
            return BasicValue.True;

        return BasicValue.False;
    }
}
