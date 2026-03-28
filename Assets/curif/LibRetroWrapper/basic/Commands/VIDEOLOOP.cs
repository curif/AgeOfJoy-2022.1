// ─────────────────────────────────────────────────────────────────────────────
// VIDEOLOOP expr
//   Enable (1) or disable (0) video looping.
//   Takes effect immediately if a video is already playing.
//   No-op (with a warning) when VideoPlayer is null (AGEBasicCabinetController).
// ─────────────────────────────────────────────────────────────────────────────
class CommandVIDEOLOOP : CommandSingleExpressionBase
{
    public CommandVIDEOLOOP(ConfigurationCommands config) : base(config) { cmdToken = "VIDEOLOOP"; }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");

        if (config.VideoPlayer == null)
        {
            ConfigManager.WriteConsoleWarning($"[{CmdToken}] VideoPlayer not available — command ignored.");
            return null;
        }

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNumber(val, "- loop flag must be a number (1=on, 0=off)");

        config.VideoPlayer.SetLoop(val.GetValueAsNumber() != 0);
        return null;
    }
}
