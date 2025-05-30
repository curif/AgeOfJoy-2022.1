using System;
using UnityEngine;

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
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");
        if (config?.ScreenGenerator == null)
            return null;

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedAtLeast(vals, 3);
        FunctionHelper.ExpectedNumber(vals[0], " - pos X");
        FunctionHelper.ExpectedNumber(vals[1], " - pos Y");

        x = vals[0].GetInt();
        y = vals[1].GetInt();

        if (x < 0 || x >= config.ScreenGenerator.CharactersXCount)
        {
            AGEBasicDebug.WriteConsole($"printing out of screen (width): {x} : {config.ScreenGenerator.CharactersXCount}");
            return null;
        }
        if (y < 0 || y >= config.ScreenGenerator.CharactersYCount)
        {
            AGEBasicDebug.WriteConsole($"printing out of screen (height): {y} : {config.ScreenGenerator.CharactersYCount}");
            return null;
        }

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
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");
        if (config?.ScreenGenerator == null)
            return null;

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedAtLeast(vals, 1);
        FunctionHelper.ExpectedNotNull(vals[0], " - content to print."); // Still good for mandatory arg

        BasicValue str = new BasicValue(vals[0]);

        bool invertedFlag = false; // Default
        bool drawFlag = true;     // Default

        if (vals.Length > 1) // Argument for invertedFlag exists
        {
            invertedFlag = vals[1].GetBoolean();
            if (vals.Length > 2) // Argument for drawFlag exists
            {
                drawFlag = vals[2].GetBoolean();
            }
        }

        if (str.IsNumber())
            str.CastTo(BasicValue.BasicValueType.String);

        config.ScreenGenerator.Print(str.GetValueAsString(), invertedFlag);

        if (drawFlag)
            config.ScreenGenerator.DrawScreen();

        return null;
    }
}
public class CommandPRINTCENTERED : ICommandBase
{
    public string CmdToken { get; } = "PRINTCENTERED";
    public CommandType.Type Type { get; } = CommandType.Type.Command;
    private readonly ConfigurationCommands _config;
    private readonly CommandExpressionList _expressions;

    public CommandPRINTCENTERED(ConfigurationCommands config)
    {
        this._config = config;
        this._expressions = new CommandExpressionList(config);
    }

    public bool Parse(TokenConsumer tokens)
    {
        return _expressions.Parse(tokens);
    }

    public BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{_config.LineNumber}] ");
        if (_config?.ScreenGenerator == null)
        {
            AGEBasicDebug.WriteConsole($"[{CmdToken} ERROR #{_config.LineNumber}] ScreenGenerator is not available.");
            return null;
        }

        BasicValue[] values = _expressions.ExecuteList(vars);
        FunctionHelper.ExpectedAtLeast(values, 2);

        FunctionHelper.ExpectedNumber(values[0], $"- Y coordinate for {CmdToken} must be a number.");
        int y = values[0].GetInt();

        FunctionHelper.ExpectedNotNull(values[1], $"- TEXT$ for {CmdToken} cannot be null."); // Good for mandatory
        BasicValue textValue = values[1];
        if (textValue.IsNumber())
        {
            textValue.CastTo(BasicValue.BasicValueType.String);
        }
        else if (!textValue.IsString())
        {
            throw new Exception($"[{CmdToken} ERROR #{_config.LineNumber}] TEXT$ argument must be a string or number.");
        }
        string textToPrint = textValue.GetString();

        bool inverted = false; // Default
        if (values.Length > 2) // Argument for inverted exists
        {
            inverted = values[2].GetBoolean();
        }

        bool drawImmediately = true; // Default
        if (values.Length > 3) // Argument for drawImmediately exists
        {
            drawImmediately = values[3].GetBoolean();
        }

        if (y < 0 || y >= _config.ScreenGenerator.CharactersYCount)
        {
            AGEBasicDebug.WriteConsole($"[{CmdToken} WARNING #{_config.LineNumber}] Y coordinate {y} is out of screen bounds (0-{_config.ScreenGenerator.CharactersYCount - 1}). Text may not be visible.");
        }

        _config.ScreenGenerator.PrintCentered(y, textToPrint, inverted);

        if (drawImmediately)
        {
            _config.ScreenGenerator.DrawScreen();
        }
        return null;
    }

}

