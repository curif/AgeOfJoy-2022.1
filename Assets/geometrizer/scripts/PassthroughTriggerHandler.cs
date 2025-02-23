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
        if (other.CompareTag("Player"))
        {
            UnityEngine.Debug.LogWarning("Turning OVRPassthroughLayer ON!");
            if (passthroughLayer != null)
            {
                passthroughLayer.enabled = true;
            }

            Camera triggerCamera = other.GetComponentInChildren<Camera>();
            if (triggerCamera != null)
            {
                triggerCamera.clearFlags = CameraClearFlags.SolidColor;
                UnityEngine.Debug.Log("Camera clear flags set to SolidColor.");

                Transform psMotes = triggerCamera.transform.Find("PS_Motes");
                if (psMotes != null)
                {
                    psMotes.gameObject.SetActive(false);
                    UnityEngine.Debug.Log("PS_Motes disabled.");
                }
                else
                {
                    UnityEngine.Debug.LogWarning("PS_Motes not found on the Camera.");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("No Camera component found on the triggering object.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UnityEngine.Debug.LogWarning("Turning OVRPassthroughLayer OFF!");
            if (passthroughLayer != null)
            {
                passthroughLayer.enabled = false;
            }

            Camera triggerCamera = other.GetComponentInChildren<Camera>();
            if (triggerCamera != null)
            {
                triggerCamera.clearFlags = CameraClearFlags.Skybox;
                UnityEngine.Debug.Log("Camera clear flags set to Skybox.");

                Transform psMotes = triggerCamera.transform.Find("PS_Motes");
                if (psMotes != null)
                {
                    psMotes.gameObject.SetActive(true);
                    UnityEngine.Debug.Log("PS_Motes enabled.");
                }
                else
                {
                    UnityEngine.Debug.LogWarning("PS_Motes not found on the Camera.");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("No Camera component found on the triggering object.");
            }
        }
    }
}
