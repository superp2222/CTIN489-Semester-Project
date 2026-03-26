using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterSwitchManager : MonoBehaviour
{
    [Header("References")]
    public Transform brother;
    public Transform sister;

    [Header("Components to enable/disable on control switch")]
    public PlayerController brotherController;
    public SisterController sisterController;

    [Header("Interactors (only active character should interact)")]
    public PlayerInteractor brotherInteractor;
    public PlayerInteractor sisterInteractor;

    [Header("Camera Rig")]
    public ThirdPersonCameraFollow cameraFollow;

    [Header("Sister follow behavior when inactive")]
    public FollowTarget sisterFollow;

    [Header("Lily Meter")]
    public LilyMeter lilyMeter;

    private bool sisterIsActive = false;
    private bool isInitialized = false;

    void Awake()
    {
        if (lilyMeter == null)
            lilyMeter = FindAnyObjectByType<LilyMeter>();
    }

    void Start()
    {
        SetActiveCharacter(false, snapInactiveSisterNearBrother: false);
        isInitialized = true;
    }

    void Update()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            if (!sisterIsActive && lilyMeter != null && !lilyMeter.CanSwapToSister())
                return;

            SetActiveCharacter(!sisterIsActive, snapInactiveSisterNearBrother: true);
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            if (lilyMeter == null) return;

            if (sisterIsActive)
            {
                // Lily is controlled: go Static and return control to Sawyer
                lilyMeter.SetLilyStaticAndReturnControl();
            }
            else
            {
                // Sawyer is controlled: recall Lily to his side
                lilyMeter.RecallLilyToPlayer();
            }
        }
    }

    public void SetActiveCharacter(bool isSisterActive, bool snapInactiveSisterNearBrother = true)
    {
        sisterIsActive = isSisterActive;

        if (brotherController != null) brotherController.enabled = !sisterIsActive;
        if (sisterController != null) sisterController.enabled = sisterIsActive;

        if (brotherInteractor != null) brotherInteractor.enabled = !sisterIsActive;
        if (sisterInteractor != null) sisterInteractor.enabled = sisterIsActive;

        if (cameraFollow != null)
            cameraFollow.target = sisterIsActive ? sister : brother;

        if (sisterFollow != null)
        {
            sisterFollow.enabled = !sisterIsActive;
            sisterFollow.target = brother;
        }

        if (!sisterIsActive && sister != null && brother != null && isInitialized && snapInactiveSisterNearBrother)
        {
            Vector3 behind = -brother.forward * 1.0f + brother.right * 0.6f;
            sister.position = brother.position + behind;
        }

        if (lilyMeter != null)
        {
            if (sisterIsActive)
                lilyMeter.OnSisterActivated();
            else
                lilyMeter.OnPlayerActivated();
        }
    }

    public void ForceSwitchToPlayer(bool snapInactiveSisterNearBrother = true)
    {
        if (sisterIsActive)
            SetActiveCharacter(false, snapInactiveSisterNearBrother);
    }
}