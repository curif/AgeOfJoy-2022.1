using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float rotationSpeed = 10f;
    public RotationAxis rotationAxis = RotationAxis.Up; // Default rotation axis is set to up
    Vector3 axisVector;

    // Enumeration to represent different rotation axes
    public enum RotationAxis
    {
        Up,
        Right,
        Forward
    }

    void Start()
    {
        // Get the vector corresponding to the selected rotation axis
        axisVector = GetAxisVector(rotationAxis);
    }

    void Update()
    {
        // Rotate the GameObject around the specified axis
        transform.Rotate(axisVector, rotationSpeed * Time.deltaTime);
    }

    // Helper method to convert enum to vector
    Vector3 GetAxisVector(RotationAxis axis)
    {
        switch (axis)
        {
            case RotationAxis.Up:
                return Vector3.up;
            case RotationAxis.Right:
                return Vector3.right;
            case RotationAxis.Forward:
                return Vector3.forward;
            default:
                return Vector3.up;
        }
    }
}
