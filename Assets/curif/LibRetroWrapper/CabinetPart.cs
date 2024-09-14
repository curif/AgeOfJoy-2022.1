using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;
using static CabinetInformation;

[System.Serializable]
public class TransformData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}

public class CabinetPart : MonoBehaviour
{
    [Header("World Space Transform")]
    public TransformData initialWorldTransform;

    [Header("Local Space Transform")]
    public TransformData initialLocalTransform;

    public GameObject GameObject { get { return gameObject; } }
    public Transform Transform { get { return transform; } }

    Renderer rendererComponent;

    private void Awake()
    {
        rendererComponent = GetComponent<Renderer>();
    }

    void Start()
    {
        // Store the initial world space transform
        initialWorldTransform = new TransformData
        {
            position = transform.position,
            rotation = transform.rotation,
            scale = transform.localScale
        };

        // Store the initial local space transform
        initialLocalTransform = new TransformData
        {
            position = transform.localPosition,
            rotation = transform.localRotation,
            scale = transform.localScale
        };
    }

    public CabinetPart Scale(float crtScalePercentage, float crtXratio, float crtYratio, float crtZratio)
    {
        float scale = crtScalePercentage / 100f;
        transform.localScale =
            new Vector3(transform.localScale.x * crtXratio * scale,
                        transform.localScale.y * crtYratio * scale,
                        transform.localScale.z * crtZratio * scale);
        return this;
    }

    public CabinetPart Rotate(Vector3 rotation)
    {
        transform.Rotate((Vector3)rotation);
        return this;
    }

    public CabinetPart Rotate(float angleX, float angleY, float angleZ)
    {
        transform.Rotate(angleX, angleY, angleZ);
        return this;
    }

    public CabinetPart ApplyUserConfigurationGeometry(Geometry g)
    {
        if (g != null)
        {
            Scale(g.scalepercentage, g.ratio.x, g.ratio.y, g.ratio.z);
            Rotate(g.rotation.x, g.rotation.y, g.rotation.z);
        }
        return this;
    }


    public CabinetPart Enable(bool enabled)
    {
        gameObject.SetActive(enabled);
        return this;
    }

    public CabinetPart RotateLocalEulerAngleByAxisFromOrigin(string axis, float angle)
    {

        // Validate rotation value to be within 0 to 360 degrees
        if (angle < -360 || angle > 360)
            throw new Exception($"Rotation: {name} value must be between -360 and 360 degrees. value {angle}");

        Quaternion newRotation;
        switch (axis)
        {
            case "X":
                newRotation = Quaternion.Euler(angle, 0, 0);
                break;
            case "Y":
                newRotation = Quaternion.Euler(0, angle, 0);
                break;
            case "Z":
                newRotation = Quaternion.Euler(0, 0, angle);
                break;
            default:
                throw new Exception($"Rotation: {name} axis should be X, Y, or Z: {axis}");
        }
        transform.localRotation = initialLocalTransform.rotation * newRotation;
        
        return this;
    }

    public CabinetPart RotateLocalEulerAngleByAxis(string axis, float angle)
    {

        // Validate rotation value to be within 0 to 360 degrees
        if (angle < -360 || angle > 360)
            throw new Exception($"Rotation: {name} value must be between -360 and 360 degrees. value {angle}");

        Quaternion newRotation;
        switch (axis)
        {
            case "X":
                newRotation = Quaternion.Euler(angle, 0, 0);
                break;
            case "Y":
                newRotation = Quaternion.Euler(0, angle, 0);
                break;
            case "Z":
                newRotation = Quaternion.Euler(0, 0, angle);
                break;
            default:
                throw new Exception("cabPartsSetGlobalRotation: axis should be X, Y, or Z");
        }
        transform.localRotation *= newRotation;

        return this;
    }

    public float GetLocalRotationByAxis(string axis)
    {
        Quaternion deltaRotation = transform.localRotation * Quaternion.Inverse(initialLocalTransform.rotation);

        // Convert the delta rotation quaternion to Euler angles
        Vector3 deltaEuler = deltaRotation.eulerAngles;

        switch (axis)
        {
            case "X":
                return deltaEuler.x;
            case "Y":
                return deltaEuler.y;
            case "Z":
                return deltaEuler.z;
            default:
                throw new Exception($"Rotation: {name} axis should be X, Y, or Z: {axis}");
        }
    }

