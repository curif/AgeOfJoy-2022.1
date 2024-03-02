using System;
using System.Collections.Generic;
using System.IO;

class CommandPRINT : ICommandBase
{
    public string CmdToken { get; } = "PRINT";
    public CommandType.Type Type { get; } = CommandType.Type.Command;
    ConfigurationCommands config;
    CommandExpressionList exprs;

    public CommandPRINT(ConfigurationCommands config)
    {
        this.config = config;
        this.exprs = new(config);
    }

    public bool Parse(TokenConsumer tokens)
    {
        exprs.Parse(tokens);
        if (exprs.Count < 3)
            throw new Exception($"{CmdToken}() parameter/s missing, expected x,y, expr, 1/0.");
        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.ScreenGenerator == null)
            return null;

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - pos X");
        FunctionHelper.ExpectedNumber(vals[1], " - pos Y");

        int x = (int)vals[0].GetValueAsNumber();
        int y = (int)vals[1].GetValueAsNumber();

        if (x < 0 || x >= config.ScreenGenerator.CharactersWidth)
            throw new Exception("printing out of screen (width)");
        if (y < 0 || y >= config.ScreenGenerator.CharactersHeight)
            throw new Exception("printing out of screen (height)");

        BasicValue str = new BasicValue(vals[2]);
        if (str.IsNumber())
            str.CastTo(BasicValue.BasicValueType.String);
        string text = str.GetValueAsString();

        bool inverted = false;
        if (vals[3] != null)
            inverted = vals[3].GetValueAsNumber() != 0;
        bool draw = true;
        if (vals[4] != null)
            draw = vals[4].GetValueAsNumber() != 0;

        AGEBasicDebug.WriteConsole($"print {x}, {y}, {text}, {inverted}  ");
        config.ScreenGenerator.Print(x, y,
                                     text,
                                     inverted);
        if (draw)
            config.ScreenGenerator.DrawScreen();

        return null;
    }

}