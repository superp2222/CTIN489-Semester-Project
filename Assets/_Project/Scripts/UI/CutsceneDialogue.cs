using UnityEngine;
using TMPro;

public class CutsceneDialogue : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBox; // the panel
    [SerializeField] private TMP_Text dialogueText;

    void Awake()
    {
        if (dialogueBox != null) dialogueBox.SetActive(false);
    }

    public void ShowLine(string line)
    {
        if (dialogueBox != null) dialogueBox.SetActive(true);
        if (dialogueText != null) dialogueText.text = line;
    }

    public void Hide()
    {
        if (dialogueBox != null) dialogueBox.SetActive(false);
    }
}
