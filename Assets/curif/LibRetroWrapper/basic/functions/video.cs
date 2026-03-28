// ─────────────────────────────────────────────────────────────────────────────
// AGEBasic video query functions
//   VIDEOTIME()     → current playback position in seconds
//   VIDEODURATION() → total duration in seconds
//   VIDEOSTATUS()   → 0=not loaded · 1=stopped/ready · 2=playing · 3=paused
// ─────────────────────────────────────────────────────────────────────────────

class CommandFunctionVIDEOTIME : CommandFunctionNoExpressionBase
{
    public CommandFunctionVIDEOTIME(ConfigurationCommands config) : base(config) { cmdToken = "VIDEOTIME"; }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");
        if (config.VideoPlayer == null)
        {
            ConfigManager.WriteConsoleWarning($"[{CmdToken}] VideoPlayer not available — returning 0.");
            return new BasicValue(0);
        }
        return new BasicValue(config.VideoPlayer.GetCurrentTime());
    }
}

class CommandFunctionVIDEODURATION : CommandFunctionNoExpressionBase
{
    public CommandFunctionVIDEODURATION(ConfigurationCommands config) : base(config) { cmdToken = "VIDEODURATION"; }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");
        if (config.VideoPlayer == null)
        {
            ConfigManager.WriteConsoleWarning($"[{CmdToken}] VideoPlayer not available — returning 0.");
            return new BasicValue(0);
        }
        return new BasicValue(config.VideoPlayer.GetDuration());
    }
}

class CommandFunctionVIDEOSTATUS : CommandFunctionNoExpressionBase
{
    public CommandFunctionVIDEOSTATUS(ConfigurationCommands config) : base(config) { cmdToken = "VIDEOSTATUS"; }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");
        if (config.VideoPlayer == null)
        {
            ConfigManager.WriteConsoleWarning($"[{CmdToken}] VideoPlayer not available — returning 0.");
            return new BasicValue(0);
        }
        return new BasicValue(config.VideoPlayer.GetStatus());
    }
}

class CommandFunctionVIDEOLOOPSTATUS : CommandFunctionNoExpressionBase
{
    public CommandFunctionVIDEOLOOPSTATUS(ConfigurationCommands config) : base(config) { cmdToken = "VIDEOLOOPSTATUS"; }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");
        if (config.VideoPlayer == null)
        {
            ConfigManager.WriteConsoleWarning($"[{CmdToken}] VideoPlayer not available — returning 0.");
            return new BasicValue(0);
        }
        return new BasicValue(config.VideoPlayer.GetLoopStatus() ? 1 : 0);
    }
}
