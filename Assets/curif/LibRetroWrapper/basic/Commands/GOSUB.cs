using System;
using System.Collections.Generic;
using System.IO;

class CommandGOSUB : CommandSingleExpressionBase
{
    public CommandGOSUB(ConfigurationCommands config) : base(config)
    {
        this.cmdToken = "GOSUB";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        expr.Parse(tokens);
        return true;
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN #{config.LineNumber}  {CmdToken}] [{expr}] ");

        config.Gosub.Push(config.LineNumber);

        BasicValue lineNumber = expr.Execute(vars);
        config.JumpTo = lineNumber.GetValueAsNumber();

        return null;
    }

}