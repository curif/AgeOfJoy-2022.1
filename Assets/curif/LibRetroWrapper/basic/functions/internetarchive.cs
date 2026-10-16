using System;
using System.IO;
using System.Linq;

// archive.org cabinet packs. Rules: docs/sharing_cabinets_archive_org.md
//
// IALOAD(idOrUrl)              starts loading the pack, returns its identifier
// IASEARCH(query)              starts an archive.org search ("" = packs tagged subject "ageofjoy-cabinets"), returns the handle
// IAPACKS(handle)              array with the pack identifiers found, newest first ("" if none)
// IASTATUS(id|handle)          "LOADING" | "READY" | "PROCESSING" | "DARK" | "NOTFOUND" | "FAILED" | "UNKNOWN"
// IAINFO(id, field)            "title" | "description" | "count" | "identifier"
// IACABS(id)                   array with the cabinet zip names ("" if none)
// IACABINFO(id, cab, field)    "name" | "description" | "size" | "md5" | "url" | "downloaded"
// IAFILES(id, [wildcard])      array with the names of any file of the item ("" if none), e.g. "*.zip", "roms/*"
// IAFILEINFO(id, file, field, [destFolder])  "name" | "size" | "md5" | "url" | "format" | "mtime" | "downloaded"
// IAFILEDOWNLOAD(id, file, [destFolder])     downloads any file into destFolder (default: DOWNLOADSPATH()), returns the file name
// IAFILEDOWNLOADSTATUS(id, file, [destFolder])    like IADOWNLOADSTATUS
// IAFILEDOWNLOADPROGRESS(id, file, [destFolder])  0..100
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

        // a search handle (see IASEARCH) never looks like an identifier: it has ':' or spaces.
        IASearch search = dm.GetArchiveSearch(val.GetValueAsString());
        if (search != null)
            return new BasicValue(search.Status.ToString().ToUpperInvariant());

        IAItem item = dm.GetArchiveItem(InternetArchive.NormalizeIdentifier(val.GetValueAsString()));
        if (item == null)
            return new BasicValue("UNKNOWN");

        return new BasicValue(item.Status.ToString().ToUpperInvariant());
    }
}

class CommandFunctionIASEARCH : CommandFunctionSingleExpressionBase
{
    public CommandFunctionIASEARCH(ConfigurationCommands config) : base(config)
    {
        cmdToken = "IASEARCH";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        DownloadManager dm = DownloadFunctionsHelper.GetManager(config);

        string query = expr.Execute(vars).GetValueAsString().Trim();
        if (query.Length == 0)
            query = InternetArchive.DefaultSearch;

        dm.FetchArchiveSearch(query);
        return new BasicValue(query);
    }
}

class CommandFunctionIAPACKS : CommandFunctionSingleExpressionBase
{
    public CommandFunctionIAPACKS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "IAPACKS";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        DownloadManager dm = DownloadFunctionsHelper.GetManager(config);

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNonEmptyString(val, "search");
        IASearch search = dm.GetArchiveSearch(val.GetValueAsString());
        if (search == null || search.Identifiers.Count == 0)
            return new BasicValue("");

        return new BasicValue(search.Identifiers.ToArray(), BasicValue.BasicValueType.String);
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
            case "files": return new BasicValue(item.Files.Count);
            case "identifier": return new BasicValue(item.Identifier);
            default:
                throw new Exception($"{CmdToken}: unknown field '{vals[1].GetValueAsString()}' (title, description, count, files, identifier)");
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


// Shared by the IAFILE* functions: finds any file of a loaded item and where it is saved.
static class IAFileHelper
{
    public static IAFile GetFile(DownloadManager dm, string cmd, BasicValue[] vals)
    {
        FunctionHelper.ExpectedNonEmptyString(vals[0], "identifier");
        FunctionHelper.ExpectedNonEmptyString(vals[1], "file");

        IAItem item = dm.GetArchiveItem(InternetArchive.NormalizeIdentifier(vals[0].GetValueAsString()));
        if (item == null)
            throw new Exception($"{cmd}: pack not loaded, use IALOAD first");
        if (item.Status != IAStatus.Ready && item.Status != IAStatus.Processing)
            throw new Exception($"{cmd}: pack {item.Identifier} is not available ({item.Status.ToString().ToUpperInvariant()})");

        IAFile file = item.FindFile(vals[1].GetValueAsString());
        if (file == null)
            throw new Exception($"{cmd}: file '{vals[1].GetValueAsString()}' not found in {item.Identifier}");
        return file;
    }

