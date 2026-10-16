using System;
using System.IO;
using System.Linq;
using UnityEngine;

// GitHub releases (public repos, no login) and in-app apk install.
// Unauthenticated GitHub API limit: 60 requests per hour per IP, so don't call GHLOAD in a loop.
//
// GHLOAD(project, [count])            starts loading the releases ("owner/repo" or github URL), returns "owner/repo".
//                                     Deletes the apk of the installed version from the Download folder.
//                                     count: 1..100, default 10
// GHSTATUS(project)                   "LOADING" | "READY" | "NOTFOUND" | "RATELIMITED" | "FAILED" | "UNKNOWN"
// GHRELEASES(project)                 array with the release tags, newest first ("" if none)
// GHRELEASEINFO(project, tag, field)  "name" | "date" | "prerelease" | "notes" | "files" | "installed" |
//                                     "apk" | "url" | "size" | "sha256"  (the last four: of the release's apk, "" if none)
// GHFILES(project, tag, [wildcard])   array with the file names of the release ("" if none)
// GHFILEINFO(project, tag, file, field, [destFolder])  "name" | "size" | "url" | "sha256" | "downloaded" | "path"
// GHDOWNLOAD(project, tag, [file], [destFolder])  downloads a release file ("" or missing: the apk), sha256 verified.
//                                     Default destFolder: the shared Download folder. Returns the full path of the file.
//                                     Other versions of the same file in that folder are deleted (only one apk is kept).
// GHDOWNLOADSTATUS(project, tag, [file], [destFolder])    "QUEUED" | "DOWNLOADING" | "COMPLETED" | "FAILED" | "NOTFOUND"
// GHDOWNLOADPROGRESS(project, tag, [file], [destFolder])  0..100
// APKINSTALL(path)                    installs an AGE of Joy apk (another app's apk is refused). Android asks the user.
//                                     "STARTED" | "NOPERMISSION" (opens the "install unknown apps" screen) | "NOTFOUND" |
//                                     "WRONGPACKAGE" | "NOSPACE" | "BUSY"
// APKINSTALLSTATUS()                  "IDLE" | "COPYING" | "COMMITTING" | "WAITINGUSER" | "SUCCESS" | "ABORTED" | "CONFLICT" |
//                                     "STORAGE" | "INCOMPATIBLE" | "INVALID" | "BLOCKED" | "FAILED"
//                                     the app is closed by Android when the update is applied.
// APKINSTALLPROGRESS()                0..100 while copying the apk to the installer
// APPVERSION()                        installed version (the release tag, e.g. "0.5.0-rc31")

static class GHHelper
{
    public static string Project(string cmd, BasicValue val)
    {
        FunctionHelper.ExpectedNonEmptyString(val, "project");
        string project = GitHubReleases.NormalizeProject(val.GetValueAsString());
        if (string.IsNullOrEmpty(project))
            throw new Exception($"{cmd}: invalid github project '{val.GetValueAsString()}', use \"owner/repo\"");
        return project;
    }

    public static GHProject GetProject(DownloadManager dm, string cmd, BasicValue val)
    {
        string project = Project(cmd, val);
        GHProject result = dm.GetGitHubReleases(project);
        if (result == null)
            throw new Exception($"{cmd}: releases of {project} not loaded, use GHLOAD first");
        if (result.Status != GHStatus.Ready)
            throw new Exception($"{cmd}: releases of {project} are not available ({result.Status.ToString().ToUpperInvariant()})");
        return result;
    }

    public static GHRelease GetRelease(DownloadManager dm, string cmd, BasicValue[] vals)
    {
        GHProject project = GetProject(dm, cmd, vals[0]);
        FunctionHelper.ExpectedNonEmptyString(vals[1], "tag");
        GHRelease release = project.FindRelease(vals[1].GetValueAsString());
        if (release == null)
            throw new Exception($"{cmd}: release '{vals[1].GetValueAsString()}' not found in {project.Project}");
        return release;
    }

    // file argument at fileIndex; "" or missing: the release's apk.
    public static GHAsset GetAsset(string cmd, GHRelease release, BasicValue[] vals, int fileIndex)
    {
        string file = "";
        if (vals.Length > fileIndex)
        {
            FunctionHelper.ExpectedString(vals[fileIndex], "file");
            file = vals[fileIndex].GetValueAsString();
        }
        GHAsset asset = string.IsNullOrEmpty(file) ? release.Apk : release.FindAsset(file);
        if (asset == null)
            throw new Exception(string.IsNullOrEmpty(file)
                ? $"{cmd}: release {release.Tag} has no apk"
                : $"{cmd}: file '{file}' not found in release {release.Tag}");
        return asset;
    }

