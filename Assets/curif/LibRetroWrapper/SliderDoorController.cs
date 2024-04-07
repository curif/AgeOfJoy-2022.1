using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SliderDoorController : MonoBehaviour
{
    public Transform doorA; // Reference to DoorA
    public Transform doorB; // Reference to DoorB

    public float openTime = 2f; // Time taken to open the doors
    public float closeTime = 0.5f; // Time taken to close the doors
    public float doorSlideUnits = 0.64f; // Slide units of the doors

    public AudioSource openAudioSource; // AudioSource for opening the doors
    public AudioSource closeAudioSource; // AudioSource for closing the doors

    public bool isDoorOpen = false; // Track the state of the doors
    private Vector3 closedPositionA; // Position when DoorA is fully closed
    private Vector3 closedPositionB; // Position when DoorB is fully closed
    private Vector3 openPositionA; // Position when the doors are fully open
    private Vector3 openPositionB; // Position when the doors are fully open
    private Coroutine currentCoroutine;

    private BoxCollider doorCollider; // Collider blocking the entrance

    void Start()
    {
        // Store the closed positions of the doors
        closedPositionA = doorA.localPosition;
        closedPositionB = doorB.localPosition;

        // Calculate the open position based on the doors' closed positions and slide units
        openPositionA = closedPositionA - Vector3.right * doorSlideUnits;
        openPositionB = closedPositionB - Vector3.right * doorSlideUnits * 2;

        // Get the box collider attached to this GameObject
        doorCollider = GetComponent<BoxCollider>();
    }

    // Method to open or close the doors based on the boolean parameter
    public void SetDoorState(bool isOpen)
    {
        if (isOpen && !isDoorOpen) // If doors should open and they are not already open
            OpenDoors();
        else if (!isOpen && isDoorOpen) // If doors should close and they are not already closed
            CloseDoors();
    }

    // Method to open the doors
    public void OpenDoors()
    {
        if (isDoorOpen) // If doors are already open, return
            return;

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        isDoorOpen = true; // Update the doors state immediately
        if (openAudioSource != null && !openAudioSource.isPlaying)
            openAudioSource.Play(); // Play the open audio if available

        // Start the coroutine to open both doors
        currentCoroutine = StartCoroutine(MoveDoors(openPositionA, openPositionB, openTime));
    }

    // Method to close the doors
    public void CloseDoors()
    {
        if (!isDoorOpen) // If doors are already closed, return
            return;

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        isDoorOpen = false; // Update the doors state immediately
        if (closeAudioSource != null && !closeAudioSource.isPlaying)
            closeAudioSource.Play(); // Play the close audio if available

        // Start the coroutine to close both doors
        currentCoroutine = StartCoroutine(MoveDoors(closedPositionA, closedPositionB, closeTime));

    }

    // Coroutine to move the doors to a target position over time
    private IEnumerator MoveDoors(Vector3 targetPositionA, Vector3 targetPositionB, float duration)
    {
        float startTimeB = Time.time;
        Vector3 startDoorBPosition = doorB.localPosition;
        Vector3 startDoorAPosition = doorA.localPosition;

        float progress = (Time.time - startTimeB) / duration;
        while (Time.time < startTimeB + duration / 2)
        {
            doorB.localPosition = Vector3.Lerp(startDoorBPosition, targetPositionB, progress);

            yield return null;
            progress = (Time.time - startTimeB) / duration;
        }

        doorCollider.enabled = !isDoorOpen; // Disable the box collider

        float startTimeA = Time.time;
        float progressA = (Time.time - startTimeA) / duration;
        while (Time.time < startTimeB + duration)
        {
            doorB.localPosition = Vector3.Lerp(startDoorBPosition, targetPositionB, progress);
            doorA.localPosition = Vector3.Lerp(startDoorAPosition, targetPositionA, progressA);

            yield return null;
            progress = (Time.time - startTimeB) / duration;
            progressA = (Time.time - startTimeA) / duration * 2;
        }

        // Ensure both doors reach the target position precisely
        doorB.localPosition = targetPositionB;
        doorA.localPosition = targetPositionA;

        currentCoroutine = null;
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(SliderDoorController))]
public class SliderDoorControllerEditor  : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Cast the target to TwoPartSliderDoorController
        SliderDoorController doorController = (SliderDoorController)target;

        // Add a button to open the door
        if (GUILayout.Button("Open Door"))
        {
            doorController.OpenDoors();
        }

        // Add a button to close the door
        if (GUILayout.Button("Close Door"))
        {
            doorController.CloseDoors();
        }
    }
}
#endif
