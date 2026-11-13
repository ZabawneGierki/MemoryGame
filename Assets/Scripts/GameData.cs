using System;

public static class GameData
{
    public static int CurrentLevel = 0;
    

    private static readonly int[] ImagesPerMode = { 11, 10, 15 }; // Number of images for each mode
    public static Modes CurrentMode = Modes.Normal;

    public static int GetImagesCountForCurrentMode()
    {
        // error handling for invalid mode
        if ((int)CurrentMode < 0 || (int)CurrentMode >= ImagesPerMode.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(CurrentMode), "Invalid game mode.");
        }
        return ImagesPerMode[(int)CurrentMode];

    }

}
