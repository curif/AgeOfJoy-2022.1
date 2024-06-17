using UnityEngine;

public class LightIntensityController : MonoBehaviour
{
    public Light myLight; // Assign your GameObject's Light component in the Inspector

    public float nearDistance = 0f; // Minimum distance for maximum light intensity
    public float farDistance = 10f; // Maximum distance for minimum light intensity
    public float maxIntensity = 1f; // Maximum light intensity

    private Transform player;
    private float range;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // Automatically find the player

        if (player != null)
        {
            Debug.Log("Success: Player found.");
            range = farDistance - nearDistance; // Calculate range once if it does not change
        }
        else
        {
            Debug.LogError("Failure: Player not found. Please ensure the player is tagged correctly.");
        }
    }

    void Update()
    {
        if (player == null) return; // In case the player isn't found, or is destroyed

        // Calculate the distance from the player to this GameObject only once per frame
        float distance = Vector3.Distance(player.position, transform.position);

        // Clamp the distance within the near and far values
        distance = Mathf.Clamp(distance, nearDistance, farDistance);

        // Calculate the light intensity (inverse linear interpolation)
        float intensity = maxIntensity * (1 - (distance - nearDistance) / range);

        // Set the light intensity, clamping it to make sure it doesn't exceed maxIntensity
        myLight.intensity = Mathf.Clamp(intensity, 0, maxIntensity);
    }
}
