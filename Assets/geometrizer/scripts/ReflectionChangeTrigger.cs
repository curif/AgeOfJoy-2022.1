using UnityEngine;
using UnityEngine.Rendering;

public class ReflectionChangeTrigger : MonoBehaviour
{
    public Cubemap newReflectionCubemap; // Assign this in the Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Make sure the player has a tag "Player"
        {
            RenderSettings.customReflection = newReflectionCubemap;
            DynamicGI.UpdateEnvironment();
        }
    }
}
