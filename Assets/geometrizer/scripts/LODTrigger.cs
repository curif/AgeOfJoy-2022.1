using UnityEngine;

public class LODTrigger : MonoBehaviour
{
    private Collider triggerCollider;
    private GameObject childToToggle;

    private void Awake()
    {
        // Get the first child GameObject
        if (transform.childCount > 0)
        {
            childToToggle = transform.GetChild(0).gameObject;
            childToToggle.SetActive(false); // Deactivate the child at start
        }
        else
        {
            UnityEngine.Debug.LogWarning("No child found on this GameObject.");
        }

        // Get the Collider on this GameObject
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null)
        {
            UnityEngine.Debug.LogError("No Collider found on this GameObject.");
            return;
        }

        if (!triggerCollider.isTrigger)
        {
            UnityEngine.Debug.Log("Assigned collider is not a trigger. Setting it to trigger.");
            triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (childToToggle != null)
        {
            childToToggle.SetActive(true);
            UnityEngine.Debug.Log("Trigger entered: activating child.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (childToToggle != null)
        {
            childToToggle.SetActive(false);
            UnityEngine.Debug.Log("Trigger exited: deactivating child.");
        }
    }
}
