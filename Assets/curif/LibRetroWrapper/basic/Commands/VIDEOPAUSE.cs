// ─────────────────────────────────────────────────────────────────────────────
// VIDEOPAUSE
//   Pause at the current position. The last video frame stays on screen.
//   Does not switch shaders.
//   No-op (with a warning) when VideoPlayer is null (AGEBasicCabinetController).
// ─────────────────────────────────────────────────────────────────────────────
class CommandVIDEOPAUSE : CommandNoExpressionBase
{
    public CommandVIDEOPAUSE(ConfigurationCommands config) : base(config) { cmdToken = "VIDEOPAUSE"; }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");

        if (config.VideoPlayer == null)
        {
            ConfigManager.WriteConsoleWarning($"[{CmdToken}] VideoPlayer not available — command ignored.");
            return null;
        }

        config.VideoPlayer.Pause();
        config.ScreenGenerator?.ActivateShader(config.GameShader);
        return null;
    }
}
