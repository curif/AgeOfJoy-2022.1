#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor-only wireframe for ZoneBlackout on PF_Payphone.
/// </summary>
[InitializeOnLoad]
static class MRPhoneBoothHeadFadeGizmoDrawer
{
    const string PayphoneObjectName = "PF_Payphone";

    static readonly Color ZoneColor = new Color(0.25f, 0.95f, 0.4f, 0.95f);
    static readonly Color EyeSafeColor = new Color(0.25f, 0.95f, 0.4f, 0.95f);
    static readonly Color EyeFadeColor = new Color(0.95f, 0.3f, 0.25f, 0.95f);

    static MRPhoneBoothHeadFadeGizmoDrawer()
    {
        SceneView.duringSceneGui += OnSceneGui;
    }

    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Active)]
    static void DrawPortalGizmo(MRPhoneBoothPortal portal, GizmoType gizmoType)
    {
        if (portal == null)
            return;

        DrawZoneBlackout(portal.transform, portal.GetComponent<MRPhoneBoothTravelHeadFade>(), drawWithHandles: false);
    }

    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Active)]
    static void DrawHeadFadeGizmo(MRPhoneBoothTravelHeadFade headFade, GizmoType gizmoType)
    {
        if (headFade == null)
            return;

        DrawZoneBlackout(headFade.transform, headFade, drawWithHandles: false);
    }

    static void OnSceneGui(SceneView view)
    {
        if (SceneManager.GetActiveScene().name != TestPhoneBoothHeadFadeBootstrap.TestSceneName)
            return;

        GameObject booth = GameObject.Find(PayphoneObjectName);
        if (booth == null)
            return;

        if (booth.GetComponent<MRPhoneBoothPortal>() != null
            || booth.GetComponent<MRPhoneBoothTravelHeadFade>() != null)
            return;

        DrawZoneBlackout(booth.transform, booth.GetComponent<MRPhoneBoothTravelHeadFade>(), drawWithHandles: true);
    }

    static void DrawZoneBlackout(Transform boothRoot, MRPhoneBoothTravelHeadFade headFade, bool drawWithHandles)
    {
        if (boothRoot == null)
            return;

        BoxCollider zone = MRPhoneBoothPortal.FindZoneBlackoutOn(boothRoot);
        if (zone == null)
            return;

        Matrix4x4 zoneMatrix = zone.transform.localToWorldMatrix;
        Vector3 center = zone.center;
        Vector3 size = zone.size;

        if (drawWithHandles)
        {
            using (new Handles.DrawingScope(zoneMatrix))
            {
                Handles.color = ZoneColor;
                Handles.DrawWireCube(center, size);
            }
        }
        else
        {
            Matrix4x4 previous = Gizmos.matrix;
            Gizmos.matrix = zoneMatrix;
            Gizmos.color = ZoneColor;
            Gizmos.DrawWireCube(center, size);
            Gizmos.matrix = previous;
        }

        if (Application.isPlaying && headFade != null && headFade.IsMonitoring)
            DrawPlayModeEye(headFade, drawWithHandles);

        if (!drawWithHandles)
            return;

        if (Selection.activeGameObject == zone.transform.gameObject
            || (Selection.activeTransform != null && Selection.activeTransform.IsChildOf(zone.transform)))
        {
            Vector3 labelPos = zoneMatrix.MultiplyPoint3x4(
                center + new Vector3(0f, size.y * 0.5f + 0.12f, 0f));
            Handles.Label(labelPos, "ZoneBlackout — icon visible inside, black outside");
        }
    }

    static void DrawPlayModeEye(MRPhoneBoothTravelHeadFade headFade, bool drawWithHandles)
    {
        Camera eyeCamera = Camera.main;
        if (eyeCamera == null)
            return;

        Vector3 eye = eyeCamera.transform.position;
        float fade = headFade.EvaluateFadeAtEyePosition(eye);
        Color color = Color.Lerp(EyeSafeColor, EyeFadeColor, fade);

        if (drawWithHandles)
        {
            Handles.color = color;
            Handles.SphereHandleCap(0, eye, Quaternion.identity, 0.09f, EventType.Repaint);
            return;
        }

        Gizmos.color = color;
        Gizmos.DrawSphere(eye, 0.045f);
    }
}
#endif
