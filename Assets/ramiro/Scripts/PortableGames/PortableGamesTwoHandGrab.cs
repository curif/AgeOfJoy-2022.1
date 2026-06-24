/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Two-handed grab for the portable games device: both handles must be held to open the menu;
/// releasing either hand ends the session and snaps the device back to its table pose.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(PortableGames))]
[DefaultExecutionOrder(1000)]
public class PortableGamesTwoHandGrab : MonoBehaviour
{
    const string LogPrefix = "[PortableGamesTwoHandGrab]";
    const string HandleLeftName = "HandleLeft";
    const string HandleRightName = "HandleRight";
    static readonly string[] GrabInteractionLayers = { "InteractablePart" };
    const string GrabPhysicsLayerName = "InteractablePart";

    [SerializeField] bool hideHandsOnGrab = true;
    [Tooltip("Empty transform on the stand where the device rests when docked. If set, grab/release uses this pose instead of the device root at Start.")]
    [SerializeField] Transform restAnchor;
    [Tooltip("Support mesh that must stay on the table when the device is picked up. Auto-detected from child named Support/Suporte/Stand.")]
    [SerializeField] Transform standMeshRoot;
    [Tooltip("Parent for the docked assembly (usually the MR spawn root). Defaults to stand root or this object's parent at Start.")]
    [SerializeField] Transform dockMountRoot;
    [Tooltip("Extra transforms to lift with this object (e.g. Screen sibling). Auto-adds sibling Screen when empty.")]
    [SerializeField] Transform[] carryOnGrab;
    [Tooltip("Only renderers under this root are used for auto handle placement (exclude stand/support mesh). Defaults to this object.")]
    [SerializeField] Transform handleMeshRoot;
    [Tooltip("Seconds to ease back to the table pose. 0 = instant snap.")]
    [SerializeField] float returnDurationSeconds = 0.15f;
    [SerializeField] bool logDebug;
    [Tooltip("When true, handle positions are derived from mesh bounds on Awake.")]
    [SerializeField] bool autoPlaceHandlesFromMesh = true;
    [SerializeField] Vector3 leftHandleLocalOffset = new Vector3(-0.055f, -0.01f, 0.03f);
    [SerializeField] Vector3 rightHandleLocalOffset = new Vector3(0.055f, -0.01f, 0.03f);
    [SerializeField] Vector3 handleColliderSize = new Vector3(0.045f, 0.055f, 0.07f);
    [SerializeField] Vector3 holdRotationOffsetEuler = new Vector3(-10f, 0f, 90f);
    [Tooltip("Smooth held rotation (0 = instant). Reduces controller jitter and twist flips.")]
    [SerializeField] float rotationSmoothing = 18f;

#if UNITY_EDITOR
    [Header("Editor Simulation")]
    [Tooltip("Play Mode only — simulates two-handed grab in front of the Scene/Game camera.")]
    [SerializeField] bool enableEditorSimulation = true;
    [SerializeField] KeyCode editorToggleGrabKey = KeyCode.G;
    [SerializeField] float editorHandDistanceMeters = 0.4f;
    [SerializeField] float editorHandSpanMeters = 0.16f;
    [SerializeField] float editorRotationStepDegrees = 1f;
    [SerializeField] bool logEditorControlsOnStart = true;

    bool editorSimActive;
    Transform editorSimLeft;
    Transform editorSimRight;
#endif

    PortableGames portable;
    Rigidbody body;
    Transform handleLeft;
    Transform handleRight;
    XRSimpleInteractable leftHandle;
    XRSimpleInteractable rightHandle;

    bool leftHeld;
    bool rightHeld;
    bool dualHeld;
    Transform leftFollow;
    Transform rightFollow;
    Vector3 lockedWorldScale;

    Vector3 homeWorldPosition;
    Quaternion homeWorldRotation;
    Vector3 homeLocalScale;
    Vector3 standLocalPosition;
    Quaternion standLocalRotation;
    Vector3 standLocalScale = Vector3.one;
    Transform resolvedDockMount;
    bool standDetachedFromDevice;
    bool deviceLiftedOffStand;
    readonly List<CarryPose> carryPoses = new List<CarryPose>();
    Vector3 lastStableForward = Vector3.forward;
    Vector3 lastStableRightDir = Vector3.right;

