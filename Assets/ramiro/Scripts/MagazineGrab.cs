/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>XR one-hand grab for the magazine root (pick up, release returns to pose).</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Magazine))]
public class MagazineGrab : MonoBehaviour
{
    [Header("Grab")]
    [Tooltip("Child mesh used for the grab collider (e.g. RightCover). Movement always uses this root.")]
    public string grabTarget = "RightCover";
    public bool returnOnRelease = true;
    public bool hideHandsOnGrab = true;
    [Min(0f)] public float returnDurationSeconds = 0.15f;

    [Header("Page turn (while held or touched)")]
    [Range(0.2f, 0.95f)] public float stickPageThreshold = 0.55f;
    [Range(0.05f, 0.5f)] public float stickPageRelease = 0.25f;
    [Min(0f)] public float touchExitGraceSeconds = 0.12f;

#if UNITY_EDITOR
    [Header("Editor Simulation")]
    [Tooltip("Play Mode only — hold the key to simulate grab in front of the camera.")]
    public bool editorSimulateGrab = true;
    public KeyCode editorGrabKey = KeyCode.G;
    public float editorGrabDistanceMeters = 0.35f;
#endif

    Magazine magazine;
    MRCustomObjectGrab customGrab;
    XRGrabInteractable grabInteractable;
    bool initialized;

    bool grabIsLeft;
    bool stickPastNext;
    bool stickPastPrev;
    readonly HashSet<XRDirectInteractor> touchingHands = new HashSet<XRDirectInteractor>();
    Coroutine touchExitResetCoroutine;

#if UNITY_EDITOR
    bool editorGrabActive;
    Vector3 homeWorldPosition;
    Quaternion homeWorldRotation;
    Vector3 homeLocalScale;
    Coroutine returnHomeCoroutine;
#endif

    void Awake()
    {
        magazine = GetComponent<Magazine>();

        customGrab = GetComponent<MRCustomObjectGrab>();
        if (customGrab == null)
            customGrab = gameObject.AddComponent<MRCustomObjectGrab>();

        customGrab.Configure(new MRCustomObjectGrabYaml
        {
            TwoHands = false,
            ReturnOnRelease = returnOnRelease,
            HideHands = hideHandsOnGrab,
            ReturnDurationSeconds = returnDurationSeconds,
            // Snap authored grip (RightCover) to the hand — not the magazine root.
            Target = grabTarget
        }, transform);

        EnsureInitialized();
    }

    void Start()
    {
        EnsureInitialized();

#if UNITY_EDITOR
        if (editorSimulateGrab && Application.isEditor)
        {
            ConfigManager.WriteConsole(
                $"[MagazineGrab] Editor: hold {editorGrabKey} to grab/release | arrows or stick to turn pages while held");
        }
#endif
    }

    void Update()
    {
        HandlePageInput();

#if UNITY_EDITOR
        if (Application.isEditor)
            HandleEditorPageKeysWhenHeld();
        if (Application.isEditor && (editorSimulateGrab || editorGrabActive))
            HandleEditorGrabInput();
#endif
    }

    public bool IsHeldNow() => IsMagazineHeld();

    void LateUpdate()
    {
#if UNITY_EDITOR
        if (Application.isEditor && editorGrabActive)
            FollowEditorCamera();
#endif
    }

    void OnDestroy()
    {
        if (!initialized || grabInteractable == null)
            return;

        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);
        grabInteractable.hoverEntered.RemoveListener(OnDirectHoverEntered);
        grabInteractable.hoverExited.RemoveListener(OnDirectHoverExited);
    }

    void EnsureInitialized()
    {
        if (initialized)
            return;

        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
            return;

        ApplyGrabColliders();
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
        grabInteractable.hoverEntered.AddListener(OnDirectHoverEntered);
        grabInteractable.hoverExited.AddListener(OnDirectHoverExited);
        NotifyPlacementPoseUpdated();
        initialized = true;
    }

    /// <summary>Call after MR placement moves this prop so release returns to the new pose.</summary>
    public void NotifyPlacementPoseUpdated()
    {
        customGrab?.NotifyPlacementPoseUpdated();

#if UNITY_EDITOR
        CaptureHomePose();
#endif
    }

    public void SetReturnOnRelease(bool enabled)
    {
        returnOnRelease = enabled;
        customGrab?.SetReturnOnRelease(enabled);
    }

#if UNITY_EDITOR
    public void SetEditorSimulateGrab(bool enabled) => editorSimulateGrab = enabled;
#endif

#if UNITY_EDITOR
    public void BeginEditorGrabExternally()
    {
        if (!Application.isEditor || editorGrabActive)
            return;

        BeginEditorGrab();
    }
