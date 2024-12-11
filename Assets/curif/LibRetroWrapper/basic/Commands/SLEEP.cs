using System;
using System.Collections.Generic;

class CommandSLEEP : ICommandBase
{
    public string CmdToken { get; } = "SLEEP";
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    CommandExpression expr;
    ConfigurationCommands config;
    public CommandSLEEP(ConfigurationCommands config)
    {
        this.config = config;
        expr = new(config);
    }

    public bool Parse(TokenConsumer tokens)
    {
        expr.Parse(tokens);
        return true;
    }

    public BasicValue Execute(BasicVars vars)
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