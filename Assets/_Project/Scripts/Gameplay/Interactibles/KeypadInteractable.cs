using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class KeypadInteractable : MonoBehaviour, IInteractable
{
    [Header("Prompt")]
    public string promptText = "Press E to use keypad";

    [Header("Code")]
    [Tooltip("Exactly 4 digits, e.g. 0420")]
    public string correctCode = "1234";
    public int codeLength = 4;

    [Header("Doors")]
    [Tooltip("All doors that should open when the correct code is entered.")]
    public KeypadDoor[] targetDoors;

    [Header("UI Root (Panel)")]
    [Tooltip("A Canvas/Panel GameObject that contains the keypad UI. This will be enabled/disabled.")]
    public GameObject keypadUIRoot;

    [Header("UI Display")]
    [Tooltip("TMP text that shows entered digits (often masked as ****).")]
    public TMP_Text inputDisplay;

    [Tooltip("Optional TMP text for feedback like 'ACCESS GRANTED' / 'WRONG CODE'")]
    public TMP_Text statusText;

    [Header("Optional: Disable player controls while using keypad")]
    [Tooltip("Drag scripts here to disable while keypad is open (e.g., PlayerController, ThirdPersonController, PlayerInteractor, etc.).")]
    public MonoBehaviour[] disableWhileOpen;

    [Header("Pause Menu CanvasGroup")]
    [Tooltip("Assign the CanvasGroup used by the pause menu so keypad can disable its interaction while open.")]
    public CanvasGroup pauseMenuCanvasGroup;

    [Header("Behavior")]
    public bool maskInput = true;
    public bool closeOnSuccess = true;
    public bool allowCancelWithEscape = true;

    private string entered = "";
    private bool uiOpen = false;

    // Save old pause menu state so we can restore it properly
    private bool savedPauseInteractable;
    private bool savedPauseBlocksRaycasts;
    private float savedPauseAlpha;

    public bool IsOpen => uiOpen;

    void Start()
    {
        if (keypadUIRoot != null)
            keypadUIRoot.SetActive(false);

        RefreshDisplay();
        ClearStatus();
    }

    void Update()
    {
        if (!uiOpen) return;

        if (allowCancelWithEscape && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseUI();
        }
    }

    public void Interact()
    {
        // Optional: do nothing if door already opened
        if (targetDoors != null && targetDoors.Length > 0)
        {
            bool anyDoor = false;
            bool allOpen = true;

            foreach (var door in targetDoors)
            {
                if (door == null) continue;
                anyDoor = true;
                if (!door.IsOpen())
                {
                    allOpen = false;
                    break;
                }
            }

            if (anyDoor && allOpen)
                return;
        }

        if (!uiOpen)
            OpenUI();
        else
            CloseUI();
    }

    public string GetPrompt()
    {
        return promptText;
    }

    public void PressDigit(string digit)
    {
        if (!uiOpen) return;
        if (entered.Length >= codeLength) return;

        if (digit.Length != 1 || digit[0] < '0' || digit[0] > '9') return;

        entered += digit;
        RefreshDisplay();
        ClearStatus();
    }

    public void Backspace()
    {
        if (!uiOpen) return;
        if (entered.Length == 0) return;

        entered = entered.Substring(0, entered.Length - 1);
        RefreshDisplay();
        ClearStatus();
    }

    public void ClearAll()
    {
        if (!uiOpen) return;

        entered = "";
        RefreshDisplay();
        ClearStatus();
    }

    public void Enter()
    {
        if (!uiOpen) return;

        if (entered.Length != codeLength)
        {
            SetStatus("ENTER 4 DIGITS");
            return;
        }

        if (entered == correctCode)
        {
            SetStatus("ACCESS GRANTED");

            if (targetDoors != null)
            {
                foreach (var door in targetDoors)
                {
                    if (door != null)
                        door.OpenDoor();
                }
            }

            if (closeOnSuccess)
                CloseUI();
        }
        else
        {
            SetStatus("WRONG CODE");
            entered = "";
            RefreshDisplay();
        }
    }

    private void OpenUI()
    {
        uiOpen = true;

        if (keypadUIRoot != null)
            keypadUIRoot.SetActive(true);

        // Disable chosen scripts
        if (disableWhileOpen != null)
        {
            foreach (var mb in disableWhileOpen)
            {
                if (mb != null)
                    mb.enabled = false;
            }
        }

        // Temporarily disable pause menu interaction/raycast blocking
        if (pauseMenuCanvasGroup != null)
        {
            savedPauseAlpha = pauseMenuCanvasGroup.alpha;
            savedPauseInteractable = pauseMenuCanvasGroup.interactable;
            savedPauseBlocksRaycasts = pauseMenuCanvasGroup.blocksRaycasts;

            pauseMenuCanvasGroup.interactable = false;
            pauseMenuCanvasGroup.blocksRaycasts = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        entered = "";
        RefreshDisplay();
        ClearStatus();
    }

    private void CloseUI()
    {
        uiOpen = false;

        if (keypadUIRoot != null)
            keypadUIRoot.SetActive(false);

        // Re-enable chosen scripts
        if (disableWhileOpen != null)
        {
            foreach (var mb in disableWhileOpen)
            {
                if (mb != null)
                    mb.enabled = true;
            }
        }

        // Restore pause menu CanvasGroup state
        if (pauseMenuCanvasGroup != null)
        {
            pauseMenuCanvasGroup.alpha = savedPauseAlpha;
            pauseMenuCanvasGroup.interactable = savedPauseInteractable;
            pauseMenuCanvasGroup.blocksRaycasts = savedPauseBlocksRaycasts;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        entered = "";
        ClearAll();
        RefreshDisplay();
        ClearStatus();
    }

    private void RefreshDisplay()
    {
        if (inputDisplay == null) return;

        if (!maskInput)
        {
            inputDisplay.text = entered;
            return;
        }

        inputDisplay.text = new string('*', entered.Length);
    }

    private void SetStatus(string msg)
    {
        if (statusText == null) return;

        statusText.gameObject.SetActive(true);
        statusText.text = msg;
    }

    private void ClearStatus()
    {
        if (statusText == null) return;

        statusText.text = "";
        statusText.gameObject.SetActive(false);
    }
}
