using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4.5f;

    [Header("Gravity & Jump")]
    public float gravity = -20f;
    public float jumpHeight = 1.2f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 0.1f;

    [Header("Jump Audio")]
    public AudioSource audioSource;
    public AudioClip jumpSFX;

    [Header("Animation")]
    public Animator animator;
    public Transform visualModel;
    public float modelTurnSpeed = 12f;

    [Header("Crouch & Sprint")]
    public float crouchSpeed = 2.5f;
    public float sprintSpeed = 8f;

    [Range(0.2f, 1f)]
    public float crouchHeightMultiplier = 0.5f;

    public float standClearance = 0.05f;

    private CharacterController controller;
    private float verticalVelocity;
    private GameManager gameManager;

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
        gameManager = FindFirstObjectByType<GameManager>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (visualModel == null && animator != null)
        {
            // Ensure the visual model is a child, so we never rotate
            // the root transform that the camera follows.
            if (animator.transform == transform && animator.transform.childCount > 0)
            {
                visualModel = animator.transform.GetChild(0);
            }
            else
            {
                visualModel = animator.transform;
            }
        }

        if (animator != null)
            animator.applyRootMotion = false;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (gameManager != null && gameManager.IsPaused)
            return;

        Vector2 lookInput = Mouse.current.delta.ReadValue();
        float mouseX = lookInput.x * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            if (!isCrouching)
                SetCrouch(true);
            else if (CanStandUp())
                SetCrouch(false);
        }

        if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
            isSprinting = !isSprinting;

        float x = 0f;
        float z = 0f;
        if (Keyboard.current.wKey.isPressed) z += 1f;
        if (Keyboard.current.sKey.isPressed) z -= 1f;
        if (Keyboard.current.aKey.isPressed) x -= 1f;
        if (Keyboard.current.dKey.isPressed) x += 1f;

        Vector3 inputDir = new Vector3(x, 0f, z).normalized;
        Vector3 move = (transform.right * x + transform.forward * z).normalized;

        float currentSpeed = moveSpeed;
        if (isCrouching) currentSpeed = crouchSpeed;
        else if (isSprinting) currentSpeed = sprintSpeed;

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (controller.isGrounded && Keyboard.current.spaceKey.wasPressedThisFrame && canJump && !isCrouching)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (audioSource && jumpSFX)
                audioSource.PlayOneShot(jumpSFX);

            if (animator != null)
                animator.SetTrigger("Jump");
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * currentSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);

        RotateVisualModel(inputDir);
        UpdateAnimator(inputDir);
    }

    void RotateVisualModel(Vector3 inputDir)
    {
        // Never rotate the root transform; only rotate a child model.
        if (visualModel == null || visualModel == transform || inputDir.sqrMagnitude < 0.01f)
            return;

        float targetY = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, targetY, 0f);
        visualModel.localRotation = Quaternion.Slerp(
            visualModel.localRotation,
            targetRotation,
            modelTurnSpeed * Time.deltaTime
        );
    }

    void UpdateAnimator(Vector3 inputDir)
    {
        if (animator == null) return;

        bool isMoving = inputDir.sqrMagnitude > 0.01f;
        bool isRunningNow = isMoving && isSprinting && !isCrouching;
        bool isWalkingNow = isMoving && !isRunningNow && !isCrouching;

        animator.SetBool("IsWalking", isWalkingNow);
        animator.SetBool("IsRunning", isRunningNow);
        animator.SetBool("IsCrouching", isCrouching);
    }

    void SetCrouch(bool crouch)
    {
        isCrouching = crouch;

        if (crouch)
            isSprinting = false;

        if (crouch)
        {
            float newHeight = originalHeight * crouchHeightMultiplier;
            controller.height = newHeight;

            float offset = (originalHeight - newHeight) * 0.5f;
            controller.center = originalCenter - new Vector3(0, offset, 0);
        }
        else
        {
            controller.height = originalHeight;
            controller.center = originalCenter;
        }
    }

    bool CanStandUp()
    {
        float radius = controller.radius;
        float currentHeight = controller.height;
        float targetHeight = originalHeight;

        if (targetHeight <= currentHeight) return true;

        float extra = (targetHeight - currentHeight) + standClearance;
        Vector3 origin = transform.position + controller.center + Vector3.up * (currentHeight * 0.5f);

        return !Physics.SphereCast(origin, radius, Vector3.up, out _, extra, ~0, QueryTriggerInteraction.Ignore);
    }
}
