using UnityEngine;

public class Barrier : MonoBehaviour, IInteractable
{
    [Header("Barrier Settings")]
    public bool disableColliders = true;
    public bool disableRenderer = true;
    public bool disableGameObject = true;

    [Header("Interact Message")]
    public string interactMessage = "Press E to inspect barrier";
    public string blockedMessage = "You cannot pass this barrier";
    public float messageDuration = 2.0f;

    private Collider[] cols;
    private Renderer[] rends;

    private bool isDisabled = false;

    void Awake()
    {
        cols = GetComponentsInChildren<Collider>(true);
        rends = GetComponentsInChildren<Renderer>(true);
    }

    // Called by the button (or anything else)
    public void DisableBarrier()
    {
        if (isDisabled) return;
        isDisabled = true;

        // If the whole object is going away, we can just disable it.
        if (disableGameObject)
        {
            gameObject.SetActive(false);
            return;
        }

        if (disableColliders && cols != null)
        {
            foreach (var c in cols)
                if (c != null) c.enabled = false;
        }

        if (disableRenderer && rends != null)
        {
            foreach (var r in rends)
                if (r != null) r.enabled = false;
        }
    }

    // IInteractable
    public void Interact()
    {
        if (isDisabled) return;

        if (MessageUI.Instance != null)
            MessageUI.Instance.ShowMessage(blockedMessage, messageDuration);
    }

    // IInteractable
    public string GetPrompt()
    {
        if (isDisabled) return "";
        return interactMessage;
    }
}