using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextLevelButton : MonoBehaviour
{
   public void OnClickNext()
    {
        //update the save data.
        SaveData.CurrentLevel++;

    }
}
