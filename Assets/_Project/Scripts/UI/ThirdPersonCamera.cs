using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Rig Offset (relative to target)")]
    public Vector3 offset = new Vector3(0f, 1.6f, 0f);
    public float followSmoothTime = 0.08f;

    [Header("Pitch")]
    [Tooltip("Default vertical angle (15–20 feels good for 3rd person).")]
    public float pitch = 15f;

    public float minPitch = -40f;
    public float maxPitch = 75f;
    public float pitchSensitivity = 0.1f;
    public float pitchSmoothSpeed = 12f;

    private float pitchVelocity;

    [Header("Camera Child")]
    public Transform cam;
    public float desiredCameraDistance = 4.0f;
    public float minCameraDistance = 0.6f;
    public float zoomSmoothTime = 0.05f;

    [Header("Collision")]
    public float probeRadius = 0.25f;
    public LayerMask collisionMask = ~0;
    public float collisionBuffer = 0.05f;

    private Vector3 velocity;
    private float currentDistance;
    private float distanceVelocity;

    private float smoothPitch;

    void Awake()
    {
        if (cam == null && Camera.main != null)
            cam = Camera.main.transform;

        currentDistance = desiredCameraDistance;
        smoothPitch = pitch; // ensure clean startup
    }

    void LateUpdate()
    {
        if (target == null || cam == null) return;

        // ===== MOUSE INPUT =====
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float mouseY = mouseDelta.y * pitchSensitivity;

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        smoothPitch = Mathf.SmoothDamp(
            smoothPitch,
            pitch,
            ref pitchVelocity,
            1f / pitchSmoothSpeed
        );

        // ===== FOLLOW POSITION =====
        Vector3 desiredPos = target.position + target.TransformDirection(offset);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, followSmoothTime);

        // ===== ROTATION =====
        transform.rotation = Quaternion.Euler(smoothPitch, target.eulerAngles.y, 0f);

        // ===== COLLISION =====
        Vector3 pivot = transform.position;
        Vector3 backDir = -transform.forward;

        float targetDist = desiredCameraDistance;

        if (Physics.SphereCast(pivot, probeRadius, backDir, out RaycastHit hit,
            desiredCameraDistance, collisionMask, QueryTriggerInteraction.Ignore))
        {
            targetDist = Mathf.Clamp(hit.distance - collisionBuffer, minCameraDistance, desiredCameraDistance);
        }

        currentDistance = Mathf.SmoothDamp(currentDistance, targetDist, ref distanceVelocity, zoomSmoothTime);

        cam.localPosition = new Vector3(0f, 0f, -currentDistance);
        cam.localRotation = Quaternion.identity;
    }
}