using UnityEngine;
using UnityEngine.Rendering;

public class SkyboxChangeTrigger : MonoBehaviour
{
    public Material newSkyboxMaterial; // Assign new Skybox Material in the Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Make sure the player has a tag "Player"
        {
            // Only update the skybox if the material is different
            if (RenderSettings.skybox != newSkyboxMaterial && newSkyboxMaterial != null)
            {
                RenderSettings.skybox = newSkyboxMaterial;
                DynamicGI.UpdateEnvironment(); // Update global illumination to reflect new skybox
            }
        }
    }
}
