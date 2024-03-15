/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using UnityEngine;
using System.Collections.Generic;
using System.IO;
//using UnityEditor;
using System;
//using UnityEngine.Networking;
using Siccity.GLTFUtility;

//store Cabinets resources
public static class CabinetFactory
{
    public static Dictionary<string, GameObject> CabinetStyles = new Dictionary<string, GameObject>();

    static CabinetFactory()
    {
        ConfigManager.WriteConsole($"[CabinetFactory] read Models Resources");
        CabinetStyles.Add("generic", Resources.Load<GameObject>($"Cabinets/PreFab/Generic"));
        CabinetStyles.Add("timeplt", Resources.Load<GameObject>($"Cabinets/PreFab/TimePilot"));
        CabinetStyles.Add("galaga", Resources.Load<GameObject>($"Cabinets/PreFab/Galaga"));
        CabinetStyles.Add("pacmancabaret", Resources.Load<GameObject>($"Cabinets/PreFab/PacManCabaret"));
        CabinetStyles.Add("frogger", Resources.Load<GameObject>($"Cabinets/PreFab/Frogger"));
        CabinetStyles.Add("defender", Resources.Load<GameObject>($"Cabinets/PreFab/Defender"));
        CabinetStyles.Add("donkeykong", Resources.Load<GameObject>($"Cabinets/PreFab/DonkeyKong"));
        CabinetStyles.Add("xevious", Resources.Load<GameObject>($"Cabinets/PreFab/Xevious"));
        CabinetStyles.Add("1942", Resources.Load<GameObject>($"Cabinets/PreFab/1942"));
        CabinetStyles.Add("stargate", Resources.Load<GameObject>($"Cabinets/PreFab/Stargate"));
        CabinetStyles.Add("junofrst", Resources.Load<GameObject>($"Cabinets/PreFab/JunoFirst"));
        CabinetStyles.Add("digdug", Resources.Load<GameObject>($"Cabinets/PreFab/DigDug"));
        CabinetStyles.Add("tron", Resources.Load<GameObject>($"Cabinets/PreFab/Tron"));
        CabinetStyles.Add("joust", Resources.Load<GameObject>($"Cabinets/PreFab/Joust"));
        CabinetStyles.Add("cocktail", Resources.Load<GameObject>($"Cabinets/PreFab/Cocktail"));
    }

    public static Cabinet Factory(string style, string name, string modelFilePath,
                                    int number, string room, Vector3 position,
                                    Quaternion rotation, Transform parent,
                                    bool cacheGlbModels = true)
    {

        ConfigManager.WriteConsole($"[CabinetFactory] Loading Cabinet style:{style} name:{name} modelFilePath:{modelFilePath} number:{number} room:{room}");

        string cacheKey = BuildKey(name, modelFilePath);
        ConfigManager.WriteConsole($"[CabinetFactory] cache key:{cacheKey}");


        GameObject model;
        if (!String.IsNullOrEmpty(modelFilePath))
        {
            if (cacheGlbModels && CabinetStyles.ContainsKey(cacheKey))
            {
                ConfigManager.WriteConsole($"[CabinetFactory] load cached model {modelFilePath}");
                model = CabinetStyles[cacheKey];
            }
            else
            {
                try
                {
                    model = Importer.LoadFromFile(modelFilePath);
                    model.SetActive(false);
                }
                catch (Exception e)
                {
                    ConfigManager.WriteConsole($"[CabinetFactory] ERROR loading model {modelFilePath}: {e}");
                    model = null;
                }
                if (model == null)
                {
                    ConfigManager.WriteConsole($"[CabinetFactory] can't get model, falls to Galaga: {modelFilePath}");
                    model = CabinetStyles["galaga"];
                }
                else
                {
                    if (cacheGlbModels)
                    {
                        ConfigManager.WriteConsole($"[CabinetFactory] add model to cache: {modelFilePath}");
                        CabinetStyles.Add(cacheKey, model);
                    }
                }
            }
        }
        else if (!CabinetStyles.ContainsKey(style) || CabinetStyles[style] == null)
        {
            Debug.LogError($"[Cabinet.Factory]: style {style} unknown or not loaded, falls to 'galaga' cabinet");
            model = CabinetStyles["galaga"];
        }
        else
        {
            model = CabinetStyles[style];
        }
        string cabinetName = $"cabinet-{name}-{room}-{number}";


        return new Cabinet(cabinetName, position, rotation, parent, go: model);
    }

