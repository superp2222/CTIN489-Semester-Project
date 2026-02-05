using UnityEngine;

public class StaircaseInteractable : MonoBehaviour, IInteractable
{
    [Header("Messages")]
    public string missingKeyMessage = "I should find the key first...";
    public string endMessage = "END OF PROTOTYPE";
    public float messageDuration = 3.0f;

    public void Interact()
    {
        bool hasKey = GameState.Instance != null && GameState.Instance.HasKey;

        if (!hasKey)
        {
            if (MessageUI.Instance != null)
                MessageUI.Instance.ShowMessage(missingKeyMessage, messageDuration);
            return;
        }

        if (MessageUI.Instance != null)
            MessageUI.Instance.ShowMessage(endMessage, messageDuration);
    }

    public string GetPrompt()
    {
        bool hasKey = GameState.Instance != null && GameState.Instance.HasKey;
        return hasKey ? "Press E to go downstairs" : "Press E to inspect staircase";
    }
}