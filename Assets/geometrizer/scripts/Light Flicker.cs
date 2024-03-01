using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public Light flickerLight; // The light component to flicker
    public float intensityLevel1 = 0.5f; // First intensity level
    public float intensityLevel2 = 1.5f; // Second intensity level
    public float minWaitTime = 0.1f; // Minimum wait time before flickering
    public float maxWaitTime = 0.5f; // Maximum wait time before flickering

    private void Start()
    {
        if (flickerLight == null)
        {
            flickerLight = GetComponent<Light>();
        }

        StartCoroutine(Flicker());
    }

    private System.Collections.IEnumerator Flicker()
    {
        while (true)
        {
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            flickerLight.intensity = flickerLight.intensity == intensityLevel1 ? intensityLevel2 : intensityLevel1;
        }
    }
}
