using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCarryController : MonoBehaviour
{
    [Header("Carry Settings")]
    public Transform carryPoint;
    public float followSpeed = 20f;
    public float rotateSpeed = 120f;

    private CarryableObjectInteractable held;
    public CarryableObjectInteractable Held => held;

    void Awake()
    {
    }

    void Update()
    {
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