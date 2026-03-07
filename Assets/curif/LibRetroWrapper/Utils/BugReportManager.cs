using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BugReportManager : MonoBehaviour
{
    public static BugReportManager Instance { get; private set; }

    private StreamWriter logWriter;
    private bool isDebugMode = false;
    private string logFilePath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (isDebugMode)
        {
            ToggleDebugMode(false);
        }
    }

    /// <summary>
    /// Toggles the recording of Unity's global log output to a file.
    /// </summary>
    public void ToggleDebugMode(bool enable)
    {
        if (enable == isDebugMode) return;

        if (enable)
        {
            try
            {
                // Ensure the logs directory exists
                string logDir = Path.Combine(ConfigManager.BaseDir, "Logs");
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                logFilePath = Path.Combine(logDir, "debug_log.txt");

                // Initialize the writer. AutoFlush is critical so crashes don't lose the last few lines.
                logWriter = new StreamWriter(logFilePath, append: false);
                logWriter.AutoFlush = true;

                // Subscribe to ALL Unity engine, plugin, and script logs (even from background threads)
                Application.logMessageReceivedThreaded += HandleUnityLog;
                isDebugMode = true;
                
                Debug.Log("[BugReportManager] Debug mode activated. Global logging started.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[BugReportManager] Failed to start debug logging: {e.Message}");
            }
        }
        else
        {
            Debug.Log("[BugReportManager] Debug mode deactivated. Stopping global logging.");
            
            Application.logMessageReceivedThreaded -= HandleUnityLog;
            if (logWriter != null)
            {
                logWriter.Close();
                logWriter.Dispose();
                logWriter = null;
            }
            isDebugMode = false;
        }
    }

    public bool IsDebugModeActive()
    {
        return isDebugMode;
    }

    private void HandleUnityLog(string logString, string stackTrace, LogType type)
    {
        if (logWriter == null) return;

        try
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            logWriter.WriteLine($"[{time}] [{type}] {logString}");

            // Include the stack trace for errors and exceptions to help track down the exact line of code
            if (type == LogType.Error || type == LogType.Exception)
            {
                logWriter.WriteLine("--- STACK TRACE ---");
                logWriter.WriteLine(stackTrace);
                logWriter.WriteLine("-------------------");
            }
        }
        catch 
        {
            // Silently ignore IO errors during logging to prevent recursive log crashes
        }
    }

    /// <summary>
    /// Generates a complete ZIP file containing the log, a map of the file system, and configuration files.
    /// Runs asynchronously to prevent freezing the VR headset during heavy disk I/O.
    /// </summary>
    public async Task<string> GenerateBugReportZipAsync()
    {
        // Must grab Unity API variables on the main thread before moving to background Task
        string systemInfoString = GenerateSystemInfoString();

        return await Task.Run(() =>
        {
            try
            {
                string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string zipFileName = $"AgeOfJoy_BugReport_{timeStamp}.zip";
                string zipFilePath = Path.Combine(ConfigManager.BaseDir, zipFileName);

                string tempDir = Path.Combine(ConfigManager.BaseDir, "Logs", "TempBugReport");
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                Directory.CreateDirectory(tempDir);

                // 1. Copy the Debug Log (if it exists)
                if (File.Exists(logFilePath))
                {
                    // If currently logging, temporarily pause AutoFlush to copy safely
                    File.Copy(logFilePath, Path.Combine(tempDir, "debug_log.txt"));
                }

                // 2. Generate System Info (using the pre-generated string from main thread)
                string sysInfoPath = Path.Combine(tempDir, "system_info.txt");
                File.WriteAllText(sysInfoPath, systemInfoString);

                // 3. Generate File Tree Map
                string treePath = Path.Combine(tempDir, "file_tree.txt");
                GenerateFileTree(ConfigManager.BaseDir, treePath);

                // 4. Copy all YAML configuration files
                CopyYamlFiles(ConfigManager.BaseDir, tempDir);

                // 5. Compress to ZIP
                if (File.Exists(zipFilePath)) File.Delete(zipFilePath);
                ZipFile.CreateFromDirectory(tempDir, zipFilePath, System.IO.Compression.CompressionLevel.Optimal, false);

                // 6. Cleanup temp folder
                Directory.Delete(tempDir, true);

                Debug.Log($"[BugReportManager] Bug report ZIP generated successfully at: {zipFilePath}");
                return zipFilePath;
            }
            catch (Exception e)
            {
                Debug.LogError($"[BugReportManager] Failed to generate bug report ZIP: {e.Message} {e.StackTrace}");
                return null;
            }
        });
    }

    private string GenerateSystemInfoString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("--- Age of Joy System Diagnostics ---");
        sb.AppendLine($"Date: {DateTime.Now}");
        sb.AppendLine($"App Version: {Application.version}");
        sb.AppendLine($"Unity Version: {Application.unityVersion}");
        sb.AppendLine($"OS: {SystemInfo.operatingSystem}");
        sb.AppendLine($"Device Name: {SystemInfo.deviceName}");
        sb.AppendLine($"Device Model: {SystemInfo.deviceModel}");
        sb.AppendLine($"System Memory: {SystemInfo.systemMemorySize} MB");
        sb.AppendLine($"Graphics Device: {SystemInfo.graphicsDeviceName}");
        
        try 
        {
            sb.AppendLine($"OVR Headset: {OVRPlugin.GetSystemHeadsetType()}");
        } 
        catch { sb.AppendLine("OVR Headset: Not detected or OVRPlugin unavailable."); }

        return sb.ToString();
    }

    private void GenerateFileTree(string rootPath, string outputPath)
    {
        using (StreamWriter writer = new StreamWriter(outputPath))
        {
            writer.WriteLine($"--- File Tree for {rootPath} ---");
            WriteDirectoryTree(new DirectoryInfo(rootPath), writer, "", true);
        }
    }

    private void WriteDirectoryTree(DirectoryInfo dir, StreamWriter writer, string indent, bool isLast)
    {
        try
        {
            writer.WriteLine(indent + "+-- " + dir.Name);
            indent += isLast ? "    " : "|   ";

            FileInfo[] files = dir.GetFiles();
            DirectoryInfo[] subDirs = dir.GetDirectories();

            for (int i = 0; i < files.Length; i++)
            {
                // Skip massive ROM files to keep the text file readable, just log their presence and size
                string sizeStr = (files[i].Length / 1024.0 / 1024.0).ToString("0.00") + " MB";
                writer.WriteLine(indent + (i == files.Length - 1 && subDirs.Length == 0 ? "\\-- " : "+-- ") + $"{files[i].Name} ({sizeStr})");
            }

            for (int i = 0; i < subDirs.Length; i++)
            {
                WriteDirectoryTree(subDirs[i], writer, indent, i == subDirs.Length - 1);
            }
        }
        catch (UnauthorizedAccessException)
        {
            writer.WriteLine(indent + "+-- [ACCESS DENIED]");
        }
    }

    private void CopyYamlFiles(string sourceRoot, string destTempFolder)
    {
        string yamlDestFolder = Path.Combine(destTempFolder, "Configurations");
        Directory.CreateDirectory(yamlDestFolder);

        // Find all YAML files in the BaseDir and its subdirectories
        string[] yamlFiles = Directory.GetFiles(sourceRoot, "*.yaml", SearchOption.AllDirectories);

        foreach (string file in yamlFiles)
        {
            try
            {
                // Recreate the folder structure inside the zip so it's easy to read
                string relativePath = file.Substring(sourceRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string destinationFile = Path.Combine(yamlDestFolder, relativePath);
                
                string destDir = Path.GetDirectoryName(destinationFile);
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                File.Copy(file, destinationFile, true);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BugReportManager] Failed to copy yaml file {file}: {e.Message}");
            }
        }
    }
}