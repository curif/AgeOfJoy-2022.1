using UnityEngine;
using System.Collections;
using Eflatun.SceneReference;

public class KillZoneTeleporter : MonoBehaviour
{
    public TeleportationController teleportController;
    public SceneDocument targetRoom;
    public string playerSpawnPointName = "PlayerSpawnRoom001";

    [Header("Timing Settings")]
    public float teleportDelay = 1.0f;

    [Header("Animation Settings")]
    public bool enableFadeAnimation = false; // Default to OFF

    private Animator fadeAnimator;
    private const string FadeSphereName = "SM_FadeSphere";
    private const string FadeOutTrigger = "FadeOutTrigger";

    private bool hasTriggered = false; // Track if teleportation has already occurred

    private void Start()
    {
        FindFadeSphere();
        FindTeleportController();
    }

    private void FindFadeSphere()
    {
        GameObject fadeSphere = GameObject.Find(FadeSphereName);
        if (fadeSphere != null)
        {
            fadeAnimator = fadeSphere.GetComponent<Animator>();
            if (fadeAnimator == null)
            {
                UnityEngine.Debug.LogError("[KillZoneTeleporter] Animator component not found on SM_FadeSphere.");
            }
        }
        else
        {
            UnityEngine.Debug.LogError("[KillZoneTeleporter] SM_FadeSphere not found in the scene.");
        }
    }

    private void FindTeleportController()
    {
        if (teleportController == null)
        {
            teleportController = FindObjectOfType<TeleportationController>();
            if (teleportController == null)
            {
                UnityEngine.Debug.LogError("[KillZoneTeleporter] TeleportationController not found in the scene.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || hasTriggered) return; // Prevent re-triggering

        hasTriggered = true; // Mark as triggered
        UnityEngine.Debug.Log("[KillZoneTeleporter] Player entered the kill zone.");

        if (enableFadeAnimation && fadeAnimator != null)
        {
            UnityEngine.Debug.Log("[KillZoneTeleporter] Triggering fade-out animation.");
            fadeAnimator.SetTrigger(FadeOutTrigger);
        }

        StartCoroutine(TeleportSequence());
    }

    private IEnumerator TeleportSequence()
    {
        yield return new WaitForSeconds(teleportDelay);

        if (teleportController != null && targetRoom != null)
        {
            UnityEngine.Debug.Log("[KillZoneTeleporter] Executing teleport.");
            targetRoom.PlayerSpawnGameObjectName = playerSpawnPointName;
            SceneReference[] scenesToUnload = new SceneReference[] { };
            teleportController.Teleport(targetRoom, scenesToUnload);
        }
        else
        {
            UnityEngine.Debug.LogError("[KillZoneTeleporter] Teleportation failed. Either teleportController or targetRoom is not set.");
        }
    }
}
