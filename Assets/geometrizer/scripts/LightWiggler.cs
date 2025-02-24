using UnityEngine;

public class LightWiggler : MonoBehaviour
{
    [Header("Offset Settings")]
    // Maximum offset from the original local position on each axis.
    public Vector3 maxOffset = new Vector3(0.1f, 0.1f, 0.1f);
    // Bounds specified as offsets from the original local position.
    public Vector3 minBounds = new Vector3(-1f, -1f, -1f);
    public Vector3 maxBounds = new Vector3(1f, 1f, 1f);

    [Header("Sine Wave Settings")]
    // Time (in seconds) for a full sine wave cycle on each axis.
    public Vector3 sineInterval = new Vector3(2f, 2f, 2f);

    private Vector3 originalLocalPosition;

    void Start()
    {
        originalLocalPosition = transform.localPosition;
    }

    void Update()
    {
        // Calculate smooth sine offsets for each axis.
        float sinX = Mathf.Sin(Time.time * (2 * Mathf.PI / sineInterval.x)) * maxOffset.x;
        float sinY = Mathf.Sin(Time.time * (2 * Mathf.PI / sineInterval.y)) * maxOffset.y;
        float sinZ = Mathf.Sin(Time.time * (2 * Mathf.PI / sineInterval.z)) * maxOffset.z;

        Vector3 newOffset = new Vector3(sinX, sinY, sinZ);
        Vector3 newLocalPosition = originalLocalPosition + newOffset;

        // Compute the allowed local bounds based on the original position.
        Vector3 allowedMin = originalLocalPosition + minBounds;
        Vector3 allowedMax = originalLocalPosition + maxBounds;

        // Check if the new local position is within allowed bounds.
        if (newLocalPosition.x < allowedMin.x || newLocalPosition.x > allowedMax.x ||
            newLocalPosition.y < allowedMin.y || newLocalPosition.y > allowedMax.y ||
            newLocalPosition.z < allowedMin.z || newLocalPosition.z > allowedMax.z)
        {
            // If out of bounds, reset to the original local position.
            transform.localPosition = originalLocalPosition;
        }
        else
        {
            transform.localPosition = newLocalPosition;
        }
    }
}
