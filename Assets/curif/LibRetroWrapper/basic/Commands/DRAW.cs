using System;

using UnityEngine;

/// <summary>
/// Implements the CHARPIXELX(CHAR_X, CHAR_Y) function for the BASIC interpreter.
/// Returns the screen pixel X coordinate of the character cell at (CHAR_X, CHAR_Y).
/// </summary>
class CommandFunctionCHARPIXELX : CommandFunctionExpressionListBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandCHARPIXELX"/> class.
    /// </summary>
    /// <param name="config">The command configuration context.</param>
    public CommandFunctionCHARPIXELX(ConfigurationCommands config) : base(config)
    {
        // The 'cmdToken' field is assumed to be defined in a base class
        // (like CommandFunctionBase or CommandFunctionExpressionListBase itself)
        // and used for logging or identification.
        this.cmdToken = "DCHARPIXELX";
    }

    /// <summary>
    /// Parses the arguments for the CHARPIXELX function.
    /// This function expects exactly 2 arguments.
    /// </summary>
    /// <param name="tokens">The token consumer.</param>
    /// <returns>True if parsing was successful, false otherwise.</returns>
    public override bool Parse(TokenConsumer tokens)
    {
        // Call the base class's Parse method, specifying that 2 arguments are expected.
        // The base class (CommandFunctionExpressionListBase) should handle consuming
        // the opening '(', parsing the expressions with CommandExpressionList,
        // and consuming the closing ')'.
        return base.Parse(tokens, 2);
    }

    /// <summary>
    /// Executes the CHARPIXELX function.
    /// </summary>
    /// <param name="vars">The current BASIC variables.</param>
    /// <returns>A BasicValue containing the pixel X coordinate, or -1 if an error occurs or coords are invalid.</returns>
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {this.cmdToken} #{config.LineNumber}] ");

        if (config?.ScreenGenerator == null)
        {
            throw new Exception($"[{this.cmdToken} ERROR #{config.LineNumber}] ScreenGenerator is not available.");
        }

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
class CommandFunctionCHARPIXELY : CommandFunctionExpressionListBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandCHARPIXELY"/> class.
    /// </summary>
    /// <param name="config">The command configuration context.</param>
    public CommandFunctionCHARPIXELY(ConfigurationCommands config) : base(config)
    {
        this.cmdToken = "DCHARPIXELY";
    }

    /// <summary>
    /// Parses the arguments for the CHARPIXELY function.
    /// This function expects exactly 2 arguments.
    /// </summary>
    /// <param name="tokens">The token consumer.</param>
    /// <returns>True if parsing was successful, false otherwise.</returns>
    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 2);
    }

    /// <summary>
    /// Executes the CHARPIXELY function.
    /// </summary>
    /// <param name="vars">The current BASIC variables.</param>
    /// <returns>A BasicValue containing the pixel Y coordinate, or -1 if an error occurs or coords are invalid.</returns>
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {this.cmdToken} #{config.LineNumber}] ");

        if (config?.ScreenGenerator == null)
        {
            throw new Exception($"[{this.cmdToken} ERROR #{config.LineNumber}] ScreenGenerator is not available.");
        }

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

/// <summary>
/// Implements the PSET PX, PY, COLOR_SPEC [, DRAW_FLAG] command.
/// Draws a single pixel at the specified (PX, PY) pixel coordinates.
/// </summary
class CommandPSET : ICommandBase
{
    public string CmdToken { get; } = "DPSET";
    public CommandType.Type Type { get; } = CommandType.Type.Command;
    private readonly ConfigurationCommands _config;
    private readonly CommandExpressionList _expressions;

    public CommandPSET(ConfigurationCommands config)
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

        FunctionHelper.ExpectedNumber(values[0], $"- PX (pixel X) for {CmdToken}");
        int pixelX = values[0].GetInt();
        FunctionHelper.ExpectedNumber(values[1], $"- PY (pixel Y) for {CmdToken}");
        int pixelY = values[1].GetInt();

        Color32 colorToDraw;
        int colorArgsConsumed;
        if (!FunctionHelper.TryParseColor(values, 2, _config, out colorToDraw, out colorArgsConsumed))
        {
            throw new Exception($"[{CmdToken} ERROR #{_config.LineNumber}] Invalid color specification. Expected color name or R,G,B values starting at argument 3.");
        }

