using System.Collections;
using UnityEngine;

public class UserLightManager : MonoBehaviour
{
    private Coroutine lightTransitionCoroutine;
    public Light targetLight;

    void Awake()
    {
        if (targetLight == null)
        {
            targetLight = GetComponent<Light>();
            if (targetLight == null)
            {
                ConfigManager.WriteConsoleError("UserLightManager requires a Light component on the same GameObject.");
            }
        }
    }

    public void ApplyUserLightSettings(RGBColor color, float intensity, float transitionDuration = 1f)
    {
        ConfigManager.WriteConsole($"[UserLightManager] ApplyUserLightSettings called with values: " +
                              $"Color = {color}, Intensity: {intensity}, Duration = {transitionDuration}");

        if (lightTransitionCoroutine != null)
        {
            StopCoroutine(lightTransitionCoroutine);
        }
        lightTransitionCoroutine = StartCoroutine(TransitionLightSettings(color, intensity, transitionDuration));
    }

    private IEnumerator TransitionLightSettings(RGBColor newColor, float newIntensity, float duration)
    {
        if (targetLight == null) yield break;

        ConfigManager.WriteConsole("[UserLightManager] Starting light transition...");

        float time = 0f;

        float initialIntensity = targetLight.intensity;
        Color initialColor = targetLight.color;
        //Quaternion initialRotation = targetLight.transform.rotation;
        //Quaternion targetRotation = Quaternion.Euler(newSettings.eulerRotation);
        Color color;
        if (newColor == null)
            color = initialColor;
        else
            color = newColor.getColor();

        if (initialColor == color && initialIntensity == newIntensity)
            yield break;

        while (time < duration)
        {
            float t = time / duration;
            targetLight.intensity = Mathf.Lerp(initialIntensity, newIntensity, t);
            targetLight.color = Color.Lerp(initialColor, color, t);
            //targetLight.transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);

            time += Time.deltaTime;
            yield return null;
        }

        // Ensure final values are set
        targetLight.intensity = newIntensity;
        targetLight.color = color;
        //targetLight.transform.rotation = targetRotation;

        ConfigManager.WriteConsole("[UserLightManager] Light transition complete. Final values applied.");
    }
}
