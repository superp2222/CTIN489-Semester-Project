using UnityEngine;

public class ButtonInteractable : MonoBehaviour, IInteractable
{
    [Header("Prompt")]
    public string promptText = "Press E to press button";

    [Header("Target")]
    public Barrier targetBarrier;

    [Header("Button Behavior")]
    public bool canPressOnlyOnce = true;

    [Header("Optional Feedback")]
    public AudioSource pressAudio;
    public Animator buttonAnimator;
    public string pressTriggerName = "Press";

    private bool pressed = false;

    void Awake()
    {
        if (pressAudio == null)
            pressAudio = GetComponent<AudioSource>();
    }

    public void Interact()
    {
        if (canPressOnlyOnce && pressed) return;

        pressed = true;

        if (pressAudio != null)
            pressAudio.Play();

        if (buttonAnimator != null && !string.IsNullOrEmpty(pressTriggerName))
            buttonAnimator.SetTrigger(pressTriggerName);

        if (targetBarrier != null)
            targetBarrier.DisableBarrier();

        // Optional: if you want the prompt to disappear instantly after press
        // you can disable the trigger collider, similar to your key pickup.
        // (Only do this if the button should never be used again.)
        // var col = GetComponent<Collider>();
        // if (col != null) col.enabled = false;
    }

    public string GetPrompt()
    {
        if (canPressOnlyOnce && pressed) return "";
        return promptText;
    }
}