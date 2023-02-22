using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadController : MonoBehaviour
{
    public float moveSpeed = 10.0f;
    public float rotateSpeed = 90.0f * 3f; //degrees per second
    public bool canMove = true;

    private Vector2 moveInput;
    private Vector2 rotateInput;

    private void OnMove(InputValue value)
    {
      ConfigManager.WriteConsole($"[GamepadController] OnMove");
      moveInput = value.Get<Vector2>();
    }

    private void OnRotate(InputValue value)
    {
      ConfigManager.WriteConsole($"[GamepadController] OnLook");

      rotateInput = value.Get<Vector2>();
    }

    void Update()
    {
        if (canMove)
        {
          // Move the player based on the input for movement
          transform.Translate(new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed * Time.deltaTime);

          // Rotate the player based on the input for rotation
          if (rotateInput.magnitude > 0)
          {
            float rotateX = rotateInput.x;
            float rotateY = rotateInput.y;
            transform.Rotate(Vector3.up, rotateX * rotateSpeed * Time.deltaTime);
            transform.Rotate(Vector3.right, -rotateY * rotateSpeed * Time.deltaTime);
          }
        }
    }
}


