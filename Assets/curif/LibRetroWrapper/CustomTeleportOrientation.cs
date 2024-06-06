using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CustomTeleportOrientation : MonoBehaviour
{
    public Transform player; // Reference to the player's XR Rig or camera
    public TeleportationArea area;
    void Start()
    {
        // Subscribe to the Teleporting event
        area.teleporting.AddListener(OnTeleporting);
    }

    void OnTeleporting(TeleportingEventArgs args)
    {
        // Calculate the inverted forward direction
        Quaternion targetRotation = transform.rotation;
        Quaternion invertedForwardRotation = Quaternion.Euler(0f, 180f, 0f);
        Quaternion finalRotation = targetRotation * invertedForwardRotation;

        // Apply the new rotation to the player
        player.rotation = finalRotation;
    }
}
