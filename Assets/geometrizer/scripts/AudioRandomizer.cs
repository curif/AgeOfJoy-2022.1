using UnityEngine;
using System.Collections;

public class AudioRandomizer : MonoBehaviour
{
    public GameObject[] audioSourceObjects;
    public GameObject[] referenceObjects;

    // New settings for randomization
    public float minTimeBetweenSounds = 5f;
    public float maxTimeBetweenSounds = 6f;
    public float chanceOfSoundEffect = 0.5f; // 0.5 = 50% chance

    private AudioSource[] audioSources;

    void Start()
    {
        // Initialize the audioSources array based on the assigned audioSourceObjects
        audioSources = new AudioSource[audioSourceObjects.Length];
        for (int i = 0; i < audioSourceObjects.Length; i++)
        {
            audioSources[i] = audioSourceObjects[i].GetComponent<AudioSource>();
        }

        ApplySettingsBasedOnTransforms();
        StartCoroutine(RandomSoundRoutine());
    }

    void ApplySettingsBasedOnTransforms()
    {
        for (int i = 0; i < audioSources.Length && i < referenceObjects.Length; i++)
        {
            if (audioSources[i] != null && referenceObjects[i] != null)
            {
                audioSourceObjects[i].transform.position = referenceObjects[i].transform.position;
            }
        }
    }

    IEnumerator RandomSoundRoutine()
    {
        while (true)
        {
            // Wait for a random time between the specified min and max
            float waitTime = Random.Range(minTimeBetweenSounds, maxTimeBetweenSounds);
            yield return new WaitForSeconds(waitTime);

            // Check the chance of playing a sound effect
            if (Random.value <= chanceOfSoundEffect)
            {
                PlayRandomSound();
            }
        }
    }

    void PlayRandomSound()
    {
        // Select a random AudioSource to play
        int index = Random.Range(0, audioSources.Length);
        if (audioSources[index] != null)
        {
            // Ensure the AudioSource has an AudioClip to play
            if (audioSources[index].clip != null)
            {
                audioSources[index].Play();

                // Debug log showing the clip name and position of the AudioSource
                Debug.Log($"Playing sound: {audioSources[index].clip.name} at position: {audioSourceObjects[index].transform.position}");
            }
            else
            {
                // If no AudioClip is assigned, you could log or handle this situation as needed
                Debug.Log($"No AudioClip assigned to AudioSource at position: {audioSourceObjects[index].transform.position}");
            }
        }
    }
}
