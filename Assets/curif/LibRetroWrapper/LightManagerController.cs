using UnityEngine;
using System;

public class LightManagerController : MonoBehaviour
{
    // Public properties
    public RoomConfiguration roomConfiguration;
    public float intensity = 1;

    private string lightName;

    // New property to get the light name
    public string LightName
    {
        get { return lightName; }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (roomConfiguration == null)
            return;
        
        lightName = roomConfiguration.Room.ToUpper() + ":" + gameObject.name.ToUpper();
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

    
    // set the light color
    public void SetColor(Color newColor)
    {
        // Get the Light component attached to the same GameObject
        Light lightComponent = GetComponent<Light>();

        // Check if a Light component is attached
        if (lightComponent != null)
        {
            // Set the color of the light
            lightComponent.color = newColor;
        }
        else
        {
            ConfigManager.WriteConsoleError("[LightManagerController.SetColor] Light component not found on the GameObject.");
        }
    }
}
