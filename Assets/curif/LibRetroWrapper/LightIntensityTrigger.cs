using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // <<< Add this namespace

#if UNITY_EDITOR
using UnityEditor;
#endif

//box collider should include "Player" layer
public class LightIntensityTrigger : MonoBehaviour
{
    public float[] Intensity;
    public float lightTransitionDuration = 1f;
    public UserLightManager userLightManager;

    // <<< New Input System specific variable
    [Tooltip("Assign the Input Action that represents the 'control trigger click' here.")]
    public InputActionReference activateTriggerAction;

    private int intensityIdx = 0;
    private bool _isPlayerInTrigger = false;
    private Collider _playerGrabVolumeCollider = null;

    void Start()
    {
        if (userLightManager == null)
        {
            GameObject userLightGO = GameObject.Find("UserLightManager");
            if (userLightGO != null)
                userLightManager = userLightGO.GetComponent<UserLightManager>();
        }

        // Optional: Check if the action reference is assigned
        if (activateTriggerAction == null || activateTriggerAction.action == null)
        {
            Debug.LogWarning("Activate Trigger Action is not assigned or its action is null on " + gameObject.name + ". Light intensity changes will not be triggered by input.");
        }
    }

    // <<< Enable the input action when the script is enabled
    void OnEnable()
    {
        if (activateTriggerAction != null && activateTriggerAction.action != null)
        {
            activateTriggerAction.action.Enable();
        }
    }

    // <<< Disable the input action when the script is disabled or destroyed
    void OnDisable()
    {
        if (activateTriggerAction != null && activateTriggerAction.action != null)
        {
            activateTriggerAction.action.Disable();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "GrabVolumeSmall")
        {
            _isPlayerInTrigger = true;
            _playerGrabVolumeCollider = other;

            // Assuming ConfigManager exists. Otherwise, use Debug.Log.
            // ConfigManager.WriteConsole($"[LightIntensityTrigger] Player GrabVolumeSmall entered trigger zone.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == _playerGrabVolumeCollider)
        {
            _isPlayerInTrigger = false;
            _playerGrabVolumeCollider = null;

            // Assuming ConfigManager exists. Otherwise, use Debug.Log.
            // ConfigManager.WriteConsole($"[LightIntensityTrigger] Player GrabVolumeSmall exited trigger zone.");
        }
    }

    void Update()
    {
        // Only allow activation if the player's GrabVolumeSmall is currently within this trigger
        if (_isPlayerInTrigger)
        {
            // Check if the assigned activateTriggerAction was performed this frame
            if (activateTriggerAction != null && activateTriggerAction.action != null && activateTriggerAction.action.WasPerformedThisFrame())
            {
                ApplyIntensityChange();
                // Optional: Prevent multiple activations if the user holds the trigger
                // (WasPerformedThisFrame already handles this for single presses)
            }
        }
    }

    public void ApplyIntensityChange()
    {
        intensityIdx++;
        if (intensityIdx >= Intensity.Length)
        {
            intensityIdx = 0;
        }
        if (userLightManager != null && Intensity != null && Intensity.Length > 0)
        {
            userLightManager.ApplyUserLightSettings(null, Intensity[intensityIdx], lightTransitionDuration);
            // Assuming ConfigManager exists. Otherwise, use Debug.Log.
            // ConfigManager.WriteConsole($"[LightIntensityTrigger] Applying intensity change to index: {intensityIdx}, value: {Intensity[intensityIdx]}");
        }
        else
        {
            // Assuming ConfigManager exists. Otherwise, use Debug.LogWarning.
            ConfigManager.WriteConsoleWarning("UserLightManager is not assigned or Intensity array is empty.");
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LightIntensityTrigger))]
    public class LightIntensityTriggerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LightIntensityTrigger script = (LightIntensityTrigger)target;

            if (GUILayout.Button("Simulate Intensity Change"))
            {
                script.ApplyIntensityChange();
            }
        }
    }
#endif
}