
using System;
using UnityEngine;

class CommandPOKE : ICommandBase
{
    public string CmdToken { get; } = "POKE";
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    CommandExpressionList exprs;
    ConfigurationCommands config;

    public CommandPOKE(ConfigurationCommands config)
    {
        this.config = config;
        exprs = new(config);
    }

    public bool Parse(TokenConsumer tokens)
    {
        exprs.Parse(tokens);
        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}  #{config.LineNumber}] ");

        BasicValue[] vals;

        vals = exprs.ExecuteList(vars);

        if (vals == null || vals[0] == null)
            throw new Exception($"{CmdToken} POKE parameters missing, example POKE offset, value.");
        FunctionHelper.ExpectedNumber(vals[0], " - memory offset");
        if (vals[1] == null)
            throw new Exception($"{CmdToken} POKE parameter value is missing, example: POKE offset, value.");

        uint offset = (uint)vals[0].GetNumber();
        if (vals[1].IsNumber())
        {
            int value = vals[0].GetInt();
            if (value < 0 || value > 255)
                throw new Exception($"{CmdToken} value should be between 0 and 255");
            LibretroMameCore.setSram(offset, (uint)value);
        }
        else
            LibretroMameCore.setSramBlock(offset, vals[1].GetString());

        return null;
    }
}