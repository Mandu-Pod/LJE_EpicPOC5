using UnityEngine;

public class Mark : MonoBehaviour
{
    public MarkType Type { get; private set; }

    public bool IsResource => Type.IsResource();
    public bool IsProduct => Type.IsProduct();
    public bool IsPerson => Type.IsPerson();
    public MarkCategory Category => Type.GetCategory();

    private Vector3 originalPosition;
    private bool isFlipped = false;

    private GameObject outlineObject;
    private SpriteRenderer outlineRenderer;
    private SpriteRenderer mainRenderer;

    [Header("아웃라인 설정")]
    private float outlineScale = 1.3f;
    private Color validCombineColor = Color.green;
    private Color invalidCombineColor = Color.red;

    public bool IsFlipped => isFlipped;
    public Vector3 OriginalPosition => originalPosition;

    public void Initialize(MarkType type, int unusedDurability = 0)
    {
        Type = type;
        originalPosition = transform.position;

        SetupOutline();
    }

    private void SetupOutline()
    {
        mainRenderer = GetComponent<SpriteRenderer>();
        if (mainRenderer == null)
            mainRenderer = GetComponentInChildren<SpriteRenderer>();

        if (mainRenderer == null) return;

        mainRenderer.sortingOrder = 100;

        outlineObject = new GameObject("Outline");
        outlineObject.transform.SetParent(transform);
        outlineObject.transform.localPosition = Vector3.zero;
        outlineObject.transform.localScale = Vector3.one * outlineScale;

        outlineRenderer = outlineObject.AddComponent<SpriteRenderer>();
        outlineRenderer.sprite = mainRenderer.sprite;
        outlineRenderer.sortingOrder = 99;
        outlineRenderer.color = Color.clear;

        outlineObject.SetActive(false);
    }

    public Vector2 GetPosition()
    {
        return transform.position;
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void MoveToFlippedPosition(Vector2 reflectedPosition)
    {
        if (!isFlipped)
        {
            originalPosition = transform.position;
        }
        isFlipped = true;
        transform.position = new Vector3(reflectedPosition.x, reflectedPosition.y, transform.position.z);
    }

    public void RestoreOriginalPosition()
    {
        if (isFlipped)
        {
            transform.position = originalPosition;
            isFlipped = false;
        }
        HideOutline();
    }

    public void ConfirmPosition()
    {
        originalPosition = transform.position;
        isFlipped = false;
        HideOutline();
    }

    public void ShowValidOutline()
    {
        ShowOutline(validCombineColor);
    }

    public void ShowInvalidOutline()
    {
        ShowOutline(invalidCombineColor);
    }

    private void ShowOutline(Color color)
    {
        if (outlineObject == null || outlineRenderer == null) return;

        outlineObject.SetActive(true);
        outlineRenderer.color = color;
    }

    public void HideOutline()
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(false);
        }
    }
}