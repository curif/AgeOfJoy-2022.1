// ─────────────────────────────────────────────────────────────────────────────
// VIDEOPLAY
//   Start or resume playback. Activates the video shader on the cabinet screen.
//   No-op (with a warning) when VideoPlayer is null (AGEBasicCabinetController).
// ─────────────────────────────────────────────────────────────────────────────
class CommandVIDEOPLAY : CommandNoExpressionBase
{
    public CommandVIDEOPLAY(ConfigurationCommands config) : base(config) { cmdToken = "VIDEOPLAY"; }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");

        if (config.VideoPlayer == null)
        {
            ConfigManager.WriteConsoleWarning($"[{CmdToken}] VideoPlayer not available — command ignored.");
            return null;
        }

        config.VideoPlayer.Play();
        return null;
    }
}
