using System;

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
        int x,y;

        // print x,y, text/number              = 3 par
        // print x,y, text/number, inv         = 4 par
        // print x,y, text/number, inv, draw   = 5 par
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.ScreenGenerator == null)
            return null;

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedAtLeast(vals, 3);
        FunctionHelper.ExpectedNumber(vals[0], " - pos X");
        FunctionHelper.ExpectedNumber(vals[1], " - pos Y");

        x = vals[0].GetInt();
        y = vals[1].GetInt();

        if (x < 0 || x >= config.ScreenGenerator.CharactersXCount)
            throw new Exception($"printing out of screen (width): {x} : {config.ScreenGenerator.CharactersXCount}");
        if (y < 0 || y >= config.ScreenGenerator.CharactersYCount)
            throw new Exception($"printing out of screen (height): {y} : {config.ScreenGenerator.CharactersYCount}");

        str = new BasicValue(vals[2]);

        if (vals.Length > 3)
        {
            inverted = vals[3];
            if (vals.Length > 4)
                draw = vals[4];
        }
        
        if (str.IsNumber())
            str.CastTo(BasicValue.BasicValueType.String);

        string text = str.GetValueAsString();

        AGEBasicDebug.WriteConsole($"print {x}, {y}, {text}, {inverted}  ");
        config.ScreenGenerator.Print(x, y,
                                        text,
                                        inverted.GetBoolean());

        if (draw.GetBoolean())
            config.ScreenGenerator.DrawScreen();

        return null;
    }

}


class CommandPRINTLN : ICommandBase
{
    public string CmdToken { get; } = "PRINTLN";
    public CommandType.Type Type { get; } = CommandType.Type.Command;
    ConfigurationCommands config;
    CommandExpressionList exprs;

    public CommandPRINTLN(ConfigurationCommands configuration)
    {
        this.config = configuration;
        this.exprs = new(configuration);
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

        // print text/number                   = 1 par
        // print text/number, inv              = 2 par
        // print text/number, inv, draw        = 3 par

        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.ScreenGenerator == null)
            return null;

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedAtLeast(vals, 1);
        FunctionHelper.ExpectedNotNull(vals[0], " - content to print.");

        str = new BasicValue(vals[0]);

        if (vals.Length > 1)
        {
            inverted = vals[1];
            if (vals.Length > 2)
                draw = vals[2];
        }

        if (str.IsNumber())
            str.CastTo(BasicValue.BasicValueType.String);

        config.ScreenGenerator.Print(str.GetValueAsString(), inverted.GetBoolean());

        if (draw.GetBoolean())
            config.ScreenGenerator.DrawScreen();

        return null;
    }

}