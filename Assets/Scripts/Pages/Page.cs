using UnityEngine;

[RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
public abstract class Page : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    public CanvasGroup CanvasGroup
    {
        get
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            return canvasGroup;
        }
    }

    public virtual void OnPageEnter()
    {
    }

    public virtual void OnPageExit()
    {
    }
}