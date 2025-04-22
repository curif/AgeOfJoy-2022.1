using System;
using System.IO;

/*
class CommandFunctionDOWNLOAD : CommandFunctionExpressionListBase
{
    public CommandFunctionDOWNLOAD(ConfigurationCommands config) : base(config)
    {
        cmdToken = "DOWNLOAD";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        // Expecting 2 parameters: URL and save path
        return base.Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        if (config.DownloadManager == null)
            throw new Exception("Download manager doesn't exist");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[0], "URL");
        FunctionHelper.ExpectedNonEmptyString(vals[1], "save path");

        string url = vals[0].GetValueAsString();
        string savePath = vals[1].GetValueAsString();

        config.DownloadManager.DownloadFile(url, savePath);

        return new BasicValue(1);
    }
}

class CommandFunctionDOWNLOADSTATUS : CommandFunctionSingleExpressionBase
{
    public CommandFunctionDOWNLOADSTATUS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "DOWNLOADSTATUS";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        if (config.DownloadManager == null)
            throw new Exception("Download manager doesn't exist");

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNonEmptyString(val, "URL or save path");
        string identifier = val.GetValueAsString();

        var (status, _) = config.DownloadManager.GetFileStatus(identifier);

        string statusString = status switch
        {
            DownloadManager.DownloadStatus.Idle => "QUEUED",
            DownloadManager.DownloadStatus.Downloading => "DOWNLOADING",
            DownloadManager.DownloadStatus.Completed => "COMPLETED",
            DownloadManager.DownloadStatus.Failed => "FAILED",
            DownloadManager.DownloadStatus.NotFound => "NOTFOUND",
            _ => "unknown" // Shouldn't happen
        };

        return new BasicValue(statusString);
    }
}

class CommandFunctionDOWNLOADPROGRESS : CommandFunctionSingleExpressionBase
{
    public CommandFunctionDOWNLOADPROGRESS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "DOWNLOADPROGRESS";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        if (config.DownloadManager == null)
            throw new Exception("Download manager doesn't exist");

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNonEmptyString(val, "URL or save path");
        string identifier = val.GetValueAsString();

        // If the identifier doesn't contain :// assume it's a filename in ConfigManager.Cabinets
        if (!identifier.Contains("://"))
        {
            identifier = Path.Combine(ConfigManager.Cabinets, identifier);
        }

        var (_, progress) = config.DownloadManager.GetFileStatus(identifier);

        // Return progress as percentage (0-100)
        return new BasicValue(progress * 100f);
    }
}
*/