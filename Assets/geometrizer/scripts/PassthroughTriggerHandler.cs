using UnityEngine;

public class PassthroughTriggerHandler : MonoBehaviour
{
    private OVRPassthroughLayer passthroughLayer;

    private void Start()
    {
        // Find the Main Camera and get the OVRPassthroughLayer component
        GameObject mainCamera = GameObject.FindWithTag("MainCamera");
        if (mainCamera != null)
        {
            passthroughLayer = mainCamera.GetComponent<OVRPassthroughLayer>();
            if (passthroughLayer == null)
            {
                UnityEngine.Debug.LogWarning("OVRPassthroughLayer component not found on the Main Camera.");
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("Main Camera not found in the scene.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && passthroughLayer != null)
        {
            UnityEngine.Debug.LogWarning("Turning OVRPassthroughLayer ON!");
            passthroughLayer.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && passthroughLayer != null)
        {
            UnityEngine.Debug.LogWarning("Turning OVRPassthroughLayer OFF!");
            passthroughLayer.enabled = false;
        }
    }
}
