using System;
using System.Collections.Generic;
using System.IO;

class CommandCALL : CommandSingleExpressionBase
{
    public CommandCALL(ConfigurationCommands config) : base(config) 
    {
        this.cmdToken = "CALL";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] [{expr}] ");

        expr.Execute(vars);
        return null;
    }

}