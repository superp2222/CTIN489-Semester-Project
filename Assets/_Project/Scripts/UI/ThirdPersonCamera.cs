using UnityEngine;

public class ThirdPersonCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // active character

    [Header("Rig Offset (relative to target)")]
    public Vector3 offset = new Vector3(0f, 1.6f, 0f);
    public float followSmoothTime = 0.08f;

    [Header("Pitch")]
    public float pitch = 5f; // degrees

    [Header("Camera Child")]
    public Transform cam;                 // assign Main Camera (child of this rig)
    public float desiredCameraDistance = 4.0f; // how far back the camera wants to be
    public float minCameraDistance = 0.6f;     // how close it can get
    public float zoomSmoothTime = 0.05f;

    [Header("Collision")]
    public float probeRadius = 0.25f;
    public LayerMask collisionMask = ~0;  // set to Environment layer(s) recommended
    public float collisionBuffer = 0.05f;

    private Vector3 velocity;
    private float currentDistance;
    private float distanceVelocity;

    void Awake()
    {
        if (cam == null && Camera.main != null)
            cam = Camera.main.transform;

        currentDistance = desiredCameraDistance;
    }

    void LateUpdate()
    {
        if (target == null || cam == null) return;

        // 1) Follow rig position behind/around the target (relative to target rotation)
        Vector3 desiredPos = target.position + target.TransformDirection(offset);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, followSmoothTime);

        // 2) Match target yaw + fixed pitch
        transform.rotation = Quaternion.Euler(pitch, target.eulerAngles.y, 0f);

        // 3) Camera collision: spherecast from pivot (rig) backward to desired camera distance
        Vector3 pivot = transform.position;
        Vector3 backDir = -transform.forward;

        float targetDist = desiredCameraDistance;

        if (Physics.SphereCast(pivot, probeRadius, backDir, out RaycastHit hit, desiredCameraDistance, collisionMask, QueryTriggerInteraction.Ignore))
        {
            targetDist = Mathf.Clamp(hit.distance - collisionBuffer, minCameraDistance, desiredCameraDistance);
        }

        // 4) Smoothly adjust distance, then apply camera local position
        currentDistance = Mathf.SmoothDamp(currentDistance, targetDist, ref distanceVelocity, zoomSmoothTime);

        // Keep camera centered on rig axis (local)
        cam.localPosition = new Vector3(0f, 0f, -currentDistance);
        cam.localRotation = Quaternion.identity;
    }
}
