using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : Page
{
     public void LoadGamePlay()
    {
        // reset the game state
        GameData.CurrentLevel = 0;

        SceneManager.LoadScene(SceneNames.GamePlay);
    }
}
