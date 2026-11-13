using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class  ImageSelector
{
    private static List<Sprite> availableSprites; // pool of unused sprites
 

    public static int NumberOfAvailableSprites()
    {
        Initialize();
        return availableSprites.Count;
    }


    public static void Initialize()
    {
        
        // Load all sprites from Resources/Images into the pool
        Sprite[] allSprites = Resources.LoadAll<Sprite>("Images/" + GameData.CurrentMode);
        availableSprites = new List<Sprite>(allSprites);
        if (availableSprites.Count == 0)
        {
            Debug.Log("No sprites found in Resources/Images/" + GameData.CurrentMode);
        }
    }

     

    public static Sprite GetSpriteById(int id)
    {
        Initialize();
        if (id < 0 || id >= availableSprites.Count)
        {
            Debug.LogError($"Invalid sprite ID: {id}");
            return null;
        }
        return availableSprites[id];
    }


}
