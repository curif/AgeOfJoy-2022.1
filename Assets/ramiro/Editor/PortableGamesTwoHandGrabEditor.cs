/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PortableGamesTwoHandGrab))]
public class PortableGamesTwoHandGrabEditor : Editor
{
    SerializedProperty hideHandsOnGrab;
    SerializedProperty restAnchor;
    SerializedProperty standMeshRoot;
    SerializedProperty dockMountRoot;
    SerializedProperty handleMeshRoot;
    SerializedProperty returnDurationSeconds;
    SerializedProperty logDebug;
    SerializedProperty autoPlaceHandlesFromMesh;
    SerializedProperty leftHandleLocalOffset;
    SerializedProperty rightHandleLocalOffset;
    SerializedProperty handleColliderSize;
    SerializedProperty holdRotationOffsetEuler;
    SerializedProperty rotationSmoothing;
    SerializedProperty enableEditorSimulation;
    SerializedProperty editorToggleGrabKey;
    SerializedProperty editorHandDistanceMeters;
    SerializedProperty editorHandSpanMeters;
    SerializedProperty editorRotationStepDegrees;
    SerializedProperty logEditorControlsOnStart;

    void OnEnable()
    {
        hideHandsOnGrab = serializedObject.FindProperty("hideHandsOnGrab");
        restAnchor = serializedObject.FindProperty("restAnchor");
        standMeshRoot = serializedObject.FindProperty("standMeshRoot");
        dockMountRoot = serializedObject.FindProperty("dockMountRoot");
        handleMeshRoot = serializedObject.FindProperty("handleMeshRoot");
        returnDurationSeconds = serializedObject.FindProperty("returnDurationSeconds");
        logDebug = serializedObject.FindProperty("logDebug");
        autoPlaceHandlesFromMesh = serializedObject.FindProperty("autoPlaceHandlesFromMesh");
        leftHandleLocalOffset = serializedObject.FindProperty("leftHandleLocalOffset");
        rightHandleLocalOffset = serializedObject.FindProperty("rightHandleLocalOffset");
        handleColliderSize = serializedObject.FindProperty("handleColliderSize");
        holdRotationOffsetEuler = serializedObject.FindProperty("holdRotationOffsetEuler");
        rotationSmoothing = serializedObject.FindProperty("rotationSmoothing");
        enableEditorSimulation = serializedObject.FindProperty("enableEditorSimulation");
        editorToggleGrabKey = serializedObject.FindProperty("editorToggleGrabKey");
        editorHandDistanceMeters = serializedObject.FindProperty("editorHandDistanceMeters");
        editorHandSpanMeters = serializedObject.FindProperty("editorHandSpanMeters");
        editorRotationStepDegrees = serializedObject.FindProperty("editorRotationStepDegrees");
        logEditorControlsOnStart = serializedObject.FindProperty("logEditorControlsOnStart");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(hideHandsOnGrab);
        EditorGUILayout.PropertyField(restAnchor);
        EditorGUILayout.PropertyField(standMeshRoot);
        EditorGUILayout.PropertyField(dockMountRoot);
        EditorGUILayout.PropertyField(handleMeshRoot);
        EditorGUILayout.HelpBox(
            "Stand setup:\n" +
            "• Stand Mesh Root = suporte (fica na mesa ao pegar; auto: Support/Suport/Suporte/Stand)\n" +
            "• Rest Anchor = empty onde o device encaixa\n" +
            "• Dock Mount Root = root do prefab MR (pai do device), se existir\n" +
            "• Handle Mesh Root = mesh do handheld apenas",
            MessageType.Info);
        EditorGUILayout.PropertyField(returnDurationSeconds);
        EditorGUILayout.PropertyField(logDebug);
        EditorGUILayout.PropertyField(autoPlaceHandlesFromMesh);
        EditorGUILayout.PropertyField(leftHandleLocalOffset);
        EditorGUILayout.PropertyField(rightHandleLocalOffset);
        EditorGUILayout.PropertyField(handleColliderSize);
        EditorGUILayout.PropertyField(holdRotationOffsetEuler);
        EditorGUILayout.PropertyField(rotationSmoothing);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Editor Simulation", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Play Mode: press G to simulate two-handed grab in front of the camera.\n" +
            "Tune rotation live:\n" +
            "  PgUp / PgDn = X\n" +
            "  Home / End = Y\n" +
            "  Insert / Delete = Z\n" +
            "Hold Shift for 5° steps. Copy Hold Rotation Offset Euler before stopping Play.",
            MessageType.Info);

        EditorGUILayout.PropertyField(enableEditorSimulation);
        EditorGUILayout.PropertyField(editorToggleGrabKey);
        EditorGUILayout.PropertyField(editorHandDistanceMeters);
        EditorGUILayout.PropertyField(editorHandSpanMeters);
        EditorGUILayout.PropertyField(editorRotationStepDegrees);
        EditorGUILayout.PropertyField(logEditorControlsOnStart);

        var grab = (PortableGamesTwoHandGrab)target;
        if (Application.isPlaying && grab != null && enableEditorSimulation.boolValue)
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Live Tuning (Play Mode)", EditorStyles.boldLabel);

            Vector3 euler = holdRotationOffsetEuler.vector3Value;
            EditorGUI.BeginChangeCheck();
            euler.x = EditorGUILayout.Slider("Hold Offset X", euler.x, -180f, 180f);
            euler.y = EditorGUILayout.Slider("Hold Offset Y", euler.y, -180f, 180f);
            euler.z = EditorGUILayout.Slider("Hold Offset Z", euler.z, -180f, 180f);
            if (EditorGUI.EndChangeCheck())
                holdRotationOffsetEuler.vector3Value = euler;

            string simLabel = grab.IsEditorSimulationActive
                ? "Stop Simulated Grab (G)"
                : "Start Simulated Grab (G)";
            if (GUILayout.Button(simLabel, GUILayout.Height(28f)))
                grab.EditorToggleSimulation();

            EditorGUILayout.LabelField(
                grab.IsEditorSimulationActive ? "Simulation: ON" : "Simulation: OFF",
                EditorStyles.miniLabel);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
