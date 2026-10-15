using System;

// Shared by the IA* functions (internetarchive.cs).
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
}