#endif

    void OnGrabbed(SelectEnterEventArgs args)
    {
        grabIsLeft = IsLeftInteractor(args.interactorObject);
        CancelTouchExitReset();
        // Direct hover normally precedes selection. Preserve the latch so a held
        // stick cannot turn a second page during the touch-to-grab transition.
        if (touchingHands.Count == 0)
            ResetStickPageState();
        magazine.OpenMagazine();
        ConfigManager.WriteConsole($"[MagazineGrab] grabbed with {(grabIsLeft ? "left" : "right")} hand");
    }

    void OnReleased(SelectExitEventArgs _)
    {
        if (touchingHands.Count == 0)
            ResetStickPageState();
        if (!IsShelfPreparedMagazine() && touchingHands.Count == 0)
            magazine.ResetMagazine();
    }

    void OnDirectHoverEntered(HoverEnterEventArgs args)
    {
        XRDirectInteractor directInteractor = args.interactorObject as XRDirectInteractor;
        if (directInteractor == null || IsDockedShelfMagazine())
            return;

        CancelTouchExitReset();
        if (!touchingHands.Add(directInteractor))
            return;

        if (touchingHands.Count == 1 && !IsMagazineHeld())
        {
            ResetStickPageState();
            magazine.OpenMagazine();
        }
    }

    void OnDirectHoverExited(HoverExitEventArgs args)
    {
        XRDirectInteractor directInteractor = args.interactorObject as XRDirectInteractor;
        if (directInteractor == null || !touchingHands.Remove(directInteractor))
            return;

        if (touchingHands.Count == 0 && !IsMagazineHeld())
        {
            CancelTouchExitReset();
            touchExitResetCoroutine = StartCoroutine(ResetTouchStateAfterGrace());
        }
    }

    IEnumerator ResetTouchStateAfterGrace()
    {
        if (touchExitGraceSeconds > 0f)
            yield return new WaitForSeconds(touchExitGraceSeconds);

        touchExitResetCoroutine = null;
        if (touchingHands.Count == 0 && !IsMagazineHeld())
        {
            ResetStickPageState();
            if (!IsShelfPreparedMagazine())
                magazine.ResetMagazine();
        }
    }

    void CancelTouchExitReset()
    {
        if (touchExitResetCoroutine == null)
            return;

        StopCoroutine(touchExitResetCoroutine);
        touchExitResetCoroutine = null;
    }

    bool IsShelfPreparedMagazine() => GetComponent<MRSpawnedShelfMagazine>() != null;

    bool IsDockedShelfMagazine()
    {
        MRSpawnedShelfMagazine shelfMagazine = GetComponent<MRSpawnedShelfMagazine>();
        return shelfMagazine != null && shelfMagazine.IsDockedAtShelf();
    }

    void ApplyGrabColliders()
    {
        if (grabInteractable == null || string.IsNullOrEmpty(grabTarget))
            return;

        Transform colliderRoot = FindChild(grabTarget);
        if (colliderRoot == null)
        {
            ConfigManager.WriteConsoleWarning($"[MagazineGrab] grab collider child not found: {grabTarget}");
            return;
        }

        grabInteractable.colliders.Clear();
        foreach (Collider collider in colliderRoot.GetComponentsInChildren<Collider>(true))
        {
            if (collider != null && !collider.isTrigger)
                grabInteractable.colliders.Add(collider);
        }
    }

    void HandlePageInput()
    {
        float stickX;
        if (IsMagazineHeld())
            stickX = ReadGrabStickX(grabIsLeft);
        else
        {
            if (IsDockedShelfMagazine())
                return;
            if (!TryReadTouchingHandStickX(out stickX))
                return;
        }

        ProcessPageStick(stickX);
    }

    void ProcessPageStick(float stickX)
    {
        if (stickX <= -stickPageThreshold)
        {
            if (!stickPastNext && magazine.CanGoNextPage())
                magazine.NextPage();
            stickPastNext = true;
            stickPastPrev = false;
        }
        else if (stickX >= stickPageThreshold)
        {
            if (!stickPastPrev && magazine.CanGoPreviousPage())
                magazine.PreviousPage();
            stickPastPrev = true;
            stickPastNext = false;
        }
        else if (Mathf.Abs(stickX) <= stickPageRelease)
            ResetStickPageState();
    }

    bool TryReadTouchingHandStickX(out float stickX)
    {
        stickX = 0f;
        bool foundHand = false;
        bool requestsNext = false;
        bool requestsPrevious = false;

        foreach (XRDirectInteractor hand in touchingHands)
        {
            if (hand == null)
                continue;

            float handStickX = ReadGrabStickX(IsLeftInteractor(hand));
            foundHand = true;
            requestsNext |= handStickX <= -stickPageThreshold;
            requestsPrevious |= handStickX >= stickPageThreshold;

            if (Mathf.Abs(handStickX) > Mathf.Abs(stickX))
                stickX = handStickX;
        }

        // Opposite commands from two touching hands cancel each other.
        if (requestsNext && requestsPrevious)
            stickX = 0f;

        return foundHand;
    }

