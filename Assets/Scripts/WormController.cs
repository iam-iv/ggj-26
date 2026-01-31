using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls top-down movement for the worm character.
/// Movement is relative to a 45-degree rotated orthographic camera.
/// Uses Rigidbody for physics-safe movement.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class WormController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [Tooltip("Optional reference to PaperTurnController to auto-animate turns")]
    [SerializeField] private PaperTurnController paperTurnController;
    [Tooltip("Minimum horizontal input magnitude to trigger a facing change")]
    [SerializeField] private float facingThreshold = 0.1f;
    [Tooltip("If true, will call PaperTurnController when horizontal input changes sign")]
    [SerializeField] private bool autoFlip = true;

    private CharacterController controller;
    private Vector3 inputDirection;
    private float verticalVelocity = 0f;
    [Header("Physics")]
    [SerializeField] private float gravity = -9.81f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (paperTurnController == null)
            paperTurnController = GetComponent<PaperTurnController>() ?? GetComponentInChildren<PaperTurnController>();
    }
    void Update()
    {
        // Get raw input (WASD / Arrow keys / Gamepad)
        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current != null)
        {
            horizontal = (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed ? 1f : 0f)
                       - (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed ? 1f : 0f);

            vertical = (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed ? 1f : 0f)
                     - (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed ? 1f : 0f);
        }

        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            // prefer gamepad stick when it has input
            if (stick.sqrMagnitude > 0.001f)
            {
                horizontal = stick.x;
                vertical = stick.y;
            }
        }

        // Auto flip the paper-style sprite when input indicates a facing change
        if (autoFlip && paperTurnController != null)
        {
            if (horizontal > facingThreshold)
                paperTurnController.SetFacing(true);
            else if (horizontal < -facingThreshold)
                paperTurnController.SetFacing(false);
        }

        // Create input vector
        Vector3 input = new Vector3(horizontal, 0f, vertical);

        // Clamp magnitude to avoid diagonal speed boost
        input = Vector3.ClampMagnitude(input, 1f);

        // Convert input from camera space to world space
        // Camera forward/right define "what feels like up/down/left/right"
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        // Remove vertical influence (we only want XZ movement)
        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        // Final movement direction relative to camera
        inputDirection = camForward * input.z + camRight * input.x;

        // Movement + gravity
        Vector3 move = inputDirection * moveSpeed;

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -1f;

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }
}
