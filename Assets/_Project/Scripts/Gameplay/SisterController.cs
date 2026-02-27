using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SisterController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;

    [Header("Gravity & Jump")]
    public float gravity = -20f;
    public float jumpHeight = 1.2f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2.0f;

    [Header("Crouch & Sprint")]
    public float crouchSpeed = 2.5f;
    public float sprintSpeed = 12f;

    [Range(0.2f, 1f)]
    public float crouchHeightMultiplier = 0.5f;

    public float standClearance = 0.05f;

    private CharacterController controller;
    private float verticalVelocity;

    private bool isCrouching = false;
    private bool isSprinting = false;

    [Header("Jump Audio")]
    public AudioSource audioSource;
    public AudioClip jumpSFX;

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
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (!isCrouching)
            {
                SetCrouch(true);
            }
            else
            {
                if (CanStandUp())
                    SetCrouch(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            isSprinting = !isSprinting;
        }

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

        if (controller.isGrounded && Input.GetKeyDown(KeyCode.Space) && !isCrouching)
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
        float radius = controller.radius;
        float currentHeight = controller.height;
        float targetHeight = originalHeight;

        if (targetHeight <= currentHeight) return true;

        float extraHeightNeeded = (targetHeight - currentHeight) + standClearance;

        Vector3 origin = transform.position + controller.center + Vector3.up * (currentHeight * 0.5f);
        return !Physics.SphereCast(origin, radius, Vector3.up, out _, extraHeightNeeded, ~0, QueryTriggerInteraction.Ignore);
    }
}