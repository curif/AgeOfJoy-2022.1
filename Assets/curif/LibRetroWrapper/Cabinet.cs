/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static CabinetInformation;

public class Cabinet
{
    public string Name = "";
    public string ControlScheme = "";
    public GameObject gameObject;

    //Al the parts in the gabinet that must exists, can be others.
    // static List<string> RequiredParts = new List<string>() { "screen-mock-vertical", "screen-mock-horizontal" };
    //Known parts that can exists or not.
    static List<string> NonStandardParts = new List<string>() { "Marquee", "bezel", "coin-slot", "screen" };
    //those parts that the user can configure, but is not limited to.
    public static List<string> userStandarConfigurableParts = new List<string>() { "front", "left", "right", "joystick", "joystick-down", "screen-base" };

    public LayerMask layerPart = LayerMask.NameToLayer("LightGunTarget");
    // load a texture from disk.
    private static Texture2D LoadTexture(string filePath)
    {

        /*
        Texture2D tex = null;
        byte[] fileData;
        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2, TextureFormat.RGB565, true);
            tex.filterMode = FilterMode.Trilinear; //provides better mip transitions in VR
            tex.mipMapBias = -0.3f; // setting mip bias to around -0.7 in Unity is recommended by meta for high-detail textures
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        */
        return CabinetTextureCache.LoadAndCacheTexture(filePath);
    }

    // public bool IsValid
    // {
    //     get
    //     {
    //         foreach (string key in RequiredParts)
    //             if (!PartsExist(key))
    //                 return false;
    //         return true;
    //     }
    // }

    private void addRigidBody()
    {

        Rigidbody cabRB = gameObject.AddComponent<Rigidbody>(); //this will excecute Start().
        cabRB.freezeRotation = true;
        cabRB.isKinematic = true;
        cabRB.useGravity = false;
        cabRB.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationZ
                            | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationX
                            | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationY;

    }

    public void AddAColliderBlocker(string part, bool debugmode = false)
    {
        GameObject blocker = Parts(part);
        if (blocker == null)
            return;

        Renderer renderer = blocker.GetComponent<Renderer>();
        if (renderer != null)
        {
            // get/create a collider on the blocker. The collider will adjust to the blocker size.
            BoxCollider collider = blocker.GetComponent<BoxCollider>();
            if (collider == null)
                collider = blocker.AddComponent<BoxCollider>();

            collider.includeLayers = LayerMask.GetMask("Player");

            renderer.enabled = false;
            if (debugmode)
                DrawBoxCollider(collider);
        }
    }
    /*
        public BoxCollider addBoxCollider()
        {
            BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
            if (boxCollider == null)
                boxCollider = gameObject.AddComponent<BoxCollider>();

            Renderer[] childRenderers = gameObject.GetComponentsInChildren<Renderer>();

            if (childRenderers.Length == 0)
            {
                ConfigManager.WriteConsoleWarning($"[addBoxCollider] {gameObject.name} No child renderers found.");
                return boxCollider;
            }

            Bounds bounds = childRenderers[0].bounds;

            for (int i = 1; i < childRenderers.Length; i++)
            {
                bounds.Encapsulate(childRenderers[i].bounds);
            }

            boxCollider.size = bounds.size;
            // Adjust the center and size of the box collider to match the parent game object
            Vector3 centerOffset = bounds.center - gameObject.transform.position;
            Vector3 adjustedCenter = new Vector3(centerOffset.x / gameObject.transform.lossyScale.x,
                                                  centerOffset.y / gameObject.transform.lossyScale.y,
                                                  centerOffset.z / gameObject.transform.lossyScale.z);
            boxCollider.center = adjustedCenter;

            return boxCollider;
        }
    */

