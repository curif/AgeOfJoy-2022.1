using UnityEngine;
using System.Collections;
using System.Collections.Generic; // This line is necessary for List<>

public class AudioManager : MonoBehaviour
{
    public List<AudioSource> audioSources = new List<AudioSource>();
    public List<Transform> positions = new List<Transform>();
    public float minPauseBeforeStart = 5f;
    public float maxPauseBeforeStart = 10f;
    public float minDelayBeforeNextSound = 2f;
    public float maxDelayBeforeNextSound = 5f;
    public float chanceToPlaySound = 0.5f; // Chance to play a sound (0 to 1)

    private void Start()
    {
        StartCoroutine(PlaySoundsWithRandomDelays());
    }

    private IEnumerator PlaySoundsWithRandomDelays()
    {
        // Initial pause before starting
        yield return new WaitForSeconds(Random.Range(minPauseBeforeStart, maxPauseBeforeStart));

        while (true)
        {
            // Check if we should play the sound this time
            if (Random.value <= chanceToPlaySound)
            {
                PlayRandomSoundAtRandomPosition();
            }

            // Wait for a random delay before playing the next sound
            yield return new WaitForSeconds(Random.Range(minDelayBeforeNextSound, maxDelayBeforeNextSound));
        }
    }

    private void PlayRandomSoundAtRandomPosition()
    {
        if (audioSources.Count == 0 || positions.Count == 0) return;

        // Choose a random audio source and position
        AudioSource selectedAudioSource = audioSources[Random.Range(0, audioSources.Count)];
        Transform selectedPosition = positions[Random.Range(0, positions.Count)];

        // Move the audio source to the position
        selectedAudioSource.transform.position = selectedPosition.position;

        // Play the sound
        selectedAudioSource.Play();
    }
}
