using System;

class CommandEND : CommandNoExpressionBase
{
    public CommandEND(ConfigurationCommands config) : base(config)
    {
        this.config = config;
        this.cmdToken = "END";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN #{config.LineNumber} {CmdToken}]");
        this.config.stop = true;
        return null;
    }
}