using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    [Serializable]
    public struct LevelUnlock
    {
        public int levelId;
        public bool isUnlocked;
    }

    public SaveData()
    {
        // Initialize all levels as locked except the first one
        for (int i = 0; i < GameData.GetImagesCountForCurrentMode(); i++)
        {
            unlockedLevels.Add(new LevelUnlock
            {
                levelId = i,
                isUnlocked = false
            });
        }

    }

    public  bool UnlockLevel(int levelId)
    {
        if (levelId < 0 || levelId >= unlockedLevels.Count)
        {
            return false; // Invalid level ID
        }
        unlockedLevels[levelId] = new LevelUnlock
        {
            levelId = levelId,
            isUnlocked = true
        };
        return true; // Level unlocked successfully
    }

    public List<LevelUnlock> unlockedLevels = new(GameData.GetImagesCountForCurrentMode());
}
