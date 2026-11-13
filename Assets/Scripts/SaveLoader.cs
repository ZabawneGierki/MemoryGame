using UnityEngine;
using UnityEngine.UI;

public class SaveLoader : MonoBehaviour
{
    [SerializeField] public SaveData saveData;

    public static SaveLoader Instance { get; private set; }

    [SerializeField] private Modes currentMode;
  

    private string SaveKey => $"SaveData_{currentMode}";

    private void Awake()
    {
        Instance = this;
        LoadSaveData();
    }

    private void OnEnable()
    {
        ModeManager.OnModeChanged += HandleModeChanged;
    }

    private void OnDisable()
    {
        ModeManager.OnModeChanged -= HandleModeChanged;
        SaveSaveData();
    }

    private void HandleModeChanged(Modes newMode)
    {
        if (currentMode == newMode)
        {
            return;
        }

        SaveSaveData();

        currentMode = newMode;
        LoadSaveData();

        Debug.Log($"Loaded save data for mode: {newMode}");
        // show the number of sprites in the save data
        Debug.Log($"Number of sprites in save data: {saveData.unlockedLevels.Count}");
    }

    public void LoadSaveData()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            saveData = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
        }
        else
        {
            saveData = new SaveData();
        }
    }

    public void SaveSaveData()
    {
        if (saveData == null)
        {
            return;
        }

        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }
}
