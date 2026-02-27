using System.Collections;
using UnityEngine;
using TMPro;

public class ControlsHint : MonoBehaviour
{
    [SerializeField] private TMP_Text controlsText;
    [SerializeField] private GameObject panelRoot; // ControlsPanel
    [TextArea] public string message;
    public bool fadeAway = true;
    public float showDuration = 3.0f;

    void Start()
    {
        if (panelRoot == null && controlsText != null)
            panelRoot = controlsText.transform.parent.gameObject;

        Show();
    }

    public void Show()
    {
        if (controlsText == null) return;

        if (panelRoot != null) panelRoot.SetActive(true);
        controlsText.text = message;

        StopAllCoroutines();
        if(fadeAway)StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(showDuration);
        if (panelRoot != null) panelRoot.SetActive(false);
    }
}
