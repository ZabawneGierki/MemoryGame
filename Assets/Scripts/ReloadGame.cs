using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadGame : MonoBehaviour
{
    public void LoadGameplay()
    {
               UnityEngine.SceneManagement.SceneManager.LoadScene("Gameplay");
    }
}