    public static string BuildKey(string name, string modelFilePath)
    {
        if (String.IsNullOrWhiteSpace(modelFilePath))
        {
            return name;
        }
        string modelDirectory = Path.GetFileName(Path.GetDirectoryName(modelFilePath)); // this gives us the actual folder, ie "tekken2a" for "tekken2"
        string modelFileName = Path.GetFileName(modelFilePath);
        ConfigManager.WriteConsole($"[CabinetFactory.BuildKey] modelDirectory:{modelDirectory}");
        CabinetMetadata cabinetMetadata = CabinetMetadata.fromName(modelDirectory);
        ConfigManager.WriteConsole($"[CabinetFactory.BuildKey] fetching hash for: {modelFileName}");
        cabinetMetadata.Hashes.TryGetValue(modelFileName, out var hash);
        ConfigManager.WriteConsole($"[CabinetFactory.BuildKey] hash: {modelFileName}:{hash}");
        return hash;
    }

    public static Cabinet skinCabinetPart(Cabinet cabinet, CabinetInformation cbinfo,
                                            CabinetInformation.Part p)
    {
        switch (p.type)
        {
            case "bezel":
                {
                    ConfigManager.WriteConsole($"[CabinetFactory.fromInformation] {cbinfo.name} bezel {p.art.file}");
                    cabinet.SetBezel(p.name, cbinfo.getPath(p.art.file));
                }
                break;

            case "marquee":
                {
                    ConfigManager.WriteConsole($"[CabinetFactory.fromInformation] {cbinfo.name} marquee {p.art.file}");
                    Material mat = CabinetMaterials.MarqueeOneLamp;
                    if (p.marquee != null)
                    {
                        if (p.marquee.illuminationType == "two-lamps")
                            mat = CabinetMaterials.MarqueeTwoLamps;
                        else if (p.marquee.illuminationType == "one-tube")
                            mat = CabinetMaterials.MarqueeOneTube;
                        else if (p.marquee.illuminationType == "two-tubes")
                            mat = CabinetMaterials.MarqueeTwoTubes;
                        else if (p.marquee.illuminationType == "none")
                            mat = CabinetMaterials.MarqueeNoLamps;
                        else
                            mat = CabinetMaterials.MarqueeOneLamp;
                    }

                    if (p.art != null)
                        cabinet.SetTextureTo(p.name, cbinfo.getPath(p.art.file), mat, invertX: p.art.invertx, invertY: p.art.inverty);
                    else
                        cabinet.SetMaterial(p.name, mat);

                    //after
                    if (p.color != null && p.marquee.illuminationType != "none")
                        cabinet.SetMarqueeEmissionColor(p.name, p.color.getColorNoIntensity(), p.color.intensity);
                }
                break;

            default:
                {
                    if (p.material == null &&
                        p.art == null &&
                        p.color == null &&
                        p.emission == null &&
                        p.transparency == 0)
                        cabinet.SetMaterialFrom(p.name, CabinetMaterials.Black);
                    else
                    {
                        int pos = cabinet.PartsPosition(p.name); //performance
                        ConfigManager.WriteConsole($"[CabinetFactory.skinCabinetPart] #{pos} {p.name}: type: {p.type} material: {p.material} color: {p.color} transp:{p.transparency} emission: {p.emission}");
                        if (p.material != null)
                            cabinet.SetMaterialFrom(pos, CabinetMaterials.fromName(p.material));
                        else
                            cabinet.SetMaterialFrom(pos, CabinetMaterials.Base);

                        if (p.art != null)
                            cabinet.SetTextureTo(pos, cbinfo.getPath(p.art.file), null, invertX: p.art.invertx, invertY: p.art.inverty);

                        if (p.color != null)
                            cabinet.SetColorPart(pos, p.color.getColor());

                        if (p.transparency != 0)
                            cabinet.SetTransparencyPart(pos, p.transparency);

                        if (p.emission != null)
                        {
                            cabinet.SetEmissionEnabledPart(pos, p.emission.emissive);
                            if (p.emission.color != null)
                                cabinet.SetEmissionColorPart(pos, p.emission.color.getColor());
                            if (p.emission.art != null)
                            {
                                cabinet.SetEmissionTextureTo(pos,
                                                                cbinfo.getPath(p.emission.art.file),
                                                                invertX: p.emission.art.invertx,
                                                                invertY: p.emission.art.inverty);
                            }
                        }
                    }
                }
                break;
        }
        // Part scale and rotation
        cabinet.ScalePart(p.name, p.geometry.scalepercentage);
        cabinet.RotatePart(p.name, p.geometry.rotation.x, p.geometry.rotation.y, p.geometry.rotation.z);

        //enable / disable
        cabinet.EnablePart(p.name, p.visible);

        return cabinet;
    }

