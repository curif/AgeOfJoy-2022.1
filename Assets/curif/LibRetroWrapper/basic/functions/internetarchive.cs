using System;
using System.IO;
using System.Linq;

// archive.org cabinet packs. Rules: docs/sharing_cabinets_archive_org.md
//
// IALOAD(idOrUrl)              starts loading the pack, returns its identifier
// IASTATUS(id)                 "LOADING" | "READY" | "PROCESSING" | "DARK" | "NOTFOUND" | "FAILED" | "UNKNOWN"
// IAINFO(id, field)            "title" | "description" | "count" | "identifier"
// IACABS(id)                   array with the cabinet zip names ("" if none)
// IACABINFO(id, cab, field)    "name" | "description" | "size" | "md5" | "url" | "downloaded"
// IADOWNLOAD(id, cab)          downloads the cabinet zip into the cabinets folder, returns the file name
// IADOWNLOADSTATUS(id, cab)    "QUEUED" | "DOWNLOADING" | "COMPLETED" | "FAILED" | "NOTFOUND"
// IADOWNLOADPROGRESS(id, cab)  0..100

class CommandFunctionIALOAD : CommandFunctionSingleExpressionBase
{
    public CommandFunctionIALOAD(ConfigurationCommands config) : base(config)
    {
        cmdToken = "IALOAD";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        DownloadManager dm = DownloadFunctionsHelper.GetManager(config);

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNonEmptyString(val, "identifier or URL");
        string id = InternetArchive.NormalizeIdentifier(val.GetValueAsString());
        if (string.IsNullOrEmpty(id))
            throw new Exception($"{CmdToken}: invalid archive.org identifier or URL");

        dm.FetchArchiveItem(id);
        return new BasicValue(id);
    }
}

class CommandFunctionIASTATUS : CommandFunctionSingleExpressionBase
{
    public CommandFunctionIASTATUS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "IASTATUS";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        DownloadManager dm = DownloadFunctionsHelper.GetManager(config);

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNonEmptyString(val, "identifier");
        IAItem item = dm.GetArchiveItem(InternetArchive.NormalizeIdentifier(val.GetValueAsString()));
        if (item == null)
            return new BasicValue("UNKNOWN");

        return new BasicValue(item.Status.ToString().ToUpperInvariant());
    }
}

class CommandFunctionIAINFO : CommandFunctionExpressionListBase
{
    public CommandFunctionIAINFO(ConfigurationCommands config) : base(config)
    {
        cmdToken = "IAINFO";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        DownloadManager dm = DownloadFunctionsHelper.GetManager(config);

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[0], "identifier");
        FunctionHelper.ExpectedNonEmptyString(vals[1], "field");

        IAItem item = dm.GetArchiveItem(InternetArchive.NormalizeIdentifier(vals[0].GetValueAsString()));
        if (item == null)
            throw new Exception($"{CmdToken}: pack not loaded, use IALOAD first");

        switch (vals[1].GetValueAsString().ToLowerInvariant())
        {
            case "title": return new BasicValue(item.Title);
            case "description": return new BasicValue(item.Description);
            case "count": return new BasicValue(item.Cabinets.Count);
            case "identifier": return new BasicValue(item.Identifier);
            default:
                throw new Exception($"{CmdToken}: unknown field '{vals[1].GetValueAsString()}' (title, description, count, identifier)");
        }
    }
}

class CommandFunctionIACABS : CommandFunctionSingleExpressionBase
{
    public CommandFunctionIACABS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "IACABS";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        DownloadManager dm = DownloadFunctionsHelper.GetManager(config);

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNonEmptyString(val, "identifier");
        IAItem item = dm.GetArchiveItem(InternetArchive.NormalizeIdentifier(val.GetValueAsString()));
        if (item == null || item.Cabinets.Count == 0)
            return new BasicValue("");

        return new BasicValue(item.Cabinets.Select(c => c.FileName).ToArray(), BasicValue.BasicValueType.String);
    }
}

