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
using System.Threading.Tasks;

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
    }

    public static Cabinet Factory(string style, string name, string modelFilePath, int number, string room, Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject model;
        if (!String.IsNullOrEmpty(modelFilePath))
        {
            if (CabinetStyles.ContainsKey(modelFilePath))
            {
                ConfigManager.WriteConsole($"[CabinetFactory] load cached model {modelFilePath}");
                model = CabinetStyles[modelFilePath];
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
                    ConfigManager.WriteConsole($"[CabinetFactory] add model to cache: {modelFilePath}");
                    CabinetStyles.Add(modelFilePath, model);
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

    public static Cabinet skinCabinetPart(Cabinet cabinet, CabinetInformation cbinfo, CabinetInformation.Part p)
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
                    if (p.material != null)
                        cabinet.SetMaterial(p.name, CabinetMaterials.fromName(p.material));
                    else if (p.art != null)
                        cabinet.SetTextureTo(p.name, cbinfo.getPath(p.art.file), CabinetMaterials.Base, invertX: p.art.invertx, invertY: p.art.inverty);
                    else if (p.color != null)
                    {
                        Material matColor = new Material(CabinetMaterials.Base);
                        matColor.SetColor("_Color", p.color.getColor());
                        cabinet.SetMaterial(p.name, matColor);
                    }
                    else
                        cabinet.SetMaterial(p.name, CabinetMaterials.Black);
                }
                break;
        }
        // Part scale and rotation
        cabinet.ScalePart(p.name, p.geometry.scalepercentage);
        cabinet.RotatePart(p.name, p.geometry.rotation.x, p.geometry.rotation.y, p.geometry.rotation.z);
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
      List<GameObject> agentPlayerPositions)
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

        Cabinet cabinet = CabinetFactory.Factory(cbinfo.style, cbinfo.name, modelFilePath, number, room, position, rotation, parent);

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
                cbinfo.agebasic);

        ConfigManager.WriteConsole($"[CabinetFactory.fromInformation] {cbinfo.name} CRT added");

        return cabinet;
    }
}
