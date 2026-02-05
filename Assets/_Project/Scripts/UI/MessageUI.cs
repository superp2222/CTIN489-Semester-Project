using System.Collections;
using UnityEngine;
using TMPro;

public class MessageUI : MonoBehaviour
{
    public static MessageUI Instance { get; private set; }

    [SerializeField] private TMP_Text messageText;
    private Coroutine currentRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (messageText != null)
            messageText.text = "";
    }

    public void ShowMessage(string message, float durationSeconds = 2.5f)
    {
        if (messageText == null)
        {
            Debug.LogWarning("MessageUI: messageText not assigned.");
            return;
        }

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowRoutine(message, durationSeconds));
    }

    private IEnumerator ShowRoutine(string message, float durationSeconds)
    {
        messageText.text = message;
        yield return new WaitForSeconds(durationSeconds);
        messageText.text = "";
        currentRoutine = null;
    }
}