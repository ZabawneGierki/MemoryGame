using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RandomImageSelector : MonoBehaviour
{
    private List<Sprite> availableSprites; // pool of unused sprites

    void Awake()
    {
        // Load all sprites from Resources/Images into the pool
        Sprite[] allSprites = Resources.LoadAll<Sprite>("Images");
        availableSprites = new List<Sprite>(allSprites);

        if (availableSprites.Count == 0)
        {
            print("No sprites found in Resources/Images!");
        }
    }

    public Sprite GetRandomUniqueSprite()
    {
        if (availableSprites.Count == 0)
        {
             
            SceneManager.LoadScene("OutOfImages");

        }

        // Pick a random index from the pool
        int index = Random.Range(0, availableSprites.Count);
        Sprite chosen = availableSprites[index];

        // Remove it so it can't be chosen again
        availableSprites.RemoveAt(index);

        return chosen;
    }
}
