using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;

class CommandIFTHEN : CommandBase
{
    double lineNoBlockStartThen=0, lineNoBlockStartElse=0, lineNoNextSentence = 0;
    protected CommandExpression expr; // The condition expression for the IF statement

    public CommandIFTHEN(ConfigurationCommands config) : base(config)
    {
        this.cmdToken = "IFTHEN"; // Or just "IF" depending on language convention
        expr = new(config);
    }

    public override bool Parse(TokenConsumer tokens)
    {
        ICommandBase localCmdThen, localCmdElse;
        List<ICommandBase> commandsThen = new List<ICommandBase>();
        List<ICommandBase> commandsElse = new List<ICommandBase>();

        CommandInternalGOTONextTo goToNextLine = new CommandInternalGOTONextTo(this.config);
        lineNoNextSentence = (int)config.LineNumber + 1 - AGEProgram.MinJump;
        goToNextLine.SetJumpLineNumber(lineNoNextSentence);

        expr.Parse(tokens);
        if (tokens.Token.ToUpper() != "THEN")
            throw new Exception($"malformed IF/THEN, THEN is missing");

        localCmdThen = Commands.GetNew(tokens.Next(), config);
        if (localCmdThen == null)
            throw new Exception($"Syntax error command not found in THEN clause: {tokens.ToString()}");
        
        tokens++;
        localCmdThen.Parse(tokens);
        commandsThen.Add(localCmdThen);
        while (tokens.Token == ":")
        {
            tokens++;
            ICommandBase cmdThen = Commands.GetNew(tokens.Token, config);
            if (cmdThen == null || cmdThen.Type != CommandType.Type.Command)
                throw new Exception($"Syntax error command not found in THEN sentence: {tokens.Token}  line: {(int)config.LineNumber}");
            commandsThen.Add(cmdThen);
            cmdThen.Parse(++tokens); //parse could change lineno
        }

        if (tokens.Token.ToUpper() == "ELSE")
        {
            localCmdElse = Commands.GetNew(tokens.Next(), config);
            if (localCmdElse == null)
                throw new Exception($"Syntax error command not found in ELSE clause: {tokens.ToString()}");

            tokens++;
            localCmdElse.Parse(tokens);
            commandsElse.Add(localCmdElse);
            while (tokens.Token == ":")
            {
                tokens++;
                ICommandBase moreCmds = Commands.GetNew(tokens.Token, config);
                if (moreCmds == null || moreCmds.Type != CommandType.Type.Command)
                    throw new Exception($"Syntax error command not found in ELSE sentence: {tokens.Token}  line: {(int)config.LineNumber}");
                commandsElse.Add(moreCmds);
                moreCmds.Parse(++tokens);
            }
        }

        foreach (ICommandBase moreCmd in commandsThen)
        {
            addCmd(moreCmd);
            if (lineNoBlockStartThen == 0)
                lineNoBlockStartThen = config.LineNumber;
        }
        addCmd(goToNextLine);

        if (commandsElse.Count > 0)
        {
            foreach (ICommandBase moreCmd in commandsElse)
            {
                addCmd(moreCmd);
                if (lineNoBlockStartElse == 0)
                    lineNoBlockStartElse = config.LineNumber;
            }
            addCmd(goToNextLine);
        }
        

        return true;
    }

    private double addCmd(ICommandBase command)
    {
        config.LineNumber += AGEProgram.MinJump; //actual line number increment.
        config.ageProgram.AddCommand(config.LineNumber, command);
        return config.LineNumber;
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN #{config.LineNumber} {CmdToken}] [{expr}] ");

        BasicValue condition = expr.Execute(vars);
        if (condition.IsTrue())
            config.JumpTo = lineNoBlockStartThen;
        else if (lineNoBlockStartElse != 0)
            config.JumpTo = lineNoBlockStartElse;
        else 
            config.JumpNextTo = lineNoNextSentence;
        return null;
    }

}