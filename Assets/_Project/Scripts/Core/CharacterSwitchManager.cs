// CharacterSwitchManager.cs
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
    public ThirdPersonCameraFollow cameraFollow; // on CameraRig

    [Header("Sister follow behavior when inactive")]
    public FollowTarget sisterFollow;

    [Header("Lily Meter")]
    public LilyMeter lilyMeter;

    [Header("Input")]
    [Tooltip("This field is deprecated - using Keyboard directly for T key")]
    public KeyCode switchKey = KeyCode.T;

    private bool sisterIsActive = false;
    private bool isInitialized = false;

    void Awake()
    {
        // Optionally auto-assign lilyMeter if not set
        if (lilyMeter == null)
            lilyMeter = FindObjectOfType<LilyMeter>();
    }

    void Start()
    {
        // Default: brother active at start
        SetActiveCharacter(isSisterActive: false);
        isInitialized = true;
    }

    void Update()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            // Only allow swap to sister if LilyMeter allows
            if (!sisterIsActive && lilyMeter != null && !lilyMeter.CanSwapToSister())
                return;

            SetActiveCharacter(!sisterIsActive);
        }
    }

    public void SetActiveCharacter(bool isSisterActive)
    {
        sisterIsActive = isSisterActive;

        // Enable only the active controller
        if (brotherController != null) brotherController.enabled = !sisterIsActive;
        if (sisterController != null) sisterController.enabled = sisterIsActive;

        // Enable only the active interactor (prevents prompt flicker/disable fights)
        if (brotherInteractor != null) brotherInteractor.enabled = !sisterIsActive;
        if (sisterInteractor != null) sisterInteractor.enabled = sisterIsActive;

        // Camera follows active character
        if (cameraFollow != null)
            cameraFollow.target = sisterIsActive ? sister : brother;

        // Sister follows ONLY when she is NOT active
        if (sisterFollow != null)
        {
            sisterFollow.enabled = !sisterIsActive;
            sisterFollow.target = brother; // per your spec: she follows the brother
        }

        // Optional: snap sister near brother when switching away from her DURING GAMEPLAY
        // (prevents her being left behind if you switched while far away)
        // BUT: only do this if game is already initialized, not on scene startup
        if (!sisterIsActive && sister != null && brother != null && isInitialized)
        {
            // Keep her close-ish, but not overlapping
            Vector3 behind = -brother.forward * 1.0f + brother.right * 0.6f;
            sister.position = brother.position + behind;
        }

        // Notify LilyMeter of control change (BOTH directions)
        if (lilyMeter != null)
        {
            if (sisterIsActive)
                lilyMeter.OnSisterActivated();
            else
                lilyMeter.OnPlayerActivated();
        }
    }

    // Called by LilyMeter to force swap to player
    public void ForceSwitchToPlayer()
    {
        if (sisterIsActive)
            SetActiveCharacter(false);
    }
}