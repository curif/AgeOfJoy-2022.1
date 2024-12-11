using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

class ChangeColorsBase : ICommandBase
{
    public string CmdToken { get { return cmdToken; } }
    string cmdToken;

    public CommandType.Type Type { get; } = CommandType.Type.Command;
    protected ConfigurationCommands config;
    protected CommandExpressionList exprs;

    public ChangeColorsBase(ConfigurationCommands config,
                                            string token)
    {
        cmdToken = token;
        this.config = config;
        exprs = new(config);
    }

    public bool Parse(TokenConsumer tokens)
    {
        exprs.Parse(tokens);
        return true;
    }

    public Color32 GetColorFromVals(BasicValue[] vals)
    {

        if (vals == null || vals.Length == 0)
        {
            throw new Exception($"{CmdToken} parameteres missing.");
        }
        Color32 color;
        if (vals.Length > 2)
        {
            FunctionHelper.ExpectedNumber(vals[0], "- R");
            FunctionHelper.ExpectedNumber(vals[1], "- G");
            FunctionHelper.ExpectedNumber(vals[2], "- B");
            color = ColorConverter.ConvertToColor(vals[0], vals[1], vals[2]);
        }
        else
        {
            if (!vals[0].IsString())
            {
                throw new Exception($"{CmdToken} accepts a color name or a RGB color.");
            }
            else
            {
                color = config.ScreenGenerator.GetColorSpace().GetColorByName(vals[0].GetString());
            }
        }
        return color;
    }

    public BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}  #{config.LineNumber}] ");
        if (config?.ScreenGenerator == null)
        {
            return null;
        }
        BasicValue[] vals;

        vals = exprs.ExecuteList(vars);
        Color32 color = GetColorFromVals(vals);
        if (CmdToken == "BGCOLOR")
            config.ScreenGenerator.BackgroundColor = color;
        else
            config.ScreenGenerator.ForegroundColor = color;

        return null;
    }
}


class CommandBGCOLOR : ChangeColorsBase
{
    public CommandBGCOLOR(ConfigurationCommands config) : base(config, "BGCOLOR") { } 
}

class CommandFGCOLOR : ChangeColorsBase
{
    public CommandFGCOLOR(ConfigurationCommands config) : base(config, "FGCOLOR") { }

}

class CommandRESETCOLOR : ICommandBase
{
    public string CmdToken { get; } = "RESETCOLOR";
    public CommandType.Type Type { get; } = CommandType.Type.Command;
    protected ConfigurationCommands config;

    public bool Parse(TokenConsumer tokens)
    {
        return true;
    }

    public CommandRESETCOLOR(ConfigurationCommands config)
    {
        this.config = config;
    }

    public BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");
        if (config?.ScreenGenerator == null)
        {
            return null;
        }
        config.ScreenGenerator.ResetColors();
        return null;
    }
}

class CommandINVERTCOLOR : ICommandBase
{
    public string CmdToken { get; } = "INVERTCOLOR";
    public CommandType.Type Type { get; } = CommandType.Type.Command;
    protected ConfigurationCommands config;

    public bool Parse(TokenConsumer tokens)
    {
        return true;
    }

    public CommandINVERTCOLOR(ConfigurationCommands config)
    {
        this.config = config;
    }

    public BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");
        if (config?.ScreenGenerator == null)
            return null;

        config.ScreenGenerator.InvertColors();
        return null;
    }
}

class CommandSETCOLORSPACE : ICommandBase
{
    public string CmdToken { get; } = "SETCOLORSPACE";
    public CommandType.Type Type { get; } = CommandType.Type.Command;
    protected ConfigurationCommands config;
    CommandExpression expr;

    public CommandSETCOLORSPACE(ConfigurationCommands config)
    {
        this.config = config;
        expr = new(config);

    }

    public bool Parse(TokenConsumer tokens)
    {
        expr.Parse(tokens);
        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");
        if (config?.ScreenGenerator == null)
        {
            return null;
        }
        BasicValue colorSpaceName = expr.Execute(vars);
        FunctionHelper.ExpectedString(colorSpaceName, "- A valid color space name is expected.");

        config.ScreenGenerator.SetColorSpace(colorSpaceName.GetString());
        return null;
    }
}
