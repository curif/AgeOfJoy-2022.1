using System;
using UnityEngine;

class CommandPRINT : CommandExpressionListBase
{

    public CommandPRINT(ConfigurationCommands config) : base(config)
    {
        // print x,y, text/number              = 3 par
        // print x,y, text/number, inv         = 4 par
        // print x,y, text/number, inv, draw   = 5 par
        this.MinCantParamsRequired = 3;
        this.cmdToken = "PRINT";
    }

    public void CheckConfigRequirements(ConfigurationCommands config)
    {
        if (config?.ScreenGenerator == null)
            throw new ArgumentException($"[{CmdToken} ScreenGenerator is not available.");
    }

    public override BasicValue Execute(BasicVars vars)
    {
        bool inverted = false;
        bool draw = true;
        int x,y;

        BasicValue[] vals = exprs.ExecuteList(vars);
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

        if (vals.Length > 3)
        {
            inverted = vals[3].GetBoolean();
            if (vals.Length > 4)
                draw = vals[4].GetBoolean();
        }
        
        AGEBasicDebug.WriteConsole($"print {x}, {y}, {vals[2]}, {inverted}  ");
        config.ScreenGenerator.Print(x, y,
                                        vals[2].GetString(),
                                        inverted);

        if (draw)
            config.ScreenGenerator.DrawScreen();

        return null;
    }

}

class CommandPRINTLN : CommandExpressionListBase
{
    public CommandPRINTLN(ConfigurationCommands config) : base(config)
    {
        this.cmdToken = "PRINTLN";
        this.MinCantParamsRequired = 1;
    }

    public void CheckConfigRequirements(ConfigurationCommands config)
    {
        if (config?.ScreenGenerator == null)
            throw new ArgumentException($"[{CmdToken} ScreenGenerator is not available.");
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNotNull(vals[0], " - content to print."); // Still good for mandatory arg

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

        config.ScreenGenerator.Print(vals[0].GetString(), invertedFlag);

        if (drawFlag)
            config.ScreenGenerator.DrawScreen();

        return null;
    }
}
class CommandPRINTCENTERED : CommandExpressionListBase
{ 

    public CommandPRINTCENTERED(ConfigurationCommands config) : base(config)
    {
        this.cmdToken = "PRINTCENTERED";
        this.MinCantParamsRequired = 3;
    }

    public void CheckConfigRequirements(ConfigurationCommands config)
    {
        if (config?.ScreenGenerator == null)
            throw new ArgumentException($"[{CmdToken} ScreenGenerator is not available.");
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);

        FunctionHelper.ExpectedNumber(vals[0], $"- Y coordinate for {CmdToken} must be a number.");
        FunctionHelper.ExpectedNotNull(vals[1], " - content to print.");
        int y = vals[0].GetInt();
        string text = vals[1].GetValueAsString();

        bool invertedFlag = false; // Default
        bool drawFlag = true;     // Default

        if (vals.Length > 2) // Argument for invertedFlag exists
        {
            invertedFlag = vals[2].GetBoolean();
            if (vals.Length > 3) // Argument for drawFlag exists
            {
                drawFlag = vals[3].GetBoolean();
            }
        }

        config.ScreenGenerator.PrintCentered(y, text, invertedFlag);

        if (drawFlag)
            config.ScreenGenerator.DrawScreen();
        return null;
    }

}

//`SCROLL N_LINES [, FILL_COLOR_SPEC [, DRAW_FLAG]]`
class CommandSCROLL : CommandExpressionListBase
{

    public CommandSCROLL(ConfigurationCommands config) : base(config)
    {
        this.cmdToken = "SCROLL";
    }

    public void CheckConfigRequirements(ConfigurationCommands config)
    {
        if (config?.ScreenGenerator == null)
            throw new ArgumentException($"[{CmdToken} ScreenGenerator is not available.");
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], $"- N_LINES for {CmdToken} must be a number.");
        int nLines = vals[0].GetInt();

        Color32? fillColor = null;
        bool drawImmediately = true; // Default

        if (vals.Length > 1)
        {
            Color32 colorToDraw;
            FunctionHelper.TryParseColor(vals[1], out colorToDraw, $"- Color for {CmdToken}");
            fillColor = colorToDraw;
            if (vals.Length > 2)
            {
                drawImmediately = vals[2].GetBoolean();
            }
        }

        config.ScreenGenerator.ScrollCharacterArea(nLines, fillColor);

        if (drawImmediately)
        {
            config.ScreenGenerator.DrawScreen();
        }
        return null;
    }
}

