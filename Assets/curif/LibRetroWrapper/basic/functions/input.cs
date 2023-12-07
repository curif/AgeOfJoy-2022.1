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
        if (config?.ConfigurationController == null ||
                !config.ConfigurationController.ControlEnabled())
            throw new Exception("no controller enabled.");
        
        BasicValue val = expr.Execute(vars);
        if (!val.IsString())
            return new BasicValue(0);

        string mameControl = val.GetValueAsString();

        BasicValue ret = (config.ConfigurationController.ControlActive(mameControl) ?
                                        new BasicValue(1) :
                                        new BasicValue(0)
                            );
        ConfigManager.WriteConsole($"[CommandFunctionCONTROLACTIVE] {mameControl} status: {ret.ToString()}]");
        return ret;
    }
}
