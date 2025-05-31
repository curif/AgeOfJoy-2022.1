using System;
using System.Collections.Generic;

class CommandSLEEP : CommandSingleExpressionBase
{   public CommandSLEEP(ConfigurationCommands config) : base(config)
    {
        this.cmdToken = "SLEEP";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] [{expr}] ");

        BasicValue timeToSleepInSecs = expr.Execute(vars);
        FunctionHelper.ExpectedNumber(timeToSleepInSecs, " - time to sleep (in secs)");
        float sleepTime = (float)timeToSleepInSecs.GetValueAsNumber();
        if (sleepTime <= 0.01)
            throw new Exception("SLEEP doesn't support values below 0.01");

        config.SleepTime = sleepTime;

        return null;
    }
}