    public CabinetPart RotateWorldEulerAngleByAxis(string axis, float angle)
    {

        // Validate rotation value to be within 0 to 360 degrees
        if (angle < -360 || angle > 360)
            throw new Exception($"Rotation: {name} value must be between -360 and 360 degrees. value {angle}");

        Quaternion newRotation;
        switch (axis)
        {
            case "X":
                newRotation = Quaternion.Euler(angle, 0, 0);
                break;
            case "Y":
                newRotation = Quaternion.Euler(0, angle, 0);
                break;
            case "Z":
                newRotation = Quaternion.Euler(0, 0, angle);
                break;
            default:
                throw new Exception($"Rotation: {name} axis should be X, Y, or Z: {axis}");
        }
        transform.rotation = initialWorldTransform.rotation * newRotation;

        return this;
    }

    public float GetWorldRotationByAxis(string axis)
    {
        Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(initialWorldTransform.rotation);

        // Convert the delta rotation quaternion to Euler angles
        Vector3 deltaEuler = deltaRotation.eulerAngles;

        switch (axis)
        {
            case "X":
                return deltaEuler.x;
            case "Y":
                return deltaEuler.y;
            case "Z":
                return deltaEuler.z;
            default:
                throw new Exception($"Rotation: {name} axis should be X, Y, or Z: {axis}");
        }
    }
    public float GetCoordinate(string axis)
    {
        float coordinateValue = 0f;
        switch (axis)
        {
            case "X":
                return transform.localPosition.x;
            case "Y":
                return transform.localPosition.y;
            case "Z":
                return transform.localPosition.z;
            default:
                throw new Exception($"get coordinate  {name}  coordinate should be X, Y or Z:  {axis}");
        }
    }

// ==================== MATERIAL ===================

//set the material to a component. Don't create new.
public CabinetPart SetMaterial(Material mat)
    {
        if (rendererComponent != null)
            rendererComponent.material = mat;

        return this;
    }
    public CabinetPart SetMaterialFrom(Material mat)
    {
        return SetMaterialFromMaterial(mat, false);
    }
    public CabinetPart SetMaterialFromMaterial(Material mat, bool OnlyAssignIfDoesntHaveOne = false)
    {
        if (rendererComponent == null)
            return this;

        if (rendererComponent.material != null && OnlyAssignIfDoesntHaveOne)
            return this;

        Material m = new Material(mat);
        m.name = $"{name}_from_{mat.name}";
        rendererComponent.material = mat;
        return this ;
    }

    //assign the Base material if doesn't have any.
    public CabinetPart PartNeedsAMaterial()
    {
        return SetMaterialFromMaterial(CabinetMaterials.Base, OnlyAssignIfDoesntHaveOne: true);
    }

    //assign the Base material
    public CabinetPart PartNeedsAMaterialBase()
    {
        return SetMaterialFromMaterial(CabinetMaterials.Base, OnlyAssignIfDoesntHaveOne: false);
    }

    private CabinetPart PaintVertexColorMaterial(Color color)
    {

        // Retrieve all MeshFilter components in the GameObject and its children
        MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
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
        return this;
    }

    // @Gometrizer technique to colorize without a material.
    public CabinetPart SetColorVertex(Color color)
    {
        SetMaterialFromMaterial(CabinetMaterials.VertexColor);
        PaintVertexColorMaterial(color);
        return this;
    }

    public Material GetMaterial()
    {
        if (rendererComponent == null) return null;
        return rendererComponent.material;
    }
    public CabinetPart SetColor(Color color)
    {
        Material mat = GetMaterial();
        if (mat == null)
            return this;

        if (mat.name.Contains("Base_VertexColor"))
            PaintVertexColorMaterial(color);
        else
            mat.color = color;
        return this;
    }

    public CabinetPart SetTransparency(ref int transpPercent)
    {
        Material mat = GetMaterial();
        if (mat == null)
            return this;

        if (transpPercent < 0)
            transpPercent = 0;
        if (transpPercent > 100)
            transpPercent = 100;

        Color currentColor = mat.color;

        float alpha = (float)transpPercent / 100f;

        currentColor.a = alpha;
        mat.color = currentColor;
        return this;
    }

    public int GetTransparency()
    {
        Material mat = GetMaterial();
        if (mat == null)
            return 0; // Default to 0% transparency if no Material found

        return (int)(mat.color.a * 100f);
    }

