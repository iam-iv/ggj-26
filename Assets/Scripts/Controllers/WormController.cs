using UnityEngine;
using UnityEngine.InputSystem;


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
    [Header("Animation")]
    [Tooltip("Animator used for player animations (optional).")]
    [SerializeField] private Animator animator;
    [Tooltip("Name of the float parameter used to drive Idle/Walk (default: Speed)")]
    [SerializeField] private string animatorSpeedParam = "Speed";
    [Tooltip("Optional SpriteRenderer if you want to flip sprite here (PaperTurnController may handle facing)")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Header("Audio")]
    [Tooltip("AudioSource used to play footstep sounds (optional).")]
    [SerializeField] private AudioSource footstepSource;
    [Tooltip("Footstep clips to randomize between.")]
    [SerializeField] private AudioClip[] footstepClips;
    [Tooltip("Footsteps per second at full input magnitude (1.0)")]
    [SerializeField] private float stepsPerSecondAtFullSpeed = 2f;
    [Tooltip("Minimum input magnitude to trigger footsteps")]
    [SerializeField] private float minInputThresholdForSteps = 0.1f;
    private float footstepTimer = 0f;
    [Header("Physics")]
    [SerializeField] private float gravity = -9.81f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (paperTurnController == null)
            paperTurnController = GetComponent<PaperTurnController>() ?? GetComponentInChildren<PaperTurnController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (footstepSource == null)
            footstepSource = GetComponent<AudioSource>() ?? GetComponentInChildren<AudioSource>();
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

        // Update animator parameter and footstep playback based on input magnitude (works with CharacterController)
        float inputMag = new Vector2(input.x, input.z).magnitude; // 0..1
        if (animator != null)
        {
            float speedValue = inputMag * moveSpeed;
            animator.SetFloat(animatorSpeedParam, speedValue);
        }

        // Footstep playback: decrease timer when moving and grounded, play random clip when elapsing
        if (footstepSource != null && footstepClips != null && footstepClips.Length > 0 && controller.isGrounded)
        {
            if (inputMag > minInputThresholdForSteps)
            {
                footstepTimer -= Time.deltaTime;
                if (footstepTimer <= 0f)
                {
                    var clip = footstepClips[Random.Range(0, footstepClips.Length)];
                    footstepSource.PlayOneShot(clip);
                    float interval = 1f / Mathf.Max(0.01f, inputMag * stepsPerSecondAtFullSpeed);
                    footstepTimer = interval;
                }
            }
            else
            {
                // reset so next step plays immediately when starting to move
                footstepTimer = 0f;
            }
        }

        // Movement + gravity
        Vector3 move = inputDirection * moveSpeed;

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -1f;

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }
}
