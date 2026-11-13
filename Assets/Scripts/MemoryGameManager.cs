using DG.Tweening;  // DOTween
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MemoryGameManager : MonoBehaviour
{

    // public event to fire when the game is completed
    public delegate void GameCompleted();
    public event GameCompleted OnGameCompleted;

    [SerializeField] TextMeshProUGUI counter;
    public Sprite[] cardImages;
    public Sprite cardBack;
    public GameObject cardPrefab;
    public Transform gridParent;
    public Image hiddenImage; // background image to reveal

     

    private MemoryCard firstRevealed;
    private MemoryCard secondRevealed;
    private int totalPairs;
    private int pairsFound;


    private string levels = "/10";


    void Start()
    {
         
        UpdateCounter();
        SetupBoard();
    }

    void SetupBoard()
    {
        
        hiddenImage.sprite =  ImageSelector.GetSpriteById(GameData.CurrentLevel);
        List<int> ids = new();

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

    private IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(0.5f);

        if (firstRevealed.id == secondRevealed.id)
        {
            firstRevealed.Remove();
            secondRevealed.Remove();
            pairsFound++;

            if (pairsFound >= totalPairs)
            {
                RevealHiddenImage();
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

    private void RevealHiddenImage()
    {
        hiddenImage.DOFade(1f, 1f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                OnGameCompleted?.Invoke();
                Invoke(nameof(LoadNextLevel), 2f);

            });
    }

    


   
    public void LoadNextLevel()
    {
        GameData.CurrentLevel++;

         
         
        if (GameData.CurrentLevel == GameData.GetImagesCountForCurrentMode() + 1)
        {
            EndGame();
            return;
        }

        

    }

    public void EndGame()
    {
        
        GameData.CurrentLevel = 0; // Reset the current level to 0
        SceneManager.LoadScene(1);
    }


    private void UpdateCounter()
    {
        counter.text = GameData.CurrentLevel +1 + levels;
    }

}
