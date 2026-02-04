using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4.5f;
    public float gravity = -20f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2.0f;

    private CharacterController controller;
    private float verticalVelocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        // Lock cursor for third-person mouse look
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // --- Mouse yaw rotates the player ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        // --- WASD movement relative to player forward ---
        float x = Input.GetAxisRaw("Horizontal"); // A/D
        float z = Input.GetAxisRaw("Vertical");   // W/S

        Vector3 move = (transform.right * x + transform.forward * z).normalized;
        Vector3 velocity = move * moveSpeed;

        // --- Gravity ---
        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f; // small stick-to-ground force

        verticalVelocity += gravity * Time.deltaTime;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);

        // Escape key to unlock cursor during dev
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}