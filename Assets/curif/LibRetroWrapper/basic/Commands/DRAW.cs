using System;
using UnityEngine;

/// <summary>
/// Implements the CHARPIXELX(CHAR_X, CHAR_Y) function for the BASIC interpreter.
/// Returns the screen pixel X coordinate of the character cell at (CHAR_X, CHAR_Y).
/// </summary>
class CommandFunctionDCHARPIXELX : CommandFunctionExpressionListBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandCHARPIXELX"/> class.
    /// </summary>
    /// <param name="config">The command configuration context.</param>
    public CommandFunctionDCHARPIXELX(ConfigurationCommands config) : base(config)
    {
        // The 'cmdToken' field is assumed to be defined in a base class
        // (like CommandFunctionBase or CommandFunctionExpressionListBase itself)
        // and used for logging or identification.
        this.cmdToken = "DCHARPIXELX";
        this.MinCantParamsRequired = 2;
    }

    public void CheckConfigRequirements(ConfigurationCommands config)
    {
        if (config?.ScreenGenerator == null)
            throw new ArgumentException($"[{CmdToken} ScreenGenerator is not available.");

    }
    /// <summary>
    /// Executes the CHARPIXELX function.
    /// </summary>
    /// <param name="vars">The current BASIC variables.</param>
    /// <returns>A BasicValue containing the pixel X coordinate, or -1 if an error occurs or coords are invalid.</returns>
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {this.cmdToken} #{config.LineNumber}] ");
        // The 'exprs' field (CommandExpressionList) is inherited from CommandFunctionExpressionListBase
        // and populated by the base.Parse method.
        BasicValue[] values = exprs.ExecuteList(vars);

        // The base.Parse(tokens, 2) should have already ensured we have 2 arguments.
        // If not, an exception would have been thrown there, or values.Length would be 2.
        // However, an explicit check here can be a safeguard if the base's behavior changes
        // or if ExecuteList could return a different number than parsed.
        if (values.Length != 2)
        {
            // This case should ideally be caught by the base.Parse(tokens, 2)
            throw new Exception($"[{this.cmdToken} ERROR #{config.LineNumber}] Internal error: Expected 2 arguments, but received {values.Length} after parsing.");
        }

        // Argument 1: CHAR_X
        FunctionHelper.ExpectedNumber(values[0], $"- CHAR_X for {this.cmdToken} must be a number.");
        int charX = values[0].GetInt();

        // Argument 2: CHAR_Y
        FunctionHelper.ExpectedNumber(values[1], $"- CHAR_Y for {this.cmdToken} must be a number.");
        int charY = values[1].GetInt();

        Vector2Int pixelPosition = config.ScreenGenerator.GetCharPixelPosition(charX, charY);

        // pixelPosition.x will be -1 if GetCharPixelPosition indicated an issue
        return new BasicValue(pixelPosition.x);
    }
}
/// <summary>
/// Implements the CHARPIXELY(CHAR_X, CHAR_Y) function for the BASIC interpreter.
/// Returns the screen pixel Y coordinate of the character cell at (CHAR_X, CHAR_Y).
/// </summary>
class CommandFunctionDCHARPIXELY : CommandFunctionExpressionListBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandCHARPIXELY"/> class.
    /// </summary>
    /// <param name="config">The command configuration context.</param>
    public CommandFunctionDCHARPIXELY(ConfigurationCommands config) : base(config)
    {
        this.cmdToken = "DCHARPIXELY";
        this.MinCantParamsRequired = 2;
    }

    public void CheckConfigRequirements(ConfigurationCommands config)
    {
        if (config?.ScreenGenerator == null)
            throw new ArgumentException($"[{CmdToken} ScreenGenerator is not available.");

    }
    /// <summary>
    /// Executes the CHARPIXELY function.
    /// </summary>
    /// <param name="vars">The current BASIC variables.</param>
    /// <returns>A BasicValue containing the pixel Y coordinate, or -1 if an error occurs or coords are invalid.</returns>
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {this.cmdToken} #{config.LineNumber}] ");

        BasicValue[] values = exprs.ExecuteList(vars);

        if (values.Length != 2)
        {
            throw new Exception($"[{this.cmdToken} ERROR #{config.LineNumber}] Internal error: Expected 2 arguments, but received {values.Length} after parsing.");
        }

        // Argument 1: CHAR_X
        FunctionHelper.ExpectedNumber(values[0], $"- CHAR_X for {this.cmdToken} must be a number.");
        int charX = values[0].GetInt();

        // Argument 2: CHAR_Y
        FunctionHelper.ExpectedNumber(values[1], $"- CHAR_Y for {this.cmdToken} must be a number.");
        int charY = values[1].GetInt();

        Vector2Int pixelPosition = config.ScreenGenerator.GetCharPixelPosition(charX, charY);

        // pixelPosition.y will be -1 if GetCharPixelPosition indicated an issue
        return new BasicValue(pixelPosition.y);
    }
}

