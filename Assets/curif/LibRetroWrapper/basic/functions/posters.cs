using System;
using System.IO;
using System.Linq;

class CommandFunctionPOSTERROOMCOUNT : CommandFunctionNoExpressionBase
{
    public CommandFunctionPOSTERROOMCOUNT(ConfigurationCommands config) : base(config)
    {
        cmdToken = "POSTERROOMCOUNT";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.PostersController == null)
            return new BasicValue(0);

        return new BasicValue((double)config.PostersController.Count());
    }
}

class CommandFunctionPOSTERROOMREPLACE : CommandFunctionExpressionListBase
{
    public CommandFunctionPOSTERROOMREPLACE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "POSTERROOMREPLACE";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        if (config?.PostersController == null)
            return BasicValue.False;

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - poster position");
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - image path");

        bool result = config.PostersController.Replace((int)vals[0].GetNumber(), vals[1].GetString());
        return new BasicValue(result ? 1 : 0);
    }
}
