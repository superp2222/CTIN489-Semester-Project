using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FollowTarget : MonoBehaviour
{
    public Transform target;
    public float followDistance = 1.5f;
    public float moveSpeed = 3.5f;
    public float rotateSpeed = 720f; // degrees/sec
    public float gravity = -20f;

    private CharacterController controller;
    private float verticalVelocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (target == null) return;

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        float dist = toTarget.magnitude;

        // If close enough, stop
        if (dist <= followDistance)
        {
            // still apply gravity so she stays grounded on slopes/steps
            ApplyGravityAndMove(Vector3.zero);
            return;
        }

        // Move toward target
        Vector3 dir = toTarget.normalized;

        // Smoothly rotate to face movement direction
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion desired = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, rotateSpeed * Time.deltaTime);
        }

        Vector3 horizontal = dir * moveSpeed;
        ApplyGravityAndMove(horizontal);
    }

    private void ApplyGravityAndMove(Vector3 horizontalVelocity)
    {
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 v = horizontalVelocity;
        v.y = verticalVelocity;

        controller.Move(v * Time.deltaTime);
    }
}
