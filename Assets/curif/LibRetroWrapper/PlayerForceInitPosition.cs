using UnityEngine;

public class PlayerForceInitPosition : MonoBehaviour
{
    public GameObject player; // the gameobject to move
    public Vector3 position; // the transform to move to

    void Start()
    {
        // move the player to the transform instantly
        player.transform.position = position;
        ConfigManager.WriteConsole($"[PlayerForceInitPosition.Start] change player {player} to position {position}");
    }
}