    void DrawBounds(Bounds bounds)
    {
        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.001f; // Set the width of the line
        lineRenderer.endWidth = 0.001f;
        lineRenderer.positionCount = 8;
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;

        Vector3[] corners = new Vector3[8];

        // Get the corners of the bounding box
        corners[0] = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z);
        corners[1] = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, -bounds.extents.z);
        corners[2] = bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, -bounds.extents.z);
        corners[3] = bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z);

        corners[4] = bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, bounds.extents.z);
        corners[5] = bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, -bounds.extents.z);
        corners[6] = bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, -bounds.extents.z);
        corners[7] = bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, bounds.extents.z);

        for (int i = 0; i < corners.Length; i++)
        {
            lineRenderer.SetPosition(i, corners[i]);
        }

        // Connect the corners to form the Bounds lines
        lineRenderer.loop = true;
    }
    void DrawBoxCollider(BoxCollider collider)
    {
        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.positionCount = 8;
        lineRenderer.startColor = Color.blue;
        lineRenderer.endColor = Color.blue;

        lineRenderer.startWidth = 0.001f; // Set the width of the line
        lineRenderer.endWidth = 0.001f;

        Vector3 center = collider.bounds.center;
        Vector3 size = collider.size / 2;

        // Get the corners of the BoxCollider
        Vector3[] corners = new Vector3[8];
        corners[0] = center + new Vector3(size.x, size.y, size.z);
        corners[1] = center + new Vector3(size.x, size.y, -size.z);
        corners[2] = center + new Vector3(-size.x, size.y, -size.z);
        corners[3] = center + new Vector3(-size.x, size.y, size.z);

        corners[4] = center + new Vector3(size.x, -size.y, size.z);
        corners[5] = center + new Vector3(size.x, -size.y, -size.z);
        corners[6] = center + new Vector3(-size.x, -size.y, -size.z);
        corners[7] = center + new Vector3(-size.x, -size.y, size.z);

        // Draw lines between corners to visualize the BoxCollider
        for (int i = 0; i < corners.Length; i++)
        {
            lineRenderer.SetPosition(i, corners[i]);
        }

        // Connect the corners to form the BoxCollider lines
        lineRenderer.loop = true;
    }
    public BoxCollider addBoxCollider(bool debugmode = false)
    {
        //https://www.reddit.com/r/Unity3D/comments/8qma81/how_to_add_a_collider_at_runtime_to_an_object/
        Quaternion currentRotation = gameObject.transform.rotation;
        gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        Vector3 currentScale = gameObject.transform.localScale;
        gameObject.transform.localScale = Vector3.one;

        BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
        if (boxCollider == null)
            boxCollider = gameObject.AddComponent<BoxCollider>();

        Renderer[] childRenderers = gameObject.GetComponentsInChildren<Renderer>();
        if (childRenderers.Length == 0)
        {
            ConfigManager.WriteConsoleWarning($"[addBoxCollider] {gameObject.name} No child renderers found.");
            return boxCollider;
        }

        Bounds bounds = new Bounds(gameObject.transform.position, Vector3.zero);
        foreach (Renderer renderer in childRenderers)
        {
            bounds.Encapsulate(renderer.bounds);
            if (debugmode)
                DrawBounds(renderer.bounds);
        }

        // Set the size of the box collider to match the bounds.
        boxCollider.size = bounds.size;

        // Calculate the center offset relative to the transform position.
        Vector3 centerOffset = bounds.center - gameObject.transform.position;
        boxCollider.center = centerOffset;

        gameObject.transform.rotation = currentRotation;
        gameObject.transform.localScale = currentScale;

        if (debugmode)
            DrawBoxCollider(boxCollider);

        return boxCollider;
    }

    /*
    Another approach:
    private void addBoxCollider(GameObject gameObject)
    {
        BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
        if (boxCollider == null)
            boxCollider = gameObject.AddComponent<BoxCollider>();

        Renderer[] childRenderers = gameObject.GetComponentsInChildren<Renderer>();

        if (childRenderers.Length == 0)
        {
            ConfigManager.WriteConsoleWarning($"[addBoxCollider] {gameObject.name} No child renderers found.");
            return;
        }

        // Initialize minimum and maximum corner points in world space
        Vector3 minWorldCorner = childRenderers[0].bounds.min;
        Vector3 maxWorldCorner = childRenderers[0].bounds.max;

        for (int i = 1; i < childRenderers.Length; i++)
        {
            Vector3 rendererMin = childRenderers[i].bounds.min;
            Vector3 rendererMax = childRenderers[i].bounds.max;

            // Update min and max corner points in world space
            minWorldCorner = Vector3.Min(minWorldCorner, rendererMin);
            maxWorldCorner = Vector3.Max(maxWorldCorner, rendererMax);
        }

        // Convert world space corner points to local space
        Vector3 minLocalCorner = gameObject.transform.InverseTransformPoint(minWorldCorner);
        Vector3 maxLocalCorner = gameObject.transform.InverseTransformPoint(maxWorldCorner);

        // Calculate the center and size of the box collider in local space
        Vector3 center = (minLocalCorner + maxLocalCorner) * 0.5f;
        Vector3 size = maxLocalCorner - minLocalCorner;

        // Adjust the center based on the parent's scale
        Vector3 adjustedCenter = new Vector3(center.x / gameObject.transform.localScale.x,
                                              center.y / gameObject.transform.localScale.y,
                                              center.z / gameObject.transform.localScale.z);
        boxCollider.center = adjustedCenter;
        boxCollider.size = size;
    }
    */


    public PutOnFloor toFloor()
    {
        PutOnFloor onFloor = gameObject.GetComponent<PutOnFloor>();
        if (onFloor == null)
            onFloor = gameObject.AddComponent<PutOnFloor>();
        return onFloor;
    }

    public Cabinet(string name, string controlScheme, Vector3 position, Quaternion rotation, Transform parent,
                        GameObject go = null, string model = "galaga")
    {
        Name = name;
        ControlScheme = controlScheme;
        if (go == null)
        {
            // Assets/Resources/Cabinets/PreFab/xxx.prefab
            go = Resources.Load<GameObject>($"Cabinets/PreFab/{model}");
            if (go == null)
                throw new System.Exception($"Cabinet tyle {Name} not found or doesn't load");
        }

        //https://docs.unity3d.com/ScriptReference/Object.Instantiate.html
        gameObject = GameObject.Instantiate<GameObject>(go, position, rotation, parent);
        gameObject.name = name;

        // if (!IsValid)
        //     throw new System.Exception($"[Cabinet] Malformed Cabinet {Name} , some parts are missing. List of expected parts: {string.Join(",", RequiredParts)}");

        //SetMaterial(CabinetMaterials.Black);
        if (PartsExist("bezel"))
            SetMaterial("bezel", CabinetMaterials.FrontGlassWithBezel);

        //cached gameObjects could be inactive.
        gameObject.SetActive(true);
    }

    public GameObject this[string part]
    {
        get
        {
            return Parts(part);
        }
    }
    public GameObject Parts(string partName)
    {
        GameObject go = PartsOrNull(partName);
        if (go == null)
            throw new Exception($"Unknown cabinet part: {partName}");
        return go;
    }
    public GameObject Parts(int partNum)
    {
        GameObject go = PartsOrNull(partNum);
        if (go == null)
            throw new Exception($"Unknown cabinet part: #{partNum}");
        return go;
    }
    public GameObject PartsOrNull(string partName)
    {
        Transform childTransform = gameObject.transform.Find(partName);
        return childTransform?.gameObject;
    }
    public GameObject PartsOrNull(int partNum)
    {
        Transform childTransform = gameObject.transform.GetChild(partNum);
        return childTransform?.gameObject;
    }
    public Transform PartsTransform(string partName)
    {
        Transform childTransform = gameObject.transform.Find(partName);
        if (childTransform == null)
            throw new Exception($"Unknown cabinet part: {partName}");
        return childTransform;
    }
    public Transform PartsTransform(int partNum)
    {
        Transform childTransform = gameObject.transform.GetChild(partNum);
        if (childTransform == null)
            throw new Exception($"Unknown cabinet part: #{partNum}");
        return childTransform;
    }
    public string PartsName(int partNum)
    {
        Transform t = PartsTransform(partNum);
        return t.name;
    }
    public int PartsPosition(string partName)
    {
        Transform t = PartsTransform(partName);
        return t.GetSiblingIndex();
    }
    public bool PartsExist(string part)
    {
        return gameObject.transform.Find(part) != null;
    }
    public bool PartsExist(int partNum)
    {
        return partNum >= 0 && gameObject.transform.childCount < partNum + 1;
    }
    public int PartsCount()
    {
        return gameObject.transform.childCount;
    }
    public Cabinet ScalePart(string partName, float percentage, float xratio, float yratio, float zratio)
    {
        Scale(Parts(partName), percentage, xratio, yratio, zratio);
        return this;
    }
    public Cabinet RotatePart(string partName, float angleX, float angleY, float angleZ)
    {
        Parts(partName).transform.Rotate(angleX, angleY, angleZ);
        return this;
    }
    public Cabinet EnablePart(string cabinetPart, bool enabled)
    {
        Parts(cabinetPart).SetActive(enabled);
        return this;
    }
    public Cabinet EnablePart(int cabinetNum, bool enabled)
    {
        Parts(cabinetNum).SetActive(enabled);
        return this;
    }
    public Cabinet SetColorPart(string partName, Color color)
    {
        SetColor(Parts(partName), color);
        return this;
    }

    //assign the Base material
    public Cabinet PartNeedsAMaterialBase(int partNum)
    {
        GameObject go = Parts(partNum);
        if (go == null)
           return this;

        SetMaterialFromMaterial(go, CabinetMaterials.Base, OnlyAssignIfDoesntHaveOne: false);
        
        return this;
    }


    //assign the Base material if doesn't have any.
    public Cabinet PartNeedsAMaterial(int partNum)
    {
        GameObject go = Parts(partNum);
        if (go == null)
            return this;

        SetMaterialFromMaterial(go, CabinetMaterials.Base, OnlyAssignIfDoesntHaveOne: true);

        return this;
    }

    public Cabinet SetColorPart(int partNum, Color color)
    {
        SetColor(Parts(partNum), color);
        return this;
    }
    public Cabinet SetColorVertexPart(int partNum, Color color)
    {
        SetColorVertex(Parts(partNum), color);
        return this;
    }
    public int GetTransparencyPart(string cabinetPart)
    {
        return GetTransparency(Parts(cabinetPart));
    }
    public int GetTransparencyPart(int partNum)
    {
        return GetTransparency(Parts(partNum));
    }
    public Cabinet SetTransparencyPart(int partNum, int transpPercent)
    {
        SetTransparency(Parts(partNum), ref transpPercent);
        return this;
    }
    public Cabinet SetTransparencyPart(string cabinetPart, int transpPercent)
    {
        SetTransparency(Parts(cabinetPart), ref transpPercent);
        return this;
    }
    public Cabinet SetEmissionEnabledPart(int partNum, bool enabled)
    {
        SetEmissionEnabled(Parts(partNum), enabled);
        return this;
    }
    public Cabinet SetEmissionEnabledPart(string cabinetPart, bool enabled)
    {
        SetEmissionEnabled(Parts(cabinetPart), enabled);
        return this;
    }
    public Cabinet SetEmissionColorPart(int partNum, Color emissionColor)
    {
        SetEmissionColor(Parts(partNum), emissionColor);
        return this;
    }
    public Cabinet SetEmissionColorPart(string cabinetPart, Color emissionColor)
    {
        SetEmissionColor(Parts(cabinetPart), emissionColor);
        return this;
    }


    //change a material, or create a new one and change it.
    public Cabinet SetTextureTo(string partName, string textureFile, Material mat,
                                    bool invertX = false, bool invertY = false)
    {
        if (!string.IsNullOrEmpty(textureFile))
            SetTextureFromFile(Parts(partName), textureFile, mat, invertX, invertY);
        return this;
    }
    public Cabinet SetTextureTo(int partNum, string textureFile, Material mat,
                                    bool invertX = false, bool invertY = false)
    {
        if (!string.IsNullOrEmpty(textureFile))
            SetTextureFromFile(Parts(partNum), textureFile, mat, invertX, invertY);
        return this;
    }
    public Cabinet SetEmissionTextureTo(int partNum, string textureFile,
                                        bool invertX = false, bool invertY = false)
    {
        if (!string.IsNullOrEmpty(textureFile))
            SetEmissionTextureFromFile(Parts(partNum), textureFile, invertX, invertY);
        return this;
    }

    public static void Scale(GameObject go, float percentage, float xratio, float yratio, float zratio)
    {
        float scale = percentage / 100f;
        Vector3 localScale = go.transform.localScale;
        go.transform.localScale = new Vector3(localScale.x * xratio * scale,
                                              localScale.y * yratio * scale,
                                              localScale.z * zratio * scale);
    }

    public static void Scale(GameObject go, float percentage)
    {
        Scale(go, percentage, 1, 1, 1);
    }

    public static void SetTextureFromFile(GameObject go, string textureFile,
                                            Material mat, bool invertX, bool invertY)
    {
        Renderer r = go.GetComponent<Renderer>();
        if (r == null)
            return;

        Material m;
        if (mat == null)
            m = r.material;
        else
        {
            m = new Material(mat);
            m.name = $"{go.name}_from_{mat.name}";
        }

        //tiling
        Vector2 mainTextureScale = new Vector2(1, 1);
        if (invertX)
            mainTextureScale.x = -1;
        if (invertY)
            mainTextureScale.y = -1;
        m.mainTextureScale = mainTextureScale;

        //main texture
        Texture2D t = LoadTexture(textureFile);
        if (t == null)
        {
            ConfigManager.WriteConsoleError($"Cabinet {go.name} texture error {textureFile}");
            return;
        }
        else
            m.SetTexture("_MainTex", t);

        r.material = m;
    }
    public static void SetEmissionTextureFromFile(GameObject go, string textureFile,
                                                     bool invertX = false, bool invertY = false)
    {
        Renderer r = go.GetComponent<Renderer>();
        if (r == null)
            return;

        Material m = r.material;
        if (m == null)
            return;

        // Tiling
        Vector2 emissionTextureScale = new Vector2(1, 1);
        if (invertX)
        {
            emissionTextureScale.x = -1;
        }
        if (invertY)
        {
            emissionTextureScale.y = -1;
        }
        m.SetTextureScale("_EmissionMap", emissionTextureScale);

        // Emission texture
        Texture2D t = LoadTexture(textureFile);
        if (t == null)
        {
            ConfigManager.WriteConsoleError($"Error loading emission texture for {go.name}: {textureFile}");
            return;
        }
        else
        {
            m.SetTexture("_EmissionMap", t);
        }

        r.material = m;
    }

    public static void SetColor(GameObject go, Color color)
    {
        Renderer r = go.GetComponent<Renderer>();
        if (r == null)
            return;

        Material mat = r.material;
        if (mat != null)
            mat.color = color;
        return;
    }

    // @Gometrizer technique to colorize without a material.
    public static void SetColorVertex(GameObject go, Color color)
    {
        SetMaterialFromMaterial(go, CabinetMaterials.VertexColor);

        // Retrieve all MeshFilter components in the GameObject and its children
        MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>();
        if (meshFilters.Length > 0)
        {
            foreach (MeshFilter meshFilter in meshFilters)
            {
                if (meshFilter != null && meshFilter.mesh != null)
                {
                    Mesh mesh = meshFilter.mesh;
                    mesh.MarkDynamic();

                    Color[] colors = new Color[mesh.vertexCount];
                    System.Array.Fill(colors, color);
                    mesh.colors = colors; // Apply the colors to the mesh
                }
                
            }
        }
    }

    public static void SetEmissionColor(GameObject go, Color emissionColor)
    {
        Renderer r = go.GetComponent<Renderer>();
        if (r == null)
            return;

        Material m = r.material;
        if (m == null)
            return;

        // Set emission color
        m.SetColor("_EmissionColor", emissionColor);
        Texture mainTexture = r.sharedMaterial.mainTexture;
        if (mainTexture != null)
            // Set the emission map to be the same as the main texture
            r.sharedMaterial.SetTexture("_EmissionMap", mainTexture);

        r.material = m;
    }

    public static void SetEmissionEnabled(GameObject go, bool enabled)
    {
        Renderer r = go.GetComponent<Renderer>();
        if (r == null)
            return;

        Material m = r.material;
        if (m == null)
            return;

        // Set emission enabled
        m.EnableKeyword("_EMISSION"); // Enable emission if true
        if (enabled)
        {
            Texture mainTexture = r.sharedMaterial.mainTexture;
            if (mainTexture != null)
                // Set the emission map to be the same as the main texture
                r.sharedMaterial.SetTexture("_EmissionMap", mainTexture);
        }
        else
            m.DisableKeyword("_EMISSION"); // Disable emission if false

        r.material = m;
    }

    public static int GetTransparency(GameObject go)
    {
        Renderer r = go.GetComponent<Renderer>();
        if (r == null)
            return 0; // Default to 0% transparency if no Renderer found

        Material mat = r.material;
        if (mat == null)
            return 0; // Default to 0% transparency if no Material found

        return (int)(mat.color.a * 100f);
    }

    public static void SetTransparency(GameObject go, ref int transpPercent)
    {
        Renderer r = go.GetComponent<Renderer>();
        if (r == null)
            return;

        Material mat = r.material;
        if (mat == null)
            return;

        if (transpPercent < 0)
            transpPercent = 0;
        if (transpPercent > 100)
            transpPercent = 100;

        Color currentColor = mat.color;

        float alpha = (float)transpPercent / 100f;

        currentColor.a = alpha;
        mat.color = currentColor;
    }

    public static void SetMaterialFromMaterial(GameObject go, Material mat, bool                                                                        OnlyAssignIfDoesntHaveOne = false)
    {
        Renderer r = go.GetComponent<Renderer>();
        if (r == null)
            return;

        if (r.material != null && OnlyAssignIfDoesntHaveOne)
            return;

        Material m = new Material(mat);
        m.name = $"{go.name}_from_{mat.name}";
        r.material = mat;
    }

    public Cabinet SetMarquee(string part, string texturePath, Material marqueeMaterial, bool invertX = false, bool invertY = false)
    {
        return SetTextureTo(part, texturePath, marqueeMaterial, invertX: invertX, invertY: invertY);
    }
    public Cabinet SetMarqueeEmissionColor(string part, Color color, float intensity)
    {
        if (!PartsExist(part))
            return this;

        Renderer r = Parts(part).GetComponent<Renderer>();
        if (r != null)
        {
            r.material.SetColor("_EmissionColor", color);
            float oldToNewIntensity = (intensity + 5) / 10;
            r.material.SetFloat("_EmissionIntensity", oldToNewIntensity);
        }

        return this;
    }

    public Cabinet SetBezel(string part, string texturePath, bool invertX = false, bool invertY = false)
    {
        return SetTextureTo(part, texturePath, CabinetMaterials.FrontGlassWithBezel, invertX: invertX, invertY: invertY);
    }

    //set the same material to all components.
    public Cabinet SetMaterial(Material mat)
    {
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (!NonStandardParts.Contains(renderer.gameObject.name))
            {
                renderer.material = mat;
            }
        }
        return this;
    }

    //set a new created material from another one.
    public Cabinet SetMaterialFrom(string part, Material mat)
    {
        return SetMaterialFrom(PartsPosition(part), mat);
    }
    public Cabinet SetMaterialFrom(int partNum, Material mat)
    {
        SetMaterialFromMaterial(Parts(partNum), mat);
        return this;
    }

    //set the material to a component. Don't create new.
    public Cabinet SetMaterial(string partName, Material mat)
    {
        return SetMaterial(PartsPosition(partName), mat);
    }
    public Cabinet SetMaterial(int partNum, Material mat)
    {
        Renderer r = Parts(partNum).GetComponent<Renderer>();
        if (r != null)
            r.material = mat;

        return this;
    }

    //set a part as lightgun target.
    public Cabinet SetLightGunTarget(string partName, LightGunInformation linfo)
    {
        return SetLightGunTarget(PartsPosition(partName), linfo);
    }
    public Cabinet SetLightGunTarget(int partNum, LightGunInformation linfo)
    {
        GameObject part = Parts(partNum);
        LightGunTarget l = part.GetComponent<LightGunTarget>();
        if (l != null)
            return this;

        part.layer = layerPart;

        //needs a Mesh collider
        MeshCollider collider = part.GetComponent<MeshCollider>();
        if (collider == null)
            part.AddComponent<MeshCollider>();
        
        return this;
    }

    public bool IsLightGunTarget(string partName)
    {
        return IsLightGunTarget(PartsPosition(partName));
    }
    public bool IsLightGunTarget(int partNum)
    {
        return Parts(partNum).layer == layerPart;
    }
    public static string CRTName(string cabinetName, string gameFile)
    {
        return "screen-" + cabinetName + "-" + gameFile;
    }

    //used instead of addCRT for cabinets without a CRT
    public Cabinet addController(string pathBase,
                                     ControlMapConfiguration cabinetControlMap = null,
                                     LightGunInformation lightGunInformation = null,
                                     CabinetAGEBasicInformation agebasic = null,
                                     BackgroundSoundController backgroundSoundController = null
                                     )
    {
        GameObject newCRT = CRTsFactory.Instantiate("no-crt", Vector3.zero, Quaternion.identity, gameObject.transform);
        if (newCRT == null)
            throw new System.Exception($"Cabinet {Name} problem: can't create a no-crt controller");

        //LibretroScreenController will find the object using this name:
        newCRT.name = CRTName(Name, "no-crt");
        AGEBasicCabinetController agec = newCRT.GetComponent<AGEBasicCabinetController>();
        agec.PathBase = pathBase;
        agec.cabinet = this;
        //control mapping
        agec.CabinetControlMapConfig = cabinetControlMap;
        agec.lightGunInformation = lightGunInformation;

        //age basic
        agec.ageBasicInformation = agebasic;

        //sound
        agec.backgroundSoundController = backgroundSoundController;
        return this;
    }

    public Cabinet addCRT(CabinetInformation cbinfo, List<AgentScenePosition> _agentPlayerPositions, BackgroundSoundController _backgroundSoundController, Vector3 _rotation)
    {
        string type = cbinfo.crt.type;
        string orientation = cbinfo.crt.orientation;
        string gameFile = cbinfo.rom;
        
        List<string> playList = cbinfo.roms;
        if (playList == null)
            playList = new List<string>() { gameFile };

        int timeToLoad = cbinfo.timetoload;
        string pathBase = cbinfo.pathBase;
        bool invertX = cbinfo.crt.screen.invertx;
        bool invertY = cbinfo.crt.screen.inverty;
        bool EnableSaveState = cbinfo.enablesavestate;
        string StateFile = cbinfo.statefile ?? "state.nv";
        Vector3? rotation = _rotation;
        float crtScalePercentage = cbinfo.crt.geometry?.scalepercentage ?? 100f;
        float crtXratio = cbinfo.crt.geometry?.ratio?.x ?? 1f;
        float crtYratio = cbinfo.crt.geometry?.ratio?.y ?? 1f;
        float crtZratio = cbinfo.crt.geometry?.ratio?.z ?? 1f;
        string gamma = cbinfo.crt.screen.gamma ?? "1.0";
        string brightness = cbinfo.crt.screen.brightness ?? "1.0";
        List<AgentScenePosition> agentPlayerPositions = _agentPlayerPositions;
        string shaderName = cbinfo.crt.screen.shader ?? "damage";
        Dictionary<string, string> shaderConfig = cbinfo.crt.screen.config();
        ControlMapConfiguration cabinetControlMap = cbinfo.ControlMap;
        LightGunInformation lightGunInformation = cbinfo.lightGunInformation;
        CabinetAGEBasicInformation agebasic = cbinfo.agebasic;
        BackgroundSoundController backgroundSoundController = _backgroundSoundController;
        string core = cbinfo.core ?? "mame2003+";
        CoreEnvironment coreEnvironment = cbinfo.environment;
        bool? insertCoinOnStartup = cbinfo.insertCoinOnStartup;
        bool? persistent = cbinfo.persistent;
        Dictionary<uint, LibretroInputDevice> libretroInputDevices = cbinfo.GetLibretroInputDevices();

        string GameVideoFile = null;
        bool GameVideoFileInvertX = false;
        bool GameVideoFileInvertY = false;
        if (cbinfo.video != null)
        {
            GameVideoFile = cbinfo.getPath(cbinfo.video.file);
            GameVideoFileInvertX = cbinfo.video.invertx;
            GameVideoFileInvertY = cbinfo.video.inverty;
        }

        string CRTType = $"screen-mock-{orientation}";
        GameObject CRT = PartsOrNull(CRTType);
        if (CRT == null)
            throw new System.Exception($"Malformed cabinet {Name} problem: mock CRT not found in model. Type: {CRTType}");

        GameObject newCRT = CRTsFactory.Instantiate(type.ToLower(), CRT.transform.position, CRT.transform.rotation, CRT.transform.parent);
        if (newCRT == null)
            throw new System.Exception($"Cabinet {Name} problem: can't create a CRT. Type: {type}");

        //LibretroScreenController will find the object using this name:
        newCRT.name = CRTName(Name, gameFile);

        // rotate and scale
        float scale = crtScalePercentage / 100f;

        newCRT.transform.localScale =
            new Vector3(newCRT.transform.localScale.x * crtXratio * scale,
                        newCRT.transform.localScale.y * crtYratio * scale,
                        newCRT.transform.localScale.z * crtZratio * scale);

        if (rotation != null)
            newCRT.transform.Rotate((Vector3)rotation);

        PartsOrNull("screen-mock-vertical")?.SetActive(false);
        PartsOrNull("screen-mock-horizontal")?.SetActive(false);

        if (type.ToLower() == "19i-agebasic")
        {
            AGEBasicScreenController ageb = newCRT.GetComponent<AGEBasicScreenController>();
            ageb.PathBase = pathBase;
            ageb.cabinet = this;
            ageb.ShaderName = shaderName;
            ageb.ShaderConfig = shaderConfig;

            ageb.AgentPlayerPositions = agentPlayerPositions;

            ageb.InvertX = invertX;
            ageb.InvertY = invertY;
            //video
            ageb.VideoFile = GameVideoFile;
            ageb.VideoInvertX = GameVideoFileInvertX;
            ageb.VideoInvertY = GameVideoFileInvertY;

            //control mapping
            ageb.CabinetControlMapConfig = cabinetControlMap;
            ageb.lightGunInformation = lightGunInformation;

            //age basic
            ageb.ageBasicInformation = agebasic;

            //sound
            ageb.backgroundSoundController = backgroundSoundController;
        }
        else
        {
            //adds a GameVideoPlayer, BoxCollider and a AudioSource to the screen
            LibretroScreenController libretroScreenController = newCRT.GetComponent<LibretroScreenController>();

            libretroScreenController.GameFile = gameFile;
            libretroScreenController.PlayList = playList;
            libretroScreenController.CabEnvironment = coreEnvironment;
            libretroScreenController.SecondsToWaitToFinishLoad = timeToLoad;
            libretroScreenController.EnableSaveState = EnableSaveState;
            libretroScreenController.StateFile = StateFile;
            libretroScreenController.PathBase = pathBase;
            libretroScreenController.InsertCoinOnStartup = insertCoinOnStartup;

            libretroScreenController.GameInvertX = invertX;
            libretroScreenController.GameInvertY = invertY;
            libretroScreenController.Gamma = gamma;
            libretroScreenController.Brightness = brightness;
            libretroScreenController.cabinet = this;
            libretroScreenController.Core = core;
            libretroScreenController.Persistent = persistent;
            libretroScreenController.LibretroInputDevices = libretroInputDevices;

            libretroScreenController.ShaderName = shaderName;
            libretroScreenController.ShaderConfig = shaderConfig;

            libretroScreenController.AgentPlayerPositions = agentPlayerPositions;

            //video
            libretroScreenController.GameVideoFile = GameVideoFile;
            libretroScreenController.GameVideoInvertX = GameVideoFileInvertX;
            libretroScreenController.GameVideoInvertY = GameVideoFileInvertY;

            //control mapping
            libretroScreenController.CabinetControlMapConfig = cabinetControlMap;
            libretroScreenController.lightGunInformation = lightGunInformation;

            //age basic
            libretroScreenController.ageBasicInformation = agebasic;

            //sound
            libretroScreenController.backgroundSoundController = backgroundSoundController;
        }
        return this;
    }

    public Cabinet AddCoinSlot(string type, float rotationAngleX, float rotationAngleY, float rotationAngleZ, float scalePercentage)
    {
        GameObject coinSlotMock = Parts("coin-slot");
        if (coinSlotMock == null)
        {
            ConfigManager.WriteConsole($"[Cabinet.AddCoinSlot] ERROR coin-slot part not present in cabinet {Name}");
            return this;
        }

        GameObject newCoinSlot = CoinSlotsFactory.Instantiate(type, coinSlotMock.transform.position, coinSlotMock.transform.rotation, coinSlotMock.transform.parent);
        if (newCoinSlot != null)
        {
            //LibretroScreenController will find the coinslot using this name:
            newCoinSlot.name = "coin-slot-added";
            ConfigManager.WriteConsole($"[Cabinet.AddCoinSlot] {Name} added part coin-slot-added");
        }
        else
            ConfigManager.WriteConsole($"[Cabinet.AddCoinSlot] ERROR {Name}: can't create the new coin-slot type: {type}");

        // rotate and scale
        float scale = scalePercentage / 100f;
        newCoinSlot.transform.localScale *= scale;
        newCoinSlot.transform.Rotate(rotationAngleX, rotationAngleY, rotationAngleZ);

        UnityEngine.Object.Destroy(coinSlotMock);

        return this;
    }
}

