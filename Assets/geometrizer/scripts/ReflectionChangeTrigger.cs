using UnityEngine;
using UnityEngine.Rendering;

public class ReflectionChangeTrigger : MonoBehaviour
{
    public Cubemap newReflectionCubemap; // Assign this in the Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Update reflection if needed
            if (RenderSettings.customReflection != newReflectionCubemap)
            {
                RenderSettings.customReflection = newReflectionCubemap;
                DynamicGI.UpdateEnvironment();
            }

            // Find PF_UserLight in the scene
            GameObject userLightGO = GameObject.Find("PF_UserLight");

            if (userLightGO != null)
            {
                Light lightComponent = userLightGO.GetComponent<Light>();
                if (lightComponent != null)
                {
                    Color userLightColor = lightComponent.color;
                    float userLightIntensity = lightComponent.intensity;
                    Quaternion userLightRotation = lightComponent.transform.rotation;

                    // Log values
                    UnityEngine.Debug.Log($"User Light - Color: {userLightColor}, Intensity: {userLightIntensity}, Rotation: {userLightRotation.eulerAngles}");
                }
                else
                {
                    UnityEngine.Debug.LogWarning("PF_UserLight found but it has no Light component.");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("PF_UserLight GameObject not found in the scene.");
            }
        }
    }
}
