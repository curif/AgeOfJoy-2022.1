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
        if (config?.ConfigurationController == null /* ||
                !config.ConfigurationController.ControlEnabled()*/)
            throw new Exception("no controller assigned.");
        
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNonEmptyString(val, "- Control");

        string mameControl = val.GetValueAsString();

        BasicValue ret = (config.ConfigurationController.ControlActive(mameControl) ?
                                        new BasicValue(1) :
                                        new BasicValue(0)
                            );
        ConfigManager.WriteConsole($"[CommandFunctionCONTROLACTIVE] {mameControl} status: {ret.ToString()}]");
        return ret;
    }
}
