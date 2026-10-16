using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System;

// Sequential file downloader used by the DOWNLOAD* AGEBasic functions, plus an archive.org item cache
// used by the IA* functions.
// Downloads go to "<savePath>.part" and are moved to savePath only when complete (and md5 verified),
// so the cabinets loader never sees a partial zip.
public class DownloadManager : MonoBehaviour
{
    public enum DownloadStatus
    {
        Idle,
        Downloading,
        Completed,
        Failed,
        NotFound
    }

    private class DownloadItem
    {
        public string url;
        public string savePath;
        public string expectedMd5;
        public DownloadStatus status;
        public float progress;

        public DownloadItem(string url, string savePath, string expectedMd5)
        {
            this.url = url;
            this.savePath = savePath;
            this.expectedMd5 = string.IsNullOrEmpty(expectedMd5) ? null : expectedMd5.ToLowerInvariant();
            this.status = DownloadStatus.Idle;
            this.progress = 0f;
        }
    }

    private const string Md5RecordFile = "internetarchive_md5.txt";

    private Queue<DownloadItem> downloadQueue = new Queue<DownloadItem>();
    private List<DownloadItem> completedDownloads = new List<DownloadItem>(); // Track completed/failed downloads
    private DownloadItem currentDownload = null;
    private bool isDownloading = false;

    private Dictionary<string, IAItem> archiveItems = new Dictionary<string, IAItem>();
    private Dictionary<string, IASearch> archiveSearches = new Dictionary<string, IASearch>();
    // cabinet file name ("galaga.zip") -> md5 of the last verified download
    private Dictionary<string, string> downloadedMd5 = null;

    // The component lives in the room's FixedObject; created on demand when the scene doesn't have it.
    public static DownloadManager Get()
    {
        GameObject root = GameObject.Find("FixedObject");
        if (root == null)
            return null;
        DownloadManager dm = root.GetComponent<DownloadManager>();
        if (dm == null)
            dm = root.AddComponent<DownloadManager>();
        return dm;
    }

    public void DownloadFile(string url, string savePath, string expectedMd5 = null)
    {
        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(savePath))
        {
            Debug.LogError("URL or save path cannot be empty");
            return;
        }

        // same destination already queued or in progress
        if ((currentDownload != null && currentDownload.savePath == savePath) ||
            downloadQueue.Any(i => i.savePath == savePath))
            return;

        DownloadItem item = new DownloadItem(url, savePath, expectedMd5);

        // already downloaded with the same md5: nothing to download.
        if (item.expectedMd5 != null && IsSavePathDownloadedWithMd5(savePath, item.expectedMd5))
        {
            item.status = DownloadStatus.Completed;
            item.progress = 1f;
            completedDownloads.Add(item);
            return;
        }

        downloadQueue.Enqueue(item);

