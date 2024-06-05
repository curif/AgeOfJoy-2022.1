using UnityEngine;
using UnityEngine.Rendering;

public class SkyboxChangeTrigger : MonoBehaviour
{
    public Material newSkyboxMaterial; // Assign new Skybox Material in the Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Make sure the player has a tag "Player"
        {
            // Swap the skybox material
            if (newSkyboxMaterial != null)
            {
                RenderSettings.skybox = newSkyboxMaterial;
                DynamicGI.UpdateEnvironment(); // Update global illumination to reflect new skybox
            }
        }
    }
}
