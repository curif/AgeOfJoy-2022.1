using UnityEngine;
using System.Collections;

public class RandomAudioPlayer : MonoBehaviour
{
    private AudioSource audioSource; // Private reference to the AudioSource component

    public float initialDelay = 5.0f; // Initial delay before the first playback check
    public float minDelay = 2.0f; // Minimum delay between playback checks
    public float maxDelay = 5.0f; // Maximum delay between playback checks
    public float playbackChance = 0.5f; // Chance of playback at each check (0.0 to 1.0)

    void Awake()
    {
        // Automatically get the AudioSource component from the current GameObject
        audioSource = GetComponent<AudioSource>();
        // Ensure there is an AudioSource component attached
        if (audioSource == null)
        {
            // Log an error if no AudioSource is found to remind to attach one
            Debug.LogError("RandomAudioPlayer: No AudioSource component found on the GameObject. Please attach one.");
        }
    }

    void Start()
    {
        if (audioSource != null) // Check if the AudioSource was successfully obtained
        {
            StartCoroutine(PlayRandomSound());
        }
    }

    IEnumerator PlayRandomSound()
    {
        yield return new WaitForSeconds(initialDelay); // Initial delay

        while (true) // Infinite loop to continuously check for sound playback
        {
            float delay = Random.Range(minDelay, maxDelay); // Get a random delay
            yield return new WaitForSeconds(delay); // Wait for the delay

            if (Random.value <= playbackChance && audioSource != null) // Check if the sound should play and AudioSource is valid
            {
                audioSource.Play(); // Play the sound
            }
        }
    }
}