    // "<repo>-<tag>-<file>" so the files of different releases don't overwrite each other.
    public static string SavePath(string cmd, string project, GHRelease release, GHAsset asset, BasicValue[] vals, int destIndex)
    {
        string dest = IAFileHelper.DestFolder(cmd, vals, destIndex, ConfigManager.PublicDownloadDir);
        return Path.Combine(dest, SafeName($"{GitHubReleases.RepoName(project)}-{release.Tag}-{asset.Name}"));
    }

    static string SafeName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars().Concat(new[] { '/', '\\', ':' }))
            name = name.Replace(c, '_');
        return name;
    }

    public static string Hash(GHAsset asset) => string.IsNullOrEmpty(asset.Sha256) ? null : "sha256:" + asset.Sha256;

    // apks are > 1GB: keep only one version of a file. Deletes "<repo>-<other tag>-<asset>" (and leftover ".part")
    // next to keepPath, except downloads still queued or running.
    public static void DeleteOtherVersions(DownloadManager dm, string project, GHAsset asset, string keepPath)
    {
        string dir = Path.GetDirectoryName(keepPath);
        string prefix = SafeName(GitHubReleases.RepoName(project) + "-");
        string suffix = SafeName("-" + asset.Name);
        DeleteMatching(dm, dir, name => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                                        (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) ||
                                         name.EndsWith(suffix + ".part", StringComparison.OrdinalIgnoreCase)) &&
                                        !string.Equals(name, Path.GetFileName(keepPath), StringComparison.OrdinalIgnoreCase));
    }

    // the apk of the running version isn't needed anymore (installed). Only the default download folder.
    public static void DeleteInstalledApk(DownloadManager dm, string project)
    {
        string version = Application.version.TrimStart('v', 'V');
        string repo = GitHubReleases.RepoName(project);
        string[] prefixes = { SafeName($"{repo}-{version}-"), SafeName($"{repo}-v{version}-") };
        DeleteMatching(dm, ConfigManager.PublicDownloadDir, name => (name.EndsWith(".apk", StringComparison.OrdinalIgnoreCase) ||
                                                                     name.EndsWith(".apk.part", StringComparison.OrdinalIgnoreCase)) &&
                                        prefixes.Any(p => name.StartsWith(p, StringComparison.OrdinalIgnoreCase)));
    }

    static void DeleteMatching(DownloadManager dm, string dir, Func<string, bool> match)
    {
        try
        {
            if (!Directory.Exists(dir))
                return;
            foreach (string file in Directory.GetFiles(dir))
            {
                if (!match(Path.GetFileName(file)))
                    continue;
                // a ".part" belongs to the download of the file without it.
                string savePath = file.EndsWith(".part", StringComparison.OrdinalIgnoreCase) ? file.Substring(0, file.Length - 5) : file;
                var (status, _) = dm.GetFileStatus(savePath);
                if (status == DownloadManager.DownloadStatus.Idle || status == DownloadManager.DownloadStatus.Downloading)
                    continue;
                File.Delete(file);
                ConfigManager.WriteConsole($"[GitHub] deleted old download {file}");
            }
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleError($"[GitHub] can't delete old downloads in {dir}: {e.Message}");
        }
    }
}

class CommandFunctionGHLOAD : CommandFunctionExpressionListBase
{
    public CommandFunctionGHLOAD(ConfigurationCommands config) : base(config)
    {
        cmdToken = "GHLOAD";
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
        string project = GHHelper.Project(CmdToken, vals[0]);

        int count = GitHubReleases.DefaultCount;
        if (vals.Length > 1)
        {
            FunctionHelper.ExpectedNumber(vals[1], "count");
            count = Math.Max(1, Math.Min(GitHubReleases.MaxCount, (int)vals[1].GetValueAsNumber()));
        }

        GHHelper.DeleteInstalledApk(dm, project);
        dm.FetchGitHubReleases(project, count);
        return new BasicValue(project);
    }
}

class CommandFunctionGHSTATUS : CommandFunctionSingleExpressionBase
{
    public CommandFunctionGHSTATUS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "GHSTATUS";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        DownloadManager dm = DownloadFunctionsHelper.GetManager(config);
        GHProject project = dm.GetGitHubReleases(GHHelper.Project(CmdToken, expr.Execute(vars)));
        if (project == null)
            return new BasicValue("UNKNOWN");
        return new BasicValue(project.Status.ToString().ToUpperInvariant());
    }
}

