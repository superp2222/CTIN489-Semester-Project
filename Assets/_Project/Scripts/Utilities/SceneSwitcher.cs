using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour, IInteractable
{
    [Header("UI / Prompt")]
    public string promptText = "Press E to use elevator";

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

    public void Interact()
    {
        if (oneShot && used) return;
        used = true;

        if (showMessage && MessageUI.Instance != null && !string.IsNullOrEmpty(messageText))
        {
            MessageUI.Instance.ShowMessage(messageText, messageDuration);
        }

        // If you want the message to display BEFORE the scene loads, uncomment coroutine section below.
        SceneManager.LoadScene(nextSceneName);
    }

    public string GetPrompt()
    {
        if (oneShot && used) return "";
        return promptText;
    }
}