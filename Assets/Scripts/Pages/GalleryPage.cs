using System;
using UnityEngine;
using UnityEngine.UI;
public class GalleryPage : Page
{
    [SerializeField] private Transform gridParent;
    [SerializeField] private GalleryImage galleryImagePrefab;

    public override void OnPageEnter()
    {
        SaveLoader.Instance.LoadSaveData();
        SaveData saveData = SaveLoader.Instance.saveData;

        FillGrid(saveData);
    }

    private void FillGrid(SaveData saveData)
    {
        foreach (Transform child in gridParent)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        for (int i = 0; i < saveData.unlockedLevels.Count; i++)
        {
            bool isUnlocked = saveData.unlockedLevels[i].isUnlocked;
            GalleryImage galleryImage = Instantiate(galleryImagePrefab, gridParent);
            galleryImage.SetImage(i, isUnlocked);
        }
    }
}
