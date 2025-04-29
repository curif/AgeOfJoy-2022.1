using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

//box collider should include "Player" layer
public class LightIntensityTrigger : MonoBehaviour
{
    public float[] Intensity;
    public float lightTransitionDuration = 1f;
    public UserLightManager userLightManager;

    private int intensityIdx = 0;
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

        ApplyIntensityChange();
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