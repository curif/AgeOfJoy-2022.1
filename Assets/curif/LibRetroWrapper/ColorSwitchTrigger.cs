using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // <<< Add this namespace

#if UNITY_EDITOR
using UnityEditor;
#endif

//box collider should include "Player" layer
public class ColorSwitchTrigger : MonoBehaviour
{
    public Color[] Colors;
    public float lightTransitionDuration = 1f;
    public UserLightManager userLightManager;

    // <<< New Input System specific variable
    [Tooltip("Assign the Input Action that represents the 'control trigger click' here.")]
    public InputActionReference activateTriggerAction;

    private int colorIdx = 0;
    private bool _isPlayerInTrigger = false; // Tracks if the player's GrabVolumeSmall is currently inside this trigger
    private Collider _playerGrabVolumeCollider = null; // Stores a reference to the specific GrabVolumeSmall that entered

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
            Debug.LogWarning("Activate Trigger Action is not assigned or its action is null on " + gameObject.name + ". Color changes will not be triggered by input.");
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
        // Check if the entering collider is the player's GrabVolumeSmall
        if (other.name == "GrabVolumeSmall")
        {
            _isPlayerInTrigger = true;
            _playerGrabVolumeCollider = other; // Store reference to prevent issues if multiple objects could enter

            // Optional: Log when player enters trigger zone
            // ConfigManager.WriteConsole($"[ColorSwitchTrigger] Player GrabVolumeSmall entered trigger zone.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the exiting collider is the one we were tracking
        if (other == _playerGrabVolumeCollider)
        {
            _isPlayerInTrigger = false;
            _playerGrabVolumeCollider = null; // Clear the reference

            // Optional: Log when player exits trigger zone
            // ConfigManager.WriteConsole($"[ColorSwitchTrigger] Player GrabVolumeSmall exited trigger zone.");
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
                ApplyColorChange();
                // WasPerformedThisFrame already handles single presses, no need for extra flags
            }
        }
    }

    public void ApplyColorChange()
    {
        colorIdx++;
        if (colorIdx >= Colors.Length)
        {
            colorIdx = 0;
        }
        if (userLightManager != null && Colors != null && Colors.Length > 0)
        {
            userLightManager.ApplyUserLightSettings(Colors[colorIdx], transitionDuration: lightTransitionDuration);
            // Optional: Log the change
            // ConfigManager.WriteConsole($"[ColorSwitchTrigger] Applying color change to index: {colorIdx}, value: {Colors[colorIdx]}");
        }
        else
        {
            // Assuming ConfigManager exists in your project. Otherwise, use Debug.LogWarning.
            ConfigManager.WriteConsoleWarning("UserLightManager is not assigned or Colors array is empty.");
        }
    }

#if UNITY_EDITOR
    // Corrected Custom Editor for ColorSwitchTrigger
    [CustomEditor(typeof(ColorSwitchTrigger))] // <<< Target this script
    public class ColorSwitchTriggerEditor : Editor // <<< Renamed class
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ColorSwitchTrigger script = (ColorSwitchTrigger)target;

            if (GUILayout.Button("Simulate Color Change")) // <<< Changed button text
            {
                script.ApplyColorChange(); // <<< Call the correct method
            }
        }
    }
#endif
}