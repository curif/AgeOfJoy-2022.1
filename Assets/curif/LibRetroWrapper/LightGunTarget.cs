//#define ON_DEBUG

using UnityEngine;
using System;
using YamlDotNet.Serialization; //https://github.com/aaubry/YamlDotNet
using YamlDotNet.Serialization.NamingConventions;

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
        public float meshFactorScaleX = 0.01f;
        [YamlMember(Alias = "mesh-factor-scale-y", ApplyNamingConventions = false)]
        public float meshFactorScaleY = 0.01f;

        [YamlMember(Alias = "border-size-x", ApplyNamingConventions = false)]
        public float borderSizeX = 1.5f;
        [YamlMember(Alias = "border-size-y", ApplyNamingConventions = false)]
        public float borderSizeY = 1f;
        public bool invertx = true;
        public bool inverty = true;
    }

    public override string ToString()
    {
        return $"active: {active} model: {gun.model} invert x,y: {crt.invertx},{crt.inverty} debug: {debug.active}";
    }

    public LightGunInformation() { }

}

// https://github.com/libretro/mame2003-plus-libretro/blob/89298ff12328433c7cdc63d38c65439079afcb5d/src/mame2003/mame2003.c#L1352
// https://github.com/libretro/mame2003-plus-libretro/blob/master/src/mame2003/core_options.c#L66
public class LightGunTarget : MonoBehaviour
{

    [Tooltip("Information from cabinet description. Transfered to others properties at Start")]
    public LightGunInformation lightGunInformation;

    [Tooltip("the gun gameobject model")]
    public GameObject spaceGun; // The Space Gun GameObject
    [Tooltip("Invert to gun model forward.")]
    public bool invertForward = false;

    // Public properties to access the hit coordinates from other scripts if needed
    [Tooltip("libretro shoot position X (calculated)")]
    public int HitX;
    [Tooltip("libretro shoot position Y (calculated).")]
    public int HitY;

    // Layer mask to filter the raycast hits
    private LayerMask layerMask;

    private GameObject hitPosition;

    [Tooltip("show the debug ball.")]
    public bool showHitPosition = false;
    [Tooltip("Invert to negative/positive the x point.")]
    public bool InvertX = true;
    [Tooltip("Invert to negative the y point.")]
    public bool InvertY = true;

    [Tooltip("Screen mesh scale factor to adjust in width.")]
    public float scaleFactorX = 0.01f;
    [Tooltip("Screen mesh scale factor to adjust in height.")]
    public float scaleFactorY = 0.01f;

    [Tooltip("CRT border size left and right to exclude.")]
    public float borderSizeX = 1.5f;
    [Tooltip("CRT border size up and down to exclude.")]
    public float borderSizeY = 1f;
    public float adjustSightVertical = 0, adjustSightHorizontal = 0;


    //Cabinet path
    private string pathBase;
    float CRTAreaWidth; //new width after substract borders
    float CRTAreaHeight; //new height after substract borders
    float factorX; //adjustment factor for hit point to translate to libretro width space
    float factorY; //adjustment factor for hit point to translate to libretro height space

    /*
    It reports X/Y coordinates in screen space (similar to the pointer)
    in the range [-0x8000, 0x7fff] in both axes, with zero being center and
    -0x8000 being out of bounds.
    [-0x7fff, 0x7fff]: -0x7fff corresponds to the far left/top of the screen,
     and 0x7fff corresponds to the far right/bottom of the screen.
    */
    const int virtualScreenWidth = 32767; //libretro constant width
    const int virtualScreenHeight = 32767; //libretro constant height

    //to start the component only if light-gun is active for the game.
    public void Init(LightGunInformation lightGunInfo, string pathBase)
    {
        if (lightGunInfo == null || !lightGunInfo.active)
            return;

        lightGunInformation = lightGunInfo;
        this.pathBase = pathBase;
        ConfigManager.WriteConsole($"[LightGunTarget.init] lightGunInfo:{lightGunInfo.ToString()} pathbase: {pathBase}");

        layerMask = LayerMask.GetMask("CRT");

        if (lightGunInformation != null)
        {
            borderSizeX = lightGunInformation.crt.borderSizeX;
            borderSizeY = lightGunInformation.crt.borderSizeY;
            scaleFactorX = lightGunInformation.crt.meshFactorScaleX;
            scaleFactorY = lightGunInformation.crt.meshFactorScaleY;
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
        if (showHitPosition)
        {
            if (hitPosition == null)
            {
                hitPosition = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                hitPosition.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f); // Adjust the size of the sphere as needed
                hitPosition.GetComponent<Renderer>().material.color = Color.red;
                hitPosition.transform.SetParent(transform);
            }
            hitPosition.SetActive(true);
        }

        // Calculate the new x width after shrinking at both ends
        CRTAreaWidth = transform.localScale.x - -borderSizeX * 2;
        CRTAreaHeight = transform.localScale.y - -borderSizeY * 2;
        //calculate the multiplication factor for the hit point after substract borders
        factorX = CRTAreaWidth / transform.localScale.x;
        factorY = CRTAreaHeight / transform.localScale.y;
    }

#if ON_DEBUG
    void Update()
    {
        Shoot();
        if (OnScreen())
            ConfigManager.WriteConsole($"[LightGunTarget] {spaceGun.name}: Hitx:{HitX} Hitx:{HitY}");
    }
#endif

