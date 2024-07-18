using UnityEngine;
using System.Collections;

public class DelayedAnimator : MonoBehaviour
{
    public Animator animator;
    public float minDelay = 2.0f; // Minimum delay in seconds
    public float maxDelay = 5.0f; // Maximum delay in seconds
    private float randomDelay;

    void Start()
    {
        if (animator != null)
        {
            // Calculate a random delay between minDelay and maxDelay
            randomDelay = Random.Range(minDelay, maxDelay);
            Debug.Log($"Random delay set to: {randomDelay} seconds");

            // Start the coroutine to delay the activation
            StartCoroutine(ActivateAnimatorAfterRandomDelay());
        }
        else
        {
            Debug.LogError("Animator component not assigned.");
        }
    }

    IEnumerator ActivateAnimatorAfterRandomDelay()
    {
        // Disable the Animator initially
        animator.enabled = false;

        // Wait for the random delay
        yield return new WaitForSeconds(randomDelay);

        // Enable the Animator after the delay
        animator.enabled = true;
        Debug.Log("Animator enabled");

        // Wait until the animation has finished
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 && !animator.IsInTransition(0));

        // Hide the GameObject after the animation has completed
        gameObject.SetActive(false);
        Debug.Log("GameObject hidden after animation completed");
    }
}
