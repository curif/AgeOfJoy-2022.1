using System.Diagnostics;
using UnityEngine;

public class LODTrigger : MonoBehaviour
{
    [SerializeField]
    private bool enableTrigger = true;

    private Collider triggerCollider;
    private GameObject childToToggle;

    public bool EnableTrigger
    {
        get => enableTrigger;
        set
        {
            enableTrigger = value;
            UpdateTriggerState();
        }
    }

    private void Awake()
    {
        // Get the first child GameObject
        if (transform.childCount > 0)
        {
            childToToggle = transform.GetChild(0).gameObject;
            childToToggle.SetActive(false); // Deactivate at start
        }
        else
        {
            //Debug.LogError("LODTrigger Error: No child found on this GameObject.");
        }

        triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null)
        {
        //    Debug.LogError("LODTrigger Error: No Collider found on this GameObject.");
            return;
        }

        if (!triggerCollider.isTrigger)
        {
          //  Debug.LogError("LODTrigger Warning: Assigned collider is not a trigger. Setting it to trigger.");
            triggerCollider.isTrigger = true;
        }

        UpdateTriggerState();
    }

#if UNITY_EDITOR
    // This ensures the toggle works at runtime in the Inspector
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateTriggerState();
        }
    }
#endif

    private void UpdateTriggerState()
    {
        if (triggerCollider != null)
        {
            triggerCollider.enabled = enableTrigger;
        }

        if (childToToggle != null && !enableTrigger)
        {
            childToToggle.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enableTrigger) return;

        if (childToToggle != null)
        {
            childToToggle.SetActive(true);
//            Debug.Log("LODTrigger: Trigger entered — activating child.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!enableTrigger) return;

        if (childToToggle != null)
        {
            childToToggle.SetActive(false);
        //    Debug.Log("LODTrigger: Trigger exited — deactivating child.");
        }
    }
}
