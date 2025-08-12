using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MemoryCard : MonoBehaviour
{
    public int id;
    public Sprite frontImage;
    public Sprite backImage;

    private Image imageComponent;
    private Button button;
    private bool isRevealed = false;
    private MemoryGameManager gameManager;

    private bool isFlipping = false;

    public void Init(MemoryGameManager manager, int id, Sprite front, Sprite back)
    {
        gameManager = manager;
        this.id = id;
        frontImage = front;
        backImage = back;

        imageComponent = GetComponent<Image>();
        button = GetComponent<Button>();

        HideInstant();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (!isRevealed && button.interactable && !isFlipping)
            gameManager.CardRevealed(this);
    }

    public void Show()
    {
        FlipTo(frontImage, true);
    }

    public void Hide()
    {
        FlipTo(backImage, false);
    }

    public void HideInstant()
    {
        isRevealed = false;
        imageComponent.sprite = backImage;
    }

    private void FlipTo(Sprite newSprite, bool revealed)
    {
        isFlipping = true;
        // Rotate halfway
        transform.DORotate(new Vector3(0, 90, 0), 0.15f).SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                imageComponent.sprite = newSprite;
                isRevealed = revealed;

                // Rotate back
                transform.DORotate(Vector3.zero, 0.15f).SetEase(Ease.OutQuad)
                    .OnComplete(() => isFlipping = false);
            });
    }

    public void Remove()
    {
        button.interactable = false;

        // Fade out the front image over 0.5s
        imageComponent.DOFade(0f, 0.5f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                imageComponent.raycastTarget = false;
            });
    }
}
