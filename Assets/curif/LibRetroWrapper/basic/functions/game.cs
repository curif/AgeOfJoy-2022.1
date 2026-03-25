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

// PEEK(offset [, region])
// region: 0=SAVE_RAM (default), 1=RTC, 2=SYSTEM_RAM, 3=VIDEO_RAM
class CommandFunctionPEEK : CommandFunctionExpressionListBase
{
    public CommandFunctionPEEK(ConfigurationCommands config) : base(config)
    {
        cmdToken = "PEEK";
        MinCantParamsRequired = 1;
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");

        if (!LibretroMameCore.GameLoaded)
            throw new Exception($"[AGE BASIC RUN {CmdToken}] A game isn't running yet");

        BasicValue[] args = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(args[0], "offset");
        uint offset = (uint)args[0].GetValueAsNumber();

        int value;
        if (args.Length > 1)
        {
            FunctionHelper.ExpectedNumber(args[1], "region");
            uint region = (uint)args[1].GetValueAsNumber();
            value = LibretroMameCore.getMemory(region, offset);
        }
        else
        {
            value = LibretroMameCore.getSram((int)offset);
        }

        return new BasicValue(value);
    }
}
