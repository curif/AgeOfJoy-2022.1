using System;
using System.Collections.Generic;
using System.IO;

class CommandRETURN : ICommandBase
{
    public string CmdToken { get; } = "RETURN";
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    CommandExpression expr = new();

    public CommandRETURN()
    {
    }
    public bool Parse(TokenConsumer tokens)
    {
        return true;
    }

    public void ExtractLastNumberAndRemainingString(string input,
                                                    out int lastNumber,
                                                    out string remainingString)
    {
        int lastCommaIndex = input.LastIndexOf(',');
        if (lastCommaIndex != -1)
        {
            remainingString = input.Substring(0, lastCommaIndex);
            lastNumber = int.Parse(input.Substring(lastCommaIndex + 1));
        }
        else
        {
            remainingString = string.Empty;
            lastNumber = int.Parse(input);
        }
    }

    public BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        BasicValue gosubstack = vars.GetValue("_gosubstack");
        string stack = gosubstack.GetValueAsString();
        if (stack == "")
            throw new Exception("RETURN without GOSUB");

        int returnLineNumber;
        string remainigLines;
        ExtractLastNumberAndRemainingString(stack, out returnLineNumber, out remainigLines);
        gosubstack.SetValue(remainigLines);
        ConfigManager.WriteConsole($"[CommandRETURN] new stack: {remainigLines} return to {returnLineNumber}");

        return new BasicValue((double)returnLineNumber);
    }

}