using System;


class CommandFunctionGAMEISRUNNING : CommandFunctionNoExpressionBase
{
    public CommandFunctionGAMEISRUNNING(ConfigurationCommands config) : base(config)
    {
        cmdToken = "GAMEISRUNNING";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        return new BasicValue(LibretroMameCore.GameLoaded);
    }
}

class CommandFunctionPEEK: CommandFunctionSingleExpressionBase
{

    public CommandFunctionPEEK(ConfigurationCommands config) : base(config)
    {
        cmdToken = "PEEK";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");

        if (!LibretroMameCore.GameLoaded)
            throw new Exception("[AGE BASIC RUN {CmdToken}] A game isn't running yet");

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNumber(val);

        int value = LibretroMameCore.getSram((int)val.GetNumber());

        return new BasicValue(value);
    }
}
