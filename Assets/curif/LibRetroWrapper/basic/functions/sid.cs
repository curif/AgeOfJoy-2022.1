using System;

// ─────────────────────────────────────────────────────────────────────────────
// SIDSTATUS(name$)  → 0=not loaded · 1=ready · 2=playing · 3=paused
// ─────────────────────────────────────────────────────────────────────────────
class CommandFunctionSIDSTATUS : CommandFunctionSingleExpressionBase
{
    public CommandFunctionSIDSTATUS(ConfigurationCommands config) : base(config) { cmdToken = "SIDSTATUS"; }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");
        if (config.SIDPlayer == null) return new BasicValue(0);
        BasicValue nameVal = expr.Execute(vars);
        FunctionHelper.ExpectedString(nameVal, "- SID name");
        return new BasicValue(config.SIDPlayer.GetStatus(nameVal.GetString()));
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// SIDTITLE(name$)  → string
// ─────────────────────────────────────────────────────────────────────────────
class CommandFunctionSIDTITLE : CommandFunctionSingleExpressionBase
{
    public CommandFunctionSIDTITLE(ConfigurationCommands config) : base(config) { cmdToken = "SIDTITLE"; }

    public override BasicValue Execute(BasicVars vars)
    {
        if (config.SIDPlayer == null) return new BasicValue("");
        BasicValue nameVal = expr.Execute(vars);
        FunctionHelper.ExpectedString(nameVal, "- SID name");
        return new BasicValue(config.SIDPlayer.GetTitle(nameVal.GetString()));
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// SIDAUTHOR(name$)  → string
// ─────────────────────────────────────────────────────────────────────────────
class CommandFunctionSIDAUTHOR : CommandFunctionSingleExpressionBase
{
    public CommandFunctionSIDAUTHOR(ConfigurationCommands config) : base(config) { cmdToken = "SIDAUTHOR"; }

    public override BasicValue Execute(BasicVars vars)
    {
        if (config.SIDPlayer == null) return new BasicValue("");
        BasicValue nameVal = expr.Execute(vars);
        FunctionHelper.ExpectedString(nameVal, "- SID name");
        return new BasicValue(config.SIDPlayer.GetAuthor(nameVal.GetString()));
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// SIDRELEASED(name$)  → string
// ─────────────────────────────────────────────────────────────────────────────
class CommandFunctionSIDRELEASED : CommandFunctionSingleExpressionBase
{
    public CommandFunctionSIDRELEASED(ConfigurationCommands config) : base(config) { cmdToken = "SIDRELEASED"; }

    public override BasicValue Execute(BasicVars vars)
    {
        if (config.SIDPlayer == null) return new BasicValue("");
        BasicValue nameVal = expr.Execute(vars);
        FunctionHelper.ExpectedString(nameVal, "- SID name");
        return new BasicValue(config.SIDPlayer.GetReleased(nameVal.GetString()));
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// SIDCOUNT(name$)  → number of sub-songs
// ─────────────────────────────────────────────────────────────────────────────
class CommandFunctionSIDCOUNT : CommandFunctionSingleExpressionBase
{
    public CommandFunctionSIDCOUNT(ConfigurationCommands config) : base(config) { cmdToken = "SIDCOUNT"; }

    public override BasicValue Execute(BasicVars vars)
    {
        if (config.SIDPlayer == null) return new BasicValue(0);
        BasicValue nameVal = expr.Execute(vars);
        FunctionHelper.ExpectedString(nameVal, "- SID name");
        return new BasicValue(config.SIDPlayer.GetSongCount(nameVal.GetString()));
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// SIDDEFAULTSONG(name$)  → default sub-song index (1-based)
// ─────────────────────────────────────────────────────────────────────────────
class CommandFunctionSIDDEFAULTSONG : CommandFunctionSingleExpressionBase
{
    public CommandFunctionSIDDEFAULTSONG(ConfigurationCommands config) : base(config) { cmdToken = "SIDDEFAULTSONG"; }

    public override BasicValue Execute(BasicVars vars)
    {
        if (config.SIDPlayer == null) return new BasicValue(0);
        BasicValue nameVal = expr.Execute(vars);
        FunctionHelper.ExpectedString(nameVal, "- SID name");
        return new BasicValue(config.SIDPlayer.GetDefaultSong(nameVal.GetString()));
    }
}
