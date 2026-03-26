using System;
using System.Collections.Generic;

// ONLED(ledIndex, "varName")
// Used with ONEVENT: ONEVENT ONLED(0, "P1_LAMP") GOTO 1000
// Returns a CONFIG-EVENT array consumed by the ONEVENT command.
class CommandFunctionONLED : CommandFunctionExpressionListBase
{
    public CommandFunctionONLED(ConfigurationCommands config) : base(config)
    {
        cmdToken = "ONLED";
        MinCantParamsRequired = 2;
    }

    public override BasicValue Execute(BasicVars vars)
    {
        BasicValue[] args = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(args[0], $"{cmdToken} - LED index must be a number (0-7)");
        FunctionHelper.ExpectedString(args[1], $"{cmdToken} - variable name must be a string");

        int ledIndex = (int)args[0].GetValueAsNumber();
        if (ledIndex < 0 || ledIndex > 7)
            throw new Exception($"{cmdToken} - LED index {ledIndex} out of range (0-7)");

        List<object> list = new List<object>
        {
            "CONFIG-EVENT",
            "on-led-change",
            (double)ledIndex,
            args[1].GetString()
        };
        return new BasicValue(list);
    }
}

// LEDSTATE(index)
// Returns the current state of LED index: 0 (off), 1 (on), -1 (unavailable or game not loaded).
// Example: LET S = LEDSTATE(0) : IF S < 0 THEN GOTO 200
class CommandFunctionLEDSTATE : CommandFunctionSingleExpressionBase
{
    public CommandFunctionLEDSTATE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "LEDSTATE";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        BasicValue idx = expr.Execute(vars);
        FunctionHelper.ExpectedNumber(idx, $"{cmdToken} - LED index must be a number");
        return new BasicValue(LibretroMameCore.getLedState((int)idx.GetValueAsNumber()));
    }
}
