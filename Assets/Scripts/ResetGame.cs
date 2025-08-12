using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetGame : MonoBehaviour
{
    public void ReloadGame()
    {

        SaveData.CurrentLevel = 0;
        SceneManager.LoadScene(0);

    }
}
