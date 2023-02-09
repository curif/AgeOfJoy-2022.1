using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class PlayerPush : MonoBehaviour
{
    public float pushForce = 10.0f;

    private void OnTriggerEnter(Collision collision)
    {
            ConfigManager.WriteConsole($"[PlayerPush] {collision.gameObject.name}");
        Rigidbody npcRigidbody = collision.gameObject.GetComponent<Rigidbody>();
        if (npcRigidbody != null)
        {
            ConfigManager.WriteConsole($"[PlayerPush] {collision.gameObject.name}");
            Vector3 pushDirection = transform.forward;
            npcRigidbody.AddForce(pushDirection * pushForce, ForceMode.Impulse);
        }
    }
}