class CommandFunctionDCHARPIXEL : CommandFunctionExpressionListBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandCHARPIXELX"/> class.
    /// </summary>
    /// <param name="config">The command configuration context.</param>
    public CommandFunctionDCHARPIXEL(ConfigurationCommands config) : base(config)
    {
        // The 'cmdToken' field is assumed to be defined in a base class
        // (like CommandFunctionBase or CommandFunctionExpressionListBase itself)
        // and used for logging or identification.
        this.cmdToken = "DCHARPIXEL";
        this.MinCantParamsRequired = 2;
    }

    public void CheckConfigRequirements(ConfigurationCommands config)
    {
        if (config?.ScreenGenerator == null)
            throw new ArgumentException($"[{CmdToken} ScreenGenerator is not available.");

    }
    /// <summary>
    /// Executes the CHARPIXELX function.
    /// </summary>
    /// <param name="vars">The current BASIC variables.</param>
    /// <returns>A BasicValue containing the pixel X coordinate, or -1 if an error occurs or coords are invalid.</returns>
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {this.cmdToken} #{config.LineNumber}] ");


        // The 'exprs' field (CommandExpressionList) is inherited from CommandFunctionExpressionListBase
        // and populated by the base.Parse method.
        BasicValue[] values = exprs.ExecuteList(vars);

        // Argument 1: CHAR_X
        FunctionHelper.ExpectedNumber(values[0], $"- CHAR_X for {this.cmdToken} must be a number.");
        int charX = values[0].GetInt();

        // Argument 2: CHAR_Y
        FunctionHelper.ExpectedNumber(values[1], $"- CHAR_Y for {this.cmdToken} must be a number.");
        int charY = values[1].GetInt();

        Vector2Int pixelPosition = config.ScreenGenerator.GetCharPixelPosition(charX, charY);
        BasicValue v = new BasicValue().Dim(2);
        v[0] = new BasicValue(pixelPosition[0]);
        v[1] = new BasicValue(pixelPosition[1]);

        // pixelPosition.x will be -1 if GetCharPixelPosition indicated an issue
        return v;
    }
}

/// <summary>
/// Implements the PSET PX, PY, COLOR_SPEC [, DRAW_FLAG] command.
/// Draws a single pixel at the specified (PX, PY) pixel coordinates.
/// </summary>
class CommandPSET : CommandExpressionListBase // Changed base class
{
    // CmdToken, config, and exprs are now inherited from CommandExpressionListBase

    public CommandPSET(ConfigurationCommands config) : base(config) // Call base constructor
    {
        this.cmdToken = "DPSET"; // Set CmdToken
        this.MinCantParamsRequired = 3;
    }


    public void CheckConfigRequirements(ConfigurationCommands config)
    {
        if (config?.ScreenGenerator == null)
            throw new ArgumentException($"[{CmdToken} ScreenGenerator is not available.");

    }
    public override BasicValue Execute(BasicVars vars) // Override Execute
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");

        if (config?.ScreenGenerator == null) // Keep null check here
            return null;

        BasicValue[] values = exprs.ExecuteList(vars); // Use exprs from base class

        // The count check is done in Parse, so values[0], [1], [2] should be safe.
        FunctionHelper.ExpectedNumber(values[0], $"- PX (pixel X) for {CmdToken}");
        FunctionHelper.ExpectedNumber(values[1], $"- PY (pixel Y) for {CmdToken}");
        int pixelX = values[0].GetInt();
        int pixelY = values[1].GetInt();

        Color32 colorToDraw;
        FunctionHelper.TryParseColor(values[2], out colorToDraw, $"- Color for {CmdToken}");

        bool drawImmediately = true; // Default
        if (values.Length > 3) // Argument for drawImmediately exists
        {
            drawImmediately = values[3].GetBoolean();
        }

        config.ScreenGenerator.DrawPoint(pixelX, pixelY, colorToDraw);

        if (drawImmediately)
        {
            config.ScreenGenerator.DrawScreen();
        }
        return null;
    }
}
/// <summary>
/// Implements the LINE PXY[2], PXY[2], COLOR_SPEC[3] [, DRAW_FLAG] command.
/// Draws a line between two specified pixel coordinate pairs.
/// </summary>
class CommandLINE : CommandExpressionListBase // Changed base class
{
    public CommandLINE(ConfigurationCommands config) : base(config) // Call base constructor
    {
        this.cmdToken = "DLINE"; // Set CmdToken
        this.MinCantParamsRequired = 3;
    }