        bool drawImmediately = true; // Default
        int drawFlagIndex = 2 + colorArgsConsumed;

        if (values.Length > drawFlagIndex) // Argument for drawImmediately exists
        {
            drawImmediately = values[drawFlagIndex].GetBoolean();
        }

        // Check for too many args: if drawFlag was processed, next index is drawFlagIndex + 1
        int expectedArgCount = drawFlagIndex + (values.Length > drawFlagIndex ? 1 : 0);
        if (values.Length > expectedArgCount)
        {
            throw new Exception($"[{CmdToken} ERROR #{_config.LineNumber}] Too many arguments provided.");
        }

        _config.ScreenGenerator.DrawPoint(pixelX, pixelY, colorToDraw);

        if (drawImmediately)
        {
            _config.ScreenGenerator.DrawScreen();
        }
        return null;
    }
}

/// <summary>
/// Implements the LINE PX1, PY1, PX2, PY2, COLOR_SPEC [, DRAW_FLAG] command.
/// Draws a line between two specified pixel coordinate pairs.
/// </summary>
public class CommandLINE : ICommandBase
{
    public string CmdToken { get; } = "DLINE";
    public CommandType.Type Type { get; } = CommandType.Type.Command;
    private readonly ConfigurationCommands _config;
    private readonly CommandExpressionList _expressions;

    public CommandLINE(ConfigurationCommands config)
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
        FunctionHelper.ExpectedAtLeast(values, 5);

        FunctionHelper.ExpectedNumber(values[0], $"- PX1 (pixel X1) for {CmdToken}");
        int pixelX1 = values[0].GetInt();
        FunctionHelper.ExpectedNumber(values[1], $"- PY1 (pixel Y1) for {CmdToken}");
        int pixelY1 = values[1].GetInt();
        FunctionHelper.ExpectedNumber(values[2], $"- PX2 (pixel X2) for {CmdToken}");
        int pixelX2 = values[2].GetInt();
        FunctionHelper.ExpectedNumber(values[3], $"- PY2 (pixel Y2) for {CmdToken}");
        int pixelY2 = values[3].GetInt();

        Color32 colorToDraw;
        int colorArgsConsumed;
        if (!FunctionHelper.TryParseColor(values, 4, _config, out colorToDraw, out colorArgsConsumed))
        {
            throw new Exception($"[{CmdToken} ERROR #{_config.LineNumber}] Invalid color specification. Expected color name or R,G,B values starting at argument 5.");
        }

        bool drawImmediately = true; // Default
        int drawFlagIndex = 4 + colorArgsConsumed;

        if (values.Length > drawFlagIndex) // Argument for drawImmediately exists
        {
            drawImmediately = values[drawFlagIndex].GetBoolean();
        }

        int expectedArgCount = drawFlagIndex + (values.Length > drawFlagIndex ? 1 : 0);
        if (values.Length > expectedArgCount)
        {
            throw new Exception($"[{CmdToken} ERROR #{_config.LineNumber}] Too many arguments provided.");
        }

        _config.ScreenGenerator.DrawLine(pixelX1, pixelY1, pixelX2, pixelY2, colorToDraw);

        if (drawImmediately)
        {
            _config.ScreenGenerator.DrawScreen();
        }
        return null;
    }
}

/// <summary>
/// Implements the OVAL PCX, PCY, PRADIUSX, PRADIUSY, BORDER_COLOR_SPEC [, FILL_FLAG [, FILL_COLOR_SPEC [, DRAW_FLAG]]] command.
/// Draws an ellipse/oval.
/// </summary>
public class CommandOVAL : ICommandBase
{
    public string CmdToken { get; } = "DOVAL";
    public CommandType.Type Type { get; } = CommandType.Type.Command;
    private readonly ConfigurationCommands _config;
    private readonly CommandExpressionList _expressions;

    public CommandOVAL(ConfigurationCommands config)
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
        int argIdx = 0;
        FunctionHelper.ExpectedAtLeast(values, 5);

