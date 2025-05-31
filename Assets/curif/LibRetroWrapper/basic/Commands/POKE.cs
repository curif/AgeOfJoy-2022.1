
using System;

class CommandPOKE : CommandExpressionListBase
{
    public CommandPOKE(ConfigurationCommands config) : base(config)
    {
        this.cmdToken = "POKE";
        this.MinCantParamsRequired = 2;
    }

    public override bool Parse(TokenConsumer tokens)
    {
        exprs.Parse(tokens);
        return true;
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}  #{config.LineNumber}] ");

        BasicValue[] vals;
        vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - memory offset");

        uint offset = (uint)vals[0].GetNumber();
        if (vals[1].IsNumber())
        {
            int value = vals[0].GetInt();
            if (value < 0 || value > 255)
                throw new Exception($"{CmdToken} value should be between 0 and 255");
            LibretroMameCore.setSram(offset, (uint)value);
            return null;
        }
        else if(vals[1].IsString())
        {
            LibretroMameCore.setSramBlock(offset, vals[1].GetString());
            return null;
        }
        throw new Exception("POKE only accepts NUMBER or STRINGS");
    }
}