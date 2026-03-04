using UnityEngine;

public class DialogueBarrier : MonoBehaviour
{
    [Header("Dialogue")]
    [TextArea(2, 5)]
    public string line = "…";
    public float duration = 3f;

    [Tooltip("Reference to the CutsceneDialogue in the scene (UI controller).")]
    public CutsceneDialogue dialogue;

    

    [Tooltip("If true, the barrier destroys itself immediately after triggering.")]
    public bool destroyImmediately = true;

    private bool triggered = false;

    void Reset()
    {
        // Helpful defaults when you add the component
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void Awake()
    {
        // Auto-find if not assigned
        if (dialogue == null)
            dialogue = FindFirstObjectByType<CutsceneDialogue>();

        // Ensure this is a trigger
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        

        triggered = true;

        if (dialogue != null)
            dialogue.ShowLineForSeconds(line, duration);

        if (destroyImmediately)
        {
            Destroy(gameObject);
        }
        else
        {
            // If you want it to wait until after the line disappears, use this mode.
            Destroy(gameObject, Mathf.Max(0.05f, duration + 0.05f));
        }
    }
}