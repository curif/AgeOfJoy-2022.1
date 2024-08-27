using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Oculus.Interaction;

[RequireComponent(typeof(BoxCollider))]
public class CollisionDetection : MonoBehaviour
{
    [Tooltip("List of object names that are allowed to collide. If empty, all objects are allowed.")]
    [SerializeField]
    public CabinetInformation.ReceiveImpacts impact;

    public class CollisionEvent : UnityEvent<string> { }

    // UnityEvents for entering, staying, and exiting trigger
    public CollisionEvent OnCollisionStart;
    public CollisionEvent onCollisionContinue;
    public CollisionEvent OnCollisionEnd;

    // Variable to store the name of the last object that collided
    [SerializeField]
    public string lastCollidingObject;

    // Ensure the Collider is set to trigger mode
    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("InteractablePart");
    }
    void OnCollisionEnter(Collision colliding)
    {
        if (IsCollisionAllowed(colliding.gameObject.name))
        {
            if (impact != null && !colliding.rigidbody.isKinematic)
            {
                // Calculate rejection direction (opposite of collision normal)
                Vector3 rejectDirection = -colliding.contacts[0].normal;

                // Apply force to reject the colliding object
                colliding.rigidbody.AddForce(rejectDirection * impact.repulsion.force, ForceMode.Impulse);
            }

            lastCollidingObject = colliding.gameObject.name; // Register the name of the colliding object
            OnCollisionStart?.Invoke(lastCollidingObject);
        }
    }

    // Called while the object is still in the trigger area
    void OnCollisionStay(Collision other)
    {
        if (IsCollisionAllowed(other.gameObject.name))
        {
            lastCollidingObject = other.gameObject.name; // Update the last colliding object
            onCollisionContinue?.Invoke(lastCollidingObject);
        }
    }

    // Called when the object exits the trigger area
    void OnCollisionExit(Collision other)
    {
        if (IsCollisionAllowed(other.gameObject.name))
        {
            lastCollidingObject = other.gameObject.name; // Register the object that was last touched
            OnCollisionEnd?.Invoke(lastCollidingObject);
        }
    }

    // Check if collision with the object is allowed based on the name list
    private bool IsCollisionAllowed(string objectName)
    {
        // If the list is empty, allow collision with any object
        //if (allowedObjects.Count == 0)
        //    return true;

        // Otherwise, only allow objects whose name is in the list
        return impact.parts.Contains(objectName);
    }

}
