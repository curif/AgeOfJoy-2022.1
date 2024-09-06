using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioRandomizer : MonoBehaviour
{
    public GameObject[] audioSourceObjects;
    public GameObject[] referenceObjects;

    // New settings for randomization
    public float minTimeBetweenSounds = 5f;
    public float maxTimeBetweenSounds = 6f;
    public float chanceOfSoundEffect = 0.5f; // 0.5 = 50% chance

    private AudioSource[] audioSources;
    private List<int> soundQueue = new List<int>();  // Queue of sounds to avoid repeats

    void Start()
    {
        // Initialize the audioSources array based on the assigned audioSourceObjects
        audioSources = new AudioSource[audioSourceObjects.Length];
        for (int i = 0; i < audioSourceObjects.Length; i++)
        {
            audioSources[i] = audioSourceObjects[i].GetComponent<AudioSource>();
        }

        // Fill the sound queue with indexes and shuffle them
        ResetSoundQueue();

        StartCoroutine(RandomSoundRoutine());
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
        if (soundQueue.Count == 0)
        {
            ResetSoundQueue();  // Refill the queue when all sounds have been played
        }

        // Take the next sound from the queue
        int index = soundQueue[0];
        soundQueue.RemoveAt(0);  // Remove the used sound

        if (audioSources[index] != null)
        {
            // Set the audio source's position to a random reference object position
            SetRandomPosition(index);

            // Ensure the AudioSource has an AudioClip to play
            if (audioSources[index].clip != null)
            {
                audioSources[index].Play();

                // Debug log showing the clip name and position of the AudioSource
                Debug.Log($"Playing sound: {audioSources[index].clip.name} at position: {audioSourceObjects[index].transform.position}");
            }
            else
            {
                Debug.Log($"No AudioClip assigned to AudioSource at position: {audioSourceObjects[index].transform.position}");
            }
        }
    }

    void ResetSoundQueue()
    {
        // Fill the queue with all available sound indices
        soundQueue.Clear();
        for (int i = 0; i < audioSources.Length; i++)
        {
            soundQueue.Add(i);
        }

        // Shuffle the queue to randomize the play order
        ShuffleList(soundQueue);
    }

    void ShuffleList(List<int> list)
    {
        // Fisher-Yates shuffle algorithm
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    void SetRandomPosition(int index)
    {
        // Pick a random reference object to assign the position
        int randomRefIndex = Random.Range(0, referenceObjects.Length);
        if (referenceObjects[randomRefIndex] != null)
        {
            audioSourceObjects[index].transform.position = referenceObjects[randomRefIndex].transform.position;
        }
    }
}
