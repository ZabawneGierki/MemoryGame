using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RandomImageSelector  
{
    private static List<Sprite> availableSprites; // pool of unused sprites

   private static bool initialized = false;




    public static void Initialize()
    {
        if (initialized) return;
        initialized = true;
        // Load all sprites from Resources/Images into the pool
        Sprite[] allSprites = Resources.LoadAll<Sprite>("Images");
        availableSprites = new List<Sprite>(allSprites);
        if (availableSprites.Count == 0)
        {
            Debug.Log("No sprites found in Resources/Images!");
        }
    }

    public static Sprite GetRandomUniqueSprite()
    {

        Initialize();
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
