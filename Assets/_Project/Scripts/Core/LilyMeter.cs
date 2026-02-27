// LilyMeter.cs
using System.Collections;
using UnityEngine;
using TMPro;

public class LilyMeter : MonoBehaviour
{
    public enum LilyState { Active, Tired, Recharging }

    [Header("Timing")]
    public float maxActiveTime = 30f;
    public float rechargeTime = 10f;

    [Header("Interaction")]
    public KeyCode pickUpKey = KeyCode.E;
    public float pickupRange = 2f;

    [HideInInspector] public LilyState state = LilyState.Recharging;
    [HideInInspector] public float timer = 0f;
    [HideInInspector] public bool canSwap = true;
    [HideInInspector] public bool isPickedUp = false;

    // NEW: track who is currently controlled
    [HideInInspector] public bool isSisterControlled = false;

    [Header("References")]
    public Transform player;
    public Transform sister;
    public FollowTarget sisterFollow;
    public CharacterSwitchManager switchManager;
    public PlayerController playerController;
    public SisterController sisterController;

    [Header("UI References")]
    public TMP_Text lilyTimerText;
    public GameObject cutsceneDialogue;
    public TMP_Text cutsceneDialogueText;
    public TMP_Text interactionPromptText;


    private bool lilyPromptOwned = false;

    Coroutine dialogueRoutine;

    void Start()
    {
        state = LilyState.Recharging;
        timer = rechargeTime;
        canSwap = true;
        isPickedUp = false;
        isSisterControlled = false;

        if (cutsceneDialogue != null)
            cutsceneDialogue.SetActive(false);

        if (interactionPromptText != null)
            interactionPromptText.text = "";

        if (lilyTimerText != null)
            lilyTimerText.gameObject.SetActive(false);
    }

    void Update()
    {
        UpdateLilyTimerUI();

        // PAUSE the ACTIVE countdown while Sawyer is controlled
        // (Recharging/Tired still run so pickup & recharge work while Sawyer is active.)
        if (state == LilyState.Active && !isSisterControlled)
            return;

        switch (state)
        {
            case LilyState.Active:
                HandleActive();
                break;

            case LilyState.Tired:
                HandleTired();
                break;

            case LilyState.Recharging:
                HandleRecharging();
                break;
        }
    }

    void UpdateLilyTimerUI()
    {
        if (lilyTimerText == null) return;

        // Show "Lily: Xs left" ONLY while Lily is controlled.
        // Optionally show recharge while being carried (even if Sawyer controlled).
        bool show = false;
        string text = "";

        if (state == LilyState.Active && isSisterControlled)
        {
            show = true;
            text = $"Lily: {Mathf.CeilToInt(timer)}s left";
        }
        else if (state == LilyState.Recharging && isPickedUp)
        {
            show = true;
            text = $"Lily recharging: {Mathf.CeilToInt(timer)}s";
        }

        lilyTimerText.text = text;
        lilyTimerText.gameObject.SetActive(show);
    }

    void HandleActive()
    {
        timer -= Time.deltaTime;

        if (timer > 0f) return;

        // Sister gets tired
        state = LilyState.Tired;
        timer = 0f;
        canSwap = false;

        // Teleport sister to player and force swap
        if (sister != null && player != null)
            sister.position = player.position + player.forward * 1.0f;

        if (switchManager != null)
            switchManager.ForceSwitchToPlayer();

        if (sisterFollow != null)
            sisterFollow.enabled = false;

        // Show dialogue panel for 3 seconds
        StartDialogue("Lily: Sawyer, I'm tired...\n(Press E to pick up Lily)", 3f);
    }

    void HandleTired()
    {
        bool inRange = IsPlayerInPickupRange();

        // Prompt (ONLY while tired)
        if (interactionPromptText != null)
        {
            if (inRange)
            {
                interactionPromptText.text = "Press E to pick up";
                lilyPromptOwned = true;
            }
            else
            {
                // Only clear if LilyMeter wrote it
                if (lilyPromptOwned)
                {
                    interactionPromptText.text = "";
                    lilyPromptOwned = false;
                }
            }
        }

        // Pickup
        if (inRange && Input.GetKeyDown(pickUpKey))
        {
            isPickedUp = true;

            if (sisterFollow != null)
                sisterFollow.enabled = true;

            state = LilyState.Recharging;
            timer = rechargeTime;

            if (playerController != null)
                playerController.canJump = false;

            // Clear prompt if we owned it
            if (interactionPromptText != null && lilyPromptOwned)
            {
                interactionPromptText.text = "";
                lilyPromptOwned = false;
            }

            if (cutsceneDialogue != null)
                cutsceneDialogue.SetActive(false);
        }
    }
    void HandleRecharging()
    {
        // DO NOT clear interactionPromptText here.
        // The PlayerInteractor needs to use it for keys/doors/etc.

        if (!isPickedUp) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        canSwap = true;
        isPickedUp = false;

        if (playerController != null)
            playerController.canJump = true;

        state = LilyState.Active;
        timer = maxActiveTime;
    }

    bool IsPlayerInPickupRange()
    {
        if (player == null || sister == null) return false;
        float sqrRange = pickupRange * pickupRange;
        return (player.position - sister.position).sqrMagnitude < sqrRange;
    }

    void StartDialogue(string message, float duration)
    {
        if (cutsceneDialogue == null || cutsceneDialogueText == null) return;

        if (dialogueRoutine != null)
            StopCoroutine(dialogueRoutine);

        dialogueRoutine = StartCoroutine(ShowDialoguePanelCoroutine(message, duration));
    }

    IEnumerator ShowDialoguePanelCoroutine(string message, float duration)
    {
        cutsceneDialogueText.text = message;
        cutsceneDialogue.SetActive(true);
        yield return new WaitForSeconds(duration);
        cutsceneDialogue.SetActive(false);
        dialogueRoutine = null;
    }

    // Called by CharacterSwitchManager when Lily becomes the controlled character
    public void OnSisterActivated()
    {
        isSisterControlled = true;

        if (!canSwap) return;

        // IMPORTANT: do NOT reset timer if already active (prevents swapping back to Lily resetting to 30s)
        if (state != LilyState.Active)
        {
            state = LilyState.Active;
            timer = maxActiveTime;
        }
    }

    // Called by CharacterSwitchManager when Sawyer becomes the controlled character
    public void OnPlayerActivated()
    {
        isSisterControlled = false;

        // Optional: instantly hide UI when Sawyer is controlled
        if (lilyTimerText != null)
            lilyTimerText.gameObject.SetActive(false);
    }

    public bool CanSwapToSister()
    {
        return canSwap && state != LilyState.Tired;
    }
}