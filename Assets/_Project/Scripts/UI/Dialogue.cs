using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Dialogue : MonoBehaviour
{
    [Header("Dialogue Source")]
    public CutsceneDialogue dialogueUI;

    [TextArea(2, 5)]
    public string[] lines;

    [Header("Input")]
    public bool advanceWithMouse = true;
    public bool advanceWithE = true;
    public bool advanceWithSpace = false;

    [Tooltip("Prevents the first click/press (often leftover input) from instantly skipping line 0.")]
    public float inputBlockTime = 0.15f;

    [Header("Cutscene Camera")]
    public Camera cutsceneCamera;
    public Camera gameplayCamera;

    [Header("Disable During Cutscene (optional)")]
    public MonoBehaviour[] disableWhilePlaying;

    [Header("End Behavior")]
    public bool loadSceneOnFinish = false;
    public string nextSceneName = "GroundFloor";
    public bool destroyAfterFinish = true;

    private int index = 0;
    private bool playing = false;
    private float ignoreInputUntil = 0f;

    void Awake()
    {
        if (dialogueUI == null)
            dialogueUI = FindFirstObjectByType<CutsceneDialogue>(FindObjectsInactive.Include);

        if (cutsceneCamera != null)
            cutsceneCamera.gameObject.SetActive(false);
    }

    void Start()
    {
        Begin();
    }

    void Update()
    {
        if (!playing) return;

        // Guard against stale input on start
        if (Time.unscaledTime < ignoreInputUntil)
            return;

        if (WasAdvancePressed())
            Advance();
    }

    public void Begin()
    {
        if (playing) return;
        if (lines == null || lines.Length == 0) return;
        if (dialogueUI == null) return;

        playing = true;
        index = 0;

        // Block input briefly so line 0 isn't skipped
        ignoreInputUntil = Time.unscaledTime + inputBlockTime;

        // Switch cameras
        if (gameplayCamera != null)
            gameplayCamera.gameObject.SetActive(false);

        if (cutsceneCamera != null)
            cutsceneCamera.gameObject.SetActive(true);

        // Disable gameplay scripts
        if (disableWhilePlaying != null)
        {
            foreach (var mb in disableWhilePlaying)
                if (mb != null) mb.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Show the first line immediately
        dialogueUI.ShowLine(lines[index]);
    }

    void Advance()
    {
        index++;

        if (index >= lines.Length)
        {
            Finish();
            return;
        }

        dialogueUI.ShowLine(lines[index]);
    }

    void Finish()
    {
        playing = false;

        if (dialogueUI != null)
            dialogueUI.Hide();

        if (disableWhilePlaying != null)
        {
            foreach (var mb in disableWhilePlaying)
                if (mb != null) mb.enabled = true;
        }

        if (cutsceneCamera != null)
            cutsceneCamera.gameObject.SetActive(false);

        if (gameplayCamera != null)
            gameplayCamera.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (loadSceneOnFinish && !string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);

        if (destroyAfterFinish)
            Destroy(gameObject);
    }

    bool WasAdvancePressed()
    {
        if (advanceWithMouse && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;

        if (advanceWithE && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            return true;

        if (advanceWithSpace && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            return true;

        return false;
    }
}