        FunctionHelper.ExpectedNumber(values[argIdx], $"PCX for {CmdToken}");
        int pcx = values[argIdx++].GetInt();
        FunctionHelper.ExpectedNumber(values[argIdx], $"PCY for {CmdToken}");
        int pcy = values[argIdx++].GetInt();
        FunctionHelper.ExpectedNumber(values[argIdx], $"PRADIUSX for {CmdToken}");
        int pradiusX = values[argIdx++].GetInt();
        if (pradiusX <= 0) throw new Exception($"[{CmdToken} ERROR #{_config.LineNumber}] PRADIUSX must be greater than 0.");
        FunctionHelper.ExpectedNumber(values[argIdx], $"PRADIUSY for {CmdToken}");
        int pradiusY = values[argIdx++].GetInt();
        if (pradiusY <= 0) throw new Exception($"[{CmdToken} ERROR #{_config.LineNumber}] PRADIUSY must be greater than 0.");

        Color32 borderColor;
        int borderColorArgsConsumed;
        if (!FunctionHelper.TryParseColor(values, argIdx, _config, out borderColor, out borderColorArgsConsumed))
        {
            throw new Exception($"[{CmdToken} ERROR #{_config.LineNumber}] Invalid BORDER_COLOR specification starting at argument {argIdx + 1}.");
        }
        argIdx += borderColorArgsConsumed;

        bool fillFlag = false; // Default
        Color32? fillColor = null;
        bool drawImmediately = true; // Default

        if (argIdx < values.Length) // Potential fillFlag argument exists
        {
            fillFlag = values[argIdx].GetBoolean();
            argIdx++;

            if (fillFlag && argIdx < values.Length) // Potential fillColor argument exists
            {
                Color32 parsedFillColor;
                int fillColorArgsConsumed;
                if (FunctionHelper.TryParseColor(values, argIdx, _config, out parsedFillColor, out fillColorArgsConsumed))
                {
                    fillColor = parsedFillColor;
                    argIdx += fillColorArgsConsumed;
                }
            }
        }

        if (argIdx < values.Length) // Potential drawFlag argument exists
        {
            drawImmediately = values[argIdx].GetBoolean();
            argIdx++;
        }

        if (argIdx < values.Length) // Still more arguments than expected
        {
            throw new Exception($"[{CmdToken} ERROR #{_config.LineNumber}] Too many arguments provided.");
        }

        _config.ScreenGenerator.DrawOval(pcx, pcy, pradiusX, pradiusY, borderColor, fillFlag, fillColor);

        if (drawImmediately)
        {
            _config.ScreenGenerator.DrawScreen();
        }
        return null;
    }
}

/// <summary>
/// Implements the CIRCLE PCX, PCY, PRADIUS, BORDER_COLOR_SPEC [, FILL_FLAG [, FILL_COLOR_SPEC [, DRAW_FLAG]]] command.
/// Draws a circle.
/// </summary>
public class CommandCIRCLE : ICommandBase
{
    public string CmdToken { get; } = "DCIRCLE";
    public CommandType.Type Type { get; } = CommandType.Type.Command;
    private readonly ConfigurationCommands _config;
    private readonly CommandExpressionList _expressions;

    public CommandCIRCLE(ConfigurationCommands config)
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
        int argIdx = 0;
        FunctionHelper.ExpectedAtLeast(values, 4);

        FunctionHelper.ExpectedNumber(values[argIdx], $"PCX for {CmdToken}");
        int pcx = values[argIdx++].GetInt();
        FunctionHelper.ExpectedNumber(values[argIdx], $"PCY for {CmdToken}");
        int pcy = values[argIdx++].GetInt();
        FunctionHelper.ExpectedNumber(values[argIdx], $"PRADIUS for {CmdToken}");
        int pradius = values[argIdx++].GetInt();
        if (pradius <= 0) throw new Exception($"[{CmdToken} ERROR #{_config.LineNumber}] PRADIUS must be greater than 0.");

        Color32 borderColor;
        int borderColorArgsConsumed;
        if (!FunctionHelper.TryParseColor(values, argIdx, _config, out borderColor, out borderColorArgsConsumed))
        {
            throw new Exception($"[{CmdToken} ERROR #{_config.LineNumber}] Invalid BORDER_COLOR specification starting at argument {argIdx + 1}.");
        }
        argIdx += borderColorArgsConsumed;

        bool fillFlag = false; // Default
        Color32? fillColor = null;
        bool drawImmediately = true; // Default

