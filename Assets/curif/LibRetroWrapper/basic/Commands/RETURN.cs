using System;
using System.Collections.Generic;
using System.IO;

class CommandRETURN : CommandNoExpressionBase
{
    public CommandRETURN(ConfigurationCommands config) : base(config)
    {
        this.config = config;
        this.cmdToken = "RETURN";
    }
    public override BasicValue Execute(BasicVars vars)
    {

        if (config.Gosub.Count == 0)
            throw new Exception("RETURN without GOSUB");

        config.JumpNextTo = config.Gosub.Pop();
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN  #{config.LineNumber} {CmdToken}] to #{config.JumpNextTo}");
        return null;
    }

}