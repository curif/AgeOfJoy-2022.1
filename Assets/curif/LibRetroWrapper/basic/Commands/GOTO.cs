using System;
using System.Collections.Generic;
using System.IO;
using static OVRHaptics;

class CommandGOTO : CommandSingleExpressionBase
{

    BasicValue lineNumber = null;
    bool exactly = true;
    public CommandGOTO(ConfigurationCommands config) : base(config)
    {
        this.cmdToken = "GOTO";
    }
    public override BasicValue Execute(BasicVars vars)
    {

        if (lineNumber == null)
            lineNumber = expr.Execute(vars);

        if (exactly)
        {
            config.JumpTo = lineNumber.GetValueAsNumber();
            AGEBasicDebug.WriteConsole($"[AGE BASIC RUN  #{config.LineNumber} {CmdToken}] [{expr}] GOTO exactly #{config.JumpTo}");
        }
        else
        {
            config.JumpNextTo = lineNumber.GetValueAsNumber();
            AGEBasicDebug.WriteConsole($"[AGE BASIC RUN  #{config.LineNumber} {CmdToken}] [{expr}] GOTO next to #{config.JumpNextTo}");
        }

        return null;
    }

    public void SetJumpLineNumber(BasicValue lineNo, bool exactly=true)
    {
        this.lineNumber = lineNo;
        this.exactly = exactly;
    }

}


class CommandInternalGOTO : CommandNoExpressionBase
{
    protected double lineNumber;
    
    public CommandInternalGOTO(ConfigurationCommands config) : base(config)
    {
        this.config = config;
        this.cmdToken = "Internal-GOTO";

    }
    public override bool Parse(TokenConsumer tokens) { return true;}

    public override BasicValue Execute(BasicVars vars)
    {
        config.JumpTo = lineNumber;
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN  #{config.LineNumber} {CmdToken}] internal-GOTO next to #{config.JumpTo}");
        return null;
    }
    public void SetJumpLineNumber(double lineNo)
    {
        this.lineNumber = lineNo;
    }
}

class CommandInternalGOTONextTo : CommandInternalGOTO
{
    public CommandInternalGOTONextTo(ConfigurationCommands config) : base(config) 
    {
        this.cmdToken = "Internal-NEXT";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        config.JumpNextTo = lineNumber;
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN  #{config.LineNumber} {CmdToken}] internal-GOTO-nextTo next to #{config.JumpNextTo}");
        return null;
    }
}