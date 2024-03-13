using UnityEngine;
using System.Collections;

public class FootstepPlayer : MonoBehaviour
{
    public StaticCheck staticCheck; // Reference to the StaticCheck script
    public float stepInterval = 0.3f; // Time in seconds between steps
    public float waitingForWalk = 1f; // Time in seconds to wait for walking

    public AudioSource audioSource;
    public AudioClip[] audioClips;
    private Coroutine footstepCoroutine;

    void Start()
    {
        // Find all AudioSource components attached to this GameObject
        StartCoroutine(PlayFootsteps());
    }

    IEnumerator PlayFootsteps()
    {
        // Randomly select one of the audio sources
        int idxAudioClip = 0;
        while (true)
        {
            // Loop to play footsteps at intervals
            if (!staticCheck.isStatic)
            {
                // Play the footstep sound through the selected audio source
                audioSource.clip = audioClips[idxAudioClip];
                if (!audioSource.isPlaying)
                    audioSource.Play();
                idxAudioClip ++;
                if (idxAudioClip > audioClips.Length - 1)
                    idxAudioClip = 0;
                yield return new WaitForSeconds(stepInterval);
            }
            else
            {
                yield return new WaitForSeconds(waitingForWalk);
            }
        }
    }
}
