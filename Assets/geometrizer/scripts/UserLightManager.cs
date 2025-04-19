using System.Collections;
using UnityEngine;

[System.Serializable]
public class UserLightSettings
{
    public float intensity = 1f;
    public Color color = Color.white;
    public Vector3 eulerRotation = Vector3.zero;
}

public class UserLightManager : MonoBehaviour
{
    private Coroutine lightTransitionCoroutine;
    private Light targetLight;

    void Awake()
    {
        targetLight = GetComponent<Light>();
        if (targetLight == null)
        {
            UnityEngine.Debug.LogError("UserLightManager requires a Light component on the same GameObject.");
        }
    }

    public void ApplyUserLightSettings(UserLightSettings newSettings, float transitionDuration = 1f)
    {
        UnityEngine.Debug.Log($"[UserLightManager] ApplyUserLightSettings called with values: " +
                              $"Color = {newSettings.color}, Intensity = {newSettings.intensity}, Rotation = {newSettings.eulerRotation}, Duration = {transitionDuration}");

        if (lightTransitionCoroutine != null)
        {
            StopCoroutine(lightTransitionCoroutine);
        }
        lightTransitionCoroutine = StartCoroutine(TransitionLightSettings(newSettings, transitionDuration));
    }

    private IEnumerator TransitionLightSettings(UserLightSettings newSettings, float duration)
    {
        if (targetLight == null) yield break;

        UnityEngine.Debug.Log("[UserLightManager] Starting light transition...");

        float time = 0f;

        float initialIntensity = targetLight.intensity;
        Color initialColor = targetLight.color;
        Quaternion initialRotation = targetLight.transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(newSettings.eulerRotation);

        while (time < duration)
        {
            float t = time / duration;
            targetLight.intensity = Mathf.Lerp(initialIntensity, newSettings.intensity, t);
            targetLight.color = Color.Lerp(initialColor, newSettings.color, t);
            targetLight.transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);

            time += Time.deltaTime;
            yield return null;
        }

        // Ensure final values are set
        targetLight.intensity = newSettings.intensity;
        targetLight.color = newSettings.color;
        targetLight.transform.rotation = targetRotation;

        UnityEngine.Debug.Log("[UserLightManager] Light transition complete. Final values applied.");
    }
}
