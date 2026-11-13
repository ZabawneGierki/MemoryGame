using DG.Tweening;
using UnityEngine;

public class RandomComic : MonoBehaviour
{
    [SerializeField] CanvasGroup Character;
    

     
    private void HideCharacter()
    {
        Character.alpha = 0f; // Make the character invisible
    }
    private void ShowCharacter()
    {
        Sequence seq = DOTween.Sequence();

        seq.Append(Character.DOFade(1f, 0.4f))  // Fade in
           .Append(Character.DOFade(0f, 1.4f)); // Fade out
    }
}
