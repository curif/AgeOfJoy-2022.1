using System;

// Cabinet install from the cabinets folder (compressed) into the cabinet DB (see CabinetInstaller).
//
// CABDBINSTALL(cab)        starts unpacking cabinets/<cab>.zip into cabinetsdb/<cab> in the background,
//                          deletes the zip. cab: "galaga" or "galaga.zip". Returns 1.
// CABDBINSTALLSTATUS(cab)  "INSTALLING" | "INSTALLED" | "FAILED" | "NOTFOUND" (never requested)
//                          the cabinet is added to the cabinet list by the first call that sees it finished.

class CommandFunctionCABDBINSTALL : CommandFunctionSingleExpressionBase
{
    public CommandFunctionCABDBINSTALL(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABDBINSTALL";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        if (config?.GameRegistry == null)
            return BasicValue.False;

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNonEmptyString(val, " - cabinet name");

        try
        {
            CabinetInstaller.Start(val.GetValueAsString());
        }
        catch (Exception e)
        {
            throw new Exception($"{CmdToken}: {e.Message}");
        }
        return BasicValue.True;
    }
}

class CommandFunctionCABDBINSTALLSTATUS : CommandFunctionSingleExpressionBase
{
    public CommandFunctionCABDBINSTALLSTATUS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABDBINSTALLSTATUS";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        if (config?.GameRegistry == null)
            return new BasicValue("FAILED");

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNonEmptyString(val, " - cabinet name");

        return new BasicValue(CabinetInstaller.Status(val.GetValueAsString(), config.GameRegistry) switch
        {
            CabinetInstaller.InstallStatus.Installing => "INSTALLING",
            CabinetInstaller.InstallStatus.Installed => "INSTALLED",
            CabinetInstaller.InstallStatus.Failed => "FAILED",
            _ => "NOTFOUND",
        });
    }
}
