using System;

// ─────────────────────────────────────────────────────────────────────────────
// VIDEOLOAD "path" [, invertX, invertY]
//   Assigns a new video file to the cabinet's video player without playing it.
//   invertX / invertY default to false when omitted.
//   No-op (with a warning) when VideoPlayer is null (AGEBasicCabinetController).
// ─────────────────────────────────────────────────────────────────────────────
class CommandVIDEOLOAD : CommandExpressionListBase
{
    public CommandVIDEOLOAD(ConfigurationCommands config) : base(config)
    {
        cmdToken = "VIDEOLOAD";
        MinCantParamsRequired = 1;
    }

    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 1);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");

        if (config.VideoPlayer == null)
        {
            ConfigManager.WriteConsoleWarning($"[{CmdToken}] VideoPlayer not available — command ignored.");
            return null;
        }

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[0], "- video path must be a non-empty string");

        string path = vals[0].GetValueAsString();
        bool invertX = vals.Length > 1 && vals[1].GetValueAsNumber() != 0;
        bool invertY = vals.Length > 2 && vals[2].GetValueAsNumber() != 0;

        config.VideoPlayer.ChangeVideo(path, invertX, invertY);
        return null;
    }
}
