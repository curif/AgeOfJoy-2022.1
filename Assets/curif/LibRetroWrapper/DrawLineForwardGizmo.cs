using UnityEngine;

public class DrawLineForwardGizmo : MonoBehaviour
{
    public int length = 5;
    void Update()
    {
        // Calculate the end position 5 meters forward in the GameObject's local space
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + transform.forward * length;

        // Draw the line in the Game view during runtime
        Debug.DrawLine(startPosition, endPosition, Color.red);
    }
}
