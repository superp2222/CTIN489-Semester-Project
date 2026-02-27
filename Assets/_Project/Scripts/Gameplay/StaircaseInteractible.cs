using UnityEngine;
using UnityEngine.SceneManagement;

public class StaircaseInteractable : MonoBehaviour, IInteractable
{
    [Header("Messages")]
    public string missingKeyMessage = "I should find the key first...";
    public float messageDuration = 2.0f;

    [Header("Scene Transition")]
    public string nextSceneName = "GroundFloor";

    public void Interact()
    {
        bool hasKey = GameState.Instance != null && GameState.Instance.HasKey;

        if (!hasKey)
        {
            if (MessageUI.Instance != null)
                MessageUI.Instance.ShowMessage(missingKeyMessage, messageDuration);
            return;
        }

        // Load next scene
        SceneManager.LoadScene(nextSceneName);
    }

    public string GetPrompt()
    {
        bool hasKey = GameState.Instance != null && GameState.Instance.HasKey;
        return hasKey ? "Press E to go downstairs" : "Press E to inspect staircase";
    }
}
