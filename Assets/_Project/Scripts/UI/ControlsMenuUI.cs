using UnityEngine;
using UnityEngine.UI;

public class ControlsMenuUI : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private CanvasGroup mainMenuGroup;

    [Header("Controls Menu Root")]
    [SerializeField] private CanvasGroup controlsMenuGroup;

    [Header("Pages")]
    [SerializeField] private GameObject[] pages;

    [Header("Navigation Buttons")]
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button closeButton;

    private int currentPage = 0;

    private void Awake()
    {
        if (leftButton != null)
            leftButton.onClick.AddListener(PreviousPage);

        if (rightButton != null)
            rightButton.onClick.AddListener(NextPage);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseMenu);

        HideControlsMenuImmediate();
    }

    public void OpenMenu()
    {
        currentPage = 0;

        SetCanvasGroup(mainMenuGroup, false);
        SetCanvasGroup(controlsMenuGroup, true);

        RefreshPage();
    }

    public void CloseMenu()
    {
        SetCanvasGroup(controlsMenuGroup, false);
        SetCanvasGroup(mainMenuGroup, true);
    }

    public void NextPage()
    {
        if (currentPage < pages.Length - 1)
        {
            currentPage++;
            RefreshPage();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            RefreshPage();
        }
    }

    private void RefreshPage()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
                pages[i].SetActive(i == currentPage);
        }

        bool isFirstPage = currentPage == 0;
        bool isLastPage = currentPage == pages.Length - 1;

        if (leftButton != null)
            leftButton.gameObject.SetActive(!isFirstPage);

        if (rightButton != null)
            rightButton.gameObject.SetActive(!isLastPage);

        if (closeButton != null)
            closeButton.gameObject.SetActive(isLastPage);
    }

    private void HideControlsMenuImmediate()
    {
        SetCanvasGroup(controlsMenuGroup, false);

        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
                pages[i].SetActive(false);
        }

        if (closeButton != null)
            closeButton.gameObject.SetActive(false);
    }

    private void SetCanvasGroup(CanvasGroup group, bool visible)
    {
        if (group == null) return;

        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
    }
}