using System;
using System.Collections.Generic;
using System.IO;

class CommandSHOW : CommandNoExpressionBase
{

    public CommandSHOW(ConfigurationCommands config) : base(config)
    {
        this.config = config;
        this.cmdToken = "SHOW";
    }

    public void CheckConfigRequirements(ConfigurationCommands config) // Retained as is
    {
        if (config?.ScreenGenerator == null)
            throw new ArgumentException($"[{CmdToken} ScreenGenerator is not available.");
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");

        config.ScreenGenerator.DrawScreen();

        return null;
    }

}