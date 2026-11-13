using UnityEngine;
using UnityEngine.UI;

public class GalleryImage : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField, Range(4, 128)] private int pixelBlockSize = 32;

    private bool isUnlocked = false;
    private int imageIndex;

    private Sprite originalSprite;
    private Sprite pixelatedSprite;
    private Texture2D pixelatedTexture;
    private Button button;

    public delegate void ImageClickedDelegate(int index, bool unlocked);
    public static event ImageClickedDelegate OnImageClickedEvent;

    private void Awake()
    {
       button = GetComponent<Button>();
    }
    public void SetImage(int index, bool unlocked)
    {
        ClearPixelatedImage();

        imageIndex = index;
        isUnlocked = unlocked;

        originalSprite = ImageSelector.GetSpriteById(imageIndex);
        image.sprite = originalSprite;
        image.color = Color.white;

        if (!isUnlocked && originalSprite != null)
        {
            MakeImagePixelated();
        }
    }

    public void OnImageClicked()
    {
        OnImageClickedEvent?.Invoke(imageIndex, isUnlocked);

    }

    private void OnEnable()
    {
        button.onClick.AddListener(OnImageClicked);
    }
    private void OnDisable()
    {
        button.onClick.RemoveListener(OnImageClicked);
    }
    public void MakeImagePixelated()
    {
        if (pixelatedSprite != null)
        {
            image.sprite = pixelatedSprite;
            return;
        }

        if (originalSprite == null)
        {
            originalSprite = image.sprite;
        }

        if (originalSprite == null)
        {
            return;
        }

        Rect rect = originalSprite.rect;
        int width = Mathf.RoundToInt(rect.width);
        int height = Mathf.RoundToInt(rect.height);
        int blockSize = Mathf.Max(1, pixelBlockSize);

        Color[] pixels = originalSprite.texture.GetPixels(
            Mathf.RoundToInt(rect.x),
            Mathf.RoundToInt(rect.y),
            width,
            height);

        for (int y = 0; y < height; y += blockSize)
        {
            for (int x = 0; x < width; x += blockSize)
            {
                int endX = Mathf.Min(x + blockSize, width);
                int endY = Mathf.Min(y + blockSize, height);
                Color averageColor = Color.clear;

                for (int py = y; py < endY; py++)
                {
                    for (int px = x; px < endX; px++)
                    {
                        averageColor += pixels[py * width + px];
                    }
                }

                averageColor /= (endX - x) * (endY - y);

                for (int py = y; py < endY; py++)
                {
                    for (int px = x; px < endX; px++)
                    {
                        pixels[py * width + px] = averageColor;
                    }
                }
            }
        }

        pixelatedTexture = new Texture2D(
            width, height, TextureFormat.RGBA32, false);

        pixelatedTexture.filterMode = FilterMode.Point;
        pixelatedTexture.wrapMode = TextureWrapMode.Clamp;
        pixelatedTexture.SetPixels(pixels);
        pixelatedTexture.Apply(false, true);

        Vector2 pivot = new Vector2(
            originalSprite.pivot.x / width,
            originalSprite.pivot.y / height);

        pixelatedSprite = Sprite.Create(
            pixelatedTexture,
            new Rect(0, 0, width, height),
            pivot,
            originalSprite.pixelsPerUnit);

        image.sprite = pixelatedSprite;
        image.color = Color.white;
    }

    private void ClearPixelatedImage()
    {
        if (pixelatedSprite != null)
        {
            Destroy(pixelatedSprite);
            pixelatedSprite = null;
        }

        if (pixelatedTexture != null)
        {
            Destroy(pixelatedTexture);
            pixelatedTexture = null;
        }
    }

    private void OnDestroy()
    {
        ClearPixelatedImage();
    }
}
