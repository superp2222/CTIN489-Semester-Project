using UnityEngine;

public class TeleportBarrier : MonoBehaviour
{
    [Header("Who can be teleported")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private SisterController sisterController; // replace with SisterController if you have that type

    [Header("Destination")]
    [SerializeField] private Transform teleportTarget;

    [Header("Optional Settings")]
    [SerializeField] private bool matchTargetRotation = false;

    private bool isTeleporting = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTeleporting) return;

        // Check if the collider belongs to the player or sister
        if (playerController != null && other.gameObject == playerController.gameObject)
        {
            Teleport(playerController.gameObject);
        }
        else if (sisterController != null && other.gameObject == sisterController.gameObject)
        {
            Teleport(sisterController.gameObject);
        }
    }

    private void Teleport(GameObject obj)
    {
        if (teleportTarget == null)
        {
            Debug.LogWarning($"TeleportBarrier on {gameObject.name} has no teleport target assigned.");
            return;
        }

        isTeleporting = true;

        CharacterController cc = obj.GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = false;

        obj.transform.position = teleportTarget.position;

        if (matchTargetRotation)
            obj.transform.rotation = teleportTarget.rotation;

        if (cc != null)
            cc.enabled = true;

        isTeleporting = false;
    }

    private void OnDrawGizmos()
    {
        if (teleportTarget == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, teleportTarget.position);
        Gizmos.DrawSphere(teleportTarget.position, 0.25f);
    }
}