// ─────────────────────────────────────────────────────────────────────────────
// VIDEOSEEK seconds
//   Seek to an absolute position (in seconds) within the loaded video.
//   Has no effect if the video is not yet prepared.
//   No-op (with a warning) when VideoPlayer is null (AGEBasicCabinetController).
// ─────────────────────────────────────────────────────────────────────────────
class CommandVIDEOSEEK : CommandSingleExpressionBase
{
    public CommandVIDEOSEEK(ConfigurationCommands config) : base(config) { cmdToken = "VIDEOSEEK"; }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");

        if (config.VideoPlayer == null)
        {
            ConfigManager.WriteConsoleWarning($"[{CmdToken}] VideoPlayer not available — command ignored.");
            return null;
        }

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNumber(val, "- seek position must be a number (seconds)");

        config.VideoPlayer.SeekTo(val.GetValueAsNumber());
        return null;
    }
}