        if (argIdx < values.Length) // Potential fillFlag argument exists
        {
            fillFlag = values[argIdx].GetBoolean();
            argIdx++;

            if (fillFlag && argIdx < values.Length) // Potential fillColor argument exists
            {
                Color32 parsedFillColor;
                int fillColorArgsConsumed;
                if (FunctionHelper.TryParseColor(values, argIdx, _config, out parsedFillColor, out fillColorArgsConsumed))
                {
                    fillColor = parsedFillColor;
                    argIdx += fillColorArgsConsumed;
                }
            }
        }

        if (argIdx < values.Length) // Potential drawFlag argument exists
        {
            drawImmediately = values[argIdx].GetBoolean();
            argIdx++;
        }

        if (argIdx < values.Length) // Still more arguments than expected
        {
            throw new Exception($"[{CmdToken} ERROR #{_config.LineNumber}] Too many arguments provided.");
        }

        _config.ScreenGenerator.DrawCircle(pcx, pcy, pradius, borderColor, fillFlag, fillColor);

        if (drawImmediately)
        {
            _config.ScreenGenerator.DrawScreen();
        }
        return null;
    }
}

/// <summary>
/// Implements the BOX PX, PY, PWIDTH, PHEIGHT, BORDER_COLOR_SPEC [, FILL_FLAG [, FILL_COLOR_SPEC [, DRAW_FLAG]]] command.
/// Draws a rectangle.
/// </summary>
class CommandBOX : ICommandBase
{
    public string CmdToken { get; } = "DBOX";
    public CommandType.Type Type { get; } = CommandType.Type.Command;
    private readonly ConfigurationCommands _config;
    private readonly CommandExpressionList _expressions;

    public CommandBOX(ConfigurationCommands config)
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
        int argIdx = 0;
        FunctionHelper.ExpectedAtLeast(values, 5);

        FunctionHelper.ExpectedNumber(values[argIdx], $"PX for {CmdToken}");
        int px = values[argIdx++].GetInt();
        FunctionHelper.ExpectedNumber(values[argIdx], $"PY for {CmdToken}");
        int py = values[argIdx++].GetInt();
        FunctionHelper.ExpectedNumber(values[argIdx], $"PWIDTH for {CmdToken}");
        int pwidth = values[argIdx++].GetInt();
        if (pwidth <= 0) throw new Exception($"[{CmdToken} ERROR #{_config.LineNumber}] PWIDTH must be greater than 0.");
        FunctionHelper.ExpectedNumber(values[argIdx], $"PHEIGHT for {CmdToken}");
        int pheight = values[argIdx++].GetInt();
        if (pheight <= 0) throw new Exception($"[{CmdToken} ERROR #{_config.LineNumber}] PHEIGHT must be greater than 0.");

        Color32 borderColor;
        int borderColorArgsConsumed;
        if (!FunctionHelper.TryParseColor(values, argIdx, _config, out borderColor, out borderColorArgsConsumed))
        {
            throw new Exception($"[{CmdToken} ERROR #{_config.LineNumber}] Invalid BORDER_COLOR specification starting at argument {argIdx + 1}.");
        }
        argIdx += borderColorArgsConsumed;

        bool fillFlag = false; // Default
        Color32? fillColor = null;
        bool drawImmediately = true; // Default

        if (argIdx < values.Length) // Potential fillFlag argument exists
        {
            fillFlag = values[argIdx].GetBoolean();
            argIdx++;

            if (fillFlag && argIdx < values.Length) // Potential fillColor argument exists
            {
                Color32 parsedFillColor;
                int fillColorArgsConsumed;
                if (FunctionHelper.TryParseColor(values, argIdx, _config, out parsedFillColor, out fillColorArgsConsumed))
                {
                    fillColor = parsedFillColor;
                    argIdx += fillColorArgsConsumed;
                }
            }
        }

        if (argIdx < values.Length) // Potential drawFlag argument exists
        {
            drawImmediately = values[argIdx].GetBoolean();
            argIdx++;
        }

        if (argIdx < values.Length) // Still more arguments than expected
        {
            throw new Exception($"[{CmdToken} ERROR #{_config.LineNumber}] Too many arguments provided. Parsed {argIdx}, total {values.Length}.");
        }

        _config.ScreenGenerator.DrawBox(px, py, pwidth, pheight, borderColor, fillFlag, fillColor);

        if (drawImmediately)
        {
            _config.ScreenGenerator.DrawScreen();
        }
        return null;
    }
}