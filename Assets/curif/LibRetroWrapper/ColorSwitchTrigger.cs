using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

//box collider should include "Player" layer
public class ColorSwitchTrigger : MonoBehaviour
{
    public Color[] Colors;
    public float lightTransitionDuration = 1f;
    public UserLightManager userLightManager;

    private int colorIdx = 0;
    void Start()
    {
        if (userLightManager == null)
        {
            GameObject userLightGO = GameObject.Find("UserLightManager");
            if (userLightGO != null)
                userLightManager = userLightGO.GetComponent<UserLightManager>();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.name != "GrabVolumeSmall")
        {
            //      ConfigManager.WriteConsole($"[LightIntensityTrigger] Triggered in scene: '{triggerScene.name}', object: '{other}'");
            return;
        }

        ApplyColorChange();
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
        }
        else
        {
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