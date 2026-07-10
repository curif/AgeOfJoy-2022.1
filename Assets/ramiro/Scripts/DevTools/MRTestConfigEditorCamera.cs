/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.SceneManagement;
#endif

/// <summary>
/// TestConfig editor fly camera — aim the placement ray with the mouse and walk around the MRUK room.
/// </summary>
public class MRTestConfigEditorCamera : MonoBehaviour
{
#if UNITY_EDITOR
    const string LogPrefix = "[MRTestConfigEditorCamera]";

    [SerializeField] float moveSpeedMeters = 2.5f;
    [SerializeField] float fastMoveMultiplier = 2.5f;
    [SerializeField] float lookSensitivity = 2.2f;
    [SerializeField] float minPitch = -80f;
    [SerializeField] float maxPitch = 80f;

    float yaw;
    float pitch;
    bool loggedControls;

    public static void EnsureOnMainCamera()
    {
        if (!Application.isEditor)
            return;
        if (SceneManager.GetActiveScene().name != MRTestConfigSceneLoader.TestSceneName)
            return;

        Camera cam = Camera.main;
        if (cam == null)
            return;
        if (cam.GetComponent<MRTestConfigEditorCamera>() != null)
            return;

        cam.gameObject.AddComponent<MRTestConfigEditorCamera>();
    }

    void Start()
    {
        Vector3 euler = transform.eulerAngles;
        yaw = euler.y;
        pitch = euler.x;
        if (pitch > 180f)
            pitch -= 360f;
    }

    void Update()
    {
        if (!Application.isEditor || !enabled)
            return;

        if (!loggedControls)
        {
            loggedControls = true;
            ConfigManager.WriteConsole($"{LogPrefix} RMB+mouse=look | WASD=move | Shift=fast | M=CRT menu");
            ConfigManager.WriteConsole($"{LogPrefix} placement ray: mouse=aim | RMB=confirm | Esc=cancel");
        }

        if (MRConfigurationCabinetController.Instance != null
            && MRConfigurationCabinetController.Instance.IsUsingCabinetCamera)
            return;

        if (MRPlacementRayController.AnyActive)
            return;

        HandleLook();
        HandleMove();
    }

    void HandleLook()
    {
        if (!MREditorInput.IsMouseRightHeld())
            return;

        Vector2 delta = MREditorInput.ReadMouseDelta();
        yaw += delta.x * lookSensitivity * 0.1f;
        pitch -= delta.y * lookSensitivity * 0.1f;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleMove()
    {
        float speed = moveSpeedMeters;
        if (MREditorInput.IsAnyHeld(KeyCode.LeftShift, KeyCode.RightShift))
            speed *= fastMoveMultiplier;

        Vector3 move = Vector3.zero;
        if (MREditorInput.IsHeld(KeyCode.W))
            move += transform.forward;
        if (MREditorInput.IsHeld(KeyCode.S))
            move -= transform.forward;
        if (MREditorInput.IsHeld(KeyCode.A))
            move -= transform.right;
        if (MREditorInput.IsHeld(KeyCode.D))
            move += transform.right;
        if (MREditorInput.IsHeld(KeyCode.E))
            move += Vector3.up;
        if (MREditorInput.IsHeld(KeyCode.Q))
            move -= Vector3.up;

        if (move.sqrMagnitude < 0.001f)
            return;

        transform.position += move.normalized * speed * Time.deltaTime;
    }
#else
    public static void EnsureOnMainCamera() { }
#endif
}
