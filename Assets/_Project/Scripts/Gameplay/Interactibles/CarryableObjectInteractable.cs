using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarryableObjectInteractable : MonoBehaviour, IInteractable
{
    [Header("Prompt Text")]
    public string pickupPrompt = "Press E to pick up";
    public string dropPrompt = "Press E to set down";

    [Header("Drop Settings")]
    public float dropForwardOffset = 0.6f;
    public float dropUpOffset = 0.1f;

    private Rigidbody rb;
    private Collider[] allColliders;

    private PlayerCarryController carrier;
    private Transform carryPoint;

    private bool isHeld = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        allColliders = GetComponentsInChildren<Collider>(true);
    }

    public void Interact()
    {
        // Find the player's carry controller.
        // Since your PlayerInteractor is on the player, the simplest assumption is:
        // the player is the one currently in range, and it has PlayerCarryController somewhere up its hierarchy.
        //
        // If this object is interacted with, we toggle carry for whoever is closest via the carrier reference.
        //
        // Easiest reliable method: cache the carrier on trigger enter using a small helper trigger,
        // BUT since you already have PlayerInteractor doing trigger enter/exit, we can do:
        // FindObjectOfType<PlayerCarryController>() as a quick start (fine for small projects),
        // or set a reference in inspector if there’s only one player.
        //
        // Best practice: the player that pressed E should call into us.
        // Your current IInteractable doesn’t pass the interactor, so we’ll do a simple approach:
        var playerCarry = FindFirstObjectByType<PlayerCarryController>();
        if (playerCarry == null) return;

        playerCarry.ToggleCarry(this);
    }

    public string GetPrompt()
    {
        return isHeld ? dropPrompt : pickupPrompt;
    }

    public void OnPickedUp(PlayerCarryController newCarrier, Transform newCarryPoint)
    {
        carrier = newCarrier;
        carryPoint = newCarryPoint;
        isHeld = true;

        // Physics off while held
        rb.isKinematic = true;
        rb.useGravity = false;

        // Keep colliders enabled if you want it to still block things,
        // but usually you disable them so it doesn't jitter against the player.
        // We'll disable all colliders except triggers (optional).
        foreach (var c in allColliders)
        {
            if (c == null) continue;
            // If you use a trigger collider specifically for interact range, keep it enabled.
            // Otherwise, disable non-trigger colliders to avoid player collision jitter.
            if (!c.isTrigger) c.enabled = false;
        }

        // Snap to carry point immediately
        transform.position = carryPoint.position;
        transform.rotation = carryPoint.rotation;
    }

    public void OnDropped()
    {
        isHeld = false;

        // Place it slightly in front of carry point to avoid clipping into player
        if (carryPoint != null)
        {
            Vector3 dropPos = carryPoint.position + (carryPoint.forward * dropForwardOffset) + (Vector3.up * dropUpOffset);
            transform.position = dropPos;
        }

        // Physics back on
        rb.isKinematic = false;
        rb.useGravity = true;

        foreach (var c in allColliders)
        {
            if (c == null) continue;
            c.enabled = true;
        }

        carrier = null;
        carryPoint = null;
    }

    // Called every frame by PlayerCarryController while held
    public void FollowCarryPoint(Vector3 targetPos, Quaternion targetRot, float speed)
    {
        if (!isHeld) return;

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * speed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * speed);
    }

    public void RotateWhileHeld(float degrees)
    {
        if (!isHeld) return;
        // rotate around world up by default
        transform.Rotate(Vector3.up, degrees, Space.World);
    }
}