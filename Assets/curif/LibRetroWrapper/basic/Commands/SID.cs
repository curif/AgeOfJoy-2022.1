using System;

// ─────────────────────────────────────────────────────────────────────────────
// SIDLOAD name$, path$
// ─────────────────────────────────────────────────────────────────────────────
class CommandSIDLOAD : CommandExpressionListBase
{
    public CommandSIDLOAD(ConfigurationCommands config) : base(config) { cmdToken = "SIDLOAD"; }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 2);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");
        if (config.SIDPlayer == null)
            throw new Exception("SIDLOAD: SIDPlayer not available (cabinet requires an AudioSource).");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0], "- SID name must be a string");
        FunctionHelper.ExpectedString(vals[1], "- SID file path must be a string");

        config.SIDPlayer.Load(vals[0].GetString(), vals[1].GetString());
        return null;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// SIDLOADDATA name$, storage$
//   Loads a SID from bytes previously written via DATA "storage$", byte, byte, ...
// ─────────────────────────────────────────────────────────────────────────────
class CommandSIDLOADDATA : CommandExpressionListBase
{
    public CommandSIDLOADDATA(ConfigurationCommands config) : base(config) { cmdToken = "SIDLOADDATA"; }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 2);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");
        if (config.SIDPlayer == null)
            throw new Exception("SIDLOADDATA: SIDPlayer not available.");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0], "- SID name must be a string");
        FunctionHelper.ExpectedString(vals[1], "- DATA storage name must be a string");

        string storageName = vals[1].GetString();
        if (!config.basicValueLists.ContainsKey(storageName))
            throw new Exception($"SIDLOADDATA: storage '{storageName}' not found. Use DATA to fill it first.");

        BasicValueList list = config.basicValueLists[storageName];
        list.Reset();
        byte[] bytes = new byte[list.Count()];
        for (int i = 0; i < bytes.Length && !list.EOF(); i++)
        {
            bytes[i] = (byte)list.CurrentValue().GetInt();
            list.Next();
        }

        config.SIDPlayer.LoadFromBytes(vals[0].GetString(), bytes);
        return null;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// SIDPLAY name$ [, song]
//   song is 1-based; omit to use the file's default song.
// ─────────────────────────────────────────────────────────────────────────────
class CommandSIDPLAY : CommandExpressionListBase
{
    public CommandSIDPLAY(ConfigurationCommands config) : base(config)
    {
        cmdToken = "SIDPLAY";
        MinCantParamsRequired = 1;
    }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 1);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");
        if (config.SIDPlayer == null)
            throw new Exception("SIDPLAY: SIDPlayer not available.");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0], "- SID name must be a string");

        int song = vals.Length > 1 ? vals[1].GetInt() : 0;
        config.SIDPlayer.Play(vals[0].GetString(), song);
        return null;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// SIDSTOP name$
// ─────────────────────────────────────────────────────────────────────────────
class CommandSIDSTOP : CommandSingleExpressionBase
{
    public CommandSIDSTOP(ConfigurationCommands config) : base(config) { cmdToken = "SIDSTOP"; }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");
        if (config.SIDPlayer == null) return null;
        BasicValue nameVal = expr.Execute(vars);
        FunctionHelper.ExpectedString(nameVal, "- SID name must be a string");
        config.SIDPlayer.Stop(nameVal.GetString());
        return null;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// SIDPAUSE name$
// ─────────────────────────────────────────────────────────────────────────────
class CommandSIDPAUSE : CommandSingleExpressionBase
{
    public CommandSIDPAUSE(ConfigurationCommands config) : base(config) { cmdToken = "SIDPAUSE"; }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");
        if (config.SIDPlayer == null) return null;
        BasicValue nameVal = expr.Execute(vars);
        FunctionHelper.ExpectedString(nameVal, "- SID name must be a string");
        config.SIDPlayer.Pause(nameVal.GetString());
        return null;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// SIDRESUME name$
// ─────────────────────────────────────────────────────────────────────────────
class CommandSIDRESUME : CommandSingleExpressionBase
{
    public CommandSIDRESUME(ConfigurationCommands config) : base(config) { cmdToken = "SIDRESUME"; }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");
        if (config.SIDPlayer == null) return null;
        BasicValue nameVal = expr.Execute(vars);
        FunctionHelper.ExpectedString(nameVal, "- SID name must be a string");
        config.SIDPlayer.Resume(nameVal.GetString());
        return null;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// SIDUNLOAD name$
// ─────────────────────────────────────────────────────────────────────────────
class CommandSIDUNLOAD : CommandSingleExpressionBase
{
    public CommandSIDUNLOAD(ConfigurationCommands config) : base(config) { cmdToken = "SIDUNLOAD"; }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");
        if (config.SIDPlayer == null) return null;
        BasicValue nameVal = expr.Execute(vars);
        FunctionHelper.ExpectedString(nameVal, "- SID name must be a string");
        config.SIDPlayer.Unload(nameVal.GetString());
        return null;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// SIDVOLUME name$, vol   (vol is 0-100)
// ─────────────────────────────────────────────────────────────────────────────
class CommandSIDVOLUME : CommandExpressionListBase
{
    public CommandSIDVOLUME(ConfigurationCommands config) : base(config) { cmdToken = "SIDVOLUME"; }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 2);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");
        if (config.SIDPlayer == null) return null;

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0], "- SID name must be a string");
        FunctionHelper.ExpectedNumber(vals[1], "- volume must be a number (0-100)");

        config.SIDPlayer.SetVolume(vals[0].GetString(), (float)vals[1].GetNumber() / 100f);
        return null;
    }
}
