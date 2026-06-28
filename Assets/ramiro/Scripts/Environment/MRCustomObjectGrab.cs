/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>XR grab for custom MR objects — one hand or two hands (object.yaml <c>grab</c>).</summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(1000)]
public class MRCustomObjectGrab : MonoBehaviour
{
    const string LogPrefix = "[MRCustomObjectGrab]";
    const string HandleLeftName = "HandleLeft";
    const string HandleRightName = "HandleRight";
    static readonly string[] GrabInteractionLayers = { "InteractablePart" };
    const string GrabPhysicsLayerName = "InteractablePart";

    bool twoHands;
    bool returnOnRelease = true;
    bool hideHands = true;
    float returnDurationSeconds = 0.15f;
    Vector3 holdRotationOffsetEuler = new Vector3(-10f, 0f, 90f);
    float rotationSmoothing = 18f;
    Vector3 leftHandleLocalOffset = new Vector3(-0.055f, -0.01f, 0.03f);
    Vector3 rightHandleLocalOffset = new Vector3(0.055f, -0.01f, 0.03f);
    Vector3 handleColliderSize = new Vector3(0.045f, 0.055f, 0.07f);

    Transform grabRoot;
    Rigidbody body;
    XRGrabInteractable oneHandGrab;
    XRSimpleInteractable leftHandle;
    XRSimpleInteractable rightHandle;
    Transform handleLeft;
    Transform handleRight;

    bool leftHeld;
    bool rightHeld;
    bool dualHeld;
    Transform leftFollow;
    Transform rightFollow;
    Vector3 lockedWorldScale;
    Vector3 lastStableForward = Vector3.forward;
    Vector3 lastStableRightDir = Vector3.right;

    Vector3 homeWorldPosition;
    Quaternion homeWorldRotation;
    Vector3 homeLocalScale;
    Coroutine returnHomeCoroutine;

    readonly List<Renderer> hiddenLeftRenderers = new List<Renderer>();
    readonly List<Renderer> hiddenRightRenderers = new List<Renderer>();

    public bool IsHeld => twoHands ? dualHeld : oneHandGrab != null && oneHandGrab.isSelected;

    public void Configure(MRCustomObjectGrabYaml config, Transform searchRoot)
    {
        if (config == null)
            return;

        twoHands = config.TwoHands;
        returnOnRelease = config.ReturnOnRelease;
        hideHands = config.HideHands;
        if (config.ReturnDurationSeconds >= 0f)
            returnDurationSeconds = config.ReturnDurationSeconds;

        grabRoot = ResolveGrabRoot(searchRoot, config.Target);
        if (grabRoot == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} grab root not found on {searchRoot?.name}");
            return;
        }

        EnsureNotStatic(grabRoot);
        body = EnsureRigidbody(grabRoot);

        if (twoHands)
            SetupTwoHands();
        else
            SetupOneHand();