    // destFolder is the optional argument at destIndex; default is the downloads folder.
    // Subfolders of the item are flattened: "roms/x.zip" is saved as <dest>/x.zip
    public static string SavePath(string cmd, IAFile file, BasicValue[] vals, int destIndex)
    {
        string dest = ConfigManager.RomsDir;
        if (vals.Length > destIndex)
        {
            FunctionHelper.ExpectedString(vals[destIndex], "destination folder");
            if (!string.IsNullOrEmpty(vals[destIndex].GetValueAsString()))
                dest = vals[destIndex].GetValueAsString();
        }
        dest = FunctionHelper.FileTraversalFree(dest, ConfigManager.BaseDir);

        // cabinets have IADOWNLOAD; a script must not drop native cores or change the configuration.
        foreach (string forbidden in new[] { ConfigManager.Cabinets, ConfigManager.CabinetsDB, ConfigManager.CoresDir,
                                             ConfigManager.ConfigDir, ConfigManager.SystemDir })
        {
            string f = Path.GetFullPath(forbidden).TrimEnd('/', '\\');
            if (dest.Equals(f, StringComparison.OrdinalIgnoreCase) ||
                dest.StartsWith(f + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                dest.StartsWith(f + "/", StringComparison.OrdinalIgnoreCase))
                throw new Exception($"{cmd}: destination folder {vals[destIndex].GetValueAsString()} is not allowed, use IADOWNLOAD for cabinets");
        }
        return Path.Combine(dest, Path.GetFileName(file.Name));
    }
}

class CommandFunctionIAFILES : CommandFunctionExpressionListBase
{
    public CommandFunctionIAFILES(ConfigurationCommands config) : base(config)
    {
        cmdToken = "IAFILES";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 1);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        DownloadManager dm = DownloadFunctionsHelper.GetManager(config);

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[0], "identifier");
        string wildcard = "*";
        if (vals.Length >= 2)
        {
            FunctionHelper.ExpectedString(vals[1], "wildcard");
            wildcard = vals[1].GetValueAsString();
        }

        IAItem item = dm.GetArchiveItem(InternetArchive.NormalizeIdentifier(vals[0].GetValueAsString()));
        if (item == null)
            return new BasicValue("");

        string[] names = item.Files.Select(f => f.Name).Where(n => InternetArchive.WildcardMatch(n, wildcard)).ToArray();
        if (names.Length == 0)
            return new BasicValue("");

        return new BasicValue(names, BasicValue.BasicValueType.String);
    }
}

class CommandFunctionIAFILEINFO : CommandFunctionExpressionListBase
{
    public CommandFunctionIAFILEINFO(ConfigurationCommands config) : base(config)
    {
        cmdToken = "IAFILEINFO";
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
        FunctionHelper.ExpectedNonEmptyString(vals[2], "field");
        IAFile file = IAFileHelper.GetFile(dm, CmdToken, vals);

        switch (vals[2].GetValueAsString().ToLowerInvariant())
        {
            case "name": return new BasicValue(file.Name);
            case "size": return new BasicValue((double)file.Size);
            case "md5": return new BasicValue(file.Md5);
            case "url": return new BasicValue(file.Url);
            case "format": return new BasicValue(file.Format);
            case "mtime": return new BasicValue(file.Mtime);
            case "downloaded": return new BasicValue(dm.FileDownloadedState(IAFileHelper.SavePath(CmdToken, file, vals, 3), file.Md5));
            default:
                throw new Exception($"{CmdToken}: unknown field '{vals[2].GetValueAsString()}' (name, size, md5, url, format, mtime, downloaded)");
        }
    }
}

class CommandFunctionIAFILEDOWNLOAD : CommandFunctionExpressionListBase
{
    public CommandFunctionIAFILEDOWNLOAD(ConfigurationCommands config) : base(config)
    {
        cmdToken = "IAFILEDOWNLOAD";
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
        IAFile file = IAFileHelper.GetFile(dm, CmdToken, vals);

        // verified against the item's md5; skipped when that version is already downloaded.
        dm.DownloadFile(file.Url, IAFileHelper.SavePath(CmdToken, file, vals, 2), file.Md5);
        return new BasicValue(Path.GetFileName(file.Name));
    }
}

class CommandFunctionIAFILEDOWNLOADSTATUS : CommandFunctionExpressionListBase
{
    public CommandFunctionIAFILEDOWNLOADSTATUS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "IAFILEDOWNLOADSTATUS";
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
        IAFile file = IAFileHelper.GetFile(dm, CmdToken, vals);

        var (status, _) = dm.GetFileStatus(IAFileHelper.SavePath(CmdToken, file, vals, 2));

        string statusString = status switch
        {
            DownloadManager.DownloadStatus.Idle => "QUEUED",
            DownloadManager.DownloadStatus.Downloading => "DOWNLOADING",
            DownloadManager.DownloadStatus.Completed => "COMPLETED",
            DownloadManager.DownloadStatus.Failed => "FAILED",
            DownloadManager.DownloadStatus.NotFound => "NOTFOUND",
            _ => "unknown"
        };
        return new BasicValue(statusString);
    }
}

class CommandFunctionIAFILEDOWNLOADPROGRESS : CommandFunctionExpressionListBase
{
    public CommandFunctionIAFILEDOWNLOADPROGRESS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "IAFILEDOWNLOADPROGRESS";
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
        IAFile file = IAFileHelper.GetFile(dm, CmdToken, vals);

        var (_, progress) = dm.GetFileStatus(IAFileHelper.SavePath(CmdToken, file, vals, 2));
        return new BasicValue(progress * 100f); // percentage 0-100
    }
}