    readonly List<Renderer> hiddenLeftRenderers = new List<Renderer>();
    readonly List<Renderer> hiddenRightRenderers = new List<Renderer>();
    Coroutine returnHomeCoroutine;

    struct CarryPose
    {
        public Transform Target;
        public Transform OriginalParent;
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public Vector3 LocalScale;
    }

    public bool IsDualHeld =>
#if UNITY_EDITOR
        dualHeld || editorSimActive;
#else
        dualHeld;
#endif

    void Awake()
    {
        portable = GetComponent<PortableGames>();
        EnsureNotStatic();
        body = EnsureRigidbody();

        if (autoPlaceHandlesFromMesh)
            FitHandlesFromMeshBounds();

        handleLeft = EnsureHandle(HandleLeftName, leftHandleLocalOffset, out leftHandle);
        handleRight = EnsureHandle(HandleRightName, rightHandleLocalOffset, out rightHandle);
        EnsureStandMeshRoot();
        EnsureRestAnchor();
        EnsureCarryOnGrab();

        leftHandle.selectEntered.AddListener(args => OnHandleGrabbed(true, args));
        leftHandle.selectExited.AddListener(_ => OnHandleReleased(true));
        rightHandle.selectEntered.AddListener(args => OnHandleGrabbed(false, args));
        rightHandle.selectExited.AddListener(_ => OnHandleReleased(false));
    }

