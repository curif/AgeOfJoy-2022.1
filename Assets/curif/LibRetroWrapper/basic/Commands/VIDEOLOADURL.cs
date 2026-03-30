using System;

// ─────────────────────────────────────────────────────────────────────────────
// VIDEOLOADURL "url" [, invertX [, invertY]]
//   Assigns an HTTP/HTTPS stream URL to the cabinet's video player without
//   starting playback. Supports direct .mp4/.mkv links and HLS (.m3u8) streams
//   served by media servers (Plex, Jellyfin, DLNA, etc.) on the local network
//   or the internet.
//
//   Unlike VIDEOLOAD, no file-system traversal check is performed — the URL is
//   passed directly to Unity's VideoPlayer (Android MediaPlayer underneath).
//   Only http:// and https:// schemes are accepted.
//
//   invertX / invertY default to false when omitted.
//   No-op (with a warning) when VideoPlayer is null (AGEBasicCabinetController).
//
//   Example:
//     VIDEOLOADURL "http://192.168.1.10:8096/stream/movie.m3u8"
//     VIDEOPLAY
// ─────────────────────────────────────────────────────────────────────────────
class CommandVIDEOLOADURL : CommandExpressionListBase
{
    public CommandVIDEOLOADURL(ConfigurationCommands config) : base(config)
    {
        cmdToken = "VIDEOLOADURL";
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
        FunctionHelper.ExpectedNonEmptyString(vals[0], "- URL must be a non-empty string");

        string url = vals[0].GetValueAsString().Trim();
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            throw new Exception($"[{CmdToken}] Only http:// and https:// URLs are accepted. Got: {url}");

        bool invertX = vals.Length > 1 && vals[1].GetValueAsNumber() != 0;
        bool invertY = vals.Length > 2 && vals[2].GetValueAsNumber() != 0;

        config.VideoPlayer.ChangeVideo(url, invertX, invertY);
        return null;
    }
}
