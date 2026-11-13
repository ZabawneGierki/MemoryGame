using UnityEngine;

[CreateAssetMenu(fileName = "ImageLibrary", menuName = "Scriptable Objects/ImageLibrary")]
public class ImageLibrary : ScriptableObject
{
    public string libraryName;
    public Modes mode;
    public Sprite[] images;
}