        CaptureHomePose();
        SetDockedPhysics(true);
        ConfigManager.WriteConsole(
            $"{LogPrefix} configured on '{grabRoot.name}' twoHands={twoHands} returnOnRelease={returnOnRelease}");
    }

    void Start() => CaptureHomePose();

    void LateUpdate()
    {
        if (twoHands && dualHeld)
            FollowTwoHands();
        else if (twoHands)
            SnapHandlesToOffsets();
    }

    void OnDestroy()
    {
        if (oneHandGrab != null)
        {
            oneHandGrab.selectEntered.RemoveAllListeners();
            oneHandGrab.selectExited.RemoveAllListeners();
        }

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
        if (IsHeld)
            return;

        CaptureHomePose();
    }

    void SetupOneHand()
    {
        oneHandGrab = grabRoot.GetComponent<XRGrabInteractable>();
        if (oneHandGrab == null)
            oneHandGrab = grabRoot.gameObject.AddComponent<XRGrabInteractable>();

        oneHandGrab.throwOnDetach = false;
        oneHandGrab.movementType = XRBaseInteractable.MovementType.Instantaneous;
        oneHandGrab.trackPosition = true;
        oneHandGrab.trackRotation = true;
        oneHandGrab.trackScale = false;
        oneHandGrab.retainTransformParent = false;

        int physicsLayer = LayerMask.NameToLayer(GrabPhysicsLayerName);
        if (physicsLayer >= 0)
            grabRoot.gameObject.layer = physicsLayer;

        oneHandGrab.interactionLayers = InteractionLayerMask.GetMask(GrabInteractionLayers);
        RebuildGrabColliders(oneHandGrab);

        oneHandGrab.selectEntered.AddListener(OnOneHandGrabbed);
        oneHandGrab.selectExited.AddListener(OnOneHandReleased);
    }

    void SetupTwoHands()
    {
        handleLeft = EnsureHandle(HandleLeftName, leftHandleLocalOffset, out leftHandle);
        handleRight = EnsureHandle(HandleRightName, rightHandleLocalOffset, out rightHandle);

        leftHandle.selectEntered.AddListener(args => OnHandleGrabbed(true, args));
        leftHandle.selectExited.AddListener(_ => OnHandleReleased(true));
        rightHandle.selectEntered.AddListener(args => OnHandleGrabbed(false, args));
        rightHandle.selectExited.AddListener(_ => OnHandleReleased(false));
    }

    void OnOneHandGrabbed(SelectEnterEventArgs args)
    {
        if (returnHomeCoroutine != null)
        {
            StopCoroutine(returnHomeCoroutine);
            returnHomeCoroutine = null;
        }

        lockedWorldScale = grabRoot.lossyScale;
        if (body != null)
        {
            body.isKinematic = true;
            body.WakeUp();
        }

        if (hideHands)
            StartCoroutine(HideHandVisualsNextFrame(args.interactorObject, true));
    }

    void OnOneHandReleased(SelectExitEventArgs _)
    {
        PayphoneHandsetGrab.ForceShowPlayerHands();
        if (returnOnRelease)
            ReturnHome();
    }

    void OnHandleGrabbed(bool isLeft, SelectEnterEventArgs args)
    {
        if (isLeft)
        {
            leftHeld = true;
            leftFollow = ResolveFollowTransform(args.interactorObject);
            if (hideHands)
                StartCoroutine(HideHandVisualsNextFrame(args.interactorObject, isLeft));
        }
        else
        {
            rightHeld = true;
            rightFollow = ResolveFollowTransform(args.interactorObject);
            if (hideHands)
                StartCoroutine(HideHandVisualsNextFrame(args.interactorObject, isLeft));
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

        bool wasDual = dualHeld;
        UpdateDualHeldState();

        if (wasDual && !dualHeld)
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
        // Skip re-capturing the dock pose during a quick release+re-grab while still returning,
        // otherwise "home" gets locked to a mid-air spot and the object never returns to base.
        bool stillReturning = returnHomeCoroutine != null;
        if (returnHomeCoroutine != null)
        {
            StopCoroutine(returnHomeCoroutine);
            returnHomeCoroutine = null;
        }

        if (!stillReturning)
            CaptureHomePose();
        lockedWorldScale = grabRoot.lossyScale;
        lastStableForward = grabRoot.forward;
        lastStableRightDir = grabRoot.right;

        if (body != null)
        {
            body.isKinematic = true;
            body.WakeUp();
        }

        grabRoot.SetParent(null, worldPositionStays: true);
    }

    void EndDualGrab()
    {
        ForceEndHandleSelection(leftHandle);
        ForceEndHandleSelection(rightHandle);
        PayphoneHandsetGrab.ForceShowPlayerHands();
        RestoreHandVisuals(true);
        RestoreHandVisuals(false);

        if (body != null)
            body.isKinematic = true;

        if (returnOnRelease)
            ReturnHome();
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
        grabRoot.SetPositionAndRotation(homeWorldPosition, homeWorldRotation);
        grabRoot.localScale = homeLocalScale;
        SetDockedPhysics(true);
    }

    IEnumerator ReturnHomeSmooth()
    {
        Vector3 startPos = grabRoot.position;
        Quaternion startRot = grabRoot.rotation;
        float elapsed = 0f;

        while (elapsed < returnDurationSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / returnDurationSeconds);
            grabRoot.SetPositionAndRotation(
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
        if (!TryComputeHeldRotation(leftFollow.position, rightFollow.position, out Quaternion targetRot))
            return;

        Quaternion rot = targetRot;
        if (rotationSmoothing > 0f)
        {
            float t = 1f - Mathf.Exp(-rotationSmoothing * Time.deltaTime);
            rot = Quaternion.Slerp(grabRoot.rotation, targetRot, t);
        }

        grabRoot.SetPositionAndRotation(mid, rot);
        ApplyLockedWorldScale();
    }

    bool TryComputeHeldRotation(Vector3 leftPos, Vector3 rightPos, out Quaternion rotation)
    {
        rotation = grabRoot.rotation;

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
            forward = lastStableForward.sqrMagnitude > 0.0001f ? lastStableForward : grabRoot.forward;
        else
        {
            forward.Normalize();
            lastStableForward = forward;
        }

        rotation = Quaternion.LookRotation(forward, up) * Quaternion.Euler(holdRotationOffsetEuler);
        return true;
    }

    void ApplyLockedWorldScale()
    {
        if (grabRoot.parent == null)
        {
            grabRoot.localScale = lockedWorldScale;
            return;
        }

        Vector3 parentScale = grabRoot.parent.lossyScale;
        grabRoot.localScale = new Vector3(
            lockedWorldScale.x / Mathf.Max(0.0001f, parentScale.x),
            lockedWorldScale.y / Mathf.Max(0.0001f, parentScale.y),
            lockedWorldScale.z / Mathf.Max(0.0001f, parentScale.z));
    }

    void CaptureHomePose()
    {
        homeWorldPosition = grabRoot.position;
        homeWorldRotation = grabRoot.rotation;
        homeLocalScale = grabRoot.localScale;
    }

    void SetDockedPhysics(bool docked)
    {
        if (body == null)
            return;

        body.isKinematic = docked;
        body.useGravity = false;
    }

    static Transform ResolveGrabRoot(Transform searchRoot, string targetName)
    {
        if (searchRoot == null)
            return null;

        if (!string.IsNullOrEmpty(targetName))
        {
            Transform named = FindChildByName(searchRoot, targetName);
            if (named != null)
                return named;
        }

        if (searchRoot.GetComponent<Rigidbody>() != null)
            return searchRoot;

        Rigidbody rb = searchRoot.GetComponentInChildren<Rigidbody>(true);
        return rb != null ? rb.transform : searchRoot;
    }

    static Transform FindChildByName(Transform root, string childName)
    {
        if (root == null || string.IsNullOrEmpty(childName))
            return null;

        if (string.Equals(root.name, childName, System.StringComparison.OrdinalIgnoreCase))
            return root;

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child != root && string.Equals(child.name, childName, System.StringComparison.OrdinalIgnoreCase))
                return child;
        }

        return null;
    }

    static void EnsureNotStatic(Transform root)
    {
        if (root == null)
            return;

        if (root.gameObject.isStatic)
            root.gameObject.isStatic = false;

        for (int i = 0; i < root.childCount; i++)
            EnsureNotStatic(root.GetChild(i));
    }

    static Rigidbody EnsureRigidbody(Transform root)
    {
        Rigidbody rb = root.GetComponent<Rigidbody>();
        if (rb == null)
            rb = root.gameObject.AddComponent<Rigidbody>();

        rb.useGravity = false;
        return rb;
    }

    void RebuildGrabColliders(XRGrabInteractable grab)
    {
        grab.colliders.Clear();

        Collider col = grabRoot.GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.enabled = true;
            grab.colliders.Add(col);
        }

        if (grab.colliders.Count == 0)
        {
            foreach (Collider childCol in grabRoot.GetComponentsInChildren<Collider>(true))
            {
                if (childCol != null && !childCol.isTrigger)
                    grab.colliders.Add(childCol);
            }
        }

        if (grab.colliders.Count == 0)
        {
            BoxCollider box = grabRoot.gameObject.AddComponent<BoxCollider>();
            FitBoxColliderToRenderers(box, grabRoot);
            grab.colliders.Add(box);
        }
    }

    static void FitBoxColliderToRenderers(BoxCollider box, Transform root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(false);
        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        box.center = root.InverseTransformPoint(bounds.center);
        Vector3 lossy = root.lossyScale;
        box.size = new Vector3(
            bounds.size.x / Mathf.Max(0.0001f, lossy.x),
            bounds.size.y / Mathf.Max(0.0001f, lossy.y),
            bounds.size.z / Mathf.Max(0.0001f, lossy.z));
    }

    Transform EnsureHandle(string handleName, Vector3 localOffset, out XRSimpleInteractable handleInteractable)
    {
        Transform existing = grabRoot.Find(handleName);
        GameObject handleObject = existing != null ? existing.gameObject : new GameObject(handleName);
        if (existing == null)
            handleObject.transform.SetParent(grabRoot, false);

        handleObject.transform.localPosition = localOffset;
        handleObject.transform.localRotation = Quaternion.identity;
        handleObject.transform.localScale = Vector3.one;

        int physicsLayer = LayerMask.NameToLayer(GrabPhysicsLayerName);
        if (physicsLayer >= 0)
            handleObject.layer = physicsLayer;

        XRGrabInteractable legacy = handleObject.GetComponent<XRGrabInteractable>();
        if (legacy != null)
            Destroy(legacy);

        Rigidbody handleBody = handleObject.GetComponent<Rigidbody>();
        if (handleBody != null)
            Destroy(handleBody);

        BoxCollider box = handleObject.GetComponent<BoxCollider>();
        if (box == null)
            box = handleObject.AddComponent<BoxCollider>();
        box.isTrigger = false;
        box.size = handleColliderSize;

        handleInteractable = handleObject.GetComponent<XRSimpleInteractable>();
        if (handleInteractable == null)
            handleInteractable = handleObject.AddComponent<XRSimpleInteractable>();

        handleInteractable.interactionLayers = InteractionLayerMask.GetMask(GrabInteractionLayers);
        handleInteractable.colliders.Clear();
        handleInteractable.colliders.Add(box);

        return handleObject.transform;
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

    static void ForceEndHandleSelection(XRSimpleInteractable handle)
    {
        if (handle == null || !handle.isSelected)
            return;

        handle.interactionManager?.SelectExit(handle.firstInteractorSelecting, handle);
    }

    static Transform ResolveFollowTransform(IXRSelectInteractor interactor)
    {
        if (interactor == null)
            return null;

        Transform attach = interactor.GetAttachTransform(null);
        return attach != null ? attach : (interactor as Component)?.transform;
    }

    IEnumerator HideHandVisualsNextFrame(IXRSelectInteractor interactor, bool isLeft)
    {
        yield return null;
        GameObject handModel = ResolveHandModel(interactor);
        if (handModel == null)
            yield break;

        List<Renderer> list = isLeft ? hiddenLeftRenderers : hiddenRightRenderers;
        list.Clear();
        foreach (Renderer renderer in handModel.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.enabled)
            {
                renderer.enabled = false;
                list.Add(renderer);
            }
        }
    }

    void RestoreHandVisuals(bool isLeft)
    {
        List<Renderer> list = isLeft ? hiddenLeftRenderers : hiddenRightRenderers;
        foreach (Renderer renderer in list)
        {
            if (renderer != null)
                renderer.enabled = true;
        }

        list.Clear();
    }

    static GameObject ResolveHandModel(IXRSelectInteractor interactor)
    {
        Component component = interactor as Component;
        if (component == null)
            return null;

        Transform node = component.transform;
        while (node != null)
        {
            if (node.name.Contains("hand", System.StringComparison.OrdinalIgnoreCase)
                || node.name.Contains("controller", System.StringComparison.OrdinalIgnoreCase))
                return node.gameObject;

            node = node.parent;
        }

        return component.transform.root.gameObject;
    }
}
