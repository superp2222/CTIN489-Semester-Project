using System.Collections;
using UnityEngine;
using TMPro;

public class ControlsHint : MonoBehaviour
{
    [Header("Enable / Disable")]
    [Tooltip("If false, this hint will never appear in this scene.")]
    public bool showHintInScene = true;

    [SerializeField] private TMP_Text controlsText;
    [SerializeField] private GameObject panelRoot; // ControlsPanel

    [Header("Hint Settings")]
    [TextArea] public string message;
    public bool fadeAway = true;
    public float showDuration = 3.0f;

    void Start()
    {
        if (panelRoot == null && controlsText != null)
            panelRoot = controlsText.transform.parent.gameObject;

        // If disabled, ensure the panel never appears
        if (!showHintInScene)
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
            return;
        }

        Show();
    }

    public void Show()
    {
        if (!showHintInScene) return;
        if (controlsText == null) return;

        if (panelRoot != null)
            panelRoot.SetActive(true);

        controlsText.text = message;

        StopAllCoroutines();

        if (fadeAway)
            StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(showDuration);

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }
}