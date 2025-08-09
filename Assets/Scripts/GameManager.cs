using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GridLayoutGroup gridLayout;
    public GameObject cardPrefab;
    public Sprite[] cardFaces;
    public Image hiddenImage;
    public float revealDuration = 1.0f;

    public bool isCheckingMatch { get; private set; } = false;

    private List<Card> cards = new List<Card>();
    private List<Card> flippedCards = new List<Card>();
    private int pairsFound = 0;
    private int totalPairs;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        totalPairs = cardFaces.Length;
        hiddenImage.gameObject.SetActive(false);
        SetupGame();
    }

    private void SetupGame()
    {
        // Create pairs of cards
        List<Sprite> cardSprites = new List<Sprite>();
        for (int i = 0; i < cardFaces.Length; i++)
        {
            cardSprites.Add(cardFaces[i]);
            cardSprites.Add(cardFaces[i]); // Add each sprite twice to make pairs
        }

        // Shuffle the cards
        for (int i = 0; i < cardSprites.Count; i++)
        {
            Sprite temp = cardSprites[i];
            int randomIndex = Random.Range(i, cardSprites.Count);
            cardSprites[i] = cardSprites[randomIndex];
            cardSprites[randomIndex] = temp;
        }

        // Create card objects
        for (int i = 0; i < cardSprites.Count; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, gridLayout.transform);
            Card card = cardObj.GetComponent<Card>();

            // Find the card face image (it's a child of the card)
            Image cardFace = cardObj.transform.GetChild(0).GetComponent<Image>();
            card.cardFace = cardFace;

            // Initialize the card with its ID and face sprite
            int cardID = System.Array.IndexOf(cardFaces, cardSprites[i]);
            card.Initialize(cardID, cardSprites[i]);

            cards.Add(card);
        }
    }

    public void CardClicked(Card clickedCard)
    {
        if (flippedCards.Count < 2)
        {
            flippedCards.Add(clickedCard);

            if (flippedCards.Count == 2)
            {
                isCheckingMatch = true;
                Invoke("CheckMatch", 1.0f);
            }
        }
    }

    private void CheckMatch()
    {
        if (flippedCards[0].cardID == flippedCards[1].cardID)
        {
            // Match found
            flippedCards[0].SetMatched();
            flippedCards[1].SetMatched();
            pairsFound++;

            if (pairsFound == totalPairs)
            {
                // All pairs found, reveal hidden image
                Invoke("RevealHiddenImage", 0.5f);
            }
        }
        else
        {
            // No match, flip cards back
            flippedCards[0].FlipCard();
            flippedCards[1].FlipCard();
        }

        flippedCards.Clear();
        isCheckingMatch = false;
    }

    private void RevealHiddenImage()
    {
        hiddenImage.gameObject.SetActive(true);
        StartCoroutine(FadeInImage());
    }

    private System.Collections.IEnumerator FadeInImage()
    {
        float elapsedTime = 0f;
        Color color = hiddenImage.color;
        color.a = 0f;
        hiddenImage.color = color;

        while (elapsedTime < revealDuration)
        {
            color.a = Mathf.Lerp(0f, 1f, elapsedTime / revealDuration);
            hiddenImage.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        color.a = 1f;
        hiddenImage.color = color;
    }

    public void ResetGame()
    {
        pairsFound = 0;
        flippedCards.Clear();
        hiddenImage.gameObject.SetActive(false);

        foreach (Card card in cards)
        {
            card.ResetCard();
        }

        // Reshuffle cards
        List<Card> cardList = new List<Card>(cards);
        for (int i = 0; i < cardList.Count; i++)
        {
            Card temp = cardList[i];
            int randomIndex = Random.Range(i, cardList.Count);
            cardList[i] = cardList[randomIndex];
            cardList[randomIndex] = temp;
        }

        // Update the card order in the grid
        for (int i = 0; i < cardList.Count; i++)
        {
            cardList[i].transform.SetSiblingIndex(i);
        }
    }
}