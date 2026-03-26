using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour, IInteractable
{
    [Header("UI / Prompt")]
    public string promptText = "Press E to use elevator";

    [Header("Allowed Users")]
    [Tooltip("If this list is empty, anyone can use it. Otherwise only listed objects may interact.")]
    public GameObject[] allowedUsers;

    [Header("Denied Message")]
    [Tooltip("Shown when a non-allowed character tries to use this.")]
    public bool showDeniedMessage = true;
    public string deniedMessageText = "Lily can't use this.";
    public float deniedMessageDuration = 1.0f;

    [Header("Optional Message")]
    [Tooltip("If set, shows a message before switching scenes.")]
    public bool showMessage = false;
    public string messageText = "Going to the next area...";
    public float messageDuration = 1.0f;

    [Header("Scene Transition")]
    [Tooltip("Exact scene name as shown in Build Settings.")]
    public string nextSceneName = "GroundFloor";

    [Tooltip("If true, this interactable becomes unusable after first use.")]
    public bool oneShot = false;

    private bool used = false;

    // Tracks all root objects currently inside this trigger
    private readonly HashSet<GameObject> occupants = new HashSet<GameObject>();

    public void Interact()
    {
        if (oneShot && used) return;

        if (!IsAllowedOccupantPresent())
        {
            if (showDeniedMessage && MessageUI.Instance != null && !string.IsNullOrEmpty(deniedMessageText))
            {
                MessageUI.Instance.ShowMessage(deniedMessageText, deniedMessageDuration);
            }
            return;
        }

        used = true;

        if (showMessage && MessageUI.Instance != null && !string.IsNullOrEmpty(messageText))
        {
            MessageUI.Instance.ShowMessage(messageText, messageDuration);
        }

        SceneManager.LoadScene(nextSceneName);
    }

    public string GetPrompt()
    {
        if (oneShot && used) return "";
        return promptText;
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