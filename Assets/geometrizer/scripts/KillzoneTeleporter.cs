using UnityEngine;
using Eflatun.SceneReference;

public class KillZoneTeleporter : MonoBehaviour
{
    public TeleportationController teleportController;
    public SceneDocument targetRoom;
    public string playerSpawnPointName = "PlayerSpawnRoom001"; // Adjust this to your spawn point's name in room001

    private void Start()
    {
        if (teleportController == null)
        {
            teleportController = FindObjectOfType<TeleportationController>(); // Find the TeleportationController in the scene
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Make sure that your player GameObject has the "Player" tag
        {
            if (teleportController != null && targetRoom != null)
            {
                targetRoom.PlayerSpawnGameObjectName = playerSpawnPointName; // Ensure the spawn point name is correctly set
                SceneReference[] scenesToUnload = new SceneReference[] { }; // List scenes to unload, if any
                teleportController.Teleport(targetRoom, scenesToUnload);
            }
            else
            {
                Debug.LogError("TeleportationController or targetRoom not set.");
            }
        }
    }
}
