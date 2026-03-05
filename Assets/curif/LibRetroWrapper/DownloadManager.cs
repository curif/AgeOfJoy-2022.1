using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using System;

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
        public DownloadStatus status;
        public float progress;

        public DownloadItem(string url, string savePath)
        {
            this.url = url;
            this.savePath = savePath;
            this.status = DownloadStatus.Idle;
            this.progress = 0f;
        }
    }

    private Queue<DownloadItem> downloadQueue = new Queue<DownloadItem>();
    private List<DownloadItem> completedDownloads = new List<DownloadItem>(); // Track completed/failed downloads
    private DownloadItem currentDownload = null;
    private bool isDownloading = false;
    private HttpClient httpClient;

    void Awake()
    {
        httpClient = new HttpClient();
        httpClient.Timeout = System.TimeSpan.FromMinutes(5);
    }

    void OnDestroy()
    {
        httpClient?.Dispose();
    }

    public void DownloadFile(string url, string savePath)
    {
        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(savePath))
        {
            Debug.LogError("URL or save path cannot be empty");
            return;
        }

        DownloadItem item = new DownloadItem(url, savePath);
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

    // New method to query status of a specific file
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

        // Check completed/failed downloads
        foreach (var item in completedDownloads)
        {
            if (item.url == identifier || item.savePath == identifier)
            {
                return (item.status, item.progress);
            }
        }

        return (DownloadStatus.NotFound, 0f);
    }

    private IEnumerator DownloadCoroutine()
    {
        isDownloading = true;

        while (downloadQueue.Count > 0 || currentDownload != null)
        {
            if (currentDownload == null)
            {
                currentDownload = downloadQueue.Dequeue();
                currentDownload.status = DownloadStatus.Downloading;

                Task downloadTask = DownloadFileAsync(currentDownload);
            }

            yield return null;

            if (currentDownload.status == DownloadStatus.Completed ||
                currentDownload.status == DownloadStatus.Failed)
            {
                completedDownloads.Add(currentDownload);
                currentDownload = null;
            }
        }

        isDownloading = false;
    }

    private async Task DownloadFileAsync(DownloadItem item)
    {
        try
        {
            if (item.url.StartsWith("file://"))
            {
                string filePath = item.url.Replace("file://", "");
                if (File.Exists(filePath))
                {
                    File.Copy(filePath, item.savePath, true);
                    item.progress = 1f;
                    item.status = DownloadStatus.Completed;
                }
                else
                {
                    throw new FileNotFoundException("Local file not found");
                }
            }
            else
            {
                using (HttpResponseMessage response = await httpClient.GetAsync(item.url, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    long totalBytes = response.Content.Headers.ContentLength ?? -1L;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(item.savePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        byte[] buffer = new byte[8192];
                        long bytesRead = 0;
                        int count;

                        while ((count = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, count);
                            bytesRead += count;

                            if (totalBytes > 0)
                            {
                                item.progress = (float)bytesRead / totalBytes;
                            }
                        }
                    }

                    item.progress = 1f;
                    item.status = DownloadStatus.Completed;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Download failed for {item.url}: {e.Message}");
            item.status = DownloadStatus.Failed;
            item.progress = 0f;
        }
    }
}