class CommandFunctionIACABINFO : CommandFunctionExpressionListBase
{
    public CommandFunctionIACABINFO(ConfigurationCommands config) : base(config)
    {
        cmdToken = "IACABINFO";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 3);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        DownloadManager dm = DownloadFunctionsHelper.GetManager(config);

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[0], "identifier");
        FunctionHelper.ExpectedNonEmptyString(vals[1], "cabinet");
        FunctionHelper.ExpectedNonEmptyString(vals[2], "field");

        IAItem item = dm.GetArchiveItem(InternetArchive.NormalizeIdentifier(vals[0].GetValueAsString()));
        if (item == null)
            throw new Exception($"{CmdToken}: pack not loaded, use IALOAD first");

        IACabinet cab = item.FindCabinet(vals[1].GetValueAsString());
        if (cab == null)
            throw new Exception($"{CmdToken}: cabinet '{vals[1].GetValueAsString()}' not found in {item.Identifier}");

        switch (vals[2].GetValueAsString().ToLowerInvariant())
        {
            case "name": return new BasicValue(cab.Name);
            case "description": return new BasicValue(cab.Description);
            case "size": return new BasicValue((double)cab.Size);
            case "md5": return new BasicValue(cab.Md5);
            case "url": return new BasicValue(cab.Url);
            case "downloaded": return new BasicValue(dm.DownloadedState(cab));
            default:
                throw new Exception($"{CmdToken}: unknown field '{vals[2].GetValueAsString()}' (name, description, size, md5, url, downloaded)");
        }
    }
}

// Shared by the IADOWNLOAD* functions: finds the cabinet of a loaded pack.
static class IADownloadHelper
{
    public static IACabinet GetCabinet(DownloadManager dm, string cmd, BasicValue[] vals)
    {
        FunctionHelper.ExpectedNonEmptyString(vals[0], "identifier");
        FunctionHelper.ExpectedNonEmptyString(vals[1], "cabinet");

        IAItem item = dm.GetArchiveItem(InternetArchive.NormalizeIdentifier(vals[0].GetValueAsString()));
        if (item == null)
            throw new Exception($"{cmd}: pack not loaded, use IALOAD first");
        if (item.Status != IAStatus.Ready && item.Status != IAStatus.Processing)
            throw new Exception($"{cmd}: pack {item.Identifier} is not available ({item.Status.ToString().ToUpperInvariant()})");

        IACabinet cab = item.FindCabinet(vals[1].GetValueAsString());
        if (cab == null)
            throw new Exception($"{cmd}: cabinet '{vals[1].GetValueAsString()}' not found in {item.Identifier}");
        return cab;
    }

    public static string SavePath(IACabinet cab) => Path.Combine(ConfigManager.Cabinets, cab.FileName);
}

class CommandFunctionIADOWNLOAD : CommandFunctionExpressionListBase
{
    public CommandFunctionIADOWNLOAD(ConfigurationCommands config) : base(config)
    {
        cmdToken = "IADOWNLOAD";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        DownloadManager dm = DownloadFunctionsHelper.GetManager(config);
        IACabinet cab = IADownloadHelper.GetCabinet(dm, CmdToken, exprs.ExecuteList(vars));

        // verified against the pack's md5; skipped when that version is already downloaded.
        dm.DownloadFile(cab.Url, IADownloadHelper.SavePath(cab), cab.Md5);
        return new BasicValue(cab.FileName);
    }
}

class CommandFunctionIADOWNLOADSTATUS : CommandFunctionExpressionListBase
{
    public CommandFunctionIADOWNLOADSTATUS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "IADOWNLOADSTATUS";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        DownloadManager dm = DownloadFunctionsHelper.GetManager(config);
        IACabinet cab = IADownloadHelper.GetCabinet(dm, CmdToken, exprs.ExecuteList(vars));

        var (status, _) = dm.GetFileStatus(IADownloadHelper.SavePath(cab));

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

class CommandFunctionIADOWNLOADPROGRESS : CommandFunctionExpressionListBase
{
    public CommandFunctionIADOWNLOADPROGRESS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "IADOWNLOADPROGRESS";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        DownloadManager dm = DownloadFunctionsHelper.GetManager(config);
        IACabinet cab = IADownloadHelper.GetCabinet(dm, CmdToken, exprs.ExecuteList(vars));

        var (_, progress) = dm.GetFileStatus(IADownloadHelper.SavePath(cab));
        return new BasicValue(progress * 100f); // percentage 0-100
    }
}
