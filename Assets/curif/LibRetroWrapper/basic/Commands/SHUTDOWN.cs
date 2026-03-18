using System;

class CommandSHUTDOWN : CommandNoExpressionBase
{
    public CommandSHUTDOWN(ConfigurationCommands config) : base(config)
    {
        this.config = config;
        this.cmdToken = "SHUTDOWN";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN #{config.LineNumber} {CmdToken}] FORCE STOP ALL");
        this.config.shutdown = true;
        
        return null;
    }
}
