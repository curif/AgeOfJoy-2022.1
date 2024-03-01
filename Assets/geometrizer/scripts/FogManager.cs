using System.Collections;
using UnityEngine;

[System.Serializable]
public class FogSettings
{
    public bool fogEnabled = true;
    public Color fogColor = Color.white;
    public FogMode fogMode = FogMode.Exponential;
    public float fogDensity = 0.01f;
    public float startDistance = 0f;
    public float endDistance = 100f;
}

public class FogManager : MonoBehaviour
{
    public static FogManager Instance;
    private Coroutine fogTransitionCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ApplyFogSettings(FogSettings newSettings, float transitionDuration = 1f)
    {
        if (fogTransitionCoroutine != null)
        {
            StopCoroutine(fogTransitionCoroutine);
        }
        fogTransitionCoroutine = StartCoroutine(TransitionFogSettings(newSettings, transitionDuration));
    }

    private IEnumerator TransitionFogSettings(FogSettings newSettings, float duration)
    {
        float time = 0;
        bool initialFogEnabled = RenderSettings.fog;
        Color initialFogColor = RenderSettings.fogColor;
        FogMode initialFogMode = RenderSettings.fogMode;
        float initialFogDensity = RenderSettings.fogDensity;
        float initialStartDistance = RenderSettings.fogStartDistance;
        float initialEndDistance = RenderSettings.fogEndDistance;

        while (time < duration)
        {
            float t = time / duration;
            // Lerp the fog settings based on t
            RenderSettings.fog = newSettings.fogEnabled;
            RenderSettings.fogColor = Color.Lerp(initialFogColor, newSettings.fogColor, t);
            // Note: Fog mode doesn't typically need to be interpolated, but you could handle different modes here if needed.
            RenderSettings.fogMode = newSettings.fogMode;
            RenderSettings.fogDensity = Mathf.Lerp(initialFogDensity, newSettings.fogDensity, t);
            RenderSettings.fogStartDistance = Mathf.Lerp(initialStartDistance, newSettings.startDistance, t);
            RenderSettings.fogEndDistance = Mathf.Lerp(initialEndDistance, newSettings.endDistance, t);

            time += Time.deltaTime;
            yield return null;
        }

        // Ensure the final settings are applied
        RenderSettings.fog = newSettings.fogEnabled;
        RenderSettings.fogColor = newSettings.fogColor;
        RenderSettings.fogMode = newSettings.fogMode;
        RenderSettings.fogDensity = newSettings.fogDensity;
        RenderSettings.fogStartDistance = newSettings.startDistance;
        RenderSettings.fogEndDistance = newSettings.endDistance;
    }
}
