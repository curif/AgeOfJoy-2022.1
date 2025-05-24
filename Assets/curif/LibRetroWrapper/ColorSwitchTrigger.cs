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

    // <<< Drag in the Animation component or GameObject with an Animation component
    [Tooltip("Drag the GameObject or component that has the Animation you want to play.")]
    public Animation targetAnimation;

    private int colorIdx = 0;
    private bool _isPlayerInTrigger = false; // Tracks if the player's GrabVolumeSmall is currently inside this trigger
    private Collider _playerGrabVolumeCollider = null; // Stores a reference to the specific GrabVolumeSmall that entered
    private AudioSource _audioSource; // Reference to AudioSource component on this GameObject

    void Awake()
    {
        // Cache AudioSource component
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            Debug.LogWarning($"[ColorSwitchTrigger] No AudioSource found on {gameObject.name}. Audio will not play.");
        }

        // Optional: warn if targetAnimation is not assigned
        if (targetAnimation == null)
        {
            Debug.LogWarning($"[ColorSwitchTrigger] No target Animation assigned on {gameObject.name}. Animation will not play.");
        }
    }

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

    // Enable the input action when the script is enabled
    void OnEnable()
    {
        if (activateTriggerAction != null && activateTriggerAction.action != null)
        {
            activateTriggerAction.action.Enable();
        }
    }

    // Disable the input action when the script is disabled or destroyed
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
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == _playerGrabVolumeCollider)
        {
            _isPlayerInTrigger = false;
            _playerGrabVolumeCollider = null;
        }
    }

    void Update()
    {
        // Only allow activation if the player's GrabVolumeSmall is currently within this trigger
        if (_isPlayerInTrigger)
        {
            if (activateTriggerAction != null && activateTriggerAction.action != null && activateTriggerAction.action.WasPerformedThisFrame())
            {
                ApplyColorChange();
            }
        }
    }

    public void ApplyColorChange()
    {
        // Play audio if available
        if (_audioSource != null)
        {
            _audioSource.Play();
        }

        // Play target animation if assigned
        if (targetAnimation != null)
        {
            targetAnimation.Play();
        }

        colorIdx++;
        if (colorIdx >= Colors.Length)
        {
            colorIdx = 0;
        }
        if (userLightManager != null && Colors != null && Colors.Length > 0)
        {
            userLightManager.ApplyUserLightSettings(Colors[colorIdx], transitionDuration: lightTransitionDuration);
        }
        else
        {
            ConfigManager.WriteConsoleWarning("UserLightManager is not assigned or Colors array is empty.");
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ColorSwitchTrigger))]
    public class ColorSwitchTriggerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ColorSwitchTrigger script = (ColorSwitchTrigger)target;
            if (GUILayout.Button("Simulate Color Change"))
            {
                script.ApplyColorChange();
            }
        }
    }
#endif
}
