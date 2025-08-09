using UnityEngine;

public class ResetButton : MonoBehaviour
{
    public void ResetGame()
    {
        GameManager.Instance.ResetGame();
    }
}