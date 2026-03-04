using UnityEngine;
using TMPro;
using System.Collections;

public class CutsceneDialogue : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBox; // the panel
    [SerializeField] private TMP_Text dialogueText;

    private Coroutine currentRoutine;

    void Awake()
    {
        if (dialogueBox != null) dialogueBox.SetActive(false);
    }

    public void ShowLine(string line)
    {
        // Stop any timed hide currently running
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        if (dialogueBox != null) dialogueBox.SetActive(true);
        if (dialogueText != null) dialogueText.text = line;
    }

    public void ShowLineForSeconds(string line, float seconds)
    {
        ShowLine(line);

        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(HideAfter(seconds));
    }

    public void Hide()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        if (dialogueBox != null) dialogueBox.SetActive(false);
    }

    private IEnumerator HideAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Hide();
    }
}