#if UNITY_EDITOR
    void HandleEditorPageKeysWhenHeld()
    {
        if (!Application.isEditor || magazine == null || !magazine.editorPageKeys || !IsMagazineHeld())
            return;

        if (MREditorInput.WasAnyPressed(KeyCode.LeftArrow, KeyCode.A))
        {
            if (magazine.CanGoNextPage())
                magazine.NextPage();
        }
        else if (MREditorInput.WasAnyPressed(KeyCode.RightArrow, KeyCode.D))
        {
            if (magazine.CanGoPreviousPage())
                magazine.PreviousPage();
        }
    }
#endif

    bool IsMagazineHeld()
    {
        if (grabInteractable != null && grabInteractable.isSelected)
            return true;

#if UNITY_EDITOR
        return Application.isEditor && editorGrabActive;
#else
        return false;
#endif
    }

    void ResetStickPageState()
    {
        stickPastNext = false;
        stickPastPrev = false;
    }

    static float ReadGrabStickX(bool isLeft)
    {
        OVRInput.Controller controller = isLeft ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
#if UNITY_EDITOR
        if (Application.isEditor && OVRInput.IsControllerConnected(controller))
            return OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, controller).x;

        if (Application.isEditor)
            return MREditorInput.GamepadStickX();
#endif
        return OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, controller).x;
    }

    static bool IsLeftInteractor(IXRSelectInteractor interactor)
    {
        var interactorBehaviour = interactor as MonoBehaviour;
        if (interactorBehaviour == null)
            return false;

        ChangeControls controls = FindObjectOfType<ChangeControls>();
        if (controls == null)
            return GuessLeftFromName(interactorBehaviour.name);

        Transform interactorTransform = interactorBehaviour.transform;
        if (controls.leftHandXRControl != null
            && interactorTransform.IsChildOf(controls.leftHandXRControl.transform))
            return true;

        if (controls.rightHandXRControl != null
            && interactorTransform.IsChildOf(controls.rightHandXRControl.transform))
            return false;

        return GuessLeftFromName(interactorBehaviour.name);
    }

    static bool GuessLeftFromName(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
            return false;

        string lower = objectName.ToLowerInvariant();
        if (lower.Contains("left"))
            return true;
        if (lower.Contains("right"))
            return false;

        return false;
    }

    Transform FindChild(string childName)
    {
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
                return child;
        }
        return null;
    }

#if UNITY_EDITOR
    void HandleEditorGrabInput()
    {
        if (grabInteractable != null && grabInteractable.isSelected)
            return;

        bool held = MREditorInput.IsHeld(editorGrabKey);

        if (!held && editorGrabActive)
            EndEditorGrab();
        else if (held && !editorGrabActive && editorSimulateGrab)
            BeginEditorGrab();
    }

    void BeginEditorGrab()
    {
        editorGrabActive = true;
        grabIsLeft = false;
        ResetStickPageState();

        if (returnHomeCoroutine != null)
        {
            StopCoroutine(returnHomeCoroutine);
            returnHomeCoroutine = null;
        }

        magazine.OpenMagazine();
    }

    void EndEditorGrab()
    {
        editorGrabActive = false;
        ResetStickPageState();
        if (!IsShelfPreparedMagazine())
            magazine.ResetMagazine();

        if (returnOnRelease)
            ReturnHome();
    }

    void FollowEditorCamera()
    {
        Transform cameraTransform = ResolveEditorCameraTransform();
        if (cameraTransform == null)
            return;

        transform.position = cameraTransform.position + cameraTransform.forward * editorGrabDistanceMeters;
    }

    void ReturnHome()
    {
        if (returnHomeCoroutine != null)
            StopCoroutine(returnHomeCoroutine);

        if (returnDurationSeconds > 0f)
            returnHomeCoroutine = StartCoroutine(ReturnHomeSmooth());
        else
            SnapHome();
    }

    void SnapHome()
    {
        transform.SetPositionAndRotation(homeWorldPosition, homeWorldRotation);
        transform.localScale = homeLocalScale;
    }

    IEnumerator ReturnHomeSmooth()
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float elapsed = 0f;

        while (elapsed < returnDurationSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / returnDurationSeconds);
            transform.SetPositionAndRotation(
                Vector3.Lerp(startPos, homeWorldPosition, t),
                Quaternion.Slerp(startRot, homeWorldRotation, t));
            yield return null;
        }

        returnHomeCoroutine = null;
        SnapHome();
    }

    void CaptureHomePose()
    {
        homeWorldPosition = transform.position;
        homeWorldRotation = transform.rotation;
        homeLocalScale = transform.localScale;
    }

    static Transform ResolveEditorCameraTransform()
    {
        if (Camera.main != null)
            return Camera.main.transform;

        Camera anyCamera = FindObjectOfType<Camera>();
        return anyCamera != null ? anyCamera.transform : null;
    }
#endif
}
