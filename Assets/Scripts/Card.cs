using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerClickHandler
{
    public int cardID;
    public Image cardFace;
    public Image cardBack;
    public float flipDuration = 0.5f;

    private bool isFlipped = false;
    private bool isMatched = false;
    private Quaternion faceUpRotation = Quaternion.Euler(0, 180, 0);
    private Quaternion faceDownRotation = Quaternion.Euler(0, 0, 0);

    public void Initialize(int id, Sprite faceSprite)
    {
        cardID = id;
        cardFace.sprite = faceSprite;
        ResetCard();
    }

    public void ResetCard()
    {
        isFlipped = false;
        isMatched = false;
        cardFace.gameObject.SetActive(false);
        transform.rotation = faceDownRotation;
        cardBack.gameObject.SetActive(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isFlipped && !isMatched && !GameManager.Instance.isCheckingMatch)
        {
            FlipCard();
            GameManager.Instance.CardClicked(this);
        }
    }

    public void FlipCard()
    {
        if (!isFlipped)
        {
            isFlipped = true;
            StartCoroutine(FlipAnimation(true));
        }
        else
        {
            isFlipped = false;
            StartCoroutine(FlipAnimation(false));
        }
    }

    private System.Collections.IEnumerator FlipAnimation(bool showFace)
    {
        float elapsedTime = 0f;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = showFace ? faceUpRotation : faceDownRotation;

        while (elapsedTime < flipDuration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / flipDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation;

        if (showFace)
        {
            cardBack.gameObject.SetActive(false);
            cardFace.gameObject.SetActive(true);
        }
        else
        {
            cardFace.gameObject.SetActive(false);
            cardBack.gameObject.SetActive(true);
        }
    }

    public void SetMatched()
    {
        isMatched = true;
        // Optional: Add a special effect for matched cards
        Color color = cardFace.color;
        color.a = 0.7f;
        cardFace.color = color;
    }
}
