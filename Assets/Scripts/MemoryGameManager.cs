using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MemoryGameManager : MonoBehaviour
{
    public Sprite[] cardImages; // pair images
    public Sprite cardBack;
    public GameObject cardPrefab; 
    public Transform gridParent; // where cards go

    private MemoryCard firstRevealed;
    private MemoryCard secondRevealed;
    private int totalPairs;
    private int pairsFound;

    void Start()
    {
        SetupBoard();
    }

    void SetupBoard()
    {
        List<int> ids = new List<int>();

        // create pairs (2 of each)
        for (int i = 0; i < cardImages.Length; i++)
        {
            ids.Add(i);
            ids.Add(i);
        }

        // shuffle
        for (int i = 0; i < ids.Count; i++)
        {
            int rand = Random.Range(i, ids.Count);
            (ids[i], ids[rand]) = (ids[rand], ids[i]);
        }

        totalPairs = cardImages.Length;
        pairsFound = 0;

        // spawn cards
        foreach (var id in ids)
        {
            GameObject cardObj = Instantiate(cardPrefab, gridParent);
            MemoryCard card = cardObj.GetComponent<MemoryCard>();
            card.Init(this, id, cardImages[id], cardBack);
        }
    }

    public void CardRevealed(MemoryCard card)
    {
        if (firstRevealed == null)
        {
            firstRevealed = card;
            card.Show();
        }
        else if (secondRevealed == null && card != firstRevealed)
        {
            secondRevealed = card;
            card.Show();

            StartCoroutine(CheckMatch());
        }
    }

    private System.Collections.IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(0.5f);

        if (firstRevealed.id == secondRevealed.id)
        {
            firstRevealed.Remove();
            secondRevealed.Remove();
            pairsFound++;

            if (pairsFound >= totalPairs)
            {
                Debug.Log("You win! Image revealed.");
            }
        }
        else
        {
            firstRevealed.Hide();
            secondRevealed.Hide();
        }

        firstRevealed = null;
        secondRevealed = null;
    }
}
