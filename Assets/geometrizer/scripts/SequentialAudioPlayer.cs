using System.Collections;
using UnityEngine;

public class SequentialAudioPlayer : MonoBehaviour
{
    public AudioSource audioSource1;
    public AudioSource audioSource2;
    public AudioSource audioSource3;

    public float delay1 = 1f; // Delay before playing the first audio
    public float delay2 = 2f; // Delay before playing the second audio
    public float delay3 = 3f; // Delay before playing the third audio

    private void Start()
    {
        StartCoroutine(PlayAudioSequence());
    }

    private IEnumerator PlayAudioSequence()
    {
        yield return new WaitForSeconds(delay1);
        audioSource1.Play();

        yield return new WaitForSeconds(audioSource1.clip.length + delay2);
        audioSource2.Play();

        yield return new WaitForSeconds(audioSource2.clip.length + delay3);
        audioSource3.Play();
    }
}