    void Start()
    {
        if (standMeshRoot != null && transform.IsChildOf(standMeshRoot))
            resolvedDockMount = standMeshRoot;
        else
            resolvedDockMount = dockMountRoot != null ? dockMountRoot : transform.parent;

        CacheStandLocalPose();
        lockedWorldScale = transform.lossyScale;
        CaptureHomePose();
        SetDockedPhysics(true);
        Log($"ready handles L={leftHandleLocalOffset} R={rightHandleLocalOffset}");
#if UNITY_EDITOR
        if (enableEditorSimulation && logEditorControlsOnStart && Application.isEditor)
        {
            ConfigManager.WriteConsole(
                $"{LogPrefix} Editor sim: {editorToggleGrabKey}=toggle grab | " +
                "PgUp/PgDn=X | Home/End=Y | Insert/Delete=Z | Shift=5° step");
        }
#endif
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Application.isEditor && enableEditorSimulation)
            HandleEditorSimulationInput();
#endif
    }

    void LateUpdate()
    {
        if (dualHeld)
            FollowTwoHands();
        else
            SnapHandlesToOffsets();
    }

    void OnDestroy()
    {
        if (leftHandle != null)
        {
            leftHandle.selectEntered.RemoveAllListeners();
            leftHandle.selectExited.RemoveAllListeners();
        }

        if (rightHandle != null)
        {
            rightHandle.selectEntered.RemoveAllListeners();
            rightHandle.selectExited.RemoveAllListeners();
        }
    }

    public void NotifyPlacementPoseUpdated()
    {
        if (dualHeld || leftHeld || rightHeld)
            return;

        CaptureHomePose();
        Log("home pose updated from MR placement");
    }

    public void CaptureHomePose()
    {
        if (restAnchor != null)
        {
            homeWorldPosition = restAnchor.position;
            homeWorldRotation = restAnchor.rotation;
        }
        else
        {
            homeWorldPosition = transform.position;
            homeWorldRotation = transform.rotation;
        }

        homeLocalScale = transform.localScale;
    }

    void OnHandleGrabbed(bool isLeft, SelectEnterEventArgs args)
    {
        if (isLeft)
        {
            leftHeld = true;
            leftFollow = ResolveFollowTransform(args.interactorObject);
            if (hideHandsOnGrab)
                StartCoroutine(HideHandVisualsNextFrame(isLeft, args.interactorObject));
        }
        else
        {
            rightHeld = true;
            rightFollow = ResolveFollowTransform(args.interactorObject);
            if (hideHandsOnGrab)
                StartCoroutine(HideHandVisualsNextFrame(isLeft, args.interactorObject));
        }

        UpdateDualHeldState();
    }

    void OnHandleReleased(bool isLeft)
    {
        if (isLeft)
        {
            leftHeld = false;
            leftFollow = null;
            RestoreHandVisuals(true);
        }
        else
        {
            rightHeld = false;
            rightFollow = null;
            RestoreHandVisuals(false);
        }

        bool wasHeld = dualHeld;
        UpdateDualHeldState();

        if (wasHeld && !dualHeld)
            EndDualGrab();
    }

    void UpdateDualHeldState()
    {
        bool nowDual = leftHeld && rightHeld;
        if (!dualHeld && nowDual)
            BeginDualGrab();

        dualHeld = nowDual;
    }

    void BeginDualGrab()
    {
        // A quick release+re-grab (controller/hand jitter, or releasing one hand and re-gripping
        // while the other stays held) can re-enter here while the device is still lifted or mid
        // return to its dock. Re-capturing the home pose then would lock "home" to a mid-air spot
        // and the device would never return to the table (only the device floats — the stand is
        // already detached on the table). Only re-dock bookkeeping when the device is truly docked.
        bool stillLifted = returnHomeCoroutine != null || deviceLiftedOffStand || standDetachedFromDevice;

        if (returnHomeCoroutine != null)
        {
            StopCoroutine(returnHomeCoroutine);
            returnHomeCoroutine = null;
        }

        if (!stillLifted)
            CaptureHomePose();

        lockedWorldScale = transform.lossyScale;
        lastStableForward = transform.forward;
        lastStableRightDir = transform.right;

        if (body != null)
        {
            body.isKinematic = true;
            body.WakeUp();
        }

        if (!stillLifted)
            DetachFromStandForGrab();

        portable?.BeginSession();
        Log("dual grab — menu session started");
    }

    void EndDualGrab()
    {
        ForceEndHandleSelection(leftHandle);
        ForceEndHandleSelection(rightHandle);

        portable?.EndSession();
        PayphoneHandsetGrab.ForceShowPlayerHands();
        RestoreHandVisuals(true);
        RestoreHandVisuals(false);

        if (body != null)
            body.isKinematic = true;

        ReturnHome();
        Log("dual grab released — returned to table pose");
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
        if (resolvedDockMount != null)
            transform.SetParent(resolvedDockMount, worldPositionStays: true);

        transform.SetPositionAndRotation(homeWorldPosition, homeWorldRotation);
        transform.localScale = homeLocalScale;
        RestoreCarryRoots();
        ReattachStandToDevice();
        deviceLiftedOffStand = false;
        SetDockedPhysics(true);
    }

    IEnumerator ReturnHomeSmooth()
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float duration = returnDurationSeconds;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.SetPositionAndRotation(
                Vector3.Lerp(startPos, homeWorldPosition, t),
                Quaternion.Slerp(startRot, homeWorldRotation, t));
            yield return null;
        }

        returnHomeCoroutine = null;
        SnapHome();
    }

    void FollowTwoHands()
    {
        if (leftFollow == null || rightFollow == null)
            return;

        Vector3 mid = (leftFollow.position + rightFollow.position) * 0.5f;
        if (!TryComputeHeldRotation(
                leftFollow.position,
                rightFollow.position,
                holdRotationOffsetEuler,
                out Quaternion targetRot))
            return;

        Quaternion rot = targetRot;
        if (rotationSmoothing > 0f)
        {
            float t = 1f - Mathf.Exp(-rotationSmoothing * Time.deltaTime);
            rot = Quaternion.Slerp(transform.rotation, targetRot, t);
        }

        transform.SetPositionAndRotation(mid, rot);
        ApplyLockedWorldScale();
    }

    bool TryComputeHeldRotation(
        Vector3 leftPos,
        Vector3 rightPos,
        Vector3 holdOffsetEuler,
        out Quaternion rotation)
    {
        rotation = transform.rotation;

        Vector3 rightDir = rightPos - leftPos;
        float span = rightDir.magnitude;
        if (span < 0.0001f)
            return false;

        rightDir /= span;

        if (lastStableRightDir.sqrMagnitude > 0.0001f && Vector3.Dot(rightDir, lastStableRightDir) < 0f)
            rightDir = lastStableRightDir;
        else
            lastStableRightDir = rightDir;

        Vector3 up = Vector3.up;
        Vector3 forward = Vector3.Cross(up, rightDir);
        if (forward.sqrMagnitude < 0.0001f)
            forward = lastStableForward.sqrMagnitude > 0.0001f ? lastStableForward : transform.forward;
        else
        {
            forward.Normalize();
            lastStableForward = forward;
        }

        Quaternion baseRot = Quaternion.LookRotation(forward, up);
        rotation = baseRot * Quaternion.Euler(holdOffsetEuler);
        return true;
    }

    void ApplyLockedWorldScale()
    {
        if (transform.parent == null)
        {
            transform.localScale = lockedWorldScale;
            return;
        }

        Vector3 parentScale = transform.parent.lossyScale;
        transform.localScale = new Vector3(
            lockedWorldScale.x / Mathf.Max(0.0001f, parentScale.x),
            lockedWorldScale.y / Mathf.Max(0.0001f, parentScale.y),
            lockedWorldScale.z / Mathf.Max(0.0001f, parentScale.z));
    }

    Transform EnsureHandle(string handleName, Vector3 localOffset, out XRSimpleInteractable handleInteractable)
    {
        Transform existing = transform.Find(handleName);
        GameObject handleObject;
        if (existing != null)
        {
            handleObject = existing.gameObject;
        }
        else
        {
            handleObject = new GameObject(handleName);
            handleObject.transform.SetParent(transform, false);
        }

        handleObject.transform.localPosition = localOffset;
        handleObject.transform.localRotation = Quaternion.identity;
        handleObject.transform.localScale = Vector3.one;

        int physicsLayer = LayerMask.NameToLayer(GrabPhysicsLayerName);
        if (physicsLayer >= 0)
            handleObject.layer = physicsLayer;

        RemoveHandlePhysicsArtifacts(handleObject);

        BoxCollider box = handleObject.GetComponent<BoxCollider>();
        if (box == null)
            box = handleObject.AddComponent<BoxCollider>();
        box.isTrigger = false;
        box.size = handleColliderSize;
        box.center = Vector3.zero;

        handleInteractable = handleObject.GetComponent<XRSimpleInteractable>();
        if (handleInteractable == null)
            handleInteractable = handleObject.AddComponent<XRSimpleInteractable>();

        handleInteractable.interactionLayers = InteractionLayerMask.GetMask(GrabInteractionLayers);
        handleInteractable.colliders.Clear();
        handleInteractable.colliders.Add(box);

        return handleObject.transform;
    }

    static void RemoveHandlePhysicsArtifacts(GameObject handleObject)
    {
        XRGrabInteractable legacyGrab = handleObject.GetComponent<XRGrabInteractable>();
        if (legacyGrab != null)
            DestroyImmediate(legacyGrab);

        Rigidbody handleBody = handleObject.GetComponent<Rigidbody>();
        if (handleBody != null)
            DestroyImmediate(handleBody);
    }

    void SnapHandlesToOffsets()
    {
        if (handleLeft != null)
        {
            handleLeft.localPosition = leftHandleLocalOffset;
            handleLeft.localRotation = Quaternion.identity;
        }

        if (handleRight != null)
        {
            handleRight.localPosition = rightHandleLocalOffset;
            handleRight.localRotation = Quaternion.identity;
        }
    }

    void EnsureStandMeshRoot()
    {
        if (standMeshRoot != null)
            return;

        standMeshRoot = transform.Find("Support")
            ?? transform.Find("Suport")
            ?? transform.Find("Suporte")
            ?? transform.Find("Stand");

        if (standMeshRoot == null && transform.parent != null)
            standMeshRoot = transform.parent;
    }

    void EnsureRestAnchor()
    {
        if (restAnchor != null && restAnchor != transform)
            return;

        Transform searchRoot = standMeshRoot != null ? standMeshRoot : transform.parent ?? transform;
        Transform found = searchRoot.Find("RestAnchor");
        if (found != null)
            restAnchor = found;
    }

    void EnsureCarryOnGrab()
    {
        if (carryOnGrab != null && carryOnGrab.Length > 0)
            return;

        if (transform.parent == null)
            return;

        Transform screen = transform.parent.Find("Screen");
        if (screen != null && screen.parent == transform.parent && screen != transform)
            carryOnGrab = new[] { screen };
    }

    void AttachCarryRoots()
    {
        carryPoses.Clear();
        if (carryOnGrab == null)
            return;

        foreach (Transform carried in carryOnGrab)
        {
            if (carried == null || carried == transform || carried.IsChildOf(transform))
                continue;

            carryPoses.Add(new CarryPose
            {
                Target = carried,
                OriginalParent = carried.parent,
                LocalPosition = carried.localPosition,
                LocalRotation = carried.localRotation,
                LocalScale = carried.localScale,
            });
            carried.SetParent(transform, worldPositionStays: true);
        }
    }

    void RestoreCarryRoots()
    {
        for (int i = 0; i < carryPoses.Count; i++)
        {
            CarryPose pose = carryPoses[i];
            if (pose.Target == null || pose.OriginalParent == null)
                continue;

            pose.Target.SetParent(pose.OriginalParent, false);
            pose.Target.localPosition = pose.LocalPosition;
            pose.Target.localRotation = pose.LocalRotation;
            pose.Target.localScale = pose.LocalScale;
        }

        carryPoses.Clear();
    }

    void DetachFromStandForGrab()
    {
        AttachCarryRoots();

        if (standMeshRoot != null && transform.IsChildOf(standMeshRoot))
        {
            resolvedDockMount = standMeshRoot;
            transform.SetParent(null, worldPositionStays: true);
            deviceLiftedOffStand = true;
            Log($"device lifted off stand {standMeshRoot.name}");
            return;
        }

        DetachStandForGrab();
        transform.SetParent(null, worldPositionStays: true);
    }

    void CacheStandLocalPose()
    {
        if (standMeshRoot == null || !standMeshRoot.IsChildOf(transform))
            return;

        standLocalPosition = standMeshRoot.localPosition;
        standLocalRotation = standMeshRoot.localRotation;
        standLocalScale = standMeshRoot.localScale;
    }

    void DetachStandForGrab()
    {
        if (standMeshRoot == null || !standMeshRoot.IsChildOf(transform))
            return;

        CacheStandLocalPose();
        Transform standParent = resolvedDockMount != null ? resolvedDockMount : transform.parent;
        standMeshRoot.SetParent(standParent, worldPositionStays: true);
        standDetachedFromDevice = true;
        Log($"stand detached under {(standParent != null ? standParent.name : "scene root")}");
    }

    void ReattachStandToDevice()
    {
        if (!standDetachedFromDevice || standMeshRoot == null)
            return;

        standMeshRoot.SetParent(transform, false);
        standMeshRoot.localPosition = standLocalPosition;
        standMeshRoot.localRotation = standLocalRotation;
        standMeshRoot.localScale = standLocalScale;
        standDetachedFromDevice = false;
    }

    void FitHandlesFromMeshBounds()
    {
        Transform boundsRoot = handleMeshRoot != null ? handleMeshRoot : transform;
        if (handleMeshRoot == standMeshRoot)
            boundsRoot = transform;
        Renderer[] renderers = boundsRoot.GetComponentsInChildren<Renderer>(includeInactive: false);
        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        Vector3 localCenter = transform.InverseTransformPoint(bounds.center);
        Vector3 localExtents = transform.InverseTransformVector(bounds.extents);
        float sideX = Mathf.Max(0.02f, localExtents.x * 0.65f);
        float gripZ = Mathf.Max(0.01f, localExtents.z * 0.35f);

        leftHandleLocalOffset = localCenter + Vector3.left * sideX + Vector3.forward * gripZ;
        rightHandleLocalOffset = localCenter + Vector3.right * sideX + Vector3.forward * gripZ;
    }

    Rigidbody EnsureRigidbody()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.useGravity = false;
        rb.detectCollisions = true;
        rb.isKinematic = true;
        return rb;
    }

    void EnsureNotStatic()
    {
        if (gameObject.isStatic)
            gameObject.isStatic = false;
    }

    void SetDockedPhysics(bool docked)
    {
        if (body == null)
            return;

        body.useGravity = false;
        body.isKinematic = true;

        if (docked)
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }

    static void ForceEndHandleSelection(XRBaseInteractable interactable)
    {
        if (interactable == null || !interactable.isSelected || interactable.interactorsSelecting.Count == 0)
            return;

        XRInteractionManager manager = interactable.interactionManager;
        if (manager == null)
            manager = FindObjectOfType<XRInteractionManager>();
        if (manager == null)
            return;

        var interactors = new List<IXRSelectInteractor>(interactable.interactorsSelecting);
        foreach (IXRSelectInteractor interactor in interactors)
            manager.SelectExit(interactor, interactable);
    }

    IEnumerator HideHandVisualsNextFrame(bool isLeft, IXRSelectInteractor interactor)
    {
        yield return null;
        if ((isLeft && !leftHeld) || (!isLeft && !rightHeld))
            yield break;

        HideHandVisuals(isLeft, ResolveHandModel(interactor));
    }

    void HideHandVisuals(bool isLeft, Transform handRoot)
    {
        RestoreHandVisuals(isLeft);
        if (handRoot == null)
            return;

        List<Renderer> bucket = isLeft ? hiddenLeftRenderers : hiddenRightRenderers;
        foreach (Renderer renderer in handRoot.GetComponentsInChildren<Renderer>(includeInactive: true))
        {
            if (renderer == null || !renderer.enabled)
                continue;

            renderer.enabled = false;
            bucket.Add(renderer);
        }
    }

    void RestoreHandVisuals(bool isLeft)
    {
        List<Renderer> bucket = isLeft ? hiddenLeftRenderers : hiddenRightRenderers;
        for (int i = 0; i < bucket.Count; i++)
        {
            Renderer renderer = bucket[i];
            if (renderer != null)
                renderer.enabled = true;
        }

        bucket.Clear();
    }

    static Transform ResolveFollowTransform(IXRSelectInteractor interactor)
    {
        var interactorBehaviour = interactor as MonoBehaviour;
        return interactorBehaviour != null ? interactorBehaviour.transform : null;
    }

    static Transform ResolveHandModel(IXRSelectInteractor interactor)
    {
        var interactorBehaviour = interactor as MonoBehaviour;
        if (interactorBehaviour == null)
            return null;

        ActionBasedController controller = interactorBehaviour.GetComponentInParent<ActionBasedController>();
        if (controller != null && controller.model != null)
            return controller.model;

        ChangeControls controls = FindObjectOfType<ChangeControls>();
        if (controls == null)
            return null;

        Transform interactorTransform = interactorBehaviour.transform;
        if (controls.leftHandXRControl != null
            && interactorTransform.IsChildOf(controls.leftHandXRControl.transform))
            return controls.LeftHand != null ? controls.LeftHand.transform : null;

        if (controls.rightHandXRControl != null
            && interactorTransform.IsChildOf(controls.rightHandXRControl.transform))
            return controls.RightHand != null ? controls.RightHand.transform : null;

        return null;
    }

    void Log(string message)
    {
        if (!logDebug)
            return;

        ConfigManager.WriteConsole($"{LogPrefix} {message}");
    }

