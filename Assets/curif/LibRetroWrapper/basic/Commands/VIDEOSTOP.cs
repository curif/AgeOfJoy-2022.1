// ─────────────────────────────────────────────────────────────────────────────
// VIDEOSTOP
//   Stop playback, rewind to position 0, and restore the game/CRT shader so
//   subsequent PRINT / DPSET / SHOW commands render normally.
//   No-op (with a warning) when VideoPlayer is null (AGEBasicCabinetController).
// ─────────────────────────────────────────────────────────────────────────────
class CommandVIDEOSTOP : CommandNoExpressionBase
{
    public CommandVIDEOSTOP(ConfigurationCommands config) : base(config) { cmdToken = "VIDEOSTOP"; }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");

        if (config.VideoPlayer == null)
        {
            ConfigManager.WriteConsoleWarning($"[{CmdToken}] VideoPlayer not available — command ignored.");
            return null;
        }

        config.VideoPlayer.Stop();

        // Restore the game/CRT shader so drawing commands work correctly after video.
        config.ScreenGenerator?.ActivateShader(config.GameShader);

        return null;
    }
}
