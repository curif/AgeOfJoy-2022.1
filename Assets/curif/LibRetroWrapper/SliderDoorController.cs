using UnityEngine;
using System.Collections;

public class SliderDoorController : MonoBehaviour
{
    public float openTime = 2f; // Time taken to open the door
    public float closeTime = 0.5f; // Time taken to close the door
    public float openPercentage = 0.5f; // Percentage at which the door is considered open
    public AudioSource openAudioSource; // AudioSource for opening the door
    public AudioSource closeAudioSource; // AudioSource for closing the door

    private Material doorMaterial; // Material with the Slider property
    private BoxCollider doorCollider; // Collider blocking the entrance
    private Coroutine currentCoroutine;
    private bool isDoorOpen = false; // Track the state of the door

    void Start()
    {
        // Get the material from the mesh renderer attached to this GameObject
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            doorMaterial = meshRenderer.material;
        }
        else
        {
            ConfigManager.WriteConsoleError("[SliderDoorController start] MeshRenderer component not found on GameObject.");
        }

        // Get the box collider attached to this GameObject
        doorCollider = GetComponent<BoxCollider>();
    }

    // Method to open or close the door based on the boolean parameter
    public void SetDoorState(bool isOpen)
    {
        if (isOpen && !isDoorOpen) // If door should open and it's not already open
            OpenDoor();
        else if (!isOpen && isDoorOpen) // If door should close and it's not already closed
            CloseDoor();
    }

    // Method to open the door
    public void OpenDoor()
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        isDoorOpen = true; // Update the door state immediately
        doorCollider.enabled = false; // Disable the box collider
        if (openAudioSource != null && !openAudioSource.isPlaying)
            openAudioSource.Play(); // Play the open audio if available
        currentCoroutine = StartCoroutine(ChangeSliderValue(1f, openTime));
    }

    // Method to close the door
    public void CloseDoor()
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        isDoorOpen = false; // Update the door state immediately
        if (closeAudioSource != null && !closeAudioSource.isPlaying)
            closeAudioSource.Play(); // Play the close audio if available
        currentCoroutine = StartCoroutine(ChangeSliderValue(0f, closeTime));
    }

    // Coroutine to change the value of the Slider property over time
    private IEnumerator ChangeSliderValue(float targetValue, float duration)
    {
        float startTime = Time.time;
        float startValue = doorMaterial.GetFloat("_Slide"); // Adjust property name
        float speedFactor;

        while (Time.time < startTime + duration)
        {
            float elapsedTime = Time.time - startTime;
            float progress = elapsedTime / duration;
            float sliderValue = Mathf.Lerp(startValue, targetValue, progress);

            // Gradually reduce the speed as it approaches the target value
            float distanceToTarget = Mathf.Abs(targetValue - sliderValue);
            speedFactor = Mathf.Clamp01(1 - distanceToTarget);

            doorMaterial.SetFloat("_Slide", sliderValue); // Adjust property name
            yield return null;
        }

        doorMaterial.SetFloat("_Slide", targetValue); // Ensure we reach the target value precisely
        if (targetValue < openPercentage) // If the door is not fully open
        {
            doorCollider.enabled = true; // Enable the box collider
        }
        else
        {
            doorCollider.enabled = false; // Disable the box collider
        }
        isDoorOpen = (targetValue == 1f); // Update the door state
        currentCoroutine = null;
    }
}
