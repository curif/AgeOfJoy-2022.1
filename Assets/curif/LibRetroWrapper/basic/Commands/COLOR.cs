using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

class ChangeColorsBase : CommandExpressionListBase
{

    public ChangeColorsBase(ConfigurationCommands config, string token) : base(config)
    {
        cmdToken = token;
        this.config = config;
        exprs = new(config);
    }

    public override bool Parse(TokenConsumer tokens)
    {
        base.Parse(tokens, 1);
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

    public void CheckConfigRequirements(ConfigurationCommands config)
    {
        if (config?.ScreenGenerator == null)
            throw new ArgumentException($"[{CmdToken} ScreenGenerator is not available.");
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}  #{config.LineNumber}] ");

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

class CommandRESETCOLOR : CommandNoExpressionBase // Changed base class
{
    public CommandRESETCOLOR(ConfigurationCommands config) : base(config) // Call base constructor
    {
        this.cmdToken = "RESETCOLOR"; // Set CmdToken
    }

    public void CheckConfigRequirements(ConfigurationCommands config)
    {
        if (config?.ScreenGenerator == null)
            throw new ArgumentException($"[{CmdToken} ScreenGenerator is not available.");
    }
    public override BasicValue Execute(BasicVars vars) // Override Execute
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");

        config.ScreenGenerator.ResetColors();
        return null;
    }
}

class CommandINVERTCOLOR : CommandNoExpressionBase // Changed base class
{
    public CommandINVERTCOLOR(ConfigurationCommands config) : base(config) // Call base constructor
    {
        this.cmdToken = "INVERTCOLOR"; // Set CmdToken
    }
    public void CheckConfigRequirements(ConfigurationCommands config)
    {
        if (config?.ScreenGenerator == null)
            throw new ArgumentException($"[{CmdToken} ScreenGenerator is not available.");
    }
    public override BasicValue Execute(BasicVars vars) // Override Execute
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");

        config.ScreenGenerator.InvertColors();
        return null;
    }
}

class CommandSETCOLORSPACE : CommandSingleExpressionBase // Changed base class
{
    public CommandSETCOLORSPACE(ConfigurationCommands config) : base(config) // Call base constructor
    {
        this.cmdToken = "SETCOLORSPACE"; // Set CmdToken
    }

    public void CheckConfigRequirements(ConfigurationCommands config)
    {
        if (config?.ScreenGenerator == null)
            throw new ArgumentException($"[{CmdToken} ScreenGenerator is not available.");
    }

    public override BasicValue Execute(BasicVars vars) // Override Execute
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");

        // expr is now a protected member from CommandSingleExpressionBase
        BasicValue colorSpaceName = expr.Execute(vars);

        FunctionHelper.ExpectedString(colorSpaceName, "- A valid color space name is expected.");

        config.ScreenGenerator.SetColorSpace(colorSpaceName.GetString());
        return null;
    }
}