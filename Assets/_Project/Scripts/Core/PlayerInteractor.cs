using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction")]
    [Tooltip("This field is deprecated - using Interact action from Input System")]
    public KeyCode interactKey = KeyCode.E;

    [Header("UI")]
    public TMP_Text interactionPrompt;

    private IInteractable current;
    private PlayerCarryController carry;

    void Awake()
    {
        carry = GetComponent<PlayerCarryController>();
        if (carry == null) carry = GetComponentInParent<PlayerCarryController>();
    }

    void Update()
    {
        // If holding something, show drop prompt and drop on Interact
        if (carry != null && carry.Held != null)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.gameObject.SetActive(true);
                interactionPrompt.text = carry.Held.GetPrompt(); // should be "Press E to set down"
            }

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                carry.DropHeld();
            }
            return;
        }

        // Normal interact logic
        if (current != null)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.gameObject.SetActive(true);
                interactionPrompt.text = current.GetPrompt();
            }

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                var interacted = current;
                current = null;

                if (interactionPrompt != null)
                    interactionPrompt.gameObject.SetActive(false);

                interacted.Interact();
            }
        }
        else
        {
            if (interactionPrompt != null)
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

    private void OnDisable()
    {
        if (interactionPrompt != null)
            interactionPrompt.gameObject.SetActive(false);
        current = null;
    }

    
}