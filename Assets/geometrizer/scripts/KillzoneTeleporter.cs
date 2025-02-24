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
    public float postTeleportDelay = 1.0f;

    private Animator fadeAnimator;
    private const string FadeSphereName = "SM_FadeSphere";
    private const string FadeOutTrigger = "FadeOutTrigger";
    private const string FadeInTrigger = "FadeInTrigger";

    private void Start()
    {
        // Keep this object alive across scene loads
        DontDestroyOnLoad(gameObject);

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
        if (!other.CompareTag("Player")) return;

        UnityEngine.Debug.Log("[KillZoneTeleporter] Player entered the kill zone.");

        if (fadeAnimator != null)
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
            yield break;
        }

        yield return new WaitForSeconds(postTeleportDelay);

        FindFadeSphere(); // Re-find in case it was lost

        if (fadeAnimator != null)
        {
            UnityEngine.Debug.Log("[KillZoneTeleporter] Triggering fade-in animation.");
            fadeAnimator.SetTrigger(FadeInTrigger);
            StartCoroutine(DestroyAfterAnimation(fadeAnimator, FadeInTrigger));
        }
        else
        {
            UnityEngine.Debug.LogError("[KillZoneTeleporter] FadeSphere Animator is missing AFTER teleport, cannot play fade-in animation.");
            Destroy(gameObject); // Just destroy if no animator
        }
    }

    private IEnumerator DestroyAfterAnimation(Animator animator, string triggerName)
    {
        if (animator == null) yield break;

        // Wait until the animator is actually in the FadeInTrigger state
        while (!animator.GetCurrentAnimatorStateInfo(0).IsTag(triggerName))
        {
            yield return null;
        }

        // Wait for the animation to fully complete
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        UnityEngine.Debug.Log("[KillZoneTeleporter] Fade-in animation complete. Destroying teleporter.");
        Destroy(gameObject);
    }
}
