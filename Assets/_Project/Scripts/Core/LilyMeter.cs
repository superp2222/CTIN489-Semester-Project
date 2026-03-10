// LilyMeter.cs
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class LilyMeter : MonoBehaviour
{
    public enum LilyState { Active, Tired, Recharging, Static }

    [Header("Start State")]
    [Tooltip("Choose which state Lily begins in when the scene starts.")]
    public LilyState startState = LilyState.Tired;

    [Header("Timing")]
    public float maxActiveTime = 30f;
    public float rechargeTime = 10f;

    [Header("Interaction")]
    [Tooltip("This field is deprecated - using Interact action from Input System")]
    public KeyCode pickUpKey = KeyCode.E;
    public float pickupRange = 2f;

    [HideInInspector] public LilyState state = LilyState.Recharging;
    [HideInInspector] public float timer = 0f;
    [HideInInspector] public bool canSwap = true;
    [HideInInspector] public bool isPickedUp = false;

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
        ApplyStartState();

        if (cutsceneDialogue != null)
            cutsceneDialogue.SetActive(false);

        if (interactionPromptText != null)
            interactionPromptText.text = "";

        if (lilyTimerText != null)
            lilyTimerText.gameObject.SetActive(false);
    }

    void ApplyStartState()
    {
        // Default assumptions: player (Sawyer) starts controlled
        isSisterControlled = false;

        isPickedUp = false;

        if (sisterFollow != null)
            sisterFollow.enabled = false;

        if (playerController != null)
            playerController.canJump = true;

        state = startState;

        switch (startState)
        {
            case LilyState.Active:
                canSwap = true;
                timer = maxActiveTime;
                break;

            case LilyState.Recharging:
                canSwap = false;
                timer = rechargeTime;
                isPickedUp = true;
                if (sisterFollow != null)
                    sisterFollow.enabled = true;
                if (playerController != null)
                    playerController.canJump = false;
                break;

            case LilyState.Tired:
                canSwap = false;
                timer = 0f;
                break;

            case LilyState.Static:
                // Static: stands still, can be picked up, NO UI, NO swapping, NO countdown/teleport
                canSwap = false;
                timer = 0f;
                break;
        }
    }

    void Update()
    {
        UpdateLilyTimerUI();

        // Pause ACTIVE countdown while Sawyer is controlled
        if (state == LilyState.Active && !isSisterControlled)
            return;

        switch (state)
        {
            case LilyState.Active:
                HandleActive();
                break;

            case LilyState.Tired:
                HandleTired(showPrompt: true);
                break;

            case LilyState.Static:
                // Static behaves like tired for pickup interaction, but with NO UI.
                // Most importantly: it NEVER becomes Active unless something external forces it.
                HandleTired(showPrompt: false);
                break;

            case LilyState.Recharging:
                HandleRecharging();
                break;
        }
    }

    void UpdateLilyTimerUI()
    {
        if (lilyTimerText == null) return;

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

        // Only teleport if NOT in Static mode - Static stays in place until picked up
        if (state != LilyState.Static && sister != null && player != null)
            sister.position = player.position + player.forward * 1.0f;

        if (switchManager != null)
            switchManager.ForceSwitchToPlayer();

        if (sisterFollow != null)
            sisterFollow.enabled = false;

        StartDialogue("Lily: Sawyer, I'm tired...\n(Press E to pick up Lily)", 3f);
    }

    void HandleTired(bool showPrompt)
    {
        // Lily should NEVER follow when tired or static
        if (sisterFollow != null)
            sisterFollow.enabled = false;

        bool inRange = IsPlayerInPickupRange();

        if (showPrompt && interactionPromptText != null)
        {
            if (inRange)
            {
                interactionPromptText.text = "Press E to pick up";
                lilyPromptOwned = true;
            }
            else
            {
                if (lilyPromptOwned)
                {
                    interactionPromptText.text = "";
                    lilyPromptOwned = false;
                }
            }
        }

        if (inRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            isPickedUp = true;

            if (sisterFollow != null)
                sisterFollow.enabled = true;

            state = LilyState.Recharging;
            timer = rechargeTime;

            if (playerController != null)
                playerController.canJump = false;

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
        if (!isPickedUp) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        canSwap = true;
        isPickedUp = false;

        if (playerController != null)
            playerController.canJump = true;

        state = LilyState.Active;
        timer = maxActiveTime;

        if (sisterFollow != null)
            sisterFollow.enabled = false;
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
        // If Lily is Static, she must NEVER enter Active (prevents later teleport)
        if (state == LilyState.Static)
        {
            isSisterControlled = false;

            // Optional: if something tried to switch to Lily anyway, force back to Sawyer
            if (switchManager != null)
                switchManager.ForceSwitchToPlayer();

            return;
        }

        isSisterControlled = true;

        if (!canSwap) return;

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

        if (lilyTimerText != null)
            lilyTimerText.gameObject.SetActive(false);
    }

    public bool CanSwapToSister()
    {
        return canSwap && state != LilyState.Tired && state != LilyState.Static;
    }
}