    public static Cabinet skinFromInformation(Cabinet cabinet, CabinetInformation cbinfo)
    {

        //process each part
        if (cbinfo.Parts != null)
        {
            ConfigManager.WriteConsole($"[CabinetFactory.fromInformation] {cbinfo.name} texture each part");
            foreach (CabinetInformation.Part p in cbinfo.Parts)
            {
                skinCabinetPart(cabinet, cbinfo, p);
            }
        }

        return cabinet;
    }

    public static Cabinet fromInformation(CabinetInformation cbinfo, string room, int number,
                                             Vector3 position, Quaternion rotation, Transform parent,
                                            List<AgentScenePosition> agentPlayerPositions,
                                            BackgroundSoundController backgroundSoundController,
                                            bool cacheGlbModels = true)
    {
        string modelFilePath = "";
        if (!String.IsNullOrEmpty(cbinfo.model.file))
        {
            if (!String.IsNullOrEmpty(cbinfo.model.style))
                modelFilePath = ConfigManager.CabinetsDB + "/" + cbinfo.model.style + "/" + cbinfo.model.file;
            else
                modelFilePath = cbinfo.pathBase + "/" + cbinfo.model.file;

            if (!File.Exists(modelFilePath))
            {
                ConfigManager.WriteConsoleError($"[CabinetFactory.fromInformation] {modelFilePath} model don't exists, falls to standar cabinet model");
                modelFilePath = "";
            }
        }

        Cabinet cabinet = CabinetFactory.Factory(cbinfo.style, cbinfo.name, modelFilePath,
                                                    number, room, position, rotation, parent,
                                                    cacheGlbModels: cacheGlbModels);

        //box colliders
        //addRigidBody();
        // cbinfo.debug = true;
        BoxCollider boxCollider = cabinet.addBoxCollider(false);
        cabinet.toFloor();

        if (cbinfo.material != null)
        {
            cabinet.SetMaterial(CabinetMaterials.fromName(cbinfo.material));
        }
        else if (cbinfo.color != null)
        {
            Material mat = new Material(CabinetMaterials.Base);
            mat.SetColor("_Color", cbinfo.color.getColor());
            cabinet.SetMaterial(mat);
        }

        if (!string.IsNullOrEmpty(cbinfo.coinslot))
        {
            ConfigManager.WriteConsole($"[CabinetFactory.fromInformation] {cbinfo.name} coinslot {cbinfo.coinslot}");
            cabinet.AddCoinSlot(cbinfo.coinslot,
                    cbinfo.coinslotgeometry.rotation.x, cbinfo.coinslotgeometry.rotation.y, cbinfo.coinslotgeometry.rotation.z,
                    cbinfo.coinslotgeometry.scalepercentage);
        }

        if (cabinet.PartsExist("screen-mock-vertical") || cabinet.PartsExist("screen-mock-horizontal"))
        {
            Vector3 CRTrotation = new Vector3(cbinfo.crt.geometry.rotation.x, cbinfo.crt.geometry.rotation.y, cbinfo.crt.geometry.rotation.z);

            cabinet.addCRT(
                    cbinfo.crt.type, cbinfo.crt.orientation, cbinfo.rom, cbinfo.getPath(cbinfo.video.file),
                    cbinfo.timetoload, cbinfo.pathBase,
                    invertX: cbinfo.crt.screen.invertx,
                    invertY: cbinfo.crt.screen.inverty,
                    GameVideoFileInvertX: cbinfo.video.invertx,
                    GameVideoFileInvertY: cbinfo.video.inverty,
                    EnableSaveState: cbinfo.enablesavestate,
                    StateFile: cbinfo.statefile,
                    rotation: CRTrotation, cbinfo.crt.geometry.scalepercentage,
                    cbinfo.crt.screen.gamma, cbinfo.crt.screen.brightness,
                    agentPlayerPositions,
                    cbinfo.crt.screen.shader, cbinfo.crt.screen.config(),
                    cbinfo.ControlMap,
                    cbinfo.lightGunInformation,
                    cbinfo.agebasic, backgroundSoundController,
                    cbinfo.core);

            ConfigManager.WriteConsole($"[CabinetFactory.fromInformation] {cbinfo.name} CRT added");
        }

        //blockers
        if (cbinfo.Parts != null)
        {
            ConfigManager.WriteConsole($"[CabinetFactory.fromInformation] {cbinfo.name} blockers");
            foreach (CabinetInformation.Part p in cbinfo.Parts)
            {
                if (p.type == "blocker")
                {
                    ConfigManager.WriteConsole($"[CabinetFactory.fromInformation] {cbinfo.name} part {p.name} blockers");
                    cabinet.AddAColliderBlocker(p.name, false);
                    //disable main box collider for collissions but works on put on floor.
                    boxCollider.excludeLayers = ~0;
                }
            }
        }

        return cabinet;
    }
}
