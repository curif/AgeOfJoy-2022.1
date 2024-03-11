using UnityEngine;

public class Swinger : MonoBehaviour
{
    public float swingSpeed = 2.0f; // Speed of the swing
    public float minAngle = -30f; // Minimum swing angle
    public float maxAngle = 30f; // Maximum swing angle
    public Vector3 swingAxis = Vector3.forward; // Axis of rotation (can be changed in inspector to Vector3.right for X, Vector3.up for Y, or Vector3.forward for Z)

    // Update is called once per frame
    void Update()
    {
        Swing();
    }

    private void Swing()
    {
        // Calculate the swing angle using a sine wave
        float angle = Mathf.Lerp(minAngle, maxAngle, (Mathf.Sin(Time.time * swingSpeed) + 1.0f) / 2.0f);
        transform.localRotation = Quaternion.Euler(swingAxis * angle);
    }
}
