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
using System.Linq;

public class CabinetAutoReload : MonoBehaviour
{
    [Tooltip("Positions where the player can stay to load the cabinet")]
    public List<AgentScenePosition> AgentPlayerPositions;
    public BackgroundSoundController backgroundSoundController;
    public CabinetDebugConsole debugConsole;

    static string testCabinetDir;
    static string testDescriptionCabinetFile;
    static string testFile;

    private Coroutine mainCoroutine;
    private bool initialized = false;

    private CabinetDBAdmin cabinetDBAdmin; 

    void Start()
    {
        ConfigManager.WriteConsole($"[CabinetAutoReload] start ");
        testCabinetDir = ConfigManager.CabinetsDB + "/test";
        testDescriptionCabinetFile = testCabinetDir + "/description.yaml";
        testFile = ConfigManager.Cabinets + "/test.zip";

        cabinetDBAdmin = GameObject.Find("FixedObject").GetComponent<CabinetDBAdmin>();

        mainCoroutine = StartCoroutine(reload());
        initialized = true;
    }

    private void OnEnable()
    {
        if (!initialized)
            return;
        if (mainCoroutine == null)
            mainCoroutine = StartCoroutine(reload());
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

    private void OnDisable()
    {
        if (!initialized)
            return;
        if (mainCoroutine != null)
        {
            StopCoroutine(mainCoroutine);
            mainCoroutine = null;
        }
    }

    IEnumerator reload()
    {
        bool loadedSuccesfully = false;
        while (true)
        {
            // ConfigManager.WriteConsole($"[CabinetAutoReload] test for file: {File.Exists(testFile)} {testFile}");
            if (File.Exists(testFile))
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
                
                loadedSuccesfully = LoadCabinet();
                if (loadedSuccesfully)
                {
                    ConfigManager.WriteConsole($"[CabinetAutoReload.reload] {testFile} successfully loaded ");
                    gameObject.SetActive(false); //don't destroy it
                    yield break;
                }
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

    private bool LoadCabinet()
    {

        if (!File.Exists(testDescriptionCabinetFile))
            return false;

        // ConfigManager.WriteConsole($"[CabinetAutoReload] New cabinet to test: {testDescriptionCabinetFile}");

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

        try
        {
            CabinetDBAdmin.MoveMameFiles(cbInfo);
        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleException($"[CabinetAutoReload] ERROR moving MAME files", e);
        }

        try
        {
            //cabinet inseption
            ConfigManager.WriteConsole($"[CabinetAutoReload] Deploy test cabinet {cbInfo.name}");
            ConfigManager.WriteConsole($"[CabinetAutoReload]AgentPlayerPositions: {string.Join(",", AgentPlayerPositions.Select(x => x.ToString()))}");

            Cabinet cab = CabinetFactory.fromInformation(cbInfo, "workshop", 0, transform.position,
                                                         transform.rotation, transform.parent,
                                                         AgentPlayerPositions, backgroundSoundController,
                                                         cacheGlbModels: false);

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
            CabinetFactory.skinFromInformation(cab, cbInfo);

            ConfigManager.WriteConsole($"[CabinetAutoReload] cabinet problems (if any):...");
            CabinetInformation.showCabinetProblems(cbInfo, "", "test");

            ConfigManager.WriteConsole("[CabinetAutoReload] New Test Cabinet deployed ******");
            //UnityEngine.Object.Destroy(gameObject);

            CabinetAutoReload cba = (CabinetAutoReload)cab.gameObject.AddComponent(typeof(CabinetAutoReload)); //this will excecute Start().
            cba.AgentPlayerPositions = AgentPlayerPositions;
            cba.backgroundSoundController = backgroundSoundController;

            //add CabinetReplace, needed for libretroController
            CabinetReplace cabReplaceComp = cab.gameObject.AddComponent<CabinetReplace>();
            cabReplaceComp.AgentPlayerPositionComponents = AgentPlayerPositions;
            cabReplaceComp.cabinet = cab;
            /*cabReplaceComp.AgentPlayerPositionComponentsToUnload = AgentPlayerPositionComponentsToUnload;
            cabReplaceComp.AgentPlayerPositionComponentsToLoad = AgentPlayerPositionComponentsToLoad;
            cabReplaceComp.game = newCabGame;
            cabReplaceComp.outOfOrderCabinet = gameObject;
            */
            cabReplaceComp.backgroundSoundController = backgroundSoundController;
            
            cab.gameObject.SetActive(true);

            return true;
        }
        catch (System.Exception ex)
        {
            ConfigManager.WriteConsoleException($"[CabinetAutoReload] ERROR loading cabinet from description {testDescriptionCabinetFile}", ex);
            CabinetInformation.showCabinetProblems(null, moreProblems: ex.Message, "test");
            return false;
        }
    }
}
