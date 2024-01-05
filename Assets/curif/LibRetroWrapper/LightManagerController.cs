using UnityEngine;

public class LightManagerController : MonoBehaviour
{
    // Public properties
    public RoomConfiguration roomConfiguration;
    public float intensity;

    // New property to get the light name
    public string LightName
    {
        get { return roomConfiguration.Room + ":" + gameObject.name; }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Set the initial light intensity
        SetIntensity(intensity);
    }

    // Method to set the light intensity
    public void SetIntensity(float newIntensity)
    {
        // Get the Light component attached to the same GameObject
        Light lightComponent = GetComponent<Light>();

        // Check if a Light component is attached
        if (lightComponent != null)
        {
            // Set the intensity of the light
            lightComponent.intensity = newIntensity;
        }
        else
        {
            ConfigManager.WriteConsoleError("[LightManagerController.SetIntensity] Light component not found on the GameObject.");
        }
    }

    
    // Method to get the current light intensity
    public float GetIntensity()
    {
        // Get the Light component attached to the same GameObject
        Light lightComponent = GetComponent<Light>();

        // Check if a Light component is attached
        if (lightComponent != null)
        {
            // Return the current intensity of the light
            return lightComponent.intensity;
        }
        else
        {
            ConfigManager.WriteConsoleError("[LightManagerController.GetIntensity] Light component not found on the GameObject. Returning 0 intensity.");
            return 0f;
        }
    }
}
