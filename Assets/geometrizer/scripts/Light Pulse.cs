using UnityEngine;

public class LightPulse : MonoBehaviour
{
    public Light pulseLight; // The light component to pulse
    public float minIntensity = 0.5f; // Minimum intensity level
    public float maxIntensity = 1.5f; // Maximum intensity level
    public float pulseSpeed = 2.0f; // Speed of the pulse

    private void Start()
    {
        if (pulseLight == null)
        {
            pulseLight = GetComponent<Light>();
        }
    }

    private void Update()
    {
        pulseLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.PingPong(Time.time * pulseSpeed, 1));
    }
}
