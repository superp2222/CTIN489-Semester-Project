using UnityEngine;

public class ThirdPersonCameraFollow : MonoBehaviour
{
    public Transform target; // Player
    public Vector3 offset = new Vector3(0f, 0f, 0f);
    public float followSmoothTime = 0.08f;

    [Header("Pitch")]
    public float pitch = 5f; // degrees, slight downward angle

    private Vector3 velocity;

    void LateUpdate()
    {
        if (target == null) return;

        // Position camera rig behind the player, relative to player's rotation
        Vector3 desiredPos = target.position + target.TransformDirection(offset);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, followSmoothTime);

        // Look direction: match player's yaw + fixed pitch
        Quaternion desiredRot = Quaternion.Euler(pitch, target.eulerAngles.y, 0f);
        transform.rotation = desiredRot;
    }
}