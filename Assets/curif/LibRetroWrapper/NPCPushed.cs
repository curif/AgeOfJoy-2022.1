using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class NPCPushed : MonoBehaviour
{
    public float pushForce = 10.0f;

    private Rigidbody rb;
    private Vector3 normalVelocity;
    private Vector3 normalAngularVelocity;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        normalVelocity = rb.velocity;
        normalAngularVelocity = rb.angularVelocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        ConfigManager.WriteConsole($"[NPCPushed] {gameObject.name} {collision.gameObject.name}");
        
        if (collision.gameObject.name == "OVRPlayerControllerGalery")
        {
            ConfigManager.WriteConsole($"[NPCPushed] {gameObject.name}: player {collision.gameObject.name}");
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Vector3 pushDirection = (transform.position - collision.transform.position).normalized;
            rb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
        }
        else
        {
            rb.velocity = normalVelocity;
            rb.angularVelocity = normalAngularVelocity;
        }
    }
}

