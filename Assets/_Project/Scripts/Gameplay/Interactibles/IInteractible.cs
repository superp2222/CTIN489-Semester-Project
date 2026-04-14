public interface IInteractable
{
    void Interact();
    string GetPrompt();
}

public interface IConditionalInteractable
{
    bool CanInteract();
}
