using System;

// Shared by the IA* (internetarchive.cs) and GH* (github.cs) functions.
static class DownloadFunctionsHelper
{
    public static DownloadManager GetManager(ConfigurationCommands config)
    {
        DownloadManager dm = config.DownloadManager;
        if (dm == null)
        {
            dm = DownloadManager.Get();
            config.DownloadManager = dm;
        }
        if (dm == null)
            throw new Exception("Download manager doesn't exist");
        return dm;
    }

    public static string StatusString(DownloadManager.DownloadStatus status) => status switch
    {
        DownloadManager.DownloadStatus.Idle => "QUEUED",
        DownloadManager.DownloadStatus.Downloading => "DOWNLOADING",
        DownloadManager.DownloadStatus.Completed => "COMPLETED",
        DownloadManager.DownloadStatus.Failed => "FAILED",
        DownloadManager.DownloadStatus.NotFound => "NOTFOUND",
        _ => "unknown" // Shouldn't happen
    };
}
