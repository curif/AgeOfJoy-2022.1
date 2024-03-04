using UnityEngine;
using System.Collections;

public class FootstepPlayer : MonoBehaviour
{
    public StaticCheck staticCheck; // Reference to the StaticCheck script
    public float stepInterval = 0.5f; // Time in seconds between steps

    private AudioSource[] audioSources;
    private Coroutine footstepCoroutine;

    void Start()
    {
        // Find all AudioSource components attached to this GameObject
        audioSources = GetComponents<AudioSource>();
        // Ensure there's at least one AudioSource component
        if (audioSources.Length == 0)
        {
            Debug.LogError("No AudioSource components found on the GameObject.");
        }
    }

    void Update()
    {
        // Check if the player has just started moving
        if (!staticCheck.isStatic && footstepCoroutine == null)
        {
            // Start playing footsteps
            footstepCoroutine = StartCoroutine(PlayFootsteps());
        }
        else if (staticCheck.isStatic && footstepCoroutine != null)
        {
            // Stop playing footsteps
            StopCoroutine(footstepCoroutine);
            footstepCoroutine = null;
        }
    }

    IEnumerator PlayFootsteps()
    {
        // Loop to play footsteps at intervals as long as the coroutine is not stopped
        while (true)
        {
            if (audioSources.Length > 0)
            {
                // Randomly select one of the audio sources
                AudioSource selectedAudioSource = audioSources[Random.Range(0, audioSources.Length)];
                // Play the footstep sound through the selected audio source
                if (!selectedAudioSource.isPlaying)
                {
                    selectedAudioSource.Play();
                }
            }
            yield return new WaitForSeconds(stepInterval);
        }
    }
}