#if UNITY_EDITOR
    public bool IsEditorSimulationActive => editorSimActive;

    public void EditorToggleSimulation()
    {
        if (!Application.isEditor || !enableEditorSimulation)
            return;

        if (editorSimActive)
            EndEditorSimulation();
        else
            BeginEditorSimulation();
    }

    void HandleEditorSimulationInput()
    {
        if (MREditorInput.WasPressed(editorToggleGrabKey))
            EditorToggleSimulation();

        if (!editorSimActive)
            return;

        UpdateEditorSimulatedHands();
        ApplyEditorRotationTuning();
    }

    void BeginEditorSimulation()
    {
        EnsureEditorSimHandTransforms();
        leftHeld = true;
        rightHeld = true;
        leftFollow = editorSimLeft;
        rightFollow = editorSimRight;
        editorSimActive = true;
        UpdateDualHeldState();
        ConfigManager.WriteConsole(
            $"{LogPrefix} editor sim ON — hold offset {holdRotationOffsetEuler}");
    }

    void EndEditorSimulation()
    {
        editorSimActive = false;
        leftHeld = false;
        rightHeld = false;
        leftFollow = null;
        rightFollow = null;

        bool wasHeld = dualHeld;
        UpdateDualHeldState();
        if (wasHeld && !dualHeld)
            EndDualGrab();

        ConfigManager.WriteConsole($"{LogPrefix} editor sim OFF");
    }

    void EnsureEditorSimHandTransforms()
    {
        if (editorSimLeft == null)
        {
            var leftObject = new GameObject("EditorSimLeftHand");
            editorSimLeft = leftObject.transform;
        }

        if (editorSimRight == null)
        {
            var rightObject = new GameObject("EditorSimRightHand");
            editorSimRight = rightObject.transform;
        }
    }

    void UpdateEditorSimulatedHands()
    {
        Transform cameraTransform = ResolveEditorCameraTransform();
        if (cameraTransform == null)
            return;

        Vector3 center = cameraTransform.position + cameraTransform.forward * editorHandDistanceMeters;
        Vector3 halfSpan = cameraTransform.right * (editorHandSpanMeters * 0.5f);
        editorSimLeft.position = center - halfSpan;
        editorSimRight.position = center + halfSpan;
    }

    void ApplyEditorRotationTuning()
    {
        float step = MREditorInput.IsAnyHeld(KeyCode.LeftShift, KeyCode.RightShift)
            ? editorRotationStepDegrees * 5f
            : editorRotationStepDegrees;
        Vector3 euler = holdRotationOffsetEuler;
        bool changed = false;

        if (MREditorInput.WasPressed(KeyCode.PageUp))
        {
            euler.x += step;
            changed = true;
        }

        if (MREditorInput.WasPressed(KeyCode.PageDown))
        {
            euler.x -= step;
            changed = true;
        }

        if (MREditorInput.WasPressed(KeyCode.Home))
        {
            euler.y += step;
            changed = true;
        }

        if (MREditorInput.WasPressed(KeyCode.End))
        {
            euler.y -= step;
            changed = true;
        }

        if (MREditorInput.WasPressed(KeyCode.Insert))
        {
            euler.z += step;
            changed = true;
        }

        if (MREditorInput.WasPressed(KeyCode.Delete))
        {
            euler.z -= step;
            changed = true;
        }

        if (!changed)
            return;

        holdRotationOffsetEuler = euler;
        ConfigManager.WriteConsole($"{LogPrefix} holdRotationOffsetEuler = {euler}");
    }

    static Transform ResolveEditorCameraTransform()
    {
        if (Camera.main != null)
            return Camera.main.transform;

        Camera anyCamera = FindObjectOfType<Camera>();
        return anyCamera != null ? anyCamera.transform : null;
    }

    void OnDrawGizmosSelected()
    {
        if (!enableEditorSimulation)
            return;

        Gizmos.color = Color.cyan;
        Vector3 leftWorld = transform.TransformPoint(leftHandleLocalOffset);
        Vector3 rightWorld = transform.TransformPoint(rightHandleLocalOffset);
        Gizmos.DrawWireCube(leftWorld, handleColliderSize);
        Gizmos.DrawWireCube(rightWorld, handleColliderSize);
        Gizmos.DrawLine(leftWorld, rightWorld);

        if (!editorSimActive || editorSimLeft == null || editorSimRight == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(editorSimLeft.position, 0.02f);
        Gizmos.DrawSphere(editorSimRight.position, 0.02f);
        Gizmos.DrawLine(editorSimLeft.position, editorSimRight.position);
    }
#endif
}