//`SCROLL N_LINES [, FILL_COLOR_SPEC [, DRAW_FLAG]]`
public class CommandSCROLL : ICommandBase
{
    public string CmdToken { get; } = "SCROLL";
    public CommandType.Type Type { get; } = CommandType.Type.Command;
    private readonly ConfigurationCommands _config;
    private readonly CommandExpressionList _expressions;

    public CommandSCROLL(ConfigurationCommands config)
    {
        this._config = config;
        this._expressions = new CommandExpressionList(config);
    }

    public bool Parse(TokenConsumer tokens)
    {
        return _expressions.Parse(tokens);
    }

    public BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{_config.LineNumber}] ");
        if (_config?.ScreenGenerator == null)
        {
            AGEBasicDebug.WriteConsole($"[{CmdToken} ERROR #{_config.LineNumber}] ScreenGenerator is not available.");
            return null;
        }

        BasicValue[] values = _expressions.ExecuteList(vars);
        FunctionHelper.ExpectedAtLeast(values, 1);

        FunctionHelper.ExpectedNumber(values[0], $"- N_LINES for {CmdToken} must be a number.");
        int nLines = values[0].GetInt();

        Color32? fillColor = null;
        bool drawImmediately = true; // Default

        if (values.Length > 1)
        {
            Color32 colorToDraw;
            FunctionHelper.TryParseColor(values[1], out colorToDraw, $"- Color for {CmdToken}");
            fillColor = colorToDraw;
            if (values.Length > 2)
            {
                drawImmediately = values[2].GetBoolean();
            }
        }

        _config.ScreenGenerator.ScrollCharacterArea(nLines, fillColor);

        if (drawImmediately)
        {
            _config.ScreenGenerator.DrawScreen();
        }
        return null;
    }
}
//SCROLLRECT CORNER, SIZE, N_LINES [, FILL_COLOR_SPEC [, DRAW_FLAG]]
public class CommandSCROLLRECT : ICommandBase
{
    public string CmdToken { get; } = "SCROLLRECT";
    public CommandType.Type Type { get; } = CommandType.Type.Command;
    private readonly ConfigurationCommands _config;
    private readonly CommandExpressionList _expressions;

    public CommandSCROLLRECT(ConfigurationCommands config)
    {
        this._config = config;
        this._expressions = new CommandExpressionList(config);
    }

    public bool Parse(TokenConsumer tokens)
    {
        return _expressions.Parse(tokens);
    }

    public BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{_config.LineNumber}] ");
        if (_config?.ScreenGenerator == null)
        {
            AGEBasicDebug.WriteConsole($"[{CmdToken} ERROR #{_config.LineNumber}] ScreenGenerator is not available.");
            return null;
        }

        BasicValue[] values = _expressions.ExecuteList(vars);
        FunctionHelper.ExpectedAtLeast(values, 3);

        FunctionHelper.ExpectedArraySize(values[0], 2, BasicValue.BasicValueType.Number, $"- Superior corner for {CmdToken}");
        FunctionHelper.ExpectedArraySize(values[1], 2, BasicValue.BasicValueType.Number, $"- Size for {CmdToken}");
        FunctionHelper.ExpectedNumber(values[2], $"- N_LINES for {CmdToken}");


        int charRectWidth = values[1][0].GetInt();
        if (charRectWidth <= 0) throw new Exception($"[{CmdToken} ERROR #{_config.LineNumber}] CW (char rect width) must be greater than 0."); 
        int charRectHeight = values[1][1].GetInt();
        if (charRectHeight <= 0) throw new Exception($"[{CmdToken} ERROR #{_config.LineNumber}] CH (char rect height) must be greater than 0.");

        int nLines = values[2].GetInt();

        Color32? fillColor = null;
        bool drawImmediately = true; // Default

        if (values.Length > 3)
        {
            Color32 colorToDraw;
            FunctionHelper.TryParseColor(values[3], out colorToDraw, $"- Color for {CmdToken}");
            fillColor = colorToDraw;
            if (values.Length > 4)
            {
                drawImmediately = values[4].GetBoolean();
            }
        }

        _config.ScreenGenerator.ScrollCharacterSubRectVertical(values[0][0].GetInt(), values[1][1].GetInt(), 
                                                                charRectWidth, charRectHeight, 
                                                                nLines, fillColor);

        if (drawImmediately)
        {
            _config.ScreenGenerator.DrawScreen();
        }
        return null;
    }
}