class CommandFunctionGHRELEASES : CommandFunctionSingleExpressionBase
{
    public CommandFunctionGHRELEASES(ConfigurationCommands config) : base(config)
    {
        cmdToken = "GHRELEASES";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        DownloadManager dm = DownloadFunctionsHelper.GetManager(config);
        GHProject project = dm.GetGitHubReleases(GHHelper.Project(CmdToken, expr.Execute(vars)));
        if (project == null || project.Releases.Count == 0)
            return new BasicValue("");

        return new BasicValue(project.Releases.Select(r => r.Tag).ToArray(), BasicValue.BasicValueType.String);
    }
}

class CommandFunctionGHRELEASEINFO : CommandFunctionExpressionListBase
{
    public CommandFunctionGHRELEASEINFO(ConfigurationCommands config) : base(config)
    {
        cmdToken = "GHRELEASEINFO";
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
        GHRelease release = GHHelper.GetRelease(dm, CmdToken, vals);
        GHAsset apk = release.Apk;

        switch (vals[2].GetValueAsString().ToLowerInvariant())
        {
            case "name": return new BasicValue(release.Name);
            case "date": return new BasicValue(release.Date);
            case "prerelease": return new BasicValue(release.Prerelease);
            case "notes": return new BasicValue(release.Notes);
            case "files": return new BasicValue(release.Assets.Count);
            case "installed": return new BasicValue(GitHubReleases.SameVersion(release.Tag, Application.version));
            case "apk": return new BasicValue(apk?.Name ?? "");
            case "url": return new BasicValue(apk?.Url ?? "");
            case "size": return new BasicValue((double)(apk?.Size ?? 0));
            case "sha256": return new BasicValue(apk?.Sha256 ?? "");
            default:
                throw new Exception($"{CmdToken}: unknown field '{vals[2].GetValueAsString()}' (name, date, prerelease, notes, files, installed, apk, url, size, sha256)");
        }
    }
}

class CommandFunctionGHFILES : CommandFunctionExpressionListBase
{
    public CommandFunctionGHFILES(ConfigurationCommands config) : base(config)
    {
        cmdToken = "GHFILES";
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
        GHRelease release = GHHelper.GetRelease(dm, CmdToken, vals);
        string wildcard = "*";
        if (vals.Length > 2)
        {
            FunctionHelper.ExpectedString(vals[2], "wildcard");
            wildcard = vals[2].GetValueAsString();
        }

        string[] names = release.Assets.Select(a => a.Name).Where(n => InternetArchive.WildcardMatch(n, wildcard)).ToArray();
        if (names.Length == 0)
            return new BasicValue("");
        return new BasicValue(names, BasicValue.BasicValueType.String);
    }
}

class CommandFunctionGHFILEINFO : CommandFunctionExpressionListBase
{
    public CommandFunctionGHFILEINFO(ConfigurationCommands config) : base(config)
    {
        cmdToken = "GHFILEINFO";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 4);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        DownloadManager dm = DownloadFunctionsHelper.GetManager(config);
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[3], "field");
        GHRelease release = GHHelper.GetRelease(dm, CmdToken, vals);
        GHAsset asset = GHHelper.GetAsset(CmdToken, release, vals, 2);

        switch (vals[3].GetValueAsString().ToLowerInvariant())
        {
            case "name": return new BasicValue(asset.Name);
            case "size": return new BasicValue((double)asset.Size);
            case "url": return new BasicValue(asset.Url);
            case "sha256": return new BasicValue(asset.Sha256);
            case "path": return new BasicValue(GHHelper.SavePath(CmdToken, GHHelper.Project(CmdToken, vals[0]), release, asset, vals, 4));
            case "downloaded":
                return new BasicValue(dm.FileDownloadedState(
                    GHHelper.SavePath(CmdToken, GHHelper.Project(CmdToken, vals[0]), release, asset, vals, 4), GHHelper.Hash(asset)));
            default:
                throw new Exception($"{CmdToken}: unknown field '{vals[3].GetValueAsString()}' (name, size, url, sha256, path, downloaded)");
        }
    }
}

class CommandFunctionGHDOWNLOAD : CommandFunctionExpressionListBase
{
    public CommandFunctionGHDOWNLOAD(ConfigurationCommands config) : base(config)
    {
        cmdToken = "GHDOWNLOAD";
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
        GHRelease release = GHHelper.GetRelease(dm, CmdToken, vals);
        GHAsset asset = GHHelper.GetAsset(CmdToken, release, vals, 2);
        string path = GHHelper.SavePath(CmdToken, GHHelper.Project(CmdToken, vals[0]), release, asset, vals, 3);

        // old versions first: the space they free counts below.
        GHHelper.DeleteOtherVersions(dm, GHHelper.Project(CmdToken, vals[0]), asset, path);

        // the apk is big (> 1GB): fail now rather than at the end of the download.
        long free = ApkInstaller.FreeBytes(Path.GetDirectoryName(path));
        if (free >= 0 && asset.Size > 0 && free < asset.Size + asset.Size / 20 &&
            dm.FileDownloadedState(path, GHHelper.Hash(asset)) != 1)
            throw new Exception($"{CmdToken}: not enough free space for {asset.Name}: {asset.Size / 1048576} MB needed, {free / 1048576} MB free");

        // verified against github's sha256; skipped when that version is already downloaded.
        dm.DownloadFile(asset.Url, path, GHHelper.Hash(asset));
        return new BasicValue(path);
    }
}

