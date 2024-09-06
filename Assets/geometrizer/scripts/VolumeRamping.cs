using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class VolumeRamping : MonoBehaviour
{
    public float rampDurationMin = 3f; // Minimum time to ramp up the volume
    public float rampDurationMax = 7f; // Maximum time to ramp up the volume
    private AudioSource audioSource;
    private float targetVolume; // The initial volume set in the AudioSource
    private float rampDuration; // The actual duration chosen randomly
    private float rampTimer = 0f;
    private bool isRamping = true;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        targetVolume = audioSource.volume; // Store the initial volume
        audioSource.volume = 0f; // Start volume at 0

        // Choose a random ramp duration between min and max
        rampDuration = Random.Range(rampDurationMin, rampDurationMax);

        // Ensure the audio clip exists and pick a random start time
        if (audioSource.clip != null)
        {
            float randomStartTime = Random.Range(0f, audioSource.clip.length);
            audioSource.time = randomStartTime; // Set the playback start time

            // Play after setting the random start time
            audioSource.Play();
        }

        audioSource.loop = true; // Ensure the audio will loop
    }

    void Update()
    {
        if (isRamping)
        {
            rampTimer += Time.deltaTime;
            float t = Mathf.Clamp01(rampTimer / rampDuration);
            audioSource.volume = Mathf.Lerp(0f, targetVolume, t);

            if (t >= 1f)
            {
                isRamping = false; // Stop the ramping once complete
            }
        }
    }
}
