/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

//using UnityEngine.Networking;
using Siccity.GLTFUtility;
//using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

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

    public static async Task<Cabinet> FactoryAsync(string style, string name, string path, string controlScheme, string modelFilePath,
                                    int number, string room, Vector3 position,
                                    Quaternion rotation, Transform parent,
                                    bool cacheGlbModels = true)
    {

        ConfigManager.WriteConsole($"[CabinetFactory] Loading Cabinet style:{style} name:{name} modelFilePath:{modelFilePath} number:{number} room:{room}");


        GameObject model;
        if (!String.IsNullOrEmpty(modelFilePath))
        {
            // Derive cabinet path and filename for metadata-backed hash + size tracking.
            string modelDirectory = Path.GetFileName(Path.GetDirectoryName(modelFilePath));
            string modelFileName = Path.GetFileName(modelFilePath);
            string cabPath = Path.Combine(ConfigManager.CabinetsDB, modelDirectory);
            CabinetMetadata cabinetMetadata = CabinetMetadata.fromName(modelDirectory);
            cabinetMetadata.verifyAndRefreshHash(cabPath, modelFileName);
            string cacheKey = cabinetMetadata.getHash(modelFileName);
            ConfigManager.WriteConsole($"[CabinetFactory] cab:{name} cache key:{cacheKey}");

            if (cacheKey != null && CabinetStyles.ContainsKey(cacheKey))
            {
                ConfigManager.WriteConsole($"[CabinetFactory] load default model {modelFilePath}");
                model = CabinetStyles[cacheKey];
            }
            else if (cacheGlbModels && cacheKey != null && ConfigManager.CabinetCache.ContainsKey(cacheKey))
            {
                ConfigManager.WriteConsole($"[CabinetFactory] load cached model {modelFilePath}");
                model = ConfigManager.CabinetCache.Get(cacheKey);
            }
            else
            {
                try
                {
                    // Cache shaders on the main thread before async load.
                    // Shader.Find() cannot be called from a background thread — without this,
                    // GLTFUtility resolves shaders to null and textures never appear.
                    ImportSettings importSettings = new ImportSettings();
                    importSettings.shaderOverrides.CacheDefaultShaders();

                    TaskCompletionSource<GameObject> tcs = new TaskCompletionSource<GameObject>();
                    Importer.LoadFromFileAsync(modelFilePath, importSettings, (loadedGo, animationClips) =>
                    {
                        tcs.SetResult(loadedGo);
                    }, null);
                    model = await tcs.Task;
                    model.SetActive(false);

                    // GLTFUtility's Importer.LoadAsync() calls GetRoot() twice for GLBs with 2+
                    // top-level nodes, leaking an empty "Root" wrapper at scene root. Detect it by
                    // name + structure (only a Transform, no children) — safe even under concurrent
                    // loads because a legitimate model always has children.
                    foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
                    {
                        if (go.name == "Root"
                            && go != model
                            && go.transform.childCount == 0
                            && go.GetComponents<Component>().Length == 1)
                        {
                            ConfigManager.WriteConsole($"[CabinetFactory] Destroying leaked GLTFUtility root: {go.name}");
                            GameObject.DestroyImmediate(go);
                        }
                    }
                    
                    // Compress GLB embedded textures — same pipeline as cabinet art.
                    // Gated on DeviceController.originalTextures (same flag as CabinetTextureCache).
                    // tex.Apply(false, true) frees the CPU copy after GPU upload, halving per-texture memory.
                    if (!DeviceController.originalTextures)
                    {
                        var seen = new HashSet<int>();
                        foreach (Renderer r in model.GetComponentsInChildren<Renderer>(true))
                        {
                            if (r.sharedMaterials == null) continue;
                            foreach (Material mat in r.sharedMaterials)
                            {
                                if (mat == null) continue;
                                foreach (string prop in mat.GetTexturePropertyNames())
                                    if (mat.GetTexture(prop) is Texture2D tex
                                        && tex.isReadable
                                        && seen.Add(tex.GetInstanceID())
                                        && tex.width % 4 == 0 && tex.height % 4 == 0)
                                    {
                                        tex.Compress(false);
                                        tex.Apply(false, true);
                                    }
                            }
                        }
                    }
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
                        // Use persisted in-memory size; calculate and persist on first load of this GLB version.
                        float actualMB = cabinetMetadata.getSize(cacheKey);
                        if (actualMB <= 0f)
                        {
                            actualMB = CalculateGameObjectSizeBytes(model) / (1024f * 1024f);
                            cabinetMetadata.setSize(cacheKey, actualMB);
                            cabinetMetadata.save(cabPath);
                        }
                        ConfigManager.WriteConsole($"[CabinetFactory] model {modelFilePath} memory: {actualMB:F2}MB");

                        // Add returns the existing value if the key is already present.
                        // A concurrent FactoryAsync call for the same GLB may have won the race
                        // and already populated the cache while we were loading. In that case,
                        // destroy our orphaned copy and use the cached one.
                        GameObject accepted = ConfigManager.CabinetCache.Add(cacheKey, model, actualMB);
                        if (accepted != model)
                        {
                            ConfigManager.WriteConsole($"[CabinetFactory] concurrent load race: destroying orphaned model {modelFilePath}");
                            GameObject.Destroy(model);
                            model = accepted;
                        }
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


        return new Cabinet(cabinetName, path, controlScheme, position, rotation, parent, go: model);
    }

    // ████████████████████████████████████████████████████████████████████████████
    // !! DO NOT USE Profiler.GetRuntimeMemorySizeLong() HERE — EVER AGAIN !!
    //
    // REASON: After GPU upload (tex.Apply(false, true) or Mesh.UploadMeshData(true)),
    //         the CPU copy is destroyed. GetRuntimeMemorySizeLong() cannot see GPU
    //         memory and returns 0, making the LRU cache believe the model uses 0 MB.
    //         Result: eviction never fires and memory grows unbounded.
    //         Same confirmed failure as textures in CabinetTextureCache (2026-03-31).
    //
    // SOLUTION: Manually sum vertex/index buffer sizes for meshes (these metadata fields
    //           stay valid after GPU upload) and delegate to
    //           CabinetTextureCache.CalculateActualSizeBytes() for textures.
    // ████████████████████████████████████████████████████████████████████████████
    private static long CalculateGameObjectSizeBytes(GameObject go)
    {
        long total = 0;
        var seenMeshes = new HashSet<int>();
        var seenTextures = new HashSet<int>();

        foreach (MeshFilter mf in go.GetComponentsInChildren<MeshFilter>(true))
        {
            Mesh m = mf.sharedMesh;
            if (m == null || !seenMeshes.Add(m.GetInstanceID())) continue;
            // Vertex buffer — stride × count (CPU + GPU copies both exist until UploadMeshData(true))
            total += (long)m.vertexCount * m.GetVertexBufferStride(0);
            // Index buffer — 2 or 4 bytes per index
            total += (long)m.GetIndexCount(0) * (m.indexFormat == IndexFormat.UInt16 ? 2 : 4);
        }

        foreach (Renderer r in go.GetComponentsInChildren<Renderer>(true))
        {
            if (r.sharedMaterials == null) continue;
            foreach (Material mat in r.sharedMaterials)
            {
                if (mat == null) continue;
                foreach (string prop in mat.GetTexturePropertyNames())
                    if (mat.GetTexture(prop) is Texture2D tex
                        && seenTextures.Add(tex.GetInstanceID()))
                        total += (long)CabinetTextureCache.CalculateActualSizeBytes(tex);
            }
        }

        return total;
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
                    cp.SubType = p.subType;
                    if (p.properties.Count > 0)
                    {
                        CabinetMaterials.MaterialPropertyTranslator t;
                        switch (p.subType)
                        {
                            case "simple":
                                t = new CabinetMaterials.FrontGlassCutOutProperties();
                                break;
                            case "dirty glass":
                                t = new CabinetMaterials.FrontGlassDirtyProperties();
                                break;
                            default:
                                t = new CabinetMaterials.FrontGlassProperties();
                                break;
                        }
                        cp.ApplyUserMaterialConfiguration(t.Translate(p.properties));
                    }
                    cp.SetBezel(cbinfo.getPath(p.art.file), p.subType);
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


                    cp.SetMarqueeEmissionColor(p.color, p.marquee?.color);

                    if (p.properties.Count > 0)
                    {
                        CabinetMaterials.MaterialPropertyTranslator t = new CabinetMaterials.MarqueeProperties();
                        cp.ApplyUserMaterialConfiguration(t.Translate(p.properties));
                    }

                    if (p.art != null)
                        cp.SetTextureTo(p.name, cbinfo.getPath(p.art.file), mat, invertX: p.art.invertx, invertY: p.art.inverty);
                    else
                        cp.SetMaterial(mat);
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
                    bool withoutMaterial = string.IsNullOrEmpty(p.material);
                    bool withoutNormal = string.IsNullOrEmpty(p.normal);

                    if (withoutMaterial && withoutNormal &&
                        p.art == null &&
                        p.color == null &&
                        p.emission == null &&
                        p.transparency == 0)
                    {
                        //cp.SetMaterial(CabinetMaterials.BlackNoNormal);
                        Color32 black = new Color32(0, 0, 0, 0);
                        cp.SetColorVertex(black, false);
                    }

                    else if (withoutMaterial &&
                            p.art == null &&
                            p.color != null &&
                            p.emission == null &&
                            p.transparency == 0)
                    {
                        //vertex color optimization
                        CabinetMaterials.MaterialPropertyTranslator t = CabinetMaterials.PropertyTranslator("Vertex Color");

                        if (!withoutNormal)
                        {
                            cp.SetColorVertex(p.color.getColor(), true);
                            string realProperty = t.GetRealPropertyName("normal");
                            if (!string.IsNullOrEmpty(realProperty))
                                cp.SetNormal(p.normal, realProperty);
                        }
                        else
                            cp.SetColorVertex(p.color.getColor(), false);

                        if (p.properties.Count > 0)
                            cp.ApplyUserMaterialConfiguration(t.Translate(p.properties));
                    }
                    else
                    {
                        int pos = cabinet.PartsPosition(p.name); //performance

                        ConfigManager.WriteConsole($"[CabinetFactory.skinCabinetPart] #{pos} {p.name}: type: {p.type} material: {p.material} color: {p.color} transp:{p.transparency} emission: {p.emission}");

                        CabinetMaterials.MaterialPropertyTranslator propTranslator;

                        // material base assignment
                        if (!withoutMaterial)
                        {
                            propTranslator = CabinetMaterials.PropertyTranslator(p.material);
                            cp.SetMaterialFrom(CabinetMaterials.fromName(p.material));
                        }
                        else if (!withoutNormal)
                        {
                            propTranslator = CabinetMaterials.PropertyTranslator("base");
                            cp.ForceMaterialBaseNormal();
                        }
                        else
                        {
                            propTranslator = CabinetMaterials.PropertyTranslator("base");
                            cp.ForceMaterialBase();
                        }

                        if (!withoutNormal)
                        {
                            string realProperty = propTranslator.GetRealPropertyName("normal");
                            if (!string.IsNullOrEmpty(realProperty))
                            {
                                cp.SetNormal(p.normal, realProperty);
                                //cp.SetNormal(p.normal, "_ParallaxMap");
                                //cp.SetNormalHeight();
                            }

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


                        if (p.emission != null)
                        {
                            //assign the texture first
                            if (!string.IsNullOrEmpty(p.emission.art?.file))
                                cp.SetEmissive()
                                  .SetEmissionColor(p.emission.color.getColor())
                                  .ActivateEmission(p.emission.emissive)
                                  .SetEmissionTextureTo(cbinfo.getPath(p.emission.art.file), //coroutine
                                                        invertX: p.emission.art.invertx,
                                                        invertY: p.emission.art.inverty);

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
                try
                {
                    skinCabinetPart(cabinet, cbinfo, p);
                }
                catch (Exception e)
                {
                    ConfigManager.WriteConsoleException($"[CabinetFactory.fromInformation] {cbinfo.name} skinning {p.name}.", e);
                }
            }
        }

        return cabinet;
    }

    public static async Task<Cabinet> fromInformationAsync(CabinetInformation cbinfo, string room, int number,
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

        CabinetDBAdmin.MoveMameFiles(cbinfo); //and delete sources

        Cabinet cabinet = await CabinetFactory.FactoryAsync(cbinfo.style, cbinfo.name, cbinfo.pathBase, cbinfo.controlScheme, modelFilePath,
                                                    number, room, position, rotation, parent,
                                                    cacheGlbModels: cacheGlbModels);

        //box colliders
        //addRigidBody();
        // cbinfo.debug = true;
        BoxCollider boxCollider = cabinet.addBoxCollider(false);
        cabinet.toFloor();
        try
        {
            //assign a material to all the components that aren't in the 
            //description's parts list.
            if (cbinfo.material != null)
            {
                //hack to avoid normals materials on default black cabinets.
                if (cbinfo.material.ToLower() == "black")
                {
                    // most used material by default, change it by a vertex.
                    Material mat = CabinetMaterials.BlackNoNormal;
                    cabinet.SetMaterialToUnknownComponents(mat, cbinfo); //only if the part doesn't have a material assigned
                    /*Color32 black = new Color32(0, 0, 0, 0);
                    Material mat = new Material(CabinetMaterials.VertexColor);
                    cabinet.SetVertexColorToUnknownComponents(black, cbinfo, mat); //only if the part doesn't have a material assigned
                    */
                }
                else
                {
                    Material mat = CabinetMaterials.fromName(cbinfo.material);
                    cabinet.SetMaterialToUnknownComponents(mat, cbinfo); //only if the part doesn't have a material assigned
                }
            }
            else if (cbinfo.color != null)
            {
                //Material mat = new Material(CabinetMaterials.Base);
                //mat.SetColor("_Color", cbinfo.color.getColor());
                Color32 color = cbinfo.color.getColor();
                cabinet.SetVertexColorToUnknownComponents(color, cbinfo); //only if the part doesn't have a material assigned
            }
            else
            {
                //fall to black any other component.
                Color32 black = new Color32(0, 0, 0, 0);
                cabinet.SetVertexColorToUnknownComponents(black, cbinfo); //only if the part doesn't have a material assigned
            }

        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[CabinetFactory.fromInformation] {cbinfo.name} assigning material.", e);
        }


        if (!string.IsNullOrEmpty(cbinfo.coinslot))
        {
            try
            {
                ConfigManager.WriteConsole($"[CabinetFactory.fromInformation] {cbinfo.name} coinslot {cbinfo.coinslot}");
                cabinet.AddCoinSlot(cbinfo.coinslot,
                        cbinfo.coinslotgeometry.rotation.x, cbinfo.coinslotgeometry.rotation.y, cbinfo.coinslotgeometry.rotation.z,
                        cbinfo.coinslotgeometry.scalepercentage,
                        cbinfo.coinslotSound);
            }
            catch (Exception e)
            {
                ConfigManager.WriteConsoleException($"[CabinetFactory.fromInformation] {cbinfo.name} setting coinslot.", e);
            }
        }

        try
        {
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
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[CabinetFactory.fromInformation] {cbinfo.name} assigning screen.", e);
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

    // MR: synchronous spawn with world pose (MRLayoutRegistry); wraps fromInformationAsync on main thread.
    public static Cabinet fromInformation(CabinetInformation cbinfo, string room, int number,
                                             Vector3 position, Quaternion rotation, Transform parent,
                                            List<AgentScenePosition> agentPlayerPositions,
                                            BackgroundSoundController backgroundSoundController,
                                            bool cacheGlbModels = true)
    {
        return fromInformationAsync(cbinfo, room, number, position, rotation, parent,
                                    agentPlayerPositions, backgroundSoundController, cacheGlbModels)
            .GetAwaiter()
            .GetResult();
    }
}
