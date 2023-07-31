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
        if (exprs.Count < 3 )
            throw new Exception($"{CmdToken}() parameter/s missing, expected x,y, expr, 1/0.");
        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.ScreenGenerator == null)
        {
            return null;
        }

        BasicValue[] vals = exprs.ExecuteList(vars);

        if (vals[2].IsNumber())
            vals[2].CastTo(BasicValue.BasicValueType.String);

        int x = (int)vals[0].GetValueAsNumber();
        int y = (int)vals[1].GetValueAsNumber();

        if (x < 0 ||
            x >= config.ScreenGenerator.CharactersWidth ||
            y < 0 ||
            y >= config.ScreenGenerator.CharactersHeight
        )
            throw new Exception("printing out of screen");

        string text = vals[2].GetValueAsString();
        
        bool inverted = false;
        if (vals[3] != null)
            inverted = vals[3].GetValueAsNumber() != 0;
        bool draw = true;
        if (vals[4] != null)
            draw = vals[4].GetValueAsNumber() != 0;

        ConfigManager.WriteConsole($"print {x}, {y}, {text}, {inverted}  ");
        config.ScreenGenerator.Print(x, y, text, inverted);
        if (draw)
            config.ScreenGenerator.DrawScreen();

        return null;
    }

}