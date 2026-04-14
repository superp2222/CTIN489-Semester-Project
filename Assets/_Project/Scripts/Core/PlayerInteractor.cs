using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction")]
    [Tooltip("This field is deprecated - using Interact action from Input System")]
    public KeyCode interactKey = KeyCode.E;

    [Header("UI")]
    public TMP_Text interactionPrompt;

    private IInteractable current;
    private readonly List<IInteractable> nearbyInteractables = new List<IInteractable>();
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
        current = GetCurrentInteractable();
        if (current != null)
        {
            string prompt = current.GetPrompt();

            if (interactionPrompt != null)
            {
                interactionPrompt.gameObject.SetActive(true);
                interactionPrompt.text = prompt;
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
        var interactable = other.GetComponentInParent<IInteractable>();
        if (interactable != null && !nearbyInteractables.Contains(interactable))
            nearbyInteractables.Add(interactable);
    }

    private void OnTriggerExit(Collider other)
    {
        var exited = other.GetComponentInParent<IInteractable>();
        if (exited != null)
            nearbyInteractables.Remove(exited);

        if (exited != null && exited == current)
            current = null;
    }

    private void OnDisable()
    {
        if (interactionPrompt != null)
            interactionPrompt.gameObject.SetActive(false);
        current = null;
        nearbyInteractables.Clear();
    }

    private IInteractable GetCurrentInteractable()
    {
        for (int i = nearbyInteractables.Count - 1; i >= 0; i--)
        {
            var interactable = nearbyInteractables[i];
            if (interactable == null)
            {
                nearbyInteractables.RemoveAt(i);
                continue;
            }

            if (IsValidInteractable(interactable))
                return interactable;
        }

        return null;
    }

    private bool IsValidInteractable(IInteractable interactable)
    {
        if (interactable == null) return false;

        if (interactable is IConditionalInteractable conditional && !conditional.CanInteract())
            return false;

        return !string.IsNullOrWhiteSpace(interactable.GetPrompt());
    }
}
