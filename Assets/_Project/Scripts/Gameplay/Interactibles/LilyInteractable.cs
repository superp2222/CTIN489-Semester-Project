using UnityEngine;

public class LilyInteractable : MonoBehaviour, IInteractable, IConditionalInteractable
{
    public LilyMeter lilyMeter;

    void Awake()
    {
        if (lilyMeter == null)
            lilyMeter = FindFirstObjectByType<LilyMeter>();
    }

    public bool CanInteract()
    {
        return lilyMeter != null && lilyMeter.CanInteractWithLily();
    }

    public void Interact()
    {
        if (!CanInteract()) return;

        lilyMeter.InteractWithLily();
    }

    public string GetPrompt()
    {
        if (lilyMeter == null) return "";

        return lilyMeter.GetLilyPrompt();
    }
}
