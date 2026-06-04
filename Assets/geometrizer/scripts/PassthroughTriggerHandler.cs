using UnityEngine;
using System.Collections;
using AOJ.Managers; // Import the namespace containing the EventManager

public class PassthroughTriggerHandler : MonoBehaviour
{
    private OVRPassthroughLayer passthroughLayer;
    private Animator fadeSphereAnimator;

    [SerializeField]
    private float timeBeforeFadeIn = 2f; // Default to 2 seconds

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

        // Find SM_FadeSphere and get its Animator component
        GameObject fadeSphere = GameObject.Find("SM_FadeSphere");
        if (fadeSphere != null)
        {
            fadeSphereAnimator = fadeSphere.GetComponent<Animator>();
            if (fadeSphereAnimator == null)
            {
                UnityEngine.Debug.LogWarning("Animator component not found on SM_FadeSphere.");
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("SM_FadeSphere not found in the scene.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (MixedRealityManager.Instance != null
                && MixedRealityManager.Instance.CurrentMode != ExperienceMode.VR)
            {
                return;
            }

            // Set the EventManager's boolean to true indicating passthrough is active.
            if (EventManager.Instance != null)
            {
                EventManager.Instance.IsPassthrough = true;
            }

            UnityEngine.Debug.LogWarning("Turning OVRPassthroughLayer ON!");
            OVRPassthroughLayer layer = ResolvePassthroughLayer();
            if (layer != null)
            {
                if (OVRManager.instance != null)
                    OVRManager.instance.isInsightPassthroughEnabled = true;
                layer.hidden = false;
                layer.enabled = true;
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

            // Start fade-in coroutine
            if (fadeSphereAnimator != null)
            {
                StartCoroutine(FadeInAfterDelay());
            }
        }
    }

    private IEnumerator FadeInAfterDelay()
    {
        yield return new WaitForSeconds(timeBeforeFadeIn);
        fadeSphereAnimator.SetTrigger("FadeInTrigger");
        UnityEngine.Debug.Log("FadeInTrigger activated.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Set the EventManager's boolean to false since passthrough is now off.
            if (EventManager.Instance != null)
            {
                EventManager.Instance.IsPassthrough = false;
            }

            UnityEngine.Debug.LogWarning("Turning OVRPassthroughLayer OFF!");
            OVRPassthroughLayer layer = ResolvePassthroughLayer();
            if (layer != null)
                layer.enabled = false;

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

    OVRPassthroughLayer ResolvePassthroughLayer()
    {
        if (passthroughLayer != null)
            return passthroughLayer;

        GameObject mainCamera = GameObject.FindWithTag("MainCamera");
        if (mainCamera == null)
            return null;

        passthroughLayer = mainCamera.GetComponent<OVRPassthroughLayer>();
        if (passthroughLayer == null)
            passthroughLayer = mainCamera.AddComponent<OVRPassthroughLayer>();

        return passthroughLayer;
    }
}
