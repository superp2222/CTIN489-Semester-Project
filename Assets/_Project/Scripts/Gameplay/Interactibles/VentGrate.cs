using System.Collections.Generic;
using UnityEngine;

public class VentGrate : MonoBehaviour, IInteractable
{
    [Header("Required Tool")]
    public string missingToolMessage = "Looks like I need a screwdriver.";
    public float missingToolMessageDuration = 2.0f;

    [Header("Allowed Users")]
    [Tooltip("If empty, anyone can open the vent once they have the screwdriver. Otherwise, only listed root objects may use it.")]
    public GameObject[] allowedUsers;

    [Header("Denied User Message")]
    public bool showDeniedMessage = true;
    public string deniedMessageText = "They can't use the screwdriver here.";
    public float deniedMessageDuration = 1.5f;

    [Header("Opening (Euler Rotation)")]
    [SerializeField] private bool isOpen = false;
    
    [Tooltip("Assign the actual vent grate mesh transform if different.")]
    public Transform grateTransform;
    [Tooltip("Local Euler angles when the vent is open.")]
    public Vector3 openRotationEuler = new Vector3(0f, 90f, 0f);

    // Track root objects inside our trigger so we can apply the allowedUsers logic.
    private readonly HashSet<GameObject> occupants = new HashSet<GameObject>();

    void Awake()
    {
        if (grateTransform == null)
            grateTransform = transform;
    }

    public void Interact()
    {
        if (isOpen) return;

        // Check allowed users (who can actually use the screwdriver here).
        if (!IsAllowedOccupantPresent())
        {
            if (showDeniedMessage && MessageUI.Instance != null && !string.IsNullOrEmpty(deniedMessageText))
                MessageUI.Instance.ShowMessage(deniedMessageText, deniedMessageDuration);
            return;
        }

        bool hasScrewdriver = GameState.Instance != null && GameState.Instance.HasScrewdriver;
        if (!hasScrewdriver)
        {
            if (MessageUI.Instance != null && !string.IsNullOrEmpty(missingToolMessage))
                MessageUI.Instance.ShowMessage(missingToolMessage, missingToolMessageDuration);
            return;
        }

        OpenVent();
    }

    public string GetPrompt()
    {
        if (isOpen) return "";

        bool hasScrewdriver = GameState.Instance != null && GameState.Instance.HasScrewdriver;
        return hasScrewdriver ? "Press E to unscrew vent" : "Press E to inspect vent";
    }

    private void OpenVent()
    {
        if (isOpen) return;
        isOpen = true;

        // Simple instant open using local Euler rotation.
        if (grateTransform != null)
            grateTransform.localRotation = Quaternion.Euler(openRotationEuler);
    }

    private bool IsAllowedOccupantPresent()
    {
        // Empty list = allow anyone
        if (allowedUsers == null || allowedUsers.Length == 0)
            return true;

        foreach (GameObject occupant in occupants)
        {
            if (occupant == null) continue;

            foreach (GameObject allowed in allowedUsers)
            {
                if (allowed == null) continue;
                if (occupant == allowed)
                    return true;
            }
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject root = other.transform.root.gameObject;
        occupants.Add(root);
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject root = other.transform.root.gameObject;
        occupants.Remove(root);
    }

    private void OnDisable()
    {
        occupants.Clear();
    }
}
