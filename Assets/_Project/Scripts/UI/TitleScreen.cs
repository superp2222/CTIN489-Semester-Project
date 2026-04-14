using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    [Header("Optional Button References")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button quitButton;

    [Header("Screen Fader")]
    [SerializeField] private ScreenFader screenFader;

    [Header("Scene Settings")]
    [SerializeField] private string cutsceneSceneName = "Cutscene";

    [Header("Controls Menu")]
    [SerializeField] private ControlsMenuUI controlsMenuUI;

    private void Awake()
    {
        if (playButton != null)
            playButton.onClick.AddListener(Play);

        if (controlsButton != null)
            controlsButton.onClick.AddListener(Controls);

        if (quitButton != null)
            quitButton.onClick.AddListener(Quit);
    }

    public void Play()
    {
        Time.timeScale = 1f;

        if (screenFader != null)
        {
            StartCoroutine(PlayRoutine());
        }
        else
        {
            SceneManager.LoadScene(cutsceneSceneName);
        }
    }

    private IEnumerator PlayRoutine()
    {
        screenFader.FadeOut();
        yield return new WaitForSeconds(screenFader.defaultDuration);
        SceneManager.LoadScene(cutsceneSceneName);
    }

    public void Controls()
    {
        if (controlsMenuUI != null)
            controlsMenuUI.OpenMenu();
    }

    public void Quit()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}