using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;

    private IInteractable current;

    void Update()
    {
        if (current != null && Input.GetKeyDown(interactKey))
        {
            current.Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Look for any interactable on this object or its parents
        current = other.GetComponentInParent<IInteractable>();
    }

    private void OnTriggerExit(Collider other)
    {
        var exited = other.GetComponentInParent<IInteractable>();
        if (exited != null && exited == current)
            current = null;
    }

    // Optional: quick debug so you know it's working
    void OnGUI()
    {
        if (current != null)
        {
            GUI.Label(new Rect(20, 20, 400, 30), current.GetPrompt());
        }
    }
}