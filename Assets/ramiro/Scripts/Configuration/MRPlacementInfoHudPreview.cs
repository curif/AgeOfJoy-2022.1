/*
This program is free software: you can redistribute it and/or modify it under
the terms of the GNU General Public License as published by the Free Software
Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>
/// Drop on a GameObject in UIInformation scene to preview the placement terminal HUD in Play mode.
/// Assign <see cref="handAnchor"/> and tweak <see cref="hudLocalOffset"/> in the Inspector (live in Play).
/// </summary>
public class MRPlacementInfoHudPreview : MonoBehaviour
{
    [SerializeField] bool showOnPlay = true;
    [SerializeField] bool destroyHudOnDisable = true;
    [Tooltip("Right-hand reference (drag AGEOfJoyHandRightPrefab or hands:b_r_hand).")]
    [SerializeField] Transform handAnchor;

    [Header("HUD position (local to hand)")]
    [Tooltip("X = along hand right, Y = along hand up, Z = along hand forward. Change while Playing.")]
    [SerializeField] Vector3 hudLocalOffset = new Vector3(-0.13f, -0.03f, -0.04f);

    void Start()
    {
        if (!showOnPlay)
            return;

        ApplyToHud(show: true);
    }

    void LateUpdate()
    {
        if (!Application.isPlaying || !showOnPlay)
            return;

        ApplyToHud(show: false);
    }

    void ApplyToHud(bool show)
    {
        if (handAnchor == null)
            handAnchor = FindHandReference();

        MRPlacementInfoHUD hud = MRPlacementInfoHUD.Ensure();
        if (handAnchor != null)
            hud.SetAnchorOverride(handAnchor);
        else
            hud.ClearAnchorOverride();

        hud.SetLocalOffsetFromController(hudLocalOffset);

        if (show)
            hud.ShowDemoPreview();
    }

    void OnDisable()
    {
        if (MRPlacementInfoHUD.Instance == null)
            return;

        MRPlacementInfoHUD.Instance.ClearAnchorOverride();
        if (destroyHudOnDisable)
            MRPlacementInfoHUD.Instance.Hide();
    }

    static Transform FindHandReference()
    {
        GameObject prefabRoot = GameObject.Find("AGEOfJoyHandRightPrefab (1)");
        if (prefabRoot == null)
            prefabRoot = GameObject.Find("AGEOfJoyHandRightPrefab");
        if (prefabRoot != null)
        {
            Transform grip = FindDeepChild(prefabRoot.transform, "hands:b_r_hand");
            if (grip != null)
                return grip;
            grip = FindDeepChild(prefabRoot.transform, "hands:b_r_grip");
            if (grip != null)
                return grip;
            grip = FindDeepChild(prefabRoot.transform, "hands:Rhand");
            if (grip != null)
                return grip;
            return prefabRoot.transform;
        }

        GameObject hand = GameObject.Find("hands:b_r_hand");
        return hand != null ? hand.transform : null;
    }

    static Transform FindDeepChild(Transform root, string exactName)
    {
        if (root == null || string.IsNullOrEmpty(exactName))
            return null;

        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == exactName)
                return t;
        }

        return null;
    }
}
