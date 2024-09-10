//#define ON_DEBUG

using UnityEngine;
using System;
using YamlDotNet.Serialization; //https://github.com/aaubry/YamlDotNet
using YamlDotNet.Serialization.NamingConventions;
using System.IO;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class LightGunInformation
{
    public bool active = false;
    public Gun gun = new();
    public CRT crt = new();
    public Debug debug = new();

    public class Debug
    {
        public bool active = false;
        public string model;
    }
    public class Gun
    {
        public string model;

        [YamlMember(Alias = "invert-pointer", ApplyNamingConventions = false)]
        public bool invertPointer = true;

        [YamlMember(Alias = "adjust-sight", ApplyNamingConventions = false)]
        public GunAdjustHitPoint adjustSight = new();
    }

    public class GunAdjustHitPoint
    {
        public float horizontal;
        public float vertical;
    }

    public class CRT
    {
        [YamlMember(Alias = "mesh-factor-scale-x", ApplyNamingConventions = false)]
        public float meshFactorScaleX = float.NaN;
        [YamlMember(Alias = "mesh-factor-scale-y", ApplyNamingConventions = false)]
        public float meshFactorScaleY = float.NaN;

        [YamlMember(Alias = "border-size-x", ApplyNamingConventions = false)]
        public float borderSizeX = float.NaN;
        [YamlMember(Alias = "border-size-y", ApplyNamingConventions = false)]
        public float borderSizeY = float.NaN;
        public bool invertx = true;
        public bool inverty = true;
    }

    public Exception Validate(string cabinetPath)
    {
        string errors = "";
        if (!String.IsNullOrEmpty(gun.model))
        {
            string path = Path.Combine(cabinetPath, gun.model);
            if (!File.Exists(path))
            {
                errors += $"file {gun.model} doesn't exists. ";
            }
        }

        if (!String.IsNullOrEmpty(errors))
            return new Exception($"{errors}]");
        return null;
    }

    public override string ToString()
    {
        return $"active: {active} model: {gun.model} invert x,y: {crt.invertx},{crt.inverty} debug: {debug.active}";
    }

    public LightGunInformation() { }

}

// https://github.com/libretro/mame2003-plus-libretro/blob/89298ff12328433c7cdc63d38c65439079afcb5d/src/mame2003/mame2003.c#L1352
// https://github.com/libretro/mame2003-plus-libretro/blob/master/src/mame2003/core_options.c#L66
// This component could be attached to a cabinet and be filled with parts that can be shooted.
// Or in a CRT (was designed originaly to be attached to a CRT). In this case it could have parts to shoot too.
public class LightGunTarget : MonoBehaviour
{

    [Tooltip("Information from cabinet description. Transfered to others properties at Start")]
    public LightGunInformation lightGunInformation;

    [Tooltip("the gun gameobject model")]
    public GameObject spaceGun; // The Space Gun GameObject
    [Tooltip("Invert to gun model forward.")]
    public bool invertForward = false;

    // Layer mask to filter the raycast hits
    private LayerMask layerMaskCRT;
    private LayerMask layerMaskParts;


    [Tooltip("show the debug ball.")]
    public bool showHitPosition = false;
    [Tooltip("Invert to negative/positive the x point.")]
    public bool InvertX = true;
    [Tooltip("Invert to negative the y point.")]
    public bool InvertY = true;

    //[Tooltip("Screen mesh scale factor to adjust in width.")]
    //public float scaleFactorX = 0.01f;
    //[Tooltip("Screen mesh scale factor to adjust in height.")]
    //public float scaleFactorY = 0.01f;

    //[Tooltip("CRT border size left and right to exclude.")]
    //public float borderSizeX = 1.5f;
    //[Tooltip("CRT border size up and down to exclude.")]
    //public float borderSizeY = 1f;
    public float adjustSightVertical = 0, adjustSightHorizontal = 0;

    public List<GameObject> parts = new List<GameObject>(); //parts to hit.
    private GameObject player;
    private bool attachedToCRT; //old behavior

    //Cabinet path
    private string pathBase;

    //CRT
    GameObject lastGameObjectHit;
    /*
    It reports X/Y coordinates in screen space (similar to the pointer)
    in the range [-0x8000, 0x7fff] in both axes, with zero being center and
    -0x8000 being out of bounds.
    [-0x7fff, 0x7fff]: -0x7fff corresponds to the far left/top of the screen,
     and 0x7fff corresponds to the far right/bottom of the screen.
    */
    int lastHitX;
    int lastHitY; 
    const int virtualScreenWidth = 32767; //libretro constant width
    const int virtualScreenHeight = 32767; //libretro constant height

    Renderer CRTRenderer;
    Texture2D texture;
    RaycastHit hit;

    // The MeshFilter from the hit GameObject
    Mesh mesh;

    // The triangle counts for each submesh
    int[] trianglesSubmesh0; // Submesh 0
    int[] trianglesSubmesh1; // Submesh 1

