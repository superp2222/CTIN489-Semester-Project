using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4.5f;

    [Header("Gravity & Jump")]
    public float gravity = -20f;
    public float jumpHeight = 1.2f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2.0f;

    [Header("Jump Audio")]
    public AudioSource audioSource;
    public AudioClip jumpSFX;

    private CharacterController controller;
    private float verticalVelocity;

    [Header("Crouch & Sprint")]
    public float crouchSpeed = 2.5f;
    public float sprintSpeed = 8f;

    [Tooltip("Multiplier on CharacterController height while crouching (e.g., 0.5 = half height).")]
    [Range(0.2f, 1f)]
    public float crouchHeightMultiplier = 0.5f;

    [Tooltip("Extra clearance required to stand up (small helps avoid ceiling clipping).")]
    public float standClearance = 0.05f;

    private bool isCrouching = false;
    private bool isSprinting = false;

    [HideInInspector] public bool canJump = true;

    private float originalHeight;
    private Vector3 originalCenter;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        originalHeight = controller.height;
        originalCenter = controller.center;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Mouse yaw rotates the player
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        // Toggle crouch
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (!isCrouching)
            {
                SetCrouch(true);
            }
            else
            {
                // Only stand if there is room
                if (CanStandUp())
                    SetCrouch(false);
            }
        }

        // Toggle sprint (optionally disable sprint while crouched)
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            isSprinting = !isSprinting;
        }

        // WASD movement relative to player facing
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 move = (transform.right * x + transform.forward * z).normalized;

        float currentSpeed = moveSpeed;
        if (isCrouching)
            currentSpeed = crouchSpeed;
        else if (isSprinting)
            currentSpeed = sprintSpeed;

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        // Jump (usually you disallow jumping while crouched; optional)
        if (controller.isGrounded && Input.GetKeyDown(KeyCode.Space) && canJump && !isCrouching)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (audioSource != null && jumpSFX != null)
                audioSource.PlayOneShot(jumpSFX);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * currentSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void SetCrouch(bool crouch)
    {
        isCrouching = crouch;

        if (crouch)
        {
            float newHeight = originalHeight * crouchHeightMultiplier;
            controller.height = newHeight;

            // Keep feet on ground: lower the center by half the height difference
            float centerOffsetY = (originalHeight - newHeight) * 0.5f;
            controller.center = originalCenter - new Vector3(0f, centerOffsetY, 0f);
        }
        else
        {
            controller.height = originalHeight;
            controller.center = originalCenter;
        }
    }

    bool CanStandUp()
    {
        // Cast upward from current capsule top to see if we have space to return to original height.
        float radius = controller.radius;
        float currentHeight = controller.height;
        float targetHeight = originalHeight;

        if (targetHeight <= currentHeight) return true;

        float extraHeightNeeded = (targetHeight - currentHeight) + standClearance;

        // We raycast from just above the head so we don't instantly hit ourselves.
        Vector3 origin = transform.position + controller.center + Vector3.up * (currentHeight * 0.5f);
        return !Physics.SphereCast(origin, radius, Vector3.up, out _, extraHeightNeeded, ~0, QueryTriggerInteraction.Ignore);
    }
}