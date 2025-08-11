using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // DOTween

public class MemoryCard : MonoBehaviour
{
    public int id;
    public Sprite frontImage;
    public Sprite backImage;

    private Image imageComponent;
    private Button button;
    private bool isRevealed = false;
    private MemoryGameManager gameManager;

    public void Init(MemoryGameManager manager, int id, Sprite front, Sprite back)
    {
        gameManager = manager;
        this.id = id;
        frontImage = front;
        backImage = back;

        imageComponent = GetComponent<Image>();
        button = GetComponent<Button>();

        Hide();
        button.onClick.AddListener(OnClick);
    }

    public void Show()
    {
        isRevealed = true;
        imageComponent.sprite = frontImage;
    }

    public void Hide()
    {
        isRevealed = false;
        imageComponent.sprite = backImage;
    }

    private void OnClick()
    {
        if (!isRevealed && button.interactable)
            gameManager.CardRevealed(this);
    }

    public void Remove()
    {
        button.interactable = false;

        // Fade out over 0.5s
        imageComponent.DOFade(0f, 0.5f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // keep it transparent so layout stays intact
                imageComponent.raycastTarget = false;
            });
    }
}
