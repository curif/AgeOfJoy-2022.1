using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// Installs a cabinet zip from cabinets/ into cabinetsdb/<name> on a background thread,
// then registers it in GameRegistry. Used by CABDBINSTALL / CABDBINSTALLSTATUS.
// Start() and Status() are called from the main thread only (AGEBasic runs there);
// only the unzip runs on the worker, and the registration is done by the first
// Status() call that sees the job finished.
public static class CabinetInstaller
{
    public enum InstallStatus { NotFound, Installing, Installed, Failed }

    private class Job
    {
        public string Name;
        public Task<string> Task;
        public bool Finalized;
        public InstallStatus Result;
    }

    private static readonly Dictionary<string, Job> jobs = new(StringComparer.OrdinalIgnoreCase);

    // "galaga" or "galaga.zip" -> "galaga"
    public static string NormalizeName(string name)
    {
        name = name?.Trim() ?? "";
        if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            name = name.Substring(0, name.Length - 4);
        return name;
    }

    // Throws when the cabinet can't be installed. Returns false if it was already being installed.
    public static bool Start(string name)
    {
        name = NormalizeName(name);
        if (string.IsNullOrEmpty(name))
            throw new Exception("cabinet name is empty");
        if (name.IndexOfAny(new[] { '/', '\\' }) >= 0 || name.Contains(".."))
            throw new Exception($"invalid cabinet name '{name}'");
        if (string.Equals(name, "test", StringComparison.OrdinalIgnoreCase))
            throw new Exception("'test' is reserved for the workshop");

        if (jobs.TryGetValue(name, out Job running) && !running.Task.IsCompleted)
            return false;

        string zipPath = Path.Combine(ConfigManager.Cabinets, name + ".zip");
        if (!File.Exists(zipPath))
            throw new Exception($"cabinet zip not found: {zipPath}");
        if (!CabinetDBAdmin.ZipFileContainsDescriptionYaml(zipPath))
            throw new Exception($"{name}.zip doesn't have a description.yaml");

        string pathDest = CabinetDBAdmin.CabinetDestPath(zipPath);
        ConfigManager.WriteConsole($"[CabinetInstaller] installing {zipPath} into {pathDest}");

        jobs[name] = new Job
        {
            Name = name,
            Task = Task.Run(() => CabinetDBAdmin.ExtractCabinetZip(zipPath, pathDest))
        };
        return true;
    }

    public static InstallStatus Status(string name, GameRegistry registry)
    {
        if (!jobs.TryGetValue(NormalizeName(name), out Job job))
            return InstallStatus.NotFound;
        if (!job.Task.IsCompleted)
            return InstallStatus.Installing;
        if (job.Finalized)
            return job.Result;

        job.Finalized = true;
        if (job.Task.IsFaulted || job.Task.IsCanceled)
        {
            ConfigManager.WriteConsoleException($"[CabinetInstaller] ERROR installing {job.Name}",
                                                job.Task.Exception?.GetBaseException() ?? new Exception("canceled"));
            job.Result = InstallStatus.Failed;
            return job.Result;
        }

        try
        {
            registry?.AddNewCabinetDirectory(job.Name);
            ConfigManager.WriteConsole($"[CabinetInstaller] {job.Name} installed in {job.Task.Result}");
            job.Result = InstallStatus.Installed;
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[CabinetInstaller] ERROR registering {job.Name}", e);
            job.Result = InstallStatus.Failed;
        }
        return job.Result;
    }
}