    public void Start()
    {
        attachedToCRT = gameObject.layer == LayerMask.NameToLayer("CRT");
        if (attachedToCRT)
        {
            CRTRenderer = gameObject.GetComponent<Renderer>();
        }
    }

    //to start the component only if light-gun is active for the game.
    public void Init(LightGunInformation lightGunInfo, string pathBase, GameObject player)
    {
        if (lightGunInfo == null || !lightGunInfo.active)
            return;
        
        ConfigManager.WriteConsole($"[LightGunTarget.init] lightGunInfo:{lightGunInfo.ToString()} pathbase: {pathBase}");

        lightGunInformation = lightGunInfo;
        this.pathBase = pathBase;
        this.player = player;

        layerMaskCRT = LayerMask.GetMask("CRT");
        layerMaskParts = LayerMask.GetMask("InteractablePart");
        texture = null;

        if (lightGunInformation != null)
        {
            /*
             * deprecated.
            if (!float.IsNaN(lightGunInformation.crt.borderSizeX))
                borderSizeX = lightGunInformation.crt.borderSizeX;
            if (!float.IsNaN(lightGunInformation.crt.borderSizeY))
                borderSizeY = lightGunInformation.crt.borderSizeY;
            if (!float.IsNaN(lightGunInformation.crt.meshFactorScaleX))
                scaleFactorX = lightGunInformation.crt.meshFactorScaleX;
            if (!float.IsNaN(lightGunInformation.crt.meshFactorScaleY))
                scaleFactorY = lightGunInformation.crt.meshFactorScaleY;
            */

            InvertX = lightGunInformation.crt.invertx;
            InvertY = lightGunInformation.crt.inverty;
            
            adjustSightVertical = lightGunInformation.gun.adjustSight.vertical;
            adjustSightHorizontal = lightGunInformation.gun.adjustSight.horizontal;

            showHitPosition = lightGunInformation.debug.active;
            invertForward = lightGunInformation.gun.invertPointer;
        }

#if ON_DEBUG
        showHitPosition = true;
#endif

        //connect to the event to know when the lightgun is ready
        ChangeControls chctrl = player.GetComponent<ChangeControls>();
        if (chctrl != null)
            chctrl.OnChangeRightJoystick.AddListener(OnLightGunActive);
    }


    //assign a part to shoot and change the part's layer
    public void addPart(GameObject part)
    {
        if (part != null)
        {
            this.parts.Add(part);
        }
    }

    public void OnLightGunActive(GameObject spaceGun)
    {
        this.spaceGun = spaceGun;
    }

    public bool Initialized()
    {
        return lightGunInformation != null;
    }



    private void declareOutOfScreen()
    {
        lastHitX = -0x8000;
        lastHitY = -0x8000;
        return;
    }

    public void GetLastHit(out int hitx, out int hity)
    {
        hitx = lastHitX; hity = lastHitY;
    }
    public GameObject GetLastGameObjectHit()
    {
        return lastGameObjectHit;
    }

    public string GetModelPath()
    {
        if (lightGunInformation?.gun == null)
            return null;
        return pathBase + "/" + lightGunInformation.gun.model;
    }

    public bool PointingToTheScreen()
    {
        return lastHitX != -0x8000 && lastHitY != -0x8000;
    }

    public void Update()
    {
        if (lightGunInformation == null || !lightGunInformation.active ||
            spaceGun == null || spaceGun.transform == null ||
            !spaceGun.activeSelf)
            return;

        if (this.parts.Count > 0)
        {
            lastGameObjectHit = null;
            if (isPoinitingToATarget(layerMaskParts))
            {
                foreach (GameObject part in this.parts)
                {
                    if (hit.collider.gameObject == part)
                    {
                        lastGameObjectHit = part;
                        break;
                    }
                }
            }
        }

        if (attachedToCRT)
        {
            if (texture == null)
            {
                Material secondMaterial = CRTRenderer.materials[1];
                texture = secondMaterial.mainTexture as Texture2D;

                // Get the MeshFilter from the hit GameObject
                MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
                mesh = meshFilter.mesh;

                // Get the triangle counts for each submesh
                trianglesSubmesh0 = mesh.GetTriangles(0); // Submesh 0
                trianglesSubmesh1 = mesh.GetTriangles(1); // Submesh 1 - where the texture lives
            }

            // if this component is attached to a CRT
            if (isPoinitingToATarget(layerMaskCRT) && hit.collider?.gameObject == gameObject)
                detectCRTCoordinates();
        }
    }

    private bool isPoinitingToATarget(LayerMask layer)
    {
        Vector3 adjustedPosition;
        Vector3 adjustedForward;
        //adjust vertical/horizontal sight (in centimeters)
        CalculateTransform(out adjustedPosition, out adjustedForward);
        // Cast a ray from the spaceGun position to its forward direction
        return Physics.Raycast(adjustedPosition, adjustedForward, out hit, Mathf.Infinity, layer);
    }

