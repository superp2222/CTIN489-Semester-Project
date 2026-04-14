using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCarryController : MonoBehaviour
{
    [Header("Carry Settings")]
    public Transform carryPoint;
    public Transform lilyPoint;
    public float followSpeed = 20f;
    public float rotateSpeed = 120f;
    public float carryPointTurnSpeed = 12f;

    private CarryableObjectInteractable held;
    public CarryableObjectInteractable Held => held;

    private PlayerController playerController;
    private float carryPointHeightOffset;
    private float carryPointDistance;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
            playerController = GetComponentInParent<PlayerController>();

        if (lilyPoint == null)
            lilyPoint = transform.Find("LilyPoint");

        if (carryPoint != null)
        {
            Vector3 initialOffset = carryPoint.position - transform.position;
            carryPointHeightOffset = initialOffset.y;

            Vector3 horizontalOffset = new Vector3(initialOffset.x, 0f, initialOffset.z);
            carryPointDistance = horizontalOffset.magnitude;
        }
    }

    void Update()
    {
        UpdateCarryPointRotation();

        if (held == null) return;

        // Smooth follow
        held.FollowCarryPoint(carryPoint.position, carryPoint.rotation, followSpeed);

        // Optional: rotate held object with Previous/Next keys
        float rotate = 0f;
        if (Keyboard.current.digit1Key.isPressed)
            rotate = -1f;
        if (Keyboard.current.digit2Key.isPressed)
            rotate = 1f;

        if (rotate != 0f)
            held.RotateWhileHeld(rotate * rotateSpeed * Time.deltaTime);
    }

    private void UpdateCarryPointRotation()
    {
        if (carryPoint == null) return;
        if (playerController == null || playerController.visualModel == null) return;
        if (Keyboard.current == null) return;

        float x = 0f;
        float z = 0f;
        if (Keyboard.current.wKey.isPressed) z += 1f;
        if (Keyboard.current.sKey.isPressed) z -= 1f;
        if (Keyboard.current.aKey.isPressed) x -= 1f;
        if (Keyboard.current.dKey.isPressed) x += 1f;

        Vector3 inputDir = new Vector3(x, 0f, z);
        if (inputDir.sqrMagnitude < 0.01f)
            return;

        Vector3 facing = playerController.visualModel.forward;
        facing.y = 0f;

        if (facing.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(facing.normalized, Vector3.up);
        Vector3 targetPosition = transform.position + (facing.normalized * carryPointDistance);
        targetPosition.y = transform.position.y + carryPointHeightOffset;

        carryPoint.position = Vector3.Lerp(
            carryPoint.position,
            targetPosition,
            carryPointTurnSpeed * Time.deltaTime
        );
        carryPoint.rotation = Quaternion.Slerp(
            carryPoint.rotation,
            targetRotation,
            carryPointTurnSpeed * Time.deltaTime
        );
    }

    public bool IsHoldingSomething() => held != null;

    public void TryPickUp(CarryableObjectInteractable obj)
    {
        if (obj == null) return;
        if (held != null) return;              // already holding one
        if (carryPoint == null) return;

        held = obj;
        held.OnPickedUp(this, carryPoint);
    }

    public void DropHeld()
    {
        if (held == null) return;

        var toDrop = held;
        held = null;
        toDrop.OnDropped();
    }

    // Called by the object when the player presses E on it.
    public void ToggleCarry(CarryableObjectInteractable obj)
    {
        // If we pressed E on the thing we're holding, drop it.
        if (held != null && obj == held)
        {
            DropHeld();
            return;
        }

        // Otherwise try to pick up that object.
        TryPickUp(obj);
    }
}
