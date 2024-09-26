using CleverCrow.Fluid.BTs.Decorators;
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
        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        BasicValue str;
        BasicValue inverted = new BasicValue(false);
        BasicValue draw = new BasicValue(true);
        int x = -1, y = -1;

        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.ScreenGenerator == null)
            return null;

        BasicValue[] vals = exprs.ExecuteList(vars);

        if (exprs.Count > 2) {
            FunctionHelper.ExpectedNumber(vals[0], " - pos X");
            FunctionHelper.ExpectedNumber(vals[1], " - pos Y");

            x = (int)vals[0].GetValueAsNumber();
            y = (int)vals[1].GetValueAsNumber();

            if (x < 0 || x >= config.ScreenGenerator.CharactersXCount)
                throw new Exception($"printing out of screen (width): {x} : {config.ScreenGenerator.CharactersXCount}");
            if (y < 0 || y >= config.ScreenGenerator.CharactersYCount)
                throw new Exception($"printing out of screen (height): {y} : {config.ScreenGenerator.CharactersYCount}");

            str = new BasicValue(vals[2]);

            if (vals[3] != null)
                inverted = vals[3];

            if (vals[4] != null)
                draw = vals[4];
        }
        else
        {
            str = new BasicValue(vals[0]);
            FunctionHelper.ExpectedNotNull(vals[0], " - content to print.");

            if (vals[1] != null)
                inverted = vals[1];

            if (vals[2] != null)
                draw = vals[2];
        }

        if (str.IsNumber())
            str.CastTo(BasicValue.BasicValueType.String);

        string text = str.GetValueAsString();

        if (x != -1)
        {
            AGEBasicDebug.WriteConsole($"print {x}, {y}, {text}, {inverted}  ");
            config.ScreenGenerator.Print(x, y,
                                         text,
                                         inverted.GetBoolean());
        }
        else
        {
            AGEBasicDebug.WriteConsole($"print {text}, {inverted}  ");
            config.ScreenGenerator.Print(text, inverted.GetBoolean());
        }

        if (draw.GetBoolean())
            config.ScreenGenerator.DrawScreen();

        return null;
    }

}