    public void CheckConfigRequirements(ConfigurationCommands config) // Retained as is
    {
        if (config?.ScreenGenerator == null)
            throw new ArgumentException($"[{CmdToken} ScreenGenerator is not available.");
    }

    public override BasicValue Execute(BasicVars vars) // Override Execute
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");

        BasicValue[] values = exprs.ExecuteList(vars); // Use exprs from base class

        // Count validation is now handled in Parse by the base class.
        // The original code uses FunctionHelper.ExpectedAtLeast(values, 3);
        // We'll keep the `ExpectedArraySize` checks, as they validate the *content* of the expressions.
        FunctionHelper.ExpectedArraySize(values[0], 2, BasicValue.BasicValueType.Number, $"- Superior corner for {CmdToken}");
        FunctionHelper.ExpectedArraySize(values[1], 2, BasicValue.BasicValueType.Number, $"- Inferior corner for {CmdToken}");

        Color32 colorToDraw;
        FunctionHelper.TryParseColor(values[2], out colorToDraw, $"- Color for {CmdToken}");

        bool drawImmediately = true; // Default
        if (values.Length > 3) // Argument for drawImmediately exists
        {
            drawImmediately = values[3].GetBoolean();
        }

        config.ScreenGenerator.DrawLine(values[0][0].GetInt(), values[0][1].GetInt(),
                                            values[1][0].GetInt(), values[1][1].GetInt(),
                                            colorToDraw);

        if (drawImmediately)
        {
            config.ScreenGenerator.DrawScreen();
        }
        return null;
    }
}
/// <summary>
/// Implements the OVAL CORNER, PRADIUSX, PRADIUSY, BORDER_COLOR_SPEC [, FILL_FLAG [, FILL_COLOR_SPEC [, DRAW_FLAG]]] command.
/// Draws an ellipse/oval.
/// </summary>
class CommandOVAL : CommandExpressionListBase // Changed base class
{
    public CommandOVAL(ConfigurationCommands config) : base(config) // Call base constructor
    {
        this.cmdToken = "DOVAL"; // Set cmdToken
        this.MinCantParamsRequired = 4;
    }

    public void CheckConfigRequirements(ConfigurationCommands config) // Retained as is
    {
        if (config?.ScreenGenerator == null)
            throw new ArgumentException($"[{CmdToken} ScreenGenerator is not available.");
    }

    public override BasicValue Execute(BasicVars vars) // Override Execute
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");

        BasicValue[] values = exprs.ExecuteList(vars); // Use exprs from base class

        FunctionHelper.ExpectedArraySize(values[0], 2, BasicValue.BasicValueType.Number, $"- Corner coordinates for {CmdToken}");
        FunctionHelper.ExpectedNumber(values[1], $"PRADIUSX for {CmdToken}");
        FunctionHelper.ExpectedNumber(values[2], $"PRADIUSY for {CmdToken}");

        int pradiusX = values[1].GetInt();
        if (pradiusX <= 0) throw new Exception($"[{CmdToken} ERROR #{config.LineNumber}] PRADIUSX must be greater than 0.");
        int pradiusY = values[2].GetInt();
        if (pradiusY <= 0) throw new Exception($"[{CmdToken} ERROR #{config.LineNumber}] PRADIUSY must be greater than 0.");

        Color32 borderColor;
        FunctionHelper.TryParseColor(values[3], out borderColor, $"- Border Color for {CmdToken}"); // Corrected index from 2 to 3

        bool fillFlag = false; // Default
        Color32 fillColor = borderColor; // Default fill color to border color
        bool drawImmediately = true; // Default

        if (values.Length > 4) // Check for FILL_FLAG
        {
            fillFlag = values[4].GetBoolean();
            if (values.Length > 5) // Check for FILL_COLOR_SPEC
            {
                FunctionHelper.TryParseColor(values[5], out fillColor, $"- Fill Color for {CmdToken}"); // Corrected index from 4 to 5
                if (values.Length > 6) // Check for DRAW_FLAG
                {
                    drawImmediately = values[6].GetBoolean(); // Corrected index from 3 to 6
                }
            }
        }

        config.ScreenGenerator.DrawOval(values[0][0].GetInt(), values[0][1].GetInt(),
                                            pradiusX, pradiusY, borderColor, // Changed colorToDraw to borderColor
                                            fillFlag, fillColor);