    private void declareOutOfScreen()
    {
        HitX = -0x8000;
        HitY = -0x8000;
        if (showHitPosition)
            hitPosition.SetActive(false);
        return;
    }

    public string GetModelPath()
    {
        if (lightGunInformation?.gun == null)
            return null;
        return pathBase + "/" + lightGunInformation.gun.model;
    }

    public bool OnScreen()
    {
        return HitX != -0x8000;
    }
    public void Shoot()
    {
        if (spaceGun?.transform == null)
            return;

        //adjust vertical/horizontal sight (in centimeters)
        Vector3 adjustedPosition = new Vector3(
            spaceGun.transform.position.x + adjustSightHorizontal / 100f,
            spaceGun.transform.position.y + adjustSightVertical / 100f,
            spaceGun.transform.position.z
        );
        // Cast a ray from the spaceGun position to its forward direction
        if (Physics.Raycast(adjustedPosition,
                invertForward ? -spaceGun.transform.forward : spaceGun.transform.forward,
                out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            // Check if the ray hits the TV screen GameObject
            if (hit.collider.gameObject == gameObject)
            {
                // Get the hit point in local TV screen space
                Vector3 localHitPoint = transform.InverseTransformPoint(hit.point);

                // the X/Y coordinates in tv screen space in the range [-0x8000, 0x7fff] in both axes, 
                // with zero being center and -0x8000 being out of bounds 
                // (the ray fail to hit the screen). Remember: zero being center of the TV screen.

                // Calculate the new x,y point value after shrink the screen to avoid borders
                float hitPointX = localHitPoint.x * factorX;
                float hitPointY = localHitPoint.y * factorY;

                float translatedHitX = hitPointX * virtualScreenWidth / scaleFactorX;
                float translatedHitY = hitPointY * virtualScreenHeight / scaleFactorY;

                // translatedHitX += adjustSightHorizontal;
                // translatedHitY += adjustSightVertical;

                HitX = Mathf.RoundToInt(Mathf.Clamp(translatedHitX, -virtualScreenWidth, virtualScreenWidth));
                HitY = Mathf.RoundToInt(Mathf.Clamp(translatedHitY, -virtualScreenHeight, virtualScreenHeight));
                if (Math.Abs(HitX) == virtualScreenWidth || Math.Abs(HitY) == virtualScreenHeight)
                {
                    declareOutOfScreen();
                }
                else
                {
                    if (InvertX)
                        HitX = -HitX;
                    if (InvertY)
                        HitY = -HitY;

                    if (showHitPosition)
                    {
                        hitPosition.SetActive(true);
                        hitPosition.transform.localPosition = localHitPoint;
                    }
                }

#if ON_DEBUG
                // ConfigManager.WriteConsole($"[LightGunTarget] screen localscale w,h: {transform.localScale.x}, {transform.localScale.y} screen scale: {scaleX}, {scaleY} - x,y:{localHitPoint.x}, {localHitPoint.y} - HitX, HitY: {HitX}, {HitY} {hit.collider.gameObject.name}");
                ConfigManager.WriteConsole($"[LightGunTarget] screen localscale w,h: {transform.localScale.x}, {transform.localScale.y} - x,y:{localHitPoint.x}, {localHitPoint.y} - HitX, HitY: {HitX}, {HitY} {hit.collider.gameObject.name}");
#endif
                return;
            }
        }

        // If the ray doesn't hit anything, reset the hit coordinates to out-of-bounds
        declareOutOfScreen();

        return;
    }

#if ON_DEBUG
    void OnDrawGizmos()
    {
        // Draw the ray as a debug ray from the spaceGun's position along its forward direction
        Debug.DrawRay(spaceGun.transform.position, spaceGun.transform.forward * 100f, Color.red); // Change 100f to adjust the ray length
    }
#endif
}
