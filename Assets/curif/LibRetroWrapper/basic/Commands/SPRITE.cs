using System;
using System.IO;

class CommandSPRITELOAD : CommandExpressionListBase
{
    public CommandSPRITELOAD(ConfigurationCommands config) : base(config)
    {
        this.cmdToken = "SPRITELOAD";
        this.MinCantParamsRequired = 2;
    }

    public void CheckConfigRequirements(ConfigurationCommands config)
    {
        if (config?.ScreenGenerator == null)
            throw new ArgumentException($"[{CmdToken}] ScreenGenerator is not available.");
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");

        BasicValue[] values = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(values[0], $"- Sprite Name for {CmdToken}");
        FunctionHelper.ExpectedString(values[1], $"- File Path for {CmdToken}");

        string name = values[0].GetString();
        string path = values[1].GetString();

        // Need to combine path properly if it's relative
        if (!Path.IsPathRooted(path) && !string.IsNullOrEmpty(config.ProgramPath))
        {
            path = Path.Combine(config.ProgramPath, path);
        }

        if (File.Exists(path))
        {
            config.ScreenGenerator.LoadSprite(name, path);
        }
        else
        {
            throw new Exception($"[{CmdToken} ERROR #{config.LineNumber}] Sprite file not found: {path}");
        }

        return null;
    }
}

class CommandSPRITE : CommandExpressionListBase
{
    public CommandSPRITE(ConfigurationCommands config) : base(config)
    {
        this.cmdToken = "SPRITE";
        this.MinCantParamsRequired = 4;
    }

    public void CheckConfigRequirements(ConfigurationCommands config)
    {
        if (config?.ScreenGenerator == null)
            throw new ArgumentException($"[{CmdToken}] ScreenGenerator is not available.");
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");

        BasicValue[] values = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(values[0], $"- Sprite Name for {CmdToken}");
        FunctionHelper.ExpectedNumber(values[1], $"- X coordinate for {CmdToken}");
        FunctionHelper.ExpectedNumber(values[2], $"- Y coordinate for {CmdToken}");
        FunctionHelper.ExpectedNumber(values[3], $"- Z layer for {CmdToken}");

        string name = values[0].GetString();
        int x = values[1].GetInt();
        int y = values[2].GetInt();
        int z = values[3].GetInt();

        config.ScreenGenerator.DrawSprite(name, x, y, z);

        return null;
    }
}

class CommandSPRITEREMOVE : CommandExpressionListBase
{
    public CommandSPRITEREMOVE(ConfigurationCommands config) : base(config)
    {
        this.cmdToken = "SPRITEREMOVE";
        this.MinCantParamsRequired = 1;
    }

    public void CheckConfigRequirements(ConfigurationCommands config)
    {
        if (config?.ScreenGenerator == null)
            throw new ArgumentException($"[{CmdToken}] ScreenGenerator is not available.");
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");

        BasicValue[] values = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(values[0], $"- Sprite Name for {CmdToken}");

        string name = values[0].GetString();
        config.ScreenGenerator.RemoveSprite(name);

        return null;
    }
}

class CommandFunctionSPRITECOLLISIONCOUNT : CommandFunctionExpressionListBase
{
    public CommandFunctionSPRITECOLLISIONCOUNT(ConfigurationCommands config) : base(config)
    {
        cmdToken = "SPRITECOLLISIONCOUNT";
        MinCantParamsRequired = 2;
    }

    public override BasicValue Execute(BasicVars vars)
    {
        BasicValue[] args = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(args[0], $"{cmdToken} - first sprite name must be a string");
        FunctionHelper.ExpectedString(args[1], $"{cmdToken} - second sprite name must be a string");

        if (config.ScreenGenerator == null)
            return new BasicValue(0);

        var pairs = config.ScreenGenerator.GetLastCollisionPairs(args[0].GetString(), args[1].GetString());
        return new BasicValue(pairs.Count);
    }
}

class CommandFunctionSPRITECOLLISIONDATA : CommandFunctionExpressionListBase
{
    public CommandFunctionSPRITECOLLISIONDATA(ConfigurationCommands config) : base(config)
    {
        cmdToken = "SPRITECOLLISIONDATA";
        MinCantParamsRequired = 2;
    }

    public override BasicValue Execute(BasicVars vars)
    {
        BasicValue[] args = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(args[0], $"{cmdToken} - first sprite name must be a string");
        FunctionHelper.ExpectedString(args[1], $"{cmdToken} - second sprite name must be a string");

        var data = new System.Collections.Generic.List<object>();
        if (config.ScreenGenerator != null)
        {
            var pairs = config.ScreenGenerator.GetLastCollisionPairs(args[0].GetString(), args[1].GetString());
            foreach (var pair in pairs)
            {
                data.Add((double)pair.ColA);
                data.Add((double)pair.RowA);
                data.Add((double)pair.ColB);
                data.Add((double)pair.RowB);
            }
        }
        return new BasicValue(data);
    }
}

class CommandFunctionSPRITESTATUS : CommandFunctionSingleExpressionBase
{
    public CommandFunctionSPRITESTATUS(ConfigurationCommands config) : base(config)
    {
        this.cmdToken = "SPRITESTATUS";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val, $"- Sprite Name for {CmdToken}");

        string name = val.GetString();
        bool isLoaded = config.ScreenGenerator.IsSpriteLoaded(name);

        return new BasicValue(isLoaded ? 1 : 0);
    }
}
