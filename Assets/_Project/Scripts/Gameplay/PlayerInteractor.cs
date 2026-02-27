using UnityEngine;
using TMPro;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;

    [Header("UI")]
    public TMP_Text interactionPrompt;

    private IInteractable current;

    void Update()
    {
        if (interactionPrompt == null) return;

        if (current != null)
        {
            interactionPrompt.gameObject.SetActive(true);
            interactionPrompt.text = current.GetPrompt();

            if (Input.GetKeyDown(interactKey))
            {
                // Cache current, then clear immediately so UI can't stick if the object disables itself
                var interacted = current;
                current = null;

                interactionPrompt.gameObject.SetActive(false);

                interacted.Interact();
            }

        }
        else
        {
            interactionPrompt.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        current = other.GetComponentInParent<IInteractable>();
    }

    private void OnTriggerExit(Collider other)
    {
        var exited = other.GetComponentInParent<IInteractable>();
        if (exited != null && exited == current)
            current = null;
    }

    // Nice to have: clears prompt when this component gets disabled (switching characters)
    private void OnDisable()
    {
        if (interactionPrompt != null)
            interactionPrompt.gameObject.SetActive(false);

        current = null;
    }
}
