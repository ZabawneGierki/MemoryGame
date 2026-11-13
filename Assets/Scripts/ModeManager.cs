using UnityEngine;
 
using TMPro;
using System.Collections.Generic;

public class ModeManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown modeDropdown;

    public delegate void ModeChanged (Modes newMode);
    public  static event ModeChanged OnModeChanged;

    public ModeOption[] OptionsToFill ;

    private void Awake()
    {
        FillTheDropDown();
    }
    public void FillTheDropDown()
    {
        modeDropdown.ClearOptions();
        var optionNames = new List<string>();
        foreach (var option in OptionsToFill)
        {
            optionNames.Add(option.modeName);
        }
        modeDropdown.AddOptions(optionNames);
    }


    public void SetMode(int modeIndex)
    {
        var selectedMode = OptionsToFill[modeIndex].modeType;
        // Set the game mode based on the selectedMode
        Debug.Log($"Selected Mode: {selectedMode}");
        GameData.CurrentMode = selectedMode;
        // Invoke the ModeChanged event to notify subscribers about the mode change
        OnModeChanged?.Invoke(selectedMode);
    }
}


[System.Serializable]
public class ModeOption
{
    public string modeName;
    public Modes modeType;
}
