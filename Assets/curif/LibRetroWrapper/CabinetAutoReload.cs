/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Threading.Tasks;

public class CabinetAutoReload : MonoBehaviour
{
    public CabinetDebugConsole debugConsole;

    [Tooltip("The CabinetsController on this same GameObject that owns position 0 (the workshop test slot)")]
    public CabinetsController cabinetsController;

    private const int WorkshopTestPosition = 0;
    private const string WorkshopRoom = "workshop";
    private const string DefaultCabinetName = "test";

    // Cadence for the recursive folder scan that detects any file change (texture, yaml, .bas, ...)
    // under the summoned cabinet's folder, so edits on disk reload the workshop cabinet without an
    // explicit WORKSHOPRELOAD() call.
    private const float FolderScanIntervalSecs = 5f;
    private const float PollIntervalSecs = 1f;

    // Files the game itself writes back into a cabinet's folder as a side effect of loading/rendering
    // it. These must be excluded from the folder scan, otherwise every redeploy rewrites them, the next
    // scan sees "a change," and it redeploys again forever.
    private const string MetadataFileName = "metadata.yaml";
    private const string TextureCacheExtension = ".aojv1";

    private string currentCabinetName;

    private Coroutine mainCoroutine;
    private bool initialized = false;
    private volatile bool reloadRequested = false;

    // Baseline snapshot of the summoned cabinet's folder (relative path -> last-write ticks),
    // captured right after a successful load. Compared against every FolderScanIntervalSecs.
    private Dictionary<string, long> folderSnapshot;

    // Snapshot seen on the previous scan when it first differed from folderSnapshot, but hadn't
    // yet been confirmed stable. Debounces bursts of saves (e.g. editing several files in a row)
    // into a single reload once the folder stops changing for one full scan interval, instead of
    // reloading after every individual save.
    private Dictionary<string, long> pendingSnapshot;

    // The workshop test-cabinet loader is a scene singleton: only one is ever active for the
    // whole scene lifetime (it no longer gets destroyed/recreated per swap - CabinetsController/
    // CabinetReplace own the actual GameObject swap now). This lets AGEBasic's WORKSHOPRELOAD()
    // trigger a redeploy (or a switch to a different cabinet) without threading a reference through
    // basicAGE/ConfigurationCommands for a component that lives in one room.
    private static CabinetAutoReload activeInstance;

    private string CabinetDir(string cabinetName) => Path.Combine(ConfigManager.CabinetsDB, cabinetName);
    private string DescriptionFile(string cabinetName) => Path.Combine(CabinetDir(cabinetName), "description.yaml");

    // Requests a redeploy of the workshop cabinet from disk, or - if cabinetName names a different,
    // already-installed cabinet under cabinetsdb/ - switches the workshop slot to it (and persists the
    // choice so it survives an app restart). Called from AGEBasic's WORKSHOPRELOAD([cabinetName$]).
    // Returns false (no-op) if there is no active workshop test-cabinet loader, or the target cabinet
    // doesn't exist on disk.
    public static bool RequestReload(string cabinetName = null)
    {
        if (activeInstance == null || !activeInstance.isActiveAndEnabled || !activeInstance.initialized)
        {
            ConfigManager.WriteConsole("[CabinetAutoReload.RequestReload] no active workshop test-cabinet loader");
            return false;
        }

        bool sameOrNoName = string.IsNullOrEmpty(cabinetName)
            || string.Equals(cabinetName, activeInstance.currentCabinetName, StringComparison.OrdinalIgnoreCase);

        if (sameOrNoName)
        {
            string descriptionFile = activeInstance.DescriptionFile(activeInstance.currentCabinetName);
            if (!File.Exists(descriptionFile))
            {
                ConfigManager.WriteConsole($"[CabinetAutoReload.RequestReload] {descriptionFile} not found");
                return false;
            }
            activeInstance.reloadRequested = true;
            return true;
        }

        return activeInstance.SwitchCabinet(cabinetName);
    }

    // Switches the workshop slot to a different, already-installed cabinet and persists the choice.
    private bool SwitchCabinet(string cabinetName)
    {
        string descriptionFile = DescriptionFile(cabinetName);
        if (!File.Exists(descriptionFile))
        {
            ConfigManager.WriteConsole($"[CabinetAutoReload.RequestReload] cabinet '{cabinetName}' not found: {descriptionFile}");
            return false;
        }

        currentCabinetName = cabinetName;
        WorkshopSettings.Save(currentCabinetName);
        folderSnapshot = null; // force a fresh baseline once the new cabinet finishes loading
        pendingSnapshot = null;
        reloadRequested = true;
        return true;
    }

