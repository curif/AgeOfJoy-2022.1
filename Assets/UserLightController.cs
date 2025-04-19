using UnityEngine;

[RequireComponent(typeof(Light))]
public class UserLightController : MonoBehaviour
{
    [Tooltip("Set the color of the directional light")]
    public Color lightColor = Color.black;

    [Tooltip("Set the intensity of the directional light")]
    [Range(0, 8)] // Creates a slider in the Inspector
    public float intensity = 0f;

    private Light directionalLight;

    private void Start()
    {
        // Get the Light component when the game starts
        directionalLight = GetComponent<Light>();
        ApplySettings();
    }

    private void OnValidate()
    {
        // Automatically update light settings when values change in the Inspector
        ApplySettings();
    }

    private void ApplySettings()
    {
        // Ensure we have a reference to the Light component
        if (directionalLight == null)
            directionalLight = GetComponent<Light>();

        // Apply the color and intensity settings
        directionalLight.color = lightColor;
        directionalLight.intensity = intensity;
    }
}