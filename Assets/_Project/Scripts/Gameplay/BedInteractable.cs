using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BedInteractable : MonoBehaviour, IInteractable
{
    [Header("UI")]
    public GameObject titleCardPanel;   // TitleCardPanel
    public float titleDuration = 3.0f;

    [Header("Fade")]
    public ScreenFader fader;
    public float fadeTime = 1.5f;


    [Header("Scene")]
    public string sceneToLoad = "Room";

    [Header("Optional: disable these while sleeping")]
    public MonoBehaviour[] disableOnSleep;

    private bool used = false;

    public void Interact()
    {
        if (used) return;
        used = true;

        // lock player input so they can't move during title
        if (disableOnSleep != null)
            foreach (var c in disableOnSleep)
                if (c != null) c.enabled = false;

        StartCoroutine(SleepSequence());
    }

    private IEnumerator SleepSequence()
    {
        if (fader != null)
            fader.FadeOut(fadeTime);

        yield return new WaitForSeconds(fadeTime);

        if (titleCardPanel != null)
            titleCardPanel.SetActive(true);

        yield return new WaitForSeconds(titleDuration);

        SceneManager.LoadScene(sceneToLoad);
    }


    public string GetPrompt()
    {
        return used ? "" : "Press E to Sleep";
    }
}