    public CabinetPart SetEmissionEnabled(bool enabled)
    {
        Material mat = GetMaterial();

        // Set emission enabled
        mat.EnableKeyword("_EMISSION"); // Enable emission if true
        if (enabled)
        {
            Texture mainTexture = rendererComponent.sharedMaterial.mainTexture;
            if (mainTexture != null)
                // Set the emission map to be the same as the main texture
                rendererComponent.sharedMaterial.SetTexture("_EmissionMap", mainTexture);
        }
        else
            mat.DisableKeyword("_EMISSION"); // Disable emission if false

        rendererComponent.material = mat;
        return this;
    }

    public CabinetPart SetMarqueeEmissionColor(RGBColor emissionColor, RGBColor backLightColor)
    {
        Material mat = GetMaterial();
        if (mat != null)
        {
            if (emissionColor != null)
            {
                //lamp color and intensity
                mat.SetColor("_EmissionColor", emissionColor.getColorNoIntensity());
                float oldToNewIntensity = (emissionColor.intensity + 5) / 10;
                mat.SetFloat("_EmissionIntensity", oldToNewIntensity);
            }
            if (backLightColor != null)
                mat.SetColor("_BacklightColor", backLightColor.getColorNoIntensity());
        }

        return this;
    }

    public CabinetPart SetMarquee(string texturePath, Material marqueeMaterial, bool invertX = false, bool invertY = false)
    {
        return SetTextureFromFile(texturePath, marqueeMaterial, invertX: invertX, invertY: invertY);
    }

    public CabinetPart SetBezel(string texturePath, bool invertX = false, bool invertY = false)
    {
        return SetTextureFromFile(texturePath, CabinetMaterials.FrontGlassWithBezel, invertX: invertX, invertY: invertY);
    }


    // load a texture from disk.
    private static Texture2D LoadTexture(string filePath)
    {
        return CabinetTextureCache.LoadAndCacheTexture(filePath);
    }


    public CabinetPart SetTextureFromFile(string textureFile, Material mat, bool invertX, bool invertY)
    {
        if (rendererComponent == null)
            return this;

        Material m;
        if (mat == null)
        {
            m = GetMaterial();
            if (m == null)
                return this;
        }
        else
        {
            m = new Material(mat);
            m.name = $"{gameObject.name}_from_{mat.name}";
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
            ConfigManager.WriteConsoleWarning($"Cabinet {gameObject.name} texture error {textureFile}");
            return this;
        }
        else
            m.SetTexture("_MainTex", t);

        rendererComponent.material = m;
        return this;
    }

    public CabinetPart SetEmissionTextureFromFile(string textureFile, bool invertX = false, bool invertY = false)
    {
        Material m = GetMaterial();
        if (m == null)
            return this;

        // Tiling
        Vector2 emissionTextureScale = new Vector2(1, 1);
        if (invertX)
            emissionTextureScale.x = -1;
        if (invertY)
            emissionTextureScale.y = -1;
        m.SetTextureScale("_EmissionMap", emissionTextureScale);

        // Emission texture
        Texture2D t = LoadTexture(textureFile);
        if (t == null)
        {
            ConfigManager.WriteConsoleError($"Error loading emission texture for {gameObject.name}: {textureFile}");
            return this;
        }
        else
        {
            m.SetTexture("_EmissionMap", t);
        }

        rendererComponent.material = m;
        return this;
    }

    public CabinetPart SetEmissionColor(Color emissionColor)
    {
        Material m = GetMaterial();
        if (m == null) return this;

        // Set emission color
        m.SetColor("_EmissionColor", emissionColor);
        Texture mainTexture = rendererComponent.sharedMaterial.mainTexture;
        if (mainTexture != null)
            // Set the emission map to be the same as the main texture
            rendererComponent.sharedMaterial.SetTexture("_EmissionMap", mainTexture);

        rendererComponent.material = m;
        return this;
    }
    //change a material, or create a new one and change it.
    public CabinetPart SetTextureTo(string partName, string textureFile, Material mat, bool invertX = false, bool invertY = false)
    {
        if (!string.IsNullOrEmpty(textureFile))
            SetTextureFromFile(textureFile, mat, invertX, invertY);
        return this;
    }
    public CabinetPart SetTextureTo(string textureFile, Material mat, bool invertX = false, bool invertY = false)
    {
        if (!string.IsNullOrEmpty(textureFile))
            SetTextureFromFile(textureFile, mat, invertX, invertY);
        return this;
    }
    public CabinetPart SetEmissionTextureTo(string textureFile, bool invertX = false, bool invertY = false)
    {
        if (!string.IsNullOrEmpty(textureFile))
            return SetEmissionTextureFromFile(textureFile, invertX, invertY);
        return this;
    }

}
