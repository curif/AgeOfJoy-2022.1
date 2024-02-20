using UnityEngine;
//Automatically updates fog settings when loaded.
public class SceneFogSettings : MonoBehaviour
{
    public FogSettings settings; // This variable holds the fog settings for the scene.
    public float transitionDuration = 2f; // Default to 2 seconds, but you can adjust this in the Inspector

    void Start()
    {
        if (FogManager.Instance != null)
        {
            FogManager.Instance.ApplyFogSettings(settings, transitionDuration); // Apply the settings with a transition.
        }
    }
}
