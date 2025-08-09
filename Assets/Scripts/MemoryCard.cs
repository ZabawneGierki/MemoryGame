using UnityEngine;
using UnityEngine.UI;

public class MemoryCard : MonoBehaviour
{
    public int id; // unique ID for matching
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
        if (!isRevealed)
            gameManager.CardRevealed(this);
    }

    public void Remove()
    {
        gameObject.SetActive(false);
    }
}
