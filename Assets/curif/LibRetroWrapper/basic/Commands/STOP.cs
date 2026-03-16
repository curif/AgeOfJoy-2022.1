using System;

class CommandSTOP : CommandNoExpressionBase
{
    public CommandSTOP(ConfigurationCommands config) : base(config)
    {
        this.config = config;
        this.cmdToken = "STOP";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN #{config.LineNumber} {CmdToken}] FORCE STOP ALL");
        this.config.stop = true;
        this.config.stopAllEvents = true;
        
        return null;
    }
}
