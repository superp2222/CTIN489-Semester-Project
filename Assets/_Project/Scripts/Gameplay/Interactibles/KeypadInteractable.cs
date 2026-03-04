using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class KeypadInteractable : MonoBehaviour, IInteractable
{
    [Header("Prompt")]
    public string promptText = "Press E to use keypad";

    [Header("Code")]
    [Tooltip("Exactly 4 digits, e.g. 0420")]
    public string correctCode = "1234";
    public int codeLength = 4;

    [Header("Door")]
    public KeypadDoor targetDoor;

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

    [Header("Behavior")]
    public bool maskInput = true;
    public bool closeOnSuccess = true;
    public bool allowCancelWithEscape = true;

    private string entered = "";
    private bool uiOpen = false;

    void Awake()
    {
    }

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
        // If the door is already open, keypad does nothing (optional).
        if (targetDoor != null && targetDoor.IsOpen())
            return;

        if (!uiOpen) OpenUI();
        else CloseUI();
    }

    public string GetPrompt()
    {
        return promptText;
    }

    public void PressDigit(string digit)
    {
        if (!uiOpen) return;
        if (entered.Length >= codeLength) return;

        // Only accept 0-9
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

            if (targetDoor != null)
                targetDoor.OpenDoor();

            // Door cannot be closed, and keypad becomes “done”
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

        // Disable chosen scripts (movement, camera look, etc.)
        if (disableWhileOpen != null)
        {
            foreach (var mb in disableWhileOpen)
            {
                if (mb != null) mb.enabled = false;
            }
        }

        // Cursor for UI
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

        // Re-enable scripts
        if (disableWhileOpen != null)
        {
            foreach (var mb in disableWhileOpen)
            {
                if (mb != null) mb.enabled = true;
            }
        }

        // Cursor back to gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        entered = "";
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

        // Masked as **** but only show how many typed
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