        if (drawImmediately)
        {
            config.ScreenGenerator.DrawScreen();
        }
        return null;
    }
}
/// <summary>
/// Implements the CIRCLE CORNER, PRADIUS, BORDER_COLOR_SPEC [, FILL_FLAG [, FILL_COLOR_SPEC [, DRAW_FLAG]]] command.
/// Draws a circle.
/// </summary>
class CommandCIRCLE : CommandExpressionListBase // Changed base class
{
    public CommandCIRCLE(ConfigurationCommands config) : base(config) // Call base constructor
    {
        this.cmdToken = "DCIRCLE"; // Set cmdToken
        this.MinCantParamsRequired = 3;
    }

    public void CheckConfigRequirements(ConfigurationCommands config) // Retained as is
    {
        if (config?.ScreenGenerator == null)
            throw new ArgumentException($"[{CmdToken} ScreenGenerator is not available.");
    }

    public override BasicValue Execute(BasicVars vars) // Override Execute
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");

        BasicValue[] values = exprs.ExecuteList(vars); // Use exprs from base class

        FunctionHelper.ExpectedArraySize(values[0], 2, BasicValue.BasicValueType.Number, $"- Corner coordinates for {CmdToken}");
        FunctionHelper.ExpectedNumber(values[1], $"PRADIUS for {CmdToken}");
        int pradius = values[1].GetInt();

        // Parameter 3 (index 2): BORDER_COLOR_SPEC
        Color32 borderColor;
        FunctionHelper.TryParseColor(values[2], out borderColor, $"- Border Color for {CmdToken}");

        bool fillFlag = false; // Default
        Color32 fillColor = borderColor; // Default fill color to border color
        bool drawImmediately = true; // Default

        if (values.Length > 3) // Check for FILL_FLAG (at index 3)
        {
            fillFlag = values[3].GetBoolean();
            if (values.Length > 4) // Check for FILL_COLOR_SPEC (at index 4)
            {
                FunctionHelper.TryParseColor(values[4], out fillColor, $"- Fill Color for {CmdToken}"); // Assign to fillColor, not overwrite borderColor
                if (values.Length > 5) // Check for DRAW_FLAG (at index 5)
                {
                    drawImmediately = values[5].GetBoolean(); // Corrected index for drawImmediately
                }
            }
        }

        config.ScreenGenerator.DrawCircle(values[0][0].GetInt(), values[0][1].GetInt(), pradius, borderColor, fillFlag, fillColor);

        if (drawImmediately)
        {
            config.ScreenGenerator.DrawScreen();
        }
        return null;
    }
}
/// <summary>
/// Implements the BOX CORNER, SIZE, BORDER_COLOR_SPEC [, FILL_FLAG [, FILL_COLOR_SPEC [, DRAW_FLAG]]] command.
/// Draws a rectangle.
/// </summary>
class CommandBOX : CommandExpressionListBase // Changed base class
{
    public CommandBOX(ConfigurationCommands config) : base(config) // Call base constructor
    {
        this.cmdToken = "DBOX"; // Set cmdToken
        this.MinCantParamsRequired = 3;
    }

    public void CheckConfigRequirements(ConfigurationCommands config) // Retained as is
    {
        if (config?.ScreenGenerator == null)
            throw new ArgumentException($"[{CmdToken} ScreenGenerator is not available.");
    }

    public override BasicValue Execute(BasicVars vars) // Override Execute
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");

        BasicValue[] values = exprs.ExecuteList(vars); // Use exprs from base class

        FunctionHelper.ExpectedArraySize(values[0], 2, BasicValue.BasicValueType.Number, $"- Superior corner for {CmdToken}");
        FunctionHelper.ExpectedArraySize(values[1], 2, BasicValue.BasicValueType.Number, $"- Size for {CmdToken}");

        Color32 borderColor; // Renamed colorToDraw to borderColor for clarity
        FunctionHelper.TryParseColor(values[2], out borderColor, $"- Color for {CmdToken}");

        bool fillFlag = false; // Default
        Color32 fillColor = borderColor; // Default fill color to border color
        bool drawImmediately = true; // Default

        if (values.Length > 3) // Check for FILL_FLAG (at index 3)
        {
            fillFlag = values[3].GetBoolean();
            if (values.Length > 4) // Check for FILL_COLOR_SPEC (at index 4)
            {
                FunctionHelper.TryParseColor(values[4], out fillColor, $"- Fill color for {CmdToken}");
                if (values.Length > 5) // Check for DRAW_FLAG (at index 5)
                {
                    drawImmediately = values[5].GetBoolean(); // Corrected index for drawImmediately
                }
            }
        }

        config.ScreenGenerator.DrawBox(values[0][0].GetInt(), values[0][1].GetInt(),
                                        values[1][0].GetInt(), values[1][1].GetInt(),
                                        borderColor, fillFlag, fillColor); // Changed colorToDraw to borderColor

        if (drawImmediately)
        {
            config.ScreenGenerator.DrawScreen();
        }
        return null;
    }
}