using UnityEngine;

public class KeyInteractable : MonoBehaviour, IInteractable
{
    public string pickupMessage = "Picked up the key.";
    public float messageDuration = 2.0f;

    public void Interact()
    {
        // Set inventory flag
        if (GameState.Instance != null)
            GameState.Instance.SetHasKey(true);

        // Feedback
        if (MessageUI.Instance != null)
            MessageUI.Instance.ShowMessage(pickupMessage, messageDuration);

        // Disable so it disappears and can't be interacted with again
        gameObject.SetActive(false);
    }

    public string GetPrompt()
    {
        return "Press E to pick up key";
    }
}