    private void CalculateTransform(out Vector3 pos, out Vector3 forward)
    {
        pos = Vector3.zero;
        pos.x = spaceGun.transform.position.x + adjustSightHorizontal / 100f;
        pos.y = spaceGun.transform.position.y + adjustSightVertical / 100f;
        pos.z = spaceGun.transform.position.z;
        forward = spaceGun.transform.forward;
        if (invertForward)
            forward = -forward;
    }

    private void detectCRTCoordinates()
    {
        if (hit.collider.gameObject == gameObject)
        {
                     
            int hitTriangleIndex = hit.triangleIndex;

            // Determine if the hit triangle is within Submesh 1
            int triangleIndexInSubmesh1 = -1; // Default invalid value

            // Check if the hit triangle index falls within the range of Submesh 1 triangles
            if (hitTriangleIndex >= trianglesSubmesh0.Length / 3 &&
                hitTriangleIndex < (trianglesSubmesh0.Length + trianglesSubmesh1.Length) / 3)
            {
                // Adjust the triangle index to the local index within Submesh 1
                triangleIndexInSubmesh1 = hitTriangleIndex - (trianglesSubmesh0.Length / 3);

                // Get the vertex indices of the hit triangle in Submesh 1
                int vertIndex1 = trianglesSubmesh1[triangleIndexInSubmesh1 * 3];
                int vertIndex2 = trianglesSubmesh1[triangleIndexInSubmesh1 * 3 + 1];
                int vertIndex3 = trianglesSubmesh1[triangleIndexInSubmesh1 * 3 + 2];

                // Get the barycentric coordinates of the hit point to interpolate UVs
                Vector3 barycentricCoord = hit.barycentricCoordinate;

                // Use the UVs (texture coordinates) for the vertices of the hit triangle
                // Interpolate the UV coordinates using the barycentric coordinates
                Vector2 interpolatedUV = mesh.uv[vertIndex1] * barycentricCoord.x + 
                                            mesh.uv[vertIndex2] * barycentricCoord.y +
                                            mesh.uv[vertIndex3] * barycentricCoord.z;

                // Now we have the correct UV coordinates for Submesh 1
                // Use interpolatedUV to work with the texture of Submesh 1
                // Perform any necessary operations using the UV and texture...

                // Convert UV to virtual screen space. Normalize texture coordinates from [0, 1] to [-1, 1]
                float normalizedX = 2 * interpolatedUV.x - 1;
                float normalizedY = 2 * interpolatedUV.y - 1;

                // Invert the coordinates if needed
                if (!InvertX) //back compat
                    normalizedX = -normalizedX;
                if (InvertY)
                    normalizedY = -normalizedY;

                // Map the normalized coordinates to the virtual screen space
                lastHitX = Mathf.Clamp(Mathf.RoundToInt(normalizedX * virtualScreenWidth), -virtualScreenWidth, virtualScreenWidth);
                lastHitY = Mathf.Clamp(Mathf.RoundToInt(normalizedY * virtualScreenHeight), -virtualScreenHeight, virtualScreenHeight);

                // Debug visualization of hit position on the texture
                if (showHitPosition)
                {
                    int x, y;
                    // Calculate the x, y coordinates in Unity texture space
                    x = Mathf.RoundToInt(interpolatedUV.x * texture.width);
                    y = Mathf.RoundToInt(interpolatedUV.y * texture.height);

                    // Invert the x and y coordinates if needed
                    if (!InvertX)
                        x = texture.width - 1 - x;
                    if (InvertY)
                        y = texture.height - 1 - y;

                    // Clamp the x, y coordinates to ensure they're within the texture bounds
                    x = Mathf.Clamp(x, 0, texture.width - 1);
                    y = Mathf.Clamp(y, 0, texture.height - 1);

                    // Draw a red circle at the hit point in the texture for debugging purposes
                    DrawRedCircle(texture, x, y, 5);
                    texture.Apply();
                }
            }
#if ON_DEBUG
            ConfigManager.WriteConsole($"[LightGunTarget]  x,y:{lastHitX}, {lastHitY}");
#endif
            return;
        }

        // If the ray doesn't hit anything, reset the hit coordinates to out-of-bounds
        declareOutOfScreen();

        return;
    }

    // Function to draw a red circle
    void DrawRedCircle(Texture2D texture, int centerX, int centerY, int radius)
    {
        Color red = Color.red;

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= radius * radius) // Circle equation
                {
                    int px = centerX + x;
                    int py = centerY + y;

                    if (px >= 0 && px < texture.width && py >= 0 && py < texture.height)
                    {
                        texture.SetPixel(px, py, red);
                    }
                }
            }
        }
    }
#if ON_DEBUG
    void OnDrawGizmos()
    {
        Vector3 adjustedPosition;
        Vector3 adjustedForward;
        if (spaceGun == null)
            return;
        //adjust vertical/horizontal sight (in centimeters)
        CalculateTransform(out adjustedPosition, out adjustedForward);

        // Draw the ray as a debug ray from the spaceGun's position along its forward direction
        Debug.DrawRay(adjustedPosition, adjustedForward * 100f, Color.red); // Change 100f to adjust the ray length
    }
#endif
}
