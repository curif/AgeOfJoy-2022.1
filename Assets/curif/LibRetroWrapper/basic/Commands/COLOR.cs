using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static OVRHaptics;

class CommandBGCOLOR : ICommandBase
{
    public string CmdToken { get; } = "BGCOLOR";
    public CommandType.Type Type { get; } = CommandType.Type.Command;
    protected ConfigurationCommands config;
    protected CommandExpressionList exprs;

    public CommandBGCOLOR(ConfigurationCommands config)
    {
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
        if (vals.Length > 1)
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
                throw new Exception($"{CmdToken} accepts a color name or and RGB color.");
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
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.ScreenGenerator == null)
        {
            return null;
        }
        BasicValue[] vals;

        vals = exprs.ExecuteList(vars);
        Color32 color = GetColorFromVals(vals);
        config.ScreenGenerator.ForegroundColor = color;

        return null;
    }
}

class CommandFGCOLOR : CommandBGCOLOR
{
    public string CmdToken { get; } = "FGCOLOR";

    public CommandFGCOLOR(ConfigurationCommands config) : base(config)
    {
    }

    public BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.ScreenGenerator == null)
        {
            return null;
        }
        BasicValue[] vals;

        vals = exprs.ExecuteList(vars);
        Color32 color = GetColorFromVals(vals);
        config.ScreenGenerator.BackgroundColor = color;

        return null;
    }
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
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.ScreenGenerator == null)
        {
            return null;
        }
        config.ScreenGenerator.ResetColors();
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
    }

    public bool Parse(TokenConsumer tokens)
    {
        expr.Parse(tokens);
        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
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
