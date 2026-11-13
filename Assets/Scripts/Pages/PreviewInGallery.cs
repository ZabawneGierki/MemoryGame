using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class PreviewInGallery : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private CanvasGroup unlockedCanvas;

    [SerializeField] private CanvasGroup lockedCanvas;
    [SerializeField] private InputActionReference cancelAction;
    [SerializeField] private Image previewImage;
    private int currentImageIndex = -1;

    private void Awake()
    {
        // Initialize the canvas groups to be fully transparent at the start
        unlockedCanvas.alpha = 0;
        lockedCanvas.alpha = 0;

        unlockedCanvas.interactable = false;
        lockedCanvas.interactable = false;

        unlockedCanvas.blocksRaycasts = false;
        lockedCanvas.blocksRaycasts = false;
    }
    private void OnEnable()
    {
        GalleryImage.OnImageClickedEvent += HandleImageClicked;
        cancelAction.action.performed += OnCancel;
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        unlockedCanvas.alpha = 0;
        lockedCanvas.alpha = 0;
        unlockedCanvas.interactable = false;
        lockedCanvas.interactable = false;
        unlockedCanvas.blocksRaycasts = false;
        lockedCanvas.blocksRaycasts = false;
    }

    private void HandleImageClicked(int index, bool unlocked)
    {
        currentImageIndex = index;

        if (unlocked)
        {
            unlockedCanvas.DOFade(1, 0.5f);
            lockedCanvas.DOFade(0, 0.5f);
            unlockedCanvas.interactable = true;
            lockedCanvas.interactable = false;
            unlockedCanvas.blocksRaycasts = true;
            lockedCanvas.blocksRaycasts = false;
            previewImage.sprite =  ImageSelector.GetSpriteById(index);
        }
        else
        {
            unlockedCanvas.DOFade(0, 0.5f);
            lockedCanvas.DOFade(1, 0.5f);
            unlockedCanvas.interactable = false;
            lockedCanvas.interactable = true;
            unlockedCanvas.blocksRaycasts = false;
            lockedCanvas.blocksRaycasts = true;
        }
    }

    private void OnDisable()
    {
        GalleryImage.OnImageClickedEvent -= HandleImageClicked;
        cancelAction.action.performed -= OnCancel;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Close all previews when clicking outside of the preview area
        unlockedCanvas.DOFade(0, 0.5f);
        lockedCanvas.DOFade(0, 0.5f);
        unlockedCanvas.interactable = false;
        lockedCanvas.interactable = false;
        unlockedCanvas.blocksRaycasts = false;
        lockedCanvas.blocksRaycasts = false;
        previewImage.sprite = null;
    }

    public void PlayButtonClick()
    {
        GameData.CurrentLevel = currentImageIndex;
        SceneManager.LoadScene(1);
    }


}
