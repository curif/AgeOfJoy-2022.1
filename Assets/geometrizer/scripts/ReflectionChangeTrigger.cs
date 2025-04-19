using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class ReflectionChangeTrigger : MonoBehaviour
{
    public Cubemap newReflectionCubemap; // Assign this in the Inspector

    public float lightTransitionDuration = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Scene triggerScene = gameObject.scene;

        // Log the scene asset path and name
        UnityEngine.Debug.Log($"[ReflectionChangeTrigger] Triggered in scene: '{triggerScene.name}', path: '{triggerScene.path}'");

        // Update reflection if needed
        if (RenderSettings.customReflection != newReflectionCubemap)
        {
            RenderSettings.customReflection = newReflectionCubemap;
            DynamicGI.UpdateEnvironment();
        }

        // Search for PF_UserLight recursively in this scene
        GameObject pfUserLight = null;
        foreach (GameObject rootObj in triggerScene.GetRootGameObjects())
        {
            pfUserLight = FindChildRecursiveByName(rootObj.transform, "PF_UserLight");
            if (pfUserLight != null) break;
        }

        // Default values if not found
        Color finalColor = Color.black;
        float finalIntensity = 0f;
        Vector3 finalEulerRotation = Vector3.zero;

        if (pfUserLight != null)
        {
            Light lightComponent = pfUserLight.GetComponent<Light>();
            if (lightComponent != null)
            {
                finalColor = lightComponent.color;
                finalIntensity = lightComponent.intensity;
            }

            finalEulerRotation = pfUserLight.transform.eulerAngles;

            UnityEngine.Debug.Log($"[ReflectionChangeTrigger] Found PF_UserLight in scene '{triggerScene.name}'. " +
                                  $"Color: {finalColor}, Intensity: {finalIntensity}, Rotation: {finalEulerRotation}");
        }
        else
        {
            UnityEngine.Debug.Log($"[ReflectionChangeTrigger] No PF_UserLight found in scene '{triggerScene.name}'. " +
                                  $"Defaulting to Color: black, Intensity: 0, Rotation: (0, 0, 0)");
        }

        // Apply to UserLightManager
        GameObject userLightGO = GameObject.Find("UserLightManager");
        if (userLightGO != null)
        {
            UserLightManager userLightManager = userLightGO.GetComponent<UserLightManager>();
            if (userLightManager != null)
            {
                UserLightSettings newSettings = new UserLightSettings
                {
                    color = finalColor,
                    intensity = finalIntensity,
                    eulerRotation = finalEulerRotation
                };

                UnityEngine.Debug.Log($"[ReflectionChangeTrigger] Applying to UserLightManager: " +
                                      $"Color = {finalColor}, Intensity = {finalIntensity}, Rotation = {finalEulerRotation}");
                userLightManager.ApplyUserLightSettings(newSettings, lightTransitionDuration);
            }
            else
            {
                UnityEngine.Debug.LogWarning("[ReflectionChangeTrigger] UserLightManager GameObject found but has no UserLightManager component.");
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("[ReflectionChangeTrigger] UserLightManager GameObject not found in the scene.");
        }
    }

    // Recursively search for child named PF_UserLight
    private GameObject FindChildRecursiveByName(Transform parent, string targetName)
    {
        if (parent.name == targetName)
            return parent.gameObject;

        foreach (Transform child in parent)
        {
            GameObject result = FindChildRecursiveByName(child, targetName);
            if (result != null)
                return result;
        }

        return null;
    }
}
