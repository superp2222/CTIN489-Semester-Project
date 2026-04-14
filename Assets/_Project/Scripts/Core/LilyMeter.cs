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

    [Header("Carried Recharge")]
    public PlayerCarryController playerCarryController;
    public Transform lilyPoint;
    public float lilyCarryFollowSpeed = 20f;
    public Animator sawyerAnimator;
    public string carryingParameterName = "Carrying";

    [Header("UI References")]
    public TMP_Text lilyTimerText;
    public GameObject cutsceneDialogue;
    public TMP_Text cutsceneDialogueText;
    public TMP_Text interactionPromptText;

    [Header("Dialogue")]
    public string tiredMessage = "Lily: Sawyer, I'm tired...\n(Press E to pick up Lily)";
    public float tiredMessageDuration = 3f;

    public string readyFromRechargeMessage = "Lily: Okay! I'm ready again!";
    public float readyFromRechargeDuration = 2f;

    public string readyFromStaticMessage = "Lily: Okay! Let's go!";
    public float readyFromStaticDuration = 2f;

    Coroutine dialogueRoutine;
    CharacterController sisterCharacterController;
    bool lilyIsBeingCarried;
    bool carryingAnimatorParameterExists;

    void Start()
    {
        CacheCarriedRechargeReferences();
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
                StartCarryingLily();
                if (playerController != null)
                    playerController.canJump = false;
                break;

            case LilyState.Tired:
                canSwap = false;
                timer = 0f;
                break;

            case LilyState.Static:
                canSwap = false;
                timer = 0f;
                break;
        }
    }

    void Update()
    {
        UpdateLilyTimerUI();

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

            case LilyState.Static:
                HandleStatic();
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

        state = LilyState.Tired;
        timer = 0f;
        canSwap = false;

        if (sister != null && player != null)
            MoveCharacterTo(sister, player.position + player.forward * 1.0f);

        if (switchManager != null)
            switchManager.ForceSwitchToPlayer();

        if (sisterFollow != null)
            sisterFollow.enabled = false;

        StartDialogue(tiredMessage, tiredMessageDuration);
    }

    void HandleTired()
    {
        if (sisterFollow != null)
            sisterFollow.enabled = false;

        if (IsPlayerInPickupRange() && Keyboard.current.eKey.wasPressedThisFrame)
            InteractWithLily();
    }

    void HandleStatic()
    {
        if (sisterFollow != null)
            sisterFollow.enabled = false;

        if (IsPlayerInPickupRange() && Keyboard.current.eKey.wasPressedThisFrame)
            InteractWithLily();
    }

    void HandleRecharging()
    {
        if (!isPickedUp) return;

        if (sisterFollow != null)
            sisterFollow.enabled = false;

        UpdateCarriedLilyPosition();

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        canSwap = true;
        isPickedUp = false;

        if (playerController != null)
            playerController.canJump = true;

        state = LilyState.Active;
        timer = maxActiveTime;

        StopCarryingLily();

        StartDialogue(readyFromRechargeMessage, readyFromRechargeDuration);
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

    public void OnSisterActivated()
    {
        if (state == LilyState.Static)
        {
            isSisterControlled = false;

            if (switchManager != null)
                switchManager.ForceSwitchToPlayer(false);

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

    public void OnPlayerActivated()
    {
        isSisterControlled = false;

        if (state == LilyState.Recharging && sisterFollow != null)
            sisterFollow.enabled = false;

        if (lilyTimerText != null)
            lilyTimerText.gameObject.SetActive(false);
    }

    public bool CanSwapToSister()
    {
        return canSwap && state != LilyState.Tired && state != LilyState.Static;
    }

    public void RecallLilyToPlayer()
    {
        if (player == null || sister == null) return;
        if (isSisterControlled) return;
        if (state == LilyState.Recharging) return;

        Vector3 offset = -player.forward * 1.0f + player.right * 0.6f;
        MoveCharacterTo(sister, player.position + offset);

        if (state == LilyState.Active && sisterFollow != null)
        {
            sisterFollow.target = player;
            sisterFollow.enabled = true;
        }

    }

    public void SetLilyStaticAndReturnControl()
    {
        if (sister == null) return;

        state = LilyState.Static;
        timer = 0f;
        canSwap = false;
        isPickedUp = false;
        isSisterControlled = false;

        if (sisterFollow != null)
            sisterFollow.enabled = false;

        StopCarryingLily();

        if (playerController != null)
            playerController.canJump = true;

        if (lilyTimerText != null)
            lilyTimerText.gameObject.SetActive(false);

        if (switchManager != null)
            switchManager.SetActiveCharacter(false, snapInactiveSisterNearBrother: false);
    }

    private void MoveCharacterTo(Transform who, Vector3 targetPosition)
    {
        if (who == null) return;

        CharacterController cc = who.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        who.position = targetPosition;

        if (cc != null) cc.enabled = true;
    }

    public bool CanInteractWithLily()
    {
        return state == LilyState.Tired || state == LilyState.Static;
    }

    public string GetLilyPrompt()
    {
        switch (state)
        {
            case LilyState.Tired:
                return "Press E to pick up";
            case LilyState.Static:
                return "Press E to wake Lily";
            default:
                return "";
        }
    }

    public void InteractWithLily()
    {
        switch (state)
        {
            case LilyState.Tired:
                PickUpTiredLily();
                break;
            case LilyState.Static:
                WakeStaticLily();
                break;
        }
    }

    private void PickUpTiredLily()
    {
        isPickedUp = true;
        canSwap = false;

        StartCarryingLily();

        state = LilyState.Recharging;
        timer = rechargeTime;

        if (playerController != null)
            playerController.canJump = false;

        if (cutsceneDialogue != null)
            cutsceneDialogue.SetActive(false);
    }

    private void WakeStaticLily()
    {
        state = LilyState.Active;
        timer = maxActiveTime;
        canSwap = true;
        isPickedUp = false;
        isSisterControlled = false;

        if (sisterFollow != null)
        {
            sisterFollow.target = player;
            sisterFollow.enabled = true;
        }

        if (playerController != null)
            playerController.canJump = true;

        StartDialogue(readyFromStaticMessage, readyFromStaticDuration);
    }

    private void CacheCarriedRechargeReferences()
    {
        if (playerCarryController == null && player != null)
            playerCarryController = player.GetComponentInChildren<PlayerCarryController>();

        if (playerCarryController == null)
            playerCarryController = FindFirstObjectByType<PlayerCarryController>();

        if (lilyPoint == null && playerCarryController != null)
            lilyPoint = playerCarryController.lilyPoint;

        if (lilyPoint == null && player != null)
            lilyPoint = player.Find("LilyPoint");

        if (sawyerAnimator == null && playerController != null)
            sawyerAnimator = playerController.animator;

        if (sawyerAnimator == null && player != null)
            sawyerAnimator = player.GetComponentInChildren<Animator>();

        if (sisterCharacterController == null && sister != null)
            sisterCharacterController = sister.GetComponent<CharacterController>();

        carryingAnimatorParameterExists = HasAnimatorBoolParameter(sawyerAnimator, carryingParameterName);
    }

    private void StartCarryingLily()
    {
        CacheCarriedRechargeReferences();

        if (sisterFollow != null)
            sisterFollow.enabled = false;

        if (sisterCharacterController != null)
            sisterCharacterController.enabled = false;

        lilyIsBeingCarried = true;
        SetCarryingAnimation(true);

        if (sister != null && lilyPoint != null)
        {
            sister.position = lilyPoint.position;
            sister.rotation = lilyPoint.rotation;
        }
    }

    private void StopCarryingLily()
    {
        if (!lilyIsBeingCarried)
        {
            SetCarryingAnimation(false);
            return;
        }

        lilyIsBeingCarried = false;
        SetCarryingAnimation(false);

        if (sisterCharacterController != null)
            sisterCharacterController.enabled = true;

        if (sisterFollow != null && state == LilyState.Active && !isSisterControlled)
        {
            sisterFollow.target = player;
            sisterFollow.enabled = true;
        }
    }

    private void UpdateCarriedLilyPosition()
    {
        if (!lilyIsBeingCarried || sister == null || lilyPoint == null) return;

        float followT = Time.deltaTime * lilyCarryFollowSpeed;
        sister.position = Vector3.Lerp(sister.position, lilyPoint.position, followT);
        sister.rotation = Quaternion.Slerp(sister.rotation, lilyPoint.rotation, followT);
    }

    private void SetCarryingAnimation(bool isCarrying)
    {
        if (sawyerAnimator == null || !carryingAnimatorParameterExists) return;

        sawyerAnimator.SetBool(carryingParameterName, isCarrying);
    }

    private bool HasAnimatorBoolParameter(Animator animator, string parameterName)
    {
        if (animator == null || string.IsNullOrEmpty(parameterName)) return false;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Bool && parameter.name == parameterName)
                return true;
        }

        return false;
    }
}