        if (!isDownloading)
        {
            StartCoroutine(DownloadCoroutine());
        }
    }

    // Query current download status
    public (DownloadStatus status, float progress) GetDownloadStatus()
    {
        if (currentDownload == null)
        {
            return (DownloadStatus.Idle, 0f);
        }
        return (currentDownload.status, currentDownload.progress);
    }

    // Query status of a specific file, by url or by save path.
    public (DownloadStatus status, float progress) GetFileStatus(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return (DownloadStatus.NotFound, 0f);
        }

        // Check current download
        if (currentDownload != null &&
            (currentDownload.url == identifier || currentDownload.savePath == identifier))
        {
            return (currentDownload.status, currentDownload.progress);
        }

        // Check queue
        foreach (var item in downloadQueue)
        {
            if (item.url == identifier || item.savePath == identifier)
            {
                return (item.status, item.progress);
            }
        }

        // Check completed/failed downloads, newest first: a repeated download reports its latest result.
        for (int i = completedDownloads.Count - 1; i >= 0; i--)
        {
            var item = completedDownloads[i];
            if (item.url == identifier || item.savePath == identifier)
            {
                return (item.status, item.progress);
            }
        }

        return (DownloadStatus.NotFound, 0f);
    }

    // ---- archive.org items ------------------------------------------------------------------

    // Starts (or restarts) the metadata fetch. Poll GetArchiveItem(id).Status.
    public void FetchArchiveItem(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
            return;
        archiveItems[identifier] = new IAItem { Identifier = identifier, Status = IAStatus.Loading };
        StartCoroutine(FetchArchiveItemCoroutine(identifier));
    }

    public IAItem GetArchiveItem(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
            return null;
        archiveItems.TryGetValue(identifier, out IAItem item);
        return item;
    }

    private IEnumerator FetchArchiveItemCoroutine(string identifier)
    {
        string json = null;
        yield return GetTextWithRetry(InternetArchive.MetadataUrl(identifier), $"metadata of {identifier}", t => json = t);
        archiveItems[identifier] = json != null
            ? InternetArchive.Parse(identifier, json)
            : new IAItem { Identifier = identifier, Status = IAStatus.Failed };
    }

    // Starts (or restarts) an advanced search. Poll GetArchiveSearch(query).Status.
    public void FetchArchiveSearch(string query)
    {
        if (string.IsNullOrEmpty(query))
            return;
        archiveSearches[query] = new IASearch { Query = query, Status = IAStatus.Loading };
        StartCoroutine(FetchArchiveSearchCoroutine(query));
    }

    public IASearch GetArchiveSearch(string query)
    {
        if (string.IsNullOrEmpty(query))
            return null;
        archiveSearches.TryGetValue(query, out IASearch search);
        return search;
    }

    private IEnumerator FetchArchiveSearchCoroutine(string query)
    {
        string json = null;
        yield return GetTextWithRetry(InternetArchive.SearchUrl(query), $"search {query}", t => json = t);
        IASearch result = json != null
            ? InternetArchive.ParseSearch(query, json)
            : new IASearch { Query = query, Status = IAStatus.Failed };
        if (result.NumFound > result.Identifiers.Count)
            Debug.LogWarning($"archive.org search {query}: {result.NumFound} found, only {result.Identifiers.Count} read");
        archiveSearches[query] = result;
    }

    // archive.org is sometimes slow or answers 5xx: retry a few times before giving up. onDone gets null on failure.
    private IEnumerator GetTextWithRetry(string url, string what, Action<string> onDone)
    {
        const int attempts = 3;
        for (int attempt = 1; attempt <= attempts; attempt++)
        {
            bool retry;
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.timeout = 60;
                www.SetRequestHeader("User-Agent", "AgeOfJoy");
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    onDone(www.downloadHandler.text);
                    yield break;
                }
                Debug.LogError($"archive.org {what} failed (attempt {attempt}/{attempts}): {www.error} (HTTP {www.responseCode})");
                // 4xx won't get better by retrying.
                retry = www.responseCode == 0 || www.responseCode >= 500;
            }
            if (!retry)
                break;
            if (attempt < attempts)
                yield return new WaitForSeconds(2f * attempt);
        }
        onDone(null);
    }

    // 0: not downloaded, 1: downloaded and md5 matches the item's, 2: downloaded but different/unknown (update available).
    public int DownloadedState(IACabinet cab)
    {
        bool present = Directory.Exists(Path.Combine(ConfigManager.CabinetsDB, cab.Name)) ||
                       File.Exists(Path.Combine(ConfigManager.Cabinets, cab.FileName));
        if (!present)
            return 0;
        return IsDownloadedWithMd5(cab.FileName, cab.Md5) ? 1 : 2;
    }

    // ---- downloaded md5 record ---------------------------------------------------------------

    // Key of a download in the md5 record. Files saved directly in the cabinets folder keep the bare file
    // name (compatible with existing records); anything else is keyed by its path relative to the base
    // dir ("downloads/x.zip"), so it can never collide with a cabinet.
    private static string RecordKey(string savePath)
    {
        string full = Path.GetFullPath(savePath);
        string dir = Path.GetDirectoryName(full);
        if (string.Equals(dir, Path.GetFullPath(ConfigManager.Cabinets), StringComparison.Ordinal))
            return Path.GetFileName(full);
        string baseDir = Path.GetFullPath(ConfigManager.BaseDir).TrimEnd('/', '\\');
        string rel = full.StartsWith(baseDir, StringComparison.Ordinal) ? full.Substring(baseDir.Length) : full;
        return rel.TrimStart('/', '\\').Replace('\\', '/');
    }

    private bool IsCabinetSavePath(string savePath) =>
        !RecordKey(savePath).Contains("/");

    // cabinets: present (decompressed or pending) with the md5 recorded. Other files: present with the md5 recorded.
    private bool IsSavePathDownloadedWithMd5(string savePath, string md5)
    {
        if (IsCabinetSavePath(savePath))
            return IsDownloadedWithMd5(Path.GetFileName(savePath), md5);
        if (string.IsNullOrEmpty(md5) || !File.Exists(savePath))
            return false;
        return LoadDownloadedMd5().TryGetValue(RecordKey(savePath), out string recorded) &&
               string.Equals(recorded, md5, StringComparison.OrdinalIgnoreCase);
    }

    // 0: not downloaded, 1: downloaded and md5 matches, 2: downloaded but different/unknown (update available).
    public int FileDownloadedState(string savePath, string md5)
    {
        if (!File.Exists(savePath))
            return 0;
        return IsSavePathDownloadedWithMd5(savePath, md5) ? 1 : 2;
    }

    private string Md5RecordPath => Path.Combine(ConfigManager.ConfigDir, Md5RecordFile);

    private Dictionary<string, string> LoadDownloadedMd5()
    {
        if (downloadedMd5 != null)
            return downloadedMd5;
        downloadedMd5 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            if (File.Exists(Md5RecordPath))
            {
                foreach (string line in File.ReadAllLines(Md5RecordPath))
                {
                    string[] parts = line.Split('\t');
                    if (parts.Length == 2 && parts[0].Length > 0)
                        downloadedMd5[parts[0]] = parts[1];
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Can't read {Md5RecordFile}: {e.Message}");
        }
        return downloadedMd5;
    }

    private void RecordDownloadedMd5(string fileName, string md5)
    {
        var map = LoadDownloadedMd5();
        map[fileName] = md5;
        try
        {
            File.WriteAllLines(Md5RecordPath, map.Select(kv => kv.Key + "\t" + kv.Value));
        }
        catch (Exception e)
        {
            Debug.LogError($"Can't write {Md5RecordFile}: {e.Message}");
        }
    }

    // True if the cabinet is present (decompressed or pending) and the last verified download had this md5.
    private bool IsDownloadedWithMd5(string fileName, string md5)
    {
        if (string.IsNullOrEmpty(md5))
            return false;
        string name = Path.GetFileNameWithoutExtension(fileName);
        bool present = Directory.Exists(Path.Combine(ConfigManager.CabinetsDB, name)) ||
                       File.Exists(Path.Combine(ConfigManager.Cabinets, fileName));
        if (!present)
            return false;
        return LoadDownloadedMd5().TryGetValue(fileName, out string recorded) &&
               string.Equals(recorded, md5, StringComparison.OrdinalIgnoreCase);
    }

    private static string ComputeMd5(string path)
    {
        using (var md5 = MD5.Create())
        using (var stream = File.OpenRead(path))
        {
            return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }
    }

    // ---- download loop ----------------------------------------------------------------------

    private IEnumerator DownloadCoroutine()
    {
        isDownloading = true;

        while (downloadQueue.Count > 0)
        {
            currentDownload = downloadQueue.Dequeue();
            currentDownload.status = DownloadStatus.Downloading;

            yield return DownloadItemCoroutine(currentDownload);

            completedDownloads.Add(currentDownload);
            currentDownload = null;
        }

        isDownloading = false;
    }

    private IEnumerator DownloadItemCoroutine(DownloadItem item)
    {
        string partPath = item.savePath + ".part";
        string error = null;

        try
        {
            string dir = Path.GetDirectoryName(item.savePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            if (File.Exists(partPath))
                File.Delete(partPath);
        }
        catch (Exception e)
        {
            error = e.Message;
        }

        if (error == null)
        {
            if (item.url.StartsWith("file://"))
            {
                try
                {
                    string filePath = item.url.Replace("file://", "");
                    if (!File.Exists(filePath))
                        throw new FileNotFoundException("Local file not found");
                    File.Copy(filePath, partPath, true);
                }
                catch (Exception e)
                {
                    error = e.Message;
                }
            }
            else
            {
                // archive.org: if the main url fails with a server error, try the item's storage servers directly.
                var urls = new List<string> { item.url };
                if (InternetArchive.TryParseDownloadUrl(item.url, out string identifier, out string archiveFile))
                    urls.AddRange(InternetArchive.MirrorUrls(GetArchiveItem(identifier), archiveFile));

                foreach (string url in urls)
                {
                    error = null;
                    using (UnityWebRequest www = UnityWebRequest.Get(url))
                    {
                        www.downloadHandler = new DownloadHandlerFile(partPath) { removeFileOnAbort = true };
                        www.SetRequestHeader("User-Agent", "AgeOfJoy");
                        www.timeout = 0; // large files: no overall timeout
                        UnityWebRequestAsyncOperation op = www.SendWebRequest();
                        while (!op.isDone)
                        {
                            item.progress = www.downloadProgress;
                            yield return null;
                        }
                        if (www.result != UnityWebRequest.Result.Success)
                            error = $"{www.error} (HTTP {www.responseCode}) {url}";
                        if (error == null || www.responseCode < 500)
                            break; // success, or an error that another server won't fix
                    }
                    Debug.LogWarning($"Download attempt failed: {error}");
                }
            }
        }

        // md5 on a worker thread: don't hitch the frame on big zips.
        string md5 = null;
        if (error == null)
        {
            Task<string> md5Task = Task.Run(() => ComputeMd5(partPath));
            while (!md5Task.IsCompleted)
                yield return null;
            if (md5Task.IsFaulted)
                error = md5Task.Exception.GetBaseException().Message;
            else
                md5 = md5Task.Result;
        }

        if (error == null && item.expectedMd5 != null && md5 != item.expectedMd5)
            error = $"md5 mismatch, expected {item.expectedMd5} got {md5}";

        if (error == null)
        {
            try
            {
                if (File.Exists(item.savePath))
                    File.Delete(item.savePath);
                File.Move(partPath, item.savePath);
                RecordDownloadedMd5(RecordKey(item.savePath), md5);
            }
            catch (Exception e)
            {
                error = e.Message;
            }
        }

        if (error == null)
        {
            item.progress = 1f;
            item.status = DownloadStatus.Completed;
        }
        else
        {
            Debug.LogError($"Download failed for {item.url}: {error}");
            try
            {
                if (File.Exists(partPath))
                    File.Delete(partPath);
            }
            catch (Exception) { }
            item.status = DownloadStatus.Failed;
            item.progress = 0f;
        }
    }
}
