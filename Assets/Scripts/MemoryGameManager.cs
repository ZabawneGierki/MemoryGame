using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;  // DOTween
using UnityEngine.SceneManagement;

public class MemoryGameManager : MonoBehaviour
{
    public Sprite[] cardImages;
    public Sprite cardBack;
    public GameObject cardPrefab;
    public Transform gridParent;
    public Image hiddenImage; // background image to reveal
    public Button nextLevelButton;
    public CanvasGroup TsundereEris;

    private MemoryCard firstRevealed;
    private MemoryCard secondRevealed;
    private int totalPairs;
    private int pairsFound;

    void Start()
    {
        FadeErisAtTheStart();
        SetupBoard();
    }

    void SetupBoard()
    {
         
        hiddenImage.sprite = Resources.Load<Sprite>(SaveData.adress + SaveData.CurrentLevel);
        List<int> ids = new List<int>();

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

        // Start with the hidden image invisible
        //hiddenImage.color = new Color(1f, 1f, 1f, 0f);
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
                MakeErisAppearForAWhile();
                nextLevelButton.interactable = true;
                
            });
    }

    private void FadeErisAtTheStart()
    {
        TsundereEris.alpha = 0f;
    }


    private void MakeErisAppearForAWhile()
    {
        Sequence seq = DOTween.Sequence();

        seq.Append(TsundereEris.DOFade(1f, 0.4f))  // Fade in
           .Append(TsundereEris.DOFade(0f, 1.4f)); // Fade out


    }
    public void NextLevel()
    {
        print(SaveData.CurrentLevel);
        SaveData.CurrentLevel++;
        if(SaveData.CurrentLevel == 10)
        {
            EndGame();
            return; //prevents from reloading scene 0.
        }
           
        SceneManager.LoadScene(0);

    }

    public void EndGame()
    {
        print("wow!");
        SceneManager.LoadScene(1);
    }
}
