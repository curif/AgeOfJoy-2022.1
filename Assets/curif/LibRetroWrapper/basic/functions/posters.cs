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
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
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
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        if (config?.PostersController == null)
            return BasicValue.False;

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - poster position");
        FunctionHelper.ExpectedString(vals[1], " - image path");

        string path = vals[1].GetString();
        if (string.IsNullOrEmpty(path))
            return BasicValue.False; //fail

        bool result = config.PostersController.Replace((int)vals[0].GetNumber(), path);

        return new BasicValue(result ? 1 : 0);
    }
}
