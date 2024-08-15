using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class TouchDetection : MonoBehaviour
{
    [Tooltip("List of object names that are allowed to collide. If empty, all objects are allowed.")]
    public List<string> allowedObjects = new List<string>();

    [System.Serializable]
    public class CollisionEvent : UnityEvent<string> { }

    // UnityEvents for entering, staying, and exiting trigger
    public CollisionEvent onTriggerEnterEvent;
    public CollisionEvent onTriggerStayEvent;
    public CollisionEvent onTriggerExitEvent;

    // Variable to store the name of the last object that collided
    public string lastCollidingObject;

    // Called when another object enters the trigger area
    private void OnTriggerEnter(Collider other)
    {
        if (IsCollisionAllowed(other.gameObject.name))
        {
            lastCollidingObject = other.gameObject.name; // Register the name of the colliding object
            Debug.Log("Touch started with: " + lastCollidingObject);
            onTriggerEnterEvent?.Invoke(lastCollidingObject);
        }
    }

    // Called while the object is still in the trigger area
    private void OnTriggerStay(Collider other)
    {
        if (IsCollisionAllowed(other.gameObject.name))
        {
            lastCollidingObject = other.gameObject.name; // Update the last colliding object
            Debug.Log("Still touching: " + lastCollidingObject);
            onTriggerStayEvent?.Invoke(lastCollidingObject);
        }
    }

    // Called when the object exits the trigger area
    private void OnTriggerExit(Collider other)
    {
        if (IsCollisionAllowed(other.gameObject.name))
        {
            lastCollidingObject = other.gameObject.name; // Register the object that was last touched
            Debug.Log("Touch ended with: " + lastCollidingObject);
            onTriggerExitEvent?.Invoke(lastCollidingObject);
        }
    }

    // Check if collision with the object is allowed based on the name list
    private bool IsCollisionAllowed(string objectName)
    {
        // If the list is empty, allow collision with any object
        //if (allowedObjects.Count == 0)
        //    return true;

        // Otherwise, only allow objects whose name is in the list
        return allowedObjects.Contains(objectName);
    }

    // Ensure the Collider is set to trigger mode
    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true; // Ensure it's set to a trigger
    }
}