class CommandFunctionGHDOWNLOADSTATUS : CommandFunctionExpressionListBase
{
    public CommandFunctionGHDOWNLOADSTATUS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "GHDOWNLOADSTATUS";
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
        GHRelease release = GHHelper.GetRelease(dm, CmdToken, vals);
        GHAsset asset = GHHelper.GetAsset(CmdToken, release, vals, 2);

        var (status, _) = dm.GetFileStatus(GHHelper.SavePath(CmdToken, GHHelper.Project(CmdToken, vals[0]), release, asset, vals, 3));
        return new BasicValue(DownloadFunctionsHelper.StatusString(status));
    }
}

class CommandFunctionGHDOWNLOADPROGRESS : CommandFunctionExpressionListBase
{
    public CommandFunctionGHDOWNLOADPROGRESS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "GHDOWNLOADPROGRESS";
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
        GHRelease release = GHHelper.GetRelease(dm, CmdToken, vals);
        GHAsset asset = GHHelper.GetAsset(CmdToken, release, vals, 2);

        var (_, progress) = dm.GetFileStatus(GHHelper.SavePath(CmdToken, GHHelper.Project(CmdToken, vals[0]), release, asset, vals, 3));
        return new BasicValue(progress * 100f); // percentage 0-100
    }
}

class CommandFunctionAPKINSTALL : CommandFunctionSingleExpressionBase
{
    public CommandFunctionAPKINSTALL(ConfigurationCommands config) : base(config)
    {
        cmdToken = "APKINSTALL";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        DownloadManager dm = DownloadFunctionsHelper.GetManager(config);
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNonEmptyString(val, "apk path");
        string path = Path.GetFullPath(val.GetValueAsString());

        // only apks downloaded by AGE of Joy: the shared Download folder or the app folder.
        if (!path.StartsWith(Path.GetFullPath(ConfigManager.PublicDownloadDir), StringComparison.Ordinal) &&
            !path.StartsWith(Path.GetFullPath(ConfigManager.BaseDir), StringComparison.Ordinal))
            throw new Exception($"{CmdToken}: {val.GetValueAsString()} must be in the Download folder or the AGE of Joy folder");

        if (!File.Exists(path))
            return new BasicValue("NOTFOUND");
        if (ApkInstaller.Busy)
            return new BasicValue("BUSY");

        // updates only: an AGEBasic script (maybe from a downloaded cabinet) can't install other apps.
        string package = ApkInstaller.ArchivePackage(path);
        if (!string.Equals(package, Application.identifier, StringComparison.Ordinal))
        {
            ConfigManager.WriteConsoleError($"[{CmdToken}] {path} package '{package}' is not {Application.identifier}");
            return new BasicValue("WRONGPACKAGE");
        }

        // the installer copies the apk into the data partition.
        long free = ApkInstaller.FreeBytes(ConfigManager.BaseAppDir);
        if (free >= 0 && free < new FileInfo(path).Length)
            return new BasicValue("NOSPACE");

        if (!ApkInstaller.CanInstall())
        {
            ApkInstaller.OpenInstallSettings();
            return new BasicValue("NOPERMISSION");
        }

        return new BasicValue(ApkInstaller.Install(dm, path) ? "STARTED" : "BUSY");
    }
}

class CommandFunctionAPKINSTALLSTATUS : CommandFunctionNoExpressionBase
{
    public CommandFunctionAPKINSTALLSTATUS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "APKINSTALLSTATUS";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");
        return new BasicValue(ApkInstaller.Status);
    }
}

class CommandFunctionAPKINSTALLPROGRESS : CommandFunctionNoExpressionBase
{
    public CommandFunctionAPKINSTALLPROGRESS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "APKINSTALLPROGRESS";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");
        return new BasicValue(ApkInstaller.Progress * 100f);
    }
}

class CommandFunctionAPPVERSION : CommandFunctionNoExpressionBase
{
    public CommandFunctionAPPVERSION(ConfigurationCommands config) : base(config)
    {
        cmdToken = "APPVERSION";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");
        return new BasicValue(Application.version);
    }
}
