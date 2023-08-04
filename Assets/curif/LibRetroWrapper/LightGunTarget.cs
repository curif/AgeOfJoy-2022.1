#define ON_DEBUG
using UnityEngine;
using System;


// https://github.com/libretro/mame2003-plus-libretro/blob/89298ff12328433c7cdc63d38c65439079afcb5d/src/mame2003/mame2003.c#L1352
// https://github.com/libretro/mame2003-plus-libretro/blob/master/src/mame2003/core_options.c#L66
public class LightGunTarget : MonoBehaviour
{
    [Tooltip("the gun gameobject model")]
    public GameObject spaceGun; // The Space Gun GameObject
    [Tooltip("Invert to gun model forward.")]
    public bool invertForward = false;

    // Public properties to access the hit coordinates from other scripts if needed
    [Tooltip("libretro shoot position X.(calculated)")]
    public int HitX;
    [Tooltip("libretro shoot position Y (calculated).")]
    public int HitY;

    // Layer mask to filter the raycast hits
    private LayerMask layerMask;

    private GameObject hitPosition;

    [Tooltip("show the debug ball.")]
    public bool showHitPossition = false;
    [Tooltip("Invert to negative the x point.")]
    public bool InvertX = true;
    [Tooltip("Invert to negative the y point.")]
    public bool InvertY = false;

    [Tooltip("Screen mesh scale factor to adjust in width.")]
    public float scaleFactorX = 0.01f;
    [Tooltip("Screen mesh scale factor to adjust in height.")]
    public float scaleFactorY = 0.01f;

    [Tooltip("CRT border size left and right to exclude.")]
    public float borderSizeX = 1.5f;
    [Tooltip("CRT border size up and down to exclude.")]
    public float borderSizeY = 1f;

    float CRTAreaWidth; //new width after substract borders
    float CRTAreaHeight; //new height after substract borders
    float factorX; //adjustment factor for hit point to translate to libretro width space
    float factorY; //adjustment factor for hit point to translate to libretro height space
    const int virtualScreenWidth = 32768; //libretro constant width
    const int virtualScreenHeight = 32768; //libretro constant height

    void Start()
    {
        layerMask = LayerMask.GetMask("CRT");

        hitPosition = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hitPosition.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f); // Adjust the size of the sphere as needed
        hitPosition.GetComponent<Renderer>().material.color = Color.red;
        hitPosition.SetActive(false);
        hitPosition.transform.SetParent(transform);

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
        showHitPossition = true;
        Shoot();
        if (OnScreen())
            ConfigManager.WriteConsole($"[LightGunTarget] {spaceGun.name}: Hitx:{HitX} Hitx:{HitY}");
    }
#endif

    private void declareOutOfScreen()
    {
        HitX = -0x8000;
        HitY = -0x8000;
        hitPosition.SetActive(false);
        return;
    }

    public bool OnScreen()
    {
        return HitX != -0x8000;
    }
    public void Shoot()
    {
        if (spaceGun?.transform == null)
            return;

        // Cast a ray from the spaceGun position to its forward direction
        if (Physics.Raycast(spaceGun.transform.position,
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

                HitX = Mathf.RoundToInt(Mathf.Clamp(hitPointX * virtualScreenWidth / scaleFactorX,
                                    -virtualScreenWidth,
                                    virtualScreenWidth));
                HitY = Mathf.RoundToInt(Mathf.Clamp(hitPointY * virtualScreenHeight / scaleFactorY,
                                    -virtualScreenHeight,
                                    virtualScreenHeight));
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

                    if (showHitPossition)
                    {
                        // hitPosition.transform.position = hit.point;
                        hitPosition.transform.localPosition = localHitPoint;
                        hitPosition.SetActive(true);
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

    void OnDisable()
    {
        DestroyImmediate(hitPosition);
    }

}
