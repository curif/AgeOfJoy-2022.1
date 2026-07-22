/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections;
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

    static string testCabinetDir;
    static string testDescriptionCabinetFile;
    static string testFile;

    private Coroutine mainCoroutine;
    private bool initialized = false;
    private volatile bool reloadRequested = false;

    private CabinetDBAdmin cabinetDBAdmin;

    // The workshop test-cabinet loader is a scene singleton: only one is ever active for the
    // whole scene lifetime (it no longer gets destroyed/recreated per swap - CabinetsController/
    // CabinetReplace own the actual GameObject swap now). This lets AGEBasic's WORKSHOPRELOAD()
    // trigger a redeploy without threading a reference through basicAGE/ConfigurationCommands
    // for a component that lives in one room.
    private static CabinetAutoReload activeInstance;

    // Requests a redeploy of the test cabinet from the on-disk cabinetsdb/test folder,
    // without needing a new test.zip. Returns false (no-op) if there is no active
    // workshop test-cabinet loader or the description.yaml doesn't exist yet.
    public static bool RequestReload()
    {
        if (activeInstance == null || !activeInstance.isActiveAndEnabled || !activeInstance.initialized)
        {
            ConfigManager.WriteConsole("[CabinetAutoReload.RequestReload] no active workshop test-cabinet loader");
            return false;
        }
        if (!File.Exists(testDescriptionCabinetFile))
        {
            ConfigManager.WriteConsole($"[CabinetAutoReload.RequestReload] {testDescriptionCabinetFile} not found");
            return false;
        }
        activeInstance.reloadRequested = true;
        return true;
    }

    void Start()
    {
        ConfigManager.WriteConsole($"[CabinetAutoReload] start ");
        testCabinetDir = ConfigManager.CabinetsDB + "/test";
        testDescriptionCabinetFile = testCabinetDir + "/description.yaml";
        testFile = ConfigManager.Cabinets + "/test.zip";

        cabinetDBAdmin = GameObject.Find("FixedObject").GetComponent<CabinetDBAdmin>();

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
        while (true)
        {
            // ConfigManager.WriteConsole($"[CabinetAutoReload] test for file: {File.Exists(testFile)} {testFile}");
            bool zipPresent = File.Exists(testFile);
            if (zipPresent || reloadRequested)
            {
                reloadRequested = false;

                if (zipPresent)
                {
                    //also deletes the zip file
                    ConfigManager.WriteConsole($"[CabinetAutoReload.reload] loading cabinet from {testFile}");
                    try
                    {
                        cabinetDBAdmin.loadCabinetFromZip(testFile);
                    }
                    catch (System.Exception ex)
                    {
                        ConfigManager.WriteConsoleException($"[CabinetAutoReload.reload] ERROR loading zip file {testFile}", ex);
                        writeGenericException(CabinetDBAdmin.GetNameFromPath(testFile), "ERROR loading zip file", ex);
                        File.Delete(testFile); //delete faulty test cabinet
                        continue;
                    }
                }
                else
                {
                    ConfigManager.WriteConsole($"[CabinetAutoReload.reload] reload requested from {testCabinetDir} (no zip)");
                }

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

                // Keep looping (don't yield break) even on success: this component is permanent
                // now (CabinetsController/CabinetReplace own the swapped cabinet's lifecycle),
                // so it must keep watching for the *next* test.zip drop indefinitely.
                if (loadedSuccesfully)
                    ConfigManager.WriteConsole($"[CabinetAutoReload.reload] {testFile} successfully loaded ");
            }
            ConfigManager.WriteConsole($"[CabinetAutoReload.reload] {testFile} waiting for a new cabinet... ");
            yield return new WaitForSeconds(2f);
        }
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
        if (!File.Exists(testDescriptionCabinetFile))
            return false;

        //new cabinet to test
        CabinetInformation cbInfo = null;
        try
        {
            ConfigManager.WriteConsole($"[CabinetAutoReload] new cabinet from yaml: {testCabinetDir}");

            cbInfo = CabinetInformation.fromYaml(testCabinetDir, cache: false); //description.yaml
            if (cbInfo == null)
            {
                ConfigManager.WriteConsole($"[CabinetAutoReload] ERROR NULL cabinet - new cabinet from yaml: {testCabinetDir}");
                throw new IOException();
            }
        }
        catch (System.Exception ex)
        {
            ConfigManager.WriteConsoleException($"[CabinetAutoReload] ERROR  parsing description {testDescriptionCabinetFile}", ex);
            writeGenericException(testDescriptionCabinetFile, "ERROR parsing description", ex);
            return false;
        }

        //force debug mode:
        cbInfo.debug = true;
        //

        ConfigManager.WriteConsole($"[CabinetAutoReload] cabinet problems (if any):...");
        CabinetInformation.showCabinetProblems(cbInfo, "", "test");

        // invalidate all cached textures for test cabinet
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
            ok = await cabinetsController.ReplaceInRoom(WorkshopTestPosition, WorkshopRoom, "test", cbInfo);
        }
        catch (System.Exception ex)
        {
            ConfigManager.WriteConsoleException($"[CabinetAutoReload] ERROR replacing test cabinet via CabinetsController {testDescriptionCabinetFile}", ex);
            CabinetInformation.showCabinetProblems(null, moreProblems: ex.Message, "test"); //write to output file
            return false;
        }

        if (!ok)
            ConfigManager.WriteConsoleError("[CabinetAutoReload] CabinetsController.ReplaceInRoom failed for test cabinet");

        return ok;
    }
}
