using UnityEngine;

public class FogTrigger : MonoBehaviour
{
    public FogSettings fogSettings;
    public float transitionDuration = 2f; // Default to 2 seconds, but you can adjust this in the Inspector

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider is the player
        if (other.CompareTag("Player")) // Ensure your player GameObject has the "Player" tag
        {
            FogManager.Instance.ApplyFogSettings(fogSettings, transitionDuration);
        }
    }
}
