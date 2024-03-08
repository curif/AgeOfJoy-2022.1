using UnityEngine;

public class StaticCheck : MonoBehaviour
{
    public float idleTimeThreshold = 1.0f;  // Adjust this threshold as needed
    private Vector2 lastPosition;
    private float idleTimer;
    public bool isStatic;

    void Start()
    {
        // Initialize the last position
        lastPosition = new Vector2(transform.position.x, transform.position.y);
    }

    void Update()
    {
        // Check if the object is static in x, y coordinates
        if (IsStatic())
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= idleTimeThreshold)
            {
                // Object has been static for the specified period
                isStatic = true;
                // Debug.Log("Object is now static!");
            }
        }
        else
        {
            // Object is not static, reset the timer
            idleTimer = 0f;
            isStatic = false;
        }

        // Update the last position
        lastPosition = new Vector2(transform.position.x, transform.position.y);
    }

    bool IsStatic()
    {
        // Check if the object's position in x and y hasn't changed
        return transform.position.x == lastPosition.x && transform.position.y == lastPosition.y;
    }
}
