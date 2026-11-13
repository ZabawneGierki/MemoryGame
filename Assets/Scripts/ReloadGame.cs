using UnityEngine;

public class ReloadGame : MonoBehaviour
{
    public void LoadGameplay()
    {
               UnityEngine.SceneManagement.SceneManager.LoadScene("Gameplay");
    }
}
