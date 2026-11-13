using DG.Tweening;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

public class MenuPageManager : MonoBehaviour
{
    [SerializeField] private MenuPage[] menuPages;
    [SerializeField] private InputActionReference cancelAction;
    [SerializeField] private float animationDuration = 0.35f;
    [SerializeField] private float pageOffset = 1000f;
    [SerializeField] private Ease animationEase = Ease.OutCubic;

    private readonly Stack<MenuPage> pageHistory = new();
    private readonly Dictionary<MenuPage, Vector2> pagePositions = new();

    private MenuPage currentPage;
    private bool isAnimating;

    private void Awake()
    {
        CachePagePositions();
        HideAllPages();

        MenuPage mainPage = FindPage(MenuPageName.MainMenu);

        if (mainPage == null)
        {
            Debug.LogError("The menu does not contain a valid page named MainMenu.", this);
            return;
        }

        currentPage = mainPage;
        SetPageVisible(currentPage);
        currentPage.page.OnPageEnter();
    }

    private void OnEnable()
    {
        if (cancelAction == null || cancelAction.action == null)
        {
            return;
        }

        cancelAction.action.performed += OnCancelPerformed;
        cancelAction.action.Enable();
    }

    private void OnDisable()
    {
        if (cancelAction == null || cancelAction.action == null)
        {
            return;
        }

        cancelAction.action.performed -= OnCancelPerformed;
        cancelAction.action.Disable();
    }

    public void FinishGame()
    {
        Application.Quit();
    }

    public void ShowMainMenu()
    {
        ShowPage(MenuPageName.MainMenu);
    }

    
    public void ShowGallery()
    {
        ShowPage(MenuPageName.Gallery);
    }



    public void ShowGameSettings()
    {
        ShowPage(MenuPageName.Settings);
    }

    public void ShowPage(MenuPageName pageName)
    {
        MenuPage nextPage = FindPage(pageName);

        if (nextPage == null)
        {
            Debug.LogWarning($"No valid menu page was found for {pageName}.", this);
            return;
        }

        ShowPage(nextPage);
    }

    public void GoBack()
    {
        if (isAnimating || currentPage == null || currentPage.pageName == MenuPageName.MainMenu)
        {
            return;
        }

        if (pageHistory.Count == 0)
        {
            return;
        }

        MenuPage previousPage = pageHistory.Pop();

        AnimatePageChange(previousPage, false);
    }

    private void ShowPage(MenuPage nextPage)
    {
        if (isAnimating || nextPage == currentPage)
        {
            return;
        }

        if (nextPage.pageName == MenuPageName.MainMenu)
        {
            pageHistory.Clear();
        }
        else if (currentPage != null)
        {
            pageHistory.Push(currentPage);
        }

        AnimatePageChange(nextPage, true);
    }

    private void AnimatePageChange(MenuPage nextPage, bool movingForward)
    {
        if (currentPage == null)
        {
            currentPage = nextPage;
            SetPageVisible(currentPage);
            currentPage.page.OnPageEnter();
            return;
        }

        isAnimating = true;

        MenuPage outgoingPage = currentPage;
        RectTransform outgoingTransform = GetPageTransform(outgoingPage);
        RectTransform incomingTransform = GetPageTransform(nextPage);
        CanvasGroup outgoingCanvasGroup = outgoingPage.page.CanvasGroup;
        CanvasGroup incomingCanvasGroup = nextPage.page.CanvasGroup;

        Vector2 outgoingPosition = pagePositions[outgoingPage];
        Vector2 incomingPosition = pagePositions[nextPage];

        Vector2 outgoingTargetPosition = outgoingPosition +
            (movingForward ? Vector2.left : Vector2.right) * pageOffset;

        Vector2 incomingStartPosition = incomingPosition +
            (movingForward ? Vector2.right : Vector2.left) * pageOffset;

        outgoingCanvasGroup.interactable = false;
        outgoingCanvasGroup.blocksRaycasts = false;

        incomingCanvasGroup.alpha = 0f;
        incomingCanvasGroup.interactable = false;
        incomingCanvasGroup.blocksRaycasts = false;
        incomingTransform.anchoredPosition = incomingStartPosition;

        outgoingPage.page.OnPageExit();
        nextPage.page.OnPageEnter();

        Sequence transition = DOTween.Sequence();

        transition.Join(
            outgoingTransform
                .DOAnchorPos(outgoingTargetPosition, animationDuration)
                .SetEase(animationEase));

        transition.Join(
            outgoingCanvasGroup
                .DOFade(0f, animationDuration)
                .SetEase(animationEase));

        transition.Join(
            incomingTransform
                .DOAnchorPos(incomingPosition, animationDuration)
                .SetEase(animationEase));

        transition.Join(
            incomingCanvasGroup
                .DOFade(1f, animationDuration)
                .SetEase(animationEase));

        transition.OnComplete(() =>
        {
            outgoingTransform.anchoredPosition = outgoingPosition;
            SetPageHidden(outgoingPage);

            SetPageVisible(nextPage);

            currentPage = nextPage;
            isAnimating = false;
        });
    }

    private void CachePagePositions()
    {
        pagePositions.Clear();

        foreach (MenuPage page in menuPages)
        {
            if (page == null || page.page == null)
            {
                continue;
            }

            pagePositions[page] = GetPageTransform(page).anchoredPosition;
        }
    }

    private void HideAllPages()
    {
        foreach (MenuPage page in menuPages)
        {
            if (page == null || page.page == null)
            {
                continue;
            }

            RectTransform pageTransform = GetPageTransform(page);

            pageTransform.DOKill();
            page.page.CanvasGroup.DOKill();

            pageTransform.anchoredPosition = pagePositions[page];
            SetPageHidden(page);
        }
    }

    private static void SetPageVisible(MenuPage page)
    {
        CanvasGroup canvasGroup = page.page.CanvasGroup;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private static void SetPageHidden(MenuPage page)
    {
        CanvasGroup canvasGroup = page.page.CanvasGroup;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private MenuPage FindPage(MenuPageName pageName)
    {
        foreach (MenuPage page in menuPages)
        {
            if (page != null && page.page != null && page.pageName == pageName)
            {
                return page;
            }
        }

        return null;
    }

    private static RectTransform GetPageTransform(MenuPage page)
    {
        return page.page.transform as RectTransform;
    }

    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        GoBack();
    }
}

public enum MenuPageName
{
    MainMenu,
    Settings,
    Gallery

}

[System.Serializable]
public class MenuPage
{
    public Page page;
    public MenuPageName pageName;
}