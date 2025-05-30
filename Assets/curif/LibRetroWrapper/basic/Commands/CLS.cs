using System;
using System.Collections.Generic;
using System.IO;

class CommandCLS : CommandNoExpressionBase
{
    public CommandCLS(ConfigurationCommands config) : base(config)
    {
        this.cmdToken = "CLS";
    }

    public void CheckConfigRequirements(ConfigurationCommands config)
    {
        if (config?.ScreenGenerator == null)
            throw new ArgumentException($"[{CmdToken} ScreenGenerator is not available.");
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");
        config.ScreenGenerator.Clear();
        config.ScreenGenerator.DrawScreen();
        return null;
    }
}