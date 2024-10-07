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
using System.Linq;

//using UnityEngine.Networking;
using Siccity.GLTFUtility;

//store Cabinets resources
public static class CabinetFactory
{
    public static Dictionary<string, GameObject> CabinetStyles = new Dictionary<string, GameObject>();
    public static ResourceCache<string, GameObject> CabinetCache = ResourceCacheManager.Create<string, GameObject>();

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

    public static Cabinet Factory(string style, string name, string controlScheme, string modelFilePath,
                                    int number, string room, Vector3 position,
                                    Quaternion rotation, Transform parent,
                                    bool cacheGlbModels = true)
    {

        ConfigManager.WriteConsole($"[CabinetFactory] Loading Cabinet style:{style} name:{name} modelFilePath:{modelFilePath} number:{number} room:{room}");


        GameObject model;
        if (!String.IsNullOrEmpty(modelFilePath))
        {
            string cacheKey = BuildKey(modelFilePath);
            ConfigManager.WriteConsole($"[CabinetFactory] cache key:{cacheKey}");

            if (cacheKey != null && CabinetStyles.ContainsKey(cacheKey))
            {
                ConfigManager.WriteConsole($"[CabinetFactory] load default model {modelFilePath}");
                model = CabinetStyles[cacheKey];
            }
            else if (cacheGlbModels && cacheKey != null && CabinetCache.ContainsKey(cacheKey))
            {
                ConfigManager.WriteConsole($"[CabinetFactory] load cached model {modelFilePath}");
                model = CabinetCache.Get(cacheKey);
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
                    if (cacheGlbModels && cacheKey != null)
                    {
                        ConfigManager.WriteConsole($"[CabinetFactory] add model to cache: {modelFilePath}");
                        CabinetCache.Add(cacheKey, model);
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


        return new Cabinet(cabinetName, controlScheme, position, rotation, parent, go: model);
    }

    public static string BuildKey(string modelFilePath)
    {
        string modelDirectory = Path.GetFileName(Path.GetDirectoryName(modelFilePath)); // this gives us the actual folder, ie "tekken2a" for "tekken2"
        string modelFileName = Path.GetFileName(modelFilePath);
        ConfigManager.WriteConsole($"[CabinetFactory.BuildKey] modelDirectory:{modelDirectory}");
        CabinetMetadata cabinetMetadata = CabinetMetadata.fromName(modelDirectory);
        ConfigManager.WriteConsole($"[CabinetFactory.BuildKey] fetching hash for: {modelFileName}");
        string hash = cabinetMetadata.getHash(modelFileName);
        ConfigManager.WriteConsole($"[CabinetFactory.BuildKey] hash: {modelFileName}:{hash}");
        return hash;
    }

    public static Cabinet skinCabinetPart(Cabinet cabinet, CabinetInformation cbinfo, CabinetInformation.Part p)
    {
        CabinetPart cp = cabinet.GetPartController(p.name);

        switch (p.type)
        {
            case "bezel":
                {
                    ConfigManager.WriteConsole($"[CabinetFactory.fromInformation] {cbinfo.name} bezel {p.art.file}");
                    cp.SetBezel(cbinfo.getPath(p.art.file));

                    if (p.properties.Count > 0)
                    {
                        CabinetMaterials.MaterialPropertyTranslator t = new CabinetMaterials.FrontGlassProperties();
                        cp.ApplyUserMaterialConfiguration(t.Translate(p.properties));
                    }
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
                        cp.SetTextureTo(p.name, cbinfo.getPath(p.art.file), mat, invertX: p.art.invertx, invertY: p.art.inverty);
                    else
                        cp.SetMaterial(mat);

                    //after
                    if (p.color != null && p.marquee.illuminationType != "none")
                        cp.SetMarqueeEmissionColor(p.color, p.marquee.color);

                    if (p.properties.Count > 0)
                    {
                        CabinetMaterials.MaterialPropertyTranslator t = new CabinetMaterials.MarqueeProperties();
                        cp.ApplyUserMaterialConfiguration(t.Translate(p.properties));
                    }
                }
                break;

            default:
                {
                    /* don't change material if the gameobject has one previously assigned #601
                    didn't work. needs more analysis.
                    GameObject go = cabinet.Parts(p.name);
                    if (Cabinet.GetMaterialPart(go) != null)
                        break;
                    */

                    if (p.material == null &&
                        p.art == null &&
                        p.color == null &&
                        p.emission == null &&
                        p.transparency == 0)
                        cp.SetMaterial(CabinetMaterials.Black);

                    else if (p.material == null &&
                            p.art == null &&
                            p.color != null &&
                            p.emission == null &&
                            p.transparency == 0)
                    {
                        //vertex color optimization
                        cp.SetColorVertex(p.color.getColor());
                        if (p.properties.Count > 0)
                        {
                            CabinetMaterials.MaterialPropertyTranslator t = CabinetMaterials.PropertyTranslator("Vertex Color");
                            cp.ApplyUserMaterialConfiguration(t.Translate(p.properties));
                        }
                    }
                    else
                    {
                        int pos = cabinet.PartsPosition(p.name); //performance

                        ConfigManager.WriteConsole($"[CabinetFactory.skinCabinetPart] #{pos} {p.name}: type: {p.type} material: {p.material} color: {p.color} transp:{p.transparency} emission: {p.emission}");

                        CabinetMaterials.MaterialPropertyTranslator propTranslator;

                        if (p.material != null)
                        {
                            Material mat = CabinetMaterials.fromName(p.material);
                            cp.SetMaterialFrom(mat);
                            propTranslator = CabinetMaterials.PropertyTranslator(p.material);
                        }
                        else
                        {
                            cp.ForceMaterialBase();
                            propTranslator = CabinetMaterials.PropertyTranslator("base");
                        }

                        if (p.art != null)
                              cp.SetTextureTo(cbinfo.getPath(p.art.file), null, invertX: p.art.invertx, invertY: p.art.inverty);

                        if (p.color != null)
                            cp.SetColor(p.color.getColor());

                        if (p.transparency != 0)
                        {
                            int transparency = p.transparency;
                            cp.SetTransparency(ref transparency);
                        }

                        if (!string.IsNullOrEmpty(p.normal))
                        {
                            string realProperty = propTranslator.GetRealPropertyName("normal");
                            cp.SetNormal(p.normal, realProperty);
                        }

                        if (p.emission != null)
                        {
                            //assign the texture first
                            if (!string.IsNullOrEmpty(p.emission.art?.file))
                                cp.SetEmissive()
                                  .SetEmissionTextureTo(cbinfo.getPath(p.emission.art.file),
                                                        invertX: p.emission.art.invertx,
                                                        invertY: p.emission.art.inverty)
                                  .SetEmissionColor(p.emission.color.getColor())
                                  .ActivateEmission(p.emission.emissive);
                            else
                                cp.SetEmissive()
                                  .UseEmissionMainTexture()
                                  .SetEmissionColor(p.emission.color.getColor())
                                  .ActivateEmission(p.emission.emissive);
                        }

                        //apply user configuration
                        cp.ApplyUserMaterialConfiguration(propTranslator.Translate(p.properties));
                        
                    }
                }
                break;
        }
        
        // Part scale and rotation
        //enable / disable
        cp.ApplyUserConfigurationGeometry(p.geometry).Enable(p.visible);

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

        CabinetDBAdmin.MoveMameFiles(cbinfo);

        Cabinet cabinet = CabinetFactory.Factory(cbinfo.style, cbinfo.name, cbinfo.controlScheme, modelFilePath,
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

        if (cbinfo.crt != null &&
            (cabinet.PartsExist("screen-mock-vertical") ||
                cabinet.PartsExist("screen-mock-horizontal")))
        {
            Vector3 CRTrotation = new Vector3(cbinfo.crt.geometry.rotation.x,
                                                cbinfo.crt.geometry.rotation.y,
                                                cbinfo.crt.geometry.rotation.z);
            ConfigManager.WriteConsole($"[fromInformation]AgentPlayerPositions: {string.Join(",", agentPlayerPositions.Select(x => x.ToString()))}");

            cabinet.addCRT(cbinfo, agentPlayerPositions, backgroundSoundController, CRTrotation);

            ConfigManager.WriteConsole($"[CabinetFactory.fromInformation] {cbinfo.name} CRT added");
        }
        else if (cbinfo.agebasic != null)
        {
            cabinet.addController(cbinfo.pathBase,
                                        cbinfo.ControlMap,
                                        cbinfo.lightGunInformation,
                                        cbinfo.agebasic,
                                        backgroundSoundController
                                    );
            ConfigManager.WriteConsole($"[CabinetFactory.fromInformation] {cbinfo.name} NO-CRT controller added");
        }

        //blockers, targets, etc
        if (cbinfo.Parts != null)
        {
            ConfigManager.WriteConsole($"[CabinetFactory.fromInformation] {cbinfo.name} blockers");
            foreach (CabinetInformation.Part p in cbinfo.Parts)
            {
                try
                {
                    int partNum = cabinet.PartsPosition(p.name);

                    if (p.type == "blocker")
                    {
                        ConfigManager.WriteConsole($"[CabinetFactory.fromInformation] {cbinfo.name} part {p.name} blockers");
                        cabinet.AddAColliderBlocker(p.name, false);
                        //disable main box collider for collissions but works on put on floor.
                        boxCollider.excludeLayers = ~0;
                    }
                    else
                    {

                        if (p.istarget)
                            cabinet.SetLightGunTarget(partNum, cbinfo.lightGunInformation);

                        if (p.physical != null)
                            cabinet.SetPhysics(partNum, p.physical);

                        if (p.speaker != null)
                            cabinet.SetAudio(partNum, cbinfo.pathBase, p.speaker);
                    }
                }
                catch (Exception e)
                {
                    ConfigManager.WriteConsoleException($"[CabinetFactory.fromInformation] {cbinfo.name} part {p.name}.", e);
                    continue;
                }
            }            
        }

        return cabinet;
    }
}
