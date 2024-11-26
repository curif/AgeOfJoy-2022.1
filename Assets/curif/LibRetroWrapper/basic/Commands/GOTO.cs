using System;
using System.Collections.Generic;
using System.IO;
using static OVRHaptics;

class CommandGOTO : ICommandBase
{
    public string CmdToken { get; } = "GOTO";
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    CommandExpression expr;
    ConfigurationCommands config;
    BasicValue lineNumber = null;
    bool exactly = true;
    public CommandGOTO(ConfigurationCommands config)
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


class CommandInternalGOTO : ICommandBase
{
    public string CmdToken { get; } = "internal-GOTO";
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    protected double lineNumber;
    protected ConfigurationCommands config;
    
    public CommandInternalGOTO(ConfigurationCommands config)
    {
        this.config = config;
    }
    public bool Parse(TokenConsumer tokens) { return true;}

    public virtual BasicValue Execute(BasicVars vars)
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
    public CommandInternalGOTONextTo(ConfigurationCommands config) : base(config) { }

    public override BasicValue Execute(BasicVars vars)
    {
        config.JumpNextTo = lineNumber;
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN  #{config.LineNumber} {CmdToken}] internal-GOTO-nextTo next to #{config.JumpNextTo}");
        return null;
    }
}