// SCROLLRECT CORNER, SIZE, N_LINES [, FILL_COLOR_SPEC [, DRAW_FLAG]]
class CommandSCROLLRECT : CommandExpressionListBase // Inherit from CommandExpressionListBase
{
    public CommandSCROLLRECT(ConfigurationCommands config) : base(config)
    {
        this.cmdToken = "SCROLLRECT"; // Set the command token via the base class's protected field.
        this.MinCantParamsRequired = 3; // Set minimum required parameters: CORNER, SIZE, N_LINES
        // The 'exprs' field (from CommandExpressionListBase) is already initialized via base(config).
    }

    // The Parse method is now handled by CommandExpressionListBase's Parse implementation.
    // It will automatically parse a list of expressions into 'this.exprs',
    // and will check for MinCantParamsRequired.
    // Removed:
    // public bool Parse(TokenConsumer tokens)
    // {
    //     return _expressions.Parse(tokens);
    // }

    public void CheckConfigRequirements(ConfigurationCommands config)
    {
        // Use the inherited 'config' field
        if (config?.ScreenGenerator == null)
            throw new ArgumentException($"[{CmdToken}] ScreenGenerator is not available for SCROLLRECT.");
    }

    // Execute method must now be marked 'override'
    public override BasicValue Execute(BasicVars vars)
    {
        // Use the inherited 'config' and 'cmdToken' fields
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");
        if (config?.ScreenGenerator == null)
        {
            AGEBasicDebug.WriteConsole($"[{CmdToken} ERROR #{config.LineNumber}] ScreenGenerator is not available.");
            return null;
        }

        // Use the inherited 'exprs' field to execute the list of expressions
        BasicValue[] values = exprs.ExecuteList(vars);

        // MinCantParamsRequired = 3 should be checked by Parse, but defensive runtime check is good.
        if (values.Length < 3)
            throw new Exception($"[{CmdToken} ERROR #{config.LineNumber}] Missing parameters. Expected: CORNER, SIZE, N_LINES.");

        // Validate CORNER (values[0]) - expected to be an array/list of 2 numbers
        FunctionHelper.ExpectedArraySize(values[0], 2, BasicValue.BasicValueType.Number, $"- CORNER for {CmdToken} must be an array of 2 numbers (X, Y).");
        int cornerX = values[0][0].GetInt();
        int cornerY = values[0][1].GetInt();

        // Validate SIZE (values[1]) - expected to be an array/list of 2 numbers
        FunctionHelper.ExpectedArraySize(values[1], 2, BasicValue.BasicValueType.Number, $"- SIZE for {CmdToken} must be an array of 2 numbers (Width, Height).");
        int charRectWidth = values[1][0].GetInt();
        if (charRectWidth <= 0) throw new Exception($"[{CmdToken} ERROR #{config.LineNumber}] Character rectangle width (SIZE[0]) must be greater than 0.");
        int charRectHeight = values[1][1].GetInt();
        if (charRectHeight <= 0) throw new Exception($"[{CmdToken} ERROR #{config.LineNumber}] Character rectangle height (SIZE[1]) must be greater than 0.");

        // Validate N_LINES (values[2]) - expected to be a number
        FunctionHelper.ExpectedNumber(values[2], $"- N_LINES for {CmdToken} must be a number.");
        int nLines = values[2].GetInt();

        Color32? fillColor = null;
        bool drawImmediately = true; // Default

        if (values.Length > 3) // Check for FILL_COLOR_SPEC (values[3])
        {
            Color32 colorToDraw;
            FunctionHelper.TryParseColor(values[3], out colorToDraw, $"- Color for {CmdToken}");
            fillColor = colorToDraw;
            if (values.Length > 4) // Check for DRAW_FLAG (values[4])
            {
                drawImmediately = values[4].GetBoolean();
            }
        }
        else if (values.Length > 3 && values[3] == null)
        {
            throw new Exception($"[{CmdToken} ERROR #{config.LineNumber}] Expected a color value for FILL_COLOR_SPEC, but found null.");
        }


        // Call the ScreenGenerator method with the extracted and validated parameters
        config.ScreenGenerator.ScrollCharacterSubRectVertical(cornerX, cornerY, // Pass cornerX and cornerY
                                                              charRectWidth, charRectHeight,
                                                              nLines, fillColor);

        if (drawImmediately)
        {
            config.ScreenGenerator.DrawScreen();
        }
        return null;
    }
}