    void Start()
    {
        ConfigManager.WriteConsole($"[CabinetAutoReload] start ");

        WorkshopSettings settings = WorkshopSettings.Load();
        if (!string.IsNullOrEmpty(settings?.SummonedCabinet))
        {
            currentCabinetName = settings.SummonedCabinet;
            // override whatever placeholder CabinetsController.initalizeCabinets() seeded for this slot
            reloadRequested = true;
        }
        else
        {
            currentCabinetName = DefaultCabinetName;
        }

        activeInstance = this;

        mainCoroutine = StartCoroutine(reload());
        initialized = true;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            //is pausing
            if (mainCoroutine != null)
            {
                StopCoroutine(mainCoroutine);
                mainCoroutine = null;
            }
        }
        else
        {
            if (initialized)
                mainCoroutine = StartCoroutine(reload());
        }
    }

    IEnumerator reload()
    {
        float sinceLastScan = 0f;

        while (true)
        {
            if (reloadRequested)
            {
                reloadRequested = false;
                pendingSnapshot = null;

                ConfigManager.WriteConsole($"[CabinetAutoReload.reload] (re)loading cabinet '{currentCabinetName}' from {CabinetDir(currentCabinetName)}");

                Task<bool> loadTask = LoadCabinet();
                yield return new WaitUntil(() => loadTask.IsCompleted);

                bool loadedSuccesfully;
                if (loadTask.IsFaulted)
                {
                    ConfigManager.WriteConsoleException("[CabinetAutoReload.reload] ERROR loading cabinet", loadTask.Exception);
                    loadedSuccesfully = false;
                }
                else
                {
                    loadedSuccesfully = loadTask.Result;
                }

                if (loadedSuccesfully)
                {
                    ConfigManager.WriteConsole($"[CabinetAutoReload.reload] '{currentCabinetName}' successfully loaded ");
                    // baseline captured after load completes, so caches the load itself writes
                    // (texture cache, metadata.yaml) don't look like a pending user change.
                    folderSnapshot = SnapshotFolder(CabinetDir(currentCabinetName));
                }

                sinceLastScan = 0f;
            }
            else
            {
                sinceLastScan += PollIntervalSecs;
                if (sinceLastScan >= FolderScanIntervalSecs && !string.IsNullOrEmpty(currentCabinetName))
                {
                    sinceLastScan = 0f;

                    // Never let a scan failure (e.g. a transient IO error while a file is mid-write)
                    // kill this coroutine - that would silently stop both auto-reload and future
                    // explicit WORKSHOPRELOAD() calls until the next app restart.
                    try
                    {
                        Dictionary<string, long> latest = SnapshotFolder(CabinetDir(currentCabinetName));
                        ConfigManager.WriteConsole($"[CabinetAutoReload.reload] scan '{currentCabinetName}': {latest.Count} files tracked");

                        if (folderSnapshot == null || !HasChanged(folderSnapshot, latest))
                        {
                            // matches what's currently deployed - nothing pending
                            folderSnapshot = latest;
                            pendingSnapshot = null;
                        }
                        else if (pendingSnapshot != null && !HasChanged(pendingSnapshot, latest))
                        {
                            // unchanged since the last scan that first saw a difference: the user has
                            // stopped editing/saving for a full interval, safe to reload now.
                            ConfigManager.WriteConsole($"[CabinetAutoReload.reload] file changes under '{currentCabinetName}' settled, reloading");
                            pendingSnapshot = null;
                            reloadRequested = true;
                        }
                        else
                        {
                            // first time we've seen this change, or it's still actively being edited -
                            // wait one more scan interval before committing to a reload.
                            ConfigManager.WriteConsole($"[CabinetAutoReload.reload] file changes under '{currentCabinetName}' still settling, re-checking next scan");
                            pendingSnapshot = latest;
                        }
                    }
                    catch (Exception ex)
                    {
                        ConfigManager.WriteConsoleException($"[CabinetAutoReload.reload] ERROR scanning '{currentCabinetName}' for changes", ex);
                    }
                }
            }

            yield return new WaitForSeconds(PollIntervalSecs);
        }
    }

    // Recursive snapshot of a cabinet folder (relative path -> last-write ticks), skipping files the
    // game itself generates as a caching side effect of loading the cabinet.
    private static Dictionary<string, long> SnapshotFolder(string dir)
    {
        Dictionary<string, long> snapshot = new();
        if (!Directory.Exists(dir))
            return snapshot;

        foreach (string file in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
        {
            string name = Path.GetFileName(file);
            if (string.Equals(name, MetadataFileName, StringComparison.OrdinalIgnoreCase))
                continue;
            if (name.EndsWith(TextureCacheExtension, StringComparison.OrdinalIgnoreCase))
                continue;

            string relative = Path.GetRelativePath(dir, file);
            snapshot[relative] = File.GetLastWriteTimeUtc(file).Ticks;
        }
        return snapshot;
    }

    private static bool HasChanged(Dictionary<string, long> oldSnapshot, Dictionary<string, long> newSnapshot)
    {
        if (oldSnapshot.Count != newSnapshot.Count)
            return true;

        foreach (KeyValuePair<string, long> kv in newSnapshot)
        {
            if (!oldSnapshot.TryGetValue(kv.Key, out long oldTicks) || oldTicks != kv.Value)
                return true;
        }
        return false;
    }

    private void writeGenericException(string cabName, string message, Exception ex)
    {
        string path = CabinetInformation.debugLogPath();
        ConfigManager.WriteConsole($"[CabinetAutoReload] {path}");
        // Write exception details to the log file
        using (StreamWriter writer = new StreamWriter(path, true))
        {
            writer.WriteLine($"CABINET: {cabName}");
            writer.WriteLine(new string('-', 50)); // Separator
            writer.WriteLine($"Error message: {message}");
            writer.WriteLine($"Exception message: {ex.Message}");
            writer.WriteLine(new string('-', 50)); // Separator
        }
        return;
    }

    private async Task<bool> LoadCabinet()
    {
        string cabinetDir = CabinetDir(currentCabinetName);
        string descriptionFile = DescriptionFile(currentCabinetName);

        if (!File.Exists(descriptionFile))
            return false;

        //new cabinet to test
        CabinetInformation cbInfo = null;
        try
        {
            ConfigManager.WriteConsole($"[CabinetAutoReload] new cabinet from yaml: {cabinetDir}");

            cbInfo = CabinetInformation.fromYaml(cabinetDir, cache: false); //description.yaml
            if (cbInfo == null)
            {
                ConfigManager.WriteConsole($"[CabinetAutoReload] ERROR NULL cabinet - new cabinet from yaml: {cabinetDir}");
                throw new IOException();
            }
        }
        catch (System.Exception ex)
        {
            ConfigManager.WriteConsoleException($"[CabinetAutoReload] ERROR  parsing description {descriptionFile}", ex);
            writeGenericException(descriptionFile, "ERROR parsing description", ex);
            return false;
        }

        //force debug mode:
        cbInfo.debug = true;
        //

        ConfigManager.WriteConsole($"[CabinetAutoReload] cabinet problems (if any):...");
        CabinetInformation.showCabinetProblems(cbInfo, "", currentCabinetName);

        // invalidate all cached textures for this cabinet
        if (cbInfo.Parts != null)
        {
            foreach (CabinetInformation.Part p in cbInfo.Parts)
            {
                if (p?.art?.file != null)
                {
                    CabinetTextureCache.InvalidateCachedTexture(cbInfo.getPath(p.art.file));
                }
            }
        }

        bool ok;
        try
        {
            ConfigManager.WriteConsole($"[CabinetAutoReload] Deploy test cabinet {cbInfo.name} via CabinetsController");
            ok = await cabinetsController.ReplaceInRoom(WorkshopTestPosition, WorkshopRoom, currentCabinetName, cbInfo);
        }
        catch (System.Exception ex)
        {
            ConfigManager.WriteConsoleException($"[CabinetAutoReload] ERROR replacing test cabinet via CabinetsController {descriptionFile}", ex);
            CabinetInformation.showCabinetProblems(null, moreProblems: ex.Message, currentCabinetName); //write to output file
            return false;
        }

        if (!ok)
            ConfigManager.WriteConsoleError("[CabinetAutoReload] CabinetsController.ReplaceInRoom failed for test cabinet");

        return ok;
    }
}
