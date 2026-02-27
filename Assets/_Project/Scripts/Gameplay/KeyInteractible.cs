using UnityEngine;

public class KeyInteractable : MonoBehaviour, IInteractable
{
    public string pickupMessage = "Picked up the key.";
    public float messageDuration = 2.0f;

    [Header("Audio")]
    public AudioSource pickupAudio;

    private bool pickedUp = false;

    void Awake()
    {
        if (pickupAudio == null)
            pickupAudio = GetComponent<AudioSource>();
    }

    public void Interact()
    {
        if (pickedUp) return;
        pickedUp = true;

        if (GameState.Instance != null)
            GameState.Instance.SetHasKey(true);

        if (MessageUI.Instance != null)
            MessageUI.Instance.ShowMessage(pickupMessage, messageDuration);

        if (pickupAudio != null)
            pickupAudio.Play();

        // Disable collider first so interaction ends immediately
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Delay hiding object so sound can finish
        StartCoroutine(DisableAfterSound());
    }

    private System.Collections.IEnumerator DisableAfterSound()
    {
        if (pickupAudio != null && pickupAudio.clip != null)
            yield return new WaitForSeconds(pickupAudio.clip.length);
        gameObject.SetActive(false);
    }

    public string GetPrompt()
    {
        return pickedUp ? "" : "Press E to pick up key";
    }
}
